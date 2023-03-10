namespace ExcelToCode
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.configListBox = new System.Windows.Forms.CheckedListBox();
            this.logTb = new System.Windows.Forms.TextBox();
            this.errLogTb = new System.Windows.Forms.TextBox();
            this.ServerAll = new System.Windows.Forms.Button();
            this.ServerSingle = new System.Windows.Forms.Button();
            this.clearLogBtn = new System.Windows.Forms.Button();
            this.ClientAll = new System.Windows.Forms.Button();
            this.ClientSingle = new System.Windows.Forms.Button();
            this.clearErrLogBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // configListBox
            // 
            this.configListBox.FormattingEnabled = true;
            this.configListBox.Location = new System.Drawing.Point(1, 0);
            this.configListBox.Name = "configListBox";
            this.configListBox.Size = new System.Drawing.Size(632, 310);
            this.configListBox.TabIndex = 0;
            // 
            // logTb
            // 
            this.logTb.Location = new System.Drawing.Point(1, 357);
            this.logTb.Multiline = true;
            this.logTb.Name = "logTb";
            this.logTb.Size = new System.Drawing.Size(243, 203);
            this.logTb.TabIndex = 1;
            // 
            // errLogTb
            // 
            this.errLogTb.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.errLogTb.Location = new System.Drawing.Point(250, 357);
            this.errLogTb.Multiline = true;
            this.errLogTb.Name = "errLogTb";
            this.errLogTb.Size = new System.Drawing.Size(383, 203);
            this.errLogTb.TabIndex = 2;
            // 
            // ServerAll
            // 
            this.ServerAll.Location = new System.Drawing.Point(18, 316);
            this.ServerAll.Name = "ServerAll";
            this.ServerAll.Size = new System.Drawing.Size(82, 32);
            this.ServerAll.TabIndex = 3;
            this.ServerAll.Text = "服务器-ALL";
            this.ServerAll.UseVisualStyleBackColor = true;
            this.ServerAll.Click += new System.EventHandler(this.ServerAll_Click);
            // 
            // ServerSingle
            // 
            this.ServerSingle.Location = new System.Drawing.Point(114, 316);
            this.ServerSingle.Name = "ServerSingle";
            this.ServerSingle.Size = new System.Drawing.Size(82, 32);
            this.ServerSingle.TabIndex = 3;
            this.ServerSingle.Text = "服务器-单选";
            this.ServerSingle.UseVisualStyleBackColor = true;
            this.ServerSingle.Click += new System.EventHandler(this.ServerSingle_Click);
            // 
            // clearLogBtn
            // 
            this.clearLogBtn.Location = new System.Drawing.Point(210, 316);
            this.clearLogBtn.Name = "clearLogBtn";
            this.clearLogBtn.Size = new System.Drawing.Size(82, 32);
            this.clearLogBtn.TabIndex = 3;
            this.clearLogBtn.Text = "清理日志";
            this.clearLogBtn.UseVisualStyleBackColor = true;
            this.clearLogBtn.Click += new System.EventHandler(this.ClearLogBtn_Click);
            // 
            // ClientAll
            // 
            this.ClientAll.Location = new System.Drawing.Point(327, 317);
            this.ClientAll.Name = "ClientAll";
            this.ClientAll.Size = new System.Drawing.Size(82, 32);
            this.ClientAll.TabIndex = 3;
            this.ClientAll.Text = "客户端-ALL";
            this.ClientAll.UseVisualStyleBackColor = true;
            this.ClientAll.Click += new System.EventHandler(this.ClientAll_Click);
            // 
            // ClientSingle
            // 
            this.ClientSingle.Location = new System.Drawing.Point(424, 317);
            this.ClientSingle.Name = "ClientSingle";
            this.ClientSingle.Size = new System.Drawing.Size(82, 32);
            this.ClientSingle.TabIndex = 3;
            this.ClientSingle.Text = "客户端-单选";
            this.ClientSingle.UseVisualStyleBackColor = true;
            this.ClientSingle.Click += new System.EventHandler(this.ClientSingle_Click);
            // 
            // clearErrLogBtn
            // 
            this.clearErrLogBtn.Location = new System.Drawing.Point(521, 317);
            this.clearErrLogBtn.Name = "clearErrLogBtn";
            this.clearErrLogBtn.Size = new System.Drawing.Size(82, 32);
            this.clearErrLogBtn.TabIndex = 3;
            this.clearErrLogBtn.Text = "清理日志";
            this.clearErrLogBtn.UseVisualStyleBackColor = true;
            this.clearErrLogBtn.Click += new System.EventHandler(this.ClearErrLogBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 561);
            this.Controls.Add(this.clearErrLogBtn);
            this.Controls.Add(this.ClientSingle);
            this.Controls.Add(this.ClientAll);
            this.Controls.Add(this.clearLogBtn);
            this.Controls.Add(this.ServerSingle);
            this.Controls.Add(this.ServerAll);
            this.Controls.Add(this.errLogTb);
            this.Controls.Add(this.logTb);
            this.Controls.Add(this.configListBox);
            this.Name = "Form1";
            this.Text = "导表工具";
            this.Load += new System.EventHandler(this.OnFormLoaded);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CheckedListBox configListBox;
        private TextBox logTb;
        private TextBox errLogTb;
        private Button ServerAll;
        private Button ServerSingle;
        private Button clearLogBtn;
        private Button ClientAll;
        private Button ClientSingle;
        private Button clearErrLogBtn;
    }
}