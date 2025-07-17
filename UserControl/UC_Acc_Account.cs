using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HRAdmin.Forms;

namespace HRAdmin.UserControl
{
    public partial class UC_Acc_Account: System.Windows.Forms.UserControl
    {
        private string LoggedInUser;
        private string loggedInDepart;
        private string loggedInIndex;
        public UC_Acc_Account(string department, string emp, string username)
        {
            InitializeComponent();
            LoggedInUser = username;
            loggedInDepart = department;
            loggedInIndex = emp;
            
        }

        private void btnMClaim_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim";
            //Form_Home.sharedbtnMCReport.Visible = true;
            //Form_Home.sharedbtnApproval.Visible = true;

            UC_M_MiscellaneousClaim ug = new UC_M_MiscellaneousClaim(LoggedInUser, loggedInDepart, loggedInIndex);
            addControls(ug);
        }
        private void addControls(System.Windows.Forms.UserControl userControl)
        {
            if (Form_Home.sharedPanel != null && Form_Home.sharedLabel != null)
            {
                Form_Home.sharedPanel.Controls.Clear();
                userControl.Dock = DockStyle.Fill;
                Form_Home.sharedPanel.Controls.Add(userControl);
                userControl.BringToFront();
                // Update label text if needed
            }
            else
            {
                MessageBox.Show("Panel not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
