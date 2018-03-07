using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSyncServ.Utils
{
    public class TrialInfo
    {
        private string activator; //发起者，工程师
        private string opertor;
        private string unique;
        private string pltfm;
        private string pdct;
        private string info;
        private string other;

        #region properties
        public string Activator
        {
            set { activator = value; }
            get { return activator; }
        }
        public string Operator
        {
            set { opertor = value; }
            get { return opertor; }
        }
        public string Unique
        {
            set { unique = value; }
            get { return unique; }
        }
        public string Pltfm
        {
            set { pltfm = value; }
            get { return pltfm; }
        }
        public string Pdct
        {
            set { pdct = value; }
            get { return pdct; }
        }
        public string Info
        {
            set { info = value; }
            get { return info; }
        }
        public string Other
        {
            set { other = value; }
            get { return other; }
        }
        #endregion

        public TrialInfo()
        {
            activator = Activator;
            opertor = Operator;
            unique = Unique;
            pltfm = Pltfm;
            pdct = Pdct;
            info = Info;
            other = Other;
        }
    }
}
