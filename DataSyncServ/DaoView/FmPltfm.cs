using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataSyncServ.Utils;

namespace DataSyncServ.DaoView
{
    public partial class FmPltfm : Form
    {
        private DataService service;
        private FmDb parent;
        public FmPltfm(DataService service,FmDb par)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
            this.service = service;
            parent = par;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtName.Text.Equals("") || txtInfo.Text.Equals(""))
            {
                MessageBox.Show("Please complete the blank space !", "warning");
                return;
            }
            else
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("pltfmName", txtName.Text.Trim());
                dict.Add("pltfmInfo", txtInfo.Text.Trim());

                if (service.add(dict, "tabplatforms"))
                {
                    MessageBox.Show("Save record ok !", "Add Platform");
                    parent.resetPltfmList();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Save record failed !", "Add Platform");
                }
            }
        }
    }
}
