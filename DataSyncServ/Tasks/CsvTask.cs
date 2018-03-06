using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncServ.Tasks
{
    public class CsvTask
    {
        private volatile bool csvRunFlg = false;  // csv dnld
        private string summaryName = null; //csv dnld
        private Socket clientSock;

        public bool CsvRunFlg
        {
            set { csvRunFlg = value; }
            get { return csvRunFlg; }
        }

        public string SummaryName
        {
            set { summaryName = value; }
            get { return summaryName; }
        }

        public Socket ClientSock
        {
            set { clientSock = value; }
            get { return clientSock; }
        }

        public CsvTask()
        {

        }
    }
}
