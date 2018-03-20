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
    public partial class Product : UserControl
    {
        private DaoProduct pdct;
        private DataService service;
        private FmDb parent;

        public Product(DaoProduct pdct,DataService service,FmDb par)
        {
            InitializeComponent();
            this.pdct = pdct;
            this.service = service;
            parent = par;

            labName.Text = pdct.name;
            labPltfm.Text = pdct.pltfm;
            labInfo.Text = pdct.info;
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure delete this record ?", "Warning", MessageBoxButtons.YesNo)
                        == DialogResult.Yes)
            {
                if (service.delete("pdctId", pdct.id, "tabproducts"))
                {
                    MessageBox.Show("Delete ok !", "delete product");
                    parent.resetPdctList();
                }
                else
                {
                    MessageBox.Show("Delete failed !", "delete product");
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
