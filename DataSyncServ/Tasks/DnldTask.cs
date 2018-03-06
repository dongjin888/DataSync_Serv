using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncServ.Tasks
{
    public class DnldTask
    {
        public Socket clientSock;
        public string[] dnldInfo;
        public string dnldFileName;

        public DnldTask(Socket clientSock,string[] dnldInfo,string fileName)
        {
            this.clientSock = clientSock;
            this.dnldInfo = dnldInfo;
            this.dnldFileName = fileName;
        }
    }
}
