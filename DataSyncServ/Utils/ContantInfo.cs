using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncServ.Utils
{
    public static class ContantInfo
    {
        public static class Database
        {
            public static string CONSQLSTR = "server=localhost;database=datasync;uid=root;pwd=Sql@My_!;charset=utf8;";
        }

        public static class SockServ
        {
            //public static string ip = "192.168.0.101";
            public static string ip = "10.113.200.34";
            public static string port = "5000";
        }

        public static class Fs
        {
            public static string path = @"c:\upload\";
        }
    }
}
