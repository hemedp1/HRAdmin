namespace HRAdmin.UserControl
{
    partial class UC_Meal_Food
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_Meal_Food));
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dgv_OS = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.GB_Authorization = new System.Windows.Forms.GroupBox();
            this.btnReject = new System.Windows.Forms.Button();
            this.btnCheck = new System.Windows.Forms.Button();
            this.btnApprove = new System.Windows.Forms.Button();
            this.panel5 = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnView = new System.Windows.Forms.Button();
            this.btnWithdraw = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnSearch = new System.Windows.Forms.Button();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.txtSearchSn = new System.Windows.Forms.TextBox();
            this.cmbOS_Occasion = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.dtpEnd = new System.Windows.Forms.DateTimePicker();
            this.dtpStart = new System.Windows.Forms.DateTimePicker();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbDepart = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbRequester = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gbFoodrequest = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label18 = new System.Windows.Forms.Label();
            this.label59 = new System.Windows.Forms.Label();
            this.dtDelivery = new System.Windows.Forms.DateTimePicker();
            this.dtRequest = new System.Windows.Forms.Label();
            this.btNext = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.EventOccasion = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbOccasion = new System.Windows.Forms.ComboBox();
            this.txtEvent = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_OS)).BeginInit();
            this.panel1.SuspendLayout();
            this.GB_Authorization.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.gbFoodrequest.SuspendLayout();
            this.panel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.White;
            this.groupBox2.Controls.Add(this.panel3);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("Calibri", 14F);
            this.groupBox2.Location = new System.Drawing.Point(0, 316);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox2.Size = new System.Drawing.Size(1920, 573);
            this.groupBox2.TabIndex = 128;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Order status";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.dgv_OS);
            this.panel3.Controls.Add(this.panel1);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 37);
            this.panel3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1914, 534);
            this.panel3.TabIndex = 18;
            // 
            // dgv_OS
            // 
            this.dgv_OS.BackgroundColor = System.Drawing.Color.White;
            this.dgv_OS.ColumnHeadersHeight = 29;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Calibri", 14F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.OrangeRed;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgv_OS.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgv_OS.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_OS.Location = new System.Drawing.Point(0, 300);
            this.dgv_OS.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv_OS.Name = "dgv_OS";
            this.dgv_OS.ReadOnly = true;
            this.dgv_OS.RowHeadersVisible = false;
            this.dgv_OS.RowHeadersWidth = 51;
            this.dgv_OS.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgv_OS.Size = new System.Drawing.Size(1914, 234);
            this.dgv_OS.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.GB_Authorization);
            this.panel1.Controls.Add(this.panel5);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 175);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1914, 125);
            this.panel1.TabIndex = 1;
            // 
            // GB_Authorization
            // 
            this.GB_Authorization.Controls.Add(this.btnReject);
            this.GB_Authorization.Controls.Add(this.btnCheck);
            this.GB_Authorization.Controls.Add(this.btnApprove);
            this.GB_Authorization.Dock = System.Windows.Forms.DockStyle.Left;
            this.GB_Authorization.Location = new System.Drawing.Point(517, 0);
            this.GB_Authorization.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.GB_Authorization.Name = "GB_Authorization";
            this.GB_Authorization.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.GB_Authorization.Size = new System.Drawing.Size(709, 125);
            this.GB_Authorization.TabIndex = 0;
            this.GB_Authorization.TabStop = false;
            this.GB_Authorization.Text = "Authorization";
            // 
            // btnReject
            // 
            this.btnReject.BackColor = System.Drawing.Color.Crimson;
            this.btnReject.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnReject.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnReject.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReject.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnReject.Location = new System.Drawing.Point(495, 51);
            this.btnReject.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnReject.Name = "btnReject";
            this.btnReject.Size = new System.Drawing.Size(140, 42);
            this.btnReject.TabIndex = 102;
            this.btnReject.Text = "Reject";
            this.btnReject.UseVisualStyleBackColor = false;
            this.btnReject.Click += new System.EventHandler(this.btnReject_Click);
            // 
            // btnCheck
            // 
            this.btnCheck.BackColor = System.Drawing.Color.PeachPuff;
            this.btnCheck.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnCheck.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnCheck.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCheck.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnCheck.Location = new System.Drawing.Point(72, 51);
            this.btnCheck.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnCheck.Name = "btnCheck";
            this.btnCheck.Size = new System.Drawing.Size(140, 42);
            this.btnCheck.TabIndex = 101;
            this.btnCheck.Text = "Check";
            this.btnCheck.UseVisualStyleBackColor = false;
            this.btnCheck.Click += new System.EventHandler(this.btnCheck_Click);
            // 
            // btnApprove
            // 
            this.btnApprove.BackColor = System.Drawing.Color.PowderBlue;
            this.btnApprove.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnApprove.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnApprove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApprove.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnApprove.Location = new System.Drawing.Point(284, 51);
            this.btnApprove.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnApprove.Name = "btnApprove";
            this.btnApprove.Size = new System.Drawing.Size(140, 42);
            this.btnApprove.TabIndex = 100;
            this.btnApprove.Text = "Approve";
            this.btnApprove.UseVisualStyleBackColor = false;
            this.btnApprove.Click += new System.EventHandler(this.btnApprove_Click);
            // 
            // panel5
            // 
            this.panel5.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel5.Location = new System.Drawing.Point(506, 0);
            this.panel5.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(11, 125);
            this.panel5.TabIndex = 2;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnView);
            this.groupBox3.Controls.Add(this.btnWithdraw);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox3.Location = new System.Drawing.Point(0, 0);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox3.Size = new System.Drawing.Size(506, 125);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Order";
            // 
            // btnView
            // 
            this.btnView.BackColor = System.Drawing.Color.White;
            this.btnView.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnView.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnView.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnView.Location = new System.Drawing.Point(72, 51);
            this.btnView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(140, 42);
            this.btnView.TabIndex = 109;
            this.btnView.Text = "View";
            this.btnView.UseVisualStyleBackColor = false;
            this.btnView.Click += new System.EventHandler(this.btnView_Click);
            // 
            // btnWithdraw
            // 
            this.btnWithdraw.BackColor = System.Drawing.Color.White;
            this.btnWithdraw.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnWithdraw.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnWithdraw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWithdraw.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnWithdraw.Location = new System.Drawing.Point(284, 51);
            this.btnWithdraw.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnWithdraw.Name = "btnWithdraw";
            this.btnWithdraw.Size = new System.Drawing.Size(140, 42);
            this.btnWithdraw.TabIndex = 108;
            this.btnWithdraw.Text = "Withdraw";
            this.btnWithdraw.UseVisualStyleBackColor = false;
            this.btnWithdraw.Click += new System.EventHandler(this.btnWithdraw_Click);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnSearch);
            this.panel4.Controls.Add(this.label19);
            this.panel4.Controls.Add(this.label20);
            this.panel4.Controls.Add(this.txtSearchSn);
            this.panel4.Controls.Add(this.cmbOS_Occasion);
            this.panel4.Controls.Add(this.label17);
            this.panel4.Controls.Add(this.label16);
            this.panel4.Controls.Add(this.label15);
            this.panel4.Controls.Add(this.label14);
            this.panel4.Controls.Add(this.label13);
            this.panel4.Controls.Add(this.label9);
            this.panel4.Controls.Add(this.dtpEnd);
            this.panel4.Controls.Add(this.dtpStart);
            this.panel4.Controls.Add(this.label11);
            this.panel4.Controls.Add(this.label10);
            this.panel4.Controls.Add(this.cmbDepart);
            this.panel4.Controls.Add(this.label8);
            this.panel4.Controls.Add(this.cmbRequester);
            this.panel4.Controls.Add(this.label1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1914, 175);
            this.panel4.TabIndex = 0;
            // 
            // btnSearch
            // 
            this.btnSearch.BackColor = System.Drawing.Color.White;
            this.btnSearch.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnSearch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnSearch.Location = new System.Drawing.Point(1519, 91);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(140, 42);
            this.btnSearch.TabIndex = 141;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Calibri", 13F);
            this.label19.Location = new System.Drawing.Point(1194, 97);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(21, 32);
            this.label19.TabIndex = 140;
            this.label19.Text = ":";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Calibri", 13F);
            this.label20.Location = new System.Drawing.Point(1081, 97);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(105, 32);
            this.label20.TabIndex = 139;
            this.label20.Text = "Order ID";
            // 
            // txtSearchSn
            // 
            this.txtSearchSn.Font = new System.Drawing.Font("Calibri", 12F);
            this.txtSearchSn.Location = new System.Drawing.Point(1221, 92);
            this.txtSearchSn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtSearchSn.Name = "txtSearchSn";
            this.txtSearchSn.Size = new System.Drawing.Size(281, 37);
            this.txtSearchSn.TabIndex = 138;
            // 
            // cmbOS_Occasion
            // 
            this.cmbOS_Occasion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOS_Occasion.Font = new System.Drawing.Font("Calibri", 12F);
            this.cmbOS_Occasion.FormattingEnabled = true;
            this.cmbOS_Occasion.IntegralHeight = false;
            this.cmbOS_Occasion.ItemHeight = 29;
            this.cmbOS_Occasion.Items.AddRange(new object[] {
            "Internal",
            "External"});
            this.cmbOS_Occasion.Location = new System.Drawing.Point(1221, 28);
            this.cmbOS_Occasion.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbOS_Occasion.Name = "cmbOS_Occasion";
            this.cmbOS_Occasion.Size = new System.Drawing.Size(281, 37);
            this.cmbOS_Occasion.TabIndex = 137;
            this.cmbOS_Occasion.SelectedIndexChanged += new System.EventHandler(this.cmbOS_Occasion_SelectedIndexChanged);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Calibri", 13F);
            this.label17.Location = new System.Drawing.Point(1194, 29);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(21, 32);
            this.label17.TabIndex = 136;
            this.label17.Text = ":";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Calibri", 13F);
            this.label16.Location = new System.Drawing.Point(714, 30);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(21, 32);
            this.label16.TabIndex = 135;
            this.label16.Text = ":";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Calibri", 13F);
            this.label15.Location = new System.Drawing.Point(212, 29);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(21, 32);
            this.label15.TabIndex = 134;
            this.label15.Text = ":";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Calibri", 13F);
            this.label14.Location = new System.Drawing.Point(1081, 29);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(109, 32);
            this.label14.TabIndex = 133;
            this.label14.Text = "Occasion";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Calibri", 13F);
            this.label13.Location = new System.Drawing.Point(590, 29);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(109, 32);
            this.label13.TabIndex = 132;
            this.label13.Text = "End date";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Calibri", 13F);
            this.label9.Location = new System.Drawing.Point(66, 29);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(119, 32);
            this.label9.TabIndex = 131;
            this.label9.Text = "Start date";
            // 
            // dtpEnd
            // 
            this.dtpEnd.CalendarFont = new System.Drawing.Font("Calibri", 13F);
            this.dtpEnd.Font = new System.Drawing.Font("Calibri", 12F);
            this.dtpEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEnd.Location = new System.Drawing.Point(741, 24);
            this.dtpEnd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtpEnd.Name = "dtpEnd";
            this.dtpEnd.Size = new System.Drawing.Size(281, 37);
            this.dtpEnd.TabIndex = 130;
            this.dtpEnd.ValueChanged += new System.EventHandler(this.dtpEnd_ValueChanged);
            // 
            // dtpStart
            // 
            this.dtpStart.CalendarFont = new System.Drawing.Font("Calibri", 13F);
            this.dtpStart.Font = new System.Drawing.Font("Calibri", 12F);
            this.dtpStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStart.Location = new System.Drawing.Point(238, 24);
            this.dtpStart.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtpStart.Name = "dtpStart";
            this.dtpStart.Size = new System.Drawing.Size(281, 37);
            this.dtpStart.TabIndex = 129;
            this.dtpStart.ValueChanged += new System.EventHandler(this.dtpStart_ValueChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Calibri", 13F);
            this.label11.Location = new System.Drawing.Point(714, 92);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(21, 32);
            this.label11.TabIndex = 104;
            this.label11.Text = ":";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Calibri", 13F);
            this.label10.Location = new System.Drawing.Point(212, 92);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(21, 32);
            this.label10.TabIndex = 103;
            this.label10.Text = ":";
            // 
            // cmbDepart
            // 
            this.cmbDepart.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDepart.Font = new System.Drawing.Font("Calibri", 12F);
            this.cmbDepart.FormattingEnabled = true;
            this.cmbDepart.ItemHeight = 29;
            this.cmbDepart.Location = new System.Drawing.Point(238, 91);
            this.cmbDepart.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbDepart.Name = "cmbDepart";
            this.cmbDepart.Size = new System.Drawing.Size(281, 37);
            this.cmbDepart.TabIndex = 24;
            this.cmbDepart.SelectedIndexChanged += new System.EventHandler(this.cmbDepart_SelectedIndexChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Calibri", 13F);
            this.label8.Location = new System.Drawing.Point(66, 92);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(144, 32);
            this.label8.TabIndex = 22;
            this.label8.Text = "Department";
            // 
            // cmbRequester
            // 
            this.cmbRequester.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRequester.Font = new System.Drawing.Font("Calibri", 12F);
            this.cmbRequester.FormattingEnabled = true;
            this.cmbRequester.IntegralHeight = false;
            this.cmbRequester.ItemHeight = 29;
            this.cmbRequester.Location = new System.Drawing.Point(741, 91);
            this.cmbRequester.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbRequester.Name = "cmbRequester";
            this.cmbRequester.Size = new System.Drawing.Size(281, 37);
            this.cmbRequester.TabIndex = 24;
            this.cmbRequester.SelectedIndexChanged += new System.EventHandler(this.cmbRequester_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 13F);
            this.label1.Location = new System.Drawing.Point(590, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 32);
            this.label1.TabIndex = 21;
            this.label1.Text = "Requester";
            // 
            // gbFoodrequest
            // 
            this.gbFoodrequest.BackColor = System.Drawing.Color.White;
            this.gbFoodrequest.Controls.Add(this.panel2);
            this.gbFoodrequest.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbFoodrequest.Font = new System.Drawing.Font("Calibri", 14F);
            this.gbFoodrequest.Location = new System.Drawing.Point(0, 58);
            this.gbFoodrequest.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gbFoodrequest.Name = "gbFoodrequest";
            this.gbFoodrequest.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gbFoodrequest.Size = new System.Drawing.Size(1920, 258);
            this.gbFoodrequest.TabIndex = 126;
            this.gbFoodrequest.TabStop = false;
            this.gbFoodrequest.Text = "Order details";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label18);
            this.panel2.Controls.Add(this.label59);
            this.panel2.Controls.Add(this.dtDelivery);
            this.panel2.Controls.Add(this.dtRequest);
            this.panel2.Controls.Add(this.btNext);
            this.panel2.Controls.Add(this.label7);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.label12);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.EventOccasion);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.cmbOccasion);
            this.panel2.Controls.Add(this.txtEvent);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 37);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1914, 219);
            this.panel2.TabIndex = 18;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Calibri", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.ForeColor = System.Drawing.Color.Red;
            this.label18.Location = new System.Drawing.Point(234, 110);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(693, 19);
            this.label18.TabIndex = 130;
            this.label18.Text = "INTERNAL Order (Food): For events involving internal activities, such as training" +
    " sessions or staff meetings.";
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.Font = new System.Drawing.Font("Calibri", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label59.ForeColor = System.Drawing.Color.Red;
            this.label59.Location = new System.Drawing.Point(234, 78);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(674, 19);
            this.label59.TabIndex = 129;
            this.label59.Text = "EXTERNAL Order (Food): For events involving external parties, such as visitors, a" +
    "uditors, or other guests.";
            // 
            // dtDelivery
            // 
            this.dtDelivery.CalendarFont = new System.Drawing.Font("Calibri", 13F);
            this.dtDelivery.Font = new System.Drawing.Font("Calibri", 12F);
            this.dtDelivery.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtDelivery.Location = new System.Drawing.Point(772, 145);
            this.dtDelivery.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dtDelivery.Name = "dtDelivery";
            this.dtDelivery.Size = new System.Drawing.Size(183, 37);
            this.dtDelivery.TabIndex = 128;
            this.dtDelivery.ValueChanged += new System.EventHandler(this.dtDelivery_ValueChanged);
            // 
            // dtRequest
            // 
            this.dtRequest.AutoSize = true;
            this.dtRequest.Font = new System.Drawing.Font("Calibri", 13F);
            this.dtRequest.Location = new System.Drawing.Point(772, 29);
            this.dtRequest.Name = "dtRequest";
            this.dtRequest.Size = new System.Drawing.Size(147, 32);
            this.dtRequest.TabIndex = 37;
            this.dtRequest.Text = "current date";
            // 
            // btNext
            // 
            this.btNext.BackColor = System.Drawing.Color.White;
            this.btNext.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btNext.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btNext.Font = new System.Drawing.Font("Calibri", 12F);
            this.btNext.Location = new System.Drawing.Point(979, 145);
            this.btNext.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btNext.Name = "btNext";
            this.btNext.Size = new System.Drawing.Size(140, 42);
            this.btNext.TabIndex = 100;
            this.btNext.Text = "Next";
            this.btNext.UseVisualStyleBackColor = false;
            this.btNext.Click += new System.EventHandler(this.btNext_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Calibri", 13F);
            this.label7.Location = new System.Drawing.Point(66, 29);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(109, 32);
            this.label7.TabIndex = 20;
            this.label7.Text = "Occasion";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Calibri", 13F);
            this.label5.Location = new System.Drawing.Point(212, 150);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(21, 32);
            this.label5.TabIndex = 33;
            this.label5.Text = ":";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 13F);
            this.label2.Location = new System.Drawing.Point(590, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(155, 32);
            this.label2.TabIndex = 22;
            this.label2.Text = "Request date";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Calibri", 13F);
            this.label12.Location = new System.Drawing.Point(212, 29);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(21, 32);
            this.label12.TabIndex = 31;
            this.label12.Text = ":";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Calibri", 13F);
            this.label6.Location = new System.Drawing.Point(745, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(21, 32);
            this.label6.TabIndex = 34;
            this.label6.Text = ":";
            // 
            // EventOccasion
            // 
            this.EventOccasion.AutoSize = true;
            this.EventOccasion.Font = new System.Drawing.Font("Calibri", 13F);
            this.EventOccasion.Location = new System.Drawing.Point(66, 150);
            this.EventOccasion.Name = "EventOccasion";
            this.EventOccasion.Size = new System.Drawing.Size(74, 32);
            this.EventOccasion.TabIndex = 21;
            this.EventOccasion.Text = "Event";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Calibri", 13F);
            this.label4.Location = new System.Drawing.Point(745, 150);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 32);
            this.label4.TabIndex = 32;
            this.label4.Text = ":";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 13F);
            this.label3.Location = new System.Drawing.Point(590, 150);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(155, 32);
            this.label3.TabIndex = 20;
            this.label3.Text = "Delivery date";
            // 
            // cmbOccasion
            // 
            this.cmbOccasion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbOccasion.Font = new System.Drawing.Font("Calibri", 12F);
            this.cmbOccasion.FormattingEnabled = true;
            this.cmbOccasion.Items.AddRange(new object[] {
            "Internal",
            "External"});
            this.cmbOccasion.Location = new System.Drawing.Point(238, 24);
            this.cmbOccasion.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmbOccasion.Name = "cmbOccasion";
            this.cmbOccasion.Size = new System.Drawing.Size(322, 37);
            this.cmbOccasion.TabIndex = 0;
            this.cmbOccasion.SelectedIndexChanged += new System.EventHandler(this.cmbOccasion_SelectedIndexChanged);
            // 
            // txtEvent
            // 
            this.txtEvent.Font = new System.Drawing.Font("Calibri", 12F);
            this.txtEvent.Location = new System.Drawing.Point(238, 145);
            this.txtEvent.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtEvent.Name = "txtEvent";
            this.txtEvent.Size = new System.Drawing.Size(322, 37);
            this.txtEvent.TabIndex = 35;
            this.txtEvent.TextChanged += new System.EventHandler(this.txtEvent_TextChanged);
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
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.flowLayoutPanel1.Controls.Add(this.button2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1920, 58);
            this.flowLayoutPanel1.TabIndex = 127;
            // 
            // UC_Meal_Food
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.gbFoodrequest);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "UC_Meal_Food";
            this.Size = new System.Drawing.Size(1920, 889);
            this.groupBox2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_OS)).EndInit();
            this.panel1.ResumeLayout(false);
            this.GB_Authorization.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.gbFoodrequest.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.DataGridView dgv_OS;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnWithdraw;
        private System.Windows.Forms.Button btnApprove;
        private System.Windows.Forms.Button btnCheck;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cmbDepart;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbRequester;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox gbFoodrequest;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DateTimePicker dtDelivery;
        private System.Windows.Forms.Label dtRequest;
        private System.Windows.Forms.Button btNext;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label EventOccasion;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbOccasion;
        private System.Windows.Forms.TextBox txtEvent;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox GB_Authorization;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Button btnView;
        private System.Windows.Forms.DateTimePicker dtpStart;
        private System.Windows.Forms.DateTimePicker dtpEnd;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cmbOS_Occasion;
        private System.Windows.Forms.Button btnReject;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label59;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox txtSearchSn;
        private System.Windows.Forms.Button btnSearch;
    }
}
