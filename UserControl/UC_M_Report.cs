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

namespace HRAdmin.UserControl
{
    public partial class UC_M_Report : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private string loggedInIndex;
        private string expensesType;
        private DateTime RequestDate;
        private byte[] pdfBytes;
        private DataTable cachedData;
        private bool isNetworkErrorShown;
        private bool isNetworkUnavailable;

        public UC_M_Report(string username, string department, string emp, DateTime? RequestDate)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            loggedInIndex = emp;
            this.RequestDate = RequestDate ?? DateTime.Now;
            LoadUsernames();
            LoadDepartments();
            LoadData();
            cmbRequester.SelectedIndexChanged += cmbRequester_SelectedIndexChanged;
            cmbDepart.SelectedIndexChanged += cmbDepart_SelectedIndexChanged;
            cachedData = new DataTable();
            isNetworkErrorShown = false;
            isNetworkUnavailable = false;
            cmbType.SelectedIndex = -1;
            cmbRequester.SelectedIndex = -1;
            cmbDepart.SelectedIndex = -1;
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

        private void LoadData(string requester = null, string department = null, DateTime? requestDate = null, string expensesType = null)
        {
            if (dgvVR == null)
            {
                MessageBox.Show("DataGridView is not initialized!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query = @"
                SELECT SerialNo, Requester, EmpNo, Department, BankName, AccountNo, ExpensesType, RequestDate, HODApprovalStatus, ApprovedByHOD, HODApprovedDate, 
                       HRApprovalStatus, ApprovedByHR, HRApprovedDate, AccountApprovalStatus, ApprovedByAccount, AccountApprovedDate 
                FROM tbl_MasterClaimForm
                WHERE (@Requester IS NULL OR Requester = @Requester)
                      AND (@Department IS NULL OR Department = @Department)
                      AND (@ExpensesType IS NULL OR ExpensesType = @ExpensesType)";

            if (requestDate.HasValue)
            {
                query += " AND CAST(RequestDate AS DATE) = @RequestDate";
            }
            else
            {
                query += " AND RequestDate >= @WeekStart AND RequestDate < @WeekEnd";
            }

            query += " ORDER BY RequestDate ASC";

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@Requester", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(requester) ? (object)DBNull.Value : requester;
                        cmd.Parameters.Add("@Department", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(department) ? (object)DBNull.Value : department;
                        cmd.Parameters.Add("@ExpensesType", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(expensesType) ? (object)DBNull.Value : expensesType;

                        if (requestDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@RequestDate", requestDate.Value.Date);
                        }
                        else
                        {
                            DateTime today = DateTime.Today;
                            DateTime weekStart = today.AddDays(-(int)today.DayOfWeek);
                            DateTime weekEnd = weekStart.AddDays(7);
                            cmd.Parameters.AddWithValue("@WeekStart", weekStart);
                            cmd.Parameters.AddWithValue("@WeekEnd", weekEnd);
                        }

                        Debug.WriteLine($"Executing LoadData with Requester: {(string.IsNullOrEmpty(requester) ? "NULL" : requester)}, Department: {(string.IsNullOrEmpty(department) ? "NULL" : department)}, RequestDate: {(requestDate.HasValue ? requestDate.Value.ToString("yyyy-MM-dd") : "Weekly")}, ExpensesType: {(string.IsNullOrEmpty(expensesType) ? "NULL" : expensesType)}");

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        cachedData = dt.Copy();

                        Debug.WriteLine($"Rows retrieved: {dt.Rows.Count}");
                        foreach (DataRow row in dt.Rows)
                        {
                            Debug.WriteLine($"Row: SerialNo={row["SerialNo"]}, Requester={row["Requester"]}, Department={row["Department"]}, ExpensesType={row["ExpensesType"]}");
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
            DateTime? selectedRequestDate = dtpRequestDate.Checked ? dtpRequestDate.Value.Date : (DateTime?)null;

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
            LoadData(selectedUsername, selectedDepartment, selectedRequestDate, selectedExpensesType);
        }

        private void cmbDepart_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedExpensesType = cmbType.SelectedItem?.ToString();
            DateTime? selectedRequestDate = dtpRequestDate.Checked ? dtpRequestDate.Value.Date : (DateTime?)null;

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
            LoadData(selectedUsername, selectedDepartment, selectedRequestDate, selectedExpensesType);
        }

        private void UC_MCReport_Load(object sender, EventArgs e)
        {
            ApplyFilter();
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

            int fixedColumnWidth = 200;

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
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvVR.DataSource = dt;
            dgvVR.CellBorderStyle = DataGridViewCellBorderStyle.None;
            Debug.WriteLine("DataGridView updated successfully.");
        }

        private void dtpRequestDate_ValueChanged(object sender, EventArgs e)
        {
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedExpensesType = cmbType.SelectedItem?.ToString();

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

            LoadData(selectedUsername, selectedDepartment, dtpRequestDate.Value.Date, selectedExpensesType);
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedExpensesType = cmbType.SelectedItem?.ToString();
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            DateTime? selectedRequestDate = dtpRequestDate.Checked ? (DateTime?)dtpRequestDate.Value.Date : null;

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

            LoadData(selectedUsername, selectedDepartment, selectedRequestDate, selectedExpensesType);
        }

        private byte[] GeneratePDF(string serialNo)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                Dictionary<string, object> orderDetails = new Dictionary<string, object>();
                List<Dictionary<string, object>> claimItems = new List<Dictionary<string, object>>();

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
                                orderDetails["HODApprovedDate"] = reader["HODApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["HODApprovedDate"]).ToString("dd.MM.yyyy HH:mm") : "";
                                orderDetails["HRApprovalStatus"] = reader["HRApprovalStatus"] != DBNull.Value ? reader["HRApprovalStatus"].ToString() : "";
                                orderDetails["ApprovedByHR"] = reader["ApprovedByHR"] != DBNull.Value ? reader["ApprovedByHR"].ToString() : "";
                                orderDetails["HRApprovedDate"] = reader["HRApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["HRApprovedDate"]).ToString("dd.MM.yyyy HH:mm") : "";
                                orderDetails["AccountApprovalStatus"] = reader["AccountApprovalStatus"] != DBNull.Value ? reader["AccountApprovalStatus"].ToString() : "";
                                orderDetails["ApprovedByAccount"] = reader["ApprovedByAccount"] != DBNull.Value ? reader["ApprovedByAccount"].ToString() : "";
                                orderDetails["AccountApprovedDate"] = reader["AccountApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["AccountApprovedDate"]).ToString("dd.MM.yyyy HH:mm") : "";
                            }
                            else
                            {
                                MessageBox.Show("Order not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return null;
                            }
                        }
                    }

                    // Fetch claim items based on the reference image
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
                                item["Invoice"] = reader["Invoice"].ToString();
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

                    string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo hosiden.jpg");
                    if (File.Exists(logoPath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
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

                    // Adjust alignment and add left indentation
                    Paragraph HODApprovalPara = new Paragraph();
                    string ApprovedByHOD = orderDetails["ApprovedByHOD"].ToString();
                    string HODApprovedDate = orderDetails["HODApprovedDate"].ToString();
                    HODApprovalPara.IndentationLeft = -50f; // Adjust this value to move left (e.g., 10f moves it 10 points left)
                    if (string.IsNullOrEmpty(ApprovedByHOD))
                    {
                        HODApprovalPara.Add(new Chunk("HOD Approval      : Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(HODApprovedDate))
                        {
                            HODApprovedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
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

                    Paragraph HRApprovalPara = new Paragraph();
                    string ApprovedByHR = orderDetails["ApprovedByHR"].ToString();
                    string HRApprovedDate = orderDetails["HRApprovedDate"].ToString();
                    HRApprovalPara.IndentationLeft = -50f; // Adjust this value to move left
                    if (string.IsNullOrEmpty(ApprovedByHR))
                    {
                        HRApprovalPara.Add(new Chunk("HR Approval         : Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(HRApprovedDate))
                        {
                            HRApprovedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
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

                    Paragraph AccountApprovalPara = new Paragraph();
                    string ApprovedByAccount = orderDetails["ApprovedByAccount"].ToString();
                    string AccountApprovedDate = orderDetails["AccountApprovedDate"].ToString();
                    AccountApprovalPara.IndentationLeft = -50f; // Adjust this value to move left
                    if (string.IsNullOrEmpty(ApprovedByAccount))
                    {
                        AccountApprovalPara.Add(new Chunk("Account Approval : Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(AccountApprovedDate))
                        {
                            AccountApprovedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
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
                    detailsTable2.SetWidths(new float[] { 0.5f, 1.5f, 2f, 1f, 1.5f, 1.5f, 1f });
                    detailsTable2.DefaultCell.Padding = 5f;
                    detailsTable2.DefaultCell.Border = iTextSharp.text.Rectangle.BOX;

                    // Add header row
                    detailsTable2.AddCell(new Phrase("No", bodyFont));
                    detailsTable2.AddCell(new Phrase("Expenses Type", bodyFont));
                    detailsTable2.AddCell(new Phrase("Vendor", bodyFont));
                    detailsTable2.AddCell(new Phrase("Item ", bodyFont));
                    detailsTable2.AddCell(new Phrase("Invoice Amount", bodyFont));
                    detailsTable2.AddCell(new Phrase("Invoice No", bodyFont));
                    detailsTable2.AddCell(new Phrase("Invoice", bodyFont));

                    // Add data rows
                    decimal totalAmount = 0;
                    int itemNo = 1;
                    foreach (var item in claimItems)
                    {
                        detailsTable2.AddCell(new Phrase(itemNo++.ToString(), bodyFont));
                        detailsTable2.AddCell(new Phrase(item["ExpensesType"].ToString(), bodyFont));
                        detailsTable2.AddCell(new Phrase(item["Vendor"].ToString(), bodyFont));
                        detailsTable2.AddCell(new Phrase(item["Item"].ToString(), bodyFont));
                        string invoiceAmount = item["InvoiceAmount"].ToString();
                        detailsTable2.AddCell(new Phrase("RM " + invoiceAmount, bodyFont)); // Added RM prefix
                        detailsTable2.AddCell(new Phrase(item["InvoiceNo"].ToString(), bodyFont));
                        detailsTable2.AddCell(new Phrase(item["Invoice"].ToString(), bodyFont));
                        totalAmount += decimal.TryParse(invoiceAmount, out decimal amount) ? amount : 0;
                    }

                    // Add total row
                    if (claimItems.Count > 0)
                    {
                        detailsTable2.AddCell(new Phrase("", bodyFont));
                        detailsTable2.AddCell(new Phrase("", bodyFont));
                        detailsTable2.AddCell(new Phrase("", bodyFont));
                        detailsTable2.AddCell(new Phrase("Total Amount", bodyFont));
                        detailsTable2.AddCell(new Phrase("RM " + totalAmount.ToString("F2"), bodyFont)); // Added RM prefix
                        detailsTable2.AddCell(new Phrase("", bodyFont));
                        detailsTable2.AddCell(new Phrase("", bodyFont));
                    }

                    document.Add(detailsTable2);

                    // Add the note paragraph
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

                    Paragraph footer = new Paragraph("This is a computer generated document, no signature is required | Generated on: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + " \nClaim No. : " + orderDetails["SerialNo"].ToString(), bodyFont);
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
        private void AddStyledTableRow(PdfPTable table, string label, string value, iTextSharp.text.Font labelFont, iTextSharp.text.Font valueFont, int rowIndex, bool multiLine = false)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont));
            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont)) { MinimumHeight = 20f };

            labelCell.BackgroundColor = new BaseColor(255, 255, 255);
            valueCell.BackgroundColor = new BaseColor(255, 255, 255);

            labelCell.Phrase = new Phrase(label, new iTextSharp.text.Font(labelFont.BaseFont, labelFont.Size, labelFont.Style, BaseColor.BLACK));
            valueCell.Phrase = new Phrase(value, new iTextSharp.text.Font(valueFont.BaseFont, valueFont.Size, valueFont.Style, BaseColor.BLACK));

            labelCell.Padding = 8f;
            valueCell.Padding = 8f;
            labelCell.BorderColor = new BaseColor(150, 150, 150);
            valueCell.BorderColor = new BaseColor(150, 150, 150);
            labelCell.HorizontalAlignment = Element.ALIGN_LEFT;
            valueCell.HorizontalAlignment = Element.ALIGN_LEFT;

            if (multiLine)
            {
                valueCell.NoWrap = false;
            }

            table.AddCell(labelCell);
            table.AddCell(valueCell);
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