using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using HRAdmin.Forms;

namespace HRAdmin.UserControl
{
    public partial class UC_M_Benefit: System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private string expensesType; // To store the selected ExpensesType
        public UC_M_Benefit(string username, string department, string selectedType)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            expensesType = selectedType; // Set ExpensesType based on navigation
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

        private void btnBack_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim";

            UC_M_MiscellaneousClaim ug = new UC_M_MiscellaneousClaim(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {

        }
    }
}
