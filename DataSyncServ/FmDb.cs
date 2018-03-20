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
using DataSyncServ.DaoView;
using DataSyncServ.Utils;

namespace DataSyncServ
{
    public partial class FmDb : Form
    {
        private List<string> tables = new List<string>();
        private List<PaintEventHandler> panPaints = new List<PaintEventHandler>();
        private Dictionary<string,Panel> dictPans = new Dictionary<string, Panel>();
        private Dictionary<string,PictureBox> picboxes = new Dictionary<string,PictureBox>();

        private List<DaoDepartment> departList = new List<DaoDepartment>();
        private List<DaoPlatform> pltfmList = new List<DaoPlatform>();
        private List<DaoProduct> pdctList = new List<DaoProduct>();
        private List<DaoTeam> teamList = new List<DaoTeam>();
        private List<DaoUser> userList = new List<DaoUser>();
        private List<DaoLevel> levelList = new List<DaoLevel>();
        private List<DaoTrial> trialList = new List<DaoTrial>();

        private DataService service;

        TextBox txtOld = null;
        TextBox txtPass = null;

        int startx = 125;
        int y = 60;
        int gap = 20;

        int addx = 30;
        int addy = 30;
        int width = 40;

        public FmDb(DataService service)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

            tables.Add("department");
            panPaints.Add(panDepartPaint);

            tables.Add("platform");
            panPaints.Add(panPltfmPaint);

            tables.Add("product");
            panPaints.Add(panPdctPaint);

            tables.Add("team");
            panPaints.Add(panTeamPaint);

            tables.Add("user");
            panPaints.Add(panUserPaint);

            tables.Add("level");
            panPaints.Add(panLevelPaint);

            tables.Add("trial");
            panPaints.Add(panTrialPaint);

            initPicbox();

            this.service = service;
        }

        private void FmDb_Load(object sender, EventArgs e)
        {
            int r = 100;
            int g = 100;
            int b = 100;
            int incre = 10;
            tabControl1.TabPages.Clear();
            string tab = null;
            for (int i = 0; i < tables.Count; i++)
            {
                tab = tables[i];
                TabPage page = new TabPage(tab);
                Panel pan = new Panel();
                pan.Dock = DockStyle.Fill;
                pan.AutoScroll = true;
                pan.Name = tables[i];
                pan.BackColor = Color.FromArgb(r, g, b);
                pan.Paint += new PaintEventHandler(panPaints[i]);
                pan.Margin = new Padding(0, 0, 0, 30);
                r += incre; b += incre; g += incre;
                page.Controls.Add(pan);
                dictPans.Add(tab, pan);
                tabControl1.TabPages.Add(page);
            }

            //加一页管理员密码修改
            TabPage admPg = new TabPage("admin");
            Panel panAdm = new Panel();
            panAdm.Dock = DockStyle.Fill;
            panAdm.AutoScroll = true;
            panAdm.Name = "admin";
            panAdm.BackColor = Color.Gray;
            panAdm.Margin = new Padding(0, 0, 0, 30);
            //在panAdm 中添加控件
            initAdmPan(panAdm);

            admPg.Controls.Add(panAdm);
            tabControl1.Controls.Add(admPg);
        }

        private void initAdmPan(Panel pan)
        {
            Label label1 = new Label();
            label1.AutoSize = true;
            label1.BackColor = Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            label1.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(134)));
            label1.ForeColor = Color.White;
            label1.Location = new Point(244, 129);
            label1.Name = "label1";
            label1.Size = new Size(162, 24);
            label1.TabIndex = 0;
            label1.Text = "old password";

            txtOld = new TextBox();
            txtOld.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold,GraphicsUnit.Point, ((byte)(134)));
            txtOld.Location = new Point(385, 129);
            txtOld.Name = "txtOld";
            txtOld.PasswordChar = '*';
            txtOld.Size = new Size(187, 28);
            txtOld.TabIndex = 2;

            Label label2 = new Label();
            label2.AutoSize = true;
            label2.BackColor = Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            label2.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(134)));
            label2.ForeColor = Color.White;
            label2.Location = new Point(237, 187);
            label2.Name = "label2";
            label2.Size = new Size(162, 24);
            label2.TabIndex = 1;
            label2.Text = "new password";

            txtPass = new TextBox();
            txtPass.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(134)));
            txtPass.Location = new Point(385, 187);
            txtPass.Name = "txtPass";
            txtPass.PasswordChar = '*';
            txtPass.Size = new Size(187, 28);
            txtPass.TabIndex = 3;

            Button btChg = new Button();
            btChg.Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(134)));
            btChg.ForeColor = Color.Red;
            btChg.Location = new Point(322, 252);
            btChg.Name = "btLogin";
            btChg.Size = new Size(105, 33);
            btChg.TabIndex = 4;
            btChg.Text = "Change";
            btChg.UseVisualStyleBackColor = true;
            btChg.Click += new EventHandler(btChg_Click);

            pan.Controls.Add(label1);
            pan.Controls.Add(txtOld);
            pan.Controls.Add(label2);
            pan.Controls.Add(txtPass);
            pan.Controls.Add(btChg);
        }

        private void btChg_Click(object sender, EventArgs e)
        {
            if (Cache.admName.Equals(""))
            {
                MessageBox.Show("Illegal login !", "error");
                return;
            }
            if(txtOld.Text.Equals("") || txtPass.Text.Equals(""))
            {
                MessageBox.Show("Input not completed !", "error");
                return;
            }

            if (service.judgeAdmin(Cache.admName, txtOld.Text))
            {
                if (service.chgAdmPass(Cache.admName, txtPass.Text))
                {
                    MessageBox.Show("Admin password change success !", "message");
                    txtOld.Text = "";
                    txtPass.Text = "";
                }
                else
                {
                    MessageBox.Show("Admin password change failed !", "failed");
                }
            }
            else
            {
                MessageBox.Show("Old password not right !", "error");
            }
        }

        private void initPicbox()
        {
            int i = 0;
            foreach(string name in tables)
            {
                if (i == tables.Count - 1)
                    break;
                PictureBox box = new PictureBox();
                box.Image = Properties.Resources.addLv;
                box.Location = new Point(addx, addy);
                box.Size = new Size(width, width);
                box.SizeMode = PictureBoxSizeMode.Zoom;
                box.TabIndex = i;
                box.TabStop = false;
                box.Name = name;
                box.Click += new EventHandler(picbox_Click);
                box.MouseEnter += new EventHandler(picbox_MouseEnter);
                box.MouseLeave += new EventHandler(picbox_MouseLeave);
                picboxes.Add(name,box);
                i++;
            }
        }
        private void picbox_MouseEnter(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;
            pic.Image = Properties.Resources.addHv;
        }
        private void picbox_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;
            pic.Image = Properties.Resources.addLv;
        }
        private void picbox_Click(object sender, EventArgs e)
        {
            PictureBox pic = sender as PictureBox;
            string name = pic.Name;
            //弹出修改框
            if (name.Equals("department"))
            {
                FmDepart fm = new FmDepart(service,this);
                if(fm.ShowDialog(this) == DialogResult.OK)
                {
                    //pan 中添加记录
                }
            }

            if (name.Equals("platform"))
            {
                FmPltfm fm = new FmPltfm(service,this);
                if(fm.ShowDialog(this) == DialogResult.OK)
                {
                    //p
                }
            }

            if (name.Equals("product"))
            {
                FmPdct fm = new FmPdct(service,this);
                if (fm.ShowDialog(this) == DialogResult.OK)
                {

                }
            }

            if (name.Equals("team"))
            {
                FmTeam fm = new FmTeam(service,this);
                if(fm.ShowDialog(this) == DialogResult.OK)
                {

                }
            }

            if (name.Equals("user"))
            {
                FmUser fm = new FmUser(service,this);
                if(fm.ShowDialog(this) == DialogResult)
                {

                }
            }
            if (name.Equals("level"))
            {
                FmLevel fm = new FmLevel(service,this);
                if(fm.ShowDialog() == DialogResult)
                {
                    
                }
            }
        }

        //每个
        private void panDepartPaint(object sender, PaintEventArgs e)
        {
            Panel pan = sender as Panel;
            int starty = y;
            if (departList.Count == 0)
            {
                pan.Controls.Add(picboxes[pan.Name]);
                departList = service.getAllDepartments();
                foreach (DaoDepartment de in departList)
                {
                    Depart depart = new Depart(de,service,this);
                    depart.Location = new Point(startx, starty);
                    starty += depart.Height + gap;
                    pan.Controls.Add(depart);
                }
            }
            else
            {
                Console.WriteLine(pan.Name+" panel repaint");
            }
        }
        private void panPltfmPaint(object sender, PaintEventArgs e)
        {
            Panel pan = sender as Panel;
            int starty = y;
            if (pltfmList.Count == 0)
            {
                pan.Controls.Add(picboxes[pan.Name]);
                pltfmList = service.getAllPltfms();
                foreach (DaoPlatform plt in pltfmList)
                {
                    Platform plat = new Platform(plt,service,this);
                    plat.Location = new Point(startx, starty);
                    starty += plat.Height + gap;
                    pan.Controls.Add(plat);
                }
            }
            else
            {
                Console.WriteLine(pan.Name + " panel repaint");
            }
        }
        private void panPdctPaint(object sender, PaintEventArgs e)
        {
            Panel pan = sender as Panel;
            int starty = y;
            if(pdctList.Count == 0)
            {
                pan.Controls.Add(picboxes[pan.Name]);
                pdctList = service.getAllpdcts();
                foreach (DaoProduct pdct in pdctList)
                {
                    Product p = new Product(pdct,service,this);
                    p.Location = new Point(startx, starty);
                    starty += p.Height + gap;
                    pan.Controls.Add(p);
                }
            }
            else
            {
                Console.WriteLine(pan.Name + " panel repaint");
            }
        }
        private void panTeamPaint(object sender, PaintEventArgs e)
        {
            Panel pan = sender as Panel;
            int starty = y;
            if(teamList.Count == 0)
            {
                pan.Controls.Add(picboxes[pan.Name]);
                teamList = service.getAllTeams();
                foreach (DaoTeam t in teamList)
                {
                    Team team = new Team(t,service,this);
                    team.Location = new Point(startx, starty);
                    starty += team.Height + gap;
                    pan.Controls.Add(team);
                }
            }else
            {
                Console.WriteLine(pan.Name+" panel repaint");
            }
        }
        private void panUserPaint(object sender, PaintEventArgs e)
        {
            Panel pan = sender as Panel;
            int starty = y;
            if(userList.Count == 0)
            {
                pan.Controls.Add(picboxes[pan.Name]);
                userList = service.getAllUsers();
                foreach (DaoUser u in userList)
                {
                    User user = new User(u,service,this);
                    user.Location = new Point(startx, starty);
                    starty += user.Height + gap;
                    pan.Controls.Add(user);
                }
            }
            else
            {
                Console.WriteLine(pan.Name + " panel repaint");
            }
        }
        private void panLevelPaint(object sender, PaintEventArgs e)
        {
            Panel pan = sender as Panel;
            int starty = y;
            if(levelList.Count == 0)
            {
                pan.Controls.Add(picboxes[pan.Name]);
                levelList = service.getAllLevels();
                foreach (DaoLevel lev in levelList)
                {
                    Level level = new Level(lev,service,this);
                    level.Location = new Point(startx, starty);
                    starty += level.Height + gap;
                    pan.Controls.Add(level);
                }
            }
            else
            {
                Console.WriteLine(pan.Name + " panel repaint");
            }
        }
        private void panTrialPaint(object sender, PaintEventArgs e)
        {
            Panel pan = sender as Panel;
            int starty = y;
            if(trialList.Count == 0)
            {
                trialList = service.getAllTrials();
                foreach (DaoTrial t in trialList)
                {
                    Trial trial = new Trial(t,service,this);
                    trial.Location = new Point(startx, starty);
                    starty += trial.Height + gap;
                    pan.Controls.Add(trial);
                }
            }
            else
            {
                Console.WriteLine(pan.Name + " panel repaint");
            }
        }

        public void resetDepartList()
        {
            departList.Clear();
            dictPans["department"].Controls.Clear();
            dictPans["department"].Refresh();
        }
        public void resetPltfmList()
        {
            pltfmList.Clear();
            dictPans["platform"].Controls.Clear();
            dictPans["platform"].Refresh();
        }
        public void resetPdctList()
        {
            pdctList.Clear();
            dictPans["product"].Controls.Clear();
            dictPans["product"].Refresh();
        }
        public void resetTeamList()
        {
            teamList.Clear();
            dictPans["team"].Controls.Clear();
            dictPans["team"].Refresh();
        }
        public void resetUserList()
        {
            userList.Clear();
            dictPans["user"].Controls.Clear();
            dictPans["user"].Refresh();
        }
        public void resetLevList()
        {
            levelList.Clear();
            dictPans["level"].Controls.Clear();
            dictPans["level"].Refresh();
        }
        public void resetTrialList()
        {
            trialList.Clear();
            dictPans["trial"].Controls.Clear();
            dictPans["trial"].Refresh();
        }
    }
}
