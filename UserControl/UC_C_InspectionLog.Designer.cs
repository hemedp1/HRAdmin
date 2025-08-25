namespace HRAdmin.UserControl
{
    partial class UC_C_InspectionLog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_C_InspectionLog));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel6 = new System.Windows.Forms.Panel();
            this.btnBack = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.EndDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.cmb_Driver = new System.Windows.Forms.ComboBox();
            this.label27 = new System.Windows.Forms.Label();
            this.StartDate = new System.Windows.Forms.DateTimePicker();
            this.label10 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cmbCar = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panel6.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.White;
            this.panel6.Controls.Add(this.btnBack);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(1602, 57);
            this.panel6.TabIndex = 49;
            // 
            // btnBack
            // 
            this.btnBack.BackColor = System.Drawing.Color.White;
            this.btnBack.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.ForeColor = System.Drawing.Color.White;
            this.btnBack.Image = ((System.Drawing.Image)(resources.GetObject("btnBack.Image")));
            this.btnBack.Location = new System.Drawing.Point(6, 3);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(54, 46);
            this.btnBack.TabIndex = 21;
            this.btnBack.UseVisualStyleBackColor = false;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.EndDate);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label26);
            this.groupBox1.Controls.Add(this.cmb_Driver);
            this.groupBox1.Controls.Add(this.label27);
            this.groupBox1.Controls.Add(this.StartDate);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.cmbCar);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Font = new System.Drawing.Font("Calibri", 14F);
            this.groupBox1.Location = new System.Drawing.Point(0, 57);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1602, 206);
            this.groupBox1.TabIndex = 50;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Inspection log";
            // 
            // EndDate
            // 
            this.EndDate.CalendarFont = new System.Drawing.Font("Calibri", 12F);
            this.EndDate.Font = new System.Drawing.Font("Calibri", 12F);
            this.EndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.EndDate.Location = new System.Drawing.Point(801, 68);
            this.EndDate.Name = "EndDate";
            this.EndDate.Size = new System.Drawing.Size(260, 37);
            this.EndDate.TabIndex = 53;
            this.EndDate.ValueChanged += new System.EventHandler(this.EndDate_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 13F);
            this.label1.Location = new System.Drawing.Point(570, 68);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 32);
            this.label1.TabIndex = 52;
            this.label1.Text = "End Date";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 13F);
            this.label2.Location = new System.Drawing.Point(774, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(21, 32);
            this.label2.TabIndex = 51;
            this.label2.Text = ":";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Font = new System.Drawing.Font("Calibri", 13F);
            this.label26.Location = new System.Drawing.Point(36, 124);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(162, 48);
            this.label26.TabIndex = 49;
            this.label26.Text = "Driver ID";
            // 
            // cmb_Driver
            // 
            this.cmb_Driver.DropDownHeight = 158;
            this.cmb_Driver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Driver.Font = new System.Drawing.Font("Calibri", 12F);
            this.cmb_Driver.FormattingEnabled = true;
            this.cmb_Driver.IntegralHeight = false;
            this.cmb_Driver.ItemHeight = 29;
            this.cmb_Driver.Location = new System.Drawing.Point(266, 125);
            this.cmb_Driver.MaxLength = 8;
            this.cmb_Driver.Name = "cmb_Driver";
            this.cmb_Driver.Size = new System.Drawing.Size(260, 37);
            this.cmb_Driver.TabIndex = 50;
            this.cmb_Driver.SelectedIndexChanged += new System.EventHandler(this.cmb_Driver_SelectedIndexChanged);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Font = new System.Drawing.Font("Calibri", 13F);
            this.label27.Location = new System.Drawing.Point(239, 123);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(21, 32);
            this.label27.TabIndex = 48;
            this.label27.Text = ":";
            // 
            // StartDate
            // 
            this.StartDate.CalendarFont = new System.Drawing.Font("Calibri", 12F);
            this.StartDate.Font = new System.Drawing.Font("Calibri", 12F);
            this.StartDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.StartDate.Location = new System.Drawing.Point(266, 68);
            this.StartDate.Name = "StartDate";
            this.StartDate.Size = new System.Drawing.Size(260, 37);
            this.StartDate.TabIndex = 22;
            this.StartDate.ValueChanged += new System.EventHandler(this.StartDate_ValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Calibri", 13F);
            this.label10.Location = new System.Drawing.Point(35, 68);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(121, 32);
            this.label10.TabIndex = 16;
            this.label10.Text = "Start Date";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Calibri", 13F);
            this.label7.Location = new System.Drawing.Point(571, 117);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(49, 32);
            this.label7.TabIndex = 12;
            this.label7.Text = "Car";
            // 
            // cmbCar
            // 
            this.cmbCar.DropDownHeight = 158;
            this.cmbCar.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCar.Font = new System.Drawing.Font("Calibri", 12F);
            this.cmbCar.FormattingEnabled = true;
            this.cmbCar.IntegralHeight = false;
            this.cmbCar.ItemHeight = 29;
            this.cmbCar.Location = new System.Drawing.Point(801, 118);
            this.cmbCar.MaxLength = 8;
            this.cmbCar.Name = "cmbCar";
            this.cmbCar.Size = new System.Drawing.Size(260, 37);
            this.cmbCar.TabIndex = 20;
            this.cmbCar.SelectedIndexChanged += new System.EventHandler(this.cmbCar_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Calibri", 13F);
            this.label9.Location = new System.Drawing.Point(239, 68);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(21, 32);
            this.label9.TabIndex = 15;
            this.label9.Text = ":";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Calibri", 13F);
            this.label4.Location = new System.Drawing.Point(774, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 32);
            this.label4.TabIndex = 10;
            this.label4.Text = ":";
            // 
            // dataGridView1
            // 
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.OrangeRed;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle5.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle5.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.OrangeRed;
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle5.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle5;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.GridColor = System.Drawing.Color.White;
            this.dataGridView1.Location = new System.Drawing.Point(0, 263);
            this.dataGridView1.Name = "dataGridView1";
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.Color.OrangeRed;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.RowTemplate.Height = 28;
            this.dataGridView1.Size = new System.Drawing.Size(1602, 574);
            this.dataGridView1.TabIndex = 51;
            // 
            // UC_C_InspectionLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel6);
            this.Name = "UC_C_InspectionLog";
            this.Size = new System.Drawing.Size(1602, 837);
            this.panel6.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.ComboBox cmb_Driver;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.DateTimePicker StartDate;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbCar;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DateTimePicker EndDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}
