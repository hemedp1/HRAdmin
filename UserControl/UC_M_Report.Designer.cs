namespace HRAdmin.UserControl
{
    partial class UC_M_Report
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_M_Report));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnBack = new System.Windows.Forms.Button();
            this.gbExternal = new System.Windows.Forms.GroupBox();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.cmbRequester = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.label52 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.label48 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.cmbDepart = new System.Windows.Forms.ComboBox();
            this.btnViewPDF = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dgvVR = new System.Windows.Forms.DataGridView();
            this.flowLayoutPanel1.SuspendLayout();
            this.gbExternal.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVR)).BeginInit();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.flowLayoutPanel1.Controls.Add(this.btnBack);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1436, 46);
            this.flowLayoutPanel1.TabIndex = 126;
            // 
            // btnBack
            // 
            this.btnBack.BackColor = System.Drawing.Color.White;
            this.btnBack.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.Image = ((System.Drawing.Image)(resources.GetObject("btnBack.Image")));
            this.btnBack.Location = new System.Drawing.Point(3, 2);
            this.btnBack.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(48, 37);
            this.btnBack.TabIndex = 103;
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // gbExternal
            // 
            this.gbExternal.BackColor = System.Drawing.Color.White;
            this.gbExternal.Controls.Add(this.dtpStart);
            this.gbExternal.Controls.Add(this.label5);
            this.gbExternal.Controls.Add(this.label4);
            this.gbExternal.Controls.Add(this.dtpEnd);
            this.gbExternal.Controls.Add(this.cmbRequester);
            this.gbExternal.Controls.Add(this.label3);
            this.gbExternal.Controls.Add(this.label2);
            this.gbExternal.Controls.Add(this.label1);
            this.gbExternal.Controls.Add(this.cmbType);
            this.gbExternal.Controls.Add(this.label52);
            this.gbExternal.Controls.Add(this.label50);
            this.gbExternal.Controls.Add(this.label48);
            this.gbExternal.Controls.Add(this.label47);
            this.gbExternal.Controls.Add(this.label45);
            this.gbExternal.Controls.Add(this.cmbDepart);
            this.gbExternal.Controls.Add(this.btnViewPDF);
            this.gbExternal.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbExternal.Font = new System.Drawing.Font("Calibri", 14F);
            this.gbExternal.Location = new System.Drawing.Point(0, 46);
            this.gbExternal.Name = "gbExternal";
            this.gbExternal.Size = new System.Drawing.Size(1436, 340);
            this.gbExternal.TabIndex = 127;
            this.gbExternal.TabStop = false;
            this.gbExternal.Text = "View Report";
            // 
            // dtpStart
            // 
            this.dtpStart.Font = new System.Drawing.Font("Calibri", 12F);
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStart.Location = new System.Drawing.Point(287, 51);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(250, 32);
            this.dtpStart.TabIndex = 146;
            this.dtpStart.ValueChanged += new System.EventHandler(this.dtpStart_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Calibri", 13F);
            this.label5.Location = new System.Drawing.Point(263, 106);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(18, 27);
            this.label5.TabIndex = 145;
            this.label5.Text = ":";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Calibri", 13F);
            this.label4.Location = new System.Drawing.Point(62, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 27);
            this.label4.TabIndex = 144;
            this.label4.Text = "End date";
            // 
            // dtpEnd
            // 
            this.dtpEnd.Font = new System.Drawing.Font("Calibri", 12F);
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEnd.Location = new System.Drawing.Point(287, 102);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(250, 32);
            this.dtpEnd.TabIndex = 143;
            this.dtpEnd.ValueChanged += new System.EventHandler(this.dtpEnd_ValueChanged);
            // 
            // cmbRequester
            // 
            this.cmbRequester.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRequester.DropDownWidth = 200;
            this.cmbRequester.Font = new System.Drawing.Font("Calibri", 12F);
            this.cmbRequester.FormattingEnabled = true;
            this.cmbRequester.IntegralHeight = false;
            this.cmbRequester.Location = new System.Drawing.Point(287, 258);
            this.cmbRequester.Name = "cmbRequester";
            this.cmbRequester.Size = new System.Drawing.Size(250, 32);
            this.cmbRequester.TabIndex = 141;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 13F);
            this.label3.Location = new System.Drawing.Point(62, 259);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 27);
            this.label3.TabIndex = 139;
            this.label3.Text = "Requester";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 13F);
            this.label2.Location = new System.Drawing.Point(263, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(18, 27);
            this.label2.TabIndex = 138;
            this.label2.Text = ":";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 13F);
            this.label1.Location = new System.Drawing.Point(62, 157);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(195, 27);
            this.label1.TabIndex = 137;
            this.label1.Text = "Expenses claim type";
            // 
            // cmbType
            // 
            this.cmbType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbType.Font = new System.Drawing.Font("Calibri", 12F);
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            "Work",
            "Benefit"});
            this.cmbType.Location = new System.Drawing.Point(287, 156);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(250, 32);
            this.cmbType.TabIndex = 136;
            this.cmbType.SelectedIndexChanged += new System.EventHandler(this.cmbType_SelectedIndexChanged);
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Font = new System.Drawing.Font("Calibri", 13F);
            this.label52.Location = new System.Drawing.Point(263, 208);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(18, 27);
            this.label52.TabIndex = 135;
            this.label52.Text = ":";
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Font = new System.Drawing.Font("Calibri", 13F);
            this.label50.Location = new System.Drawing.Point(263, 259);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(18, 27);
            this.label50.TabIndex = 133;
            this.label50.Text = ":";
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Font = new System.Drawing.Font("Calibri", 13F);
            this.label48.Location = new System.Drawing.Point(263, 157);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(18, 27);
            this.label48.TabIndex = 132;
            this.label48.Text = ":";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Font = new System.Drawing.Font("Calibri", 13F);
            this.label47.Location = new System.Drawing.Point(62, 208);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(123, 27);
            this.label47.TabIndex = 130;
            this.label47.Text = "Department";
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Font = new System.Drawing.Font("Calibri", 13F);
            this.label45.Location = new System.Drawing.Point(62, 55);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(101, 27);
            this.label45.TabIndex = 129;
            this.label45.Text = "Start date";
            // 
            // cmbDepart
            // 
            this.cmbDepart.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDepart.DropDownWidth = 200;
            this.cmbDepart.Font = new System.Drawing.Font("Calibri", 12F);
            this.cmbDepart.FormattingEnabled = true;
            this.cmbDepart.IntegralHeight = false;
            this.cmbDepart.Location = new System.Drawing.Point(287, 207);
            this.cmbDepart.Name = "cmbDepart";
            this.cmbDepart.Size = new System.Drawing.Size(250, 32);
            this.cmbDepart.TabIndex = 24;
            // 
            // btnViewPDF
            // 
            this.btnViewPDF.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnViewPDF.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnViewPDF.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnViewPDF.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnViewPDF.Location = new System.Drawing.Point(590, 256);
            this.btnViewPDF.Name = "btnViewPDF";
            this.btnViewPDF.Size = new System.Drawing.Size(124, 34);
            this.btnViewPDF.TabIndex = 125;
            this.btnViewPDF.Text = "View";
            this.btnViewPDF.UseVisualStyleBackColor = true;
            this.btnViewPDF.Click += new System.EventHandler(this.btnViewPDF_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dgvVR);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 386);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1436, 409);
            this.panel1.TabIndex = 128;
            // 
            // dgvVR
            // 
            this.dgvVR.BackgroundColor = System.Drawing.Color.White;
            this.dgvVR.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.OrangeRed;
            dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvVR.DefaultCellStyle = dataGridViewCellStyle8;
            this.dgvVR.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvVR.Location = new System.Drawing.Point(0, 0);
            this.dgvVR.Name = "dgvVR";
            this.dgvVR.RowHeadersVisible = false;
            this.dgvVR.RowHeadersWidth = 51;
            this.dgvVR.RowTemplate.Height = 24;
            this.dgvVR.Size = new System.Drawing.Size(1436, 409);
            this.dgvVR.TabIndex = 0;
            // 
            // UC_M_Report
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.gbExternal);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "UC_M_Report";
            this.Size = new System.Drawing.Size(1436, 795);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.gbExternal.ResumeLayout(false);
            this.gbExternal.PerformLayout();
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvVR)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.GroupBox gbExternal;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Label label52;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.ComboBox cmbDepart;
        private System.Windows.Forms.Button btnViewPDF;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dgvVR;
        private System.Windows.Forms.ComboBox cmbRequester;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dtpStart;
    }
}
