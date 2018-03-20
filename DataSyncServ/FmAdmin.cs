using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataSyncServ.Dao;
using DataSyncServ.Utils;

namespace DataSyncServ
{
    public partial class FmAdmin : Form
    {
        private DataService service;
        public FmAdmin(DataService service)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            this.service = service;
        }

        private void btLogin_Click(object sender, EventArgs e)
        {
            if(txtName.Text.Equals("") || txtPass.Text.Equals(""))
            {
                MessageBox.Show("Please input the name and password !", "error");
                return;
            }
            if (service.judgeAdmin(txtName.Text, txtPass.Text.Trim())){
                DialogResult = DialogResult.OK;
                Cache.admName = txtName.Text;
                this.Close();
            }
            else
            {
                MessageBox.Show("Admin name or password error !", "failed");
            }
        }
    }
}
