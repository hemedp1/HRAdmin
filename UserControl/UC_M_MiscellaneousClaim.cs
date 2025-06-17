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
    public partial class UC_M_MiscellaneousClaim : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        public UC_M_MiscellaneousClaim(string username, string department)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            dtRequest.Text = DateTime.Now.ToString("dd.MM.yyyy");
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
            Form_Home.sharedLabel.Text = "Account";

            UC_A_Account ug = new UC_A_Account(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            string selectedType = cmbType.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedType))
            {
                MessageBox.Show("Please select an Expenses claim type before proceeding.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedType == "Work")
            {
                Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Work";
                UC_M_Work ug = new UC_M_Work(loggedInUser, loggedInDepart, selectedType);
                addControls(ug);
            }
            else if (selectedType == "Benefit")
            {
                Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Benefit";
                UC_M_Benefit ug = new UC_M_Benefit(loggedInUser, loggedInDepart);
                addControls(ug);
            }
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {

        }
    }
}
