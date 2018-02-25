namespace DataSyncServ
{
    partial class FmServ
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.btStartListen = new System.Windows.Forms.Button();
            this.btStopListen = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labServStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labPort
            // 
            this.labPort.AutoSize = true;
            this.labPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labPort.Location = new System.Drawing.Point(91, 57);
            this.labPort.Name = "labPort";
            this.labPort.Size = new System.Drawing.Size(101, 24);
            this.labPort.TabIndex = 0;
            this.labPort.Text = "listen port";
            // 
            // txtPort
            // 
            this.txtPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtPort.Location = new System.Drawing.Point(210, 58);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(123, 28);
            this.txtPort.TabIndex = 1;
            // 
            // btStartListen
            // 
            this.btStartListen.BackColor = System.Drawing.Color.SkyBlue;
            this.btStartListen.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btStartListen.Location = new System.Drawing.Point(440, 56);
            this.btStartListen.Name = "btStartListen";
            this.btStartListen.Size = new System.Drawing.Size(121, 37);
            this.btStartListen.TabIndex = 2;
            this.btStartListen.Text = "start listen";
            this.btStartListen.UseVisualStyleBackColor = false;
            this.btStartListen.Click += new System.EventHandler(this.btStartListen_Click);
            // 
            // btStopListen
            // 
            this.btStopListen.BackColor = System.Drawing.Color.Silver;
            this.btStopListen.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btStopListen.Location = new System.Drawing.Point(610, 57);
            this.btStopListen.Name = "btStopListen";
            this.btStopListen.Size = new System.Drawing.Size(121, 38);
            this.btStopListen.TabIndex = 3;
            this.btStopListen.Text = "stop listen";
            this.btStopListen.UseVisualStyleBackColor = false;
            this.btStopListen.Click += new System.EventHandler(this.btStopListen_Click);
            // 
            // txtLog
            // 
            this.txtLog.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtLog.Location = new System.Drawing.Point(95, 144);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(636, 255);
            this.txtLog.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 494);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 17);
            this.label1.TabIndex = 5;
            this.label1.Text = "server status:";
            // 
            // labServStatus
            // 
            this.labServStatus.AutoSize = true;
            this.labServStatus.Location = new System.Drawing.Point(95, 494);
            this.labServStatus.Name = "labServStatus";
            this.labServStatus.Size = new System.Drawing.Size(0, 17);
            this.labServStatus.TabIndex = 6;
            // 
            // FmServ
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(908, 516);
            this.Controls.Add(this.labServStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btStopListen);
            this.Controls.Add(this.btStartListen);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.labPort);
            this.MaximizeBox = false;
            this.Name = "FmServ";
            this.Text = "DataSyncServ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FmServ_FormClosing);
            this.Load += new System.EventHandler(this.FmServ_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labPort;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.Button btStartListen;
        private System.Windows.Forms.Button btStopListen;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labServStatus;
    }
}

