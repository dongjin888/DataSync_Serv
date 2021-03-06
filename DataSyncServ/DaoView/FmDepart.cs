﻿using System;
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
    public partial class FmDepart : Form
    {
        private DataService service;
        private FmDb parent;
        public FmDepart(DataService service,FmDb par)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
            this.service = service;
            parent = par;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(txtName.Text == "" || txtInfo.Text == null)
            {
                MessageBox.Show("Complete the blank space !", "waraing");
                return;
            }
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("departmentname", txtName.Text.Trim());
            dict.Add("departmentinfo", txtInfo.Text.Trim());
            
            if(service.add(dict, "tabdepartments"))
            {
                MessageBox.Show("Save record ok !", "Add Department");
                parent.resetDepartList();
                this.Close();
            }
            else
            {
                MessageBox.Show("Save record failed !", "Add Department");
            }
        }
    }
}
