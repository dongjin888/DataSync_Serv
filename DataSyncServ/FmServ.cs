using DataSyncServ.Tasks;
using DataSyncServ.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DataSyncServ
{
    public partial class FmServ : Form
    {
        //server socket
        Socket serverSock = null;
        volatile bool listenFlg = true;

        //client socket
        Dictionary<string, Socket> clientSockDict = null;

        //listen button control
        volatile string curStatuStr = "start";

        // data service
        DataService service = new DataService();

        // file save path
        string dataPath = ContantInfo.Fs.path; //服务端文件存储的根路径

        //dnld lock 
        object myLock = new object();

        //创建平台,产品目录时的锁
        //object pltfmPdctLock = new object();

        // csv lock 
        //object csvLock = new object();

        //object bunchLock = new object();

        //object upldLock = new object();

        //object insertLock = new object();

        public FmServ()
        {
            //serverSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSockDict = new Dictionary<string, Socket>();

            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            setButtonStatus();
        }

        private void FmServ_Load(object sender, EventArgs e)
        {
            //取消跨线程安全检查
            CheckForIllegalCrossThreadCalls = false;
        }

        private void FmServ_FormClosing(object sender, FormClosingEventArgs e)
        {
            listenFlg = false;
            service.closeCon();

            
            using (FileStream logFile = new FileStream(Environment.CurrentDirectory + "\\server.log", FileMode.Append))
            {
                using(StreamWriter sw = new StreamWriter(logFile))
                {
                    sw.Write(txtLog.Text);
                    sw.WriteLine("======close======" + DateTime.Now.ToLocalTime() + "===========");
                    sw.Flush();
                }
            }

            //错误日志
            using(FileStream fs = new FileStream(Environment.CurrentDirectory + "\\server-err.log", FileMode.Append))
            {
                using(StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(LogEx.buf.ToString());
                    sw.Flush();
                }
            }

            if(serverSock!=null)
            {
                try
                {
                    serverSock.Shutdown(SocketShutdown.Receive);
                    serverSock.Close();
                    txtLog.AppendText("关闭服务端 接收的socket.\r\n");
                }
                catch(Exception ex)
                {
                    LogEx.log("关闭服务端 接收的socket exception:\r\n" + ex.Message);
                }
            }
        }

        private void btStopListen_Click(object sender, EventArgs e)
        {
            curStatuStr = "start";
            listenFlg = false;
            setButtonStatus();

            using (FileStream logFile = new FileStream(Environment.CurrentDirectory + "\\server.log", FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(logFile))
                {
                    sw.Write(txtLog.Text);
                    sw.WriteLine("============" + DateTime.Now.ToLocalTime() + "===========");
                    sw.Flush();
                }
            }

            //错误日志
            using (FileStream fs = new FileStream(Environment.CurrentDirectory + "\\server-err.log", FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(LogEx.buf.ToString());
                    sw.Flush();
                }
            }

            if (serverSock != null)
            {
                try
                {
                    if (serverSock.Connected)
                    {
                        serverSock.Shutdown(SocketShutdown.Both);
                    }
                    serverSock.Close(); 
                    serverSock = null;
                    Console.WriteLine("close serversocket ok");
                }
                catch(Exception ex)
                {
                    LogEx.log("serversock shutdown:\r\n"+ex.Message);
                }
            }
        }

        private void btStartListen_Click(object sender, EventArgs e)
        {
            string port = txtPort.Text.Trim();
            if (port.Equals(""))
            {
                MessageBox.Show("input the port !", "message");
            }
            else
            {
                if (Int32.Parse(port) < 2048 || Int32.Parse(port) > 65525)
                {
                    MessageBox.Show("please check you input port between 2048~65525 !", "message");
                }
                else
                {
                    IPAddress ip = IPAddress.Any;
                    IPEndPoint pt = new IPEndPoint(ip, Int32.Parse(port));
                    try
                    {
                        serverSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, 
                            ProtocolType.Tcp);
                        serverSock.Bind(pt);
                        serverSock.Listen(50);
                        listenFlg = true;
                    }
                    catch(Exception ex)
                    {
                        LogEx.log("bind servSock exception:\r\n" + ex.Message);
                        return;
                    }

                    //开启client socket的链接请求
                    Thread listenThread = new Thread(listen);
                    listenThread.IsBackground = true;
                    listenThread.Start();

                    txtLog.AppendText("server open success!\r\n");

                    //设置按钮
                    curStatuStr = "stop";
                    setButtonStatus();
                }
            }
        }

        //接收client 连接请求的线程函数
        private void listen()
        {
            if(serverSock == null)
            {
                MessageBox.Show("socket is null!", "message");
                return;
            }

            while (listenFlg)
            {
                Socket clientSock = null;
                try
                {
                    txtLog.AppendText("wait a connection.... \r\n");
                    clientSock = serverSock.Accept();
                    txtLog.AppendText(clientSock.RemoteEndPoint + "connected to server !" + clientSock.LocalEndPoint + "\r\n");
                }
                catch(Exception ex)
                {
                    LogEx.log("serversock accept exception!\r\n" + ex.Message);
                    break;
                }

                try
                {
                    clientSockDict.Add(clientSock.RemoteEndPoint.ToString(), clientSock);
                }
                catch { LogEx.log("client socket dict exception! \r\n"); }

                //每连接到一个client socket ，就开启一个线程接收client 消息
                Thread thread = new Thread(recvMsg);
                thread.IsBackground = true;
                thread.Start(clientSock);
            }
        }

        //接收到每个client的连接后，会针对每个client开启线程接收消息
        private void recvMsg(object client)
        {
            Socket clientSock = client as Socket;
            bool endRecvFlg = false;
            string msg = null;
            byte[] msgBuf = null;

            // heads[1]-activator  heads[2]-operator  heads[3]-unique
            // heads[4]-pltfm  heads[5]-pdct heads[6]-info heads[7]-other
            string[] heads = null;
            CsvTask csvTask = null ;

            while (!endRecvFlg)
            {
                txtLog.AppendText("wait next msg...\r\n");
                msgBuf = new byte[125];
                int count;
                try{
                    count = clientSock.Receive(msgBuf);
                }catch{
                    try
                    {
                        clientSock.Shutdown(SocketShutdown.Both);
                        clientSock.Close();
                        txtLog.AppendText(clientSock.RemoteEndPoint + " shutdown !\r\n");
                    }
                    catch { LogEx.log("server-client sock close exception!\r\n"); }
                    return ;
                }
                if (count == 0)
                    return;
                msg = Encoding.UTF8.GetString(msgBuf);

                #region 断开连接
                if (msg.StartsWith("exit:"))
                {
                    string whoExit = clientSock.RemoteEndPoint.ToString();
                    bool exitException = false;
                    txtLog.AppendText( "<>client "+whoExit+ " request exit ! \r\n");

                    //关闭连接
                    try
                    { clientSock.Shutdown(SocketShutdown.Both);}
                    catch(Exception ex) { exitException = true;LogEx.log(ex.Message+"\r\n"); }

                    try
                    {
                        clientSock.Close();
                    }
                    catch { LogEx.log("client "+whoExit+" close exception !\r\n");exitException = true; }

                    //从dict中移除socket连接
                    try
                    {
                        clientSockDict.Remove(whoExit);
                    }
                    catch { LogEx.log("remove " + whoExit + " exception !\r\n"); exitException = true; }

                    if (!exitException)
                    {
                        txtLog.AppendText(whoExit + " exit ok ! \r\n");
                    }
                    else { LogEx.log(whoExit + " may exit err !\r\n  "); }

                    return; //退出监听函数
                }
                #endregion

                #region 上传 请求
                if (msg.StartsWith("upld:"))
                {
                    heads = msg.Split('#');
                    txtLog.AppendText("<>client upld head:\r\n" + msg + "\r\n");

                    msg = "resupld:#" + heads[3] + "#"; //file:#unique#
                    try
                    {
                        clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));// head response
                        txtLog.AppendText("[]server resupld:" + msg + "\r\n");
                    }
                    catch(Exception ex)
                    { LogEx.log("[]server resupld exception:\r\n"+ex.Message); }

                    //重新下一次等待，等待 file: 请求
                    continue;
                }
                #endregion

                #region 上传真正的压缩文件
                if (msg.StartsWith("file:")) ////file: # file_len # file_name #
                {
                    string fileName = msg.Split('#')[2];
                    txtLog.AppendText("<>client file head:\r\n" + msg + "\r\n");

                    msg = "resfile:#" + fileName + "#"; //file:#file_name#
                    try
                    {
                        clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                        txtLog.AppendText("[]server resfile:\r\n" + msg + "\r\n");

                        //封装upld任务
                        UpldTask task = new UpldTask(clientSock, heads);

                        //然后会进入到下面的文件数据接收部分
                        //LogEx.log(clientSock.RemoteEndPoint + " upld lock");
                        //lock (upldLock)
                        //{
                            recvData(task);
                        //}
                        //LogEx.log(clientSock.RemoteEndPoint + " release upld lock");
                    }
                    catch(Exception  ex)
                    { LogEx.log("[]server resfile exception:\r\n"+ex.Message); }

                    /*不能用线程，这样会在sock.Receive()时出错
                    Thread recvDataTh = new Thread(recvData);
                    recvDataTh.IsBackground = true;
                    recvDataTh.Start(clientSock);
                    */

                    //break;
                    continue;//这里是后面更改了client的socket 只有一个以后
                }
                #endregion

                #region 下载文件请求代码段
                if (msg.StartsWith("dnld:"))
                {
                    txtLog.AppendText("<>client dnld head:\r\n" + msg + "\r\n");

                    string[] dnldInfo = (msg.Split('#')[1]).Split('_'); //dnld:#userid_datestring#

                    //先连接数据库进行检测，是否存在这条记录
                    string[] path = service.getTrialPath(dnldInfo);

                    string dnldFileName = null;

                    if(path != null)
                    {
                        // [change the process]
                        dnldFileName = path[0] + "\\" + "data.zip";

                        //获取pltfm名字和pdct 名字
                        DirectoryInfo last = new DirectoryInfo(path[0]);
                        string pltfmPdctStr = last.Parent.Parent.Name+ "_" +last.Parent.Name;

                        try
                        {
                            //如果存在的话，再进行下面的响应
                            FileInfo file = new FileInfo(dnldFileName);
                            string response = "resdnld:#" + msg.Split('#')[1] + "#" + file.Length + "#"+pltfmPdctStr+"#";
                            clientSock.Send(Encoding.UTF8.GetBytes(response.ToCharArray())); //响应client的下载请求
                            txtLog.AppendText("[]server resdnld:\r\n" + response + "\r\n");

                            //组装dnld任务
                            DnldTask dnldTask = new DnldTask(clientSock, dnldInfo, dnldFileName);
                            
                            //开启向client 传输文件的线程
                            Thread sendDataTh = new Thread(sendData);
                            sendDataTh.IsBackground = true;
                            sendDataTh.Start(dnldTask);
                        }
                        catch(Exception ex)
                        { LogEx.log("[]server resdnld exception:\r\n"+ex.Message); }
                    }
                    else
                    {
                        string response = "errdnld:#" + msg.Split('#')[1] + "#";
                        try
                        {
                            clientSock.Send(Encoding.UTF8.GetBytes(response.ToCharArray()));//下载错误
                            txtLog.AppendText("[]server reserrdnld:\r\n" + response + "\r\n");
                        }
                        catch (Exception ex)
                        { LogEx.log("[]server reserrdnld exception:\r\n"+ex.Message); }
                    }

                    //这是文件下载，前面已经开启下载线程，退出该线程
                    //break;
                    continue;//这里是后面更改了client的socket 只有一个以后
                }//if (msg.StartsWith("dnld:"))
                #endregion

                #region 请求 summary.csv 消息处理代码段
                //csv 文件请求
                if (msg.StartsWith("reqcsv:")) //"reqcsv:#" + userId + "_" + trialDate + "#" [+ sumId + "#"]
                {
                    txtLog.AppendText("<>client reqcsv head:\r\n" + msg+"\r\n");
                    csvTask = new CsvTask();

                    //根据请求头中的字段userid,trialdate 去数据库查询该trail路径
                    //检查文件系统中trail路径里是否有.csv 文件
                    string[] splits = msg.Split('#');
                    string[] trialUnique = splits[1].Split('_');
                    string[] path = service.getTrialPath(trialUnique);
                    bool ifReqCsvErr = false;
                    string reqCsvErrStr = "";
                    int sumId = Int32.Parse(splits[2]);
                    List<FileInfo> csvFiles = new List<FileInfo>();

                    #region Trial 记录检查
                    if (path == null && path.Length<2) //服务端没有数据记录
                    {
                        ifReqCsvErr = true;
                        reqCsvErrStr = "没有这条Trail 记录！";
                    }
                    else  // 有数据记录
                    {
                        //然后找到 trial/debug/ 中的 summary.csv 文件
                        DirectoryInfo trialPath = new DirectoryInfo(path[1]);
                        if (!trialPath.Exists)
                        {
                            ifReqCsvErr = true;
                            reqCsvErrStr = "数据库中有Trail记录，但文件系统中Trail数据丢失！";
                        }
                        else
                        {
                            foreach (FileInfo csv in trialPath.GetFiles())
                            {
                                if (csv.Name.EndsWith(".csv") || csv.Name.EndsWith(".Csv"))
                                    csvFiles.Add(csv);
                            }

                            if(csvFiles.Count <= 0)
                            {
                                ifReqCsvErr = true;
                                reqCsvErrStr = "数据库中有Trail记录，但文件系统中Trail数据丢失！";
                            }
                        }
                    }
                    #endregion

                    //如果文件不存在，回应错误
                    if (ifReqCsvErr)
                    {
                        msg = "errreqcsv:#"+reqCsvErrStr+"#";
                        try
                        {
                            clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                        }
                        catch(Exception ex)
                        { LogEx.log("[]server reserrreqscv exception:\r\n"+ex.Message); }
                       
                        txtLog.AppendText("dnld error:\r\n" + reqCsvErrStr+"\r\n");
                        //重新等待下一次csv 请求
                        continue;
                    }
                    else //如果文件存在，正常回应，并开始发送数据线程
                    {
                        csvTask.CsvRunFlg = true;
                        csvTask.SummaryName = csvFiles[sumId].FullName; // 发送第一个csv文件

                        try
                        {
                            //发送回应
                            //msg = "resreqcsv:#" + csvTask.SummaryName + "#";
                            msg = "resreqcsv:#" + csvFiles.Count + "#" + sumId + "#";
                            clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                            txtLog.AppendText("[]server res reqcsv:\r\n" + msg + "\r\n");

                            csvTask.ClientSock = clientSock;

                            Thread.Sleep(200);
                            //开启数据传输线程
                            Thread sendCsvTh = new Thread(sendCsvData);
                            sendCsvTh.IsBackground = true;
                            sendCsvTh.Start(csvTask);
                        }
                        catch(Exception ex)
                        { LogEx.log("[]server res reqcsv exception:\r\n"+ex.Message); }
                    }

                }
                //客户端接收过程中出错 ==>"errdnldcsv:#" + fileName + "#"
                if (msg.StartsWith("errdnldcsv:"))
                {
                    txtLog.AppendText("client accept csv error:\r\n" + msg+"\r\n");
                    //停止上面的csv 文件传输线程，并重新等待下一次csv请求
                    if (csvTask.CsvRunFlg) { csvTask.CsvRunFlg = false; }

                    continue;
                }
                #endregion

                #region 请求debug/files
                if (msg.StartsWith("reqdbgfile:")) //"reqdbgfile:#" + userId + "_" + trialDate + "#";
                {
                    string unique = msg.Split('#')[1];
                    string[] trialUnique = (unique).Split('_');  
                    //获取数据库中该条trail的文件目录
                    string[] path = service.getTrialPath(trialUnique); //[0]trial/  [1]/trial/debug/
                    bool ifReqDbgErr = false;
                    string reqDbgErrStr = "";

                    List<FileInfo> dbgFiles = new List<FileInfo>(); // 用来装 debug/ 中所有文件

                    #region 检测Trail 记录合法性
                    if (path == null) // 数据库中文件路径不存在
                    {
                        ifReqDbgErr = true;
                        reqDbgErrStr = "reqdbg:数据库中没有改Trial 记录";
                    }
                    else // 数据库中文件路径存在
                    {
                        //> 文件系统中的 trial/debug/ 
                        DirectoryInfo trialDbgPath = new DirectoryInfo(path[1]);
                        if (!trialDbgPath.Exists) //>> trail/debug/不存在
                        {
                            ifReqDbgErr = true;
                            reqDbgErrStr = "reqdbg:数据库中有Trail记录，但文件系统中Trail数据丢失！";
                        }
                        else  //>> trail/debug/不存在
                        {
                            FileHandle.traceAllFile(trialDbgPath, dbgFiles);
                            if(dbgFiles.Count == 0) //>>> trail/debug/ 中没有一个文件
                            {
                                ifReqDbgErr = true;
                                reqDbgErrStr = "reqdbg:数据库和文件系统中都有记录，但是数据目录为空!";
                            }
                        }
                    }
                    #endregion
                    //--+ 上面的检测完成，下面处理响应信息

                    if (ifReqDbgErr)
                    {
                        msg = "errreqdbgfile:#" + reqDbgErrStr + "#";
                        try
                        {
                            clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                        }
                        catch(Exception ex)
                        { LogEx.log("[]server res errreqcsv exception:\r\n"+ex.Message); }

                        txtLog.AppendText("reqdbgfile error:\r\n" + reqDbgErrStr+"\r\n");
                        //重新等待下一次请求
                        continue;
                    }
                    else
                    {
                        msg = "resreqdbgfile:#"+ unique + "#";
                        try
                        {
                            clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                            txtLog.AppendText("[]server resreqdbgfile:\r\n" + msg + "\r\n");
                        }
                        catch(Exception ex)
                        {
                            txtLog.AppendText("server resreqdbgfile exception !\r\n");
                            LogEx.log("server resreqdbgfile exception !\r\n" + ex.Message);
                            continue;
                        }

                        Thread.Sleep(400);

                        //发送文件列表信息
                        StringBuilder sb = new StringBuilder("");
                        DirectoryInfo dbg = new DirectoryInfo(path[1]);
                        for(int i=0; i<dbgFiles.Count; i++)
                        {
                            //i + "*" + debugFiles[i].FullName
                            if (dbg.Name.Equals(dbgFiles[i].Directory.Name)) //就在debug/ 中的文件
                                sb.Append(i + "*" + dbgFiles[i].Name.Replace('#', '@') + ",");
                            else //其他子目录中的文件
                                sb.Append(i + "*" + dbgFiles[i].Directory.Name + "/" + dbgFiles[i].Name.Replace('#', '@') + ",");
                        }
                        sb.Append("#"); //用来分开前面的文件列表#??????及后面多余的东西
                        msg = sb.ToString();
                        try
                        {
                            clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                            txtLog.AppendText("[]server res dbgfiles !");
                        }
                        catch(Exception ex) {
                            txtLog.AppendText("exception-server res dbgfiles:\r\n"+ex.Message+"\r\n");
                            LogEx.log("exception-server res dbgfiles:\r\n" + ex.Message);
                        }
                    }

                }//if(msg.startwith('reqdbgfile:')
                #endregion

                #region 一堆文件下载请求
                if (msg.StartsWith("reqbunchfiles:"))
                {
                    string[] splits = msg.Split('#');
                    string[] trialUnique = splits[1].Split('_');

                    byte[] bunchIdBuf = new byte[1024]; //1k
                    string tmp = null;
                    try
                    {
                        clientSock.Receive(bunchIdBuf);                     // 接收下载文件列表ids
                        tmp = Encoding.UTF8.GetString(bunchIdBuf);
                        if (tmp.StartsWith("bunchfileids"))
                        {
                            txtLog.AppendText("<>client bunchfileids:\r\n" + tmp + "\r\n");

                            //检查完下载目录后回应客户端
                            List<string> reqIds = new List<string>(tmp.Split('#')[1].Split(','));
                            List<string> bunchFiles = new List<string>();
                            long bunchFilesLength = 0; //所有待传输文件大小

                            //获取数据库中该条trail的文件目录
                            string[] path = service.getTrialPath(trialUnique); //[0]trial/  [1]/trial/debug/
                            bool ifReqBunchErr = false;
                            string reqBunchErrStr = "";
                            List<FileInfo> dbgFiles = new List<FileInfo>(); // 用来装 debug/ 中所有文件

                            #region 检测Trail 记录合法性
                            if (path == null || path.Length < 2) // 数据库中文件路径不存在
                            {
                                ifReqBunchErr = true;
                                reqBunchErrStr = "reqbunch:数据库中没有改Trial 记录";
                            }
                            else // 数据库中文件路径存在
                            {
                                //> 文件系统中的 trial/debug/ 
                                DirectoryInfo trialDbgPath = new DirectoryInfo(path[1]);
                                if (!trialDbgPath.Exists) //>> trail/debug/不存在
                                {
                                    ifReqBunchErr = true;
                                    reqBunchErrStr = "reqbunch:数据库中有Trail记录，但文件系统中Trail数据丢失！";
                                }
                                else  //>> trail/debug/不存在
                                {
                                    FileHandle.traceAllFile(trialDbgPath, dbgFiles);
                                    if (dbgFiles.Count == 0) //>>> trail/debug/ 中没有一个文件
                                    {
                                        ifReqBunchErr = true;
                                        reqBunchErrStr = "reqbunch:数据库和文件系统中都有记录，但是数据目录为空!";
                                    }
                                    else
                                    {
                                        string id = null;
                                        for (int i = 0; i < dbgFiles.Count; i++)
                                        {
                                            id = i + "";
                                            if (reqIds.Contains(id))
                                            {
                                                bunchFiles.Add(dbgFiles[i].FullName);  
                                                txtLog.AppendText(id + "<>" + dbgFiles[i].FullName + " 加入下载队列!");
                                                bunchFilesLength += dbgFiles[i].Length;
                                            }
                                        }
                                        if (bunchFiles.Count == 0)
                                        {
                                            ifReqBunchErr = true;
                                            reqBunchErrStr = "reqbunch:下载文件为零 !";
                                        }
                                    }
                                }
                            }
                            #endregion
                            if (ifReqBunchErr)
                            {
                                msg = "errreqbunchfile:#" + reqBunchErrStr + "#";

                                try
                                {
                                    clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                                    txtLog.AppendText("error-reqdbgfile error:\r\n" + reqBunchErrStr + "\r\n");
                                }catch(Exception ex)
                                {
                                    txtLog.AppendText("exception-res reqbunchfiles:\r\n" + ex.Message+"\r\n");
                                    LogEx.log("exception-res reqbunchfiles:\r\n" + ex.Message);
                                }

                                //重新等待下一次请求
                                continue;
                            }
                            else //检查合法，可以下载
                            {
                                //检查完成后，开启线程传输,continue
                                msg = "resreqbunchfile:#" + bunchFiles.Count + "#" +bunchFilesLength + "#";
                                try
                                {
                                    clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                                    txtLog.AppendText("server-" + msg + "\r\n");

                                    // 封装BchDnld 任务
                                    BchDnldTask bchTask = new BchDnldTask(clientSock, bunchFiles);

                                    Thread bunchDnldTh = new Thread(bunchDnld);//开启一堆文件下载线程
                                    bunchDnldTh.IsBackground = true;
                                    bunchDnldTh.Start(bchTask);

                                }catch(Exception ex)
                                {
                                    txtLog.AppendText("回应reqbuchfile时发送错误！" + ex.Message+"\r\n");
                                    LogEx.log("回应reqbuchfile时发送错误！" + ex.Message + "\r\n");
                                }
                            }
                        }
                        else
                        {
                            txtLog.AppendText("接收bunchfileid 失败！\r\n" + msg + "\r\n");
                        }

                    }catch(Exception ex2)
                    {
                        txtLog.AppendText("接收bunchfileid 失败！" + ex2.Message+"\r\n");
                        LogEx.log("接收bunchfileid 失败！" + ex2.Message + "\r\n");
                    }
                }
                #endregion

            }//while(!endRecvFlg)
            txtLog.AppendText("服务端处理请求完成!\r\n");
        }//private void recvMsg()

        //接收压缩文件 upld线程函数
        private void recvData(object upldTask)
        {
            UpldTask task = upldTask as UpldTask;
            Socket clientSock = task.clientSock;
            if(clientSock == null){
                Console.WriteLine("recvData thread start error!");
                return;
            }
            //接收文件的所需的一些组件
            int maxFileBufLen = 1024 * 512;//512 k
            byte[] fileBuf = new byte[maxFileBufLen];

            string msg = null;
            byte[] msgBuf = new byte[64];

            //下面是client 上传的 处理
            DirectoryInfo pltfmDir = null;
            DirectoryInfo pdctDir = null;
            //LogEx.log(clientSock.RemoteEndPoint+" lock pltfm");
            //lock (pltfmPdctLock)
            //{
            pltfmDir = new DirectoryInfo(dataPath + task.heads[4] + "\\");
            if (!pltfmDir.Exists) { Directory.CreateDirectory(pltfmDir.FullName); }
            pdctDir = new DirectoryInfo(pltfmDir.FullName + "\\" + task.heads[5] + "\\");
            if (!pdctDir.Exists) { Directory.CreateDirectory(pdctDir.FullName); }
            //}
            //LogEx.log(clientSock.RemoteEndPoint+" release pltfm lock");

            bool isNewUpld = false;
            DirectoryInfo trialDir = new DirectoryInfo(pdctDir.FullName + "\\" + task.heads[3] + "\\");
            if (!trialDir.Exists) //文件不存在
            {
                isNewUpld = true;
                Directory.CreateDirectory(trialDir.FullName);
            }
            else//要上传的文件夹已经存在
            {
                // 之前没有使用多次上传时的代码
                //string errUpld = "errupld:#";
                //clientSock.Send(Encoding.UTF8.GetBytes(errUpld.ToCharArray()));
                //return;

                LogEx.log(clientSock.RemoteEndPoint+" dlt file lock");
                lock (myLock)
                {
                    //删除这个文件夹中的数据
                    foreach (FileInfo f in trialDir.GetFiles())
                    {
                        File.Delete(f.FullName);
                    }
                    foreach (DirectoryInfo d in trialDir.GetDirectories())
                    {
                        FileHandle.cycDeleteDir(d);
                    }
                }
                LogEx.log(clientSock.RemoteEndPoint + " release dlt lock");
                txtLog.AppendText("服务端删除已经存在的Trial文件夹\r\n");
            }

            //把client的文件存储为这个
            string zipFile = trialDir.FullName + "\\" + "data.zip";
            bool endFileFlg = false;
            bool ifRecvErr = false;
            string recvErrMsg = "";
            int count = 0;
            using (FileStream fs = new FileStream(zipFile, FileMode.Create))
            {
                while (!endFileFlg)
                {
                    try
                    {
                        count = clientSock.Receive(fileBuf);
                    }
                    catch(Exception ex)
                    {
                        LogEx.log("recvData(object obj):接收压缩文件异常！\r\n");
                        ifRecvErr = true;
                        recvErrMsg = "接收文件的socket异常";
                        break;
                    }
                    //正常数据
                    if (count > 128)
                    {
                        fs.Write(fileBuf, 0, count);
                    }
                    else //count < 128
                    {
                        msg = Encoding.UTF8.GetString(fileBuf);

                        //文件结束标志  end:# file_name #
                        if (msg.StartsWith("end:"))
                        {
                            //暂时不发送回应，插完数据记录再回应
                            //msg = "resend:#" + msg.Split('#')[1] + "#"; // end:# file_name #
                            //clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));//response
                            //txtLog.AppendText("client:" + msg + "\r\n");

                            //防止收到结束符时，文件都还没关闭
                            if (fs.CanWrite)
                            {
                                fs.Close();
                            }

                            //结束
                            endFileFlg = true;
                        }

                        //不能整段发送的剩余数据
                        else
                        {
                            try
                            {
                                if(fs != null && fs.CanWrite)
                                {
                                    fs.Write(fileBuf, 0, count);
                                    fs.Close();
                                }
                            }
                            catch(Exception ex)
                            {
                                endFileFlg = true;
                                ifRecvErr = true;
                                recvErrMsg = "接收文件时，写入文件错误！";
                                LogEx.log("接收文件时，写入文件错误:\r\n"+ex.Message);
                                break;
                            }

                            txtLog.AppendText("保存文件:" + msg.Split('#')[1] + " 成功!\r\n");
                        }
                    } // else count < 128
                } // while(!endFileFlg)
            } // using(filestream fs= xxxx )

            if (!ifRecvErr)
            {
                //解压文件，删除多余的文件
                DirectoryInfo debugDir = new DirectoryInfo(trialDir.FullName + "\\debug\\");
                if (!debugDir.Exists) { Directory.CreateDirectory(debugDir.FullName); }
                //> 解压
                ZipFile.ExtractToDirectory(zipFile, debugDir.FullName);

                //> 遍历debugDir 目录中所有解压出的文件
                //> 解压csv.zip,删除其他的.zip 
                List<FileInfo> zipList = new List<FileInfo>();
                FileHandle.traceAllFile(debugDir, zipList);
                foreach (FileInfo f in zipList)
                {
                    //>> 先看是否是.zip 文件
                    if (f.Name.Substring(f.Name.LastIndexOf('.') + 1).Equals("zip"))
                    {
                        //>>> 解压csv.zip 文件
                        string real = f.Name.Substring(0, f.Name.LastIndexOf('.'));
                        if (real.Contains("csv") || real.Contains("Csv"))
                        {
                            DirectoryInfo tmp = new DirectoryInfo(debugDir.FullName + real + "\\");
                            if (!tmp.Exists) { Directory.CreateDirectory(tmp.FullName); }
                            ZipFile.ExtractToDirectory(f.FullName, tmp.FullName);
                        }

                        //>>> 然后删除bin和sv 的zip 文件
                        if ((real.Contains("bin") || real.Contains("Bin")) ||
                            (real.Contains("SV") || real.Contains("sv")))
                            File.Delete(f.FullName);
                    }
                }
                txtLog.AppendText("服务端解压并删除文件成功!\r\n");
                //向数据库插入记录
                try
                {
                    if (isNewUpld && !ifRecvErr)
                    {
                        TrialInfo trialInfo = new TrialInfo();
                        trialInfo.Activator = task.heads[1];
                        trialInfo.Operator = task.heads[2];
                        trialInfo.Unique = task.heads[3];
                        trialInfo.Pltfm = task.heads[4];
                        trialInfo.Pdct = task.heads[5];
                        trialInfo.Info = task.heads[6];
                        trialInfo.Other = task.heads[7];

                        //lock (insertLock)
                        //{
                            try
                            {
                                //插入该条记录
                                service.insertTrial(trialInfo);
                            }
                            catch (Exception sqlEx){ throw new Exception("insert trial record error!\n"+sqlEx.Message); }
                        //}

                        txtLog.AppendText("插入数据记录成功！\r\n");
                    }

                    try
                    {
                        msg = "resend:#" + msg.Split('#')[1] + "#"; // end:# file_name #
                        clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));//response
                        txtLog.AppendText("上传结束，回应客户端:" + msg + "\r\n");
                    }
                    catch(Exception ex)
                    { LogEx.log(ex.Message); }
                }
                catch (Exception ex)
                {
                    try
                    {
                        msg = "errupld:#" + ex.Message + "#"; // end:# file_name #
                        clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));//response
                        txtLog.AppendText("上传失败,回应客户端:" + msg + "\r\n");
                    }catch(Exception ex2)
                    { LogEx.log(ex2.Message); }

                    //删除文件系统中已经保存的文件
                    try
                    {
                        FileHandle.cycDeleteDir(trialDir);
                        txtLog.AppendText("错误上传的文件删除!\r\n");
                    }
                    catch (Exception ex2) {
                        txtLog.AppendText("删除错误文件时错误!" + ex2.Message + "\r\n");
                        LogEx.log("删除错误文件时错误!" + ex2.Message);
                    }
                }
            }
            else
            {
                try
                {
                    msg = "errupld:#" + recvErrMsg + "#"; // end:# file_name #
                    clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));//response
                    txtLog.AppendText("接收上传文件时错误,回应客户端:" + msg + "\r\n");
                }
                catch (Exception ex){
                    LogEx.log(ex.Message);
                }
            }
        }

        //发送压缩文件 dnld的线程函数
        private void sendData(object dnldTask)
        {
            DnldTask task = dnldTask as DnldTask;
            Socket socket = task.clientSock;
            FileInfo file = new FileInfo(task.dnldFileName);
            if (socket != null && file.Exists)
            {
                int transMaxLen = 1024 * 512; //512k
                byte[] fileBuf = null;
                string msg = null;

                //加锁了
                LogEx.log(socket.RemoteEndPoint+" dnld lock");
                lock (myLock)
                {
                    using (FileStream fs = new FileStream(file.FullName, FileMode.Open,FileAccess.Read))
                    {
                        //文件一次传输可以完成
                        if (file.Length < transMaxLen)
                        {
                            fileBuf = new byte[file.Length];
                            fs.Read(fileBuf, 0, (int)file.Length);

                            try
                            {
                                //设置立即发送
                                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                                socket.Send(fileBuf);
                                txtLog.AppendText(file.Name + "一次传输完成!" + "\r\n");
                            }
                            catch (Exception ex)
                            {
                                txtLog.AppendText("发送data.zip数据时，socket 异常！\r\n");
                                LogEx.log("发送data.zip数据时，socket 异常:" + ex.Message);
                            }

                            /*方法一：
                             * 使用线程延时后，前面的数据就会先发送，
                            * 不然socket 算法会让后面的数据和前面的数据一起发送
                             * 这样的话，就不能正确的接收文件发送结束标志*/
                            try { Thread.Sleep(500); }
                            catch { Console.WriteLine("sleep error!"); }

                            //发送文件结束标志
                            msg = "end:#" + task.dnldInfo[0] + "_" + task.dnldInfo[1] + "#";
                            try
                            {
                                socket.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                                txtLog.AppendText("data.zip文件结束标志:" + msg + "\r\n");
                            }
                            catch (Exception ex)
                            {
                                txtLog.AppendText("发送data.zip文件传输结束标识时socket 异常！\r\n");
                                LogEx.log("发送data.zip文件传输结束标识时socket 异常:\r\n" + ex.Message);
                            }
                        }
                        //文件过大，需要分段传输
                        else
                        {
                            fileBuf = new byte[transMaxLen];
                            long fileLen = file.Length;
                            int times = (int)(fileLen / transMaxLen); //整数次
                            int leftLen = (int)(fileLen % transMaxLen);//剩下的字节数

                            //发送整数次
                            for (int i = 1; i <= times; i++)
                            {
                                try
                                {
                                    fs.Read(fileBuf, 0, transMaxLen);
                                    socket.Send(fileBuf);
                                }
                                catch (Exception ex)
                                {
                                    txtLog.AppendText("发送data.zip数据时,读取文件数据或者发送文件数据异常！\r\n");
                                    LogEx.log("发送data.zip数据时,读取文件数据或者发送文件数据异常:" + ex.Message);
                                }
                            }

                            //发送剩余的字节数
                            fileBuf = new byte[leftLen];
                            try
                            {
                                fs.Read(fileBuf, 0, leftLen);
                                socket.Send(fileBuf);
                            }
                            catch (Exception ex)
                            {
                                txtLog.AppendText("发送data.zip最后一个段数据时，读取文件数据或者发送文件数据异常\r\n");
                                LogEx.log("发送data.zip 最后一组数据时,读取文件数据或者发送文件数据异常:" + ex.Message);
                            }

                            //设置延时，使剩余文件信息和 文件结束标志分开发送
                            try { Thread.Sleep(500); }
                            catch { Console.WriteLine("sleep error!"); }

                            txtLog.AppendText(file.Name + " 数据传输完成!\r\n");

                            //最后是 end:# file_name # file_left #   6
                            //接收response dne: # file_name #        4
                            try
                            {
                                msg = "end:#" + task.dnldInfo[0] + "_" + task.dnldInfo[1] + "#";
                                socket.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                                txtLog.AppendText("server:" + msg + "\r\n");
                            }
                            catch (Exception ex)
                            {
                                txtLog.AppendText("发送data.zip 文件传输结束标识时,socket异常！\r\n");
                                LogEx.log("发送data.zip文件传输结束标识时socket 异常:\r\n" + ex.Message);
                            }
                        }
                    }
                }//lock(myLock) 
                LogEx.log(socket.RemoteEndPoint+" release dnld lock");
            }
            else
            {
                MessageBox.Show("sendData(object sock) error!", "error");
            }
        }

        //发送summary.csv 文件
        private void sendCsvData(object csvTask)
        {
            int transMaxLen = 1024 * 512; //512k
            byte[] fileBuf = null;
            string msg = null;

            CsvTask task = csvTask as CsvTask;
            Socket socket = task.ClientSock;
            FileInfo file = new FileInfo(task.SummaryName);

            LogEx.log(socket.RemoteEndPoint+" csv lock");
            //lock (csvLock)
            //{
                using (FileStream fs = new FileStream(file.FullName, FileMode.Open,FileAccess.Read))
                {
                    //文件一次传输可以完成
                    if (file.Length < transMaxLen)
                    {
                        fileBuf = new byte[file.Length];
                        fs.Read(fileBuf, 0, (int)file.Length);

                        try
                        {
                            //设置立即发送
                            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                            socket.Send(fileBuf);
                            txtLog.AppendText(file.Name + "summary.csv 一次传输完成!" + "\r\n");
                        }
                        catch (Exception ex)
                        {
                            txtLog.AppendText("一次发送summary.csv时，socket 异常！\r\n");
                            LogEx.log("一次发送summary.csv时，socket 异常:\r\n" + ex.Message);
                        }

                        try { Thread.Sleep(500); }
                        catch { txtLog.AppendText("sleep error!"); }

                        //发送文件结束标志
                        msg = "endcsv:#" + file.Name + "#";
                        try
                        {
                            socket.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                            txtLog.AppendText("server 发送文件结束标志!:" + msg + "\r\n");
                        }
                        catch (Exception ex)
                        {
                            txtLog.AppendText("发送summary.csv文件结束标识时，socket错误!\r\n");
                            LogEx.log("发送summary.csv文件结束标识时，socket错误:\r\n" + ex.Message);
                        }
                    }
                    //文件过大，需要分段传输
                    else
                    {
                        fileBuf = new byte[transMaxLen];
                        long fileLen = file.Length;
                        int times = (int)(fileLen / transMaxLen); //整数次
                        int leftLen = (int)(fileLen % transMaxLen);//剩下的字节数

                        //发送整数次
                        for (int i = 1; i <= times; i++)
                        {
                            fs.Read(fileBuf, 0, transMaxLen);

                            if (!task.CsvRunFlg) //防止csv接收错误,而该线程文件还没有关闭
                            {
                                fs.Close();
                                return;
                            }
                            try
                            {
                                socket.Send(fileBuf);
                            }
                            catch (Exception ex)
                            {
                                txtLog.AppendText("分段传输summary.csv数据时，socket异常!\r\n");
                                LogEx.log("分段传输summary.csv数据时，socket异常:\r\n" + ex.Message);
                            }
                        }

                        //发送剩余的字节数
                        fileBuf = new byte[leftLen];
                        fs.Read(fileBuf, 0, leftLen);

                        if (!task.CsvRunFlg)
                        {
                            fs.Close();
                            return;
                        }
                        try
                        {
                            socket.Send(fileBuf);
                        }
                        catch (Exception ex)
                        {
                            txtLog.AppendText("分段传输最后的summary.csv数据时,socket异常！\r\n");
                            LogEx.log("分段传输最后的summary.csv数据时,socket异常:\r\n" + ex.Message);
                        }

                        //设置延时，使剩余文件信息和 文件结束标志分开发送
                        try { Thread.Sleep(500); }
                        catch { txtLog.AppendText("sleep error!"); }

                        txtLog.AppendText(file.Name + " 数据传输完成!\r\n");

                        if (!task.CsvRunFlg)
                        {
                            fs.Close();
                            return;
                        }

                        //最后是 end:# file_name # file_left #
                        //接收response dne: # file_name #
                        msg = "endcsv:#" + file.Name + "#";
                        try
                        {
                            socket.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                            txtLog.AppendText("server 发送csv 文件结束标志!:" + msg + "\r\n");
                        }
                        catch (Exception ex)
                        {
                            txtLog.AppendText("发送csv 文件结束标志时，socket异常!\r\n");
                            LogEx.log("发送csv 文件结束标志时，socket异常:\r\n" + ex.Message);
                        }
                        finally
                        {
                            fs.Close();
                        }
                    }
                } // using(FileStream fs = xxx )
            //}
            LogEx.log(socket.RemoteEndPoint+" release csv lock ");

        }

        //发送一堆文件
        private void bunchDnld(object bchTask)
        {
            BchDnldTask task = bchTask as BchDnldTask;
            Socket socket = task.clientSock;
            int sent = 0;
            int transMaxLen = 1024 * 512; //512k
            byte[] fileBuf = null;
            string msg = null;
            FileInfo file = null;

            while (sent < task.bunchFiles.Count)
            {
                bool singleFileEnd = false;
                while (!singleFileEnd)
                {
                    //直接发送单个文件数据，客户端直接接收
                    file = new FileInfo(task.bunchFiles[sent]);

                    //发送单个文件信息
                    if(file.Directory.Name.Equals("debug")) 
                        msg = "singleinfo:#" + file.Length + "#" + file.Name.Replace('#', '@') + "#";
                    else  //非debug 中的文件，返回目录
                        msg = "singleinfo:#" + file.Length + "#" + file.Directory.Name + "-" + file.Name.Replace('#', '@') + "#";
                    try
                    {
                        socket.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                    }catch(Exception ex)
                    {
                        txtLog.AppendText("singleinfo:发送失败！" + ex.Message);
                        LogEx.log("singleinfo:发送失败:\r\n" + ex.Message);
                        break;
                    }

                    //延时
                    try { Thread.Sleep(500); }
                    catch { Console.WriteLine("sleep error!"); }

                   
                    //LogEx.log(socket.RemoteEndPoint + " dunch lock");
                    //lock (bunchLock)
                    //{
                        using (FileStream fs = new FileStream(file.FullName, FileMode.Open,FileAccess.Read))
                        {
                            //文件一次传输可以完成
                            if (file.Length < transMaxLen)
                            {
                                fileBuf = new byte[file.Length];
                                fs.Read(fileBuf, 0, (int)file.Length);

                                //设置立即发送
                                try
                                {
                                    socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                                    socket.Send(fileBuf);
                                    txtLog.AppendText(file.Name + "一次文件传输完成!\r\n");
                                }
                                catch (Exception ex)
                                {
                                    txtLog.AppendText("bunchfile一次性传送文件时socket错误!" + ex.Message);
                                    LogEx.log("bunchfile一次性传送文件时socket错误:\r\n" + ex.Message);
                                    break;
                                }

                                //延时
                                try { Thread.Sleep(500); }
                                catch { Console.WriteLine("sleep error!"); }

                                //发送单个文件结束标志
                                msg = "singleend:#" + file.Name.Replace('#', '@') + "#" + (task.bunchFiles.Count - sent - 1) + "#"; // left count 
                                try
                                {
                                    socket.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                                    singleFileEnd = true;
                                }
                                catch (Exception ex)
                                {
                                    txtLog.AppendText("发送单个文件结束符时错误！" + ex.Message);
                                    LogEx.log("发送单个文件结束符时错误:\r\n" + ex.Message);
                                    break;
                                }
                            }
                            //文件过大，需要分段传输
                            else
                            {
                                fileBuf = new byte[transMaxLen];
                                long fileLen = file.Length;
                                int times = (int)(fileLen / transMaxLen); //整数次
                                int leftLen = (int)(fileLen % transMaxLen);//剩下的字节数

                                //发送整数次
                                for (int i = 1; i <= times; i++)
                                {
                                    fs.Read(fileBuf, 0, transMaxLen);
                                    try
                                    {
                                        socket.Send(fileBuf);
                                    }
                                    catch (Exception ex)
                                    {
                                        txtLog.AppendText("bunchfile分段传输文件时，socket错误!" + ex.Message);
                                        LogEx.log("bunchfile分段传输文件时，socket错误:\r\n" + ex.Message);
                                        break;
                                    }
                                }

                                //发送剩余的字节数
                                fileBuf = new byte[leftLen];
                                fs.Read(fileBuf, 0, leftLen);
                                try
                                {
                                    socket.Send(fileBuf);
                                }
                                catch (Exception ex)
                                {
                                    txtLog.AppendText("bunchfile分段传输文件时，最后一次传输端口错误!" + ex.Message);
                                    LogEx.log("bunchfile分段传输文件时，最后一次传输端口错误:\r\n" + ex.Message);
                                    break;
                                }

                                //设置延时，使剩余文件信息和 文件结束标志分开发送
                                try { Thread.Sleep(500); }
                                catch { Console.WriteLine("sleep error!"); }

                                txtLog.AppendText(file.Name + " 数据传输完成!\n");

                                // 单个文件结束标志
                                msg = "singleend:#" + file.Name.Replace('#', '@') + "#" + (task.bunchFiles.Count - sent - 1) + "#";
                                try
                                {
                                    socket.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                                    singleFileEnd = true;
                                }
                                catch (Exception ex)
                                {
                                    txtLog.AppendText("bunchfile传输单个文件结束符时错误！" + ex.Message);
                                    LogEx.log("bunchfile传输单个文件结束符时错误:\r\n" + ex.Message);
                                    break;
                                }
                            }
                        }//using(FileStream fs = new FileStream())
                    //}
                    LogEx.log(socket.RemoteEndPoint + " release dunch lock");
                }
                sent++;
            }//while(sent < bunchFiles.Count)
        }

        private void setButtonStatus()
        {
            if (curStatuStr.Equals("start"))
            {
                btStartListen.Enabled = true;
                btStartListen.BackColor = Color.SkyBlue;

                btStopListen.Enabled = false;
                btStopListen.BackColor = Color.Silver;
            }

            if (curStatuStr.Equals("stop"))
            {
                btStopListen.Enabled = true;
                btStopListen.BackColor = Color.SkyBlue;

                btStartListen.Enabled = false;
                btStartListen.BackColor = Color.Silver;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2.Image = Properties.Resources.rightHv;
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Image = Properties.Resources.rightLv;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            FmAdmin fmAdmin = new FmAdmin(service);
            if (fmAdmin.ShowDialog() == DialogResult.OK)
            {
                FmDb fm = new FmDb(service);
                fm.Show();
            }
        }
    }
}
