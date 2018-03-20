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
    public partial class User : UserControl
    {
        private DaoUser user;
        private DataService service;
        private FmDb parent;

        public User(DaoUser user,DataService service,FmDb par)
        {
            InitializeComponent();
            this.user = user;
            this.service = service;
            parent = par;

            labUid.Text = user.uid;
            labName.Text = user.name;
            labTeam.Text = user.team;
            labTel.Text = user.tel;
            labLev.Text = user.level;
            labInfo.Text = user.info;
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure delete this record ?", "Warning", MessageBoxButtons.YesNo)
                        == DialogResult.Yes)
            {
                if (service.delete("id", user.id, "tabusers"))
                {
                    MessageBox.Show("Delete ok !", "delete user");
                    parent.resetUserList();
                }
                else
                {
                    MessageBox.Show("Delete failed !", "delete user");
                }
            }
        }

        private void btChg_Click(object sender, EventArgs e)
        {

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
