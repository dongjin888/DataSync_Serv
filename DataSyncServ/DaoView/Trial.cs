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
using System.IO;

namespace DataSyncServ.DaoView
{
    public partial class Trial : UserControl
    {
        private DaoTrial trial;
        private DataService service;
        private FmDb parent;

        public Trial(DaoTrial trial,DataService service,FmDb par)
        {
            InitializeComponent();
            this.trial = trial;
            this.service = service;
            parent = par;

            labPltfm.Text = trial.pltfm;
            labPdct.Text = trial.pdct;
            labDate.Text = trial.date;
            labActivator.Text = trial.activator;
            labOperator.Text = trial.operater;
            labInfo.Text = trial.info;
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure delete this record ?", "Warning", MessageBoxButtons.YesNo)
                        == DialogResult.Yes)
            {
                if (service.delete("id", trial.id , "tabtrials"))
                {

                    //删除文件系统中的文件
                    string path = trial.dbgPath;
                    if (!Directory.Exists(path))
                    {
                        MessageBox.Show("FileSystem lost file data !", "error");
                    }
                    else
                    {
                        DirectoryInfo dir = new DirectoryInfo(path);
                        foreach(FileInfo f in dir.GetFiles())
                        {
                            try
                            {
                                File.Delete(f.FullName);
                            }
                            catch(Exception ex)
                            { Console.WriteLine("delete " + f.Name + " Exception!"+ex.Message); }
                        }

                        DirectoryInfo[] sonDirs = dir.GetDirectories();
                        foreach(DirectoryInfo d in sonDirs)
                        {
                            try
                            {
                                FileHandle.cycDeleteDir(d);
                            }
                            catch(Exception ex)
                            { Console.WriteLine("delete dir:" + dir.Name + " exception! "+ex.Message); }
                        }

                        try
                        {
                            Directory.Delete(path);
                        }
                        catch(Exception ex)
                        { Console.WriteLine("delete dir:" + path + " Exception!"+ex.Message); }
                    }

                    //删除本地文件系统中的文件
                    parent.resetTrialList();
                    MessageBox.Show("Delete ok !", "delete trial");
                }
                else
                {
                    MessageBox.Show("Delete failed !", "delete trial");
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
