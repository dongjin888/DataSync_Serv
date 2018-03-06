using DataSyncServ.Tasks;
using DataSyncServ.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        // lock 
        object myLock = new object();

        public FmServ()
        {
            serverSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSockDict = new Dictionary<string, Socket>();

            InitializeComponent();
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

            FileStream logFile = new FileStream(Environment.CurrentDirectory + "\\server.log", FileMode.Append);
            StreamWriter sw = new StreamWriter(logFile);
            sw.Write(txtLog.Text);
            sw.WriteLine("============" + DateTime.Now.ToLocalTime() + "===========");
            sw.Flush();
            sw.Close();
            logFile.Close();

            if(serverSock!=null)
            {
                try
                {
                    serverSock.Shutdown(SocketShutdown.Receive);
                    serverSock.Close();
                }
                catch
                {
                    Console.WriteLine("关闭服务端 接收的socket");
                }
            }
        }

        private void btStopListen_Click(object sender, EventArgs e)
        {
            curStatuStr = "start";
            listenFlg = false;
            setButtonStatus();

            if (serverSock != null)
            {
                try
                {
                    serverSock.Shutdown(SocketShutdown.Both);
                    serverSock.Close();
                    serverSock = null;
                    Console.WriteLine("关闭服务端 接收的socket");
                }
                catch
                {
                    Console.WriteLine("关闭服务端 接收的socket");
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
                        serverSock.Bind(pt);
                        serverSock.Listen(10); //最大监听100 个请求
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("绑定servSock 出错"+ex.Message);
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
                    clientSock = serverSock.Accept();
                }
                catch { txtLog.AppendText("serversock accept 异常!\r\n");break; }

                txtLog.AppendText(clientSock.RemoteEndPoint + "连接到服务器!"+clientSock.LocalEndPoint+"\r\n");
                clientSockDict.Add(clientSock.RemoteEndPoint.ToString(), clientSock);

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
                Console.WriteLine("等待下一次消息\r\n");
                msgBuf = new byte[64];
                int count;
                try{
                    count = clientSock.Receive(msgBuf);
                }catch{
                    return ;
                }
                if (count == 0)
                    return;
                msg = Encoding.UTF8.GetString(msgBuf);

                #region 上传 请求
                if (msg.StartsWith("upld:"))
                {
                    heads = msg.Split('#');
                    txtLog.AppendText("upld请求头:" + msg + "\r\n");

                    msg = "resupld:#" + heads[3] + "#"; //file:#unique#
                    try
                    {
                        clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));// head response
                        txtLog.AppendText("server res:" + msg + "\r\n");
                    }
                    catch { txtLog.AppendText("回应resupld时 socket异常！\r\n"); }

                    //重新下一次等待，等待 file: 请求
                    continue;
                }
                #endregion

                #region 上传真正的压缩文件
                if (msg.StartsWith("file:")) ////file: # file_len # file_name #
                {
                    string fileName = msg.Split('#')[2];
                    txtLog.AppendText("接收到客户端文件头:" + fileName + "文件上传请求!\r\n");

                    msg = "resfile:#" + fileName + "#"; //file:#file_name#
                    try
                    {
                        clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                        txtLog.AppendText("server res:" + msg + "\r\n");

                        //封装upld任务
                        UpldTask task = new UpldTask(clientSock, heads);

                        //然后会进入到下面的文件数据接收部分
                        recvData(task);
                    }
                    catch { txtLog.AppendText("回应resfile 时socket 异常!\r\n"); }

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
                    txtLog.AppendText("client下载请求头:" + msg + "\r\n");

                    string[] dnldInfo = (msg.Split('#')[1]).Split('_'); //dnld:#userid_datestring#

                    //先连接数据库进行检测，是否存在这条记录
                    string[] path = service.getTrialPath(dnldInfo);

                    string dnldFileName = path[0]+"\\"+"data.zip";
                    if(path != null)
                    {
                        // [change the process]

                        try
                        {
                            //如果存在的话，再进行下面的响应
                            FileInfo file = new FileInfo(dnldFileName);
                            string response = "resdnld:#" + msg.Split('#')[1] + "#" + file.Length + "#";
                            clientSock.Send(Encoding.UTF8.GetBytes(response.ToCharArray())); //响应client的下载请求
                            txtLog.AppendText("server response:" + response + "\r\n");

                            //组装dnld任务
                            DnldTask dnldTask = new DnldTask(clientSock, dnldInfo, dnldFileName);
                            
                            //开启向client 传输文件的线程
                            Thread sendDataTh = new Thread(sendData);
                            sendDataTh.IsBackground = true;
                            sendDataTh.Start(dnldTask);
                        }
                        catch { txtLog.AppendText("发送data.zip 回应时文件或socket错误！\r\n"); }
                    }
                    else
                    {
                        string response = "errdnld:#" + msg.Split('#')[1] + "#";
                        try
                        {
                            clientSock.Send(Encoding.UTF8.GetBytes(response.ToCharArray()));//下载错误
                            txtLog.AppendText("server response:" + response + "\r\n");
                        }
                        catch { txtLog.AppendText("发送errdnld回应时socket 异常！\r\n"); }
                    }

                    //这是文件下载，前面已经开启下载线程，退出该线程
                    //break;
                    continue;//这里是后面更改了client的socket 只有一个以后
                }//if (msg.StartsWith("dnld:"))
                #endregion

                #region 请求 summary.csv 消息处理代码段
                //csv 文件请求
                if (msg.StartsWith("reqcsv:")) //"reqcsv:#" + userId + "_" + trialDate + "#"
                {
                    txtLog.AppendText("csv请求头:" + msg+"\r\n");
                    csvTask = new CsvTask();

                    //根据请求头中的字段userid,trialdate 去数据库查询该trail路径
                    //检查文件系统中trail路径里是否有.csv 文件
                    string[] trialUnique = (msg.Split('#')[1]).Split('_');
                    string[] path = service.getTrialPath(trialUnique);
                    bool ifReqCsvErr = false;
                    string reqCsvErrStr = "";
                    List<FileInfo> csvFiles = new List<FileInfo>();

                    #region Trial 记录检查
                    if (path == null) //服务端没有数据记录
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
                                if (csv.Name.Split('.')[1].Equals("csv") || csv.Name.Split('.')[1].Equals("Csv"))
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
                        catch { txtLog.AppendText("回应errreqcsv 信息时socket异常！\r\n"); }
                       
                        txtLog.AppendText("下载出错! 详细信息:\r\n" + reqCsvErrStr+"\r\n");
                        //重新等待下一次csv 请求
                        continue;
                    }
                    else //如果文件存在，正常回应，并开始发送数据线程
                    {
                        csvTask.CsvRunFlg = true;
                        csvTask.SummaryName = csvFiles[0].FullName;

                        try
                        {
                            //发送回应
                            msg = "resreqcsv:#" + csvTask.SummaryName + "#";
                            clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                            txtLog.AppendText("正常响应reqcsv:\r\n" + msg + "\r\n");

                            csvTask.ClientSock = clientSock;

                            Thread.Sleep(200);
                            //开启数据传输线程
                            Thread sendCsvTh = new Thread(sendCsvData);
                            sendCsvTh.IsBackground = true;
                            sendCsvTh.Start(csvTask);
                        }
                        catch { txtLog.AppendText("发送resreqcsv回应时，socket异常!\r\n"); }
                    }

                }
                //客户端接收过程中出错 ==>"errdnldcsv:#" + fileName + "#"
                if (msg.StartsWith("errdnldcsv:"))
                {
                    txtLog.AppendText("客户端接收出错:" + msg+"\r\n");
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
                        catch { txtLog.AppendText("发送errreqcsv 回应时socket异常！\r\n"); }

                        txtLog.AppendText("reqdbgfile出错! 详细信息:\r\n" + reqDbgErrStr+"\r\n");
                        //重新等待下一次请求
                        continue;
                    }
                    else
                    {
                        msg = "resreqdbgfile:#"+ unique + "#";
                        try
                        {
                            clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                            txtLog.AppendText("服务端回应 resreqdbgfile:#\r\n" + msg + "\r\n");
                        }
                        catch { txtLog.AppendText("回应resreqdbgfile 时socket异常!\r\n");continue; }

                        Thread.Sleep(400);
                        StringBuilder sb = new StringBuilder("");
                        string[] splits = null;
                        for(int i=0; i<dbgFiles.Count; i++)
                        {
                            //("fileid-" + i + "*" + debugFiles[i].FullName);
                            splits = dbgFiles[i].FullName.Split('\\');
                            sb.Append(i + "*" + splits[splits.Length-1].Replace('#', '@') + ",");
                        }
                        sb.Append("#"); //用来分开前面的文件列表#??????及后面多余的东西
                        msg = sb.ToString();
                        try
                        {
                            clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                            txtLog.AppendText("服务端回应了debug/文件:msg\r\n");
                        }
                        catch { txtLog.AppendText("发送dbg files list 时socket异常！\r\n"); }
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
                        clientSock.Receive(bunchIdBuf);
                        tmp = Encoding.UTF8.GetString(bunchIdBuf);
                        if (tmp.StartsWith("bunchfileids"))
                        {
                            txtLog.AppendText("接收bunchfileid 成功！\r\n" + tmp + "\r\n");

                            //检查完下载目录后回应客户端
                            List<string> reqIds = tmp.Split('#')[1].Split(',').ToList();
                            List<string> bunchFiles = new List<string>();

                            //获取数据库中该条trail的文件目录
                            string[] path = service.getTrialPath(trialUnique); //[0]trial/  [1]/trial/debug/
                            bool ifReqBunchErr = false;
                            string reqBunchErrStr = "";
                            List<FileInfo> dbgFiles = new List<FileInfo>(); // 用来装 debug/ 中所有文件

                            #region 检测Trail 记录合法性
                            if (path == null) // 数据库中文件路径不存在
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
                                    txtLog.AppendText("reqdbgfile出错! 详细信息:\r\n" + reqBunchErrStr + "\r\n");
                                }catch(Exception ex)
                                {
                                    txtLog.AppendText("回应errreqbunchfile时错误！" + ex.Message);
                                }

                                //重新等待下一次请求
                                continue;
                            }
                            else //检查合法，可以下载
                            {
                                //检查完成后，开启线程传输,continue
                                msg = "resreqbunchfile:#" + bunchFiles.Count + "#";
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
                                    txtLog.AppendText("回应reqbuchfile时发送错误！" + ex.Message);
                                }
                            }
                        }
                        else
                        {
                            txtLog.AppendText("接收bunchfileid 失败！\r\n" + msg + "\r\n");
                        }

                    }catch(Exception ex2)
                    {
                        txtLog.AppendText("接收bunchfileid 失败！" + ex2.Message);
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
            DirectoryInfo pltfmDir = new DirectoryInfo(dataPath + task.heads[4] + "\\");
            if (!pltfmDir.Exists) { Directory.CreateDirectory(pltfmDir.FullName); }

            DirectoryInfo pdctDir = new DirectoryInfo(pltfmDir.FullName + "\\" + task.heads[5] + "\\");
            if (!pdctDir.Exists) { Directory.CreateDirectory(pdctDir.FullName); }

            DirectoryInfo trialDir = new DirectoryInfo(pdctDir.FullName + "\\" + task.heads[3] + "\\");
            if (!trialDir.Exists) //文件不存在
            {
                Directory.CreateDirectory(trialDir.FullName);
            }
            else//要上传的文件夹已经存在
            {
                // 之前没有使用多次上传时的代码
                //string errUpld = "errupld:#";
                //clientSock.Send(Encoding.UTF8.GetBytes(errUpld.ToCharArray()));
                //return;

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
                Console.WriteLine("释放锁！");
                txtLog.AppendText("服务端删除已经存在的Trial文件夹\r\n");
            }

            //把client的文件存储为这个
            string zipFile = trialDir.FullName + "\\" + "data.zip";
            bool endFileFlg = false;
            int count = 0;
            using (FileStream fs = new FileStream(zipFile, FileMode.Create))
            {
                while (!endFileFlg)
                {
                    try
                    {
                        count = clientSock.Receive(fileBuf);
                    }
                    catch
                    {
                        Console.WriteLine("recvData(object obj):接收压缩文件异常！\r\n");
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
                            msg = "resend:#" + msg.Split('#')[1] + "#"; // end:# file_name #
                            clientSock.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));//response

                            txtLog.AppendText("client:" + msg + "\r\n");

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
                            catch {
                                endFileFlg = true;
                            }

                            txtLog.AppendText("保存文件:" + msg.Split('#')[1] + " 成功!\r\n");
                        }
                    } // else count < 128
                } // while(!endFileFlg)
            } // using(filestream fs= xxxx )

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
                    string real = real = f.Name.Substring(0, f.Name.LastIndexOf('.'));
                    if (real.Contains("csv") || real.Contains("Csv"))
                    {
                        DirectoryInfo tmp = new DirectoryInfo(debugDir.FullName + real + "\\");
                        if (!tmp.Exists) { Directory.CreateDirectory(tmp.FullName); }
                        ZipFile.ExtractToDirectory(f.FullName, tmp.FullName);
                    }

                    //>>> 然后删除zip 文件
                    File.Delete(f.FullName);
                }
            }
            txtLog.AppendText("服务端解压并删除文件成功!\r\n");
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

                byte[] msgBuf = new byte[200];
                byte[] fileBuf = null;

                string msg = null;

                lock (myLock)
                {
                    using (FileStream fs = new FileStream(file.FullName, FileMode.Open))
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
                            catch { txtLog.AppendText("发送data.zip数据时，socket 异常！\r\n"); }

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
                            catch { txtLog.AppendText("发送data.zip文件传输结束标识时socket 异常！\r\n"); }
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
                                catch { txtLog.AppendText("发送data.zip数据时,读取文件数据或者发送文件数据异常！\r\n"); }
                            }

                            //发送剩余的字节数
                            fileBuf = new byte[leftLen];
                            try
                            {
                                fs.Read(fileBuf, 0, leftLen);
                                socket.Send(fileBuf);
                            }
                            catch { txtLog.AppendText("发送data.zip最后一个段数据时，读取文件数据或者发送文件数据异常\r\n"); }

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
                            catch { txtLog.AppendText("发送data.zip 文件传输结束标识时,socket异常！\r\n"); }
                        }
                    }
                }//lock(myLock) 
                Console.WriteLine("释放锁");
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
            using(FileStream fs = new FileStream(file.FullName, FileMode.Open))
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
                    catch { txtLog.AppendText("一次发送summary.csv时，socket 异常！\r\n"); }

                    try { Thread.Sleep(500); }
                    catch { txtLog.AppendText("sleep error!"); }

                    //发送文件结束标志
                    msg = "endcsv:#" + file.Name + "#";
                    try
                    {
                        socket.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                        txtLog.AppendText("server 发送文件结束标志!:" + msg + "\r\n");
                    }
                    catch { txtLog.AppendText("发送summary.csv文件结束标识时，socket错误!\r\n"); }
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

                        if (!task.CsvRunFlg)
                        {
                            fs.Close();
                            return;
                        }
                        try
                        {
                            socket.Send(fileBuf);
                        }
                        catch { txtLog.AppendText("分段传输summary.csv数据时，socket异常!\r\n"); }
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
                    catch { txtLog.AppendText("分段传输最后的summary.csv数据时,socket异常！\r\n"); }

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
                    catch {
                        txtLog.AppendText("发送csv 文件结束标志时，socket异常!\r\n");
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
            } // using(FileStream fs = xxx )
        }

        //发送一堆文件
        private void bunchDnld(object bchTask)
        {
            BchDnldTask task = bchTask as BchDnldTask;
            Socket socket = task.clientSock;
            int sent = 0;
            int transMaxLen = 1024 * 512; //512k
            byte[] msgBuf = new byte[200];
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
                    msg = "singleinfo:#" + file.Length + "#"+file.Name.Replace('#', '@') + "#";
                    msgBuf = Encoding.UTF8.GetBytes(msg.ToCharArray());
                    try
                    {
                        socket.Send(msgBuf);
                    }catch(Exception ex)
                    {
                        txtLog.AppendText("singleinfo:发送失败！" + ex.Message);
                        break;
                    }

                    using (FileStream fs = new FileStream(file.FullName, FileMode.Open))
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
                            }catch(Exception ex)
                            {
                                txtLog.AppendText("一次性传送文件时socket错误!" + ex.Message);
                                break;
                            }
                            

                            //延时
                            try { Thread.Sleep(500); }
                            catch { Console.WriteLine("sleep error!"); }

                            //发送单个文件结束标志
                            msg = "singleend:#" + file.Name.Replace('#','@') + "#" + (task.bunchFiles.Count - sent -1) + "#"; // left count 
                            try
                            {
                                socket.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                                singleFileEnd = true;
                            }catch(Exception ex)
                            {
                                txtLog.AppendText("发送单个文件结束符时错误！" + ex.Message);
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
                                }catch(Exception ex)
                                {
                                    txtLog.AppendText("分段传输文件时，socket错误!" + ex.Message);
                                    break;
                                }
                            }

                            //发送剩余的字节数
                            fileBuf = new byte[leftLen];
                            fs.Read(fileBuf, 0, leftLen);
                            try
                            {
                                socket.Send(fileBuf);
                            }catch(Exception ex)
                            {
                                txtLog.AppendText("分段传输文件时，最后一次传输端口错误!" + ex.Message);
                                break;
                            }

                            //设置延时，使剩余文件信息和 文件结束标志分开发送
                            try { Thread.Sleep(500); }
                            catch { Console.WriteLine("sleep error!"); }

                            txtLog.AppendText(file.Name + " 数据传输完成!\n");

                            // 单个文件结束标志
                            msg = "singleend:#" + file.Name.Replace('#', '@') + "#" + (task.bunchFiles.Count - sent -1) + "#";
                            try
                            {
                                socket.Send(Encoding.UTF8.GetBytes(msg.ToCharArray()));
                                singleFileEnd = true;
                            }
                            catch(Exception ex)
                            {
                                txtLog.AppendText("传输单个文件结束符时错误！" + ex.Message);
                                break;
                            }
                        }
                    }//using(FileStream fs = new FileStream())
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
    }
}
