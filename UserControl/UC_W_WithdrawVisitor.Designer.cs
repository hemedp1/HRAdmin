namespace HRAdmin.UserControl
{
    partial class UC_W_WithdrawVisitor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_W_WithdrawVisitor));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.button2 = new System.Windows.Forms.Button();
            this.gbExternal = new System.Windows.Forms.GroupBox();
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.cmbCompany = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnWithdraw = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            this.gbExternal.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.button2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1682, 57);
            this.flowLayoutPanel1.TabIndex = 130;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.White;
            this.button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Image = ((System.Drawing.Image)(resources.GetObject("button2.Image")));
            this.button2.Location = new System.Drawing.Point(3, 2);
            this.button2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(54, 46);
            this.button2.TabIndex = 103;
            this.button2.UseVisualStyleBackColor = false;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // gbExternal
            // 
            this.gbExternal.BackColor = System.Drawing.Color.White;
            this.gbExternal.Controls.Add(this.dtpStartDate);
            this.gbExternal.Controls.Add(this.cmbCompany);
            this.gbExternal.Controls.Add(this.label4);
            this.gbExternal.Controls.Add(this.label3);
            this.gbExternal.Controls.Add(this.label2);
            this.gbExternal.Controls.Add(this.label1);
            this.gbExternal.Controls.Add(this.btnWithdraw);
            this.gbExternal.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbExternal.Font = new System.Drawing.Font("Calibri", 14F);
            this.gbExternal.Location = new System.Drawing.Point(0, 57);
            this.gbExternal.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbExternal.Name = "gbExternal";
            this.gbExternal.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbExternal.Size = new System.Drawing.Size(1682, 289);
            this.gbExternal.TabIndex = 131;
            this.gbExternal.TabStop = false;
            this.gbExternal.Text = "Withdraw";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Font = new System.Drawing.Font("Calibri", 12F);
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartDate.Location = new System.Drawing.Point(217, 64);
            this.dtpStartDate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(281, 37);
            this.dtpStartDate.TabIndex = 145;
            // 
            // cmbCompany
            // 
            this.cmbCompany.Font = new System.Drawing.Font("Calibri", 12F);
            this.cmbCompany.FormattingEnabled = true;
            this.cmbCompany.Location = new System.Drawing.Point(217, 122);
            this.cmbCompany.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbCompany.Name = "cmbCompany";
            this.cmbCompany.Size = new System.Drawing.Size(281, 37);
            this.cmbCompany.TabIndex = 143;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Calibri", 13F);
            this.label4.Location = new System.Drawing.Point(188, 126);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 32);
            this.label4.TabIndex = 140;
            this.label4.Text = ":";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 13F);
            this.label3.Location = new System.Drawing.Point(70, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 32);
            this.label3.TabIndex = 139;
            this.label3.Text = "Company";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 13F);
            this.label2.Location = new System.Drawing.Point(190, 71);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 32);
            this.label2.TabIndex = 138;
            this.label2.Text = ":";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 13F);
            this.label1.Location = new System.Drawing.Point(70, 71);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 32);
            this.label1.TabIndex = 137;
            this.label1.Text = "Start date";
            // 
            // btnWithdraw
            // 
            this.btnWithdraw.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnWithdraw.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnWithdraw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWithdraw.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnWithdraw.Location = new System.Drawing.Point(358, 180);
            this.btnWithdraw.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnWithdraw.Name = "btnWithdraw";
            this.btnWithdraw.Size = new System.Drawing.Size(140, 42);
            this.btnWithdraw.TabIndex = 131;
            this.btnWithdraw.Text = "Withdraw";
            this.btnWithdraw.UseVisualStyleBackColor = true;
            this.btnWithdraw.Click += new System.EventHandler(this.btnWithdraw_Click);
            // 
            // UC_W_WithdrawVisitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.gbExternal);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "UC_W_WithdrawVisitor";
            this.Size = new System.Drawing.Size(1682, 771);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.gbExternal.ResumeLayout(false);
            this.gbExternal.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox gbExternal;
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.ComboBox cmbCompany;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnWithdraw;
    }
}
