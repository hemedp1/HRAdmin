namespace HRAdmin.UserControl
{
    partial class UC_A_Admin
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UC_A_Admin));
            this.btnWB = new System.Windows.Forms.Button();
            this.btnMeeting = new System.Windows.Forms.Button();
            this.btnCarBooking = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnMeal = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnWB
            // 
            this.btnWB.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnWB.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnWB.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWB.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9F);
            this.btnWB.ForeColor = System.Drawing.Color.Black;
            this.btnWB.Image = ((System.Drawing.Image)(resources.GetObject("btnWB.Image")));
            this.btnWB.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnWB.Location = new System.Drawing.Point(462, 41);
            this.btnWB.Name = "btnWB";
            this.btnWB.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnWB.Size = new System.Drawing.Size(222, 73);
            this.btnWB.TabIndex = 7;
            this.btnWB.Tag = "       Welcome Board";
            this.btnWB.Text = "       Welcome Board";
            this.btnWB.UseVisualStyleBackColor = true;
            this.btnWB.Click += new System.EventHandler(this.btnWB_Click);
            // 
            // btnMeeting
            // 
            this.btnMeeting.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnMeeting.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnMeeting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMeeting.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9F);
            this.btnMeeting.ForeColor = System.Drawing.Color.Black;
            this.btnMeeting.Image = ((System.Drawing.Image)(resources.GetObject("btnMeeting.Image")));
            this.btnMeeting.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMeeting.Location = new System.Drawing.Point(234, 41);
            this.btnMeeting.Name = "btnMeeting";
            this.btnMeeting.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnMeeting.Size = new System.Drawing.Size(222, 73);
            this.btnMeeting.TabIndex = 6;
            this.btnMeeting.Tag = "    Meeting Room";
            this.btnMeeting.Text = "    Meeting Room";
            this.btnMeeting.UseVisualStyleBackColor = true;
            this.btnMeeting.Click += new System.EventHandler(this.btnMeeting_Click);
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
            this.btnCarBooking.Text = "Car Booking";
            this.btnCarBooking.UseVisualStyleBackColor = true;
            this.btnCarBooking.Click += new System.EventHandler(this.btnCarBooking_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.btnMeal);
            this.groupBox1.Controls.Add(this.btnWB);
            this.groupBox1.Controls.Add(this.btnCarBooking);
            this.groupBox1.Controls.Add(this.btnMeeting);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Calibri", 14F);
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1416, 789);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Admin";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // btnMeal
            // 
            this.btnMeal.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnMeal.FlatAppearance.MouseOverBackColor = System.Drawing.Color.OrangeRed;
            this.btnMeal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMeal.Font = new System.Drawing.Font("Arial Rounded MT Bold", 9F);
            this.btnMeal.ForeColor = System.Drawing.Color.Black;
            this.btnMeal.Image = ((System.Drawing.Image)(resources.GetObject("btnMeal.Image")));
            this.btnMeal.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMeal.Location = new System.Drawing.Point(681, 42);
            this.btnMeal.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnMeal.Name = "btnMeal";
            this.btnMeal.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.btnMeal.Size = new System.Drawing.Size(222, 72);
            this.btnMeal.TabIndex = 9;
            this.btnMeal.Tag = "Meal Request";
            this.btnMeal.Text = "Meal Request";
            this.btnMeal.UseVisualStyleBackColor = true;
            this.btnMeal.Click += new System.EventHandler(this.btnMeal_Click);
            // 
            // UC_A_Admin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "UC_A_Admin";
            this.Size = new System.Drawing.Size(1416, 789);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnWB;
        private System.Windows.Forms.Button btnMeeting;
        private System.Windows.Forms.Button btnCarBooking;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnMeal;
    }
}
