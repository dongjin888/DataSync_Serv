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
    public partial class FmLevel : Form
    {
        private DataService service;
        private FmDb parent;
        public FmLevel(DataService service,FmDb par)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
            this.service = service;
            parent = par;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtName.Text == "" || txtInfo.Text == null)
            {
                MessageBox.Show("Complete the blank space !", "waraing");
                return;
            }
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("levelname", txtName.Text.Trim());
            dict.Add("levelinfo", txtInfo.Text.Trim());

            if (service.add(dict, "tabuserlevels"))
            {
                MessageBox.Show("Save record ok !", "Add Department");
                parent.resetLevList();
                this.Close();
            }
            else
            {
                MessageBox.Show("Save record failed !", "Add Department");
            }
        }
    }
}
