namespace HRAdmin.UserControl
{
    partial class UC_W_UpdateCompany
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_W_UpdateCompany));
            this.gbExternal = new System.Windows.Forms.GroupBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.cmb_Company = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.button2 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dgv_UC = new System.Windows.Forms.DataGridView();
            this.gbExternal.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_UC)).BeginInit();
            this.SuspendLayout();
            // 
            // gbExternal
            // 
            this.gbExternal.BackColor = System.Drawing.Color.White;
            this.gbExternal.Controls.Add(this.btnDelete);
            this.gbExternal.Controls.Add(this.btnSave);
            this.gbExternal.Controls.Add(this.cmb_Company);
            this.gbExternal.Controls.Add(this.label4);
            this.gbExternal.Controls.Add(this.label3);
            this.gbExternal.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbExternal.Font = new System.Drawing.Font("Calibri", 14F);
            this.gbExternal.Location = new System.Drawing.Point(0, 58);
            this.gbExternal.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbExternal.Name = "gbExternal";
            this.gbExternal.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbExternal.Size = new System.Drawing.Size(1771, 156);
            this.gbExternal.TabIndex = 136;
            this.gbExternal.TabStop = false;
            this.gbExternal.Text = "Update company";
            // 
            // btnDelete
            // 
            this.btnDelete.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnDelete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnDelete.Location = new System.Drawing.Point(699, 64);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(140, 42);
            this.btnDelete.TabIndex = 145;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnSave
            // 
            this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.DarkGray;
            this.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Calibri", 12F);
            this.btnSave.Location = new System.Drawing.Point(535, 64);
            this.btnSave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(140, 42);
            this.btnSave.TabIndex = 144;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // cmb_Company
            // 
            this.cmb_Company.Font = new System.Drawing.Font("Calibri", 12F);
            this.cmb_Company.FormattingEnabled = true;
            this.cmb_Company.Location = new System.Drawing.Point(215, 64);
            this.cmb_Company.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cmb_Company.Name = "cmb_Company";
            this.cmb_Company.Size = new System.Drawing.Size(281, 37);
            this.cmb_Company.TabIndex = 143;
            this.cmb_Company.SelectedIndexChanged += new System.EventHandler(this.cmb_Company_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Calibri", 13F);
            this.label4.Location = new System.Drawing.Point(188, 68);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(21, 32);
            this.label4.TabIndex = 140;
            this.label4.Text = ":";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 13F);
            this.label3.Location = new System.Drawing.Point(70, 68);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 32);
            this.label3.TabIndex = 139;
            this.label3.Text = "Company";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.flowLayoutPanel1.Controls.Add(this.button2);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1771, 58);
            this.flowLayoutPanel1.TabIndex = 135;
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
            // panel1
            // 
            this.panel1.Controls.Add(this.dgv_UC);
            this.panel1.Location = new System.Drawing.Point(76, 270);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(675, 500);
            this.panel1.TabIndex = 137;
            // 
            // dgv_UC
            // 
            this.dgv_UC.BackgroundColor = System.Drawing.Color.White;
            this.dgv_UC.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_UC.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_UC.Location = new System.Drawing.Point(0, 0);
            this.dgv_UC.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgv_UC.Name = "dgv_UC";
            this.dgv_UC.RowHeadersVisible = false;
            this.dgv_UC.RowHeadersWidth = 51;
            this.dgv_UC.RowTemplate.Height = 24;
            this.dgv_UC.Size = new System.Drawing.Size(675, 500);
            this.dgv_UC.TabIndex = 0;
            this.dgv_UC.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_UC_CellContentClick);
            // 
            // UC_W_UpdateCompany
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.gbExternal);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.Name = "UC_W_UpdateCompany";
            this.Size = new System.Drawing.Size(1771, 855);
            this.gbExternal.ResumeLayout(false);
            this.gbExternal.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_UC)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbExternal;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ComboBox cmb_Company;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.DataGridView dgv_UC;
    }
}
