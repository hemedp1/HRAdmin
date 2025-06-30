namespace HRAdmin.UserControl
{
    partial class UC_Acc_Account
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_Acc_Account));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnMClaim = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.btnMClaim);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Calibri", 14F);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.groupBox1.Size = new System.Drawing.Size(2019, 1176);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Account";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // btnMClaim
            // 
            this.btnMClaim.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnMClaim.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnMClaim.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMClaim.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnMClaim.ForeColor = System.Drawing.Color.Black;
            this.btnMClaim.Image = ((System.Drawing.Image)(resources.GetObject("btnMClaim.Image")));
            this.btnMClaim.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMClaim.Location = new System.Drawing.Point(6, 41);
            this.btnMClaim.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnMClaim.Name = "btnMClaim";
            this.btnMClaim.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnMClaim.Size = new System.Drawing.Size(259, 72);
            this.btnMClaim.TabIndex = 10;
            this.btnMClaim.Tag = "Miscellaneous Claim";
            this.btnMClaim.Text = "            Miscellaneous Claim";
            this.btnMClaim.UseVisualStyleBackColor = true;
            this.btnMClaim.Click += new System.EventHandler(this.btnMClaim_Click);
            // 
            // UC_Acc_Account
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "UC_Acc_Account";
            this.Size = new System.Drawing.Size(2019, 1176);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnMClaim;
    }
}
