﻿using System;
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
using System.IO;

namespace HRAdmin.UserControl
{
    public partial class UC_M_Work : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private string loggedInIndex;
        private string expensesType; // To store the selected ExpensesType

        public UC_M_Work(string username, string department, string selectedType, string emp)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            loggedInIndex = emp;
            expensesType = selectedType; // Set ExpensesType based on navigation
            InitializeDataTable();
            ConfigureDataGridView();
            StyleDataGridView(dgvW); // Apply styling to the DataGridView
            dgvW.DataError += DgvW_DataError; // Attach the DataError event handler
        }

        private void DgvW_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // Check if the error is related to the InvoiceDate column
            if (e.ColumnIndex == dgvW.Columns["InvoiceDate"].Index && e.Exception is FormatException)
            {
                e.Cancel = true; // Prevent the default error dialog
                MessageBox.Show("Only can enter in date format in column InvoiceDate", "Invalid Date Format",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgvW.Rows[e.RowIndex].Cells["InvoiceDate"].Value = DBNull.Value; // Clear invalid input
            }
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
            dt.Columns.Add("EmpNo", typeof(string));
            dt.Columns.Add("Department", typeof(string));
            dt.Columns.Add("BankName", typeof(string));
            dt.Columns.Add("AccountNo", typeof(string));
            dt.Columns.Add("ExpensesType", typeof(string));
            dt.Columns.Add("RequestDate", typeof(DateTime));
            dt.Columns.Add("HODApprovalStatus", typeof(string));
            dt.Columns.Add("ApprovedByHOD", typeof(string));
            dt.Columns.Add("HODApprovedDate", typeof(DateTime));
            dt.Columns.Add("HRApprovalStatus", typeof(string));
            dt.Columns.Add("ApprovedByHR", typeof(string));
            dt.Columns.Add("HRApprovedDate", typeof(DateTime));
            dt.Columns.Add("AccountApprovalStatus", typeof(string));
            dt.Columns.Add("ApprovedByAccount", typeof(string));
            dt.Columns.Add("AccountApprovedDate", typeof(DateTime));
            dt.Columns.Add("Vendor", typeof(string));
            dt.Columns.Add("Item", typeof(string));
            dt.Columns.Add("InvoiceAmount", typeof(decimal));
            dt.Columns.Add("InvoiceNo", typeof(string));
            dt.Columns.Add("InvoiceDate", typeof(DateTime));
            dgvW.DataSource = dt;
        }

        private void ConfigureDataGridView()
        {
            // Hide columns that should not be edited by the user
            dgvW.Columns["SerialNo"].Visible = false;
            dgvW.Columns["Requester"].Visible = false;
            dgvW.Columns["EmpNo"].Visible = false;
            dgvW.Columns["Department"].Visible = false;
            dgvW.Columns["BankName"].Visible = false;
            dgvW.Columns["AccountNo"].Visible = false;
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

            if (dgvW.Columns.Contains("ID"))
            {
                dgvW.Columns["ID"].Width = 30;
            }

            if (dgvW.Columns.Contains("InvoiceAmount"))
            {
                dgvW.Columns["InvoiceAmount"].Width = 150;
            }

            if (dgvW.Columns.Contains("InvoiceNo"))
            {
                dgvW.Columns["InvoiceNo"].Width = 200;
            }

            if (dgvW.Columns.Contains("InvoiceDate"))
            {
                dgvW.Columns["InvoiceDate"].Width = 150;
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
            Form_Home.sharedbtnMCReport.Visible = true;
            Form_Home.sharedbtnApproval.Visible = true;

            UC_M_MiscellaneousClaim ug = new UC_M_MiscellaneousClaim(loggedInUser, loggedInDepart, loggedInIndex);
            addControls(ug);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();

                    // Query to get the highest submission number for the current day across all departments
                    string checkSerialNoQuery = @"SELECT MAX(CAST(RIGHT(SerialNo, CHARINDEX('_', REVERSE(SerialNo)) - 1) AS INT)) 
                                                FROM tbl_DetailClaimForm 
                                                WHERE SerialNo LIKE @DatePattern";
                    string datePattern = $"_%{DateTime.Now:ddMMyyyy}_%"; // Match any SerialNo with the current date
                    int nextNumber = 1;

                    using (SqlCommand cmdCheck = new SqlCommand(checkSerialNoQuery, con, transaction))
                    {
                        cmdCheck.Parameters.AddWithValue("@DatePattern", datePattern);
                        var result = cmdCheck.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            nextNumber = Convert.ToInt32(result) + 1;
                        }
                    }

                    // Generate SerialNo with format Department_ddMMyyyy_N
                    string serialNo = $"{loggedInDepart}_{DateTime.Now:ddMMyyyy}_{nextNumber}";

                    string insertDetailQuery = @"INSERT INTO tbl_DetailClaimForm 
                                        (SerialNo, ExpensesType, Vendor, Item, InvoiceAmount, InvoiceNo, InvoiceDate) 
                                        VALUES (@SerialNo, @ExpensesType, @Vendor, @Item, @InvoiceAmount, @InvoiceNo, @InvoiceDate)";

                    string insertMasterQuery = @"INSERT INTO tbl_MasterClaimForm 
                                        (SerialNo, Requester, EmpNo, Department, BankName, AccountNo, ExpensesType, RequestDate, 
                                         HODApprovalStatus, HRApprovalStatus, AccountApprovalStatus) 
                                        VALUES (@SerialNo, @Requester, @EmpNo, @Department, @BankName, @AccountNo, @ExpensesType, @RequestDate, 
                                                @HODApprovalStatus, @HRApprovalStatus, @AccountApprovalStatus)";

                    string checkDuplicateQuery = @"SELECT COUNT(*) 
                                                 FROM tbl_DetailClaimForm 
                                                 WHERE InvoiceNo = @InvoiceNo AND InvoiceAmount = @InvoiceAmount AND InvoiceDate = @InvoiceDate";

                    DataTable dt = (DataTable)dgvW.DataSource;
                    DataTable newRows = dt?.GetChanges(DataRowState.Added);

                    if (newRows == null || newRows.Rows.Count == 0)
                    {
                        MessageBox.Show("No data to submit the claim.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Check for duplicate InvoiceNo and InvoiceAmount
                    using (SqlCommand cmdCheckDuplicate = new SqlCommand(checkDuplicateQuery, con, transaction))
                    {
                        foreach (DataRow row in newRows.Rows)
                        {
                            if (row["InvoiceNo"] != DBNull.Value && row["InvoiceAmount"] != DBNull.Value && row["InvoiceDate"] != DBNull.Value)
                            {
                                cmdCheckDuplicate.Parameters.Clear();
                                cmdCheckDuplicate.Parameters.AddWithValue("@InvoiceNo", row["InvoiceNo"]);
                                cmdCheckDuplicate.Parameters.AddWithValue("@InvoiceAmount", row["InvoiceAmount"]);
                                cmdCheckDuplicate.Parameters.AddWithValue("@InvoiceDate", row["InvoiceDate"] ?? (object)DBNull.Value);
                                int duplicateCount = (int)cmdCheckDuplicate.ExecuteScalar();
                                if (duplicateCount > 0)
                                {
                                    transaction?.Rollback();
                                    MessageBox.Show($"Reduntant claim request.", "Duplicate Request", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }
                        }
                    }

                    using (SqlCommand cmdDetail = new SqlCommand(insertDetailQuery, con, transaction))
                    using (SqlCommand cmdMaster = new SqlCommand(insertMasterQuery, con, transaction))
                    {
                        foreach (DataRow row in newRows.Rows)
                        {
                            // Set default values for null or empty fields
                            row["Requester"] = row["Requester"] == DBNull.Value || string.IsNullOrEmpty(row["Requester"]?.ToString())
                                ? loggedInUser : row["Requester"];
                            row["EmpNo"] = row["EmpNo"] == DBNull.Value || string.IsNullOrEmpty(row["EmpNo"]?.ToString())
                                ? loggedInIndex : row["EmpNo"];
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

                            // Assign the same SerialNo to all rows in this submission
                            row["SerialNo"] = serialNo;

                            // Insert into tbl_DetailClaimForm
                            cmdDetail.Parameters.Clear();
                            cmdDetail.Parameters.AddWithValue("@SerialNo", serialNo);
                            cmdDetail.Parameters.AddWithValue("@ExpensesType", row["ExpensesType"]);
                            cmdDetail.Parameters.AddWithValue("@Vendor", row["Vendor"] ?? (object)DBNull.Value);
                            cmdDetail.Parameters.AddWithValue("@Item", row["Item"] ?? (object)DBNull.Value);
                            cmdDetail.Parameters.AddWithValue("@InvoiceAmount", row["InvoiceAmount"] != DBNull.Value ? row["InvoiceAmount"] : (object)DBNull.Value);
                            cmdDetail.Parameters.AddWithValue("@InvoiceNo", row["InvoiceNo"] ?? (object)DBNull.Value);
                            cmdDetail.Parameters.AddWithValue("@InvoiceDate", row["InvoiceDate"] ?? (object)DBNull.Value);
                            cmdDetail.ExecuteNonQuery();

                            // Insert into tbl_MasterClaimForm (only once for the first row to avoid duplicates)
                            if (row == newRows.Rows[0])
                            {
                                cmdMaster.Parameters.Clear();
                                cmdMaster.Parameters.AddWithValue("@SerialNo", serialNo);
                                cmdMaster.Parameters.AddWithValue("@Requester", row["Requester"]);
                                cmdMaster.Parameters.AddWithValue("@EmpNo", row["EmpNo"]);
                                cmdMaster.Parameters.AddWithValue("@Department", row["Department"]);
                                cmdMaster.Parameters.AddWithValue("@BankName", row["BankName"] ?? (object)DBNull.Value);
                                cmdMaster.Parameters.AddWithValue("@AccountNo", row["AccountNo"] ?? (object)DBNull.Value);
                                cmdMaster.Parameters.AddWithValue("@ExpensesType", row["ExpensesType"]);
                                cmdMaster.Parameters.AddWithValue("@RequestDate", row["RequestDate"]);
                                cmdMaster.Parameters.AddWithValue("@HODApprovalStatus", row["HODApprovalStatus"]);
                                cmdMaster.Parameters.AddWithValue("@HRApprovalStatus", row["HRApprovalStatus"]);
                                cmdMaster.Parameters.AddWithValue("@AccountApprovalStatus", row["AccountApprovalStatus"]);
                                cmdMaster.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        dt.AcceptChanges();
                        MessageBox.Show("New claim added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Update UI after successful insertion
                        Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Work";
                        UC_M_Work ug = new UC_M_Work(loggedInUser, loggedInDepart, expensesType, loggedInIndex);
                        addControls(ug);
                    }
                }
                catch (SqlException ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show($"A database error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
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