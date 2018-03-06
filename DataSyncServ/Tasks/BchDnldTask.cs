using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncServ.Tasks
{
    public class BchDnldTask
    {
        public Socket clientSock; 
        public List<string> bunchFiles = null;

        public BchDnldTask(Socket clientSock,List<string> fileList)
        {
            this.clientSock = clientSock;
            bunchFiles = fileList;
        }
    }
}
