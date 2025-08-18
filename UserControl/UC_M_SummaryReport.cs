using ClosedXML.Excel;
using HRAdmin.Components;
using HRAdmin.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRAdmin.UserControl
{
    public partial class UC_M_SummaryReport : System.Windows.Forms.UserControl
    {
        private string LoggedInUser;
        private string loggedInDepart;
        private string loggedInIndex;
        private string loggedInName;
        private string LoggedInBank;
        private string LoggedInAccNo;
        private DataTable cachedData; // Declare cachedData
        private bool isNetworkErrorShown; // Declare isNetworkErrorShown
        private bool isNetworkUnavailable; // Declare isNetworkUnavailable
        private byte[] pdfBytes;
        private DataTable masterData; // Stores all loaded data
        private BindingSource bs = new BindingSource(); // For filtering

        public UC_M_SummaryReport()
        {
            InitializeComponent();

            string user = UserSession.LoggedInUser;
            string department = UserSession.loggedInDepart;
            string userlevel = UserSession.logginInUserAccessLevel;
            string userfullName = UserSession.loggedInName;
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
            Form_Home.sharedLabel.Text = "Account";
            UC_Acc_Account ug = new UC_Acc_Account(LoggedInUser, loggedInDepart, loggedInIndex, LoggedInBank, LoggedInAccNo);
            addControls(ug);
        }

        private void dtpStart_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
            GrandTotal();
        }

        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
            GrandTotal();
        }

        private void GrandTotal()
        {
            string query = @"
    SELECT SUM(TotalAmount) AS GrandTotal
    FROM (
        SELECT 
            A.SerialNo, 
            A.ExpensesType, 
            A.RequestDate,
            C.Email,
            SUM(B.InvoiceAmount) AS TotalAmount
        FROM tbl_MasterClaimForm A
        LEFT JOIN tbl_DetailClaimForm B ON A.SerialNo = B.SerialNo
        LEFT JOIN tbl_UserDetail C ON A.EmpNo = C.IndexNo
        WHERE A.RequestDate BETWEEN @StartDate AND @EndDate
        GROUP BY 
            A.SerialNo, 
            A.ExpensesType, 
            A.RequestDate,
            C.Email
    ) AS X;";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        DateTime startDate = dtpStart.Value.Date;
                        DateTime endDate = dtpEnd.Value.Date;

                        if (startDate == DateTime.MinValue || endDate == DateTime.MinValue)
                        {
                            DateTime today = DateTime.Today;
                            startDate = today.AddDays(-(int)today.DayOfWeek);
                            endDate = startDate.AddDays(7).AddTicks(-1);
                        }

                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@EndDate", endDate);

                        object result = cmd.ExecuteScalar();
                        decimal grandTotal = (result == DBNull.Value) ? 0 : Convert.ToDecimal(result);

                        lblGrandTotal.Text = "RM " + grandTotal.ToString("N2");
                    }

                    LoadExpenseTypes();
                }
                catch (Exception ex)
                {
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Error loading data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (cachedData != null)
                    {
                        BindDataGridView(cachedData);
                        MessageBox.Show("Network unavailable. Displaying cached data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Network unavailable and no cached data available.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void LoadExpenseTypes()
        {
            cmbECtype.SelectedIndexChanged -= cmbECtype_SelectedIndexChanged;

            var expenseTypes = masterData.AsEnumerable()
                .Select(r => r.Field<string>("ExpensesType"))
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            cmbECtype.Items.Clear();
            cmbECtype.Items.Add("-- All --");
            cmbECtype.Items.AddRange(expenseTypes.ToArray());
            cmbECtype.SelectedIndex = 0;

            cmbECtype.SelectedIndexChanged += cmbECtype_SelectedIndexChanged;
        }

        private void cmbECtype_SelectedIndexChanged(object sender, EventArgs e)
        {
            string ExpensesType = cmbECtype.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(ExpensesType))
            {
                if (ExpensesType == "-- All --")
                {
                    BindDataGridView(masterData);
                }
                else
                {
                    var filtered = masterData.AsEnumerable()
                        .Where(r => r.Field<string>("ExpensesType") == ExpensesType)
                        .CopyToDataTable();

                    BindDataGridView(filtered);
                }
            }
        }

        private void LoadData()
        {
            string query = @"
            SELECT 
                 A.SerialNo, 
                 A.ExpensesType, 
                 A.PaymentStatus,
                 A.RequestDate,
                 SUM(B.InvoiceAmount) AS TotalAmount
            FROM tbl_MasterClaimForm A
            LEFT JOIN tbl_DetailClaimForm B 
            ON A.SerialNo = B.SerialNo
            WHERE A.RequestDate BETWEEN @StartDate AND @EndDate
            GROUP BY 
                 A.SerialNo, 
                 A.ExpensesType, 
                 A.PaymentStatus,
                 A.RequestDate;";

            SqlConnection con = null;

            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString);
                con.Open();

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    DateTime startDate = dtpStart.Value.Date;
                    DateTime endDate = dtpEnd.Value.Date;

                    if (startDate != DateTime.MinValue && endDate != DateTime.MinValue)
                    {
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@EndDate", endDate);
                    }
                    else
                    {
                        DateTime today = DateTime.Today;
                        DateTime weekStart = today.AddDays(-(int)today.DayOfWeek);
                        DateTime weekEnd = weekStart.AddDays(7).AddTicks(-1);
                        cmd.Parameters.AddWithValue("@StartDate", weekStart);
                        cmd.Parameters.AddWithValue("@EndDate", weekEnd);
                    }

                    DataTable dt = new DataTable();
                    masterData = new DataTable();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(masterData);
                        da.Fill(dt);
                    }

                    cmbECtype.SelectedIndexChanged -= cmbECtype_SelectedIndexChanged;

                    var expenseTypes = masterData.AsEnumerable()
                        .Select(r => r.Field<string>("ExpensesType"))
                        .Where(d => !string.IsNullOrEmpty(d))
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();

                    cmbECtype.Items.Clear();
                    cmbECtype.Items.Add("-- All --");
                    cmbECtype.Items.AddRange(expenseTypes.ToArray());
                    cmbECtype.SelectedIndex = 0;

                    cmbECtype.SelectedIndexChanged += cmbECtype_SelectedIndexChanged;

                    cachedData = dt.Copy();
                    BindDataGridView(dt);
                }
            }
            catch (Exception ex)
            {
                if (!isNetworkErrorShown)
                {
                    isNetworkErrorShown = true;
                    MessageBox.Show("Error loading data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (cachedData != null)
                {
                    BindDataGridView(cachedData);
                    MessageBox.Show("Network unavailable. Displaying cached data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Network unavailable and no cached data available.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                if (con != null && con.State == ConnectionState.Open)
                {
                    con.Close();
                }
            }
        }

        private void BindDataGridView(DataTable dt)
        {
            dgvSR.AutoGenerateColumns = false;
            dgvSR.Columns.Clear();
            dgvSR.ReadOnly = true;
            dgvSR.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new System.Drawing.Font("Arial", 11, FontStyle.Bold),
            };

            int fixedColumnWidth = 200;

            dgvSR.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "SerialNo",
                HeaderText = "Serial No",
                DataPropertyName = "SerialNo",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvSR.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ExpensesType",
                HeaderText = "Expenses Type",
                DataPropertyName = "ExpensesType",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvSR.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "RequestDate",
                HeaderText = "Request Date",
                DataPropertyName = "RequestDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11),
                    Format = "dd.MM.yyyy"
                },
            });

            dgvSR.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "TotalAmount",
                HeaderText = "Total Amount",
                DataPropertyName = "TotalAmount",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvSR.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "PaymentStatus",
                HeaderText = "Payment Status",
                DataPropertyName = "PaymentStatus",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvSR.DataSource = dt;
            dgvSR.CellBorderStyle = DataGridViewCellBorderStyle.None;
            Debug.WriteLine("DataGridView updated successfully.");
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Get the current data from the DataGridView
            DataTable dt = (DataTable)dgvSR.DataSource;
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("No data available to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if all PaymentStatus are already "Paid"
            bool allPaid = dt.AsEnumerable().All(row => row.Field<string>("PaymentStatus") == "Paid");
            if (allPaid)
            {
                MessageBox.Show("All records are already marked as 'Paid'. No update is needed.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Show confirmation dialog only if some records are not "Paid"
            DialogResult result = MessageBox.Show(
                "Are you sure you want to update the Payment Status to 'Paid' for all records?",
                "Confirm Update",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return; // Exit if user selects "No"
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();

                    // Update PaymentStatus in the database
                    string updateQuery = @"
                        UPDATE tbl_MasterClaimForm
                        SET PaymentStatus = 'Paid'
                        WHERE SerialNo = @SerialNo";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.Add("@SerialNo", SqlDbType.VarChar);

                        foreach (DataRow row in dt.Rows)
                        {
                            cmd.Parameters["@SerialNo"].Value = row["SerialNo"];
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Update the DataTable and masterData
                    foreach (DataRow row in dt.Rows)
                    {
                        row["PaymentStatus"] = "Paid";
                    }

                    foreach (DataRow row in masterData.Rows)
                    {
                        if (dt.AsEnumerable().Any(r => r["SerialNo"].ToString() == row["SerialNo"].ToString()))
                        {
                            row["PaymentStatus"] = "Paid";
                        }
                    }

                    // Update cachedData if it exists
                    if (cachedData != null)
                    {
                        foreach (DataRow row in cachedData.Rows)
                        {
                            if (dt.AsEnumerable().Any(r => r["SerialNo"].ToString() == row["SerialNo"].ToString()))
                            {
                                row["PaymentStatus"] = "Paid";
                            }
                        }
                    }

                    // Refresh the DataGridView
                    BindDataGridView(dt);

                    MessageBox.Show("Payment status updated to 'Paid' for all records.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Error updating payment status: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    if (cachedData != null)
                    {
                        BindDataGridView(cachedData);
                        MessageBox.Show("Network unavailable. Displaying cached data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Network unavailable and no cached data available.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }
        private void btnExcel_Click(object sender, EventArgs e)
        {
            // Get the current filtered data from the DataGridView
            DataTable dt = (DataTable)dgvSR.DataSource;
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("No data available to export.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Create a new DataTable for Excel with the required columns
            DataTable excelData = new DataTable();
            //excelData.Columns.Add("Serial No", typeof(string));
            //excelData.Columns.Add("Requester", typeof(string));
            //excelData.Columns.Add("Expenses Type", typeof(string));
            //excelData.Columns.Add("Request Date", typeof(DateTime));
            //excelData.Columns.Add("Item", typeof(string));
            //excelData.Columns.Add("Total Amount", typeof(decimal));

            // Fetch requester and item details from the database based on SerialNo
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();

                    string query = @"
                SELECT 
                    A.SerialNo AS [Serial No], 
                    A.Requester AS [Requester],
                    A.ExpensesType AS [Expenses Type],
                    A.RequestDate AS [Request Date],
                    B.Item AS [GL A/C & Description],
                    SUM(B.InvoiceAmount) AS [Amount (RM)]
                FROM tbl_MasterClaimForm A
                LEFT JOIN tbl_DetailClaimForm B 
                    ON A.SerialNo = B.SerialNo
                LEFT JOIN tbl_UserDetail C 
                    ON A.EmpNo = C.IndexNo
                WHERE A.SerialNo IN ({0}) 
                GROUP BY 
                    A.SerialNo, 
                    A.Requester,
                    A.ExpensesType,
                    A.RequestDate,
                    B.Item;";

                    // Build the IN clause with SerialNo values from DataGridView
                    var serialNos = string.Join(",", dt.AsEnumerable().Select(r => $"'{r["SerialNo"]}'"));
                    query = string.Format(query, serialNos);

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(excelData);
                        }
                    }

                    // Create Excel file in memory and save to a temporary file
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("SummaryReport");
                        worksheet.Cell("A1").InsertTable(excelData);

                        // Auto-fit columns
                        worksheet.Columns().AdjustToContents();

                        // Save to a temporary file
                        string tempFilePath = Path.GetTempFileName() + ".xlsx";
                        workbook.SaveAs(tempFilePath);

                        // Open the temporary file in the default Excel application
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = tempFilePath,
                            UseShellExecute = true
                        });

                        // Optional: Delete the temporary file after opening (note: file may still be in use)
                        // This will be handled by the system when the file is closed by the user
                    }
                }
                catch (Exception ex)
                {
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Error generating Excel file: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}