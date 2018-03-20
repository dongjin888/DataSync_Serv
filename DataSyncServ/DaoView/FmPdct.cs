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
    public partial class FmPdct : Form
    {
        private DataService service;
        private FmDb parent;

        public FmPdct(DataService service,FmDb par)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
            this.service = service;
            parent = par;

            List<string> pltfms = service.getPltfmStr();
            foreach(string pltfm in pltfms)
            {
                combPltfm.Items.Add(pltfm);
            }
            combPltfm.SelectedIndex = 0;
            combPltfm.Text = pltfms[0];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(combPltfm.Text.Equals("")||txtName.Text.Equals("")
                 || txtInfo.Text.Equals(""))
            {
                MessageBox.Show("Complete the blank space !", "waraing");
                return;
            }
            else
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("pltfmname", combPltfm.Text);
                dict.Add("pdctname", txtName.Text.Trim());
                dict.Add("pdctinfo", txtInfo.Text.Trim());

                if (service.add(dict, "tabproducts"))
                {
                    MessageBox.Show("Save record ok !", "Add Product");
                    parent.resetPdctList();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Save record failed !", "Add Product");
                }
            }
        }
    }
}
