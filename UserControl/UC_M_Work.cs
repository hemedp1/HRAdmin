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
    public partial class UC_M_Work : System.Windows.Forms.UserControl
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
            dgvW.AllowUserToAddRows = true; // Ensure users can add rows
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
            dt.Columns.Add("InvoiceAmount", typeof(decimal));
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
                con.Open(); // Explicitly open the connection

                // Define the INSERT command for all nullable columns, excluding ID (auto-increment)
                string insertQuery = "INSERT INTO tbl_MiscellaneousClaim (SerialNo, Requester, Department, ExpensesType, RequestDate, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice) " +
                                     "VALUES (@SerialNo, @Requester, @Department, @ExpensesType, @RequestDate, @Vendor, @Item, @InvoiceAmount, @InvoiceNo, @Invoice)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                {
                    cmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar, 50, "SerialNo");
                    cmd.Parameters.Add("@Requester", SqlDbType.NVarChar, 50, "Requester");
                    cmd.Parameters.Add("@Department", SqlDbType.NVarChar, 50, "Department");
                    cmd.Parameters.Add("@ExpensesType", SqlDbType.NVarChar, 50, "ExpensesType");
                    cmd.Parameters.Add("@RequestDate", SqlDbType.DateTime, 8, "RequestDate");
                    cmd.Parameters.Add("@Vendor", SqlDbType.NVarChar, 50, "Vendor");
                    cmd.Parameters.Add("@Item", SqlDbType.NVarChar, 50, "Item");
                    cmd.Parameters.Add("@InvoiceAmount", SqlDbType.Decimal, 18, "InvoiceAmount");
                    cmd.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar, 50, "InvoiceNo");
                    cmd.Parameters.Add("@Invoice", SqlDbType.NVarChar, 50, "Invoice");


                    using (SqlDataAdapter adapter = new SqlDataAdapter())
                    {
                        adapter.InsertCommand = cmd;

                        // Get the DataTable from the DataGridView
                        DataTable dt = (DataTable)dgvW.DataSource;
                        DataTable newRows = dt.GetChanges(DataRowState.Added);

                        try
                        {
                            if (newRows != null && newRows.Rows.Count > 0)
                            {
                                // Set default values for new rows
                                foreach (DataRow row in newRows.Rows)
                                {
                                    if (row["Requester"] == DBNull.Value || string.IsNullOrEmpty(row["Requester"].ToString()))
                                        row["Requester"] = loggedInUser;
                                    if (row["Department"] == DBNull.Value || string.IsNullOrEmpty(row["Department"].ToString()))
                                        row["Department"] = loggedInDepart;
                                    if (row["RequestDate"] == DBNull.Value)
                                        row["RequestDate"] = DateTime.Now; // Current date and time: 2025-06-17 10:58 AM +08
                                    if (row["ExpensesType"] == DBNull.Value || string.IsNullOrEmpty(row["ExpensesType"].ToString()))
                                        row["ExpensesType"] = expensesType;
                                    // SerialNo can remain NULL or be generated if required
                                }

                                adapter.Update(newRows);
                                dt.AcceptChanges();
                            }
                            else
                            {
                                MessageBox.Show("No new rows to add.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }

                            MessageBox.Show("New claim added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                // Return to same page
                Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Work"; // Updated label to reflect Work
                UC_M_Work ug = new UC_M_Work(loggedInUser, loggedInDepart, expensesType);
                addControls(ug);
            }
        }
    }
}