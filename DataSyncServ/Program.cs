using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DataSyncServ.Utils;

namespace DataSyncServ
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FileInfo cfg = new FileInfo(Environment.CurrentDirectory+"\\.servcfg.cfg");
            FileStream stream = new FileStream(cfg.FullName, FileMode.Open);
            StreamReader sr = new StreamReader(stream);
            string tmp = "";
            string[] parts = new string[4];
            int i = 0;
            while ((tmp = sr.ReadLine()) != null)
            {
                if (!tmp.StartsWith("#"))
                {
                    parts[i] = tmp.Split('%')[1];
                    Console.WriteLine("cfg:" + parts[i]);
                    i++;
                }
            }
            sr.Close();
            stream.Close();

            ContantInfo.Database.CONSQLSTR = parts[0];
            ContantInfo.SockServ.ip = parts[1];
            ContantInfo.SockServ.port = parts[2];
            ContantInfo.Fs.path = parts[3];

            Application.Run(new FmServ());
        }
    }
}
