namespace DataSyncServ.DaoView
{
    partial class User
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labUid = new System.Windows.Forms.Label();
            this.labName = new System.Windows.Forms.Label();
            this.labTeam = new System.Windows.Forms.Label();
            this.labInfo = new System.Windows.Forms.Label();
            this.labLev = new System.Windows.Forms.Label();
            this.labTel = new System.Windows.Forms.Label();
            this.btChg = new System.Windows.Forms.Button();
            this.btDel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labUid
            // 
            this.labUid.AutoSize = true;
            this.labUid.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labUid.ForeColor = System.Drawing.Color.White;
            this.labUid.Location = new System.Drawing.Point(17, 13);
            this.labUid.Name = "labUid";
            this.labUid.Size = new System.Drawing.Size(52, 17);
            this.labUid.TabIndex = 0;
            this.labUid.Text = "label1";
            // 
            // labName
            // 
            this.labName.AutoSize = true;
            this.labName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labName.ForeColor = System.Drawing.Color.White;
            this.labName.Location = new System.Drawing.Point(200, 13);
            this.labName.Name = "labName";
            this.labName.Size = new System.Drawing.Size(52, 17);
            this.labName.TabIndex = 1;
            this.labName.Text = "label2";
            // 
            // labTeam
            // 
            this.labTeam.AutoSize = true;
            this.labTeam.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labTeam.ForeColor = System.Drawing.Color.White;
            this.labTeam.Location = new System.Drawing.Point(396, 13);
            this.labTeam.Name = "labTeam";
            this.labTeam.Size = new System.Drawing.Size(52, 17);
            this.labTeam.TabIndex = 2;
            this.labTeam.Text = "label3";
            // 
            // labInfo
            // 
            this.labInfo.AutoSize = true;
            this.labInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labInfo.ForeColor = System.Drawing.Color.White;
            this.labInfo.Location = new System.Drawing.Point(396, 53);
            this.labInfo.Name = "labInfo";
            this.labInfo.Size = new System.Drawing.Size(52, 17);
            this.labInfo.TabIndex = 5;
            this.labInfo.Text = "label4";
            // 
            // labLev
            // 
            this.labLev.AutoSize = true;
            this.labLev.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labLev.ForeColor = System.Drawing.Color.White;
            this.labLev.Location = new System.Drawing.Point(200, 53);
            this.labLev.Name = "labLev";
            this.labLev.Size = new System.Drawing.Size(52, 17);
            this.labLev.TabIndex = 4;
            this.labLev.Text = "label5";
            // 
            // labTel
            // 
            this.labTel.AutoSize = true;
            this.labTel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labTel.ForeColor = System.Drawing.Color.White;
            this.labTel.Location = new System.Drawing.Point(17, 53);
            this.labTel.Name = "labTel";
            this.labTel.Size = new System.Drawing.Size(53, 17);
            this.labTel.TabIndex = 3;
            this.labTel.Text = "labTel";
            // 
            // btChg
            // 
            this.btChg.Enabled = false;
            this.btChg.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btChg.Location = new System.Drawing.Point(604, 30);
            this.btChg.Name = "btChg";
            this.btChg.Size = new System.Drawing.Size(75, 23);
            this.btChg.TabIndex = 14;
            this.btChg.Text = "modify";
            this.btChg.UseVisualStyleBackColor = true;
            this.btChg.Click += new System.EventHandler(this.btChg_Click);
            this.btChg.MouseEnter += new System.EventHandler(this.btChg_MouseEnter);
            this.btChg.MouseLeave += new System.EventHandler(this.btChg_MouseLeave);
            // 
            // btDel
            // 
            this.btDel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btDel.Location = new System.Drawing.Point(494, 30);
            this.btDel.Name = "btDel";
            this.btDel.Size = new System.Drawing.Size(75, 23);
            this.btDel.TabIndex = 13;
            this.btDel.Text = "delete";
            this.btDel.UseVisualStyleBackColor = true;
            this.btDel.Click += new System.EventHandler(this.btDel_Click);
            this.btDel.MouseEnter += new System.EventHandler(this.btDel_MouseEnter);
            this.btDel.MouseLeave += new System.EventHandler(this.btDel_MouseLeave);
            // 
            // User
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Controls.Add(this.btChg);
            this.Controls.Add(this.btDel);
            this.Controls.Add(this.labInfo);
            this.Controls.Add(this.labLev);
            this.Controls.Add(this.labTel);
            this.Controls.Add(this.labTeam);
            this.Controls.Add(this.labName);
            this.Controls.Add(this.labUid);
            this.Name = "User";
            this.Size = new System.Drawing.Size(696, 85);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labUid;
        private System.Windows.Forms.Label labName;
        private System.Windows.Forms.Label labTeam;
        private System.Windows.Forms.Label labInfo;
        private System.Windows.Forms.Label labLev;
        private System.Windows.Forms.Label labTel;
        private System.Windows.Forms.Button btChg;
        private System.Windows.Forms.Button btDel;
    }
}
