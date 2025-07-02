namespace HRAdmin.UserControl
{
    partial class UC_M_Approval
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_M_Approval));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnBack = new System.Windows.Forms.Button();
            this.gbExternal = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.dgvA = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.btnView = new System.Windows.Forms.Button();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbDepartment = new System.Windows.Forms.ComboBox();
            this.cmbRequester = new System.Windows.Forms.ComboBox();
            this.btnReject = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnApprove = new System.Windows.Forms.Button();
            this.flowLayoutPanel1.SuspendLayout();
            this.gbExternal.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvA)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnBack);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1857, 46);
            this.flowLayoutPanel1.TabIndex = 136;
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
            this.gbExternal.Controls.Add(this.panel2);
            this.gbExternal.Controls.Add(this.panel1);
            this.gbExternal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbExternal.Font = new System.Drawing.Font("Calibri", 14F);
            this.gbExternal.Location = new System.Drawing.Point(0, 46);
            this.gbExternal.Name = "gbExternal";
            this.gbExternal.Size = new System.Drawing.Size(1857, 1232);
            this.gbExternal.TabIndex = 137;
            this.gbExternal.TabStop = false;
            this.gbExternal.Text = "Miscellaneous Claim Form Approval";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dgvA);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 312);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1851, 917);
            this.panel2.TabIndex = 1;
            // 
            // dgvA
            // 
            this.dgvA.BackgroundColor = System.Drawing.Color.White;
            this.dgvA.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 14F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.OrangeRed;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvA.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvA.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvA.Location = new System.Drawing.Point(0, 0);
            this.dgvA.Name = "dgvA";
            this.dgvA.RowHeadersVisible = false;
            this.dgvA.RowHeadersWidth = 51;
            this.dgvA.RowTemplate.Height = 24;
            this.dgvA.Size = new System.Drawing.Size(1851, 917);
            this.dgvA.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label11);
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.btnView);
            this.panel1.Controls.Add(this.dtpEnd);
            this.panel1.Controls.Add(this.dtpStart);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.cmbDepartment);
            this.panel1.Controls.Add(this.cmbRequester);
            this.panel1.Controls.Add(this.btnReject);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnApprove);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 32);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1851, 280);
            this.panel1.TabIndex = 0;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Calibri", 13F);
            this.label11.Location = new System.Drawing.Point(713, 149);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(18, 27);
            this.label11.TabIndex = 182;
            this.label11.Text = ":";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Calibri", 13F);
            this.label12.Location = new System.Drawing.Point(582, 149);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(125, 27);
            this.label12.TabIndex = 181;
            this.label12.Text = "View invoice";
            // 
            // btnView
            // 
            this.btnView.BackColor = System.Drawing.Color.White;
            this.btnView.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnView.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnView.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnView.Location = new System.Drawing.Point(737, 146);
            this.btnView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(124, 34);
            this.btnView.TabIndex = 180;
            this.btnView.Text = "View";
            this.btnView.UseVisualStyleBackColor = false;
            // 
            // dtpEnd
            // 
            this.dtpEnd.CalendarFont = new System.Drawing.Font("Calibri", 13F);
            this.dtpEnd.Location = new System.Drawing.Point(220, 94);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(250, 36);
            this.dtpEnd.TabIndex = 179;
            this.dtpEnd.ValueChanged += new System.EventHandler(this.dtpEnd_ValueChanged);
            // 
            // dtpStart
            // 
            this.dtpStart.CalendarFont = new System.Drawing.Font("Calibri", 13F);
            this.dtpStart.Location = new System.Drawing.Point(220, 44);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(250, 36);
            this.dtpStart.TabIndex = 177;
            this.dtpStart.ValueChanged += new System.EventHandler(this.dtpStart_ValueChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Calibri", 13F);
            this.label9.Location = new System.Drawing.Point(196, 200);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(18, 27);
            this.label9.TabIndex = 176;
            this.label9.Text = ":";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Calibri", 13F);
            this.label10.Location = new System.Drawing.Point(67, 98);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(93, 27);
            this.label10.TabIndex = 175;
            this.label10.Text = "End date";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Calibri", 13F);
            this.label7.Location = new System.Drawing.Point(196, 149);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(18, 27);
            this.label7.TabIndex = 174;
            this.label7.Text = ":";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Calibri", 13F);
            this.label8.Location = new System.Drawing.Point(67, 149);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(123, 27);
            this.label8.TabIndex = 173;
            this.label8.Text = "Department";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Calibri", 13F);
            this.label4.Location = new System.Drawing.Point(196, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(18, 27);
            this.label4.TabIndex = 172;
            this.label4.Text = ":";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Calibri", 13F);
            this.label5.Location = new System.Drawing.Point(67, 200);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 27);
            this.label5.TabIndex = 171;
            this.label5.Text = "Requester";
            // 
            // cmbDepartment
            // 
            this.cmbDepartment.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDepartment.Font = new System.Drawing.Font("Calibri", 13F);
            this.cmbDepartment.FormattingEnabled = true;
            this.cmbDepartment.Location = new System.Drawing.Point(220, 146);
            this.cmbDepartment.Name = "cmbDepartment";
            this.cmbDepartment.Size = new System.Drawing.Size(250, 35);
            this.cmbDepartment.TabIndex = 170;
            this.cmbDepartment.SelectedIndexChanged += new System.EventHandler(this.cmbDepartment_SelectedIndexChanged);
            // 
            // cmbRequester
            // 
            this.cmbRequester.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRequester.Font = new System.Drawing.Font("Calibri", 13F);
            this.cmbRequester.FormattingEnabled = true;
            this.cmbRequester.Location = new System.Drawing.Point(220, 197);
            this.cmbRequester.Name = "cmbRequester";
            this.cmbRequester.Size = new System.Drawing.Size(250, 35);
            this.cmbRequester.TabIndex = 169;
            this.cmbRequester.SelectedIndexChanged += new System.EventHandler(this.cmbRequester_SelectedIndexChanged);
            // 
            // btnReject
            // 
            this.btnReject.BackColor = System.Drawing.Color.White;
            this.btnReject.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnReject.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnReject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReject.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnReject.Location = new System.Drawing.Point(867, 197);
            this.btnReject.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnReject.Name = "btnReject";
            this.btnReject.Size = new System.Drawing.Size(124, 34);
            this.btnReject.TabIndex = 168;
            this.btnReject.Text = "Reject";
            this.btnReject.UseVisualStyleBackColor = false;
            this.btnReject.Click += new System.EventHandler(this.btnReject_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 13F);
            this.label1.Location = new System.Drawing.Point(713, 200);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(18, 27);
            this.label1.TabIndex = 167;
            this.label1.Text = ":";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 13F);
            this.label2.Location = new System.Drawing.Point(582, 200);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 27);
            this.label2.TabIndex = 166;
            this.label2.Text = "Verification";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Calibri", 13F);
            this.label6.Location = new System.Drawing.Point(196, 47);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(18, 27);
            this.label6.TabIndex = 165;
            this.label6.Text = ":";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 13F);
            this.label3.Location = new System.Drawing.Point(67, 47);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(101, 27);
            this.label3.TabIndex = 164;
            this.label3.Text = "Start date";
            // 
            // btnApprove
            // 
            this.btnApprove.BackColor = System.Drawing.Color.White;
            this.btnApprove.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnApprove.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnApprove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApprove.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnApprove.Location = new System.Drawing.Point(737, 197);
            this.btnApprove.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnApprove.Name = "btnApprove";
            this.btnApprove.Size = new System.Drawing.Size(124, 34);
            this.btnApprove.TabIndex = 162;
            this.btnApprove.Text = "Approve";
            this.btnApprove.UseVisualStyleBackColor = false;
            this.btnApprove.Click += new System.EventHandler(this.btnApprove_Click);
            // 
            // UC_M_Approval
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.gbExternal);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "UC_M_Approval";
            this.Size = new System.Drawing.Size(1857, 1278);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.gbExternal.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvA)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.GroupBox gbExternal;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dgvA;
        private System.Windows.Forms.Button btnApprove;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnReject;
        private System.Windows.Forms.ComboBox cmbRequester;
        private System.Windows.Forms.ComboBox cmbDepartment;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Button btnView;
    }
}
