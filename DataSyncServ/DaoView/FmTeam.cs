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
    public partial class FmTeam : Form
    {
        private DataService service;
        private FmDb parent;
        public FmTeam(DataService service,FmDb par)
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(combDepart.Text.Equals("")|| txtName.Text.Equals("") ||
                txtInfo.Text.Equals(""))
            {
                MessageBox.Show("Please complete the blank space !", "warning");
                return;
            }
            else
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("teamname", txtName.Text);
                dict.Add("departname", combDepart.Text);
                dict.Add("teaminfo", txtInfo.Text);
                if (service.add(dict, "tabteams"))
                {
                    MessageBox.Show("Save record ok !", "Add Team");
                    parent.resetTeamList();
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
