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
using System.Data.SqlClient;
using System.Configuration;

namespace HRAdmin.UserControl
{
    public partial class UC_W_WithdrawVisitor : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private UC_W_WelcomeBoard welcomeBoardInstance;

        public UC_W_WithdrawVisitor(string username, string department, UC_W_WelcomeBoard welcomeBoard)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            welcomeBoardInstance = welcomeBoard;

            // Attach the ValueChanged event handler for dtpStartDate
            dtpStartDate.ValueChanged += dtpStartDate_ValueChanged;
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

        private void button2_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "Welcome Board";
            Form_Home.sharedButton4.Visible = false;
            Form_Home.sharedButton5.Visible = false;
            Form_Home.sharedButton6.Visible = false;
            Form_Home.sharedbtnVisitor.Visible = true;
            Form_Home.sharedbtnWithdrawEntry.Visible = true;
            Form_Home.sharedbtnNewVisitor.Visible = true;
            Form_Home.sharedbtnUpdate.Visible = true;
            UC_W_WelcomeBoard ug = new UC_W_WelcomeBoard(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void dtpStartDate_ValueChanged(object sender, EventArgs e)
        {
            // Clear the existing items in the ComboBox
            cmbCompany.Items.Clear();
            cmbCompany.Text = string.Empty; // Reset the selected text

            // Get the selected date from dtpStartDate
            DateTime selectedDate = dtpStartDate.Value.Date;

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Query to get distinct companies for the selected StartDate
                    string query = "SELECT DISTINCT Company FROM tbl_WelcomeBoard WHERE CAST(StartDate AS DATE) = @StartDate";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameter to prevent SQL injection
                        command.Parameters.AddWithValue("@StartDate", selectedDate);

                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        // Check if there are any results
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                // Add each company to the ComboBox
                                string company = reader["Company"].ToString();
                                if (!string.IsNullOrEmpty(company))
                                {
                                    cmbCompany.Items.Add(company);
                                }
                            }
                        }

                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving companies: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnWithdraw_Click(object sender, EventArgs e)
        {
            // Validate inputs
            if (cmbCompany.SelectedItem == null || string.IsNullOrEmpty(cmbCompany.SelectedItem.ToString()))
            {
                MessageBox.Show("Please select a company.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime selectedDate = dtpStartDate.Value.Date;
            string selectedCompany = cmbCompany.SelectedItem.ToString();

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Query to delete records matching the selected StartDate and Company
                    string query = "DELETE FROM tbl_WelcomeBoard WHERE CAST(StartDate AS DATE) = @StartDate AND Company = @Company";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to prevent SQL injection
                        command.Parameters.AddWithValue("@StartDate", selectedDate);
                        command.Parameters.AddWithValue("@Company", selectedCompany);

                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Record successfully withdrawn.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Refresh the ComboBox to reflect the updated data
                            dtpStartDate_ValueChanged(null, null);

                            Form_Home.sharedLabel.Text = "Welcome Board > Withdraw";
                            Form_Home.sharedButton4.Visible = false;
                            Form_Home.sharedButton5.Visible = false;
                            Form_Home.sharedButton6.Visible = false;
                            Form_Home.sharedbtnVisitor.Visible = false;
                            Form_Home.sharedbtnWithdrawEntry.Visible = false;
                            Form_Home.sharedbtnNewVisitor.Visible = false;
                            Form_Home.sharedbtnUpdate.Visible = false;
                            UC_W_WithdrawVisitor ug = new UC_W_WithdrawVisitor(loggedInUser, loggedInDepart, welcomeBoardInstance);
                            addControls(ug);
                        }
                        else
                        {
                            MessageBox.Show("No records found to delete for the selected date and company.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting record: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

  
    }
}