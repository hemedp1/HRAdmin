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
using System.IO;

namespace HRAdmin.UserControl
{
    public partial class UC_M_Work : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private string expensesType; // To store the selected ExpensesType
        private OpenFileDialog openFileDialog;

        public UC_M_Work(string username, string department, string selectedType)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            expensesType = selectedType; // Set ExpensesType based on navigation
            InitializeDataTable();
            ConfigureDataGridView();
            StyleDataGridView(dgvW); // Apply styling to the DataGridView
            dgvW.AllowUserToAddRows = true; // Ensure users can add rows
            // Attach the CellFormatting event to handle the RM prefix display
            dgvW.CellFormatting += new DataGridViewCellFormattingEventHandler(dgvW_CellFormatting);
            openFileDialog = new OpenFileDialog { Filter = "PDF files (*.pdf)|*.pdf", Title = "Select Invoice PDF" };
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
            dt.Columns.Add("InvoiceAmount", typeof(decimal)); // Changed to decimal for proper numeric handling
            dt.Columns.Add("Invoice No", typeof(string));
            dt.Columns.Add("Invoice", typeof(byte[])); // Store as byte array
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

            // Add button column for uploading/viewing PDF
            DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn
            {
                Name = "UploadViewInvoice",
                HeaderText = "Invoice",
                Text = "Upload/View",
                UseColumnTextForButtonValue = true
            };
            dgvW.Columns.Add(btnCol);
            dgvW.CellContentClick += new DataGridViewCellEventHandler(dgvW_CellContentClick);
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.ColumnHeadersVisible = true;
            dgv.Dock = DockStyle.Fill;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false; // Override to false as per method, but set to true in constructor
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

            // Configure InvoiceAmount column to show RM prefix
            if (dgvW.Columns.Contains("InvoiceAmount"))
            {
                dgvW.Columns["InvoiceAmount"].DefaultCellStyle.Format = "RM #,##0.00";
                dgvW.Columns["InvoiceAmount"].DefaultCellStyle.NullValue = "RM 0.00"; // Default to 0.00 if null
            }

            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.Black,
                    Font = new Font("Helvetica", 12),
                    BackColor = Color.WhiteSmoke
                };
                column.Resizable = DataGridViewTriState.False;
                column.ReadOnly = false; // Ensure columns are editable
            }
        }

        private void dgvW_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Apply RM prefix to InvoiceAmount column
            if (e.ColumnIndex == dgvW.Columns["InvoiceAmount"].Index && e.RowIndex >= 0)
            {
                if (e.Value == null || e.Value == DBNull.Value)
                {
                    e.Value = "RM 0.00";
                    e.FormattingApplied = true;
                }
                else if (e.Value is decimal amount)
                {
                    e.Value = "RM " + amount.ToString("#,##0.00");
                    e.FormattingApplied = true;
                }
            }
        }

        private void dgvW_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvW.Columns[e.ColumnIndex].Name == "UploadViewInvoice")
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    byte[] fileBytes = File.ReadAllBytes(filePath);
                    dgvW.Rows[e.RowIndex].Cells["Invoice"].Value = fileBytes; // Store byte array
                    MessageBox.Show("Invoice uploaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else if (dgvW.Columns[e.ColumnIndex].Name == "UploadViewInvoice" && dgvW.Rows[e.RowIndex].Cells["Invoice"].Value != null)
            {
                byte[] fileBytes = (byte[])dgvW.Rows[e.RowIndex].Cells["Invoice"].Value;
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    Title = "Save Invoice PDF",
                    FileName = "Invoice_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf"
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, fileBytes);
                    // Optionally open the file after saving
                    System.Diagnostics.Process.Start(saveFileDialog.FileName);
                    MessageBox.Show("Invoice saved and opened successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
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
                con.Open();

                string insertQuery = "INSERT INTO tbl_MiscellaneousClaim (SerialNo, Requester, Department, ExpensesType, RequestDate, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice, " +
                                     "HODApprovalStatus, HRApprovalStatus, AccountApprovalStatus) " +
                                     "VALUES (@SerialNo, @Requester, @Department, @ExpensesType, @RequestDate, @Vendor, @Item, @InvoiceAmount, @InvoiceNo, @Invoice, " +
                                     "@HODApprovalStatus, @HRApprovalStatus, @AccountApprovalStatus)";
                using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter())
                    {
                        adapter.InsertCommand = cmd;

                        DataTable dt = (DataTable)dgvW.DataSource;
                        DataTable newRows = dt.GetChanges(DataRowState.Added);

                        try
                        {
                            if (newRows != null && newRows.Rows.Count > 0)
                            {
                                foreach (DataRow row in newRows.Rows)
                                {
                                    if (row["Requester"] == DBNull.Value || string.IsNullOrEmpty(row["Requester"].ToString()))
                                        row["Requester"] = loggedInUser;
                                    if (row["Department"] == DBNull.Value || string.IsNullOrEmpty(row["Department"].ToString()))
                                        row["Department"] = loggedInDepart;
                                    if (row["RequestDate"] == DBNull.Value)
                                        row["RequestDate"] = DateTime.Now; // Use DateTime.Now directly
                                    if (row["ExpensesType"] == DBNull.Value || string.IsNullOrEmpty(row["ExpensesType"].ToString()))
                                        row["ExpensesType"] = expensesType;
                                    if (row["HODApprovalStatus"] == DBNull.Value || string.IsNullOrEmpty(row["HODApprovalStatus"].ToString()))
                                        row["HODApprovalStatus"] = "Pending";
                                    if (row["HRApprovalStatus"] == DBNull.Value || string.IsNullOrEmpty(row["HRApprovalStatus"].ToString()))
                                        row["HRApprovalStatus"] = "Pending";
                                    if (row["AccountApprovalStatus"] == DBNull.Value || string.IsNullOrEmpty(row["AccountApprovalStatus"].ToString()))
                                        row["AccountApprovalStatus"] = "Pending";

                                    // Handle InvoiceAmount
                                    if (row["InvoiceAmount"] == DBNull.Value || row["InvoiceAmount"] == null)
                                    {
                                        row["InvoiceAmount"] = 0.00m; // Default to 0.00 if null
                                    }

                                    // Generate SerialNo
                                    int id = (int)row["ID"];
                                    string combinedValue = $"{loggedInDepart}_{DateTime.Now:ddMMyyyy_HHmmss}_{id}";
                                    row["SerialNo"] = combinedValue;

                                    // Add parameters dynamically for each row
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("@SerialNo", combinedValue);
                                    cmd.Parameters.AddWithValue("@Requester", row["Requester"]);
                                    cmd.Parameters.AddWithValue("@Department", row["Department"]);
                                    cmd.Parameters.AddWithValue("@ExpensesType", row["ExpensesType"]);
                                    cmd.Parameters.AddWithValue("@RequestDate", row["RequestDate"]);
                                    cmd.Parameters.AddWithValue("@Vendor", row["Vendor"] ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Item", row["Item"] ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@InvoiceAmount", row["InvoiceAmount"] ?? (object)0.00m);
                                    cmd.Parameters.AddWithValue("@InvoiceNo", row["Invoice No"] ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@Invoice", row["Invoice"] ?? (object)DBNull.Value);
                                    cmd.Parameters.AddWithValue("@HODApprovalStatus", row["HODApprovalStatus"]);
                                    cmd.Parameters.AddWithValue("@HRApprovalStatus", row["HRApprovalStatus"]);
                                    cmd.Parameters.AddWithValue("@AccountApprovalStatus", row["AccountApprovalStatus"]);

                                    cmd.ExecuteNonQuery(); // Execute insert for each row
                                }
                                dt.AcceptChanges();
                                MessageBox.Show("New claim added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("No new rows to add.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
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

                Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Work";
                UC_M_Work ug = new UC_M_Work(loggedInUser, loggedInDepart, expensesType);
                addControls(ug);
            }
        }
    }
}