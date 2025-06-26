namespace HRAdmin.UserControl
{
    partial class UC_M_Work
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_M_Work));
            this.gbExternal = new System.Windows.Forms.GroupBox();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dgvW = new System.Windows.Forms.DataGridView();
            this.btnBack = new System.Windows.Forms.Button();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.EventOccasion = new System.Windows.Forms.Label();
            this.gbExternal.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvW)).BeginInit();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbExternal
            // 
            this.gbExternal.BackColor = System.Drawing.Color.White;
            this.gbExternal.Controls.Add(this.btnSubmit);
            this.gbExternal.Controls.Add(this.panel1);
            this.gbExternal.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbExternal.Font = new System.Drawing.Font("Calibri", 14F);
            this.gbExternal.Location = new System.Drawing.Point(0, 46);
            this.gbExternal.Name = "gbExternal";
            this.gbExternal.Size = new System.Drawing.Size(1479, 550);
            this.gbExternal.TabIndex = 140;
            this.gbExternal.TabStop = false;
            this.gbExternal.Text = "Miscellaneous Claim Form for Work or Benefit";
            // 
            // btnSubmit
            // 
            this.btnSubmit.BackColor = System.Drawing.Color.White;
            this.btnSubmit.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnSubmit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSubmit.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnSubmit.Location = new System.Drawing.Point(71, 468);
            this.btnSubmit.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(124, 34);
            this.btnSubmit.TabIndex = 102;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.UseVisualStyleBackColor = false;
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dgvW);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 32);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1473, 400);
            this.panel1.TabIndex = 0;
            // 
            // dgvW
            // 
            this.dgvW.BackgroundColor = System.Drawing.Color.White;
            this.dgvW.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvW.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvW.Location = new System.Drawing.Point(0, 0);
            this.dgvW.Name = "dgvW";
            this.dgvW.RowHeadersVisible = false;
            this.dgvW.RowHeadersWidth = 51;
            this.dgvW.RowTemplate.Height = 24;
            this.dgvW.Size = new System.Drawing.Size(1473, 400);
            this.dgvW.TabIndex = 0;
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
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnBack);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1479, 46);
            this.flowLayoutPanel1.TabIndex = 139;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.EventOccasion);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Font = new System.Drawing.Font("Calibri", 14F);
            this.groupBox1.Location = new System.Drawing.Point(0, 596);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1479, 458);
            this.groupBox1.TabIndex = 141;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Note";
            // 
            // EventOccasion
            // 
            this.EventOccasion.AutoSize = true;
            this.EventOccasion.Font = new System.Drawing.Font("Calibri", 13F);
            this.EventOccasion.Location = new System.Drawing.Point(52, 54);
            this.EventOccasion.Name = "EventOccasion";
            this.EventOccasion.Size = new System.Drawing.Size(1144, 324);
            this.EventOccasion.TabIndex = 23;
            this.EventOccasion.Text = resources.GetString("EventOccasion.Text");
            // 
            // UC_M_Work
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.gbExternal);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "UC_M_Work";
            this.Size = new System.Drawing.Size(1479, 1209);
            this.gbExternal.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvW)).EndInit();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbExternal;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dgvW;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label EventOccasion;
    }
}
