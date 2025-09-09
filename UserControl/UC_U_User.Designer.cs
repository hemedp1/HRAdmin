namespace HRAdmin.UserControl
{
    partial class UC_U_User
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_U_User));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnCarBooking = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.btnCarBooking);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Calibri", 14F);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1142, 596);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "User";
            // 
            // btnCarBooking
            // 
            this.btnCarBooking.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnCarBooking.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnCarBooking.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCarBooking.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9F);
            this.btnCarBooking.ForeColor = System.Drawing.Color.Black;
            this.btnCarBooking.Image = ((System.Drawing.Image)(resources.GetObject("btnCarBooking.Image")));
            this.btnCarBooking.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCarBooking.Location = new System.Drawing.Point(6, 41);
            this.btnCarBooking.Name = "btnCarBooking";
            this.btnCarBooking.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnCarBooking.Size = new System.Drawing.Size(222, 73);
            this.btnCarBooking.TabIndex = 5;
            this.btnCarBooking.Tag = " Car Booking";
            this.btnCarBooking.Text = "Profile";
            this.btnCarBooking.UseVisualStyleBackColor = true;
            this.btnCarBooking.Click += new System.EventHandler(this.btnCarBooking_Click);
            // 
            // UC_U_User
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "UC_U_User";
            this.Size = new System.Drawing.Size(1142, 596);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnCarBooking;
    }
}
