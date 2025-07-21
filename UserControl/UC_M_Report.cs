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
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using iTextRectangle = iTextSharp.text.Rectangle;
using System.Net.NetworkInformation;
using WinFormsApp = System.Windows.Forms.Application;

namespace HRAdmin.UserControl
{
    public partial class UC_M_Report : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private string loggedInIndex;
        private string LoggedInBank;
        private string LoggedInAccNo;
        private string expensesType;
        private byte[] pdfBytes;
        private DataTable cachedData;
        private bool isNetworkErrorShown;
        private bool isNetworkUnavailable;

        public UC_M_Report(string username, string department, string emp)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            loggedInIndex = emp;
            LoadUsernames();
            LoadDepartments();
            cmbRequester.SelectedIndexChanged += cmbRequester_SelectedIndexChanged;
            cmbDepart.SelectedIndexChanged += cmbDepart_SelectedIndexChanged;
            dtpStart.ValueChanged += dtpStart_ValueChanged;
            dtpEnd.ValueChanged += dtpEnd_ValueChanged;
            cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;
            cachedData = new DataTable();
            isNetworkErrorShown = false;
            isNetworkUnavailable = false;
            cmbType.SelectedIndex = -1;
            cmbRequester.SelectedIndex = -1;
            cmbDepart.SelectedIndex = -1;
            dgvVR.DataSource = null; // Ensure DataGridView starts empty
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

            UC_M_MiscellaneousClaim ug = new UC_M_MiscellaneousClaim(loggedInUser, loggedInDepart, loggedInIndex, LoggedInBank, LoggedInAccNo);
            addControls(ug);
        }

        private void LoadUsernames()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = "SELECT Username FROM tbl_Users";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            cmbRequester.Items.Clear();
                            while (reader.Read())
                            {
                                string username = reader["Username"].ToString();
                                cmbRequester.Items.Add(username);
                                Debug.WriteLine($"Loaded username: {username}");
                            }
                        }
                    }
                    cmbRequester.SelectedIndex = -1;
                    Debug.WriteLine("Usernames loaded successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading usernames: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error loading usernames: {ex.Message}");
                }
            }
        }

        private void LoadUsernamesByDepartment(string department)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = "SELECT Username FROM tbl_Users WHERE Department = @Department";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Department", department);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            cmbRequester.Items.Clear();
                            while (reader.Read())
                            {
                                string username = reader["Username"].ToString();
                                cmbRequester.Items.Add(username);
                                Debug.WriteLine($"Loaded username: {username} for department: {department}");
                            }
                        }
                    }
                    cmbRequester.SelectedIndex = -1;
                    Debug.WriteLine($"Usernames loaded successfully for department: {department}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading usernames: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error loading usernames for department {department}: {ex.Message}");
                }
            }
        }

        private void LoadDepartments()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = "SELECT DISTINCT Department FROM tbl_Users WHERE Department IS NOT NULL";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            cmbDepart.Items.Clear();
                            while (reader.Read())
                            {
                                string department = reader["Department"].ToString();
                                cmbDepart.Items.Add(department);
                                Debug.WriteLine($"Loaded department: {department}");
                            }
                        }
                    }
                    cmbDepart.SelectedIndex = -1;
                    Debug.WriteLine("Departments loaded successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading departments: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error loading departments: {ex.Message}");
                }
            }
        }

        private void LoadData(string requester = null, string department = null, string expensesType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (dgvVR == null)
            {
                MessageBox.Show("DataGridView is not initialized!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query = @"
        SELECT SerialNo, Requester, EmpNo, Department, BankName, AccountNo, ExpensesType, RequestDate, 
               HODApprovalStatus, ApprovedByHOD, HODApprovedDate, HRApprovalStatus, ApprovedByHR, 
               HRApprovedDate, AccountApprovalStatus, ApprovedByAccount, AccountApprovedDate 
        FROM tbl_MasterClaimForm
        WHERE 1=1";

            // Restrict regular requesters to their own orders
            if (loggedInDepart != "HR & ADMIN" && loggedInDepart != "ACCOUNT")
            {
                query += " AND Requester = @LoggedInUser";
            }

            // Apply additional filters if provided
            query += @"
        AND (@Requester IS NULL OR Requester = @Requester)
        AND (@Department IS NULL OR Department = @Department)
        AND (@ExpensesType IS NULL OR ExpensesType = @ExpensesType)";

            if (startDate.HasValue && endDate.HasValue)
            {
                query += " AND RequestDate >= @StartDate AND RequestDate <= @EndDate";
            }

            query += " ORDER BY RequestDate ASC";

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Always add the logged-in user parameter for regular requesters
                        if (loggedInDepart != "HR & ADMIN" && loggedInDepart != "ACCOUNT")
                        {
                            cmd.Parameters.AddWithValue("@LoggedInUser", loggedInUser);
                        }

                        cmd.Parameters.Add("@Requester", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(requester) ? (object)DBNull.Value : requester;
                        cmd.Parameters.Add("@Department", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(department) ? (object)DBNull.Value : department;
                        cmd.Parameters.Add("@ExpensesType", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(expensesType) ? (object)DBNull.Value : expensesType;

                        if (startDate.HasValue && endDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
                            cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date.AddDays(1).AddTicks(-1)); // Include entire end date
                        }

                        Debug.WriteLine($"Executing LoadData with LoggedInUser: {loggedInUser}, Requester: {(string.IsNullOrEmpty(requester) ? "NULL" : requester)}, Department: {(string.IsNullOrEmpty(department) ? "NULL" : department)}, ExpensesType: {(string.IsNullOrEmpty(expensesType) ? "NULL" : expensesType)}, StartDate: {(startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "NULL")}, EndDate: {(endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "NULL")}");

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        cachedData = dt.Copy();

                        Debug.WriteLine($"Rows retrieved: {dt.Rows.Count}");
                        foreach (DataRow row in dt.Rows)
                        {
                            Debug.WriteLine($"Row: SerialNo={row["SerialNo"]}, Requester={row["Requester"]}, Department={row["Department"]}, ExpensesType={row["ExpensesType"]}, RequestDate={row["RequestDate"]}");
                        }

                        BindDataGridView(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void cmbRequester_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedExpensesType = cmbType.SelectedItem?.ToString();
            DateTime? startDate = dtpStart.Checked ? dtpStart.Value.Date : (DateTime?)null;
            DateTime? endDate = dtpEnd.Checked ? dtpEnd.Value.Date : (DateTime?)null;

            Debug.WriteLine($"cmbRequester selected: {selectedUsername}");
            if (string.IsNullOrEmpty(selectedUsername))
            {
                selectedUsername = null;
                Debug.WriteLine("Loading data with no Requester filter.");
            }
            if (string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
            }
            if (string.IsNullOrEmpty(selectedExpensesType))
            {
                selectedExpensesType = null;
            }
            LoadData(selectedUsername, selectedDepartment, selectedExpensesType, startDate, endDate);
        }

        private void cmbDepart_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedExpensesType = cmbType.SelectedItem?.ToString();
            DateTime? startDate = dtpStart.Checked ? dtpStart.Value.Date : (DateTime?)null;
            DateTime? endDate = dtpEnd.Checked ? dtpEnd.Value.Date : (DateTime?)null;

            Debug.WriteLine($"cmbDepart selected: {selectedDepartment}");

            if (string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
                LoadUsernames();
                Debug.WriteLine("Loading all usernames for no department selected.");
            }
            else
            {
                LoadUsernamesByDepartment(selectedDepartment);
                Debug.WriteLine($"Loading usernames for department: {selectedDepartment}");
            }

            cmbRequester.SelectedIndex = -1;

            if (string.IsNullOrEmpty(selectedUsername))
            {
                selectedUsername = null;
            }
            if (string.IsNullOrEmpty(selectedExpensesType))
            {
                selectedExpensesType = null;
            }
            LoadData(selectedUsername, selectedDepartment, selectedExpensesType, startDate, endDate);
        }

        private void dtpStart_ValueChanged(object sender, EventArgs e)
        {
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedExpensesType = cmbType.SelectedItem?.ToString();
            DateTime? startDate = dtpStart.Checked ? dtpStart.Value.Date : (DateTime?)null;
            DateTime? endDate = dtpEnd.Checked ? dtpEnd.Value.Date : (DateTime?)null;

            if (string.IsNullOrEmpty(selectedUsername))
            {
                selectedUsername = null;
            }
            if (string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
            }
            if (string.IsNullOrEmpty(selectedExpensesType))
            {
                selectedExpensesType = null;
            }

            LoadData(selectedUsername, selectedDepartment, selectedExpensesType, startDate, endDate);
        }

        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedExpensesType = cmbType.SelectedItem?.ToString();
            DateTime? startDate = dtpStart.Checked ? dtpStart.Value.Date : (DateTime?)null;
            DateTime? endDate = dtpEnd.Checked ? dtpEnd.Value.Date : (DateTime?)null;

            if (string.IsNullOrEmpty(selectedUsername))
            {
                selectedUsername = null;
            }
            if (string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
            }
            if (string.IsNullOrEmpty(selectedExpensesType))
            {
                selectedExpensesType = null;
            }

            LoadData(selectedUsername, selectedDepartment, selectedExpensesType, startDate, endDate);
        }

        private void UC_MCReport_Load(object sender, EventArgs e)
        {
            // Removed ApplyFilter() to prevent initial data load
            dgvVR.DataSource = null; // Ensure DataGridView remains empty on load
        }

        private bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        private void ApplyFilter()
        {
            if (dgvVR == null)
            {
                MessageBox.Show("DataGridView is not initialized!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsNetworkAvailable())
            {
                if (!isNetworkUnavailable)
                {
                    isNetworkUnavailable = true;
                    MessageBox.Show("Network disconnected.",
                                    "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (cachedData != null)
                {
                    BindDataGridView(cachedData);
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Network unavailable. Displaying cached data.",
                                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Network unavailable and no cached data available.",
                                        "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                return;
            }

            string query = @"
                SELECT SerialNo, Requester, EmpNo, Department, BankName, AccountNo, ExpensesType, RequestDate, HODApprovalStatus, ApprovedByHOD, HODApprovedDate, 
                       HRApprovalStatus, ApprovedByHR, HRApprovedDate, AccountApprovalStatus, ApprovedByAccount, AccountApprovedDate 
                FROM tbl_MasterClaimForm
                WHERE RequestDate >= @WeekStart AND RequestDate < @WeekEnd
                ORDER BY RequestDate ASC";

            try
            {
                var connString = ConfigurationManager.ConnectionStrings["ConnString"];
                if (connString == null)
                {
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Connection string 'ConnString' not found in configuration!",
                                        "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }

                using (SqlConnection con = new SqlConnection(connString.ConnectionString))
                {
                    if (con == null)
                    {
                        if (!isNetworkErrorShown)
                        {
                            isNetworkErrorShown = true;
                            MessageBox.Show("Failed to create SqlConnection object!",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        return;
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        DateTime today = DateTime.Today;
                        DateTime weekStart = today.AddDays(-(int)today.DayOfWeek);
                        DateTime weekEnd = weekStart.AddDays(7);
                        cmd.Parameters.AddWithValue("@WeekStart", weekStart);
                        cmd.Parameters.AddWithValue("@WeekEnd", weekEnd);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        cachedData = dt.Copy();

                        BindDataGridView(dt);

                        isNetworkErrorShown = false;
                        isNetworkUnavailable = false;
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == -1 || ex.Number == 26)
                {
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Unable to connect to the database. Please check your network connection.",
                                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Database error: " + ex.Message,
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (NullReferenceException ex)
            {
                if (!isNetworkErrorShown)
                {
                    isNetworkErrorShown = true;
                    MessageBox.Show("Null reference error: " + ex.Message + "\nStack Trace: " + ex.StackTrace,
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                if (!isNetworkErrorShown)
                {
                    isNetworkErrorShown = true;
                    MessageBox.Show("Error loading data: " + ex.Message,
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BindDataGridView(DataTable dt)
        {
            dgvVR.AutoGenerateColumns = false;
            dgvVR.Columns.Clear();

            dgvVR.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new System.Drawing.Font("Arial", 11, FontStyle.Bold),
            };

            int fixedColumnWidth = 150;

            dgvVR.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvVR.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Requester",
                HeaderText = "Requester",
                DataPropertyName = "Requester",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvVR.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Department",
                HeaderText = "Section",
                DataPropertyName = "Department",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvVR.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvVR.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvVR.DataSource = dt;
            dgvVR.CellBorderStyle = DataGridViewCellBorderStyle.None;
            Debug.WriteLine("DataGridView updated successfully.");
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedExpensesType = cmbType.SelectedItem?.ToString();
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            DateTime? startDate = dtpStart.Checked ? dtpStart.Value.Date : (DateTime?)null;
            DateTime? endDate = dtpEnd.Checked ? dtpEnd.Value.Date : (DateTime?)null;

            Debug.WriteLine($"cmbType selected: {selectedExpensesType}");

            if (string.IsNullOrEmpty(selectedUsername))
            {
                selectedUsername = null;
            }
            if (string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
            }
            if (string.IsNullOrEmpty(selectedExpensesType))
            {
                selectedExpensesType = null;
            }

            LoadData(selectedUsername, selectedDepartment, selectedExpensesType, startDate, endDate);
        }

        private byte[] GeneratePDF(string serialNo)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                Dictionary<string, object> orderDetails = new Dictionary<string, object>();
                List<Dictionary<string, object>> claimItems = new List<Dictionary<string, object>>();
                Dictionary<string, string> tempFiles = new Dictionary<string, string>(); // To store temporary file paths

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                    SELECT SerialNo, Requester, EmpNo, Department, BankName, AccountNo, ExpensesType, RequestDate, HODApprovalStatus, ApprovedByHOD, HODApprovedDate, 
                           HRApprovalStatus, ApprovedByHR, HRApprovedDate, AccountApprovalStatus, ApprovedByAccount, AccountApprovedDate
                    FROM tbl_MasterClaimForm
                    WHERE SerialNo = @SerialNo";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SerialNo", serialNo);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orderDetails["SerialNo"] = reader["SerialNo"].ToString();
                                orderDetails["Requester"] = reader["Requester"].ToString();
                                orderDetails["EmpNo"] = reader["EmpNo"].ToString();
                                orderDetails["Department"] = reader["Department"].ToString();
                                orderDetails["BankName"] = reader["BankName"].ToString();
                                orderDetails["AccountNo"] = reader["AccountNo"].ToString();
                                orderDetails["ExpensesType"] = reader["ExpensesType"].ToString();
                                orderDetails["RequestDate"] = reader["RequestDate"];
                                orderDetails["HODApprovalStatus"] = reader["HODApprovalStatus"] != DBNull.Value ? reader["HODApprovalStatus"].ToString() : "";
                                orderDetails["ApprovedByHOD"] = reader["ApprovedByHOD"] != DBNull.Value ? reader["ApprovedByHOD"].ToString() : "";
                                orderDetails["HODApprovedDate"] = reader["HODApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["HODApprovedDate"]).ToString("dd.MM.yyyy") : "";
                                orderDetails["HRApprovalStatus"] = reader["HRApprovalStatus"] != DBNull.Value ? reader["HRApprovalStatus"].ToString() : "";
                                orderDetails["ApprovedByHR"] = reader["ApprovedByHR"] != DBNull.Value ? reader["ApprovedByHR"].ToString() : "";
                                orderDetails["HRApprovedDate"] = reader["HRApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["HRApprovedDate"]).ToString("dd.MM.yyyy") : "";
                                orderDetails["AccountApprovalStatus"] = reader["AccountApprovalStatus"] != DBNull.Value ? reader["AccountApprovalStatus"].ToString() : "";
                                orderDetails["ApprovedByAccount"] = reader["ApprovedByAccount"] != DBNull.Value ? reader["ApprovedByAccount"].ToString() : "";
                                orderDetails["AccountApprovedDate"] = reader["AccountApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["AccountApprovedDate"]).ToString("dd.MM.yyyy") : "";
                            }
                            else
                            {
                                MessageBox.Show("Order not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return null;
                            }
                        }
                    }

                    string itemsQuery = @"
                    SELECT SerialNo, ExpensesType, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice
                    FROM tbl_DetailClaimForm
                    WHERE SerialNo = @SerialNo";
                    using (SqlCommand cmd = new SqlCommand(itemsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@SerialNo", serialNo);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new Dictionary<string, object>();
                                item["ExpensesType"] = reader["ExpensesType"].ToString();
                                item["Vendor"] = reader["Vendor"].ToString();
                                item["Item"] = reader["Item"].ToString();
                                item["InvoiceAmount"] = reader["InvoiceAmount"] != DBNull.Value ? reader["InvoiceAmount"].ToString() : "0.00";
                                item["InvoiceNo"] = reader["InvoiceNo"].ToString();

                                // Handle binary data
                                if (reader["Invoice"] != DBNull.Value)
                                {
                                    byte[] invoiceBinary = (byte[])reader["Invoice"];
                                    string tempFile = Path.GetTempFileName() + ".pdf";
                                    File.WriteAllBytes(tempFile, invoiceBinary);
                                    item["Invoice"] = tempFile;
                                    tempFiles[Path.GetFileName(tempFile)] = tempFile; // Store for potential cleanup
                                }
                                else
                                {
                                    item["Invoice"] = null;
                                }
                                claimItems.Add(item);
                            }
                        }
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 36f, 36f, 36f, 36f);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new PdfPageEventHelper();
                    document.Open();

                    iTextSharp.text.Font titleFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font addressFont = FontFactory.GetFont("Helvetica", 8f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font headerFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font bodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font italicBodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.ITALIC, BaseColor.BLACK);
                    iTextSharp.text.Font boldBodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font linkFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.UNDERLINE, BaseColor.BLUE);

                    string imagePath = Path.Combine(WinFormsApp.StartupPath, "Img", "hosiden.jpg");
                    if (File.Exists(imagePath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imagePath);
                        logo.ScaleToFit(100f, 100f);
                        logo.Alignment = Element.ALIGN_CENTER;
                        logo.SpacingAfter = 0f;
                        document.Add(logo);
                    }
                    else
                    {
                        Paragraph companyPara = new Paragraph("Hosiden Electronics (M) Sdn Bhd", headerFont);
                        companyPara.Alignment = Element.ALIGN_CENTER;
                        companyPara.SpacingAfter = 0f;
                        document.Add(companyPara);
                    }

                    Paragraph titlePara = new Paragraph();
                    titlePara.Add(new Chunk("HOSIDEN ELECTRONICS (M) SDN BHD (198901000700)\n", titleFont));
                    titlePara.Add(new Chunk("Lot 1, Jalan P/1A, Bangi Industrial Estate, 43650 Bandar Baru Bangi, Selangor, Malaysia\n", addressFont));
                    titlePara.Add(new Chunk("\n", addressFont));
                    titlePara.Add(new Chunk("MISCELLANEOUS CLAIM FORM", titleFont));
                    titlePara.Alignment = Element.ALIGN_CENTER;
                    titlePara.SpacingBefore = 0f;
                    titlePara.SpacingAfter = 5f;
                    document.Add(titlePara);

                    PdfPTable mainLayoutTable = new PdfPTable(2);
                    mainLayoutTable.WidthPercentage = 100;
                    mainLayoutTable.SetWidths(new float[] { 1.5f, 0.8f });
                    mainLayoutTable.SpacingBefore = 10f;

                    PdfPCell leftCell = new PdfPCell();
                    leftCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    leftCell.Padding = 0f;

                    PdfPTable detailsTable = new PdfPTable(2);
                    detailsTable.WidthPercentage = 100;
                    detailsTable.SetWidths(new float[] { 0.2f, 0.8f });
                    detailsTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    detailsTable.DefaultCell.Padding = 2f;
                    detailsTable.SpacingBefore = 5f;

                    detailsTable.AddCell(new Phrase("Requester     :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["Requester"].ToString(), bodyFont));

                    detailsTable.AddCell(new Phrase("Emp No.       :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["EmpNo"].ToString(), bodyFont));

                    detailsTable.AddCell(new Phrase("Department  :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["Department"].ToString(), bodyFont));

                    detailsTable.AddCell(new Phrase("Bank name   :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["BankName"].ToString(), bodyFont));

                    detailsTable.AddCell(new Phrase("Account No.  :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["AccountNo"].ToString(), bodyFont));

                    detailsTable.AddCell(new Phrase("Request date:", bodyFont));
                    detailsTable.AddCell(new Phrase(Convert.ToDateTime(orderDetails["RequestDate"]).ToString("dd.MM.yyyy"), bodyFont));

                    leftCell.AddElement(detailsTable);

                    PdfPCell rightCell = new PdfPCell();
                    rightCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    rightCell.Padding = 0f;

                    Paragraph HODApprovalPara = new Paragraph();
                    string ApprovedByHOD = orderDetails["ApprovedByHOD"].ToString();
                    string HODApprovedDate = orderDetails["HODApprovedDate"].ToString();
                    HODApprovalPara.IndentationLeft = -50f;
                    if (string.IsNullOrEmpty(ApprovedByHOD))
                    {
                        HODApprovalPara.Add(new Chunk("HOD Approval      : Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(HODApprovedDate))
                        {
                            HODApprovedDate = DateTime.Now.ToString("dd.MM.yyyy");
                        }
                        HODApprovalPara.Add(new Chunk($"Approved by HOD      : {ApprovedByHOD}   {HODApprovedDate}", bodyFont));
                    }
                    HODApprovalPara.SpacingBefore = 0f;
                    rightCell.AddElement(HODApprovalPara);

                    Paragraph approvedHODPara = new Paragraph();
                    approvedHODPara.Add(new Chunk("", bodyFont));
                    approvedHODPara.SpacingBefore = 0f;
                    approvedHODPara.SpacingAfter = 0f;
                    rightCell.AddElement(approvedHODPara);

                    // Remove HR Approval section if ExpensesType is "work"
                    if (orderDetails["ExpensesType"].ToString().ToLower() != "work")
                    {
                        Paragraph HRApprovalPara = new Paragraph();
                        string ApprovedByHR = orderDetails["ApprovedByHR"].ToString();
                        string HRApprovedDate = orderDetails["HRApprovedDate"].ToString();
                        HRApprovalPara.IndentationLeft = -50f;
                        if (string.IsNullOrEmpty(ApprovedByHR))
                        {
                            HRApprovalPara.Add(new Chunk("HR Approval         : Pending", bodyFont));
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(HRApprovedDate))
                            {
                                HRApprovedDate = DateTime.Now.ToString("dd.MM.yyyy");
                            }
                            HRApprovalPara.Add(new Chunk($"Approved by HR         : {ApprovedByHR}   {HRApprovedDate}", bodyFont));
                        }
                        HRApprovalPara.SpacingBefore = 0f;
                        rightCell.AddElement(HRApprovalPara);

                        Paragraph approvedHRPara = new Paragraph();
                        approvedHRPara.Add(new Chunk("", bodyFont));
                        approvedHRPara.SpacingBefore = 0f;
                        approvedHRPara.SpacingAfter = 0f;
                        rightCell.AddElement(approvedHRPara);
                    }

                    Paragraph AccountApprovalPara = new Paragraph();
                    string ApprovedByAccount = orderDetails["ApprovedByAccount"].ToString();
                    string AccountApprovedDate = orderDetails["AccountApprovedDate"].ToString();
                    AccountApprovalPara.IndentationLeft = -50f;
                    if (string.IsNullOrEmpty(ApprovedByAccount))
                    {
                        AccountApprovalPara.Add(new Chunk("Account Approval : Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(AccountApprovedDate))
                        {
                            AccountApprovedDate = DateTime.Now.ToString("dd.MM.yyyy");
                        }
                        AccountApprovalPara.Add(new Chunk($"Approved by Account : {ApprovedByAccount}   {AccountApprovedDate}", bodyFont));
                    }
                    AccountApprovalPara.SpacingBefore = 0f;
                    rightCell.AddElement(AccountApprovalPara);

                    Paragraph approvedAccountPara = new Paragraph();
                    approvedAccountPara.Add(new Chunk("", bodyFont));
                    approvedAccountPara.SpacingBefore = 0f;
                    approvedAccountPara.SpacingAfter = 0f;
                    rightCell.AddElement(approvedAccountPara);

                    mainLayoutTable.AddCell(leftCell);
                    mainLayoutTable.AddCell(rightCell);
                    document.Add(mainLayoutTable);

                    Paragraph detailsHeading = new Paragraph("Details of the claim:", bodyFont);
                    detailsHeading.SpacingBefore = 10f;
                    detailsHeading.SpacingAfter = 5f;
                    document.Add(detailsHeading);

                    PdfPTable detailsTable2 = new PdfPTable(7);
                    detailsTable2.WidthPercentage = 100;
                    detailsTable2.SetWidths(new float[] { 0.5f, 1.5f, 1.5f, 2f, 1.5f, 1.5f, 1f });
                    detailsTable2.DefaultCell.Padding = 5f;
                    detailsTable2.DefaultCell.Border = iTextSharp.text.Rectangle.BOX;

                    detailsTable2.AddCell(new Phrase("No", bodyFont));
                    detailsTable2.AddCell(new Phrase("Expenses Type", bodyFont));
                    detailsTable2.AddCell(new Phrase("Vendor", bodyFont));
                    detailsTable2.AddCell(new Phrase("Item ", bodyFont));
                    detailsTable2.AddCell(new Phrase("Invoice Amount", bodyFont));
                    detailsTable2.AddCell(new Phrase("Invoice No", bodyFont));
                    detailsTable2.AddCell(new Phrase("Invoice", bodyFont));

                    decimal totalAmount = 0;
                    int itemNo = 1;
                    foreach (var item in claimItems)
                    {
                        detailsTable2.AddCell(new Phrase(itemNo++.ToString(), bodyFont));
                        detailsTable2.AddCell(new Phrase(item["ExpensesType"].ToString(), bodyFont));
                        detailsTable2.AddCell(new Phrase(item["Vendor"].ToString(), bodyFont));
                        detailsTable2.AddCell(new Phrase(item["Item"].ToString(), bodyFont));
                        string invoiceAmount = item["InvoiceAmount"].ToString();
                        detailsTable2.AddCell(new Phrase("RM " + invoiceAmount, bodyFont));
                        detailsTable2.AddCell(new Phrase(item["InvoiceNo"].ToString(), bodyFont));

                        // Add hyperlink to the Invoice column using temporary file
                        PdfPCell invoiceCell = new PdfPCell();
                        invoiceCell.Border = iTextSharp.text.Rectangle.BOX;
                        //invoiceCell.Padding = 10f;
                        string invoicePath = item["Invoice"] as string;
                        if (!string.IsNullOrEmpty(invoicePath) && File.Exists(invoicePath))
                        {
                            Phrase linkPhrase = new Phrase();
                            Anchor invoiceLink = new Anchor("View", linkFont);
                            invoiceLink.Reference = $"file:///{invoicePath.Replace("\\", "/")}";
                            linkPhrase.Add(invoiceLink);
                            invoiceCell.AddElement(linkPhrase);
                        }
                        else
                        {
                            invoiceCell.AddElement(new Phrase("No Invoice", bodyFont));
                        }
                        invoiceCell.HorizontalAlignment = Element.ALIGN_LEFT; // Align text to the left
                        invoiceCell.VerticalAlignment = Element.ALIGN_TOP;   // Align text to the top
                        detailsTable2.AddCell(invoiceCell);

                        totalAmount += decimal.TryParse(invoiceAmount, out decimal amount) ? amount : 0;
                    }

                    if (claimItems.Count > 0)
                    {
                        detailsTable2.AddCell(new Phrase("", bodyFont));
                        detailsTable2.AddCell(new Phrase("", bodyFont));
                        detailsTable2.AddCell(new Phrase("", bodyFont));
                        detailsTable2.AddCell(new Phrase("Total Amount", bodyFont));
                        detailsTable2.AddCell(new Phrase("RM " + totalAmount.ToString("F2"), bodyFont));
                        detailsTable2.AddCell(new Phrase("", bodyFont));
                        detailsTable2.AddCell(new Phrase("", bodyFont));
                    }

                    document.Add(detailsTable2);

                    Paragraph notePara = new Paragraph("", bodyFont);
                    notePara.Add(new Chunk("Note:\n", boldBodyFont));
                    notePara.Add(new Chunk("1. Claim of 'Miscellaneous Item' refers to claim for:\n", bodyFont));
                    notePara.Add(new Chunk("                     i. Expenses related to own department work\n", bodyFont));
                    notePara.Add(new Chunk("                     Ex: Purchase of work-related supplies/tools, that are not available in General Affairs stock\n", bodyFont));
                    notePara.Add(new Chunk("                     ii. Expenses related to employee benefits/company event, medical expenses, auditor's meal, etc.\n", bodyFont));
                    notePara.Add(new Chunk("                     Ex: Expenses incurred during business trip/external training, medical expenses, auditor's meal, etc.\n", bodyFont));
                    notePara.Add(new Chunk("2. Every claim must be attached with official invoice.\n", bodyFont));
                    notePara.Add(new Chunk("3. ", boldBodyFont));
                    notePara.Add(new Chunk("Every claim must be approved by respective department head\n", boldBodyFont));
                    notePara.Add(new Chunk("   ", boldBodyFont));
                    notePara.Add(new Chunk("                  For expenses related to employee benefits/company event, approval from HR & Administration ", boldBodyFont));
                    notePara.Add(new Chunk("                           must be obtained.\n", boldBodyFont));
                    notePara.Add(new Chunk("4. Payment will be made by Account section on 15th and 30th of the month.\n", bodyFont));
                    notePara.Add(new Chunk("5. Procedure :-\n", bodyFont));
                    notePara.Add(new Chunk("                     Requisitor > Department Head > HR & Administration > Accounts > Requisitor\n", bodyFont));
                    notePara.Add(new Chunk("                     (employee benefits/company event)\n", bodyFont));
                    notePara.SpacingBefore = 10f;
                    notePara.SpacingAfter = 10f;
                    document.Add(notePara);

                    Paragraph footer = new Paragraph("This is a computer generated document, no signature is required | Generated on: " + DateTime.Now.ToString("dd.MM.yyyy") + " \nClaim No. : " + orderDetails["SerialNo"].ToString(), bodyFont);
                    footer.Alignment = Element.ALIGN_LEFT;
                    footer.SpacingBefore = 20f;
                    footer.Font.Color = new BaseColor(100, 100, 100);
                    document.Add(footer);

                    document.Close();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void btnViewPDF_Click(object sender, EventArgs e)
        {
            Debug.WriteLine($"SelectedRows.Count: {dgvVR.SelectedRows.Count}");
            Debug.WriteLine($"CurrentRow.Index: {dgvVR.CurrentRow?.Index}");
            DataGridViewRow selectedRow = dgvVR.SelectedRows.Count > 0 ? dgvVR.SelectedRows[0] : dgvVR.CurrentRow;
            if (selectedRow != null)
            {
                string serialNo = selectedRow.Cells["SerialNo"].Value?.ToString();
                Debug.WriteLine($"Selected SerialNo: {serialNo}");
                if (string.IsNullOrEmpty(serialNo))
                {
                    MessageBox.Show("Invalid row selection: SerialNo is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string selectedMeal = cmbType.SelectedItem?.ToString() ?? "DefaultMeal";
                pdfBytes = GeneratePDF(serialNo);
                if (pdfBytes != null)
                {
                    string tempFile = Path.GetTempFileName() + ".pdf";
                    File.WriteAllBytes(tempFile, pdfBytes);
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempFile,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("No PDF data available to view.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please select a row to view the PDF.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public class PdfPageEventHelper : iTextSharp.text.pdf.PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                PdfPTable footerTbl = new PdfPTable(1);
                footerTbl.TotalWidth = document.PageSize.Width - 72;
                PdfPCell cell = new PdfPCell(new Phrase($"Page {writer.PageNumber}", FontFactory.GetFont("Helvetica", 8, BaseColor.GRAY)));
                cell.Border = iTextRectangle.NO_BORDER;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                footerTbl.AddCell(cell);
                footerTbl.WriteSelectedRows(0, -1, 36, 20, writer.DirectContent);
            }
        }
    }
}