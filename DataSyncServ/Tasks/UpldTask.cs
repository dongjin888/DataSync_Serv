using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncServ.Tasks
{
    public class UpldTask
    {
        public Socket clientSock;
        public string[] heads;

        public UpldTask(Socket client,string[] heads)
        {
            this.clientSock = client;
            this.heads = heads;
        }
    }
}
