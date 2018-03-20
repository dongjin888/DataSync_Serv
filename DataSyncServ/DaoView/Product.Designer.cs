namespace DataSyncServ.DaoView
{
    partial class Product
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
            this.btChg = new System.Windows.Forms.Button();
            this.btDel = new System.Windows.Forms.Button();
            this.labInfo = new System.Windows.Forms.Label();
            this.labName = new System.Windows.Forms.Label();
            this.labPltfm = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btChg
            // 
            this.btChg.Enabled = false;
            this.btChg.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btChg.Location = new System.Drawing.Point(584, 8);
            this.btChg.Name = "btChg";
            this.btChg.Size = new System.Drawing.Size(75, 23);
            this.btChg.TabIndex = 7;
            this.btChg.Text = "modify";
            this.btChg.UseVisualStyleBackColor = true;
            this.btChg.Click += new System.EventHandler(this.btChg_Click);
            this.btChg.MouseEnter += new System.EventHandler(this.btChg_MouseEnter);
            this.btChg.MouseLeave += new System.EventHandler(this.btChg_MouseLeave);
            // 
            // btDel
            // 
            this.btDel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btDel.Location = new System.Drawing.Point(474, 8);
            this.btDel.Name = "btDel";
            this.btDel.Size = new System.Drawing.Size(75, 23);
            this.btDel.TabIndex = 6;
            this.btDel.Text = "delete";
            this.btDel.UseVisualStyleBackColor = true;
            this.btDel.Click += new System.EventHandler(this.btDel_Click);
            this.btDel.MouseEnter += new System.EventHandler(this.btDel_MouseEnter);
            this.btDel.MouseLeave += new System.EventHandler(this.btDel_MouseLeave);
            // 
            // labInfo
            // 
            this.labInfo.AutoSize = true;
            this.labInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labInfo.ForeColor = System.Drawing.Color.White;
            this.labInfo.Location = new System.Drawing.Point(275, 11);
            this.labInfo.Name = "labInfo";
            this.labInfo.Size = new System.Drawing.Size(52, 17);
            this.labInfo.TabIndex = 5;
            this.labInfo.Text = "label2";
            // 
            // labName
            // 
            this.labName.AutoSize = true;
            this.labName.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labName.ForeColor = System.Drawing.Color.White;
            this.labName.Location = new System.Drawing.Point(28, 11);
            this.labName.Name = "labName";
            this.labName.Size = new System.Drawing.Size(52, 17);
            this.labName.TabIndex = 4;
            this.labName.Text = "label1";
            // 
            // labPltfm
            // 
            this.labPltfm.AutoSize = true;
            this.labPltfm.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labPltfm.ForeColor = System.Drawing.Color.White;
            this.labPltfm.Location = new System.Drawing.Point(143, 13);
            this.labPltfm.Name = "labPltfm";
            this.labPltfm.Size = new System.Drawing.Size(52, 17);
            this.labPltfm.TabIndex = 8;
            this.labPltfm.Text = "label1";
            // 
            // Product
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.Controls.Add(this.labPltfm);
            this.Controls.Add(this.btChg);
            this.Controls.Add(this.btDel);
            this.Controls.Add(this.labInfo);
            this.Controls.Add(this.labName);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "Product";
            this.Size = new System.Drawing.Size(696, 40);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btChg;
        private System.Windows.Forms.Button btDel;
        private System.Windows.Forms.Label labInfo;
        private System.Windows.Forms.Label labName;
        private System.Windows.Forms.Label labPltfm;
    }
}
