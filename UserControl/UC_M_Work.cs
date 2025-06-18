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

namespace HRAdmin.UserControl
{
    public partial class UC_M_Work: System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private string expensesType; // To store the selected ExpensesType

        public UC_M_Work(string username, string department, string selectedType)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            expensesType = selectedType; // Set ExpensesType based on navigation
            InitializeDataTable();
            ConfigureDataGridView();
            StyleDataGridView(dgvW); // Apply styling to the DataGridView
        }
        private void InitializeDataTable()
        {
            // Initialize an empty DataTable with all columns
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int)).AutoIncrement = true; // Auto-increment for ID
            dt.Columns["ID"].AutoIncrementSeed = 1; // Start ID from 1
            dt.Columns["ID"].AutoIncrementStep = 1; // Increment by 1
            dt.Columns.Add("SerialNo", typeof(string));
            dt.Columns.Add("Requester", typeof(string));
            dt.Columns.Add("Department", typeof(string));
            dt.Columns.Add("ExpensesType", typeof(string));
            dt.Columns.Add("RequestDate", typeof(DateTime));
            dt.Columns.Add("Vendor", typeof(string));
            dt.Columns.Add("Item", typeof(string));
            dt.Columns.Add("InvoiceAmount", typeof(string));
            dt.Columns.Add("InvoiceNo", typeof(string));
            dt.Columns.Add("Invoice", typeof(string));
            dt.Columns.Add("HODApprovalStatus", typeof(string));
            dt.Columns.Add("ApprovedByHOD", typeof(string));
            dt.Columns.Add("HODApprovedDate", typeof(DateTime));
            dt.Columns.Add("HRApprovalStatus", typeof(string));
            dt.Columns.Add("ApprovedByHR", typeof(string));
            dt.Columns.Add("HRApprovedDate", typeof(DateTime));
            dt.Columns.Add("AccountApprovalStatus", typeof(string));
            dt.Columns.Add("ApprovedByAccount", typeof(string));
            dt.Columns.Add("AccountApprovedDate", typeof(DateTime));
            dgvW.DataSource = dt;
        }
        private void ConfigureDataGridView()
        {
            // Hide columns that should not be edited by the user
            dgvW.Columns["SerialNo"].Visible = false;
            dgvW.Columns["Requester"].Visible = false;
            dgvW.Columns["Department"].Visible = false;
            dgvW.Columns["ExpensesType"].Visible = false;
            dgvW.Columns["RequestDate"].Visible = false;
            dgvW.Columns["HODApprovalStatus"].Visible = false;
            dgvW.Columns["ApprovedByHOD"].Visible = false;
            dgvW.Columns["HODApprovedDate"].Visible = false;
            dgvW.Columns["HRApprovalStatus"].Visible = false;
            dgvW.Columns["ApprovedByHR"].Visible = false;
            dgvW.Columns["HRApprovedDate"].Visible = false;
            dgvW.Columns["AccountApprovalStatus"].Visible = false;
            dgvW.Columns["ApprovedByAccount"].Visible = false;
            dgvW.Columns["AccountApprovedDate"].Visible = false;
        }
        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.ColumnHeadersVisible = true;
            dgv.Dock = DockStyle.Fill;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = true; // Override to false as per method, but set to true in constructor
            dgv.ReadOnly = false; // Allow editing

            // Increase header font size and height
            dgv.EnableHeadersVisualStyles = false; // Allow custom styling
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Helvetica", 13, FontStyle.Bold); // Larger font for headers
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray; // Optional: Add a background color for headers
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black; // Text color for headers
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing; // Allow resizing of header height
            dgv.ColumnHeadersHeight = 30; // Increase header height for better visibility

            // Set a smaller width for the ID column (e.g., 50 pixels)
            if (dgvW.Columns.Contains("ID"))
            {
                dgvW.Columns["ID"].Width = 50;
            }

            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.Black,
                    Font = new Font("Helvetica", 12),
                    BackColor = Color.WhiteSmoke
                };
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

        private void btnBack_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim";

            UC_M_MiscellaneousClaim ug = new UC_M_MiscellaneousClaim(loggedInUser, loggedInDepart);
            addControls(ug);

        }
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    string insertQuery = @"INSERT INTO tbl_MiscellaneousClaim 
                                         (SerialNo, Requester, Department, ExpensesType, RequestDate, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice, 
                                         HODApprovalStatus, HRApprovalStatus, AccountApprovalStatus) 
                                         VALUES (@SerialNo, @Requester, @Department, @ExpensesType, @RequestDate, @Vendor, @Item, @InvoiceAmount, 
                                         @InvoiceNo, @Invoice, @HODApprovalStatus, @HRApprovalStatus, @AccountApprovalStatus)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                    {
                        DataTable dt = (DataTable)dgvW.DataSource;
                        DataTable newRows = dt?.GetChanges(DataRowState.Added);

                        if (newRows == null || newRows.Rows.Count == 0)
                        {
                            MessageBox.Show("No new rows to add.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        foreach (DataRow row in newRows.Rows)
                        {
                            // Set default values for null or empty fields
                            row["Requester"] = row["Requester"] == DBNull.Value || string.IsNullOrEmpty(row["Requester"]?.ToString())
                                ? loggedInUser : row["Requester"];
                            row["Department"] = row["Department"] == DBNull.Value || string.IsNullOrEmpty(row["Department"]?.ToString())
                                ? loggedInDepart : row["Department"];
                            row["RequestDate"] = row["RequestDate"] == DBNull.Value ? DateTime.Now : row["RequestDate"];
                            row["ExpensesType"] = row["ExpensesType"] == DBNull.Value || string.IsNullOrEmpty(row["ExpensesType"]?.ToString())
                                ? expensesType : row["ExpensesType"];
                            row["HODApprovalStatus"] = row["HODApprovalStatus"] == DBNull.Value || string.IsNullOrEmpty(row["HODApprovalStatus"]?.ToString())
                                ? "Pending" : row["HODApprovalStatus"];
                            row["HRApprovalStatus"] = row["HRApprovalStatus"] == DBNull.Value || string.IsNullOrEmpty(row["HRApprovalStatus"]?.ToString())
                                ? "Pending" : row["HRApprovalStatus"];
                            row["AccountApprovalStatus"] = row["AccountApprovalStatus"] == DBNull.Value || string.IsNullOrEmpty(row["AccountApprovalStatus"]?.ToString())
                                ? "Pending" : row["AccountApprovalStatus"];

                            // Generate SerialNo (assuming ID column exists or using a counter)
                            int id = dt.Rows.IndexOf(row) + 1; // Fallback if ID column is unavailable
                            if (row.Table.Columns.Contains("ID"))
                                id = Convert.ToInt32(row["ID"]);
                            string combinedValue = $"{loggedInDepart}_{DateTime.Now:ddMMyyyy_HHmmss}_{id}";
                            row["SerialNo"] = combinedValue;

                            // Clear previous parameters
                            cmd.Parameters.Clear();

                            // Add parameters with explicit types
                            cmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = combinedValue;
                            cmd.Parameters.Add("@Requester", SqlDbType.NVarChar).Value = row["Requester"];
                            cmd.Parameters.Add("@Department", SqlDbType.NVarChar).Value = row["Department"];
                            cmd.Parameters.Add("@ExpensesType", SqlDbType.NVarChar).Value = row["ExpensesType"];
                            cmd.Parameters.Add("@RequestDate", SqlDbType.DateTime).Value = row["RequestDate"];
                            cmd.Parameters.Add("@Vendor", SqlDbType.NVarChar).Value = row["Vendor"] ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@Item", SqlDbType.NVarChar).Value = row["Item"] ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@InvoiceAmount", SqlDbType.NVarChar).Value = row["InvoiceAmount"] ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar).Value = row["InvoiceNo"] ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@Invoice", SqlDbType.NVarChar).Value = row["Invoice"] ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@HODApprovalStatus", SqlDbType.NVarChar).Value = row["HODApprovalStatus"];
                            cmd.Parameters.Add("@HRApprovalStatus", SqlDbType.NVarChar).Value = row["HRApprovalStatus"];
                            cmd.Parameters.Add("@AccountApprovalStatus", SqlDbType.NVarChar).Value = row["AccountApprovalStatus"];

                            cmd.ExecuteNonQuery(); // Execute insert for each row
                        }

                        dt.AcceptChanges();
                        MessageBox.Show("New claim added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Update UI after successful insertion
                        Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Work";
                        UC_M_Work ug = new UC_M_Work(loggedInUser, loggedInDepart, expensesType);
                        addControls(ug);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"A database error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (con.State == ConnectionState.Open)
                        con.Close();
                }
            }
        }
    }
}
