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
    public partial class FmUser : Form
    {
        private DataService service;
        private FmDb parent;

        public FmUser(DataService service,FmDb par)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
            this.service = service;
            parent = par;

            List<string> departs = service.getDepartStr();
            foreach(string de in departs)
            {
                combDepart.Items.Add(de);
            }
            combDepart.SelectedIndex = 0;
            combDepart.Text = departs[0];

            List<string> teams = service.getTeamStr(departs[0]);
            foreach(string t in teams)
            {
                combTeam.Items.Add(t);
            }
            if(teams != null && teams.Count > 0)
            {
                combTeam.SelectedIndex = 0;
                combTeam.Text = teams[0];
            }

            List<string> levs = service.getLevelStr();
            foreach(string lv in levs)
            {
                combLevel.Items.Add(lv);
            }
            if(levs != null && levs.Count > 0)
            {
                combLevel.SelectedIndex = 0;
                combLevel.Text = levs[0];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (combTeam.Text.Equals("") || combLevel.Text.Equals("") ||
                txtName.Text.Equals("") || txtUid.Text.Equals("") ||
                txtTel.Text.Equals(""))
            {
                MessageBox.Show("Please complete the blank space !", "warning");
                return;
            }
            else
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("userid", txtUid.Text);
                dict.Add("username", txtName.Text);
                dict.Add("teamname", combTeam.Text);
                dict.Add("usertel", txtTel.Text);
                dict.Add("userlevel", combLevel.Text);
                dict.Add("userimgpath", null);
                dict.Add("userinfo", txtInfo.Text);
                dict.Add("userpass", MyMd5.getMd5EncryptedStr(txtUid.Text));
                if (service.add(dict, "tabusers"))
                {
                    MessageBox.Show("Save record ok !", "Add User");
                    parent.resetUserList();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Save record failed !", "Add User");
                }
            }
        }

        private void combDepart_SelectedIndexChanged(object sender, EventArgs e)
        {
            string depart = (string)combDepart.SelectedItem;
            if(depart != null && !depart.Equals(""))
            {
                List<string> list = service.getTeamStr(depart);
                combTeam.Items.Clear();
                foreach(string te in list)
                {
                    combTeam.Items.Add(te);
                }
                if(list != null && list.Count > 0)
                {
                    combTeam.SelectedIndex = 0;
                    combTeam.Text = list[0];
                }
            }
        }
    }
}
