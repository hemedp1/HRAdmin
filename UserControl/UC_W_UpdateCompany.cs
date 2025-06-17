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
using System.Configuration;
using System.Data.SqlClient;
using DrawingFont = System.Drawing.Font;

namespace HRAdmin.UserControl
{
    public partial class UC_W_UpdateCompany : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;

        public UC_W_UpdateCompany(string username, string department)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            InitializeDataGridView(); // Initialize the DataGridView columns
            LoadCompanies(); // Load companies into ComboBox when the control is initialized
            cmb_Company.SelectedIndexChanged += cmb_Company_SelectedIndexChanged; // Attach event handler
        }

        private void InitializeDataGridView()
        {
            // Clear any existing columns
            dgv_UC.Columns.Clear();

            // Add columns for Company and Visitor (visible)
            DataGridViewTextBoxColumn companyColumn = new DataGridViewTextBoxColumn
            {
                Name = "CompanyColumn",
                HeaderText = "Company",
                Width = 150
            };

            DataGridViewTextBoxColumn visitorColumn = new DataGridViewTextBoxColumn
            {
                Name = "VisitorColumn",
                HeaderText = "Visitor",
                Width = 150
            };

            // Add hidden columns to store original values
            DataGridViewTextBoxColumn originalCompanyColumn = new DataGridViewTextBoxColumn
            {
                Name = "OriginalCompanyColumn",
                HeaderText = "Original Company",
                Visible = false // Hidden
            };

            DataGridViewTextBoxColumn originalVisitorColumn = new DataGridViewTextBoxColumn
            {
                Name = "OriginalVisitorColumn",
                HeaderText = "Original Visitor",
                Visible = false // Hidden
            };

            dgv_UC.Columns.Add(companyColumn);
            dgv_UC.Columns.Add(visitorColumn);
            dgv_UC.Columns.Add(originalCompanyColumn);
            dgv_UC.Columns.Add(originalVisitorColumn);

            // Style the DataGridView
            StyleDataGridView(dgv_UC);
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.ColumnHeadersVisible = true;
            dgv.Dock = DockStyle.Fill;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = false; // Allow editing

            // Increase header font size and height
            dgv.EnableHeadersVisualStyles = false; // Allow custom styling
            dgv.ColumnHeadersDefaultCellStyle.Font = new DrawingFont("Helvetica", 13, FontStyle.Bold); // Larger font for headers
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray; // Optional: Add a background color for headers
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black; // Text color for headers
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing; // Allow resizing of header height
            dgv.ColumnHeadersHeight = 30; // Increase header height for better visibility

            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.Black,
                    Font = new DrawingFont("Helvetica", 12),
                    BackColor = Color.WhiteSmoke
                };
                column.Resizable = DataGridViewTriState.False;
                column.ReadOnly = false; // Ensure columns are editable
            }
        }

        private void LoadCompanies()
        {
            try
            {
                // Clear existing items in the ComboBox
                cmb_Company.Items.Clear();

                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // SQL query to fetch distinct company names from tbl_RegisterVisitor
                    string query = "SELECT DISTINCT Company FROM tbl_RegisterVisitor ORDER BY Company";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Add each company to the ComboBox
                            while (reader.Read())
                            {
                                string company = reader["Company"].ToString();
                                if (!string.IsNullOrEmpty(company))
                                {
                                    cmb_Company.Items.Add(company);
                                }
                            }
                        }
                        conn.Close();
                    }
                }

                // Optional: Select the first item in the ComboBox if available
                if (cmb_Company.Items.Count > 0)
                {
                    cmb_Company.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading companies: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadVisitorsByCompany(string company)
        {
            try
            {
                // Clear existing rows in the DataGridView
                dgv_UC.Rows.Clear();

                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // SQL query to fetch all visitors for the selected company
                    string query = "SELECT Company, Visitor FROM tbl_RegisterVisitor WHERE Company = @Company";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Company", company);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string selectedCompany = reader["Company"].ToString();
                                string visitor = reader["Visitor"].ToString();
                                if (!string.IsNullOrEmpty(visitor))
                                {
                                    // Add the row with both current and original values
                                    dgv_UC.Rows.Add(selectedCompany, visitor, selectedCompany, visitor);
                                }
                            }
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading visitors: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmb_Company_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_Company.SelectedItem != null)
            {
                string selectedCompany = cmb_Company.SelectedItem.ToString();
                LoadVisitorsByCompany(selectedCompany);
            }
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

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmb_Company.SelectedItem == null)
            {
                MessageBox.Show("Company is required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string initialCompany = cmb_Company.SelectedItem.ToString();
            bool changesSaved = false;

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Step 1: Collect all changes
                    foreach (DataGridViewRow row in dgv_UC.Rows)
                    {
                        if (row.Cells["CompanyColumn"].Value != null && row.Cells["VisitorColumn"].Value != null &&
                            row.Cells["OriginalCompanyColumn"].Value != null && row.Cells["OriginalVisitorColumn"].Value != null &&
                            !string.IsNullOrEmpty(row.Cells["CompanyColumn"].Value.ToString()) &&
                            !string.IsNullOrEmpty(row.Cells["VisitorColumn"].Value.ToString()))
                        {
                            string newCompany = row.Cells["CompanyColumn"].Value.ToString();
                            string newVisitor = row.Cells["VisitorColumn"].Value.ToString();
                            string originalCompany = row.Cells["OriginalCompanyColumn"].Value.ToString();
                            string originalVisitor = row.Cells["OriginalVisitorColumn"].Value.ToString();

                            // Skip if neither company nor visitor has changed
                            if (newCompany == originalCompany && newVisitor == originalVisitor)
                                continue;

                            // Check if the new company exists
                            string checkQuery = "SELECT COUNT(*) FROM tbl_RegisterVisitor WHERE Company = @NewCompany";
                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                            {
                                checkCmd.Parameters.AddWithValue("@NewCompany", newCompany);
                                int count = (int)checkCmd.ExecuteScalar();
                                if (count == 0)
                                {
                                    MessageBox.Show($"{newCompany} company does not exist in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }

                            // Check for duplicate visitor in the new company
                            string duplicateCheckQuery = "SELECT COUNT(*) FROM tbl_RegisterVisitor WHERE Company = @NewCompany AND Visitor = @NewVisitor AND (Company != @OriginalCompany OR Visitor != @OriginalVisitor)";
                            using (SqlCommand duplicateCheckCmd = new SqlCommand(duplicateCheckQuery, conn))
                            {
                                duplicateCheckCmd.Parameters.AddWithValue("@NewCompany", newCompany);
                                duplicateCheckCmd.Parameters.AddWithValue("@NewVisitor", newVisitor);
                                duplicateCheckCmd.Parameters.AddWithValue("@OriginalCompany", originalCompany);
                                duplicateCheckCmd.Parameters.AddWithValue("@OriginalVisitor", originalVisitor);
                                int duplicateCount = (int)duplicateCheckCmd.ExecuteScalar();
                                if (duplicateCount > 0)
                                {
                                    MessageBox.Show($"Visitor {newVisitor} already exists in {newCompany} company.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }

                            // Update the database with the new values
                            string updateQuery = "UPDATE tbl_RegisterVisitor SET Company = @NewCompany, Visitor = @NewVisitor WHERE Company = @OriginalCompany AND Visitor = @OriginalVisitor";
                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                            {
                                updateCmd.Parameters.AddWithValue("@NewCompany", newCompany);
                                updateCmd.Parameters.AddWithValue("@NewVisitor", newVisitor);
                                updateCmd.Parameters.AddWithValue("@OriginalCompany", originalCompany);
                                updateCmd.Parameters.AddWithValue("@OriginalVisitor", originalVisitor);
                                int rowsAffected = updateCmd.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    MessageBox.Show($"Failed to update {originalVisitor} from {originalCompany}. Record not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }

                            // Update the original values in the DataGridView to reflect the new state
                            row.Cells["OriginalCompanyColumn"].Value = newCompany;
                            row.Cells["OriginalVisitorColumn"].Value = newVisitor;
                        }
                    }

                    changesSaved = true;
                    conn.Close();
                }

                if (changesSaved)
                {
                    MessageBox.Show("Changes updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadVisitorsByCompany(initialCompany); // Refresh the DataGridView

                    // Stay on the same page
                    Form_Home.sharedLabel.Text = "Register Visitor";
                    Form_Home.sharedButton.Visible = false;
                    Form_Home.sharedButtonew.Visible = false;
                    Form_Home.sharedButtonBC.Visible = false;
                    Form_Home.sharedButton4.Visible = false;
                    Form_Home.sharedButton5.Visible = false;
                    Form_Home.sharedbtnVisitor.Visible = false;
                    Form_Home.sharedbtnWithdrawEntry.Visible = false;
                    Form_Home.sharedbtnNewVisitor.Visible = false;

                    UC_W_UpdateCompany mainPage = new UC_W_UpdateCompany(loggedInUser, loggedInDepart);
                    addControls(mainPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Check if a row or a cell is selected
            DataGridViewRow selectedRow = null;
            if (dgv_UC.SelectedRows.Count > 0)
            {
                // If a row is selected, use it
                selectedRow = dgv_UC.SelectedRows[0];
            }
            else if (dgv_UC.SelectedCells.Count > 0)
            {
                // If a cell is selected, use the row of the selected cell
                selectedRow = dgv_UC.SelectedCells[0].OwningRow;
            }
            else
            {
                MessageBox.Show("Please select a cell to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the company and visitor from the selected row
            string company = selectedRow.Cells["CompanyColumn"].Value?.ToString();
            string visitor = selectedRow.Cells["VisitorColumn"].Value?.ToString();

            // Validate inputs
            if (string.IsNullOrEmpty(company) || string.IsNullOrEmpty(visitor))
            {
                MessageBox.Show("Invalid cell selected. Please ensure both company and visitor are specified.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check for duplicate rows
                    string checkQuery = "SELECT COUNT(*) FROM tbl_RegisterVisitor WHERE Company = @Company AND Visitor = @Visitor";
                    int duplicateCount;
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Company", company);
                        checkCmd.Parameters.AddWithValue("@Visitor", visitor);
                        duplicateCount = (int)checkCmd.ExecuteScalar();
                    }

                    // Confirm deletion
                    DialogResult confirmResult = MessageBox.Show(
                        $"Are you sure you want to delete visitor {visitor} from {company} company?",
                        "Confirm Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (confirmResult != DialogResult.Yes)
                    {
                        conn.Close();
                        return;
                    }

                    // Delete only one matching row
                    string deleteQuery = "DELETE TOP (1) FROM tbl_RegisterVisitor WHERE Company = @Company AND Visitor = @Visitor";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn))
                    {
                        deleteCmd.Parameters.AddWithValue("@Company", company);
                        deleteCmd.Parameters.AddWithValue("@Visitor", visitor);
                        int rowsAffected = deleteCmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            MessageBox.Show($"Failed to delete {visitor} from {company}. Record not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            conn.Close();
                            return;
                        }
                    }

                    conn.Close();
                    MessageBox.Show("Visitor deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh the DataGridView
                    LoadVisitorsByCompany(company);

                    // Stay on the same page
                    Form_Home.sharedLabel.Text = "Register Visitor";
                    Form_Home.sharedButton.Visible = false;
                    Form_Home.sharedButtonew.Visible = false;
                    Form_Home.sharedButtonBC.Visible = false;
                    Form_Home.sharedButton4.Visible = false;
                    Form_Home.sharedButton5.Visible = false;
                    Form_Home.sharedbtnVisitor.Visible = false;
                    Form_Home.sharedbtnWithdrawEntry.Visible = false;
                    Form_Home.sharedbtnNewVisitor.Visible = false;

                    UC_W_UpdateCompany mainPage = new UC_W_UpdateCompany(loggedInUser, loggedInDepart);
                    addControls(mainPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting visitor: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgv_UC_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}