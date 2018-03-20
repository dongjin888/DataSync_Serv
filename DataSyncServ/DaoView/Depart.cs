using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataSyncServ.Dao;
using DataSyncServ.Utils;

namespace DataSyncServ.DaoView
{
    public partial class Depart : UserControl
    {
        private DaoDepartment depart;
        private DataService service;
        private FmDb parent;
        public Depart(DaoDepartment depart,DataService service,FmDb par)
        {
            InitializeComponent();
            this.depart = depart;
            this.service = service;
            parent = par;

            labName.Text = depart.name;
            labInfo.Text = depart.info;
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Are you sure delete this record ?","Warning",MessageBoxButtons.YesNo)
                        == DialogResult.Yes)
            {
                if(service.delete("id", depart.id, "tabdepartments"))
                {
                    MessageBox.Show("Delete ok !", "delete department");
                    parent.resetDepartList();
                }
                else
                {
                    MessageBox.Show("Delete failed !", "delete department");
                }
            }
        }

        private void btChg_Click(object sender, EventArgs e)
        {
            //弹出修改框
        }

        private void btDel_MouseEnter(object sender, EventArgs e)
        {
            btDel.BackColor = Color.Red;
            btDel.ForeColor = Color.White;
        }

        private void btDel_MouseLeave(object sender, EventArgs e)
        {
            btDel.BackColor = Color.White;
            btDel.ForeColor = Color.Black;
        }

        private void btChg_MouseEnter(object sender, EventArgs e)
        {
            btChg.BackColor = Color.Red;
            btChg.ForeColor = Color.White;
        }

        private void btChg_MouseLeave(object sender, EventArgs e)
        {
            btChg.BackColor = Color.White;
            btChg.ForeColor = Color.Black;
        }
    }
}
