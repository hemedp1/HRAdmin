namespace HRAdmin.UserControl
{
    partial class UC_Meal_ViewReport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_Meal_ViewReport));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.button2 = new System.Windows.Forms.Button();
            this.gbExternal = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.label52 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.label48 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.dtpToDate = new System.Windows.Forms.DateTimePicker();
            this.dtpFromDate = new System.Windows.Forms.DateTimePicker();
            this.cmbOrderIds = new System.Windows.Forms.ComboBox();
            this.btnViewPDF = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            this.gbExternal.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.flowLayoutPanel1.Controls.Add(this.button2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1416, 58);
            this.flowLayoutPanel1.TabIndex = 124;
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
            this.gbExternal.Controls.Add(this.label3);
            this.gbExternal.Controls.Add(this.label2);
            this.gbExternal.Controls.Add(this.label1);
            this.gbExternal.Controls.Add(this.comboBox1);
            this.gbExternal.Controls.Add(this.label52);
            this.gbExternal.Controls.Add(this.label50);
            this.gbExternal.Controls.Add(this.label48);
            this.gbExternal.Controls.Add(this.label47);
            this.gbExternal.Controls.Add(this.label45);
            this.gbExternal.Controls.Add(this.dtpToDate);
            this.gbExternal.Controls.Add(this.dtpFromDate);
            this.gbExternal.Controls.Add(this.cmbOrderIds);
            this.gbExternal.Controls.Add(this.btnViewPDF);
            this.gbExternal.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbExternal.Font = new System.Drawing.Font("Calibri", 14F);
            this.gbExternal.Location = new System.Drawing.Point(0, 58);
            this.gbExternal.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbExternal.Name = "gbExternal";
            this.gbExternal.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbExternal.Size = new System.Drawing.Size(1416, 375);
            this.gbExternal.TabIndex = 125;
            this.gbExternal.TabStop = false;
            this.gbExternal.Text = "View Report";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 13F);
            this.label3.Location = new System.Drawing.Point(70, 279);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(141, 32);
            this.label3.TabIndex = 139;
            this.label3.Text = "View report";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 13F);
            this.label2.Location = new System.Drawing.Point(280, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 32);
            this.label2.TabIndex = 138;
            this.label2.Text = ":";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 13F);
            this.label1.Location = new System.Drawing.Point(70, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 32);
            this.label1.TabIndex = 137;
            this.label1.Text = "Occasion";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Font = new System.Drawing.Font("Calibri", 13F);
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(307, 64);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(281, 40);
            this.comboBox1.TabIndex = 136;
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Font = new System.Drawing.Font("Calibri", 13F);
            this.label52.Location = new System.Drawing.Point(280, 212);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(21, 32);
            this.label52.TabIndex = 135;
            this.label52.Text = ":";
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Font = new System.Drawing.Font("Calibri", 13F);
            this.label50.Location = new System.Drawing.Point(280, 279);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(21, 32);
            this.label50.TabIndex = 133;
            this.label50.Text = ":";
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Font = new System.Drawing.Font("Calibri", 13F);
            this.label48.Location = new System.Drawing.Point(280, 142);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(21, 32);
            this.label48.TabIndex = 132;
            this.label48.Text = ":";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Font = new System.Drawing.Font("Calibri", 13F);
            this.label47.Location = new System.Drawing.Point(70, 212);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(181, 32);
            this.label47.TabIndex = 130;
            this.label47.Text = "To request date";
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Font = new System.Drawing.Font("Calibri", 13F);
            this.label45.Location = new System.Drawing.Point(70, 142);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(212, 32);
            this.label45.TabIndex = 129;
            this.label45.Text = "From request date";
            // 
            // dtpToDate
            // 
            this.dtpToDate.Font = new System.Drawing.Font("Calibri", 13F);
            this.dtpToDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpToDate.Location = new System.Drawing.Point(307, 205);
            this.dtpToDate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtpToDate.Name = "dtpToDate";
            this.dtpToDate.Size = new System.Drawing.Size(281, 39);
            this.dtpToDate.TabIndex = 128;
            // 
            // dtpFromDate
            // 
            this.dtpFromDate.Font = new System.Drawing.Font("Calibri", 13F);
            this.dtpFromDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpFromDate.Location = new System.Drawing.Point(307, 135);
            this.dtpFromDate.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtpFromDate.Name = "dtpFromDate";
            this.dtpFromDate.Size = new System.Drawing.Size(281, 39);
            this.dtpFromDate.TabIndex = 127;
            // 
            // cmbOrderIds
            // 
            this.cmbOrderIds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOrderIds.DropDownWidth = 200;
            this.cmbOrderIds.Font = new System.Drawing.Font("Calibri", 13F);
            this.cmbOrderIds.FormattingEnabled = true;
            this.cmbOrderIds.IntegralHeight = false;
            this.cmbOrderIds.Location = new System.Drawing.Point(307, 275);
            this.cmbOrderIds.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbOrderIds.Name = "cmbOrderIds";
            this.cmbOrderIds.Size = new System.Drawing.Size(281, 40);
            this.cmbOrderIds.TabIndex = 24;
            // 
            // btnViewPDF
            // 
            this.btnViewPDF.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnViewPDF.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnViewPDF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewPDF.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnViewPDF.Location = new System.Drawing.Point(657, 276);
            this.btnViewPDF.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnViewPDF.Name = "btnViewPDF";
            this.btnViewPDF.Size = new System.Drawing.Size(140, 42);
            this.btnViewPDF.TabIndex = 125;
            this.btnViewPDF.Text = "View Report";
            this.btnViewPDF.UseVisualStyleBackColor = true;
            this.btnViewPDF.Click += new System.EventHandler(this.btnViewPDF_Click);
            // 
            // UC_Meal_ViewReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.gbExternal);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "UC_Meal_ViewReport";
            this.Size = new System.Drawing.Size(1416, 919);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.gbExternal.ResumeLayout(false);
            this.gbExternal.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox gbExternal;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label52;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.DateTimePicker dtpToDate;
        private System.Windows.Forms.DateTimePicker dtpFromDate;
        private System.Windows.Forms.ComboBox cmbOrderIds;
        private System.Windows.Forms.Button btnViewPDF;
    }
}
