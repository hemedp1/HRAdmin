using HRAdmin.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRAdmin.UserControl
{
    public partial class UC_U_User : System.Windows.Forms.UserControl
    {
        public UC_U_User()
        {
            InitializeComponent();
        }
        private void addControls(System.Windows.Forms.UserControl userControl)
        {
            if (Form_Home.sharedPanel != null && Form_Home.sharedLabel != null)
            {
                Form_Home.sharedPanel.Controls.Clear();
                userControl.Dock = DockStyle.Fill;
                Form_Home.sharedPanel.Controls.Add(userControl);
                userControl.BringToFront();
            }
            else
            {
                MessageBox.Show("Panel not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnCarBooking_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "User > User Profile";
            UC_U_UserProfile ug = new UC_U_UserProfile();
            addControls(ug);
        }
    }
}
