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
using System.Diagnostics;

namespace HRAdmin.UserControl
{
    public partial class UC_M_Work : System.Windows.Forms.UserControl
    {
        private string LoggedInUser;
        private string loggedInDepart;
        private string loggedInIndex;
        private string expensesType; // To store the selected ExpensesType
        private DataGridViewCellStyle defaultCellStyle; // Store default cell style for reverting

        public UC_M_Work(string username, string department, string selectedType, string emp)
        {
            InitializeComponent();
            LoggedInUser = username;
            loggedInDepart = department;
            loggedInIndex = emp;
            expensesType = selectedType;
            InitializeDataTable();
            ConfigureDataGridView();
            StyleDataGridView(dgvW);
            dgvW.DataError += DgvW_DataError;
            dgvW.CellValueChanged += dgvW_CellValueChanged;
            dgvW.CellFormatting += dgvW_CellFormatting;
        }

        private void UC_M_Work_Load(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvW.Rows)
            {
                UpdateInvoiceButtonState(row);
            }
        }
        private void UpdateInvoiceButtonState(DataGridViewRow row)
        {
            //if (row.IsNewRow) return;

            bool hasPdf = row.Cells["Invoice"].Value is byte[] pdfData && pdfData.Length > 0;
            row.Cells["InvoiceAttached"].Value = hasPdf ? "✅" : "❌";
            ((DataGridViewButtonCell)row.Cells["btnInvoice"]).Value = hasPdf ? "View / Reupload" : "Upload";
        }

        private void DgvW_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex == dgvW.Columns["Invoice Date"].Index && e.Exception is FormatException)
            {
                e.Cancel = true;
                MessageBox.Show("Only can enter in date format in column Invoice Date", "Invalid Date Format",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgvW.Rows[e.RowIndex].Cells["Invoice Date"].Value = DBNull.Value;
            }
        }

        private void InitializeDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
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
            dt.Columns.Add("Invoice Amount", typeof(decimal));
            dt.Columns.Add("Invoice No", typeof(string));
            dt.Columns.Add("Invoice Date", typeof(DateTime));
            dt.Columns.Add("Invoice", typeof(byte[]));
            dt.Columns.Add("InvoiceAttached", typeof(string));

            dgvW.DataSource = dt;

            DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn
            {
                HeaderText = "Invoice",
                Text = "Upload",
                UseColumnTextForButtonValue = false, // Critical change
                Name = "btnInvoice"
            };
            dgvW.Columns.Add(btnCol);
            dgvW.AllowUserToAddRows = true;
        }

        private void AssignNextId(DataGridViewRow row)
        {
            int maxId = 0;
            foreach (DataGridViewRow existingRow in dgvW.Rows)
            {
                if (existingRow.Cells["ID"].Value != null && existingRow.Cells["ID"].Value != DBNull.Value)
                {
                    int currentId = Convert.ToInt32(existingRow.Cells["ID"].Value);
                    if (currentId > maxId) maxId = currentId;
                }
            }
            row.Cells["ID"].Value = maxId + 1;
        }

        private void dgvW_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // Skip handling for these specific columns
                string columnName = dgvW.Columns[e.ColumnIndex].Name;
                if (columnName == "btnInvoice" || columnName == "InvoiceAttached" || columnName == "Invoice")
                {
                    return;
                }

                DataGridViewRow row = dgvW.Rows[e.RowIndex];

                // Assign ID when a new row is created or edited
                if (row.Cells["ID"].Value == null || Convert.IsDBNull(row.Cells["ID"].Value))
                {
                    AssignNextId(row);
                }

                // Automatically add a new row when the user starts entering data
                if (!dgvW.Rows.Cast<DataGridViewRow>().Any(r => r.IsNewRow && r.Index == dgvW.Rows.Count - 1))
                {
                    int newRowIndex = dgvW.Rows.Add();
                    AssignNextId(dgvW.Rows[newRowIndex]);
                }
            }
        }
        private void UpdateInvoiceButtonState(DataGridViewRow row, bool forceUpdate = false)
        {
            if (row.IsNewRow) return;

            // Only update if forced or if the invoice state actually changed
            bool currentHasPdf = row.Cells["Invoice"].Value is byte[] pdfData && pdfData.Length > 0;
            bool currentAttachedState = row.Cells["InvoiceAttached"].Value?.ToString() == "✅";
            bool currentButtonState = row.Cells["btnInvoice"].Value?.ToString() == "View / Reupload";

            if (forceUpdate ||
                (currentHasPdf && (!currentAttachedState || !currentButtonState)) ||
                (!currentHasPdf && (currentAttachedState || currentButtonState)))
            {
                row.Cells["InvoiceAttached"].Value = currentHasPdf ? "✅" : "❌";
                ((DataGridViewButtonCell)row.Cells["btnInvoice"]).Value = currentHasPdf ? "View / Reupload" : "Upload";
            }
        }
        private void dgvW_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            UpdateInvoiceButtonState(dgvW.Rows[e.RowIndex]);
        }
        private void dgvW_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                UpdateInvoiceButtonState(dgvW.Rows[e.RowIndex]);
            }
        }
        private void dgvW_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dgvW.Columns["Invoice Amount"].Index && e.RowIndex >= 0)
            {
                if (e.Value != null && e.Value != DBNull.Value)
                {
                    e.Value = $"RM {Convert.ToDecimal(e.Value):N2}";
                    e.FormattingApplied = true;
                }
                else
                {
                    e.Value = "RM";
                    e.FormattingApplied = true;
                }
            }
        }
        private void dgvW_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Skip if we're clicking the button column itself
                if (dgvW.Columns[e.ColumnIndex].Name == "btnInvoice")
                    return;

                // Refresh the button state immediately
                BeginInvoke((MethodInvoker)delegate {
                    UpdateInvoiceButtonState(dgvW.Rows[e.RowIndex]);
                });
            }
        }
        private void dgvW_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

            if (dgvW.Columns[e.ColumnIndex].Name == "btnInvoice" && e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvW.Rows[e.RowIndex];
                if (row.IsNewRow) return;

                byte[] currentPdf = row.Cells["Invoice"].Value as byte[];
                bool hasPdf = currentPdf != null && currentPdf.Length > 0;

                if (!hasPdf)
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "PDF files (*.pdf)|*.pdf";
                        ofd.Title = "Select Invoice PDF";

                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                byte[] fileBytes = File.ReadAllBytes(ofd.FileName);
                                row.Cells["Invoice"].Value = fileBytes;
                                UpdateInvoiceButtonState(row);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Error loading file: {ex.Message}", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    return;
                }

                string message = "An invoice is already uploaded. Do you want to reupload it?\n\n" +
                                 "Yes: Reupload\n" +
                                 "No: View";

                DialogResult result = MessageBox.Show(message, "PDF Attachment",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    using (OpenFileDialog ofd = new OpenFileDialog())
                    {
                        ofd.Filter = "PDF files (*.pdf)|*.pdf";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            byte[] fileBytes = File.ReadAllBytes(ofd.FileName);
                            row.Cells["Invoice"].Value = fileBytes;
                            UpdateInvoiceButtonState(row);
                        }
                    }
                }
                else if (result == DialogResult.No)
                {
                    try
                    {
                        string tempPath = Path.Combine(Path.GetTempPath(), $"invoice_{Guid.NewGuid()}.pdf");
                        File.WriteAllBytes(tempPath, currentPdf);
                        Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error opening PDF: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ConfigureDataGridView()
        {
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
            dgvW.Columns["Invoice"].Visible = false;
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.ColumnHeadersVisible = true;
            dgv.Dock = DockStyle.Fill;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = true;
            dgv.ReadOnly = false;

            // Set the row height for all rows
            dgv.RowTemplate.Height = 27;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Helvetica", 13, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgv.ColumnHeadersHeight = 30;

            if (dgvW.Columns.Contains("ID"))
            {
                dgvW.Columns["ID"].Width = 30;
            }

            if (dgvW.Columns.Contains("Invoice Amount"))
            {
                dgvW.Columns["Invoice Amount"].Width = 120;
            }

            if (dgvW.Columns.Contains("Invoice No"))
            {
                dgvW.Columns["Invoice No"].Width = 150;
            }

            if (dgvW.Columns.Contains("Invoice Date"))
            {
                dgvW.Columns["Invoice Date"].Width = 100;
            }

            if (dgvW.Columns.Contains("InvoiceAttached"))
            {
                dgvW.Columns["InvoiceAttached"].Width = 120;
            }

            if (dgvW.Columns.Contains("btnInvoice"))
            {
                dgvW.Columns["btnInvoice"].Width = 120;
            }

            dgvW.Columns["InvoiceAttached"].HeaderText = "Invoice Attached";

            // Define default cell style
            defaultCellStyle = new DataGridViewCellStyle
            {
                ForeColor = Color.Black,
                Font = new Font("Helvetica", 12),
                BackColor = Color.WhiteSmoke
            };

            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.DefaultCellStyle = defaultCellStyle;
            }
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

                    string checkSerialNoQuery = @"SELECT MAX(CAST(RIGHT(SerialNo, CHARINDEX('_', REVERSE(SerialNo)) - 1) AS INT)) 
                                        FROM tbl_DetailClaimForm 
                                        WHERE SerialNo LIKE @DatePattern";
                    string datePattern = $"_%{DateTime.Now:ddMMyyyy}_%";
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

                    string serialNo = $"{loggedInDepart}_{DateTime.Now:ddMMyyyy}_{nextNumber}";

                    string insertDetailQuery = @"INSERT INTO tbl_DetailClaimForm 
    (SerialNo, ExpensesType, Vendor, Item, InvoiceAmount, InvoiceNo, InvoiceDate, Invoice) 
    VALUES (@SerialNo, @ExpensesType, @Vendor, @Item, @InvoiceAmount, @InvoiceNo, @InvoiceDate, @Invoice)";

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

                    // Validate that each row is either fully completed or fully empty
                    foreach (DataGridViewRow row in dgvW.Rows)
                    {
                        if (row.IsNewRow) continue; // Skip the new row placeholder

                        bool isRowEmpty = row.Cells["Vendor"].Value == null || string.IsNullOrEmpty(row.Cells["Vendor"].Value?.ToString()) &&
                                          row.Cells["Item"].Value == null || string.IsNullOrEmpty(row.Cells["Item"].Value?.ToString()) &&
                                          row.Cells["Invoice Amount"].Value == null || row.Cells["Invoice Amount"].Value == DBNull.Value &&
                                          row.Cells["Invoice No"].Value == null || string.IsNullOrEmpty(row.Cells["Invoice No"].Value?.ToString()) &&
                                          row.Cells["Invoice Date"].Value == null || row.Cells["Invoice Date"].Value == DBNull.Value &&
                                          row.Cells["Invoice"].Value == null;

                        bool isRowFullyFilled = row.Cells["Vendor"].Value != null && !string.IsNullOrEmpty(row.Cells["Vendor"].Value?.ToString()) &&
                                               row.Cells["Item"].Value != null && !string.IsNullOrEmpty(row.Cells["Item"].Value?.ToString()) &&
                                               row.Cells["Invoice Amount"].Value != null && row.Cells["Invoice Amount"].Value != DBNull.Value &&
                                               row.Cells["Invoice No"].Value != null && !string.IsNullOrEmpty(row.Cells["Invoice No"].Value?.ToString()) &&
                                               row.Cells["Invoice Date"].Value != null && row.Cells["Invoice Date"].Value != DBNull.Value &&
                                               row.Cells["Invoice"].Value != null;

                        if (!isRowEmpty && !isRowFullyFilled)
                        {
                            List<string> emptyColumns = new List<string>();
                            if (row.Cells["Vendor"].Value == null || string.IsNullOrEmpty(row.Cells["Vendor"].Value?.ToString()))
                                emptyColumns.Add("Vendor");
                            if (row.Cells["Item"].Value == null || string.IsNullOrEmpty(row.Cells["Item"].Value?.ToString()))
                                emptyColumns.Add("Item");
                            if (row.Cells["Invoice Amount"].Value == null || row.Cells["Invoice Amount"].Value == DBNull.Value)
                                emptyColumns.Add("Invoice Amount");
                            if (row.Cells["Invoice No"].Value == null || string.IsNullOrEmpty(row.Cells["Invoice No"].Value?.ToString()))
                                emptyColumns.Add("Invoice No");
                            if (row.Cells["Invoice Date"].Value == null || row.Cells["Invoice Date"].Value == DBNull.Value)
                                emptyColumns.Add("Invoice Date");
                            if (row.Cells["Invoice"].Value == null)
                                emptyColumns.Add("Invoice");

                            // Highlight empty cells in red
                            foreach (string colName in emptyColumns)
                            {
                                row.Cells[colName].Style.BackColor = Color.Red;
                            }

                            transaction?.Rollback();
                            string errorMessage = $"Row {row.Index + 1} is incomplete. Missing values in: {string.Join(", ", emptyColumns)}.";
                            MessageBox.Show(errorMessage, "Incomplete Submission", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            // Revert cell colors to original after OK is clicked
                            foreach (string colName in emptyColumns)
                            {
                                row.Cells[colName].Style = defaultCellStyle;
                            }
                            return;
                        }
                    }

                    // Filter out completely empty rows and check if there are any valid rows to submit
                    DataTable validRows = newRows.Clone();
                    foreach (DataRow row in newRows.Rows)
                    {
                        bool isRowEmpty = row["Vendor"] == DBNull.Value && string.IsNullOrEmpty(row["Vendor"]?.ToString()) &&
                                          row["Item"] == DBNull.Value && string.IsNullOrEmpty(row["Item"]?.ToString()) &&
                                          row["Invoice Amount"] == DBNull.Value &&
                                          row["Invoice No"] == DBNull.Value && string.IsNullOrEmpty(row["Invoice No"]?.ToString()) &&
                                          row["Invoice Date"] == DBNull.Value &&
                                          row["Invoice"] == DBNull.Value;

                        if (!isRowEmpty)
                        {
                            validRows.ImportRow(row);
                        }
                    }

                    if (validRows.Rows.Count == 0)
                    {
                        transaction?.Rollback();
                        MessageBox.Show("No valid data to submit the claim. All rows are empty.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // Check for duplicate claims
                    using (SqlCommand cmdCheckDuplicate = new SqlCommand(checkDuplicateQuery, con, transaction))
                    {
                        foreach (DataRow row in validRows.Rows)
                        {
                            cmdCheckDuplicate.Parameters.Clear();
                            cmdCheckDuplicate.Parameters.AddWithValue("@InvoiceNo", row["Invoice No"]);
                            cmdCheckDuplicate.Parameters.AddWithValue("@InvoiceAmount", row["Invoice Amount"]);
                            cmdCheckDuplicate.Parameters.AddWithValue("@InvoiceDate", row["Invoice Date"]);
                            int duplicateCount = (int)cmdCheckDuplicate.ExecuteScalar();
                            if (duplicateCount > 0)
                            {
                                transaction?.Rollback();
                                MessageBox.Show($"Redundant claim request.", "Duplicate Request", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                    }

                    // Insert data into database
                    using (SqlCommand cmdDetail = new SqlCommand(insertDetailQuery, con, transaction))
                    using (SqlCommand cmdMaster = new SqlCommand(insertMasterQuery, con, transaction))
                    {
                        foreach (DataRow row in validRows.Rows)
                        {
                            row["Requester"] = row["Requester"] == DBNull.Value || string.IsNullOrEmpty(row["Requester"]?.ToString())
                                ? LoggedInUser : row["Requester"];
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

                            row["SerialNo"] = serialNo;

                            cmdDetail.Parameters.Clear();
                            cmdDetail.Parameters.AddWithValue("@SerialNo", serialNo);
                            cmdDetail.Parameters.AddWithValue("@ExpensesType", row["ExpensesType"]);
                            cmdDetail.Parameters.AddWithValue("@Vendor", row["Vendor"] ?? (object)DBNull.Value);
                            cmdDetail.Parameters.AddWithValue("@Item", row["Item"] ?? (object)DBNull.Value);
                            cmdDetail.Parameters.AddWithValue("@InvoiceAmount", row["Invoice Amount"] != DBNull.Value ? row["Invoice Amount"] : (object)DBNull.Value);
                            cmdDetail.Parameters.AddWithValue("@InvoiceNo", row["Invoice No"] ?? (object)DBNull.Value);
                            cmdDetail.Parameters.AddWithValue("@InvoiceDate", row["Invoice Date"] ?? (object)DBNull.Value);
                            cmdDetail.Parameters.Add("@Invoice", SqlDbType.VarBinary).Value = row["Invoice"] ?? (object)DBNull.Value;

                            cmdDetail.ExecuteNonQuery();

                            if (row == validRows.Rows[0])
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

                        Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Work";
                        UC_M_Work ug = new UC_M_Work(LoggedInUser, loggedInDepart, expensesType, loggedInIndex);
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
            UC_M_MiscellaneousClaim ug = new UC_M_MiscellaneousClaim(LoggedInUser, loggedInDepart, loggedInIndex);
            addControls(ug);
        }
    }
}