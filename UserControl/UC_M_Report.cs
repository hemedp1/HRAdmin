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
        private string expensesType;
        private DateTime RequestDate;
        private byte[] pdfBytes;
        private DataTable cachedData;
        private bool isNetworkErrorShown;
        private bool isNetworkUnavailable;

        public UC_M_Report(string username, string department, DateTime? RequestDate)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
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

            UC_M_MiscellaneousClaim ug = new UC_M_MiscellaneousClaim(loggedInUser, loggedInDepart);
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
                SELECT SerialNo, Requester, Department, ExpensesType, RequestDate, HODApprovalStatus, ApprovedByHOD, HODApprovedDate, 
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
                SELECT SerialNo, Requester, Department, ExpensesType, RequestDate, HODApprovalStatus, ApprovedByHOD, HODApprovedDate, 
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

        private byte[] GeneratePDF(string serialNo, string selectedMeal)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 36f, 36f, 36f, 36f);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new PdfPageEventHelper();
                    document.Open();

                    iTextSharp.text.Font titleFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font addressFont = FontFactory.GetFont("Helvetica", 8f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font titleFont1 = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font headerFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font bodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font italicBodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.ITALIC, BaseColor.BLACK);

                    string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hosiden.jpg");
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
                    titlePara.Add(new Chunk("CANTEEN MEAL REQUEST FORM", titleFont));
                    titlePara.Alignment = Element.ALIGN_CENTER;
                    titlePara.SpacingBefore = 0f;
                    titlePara.SpacingAfter = 5f;
                    document.Add(titlePara);

                    string checkStatus = "";
                    string approveStatus = "";
                    string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "SELECT CheckStatus, ApproveStatus FROM tbl_InternalFoodOrder WHERE SerialNo = @SerialNo";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    checkStatus = reader["CheckStatus"] != DBNull.Value ? reader["CheckStatus"].ToString() : "";
                                    approveStatus = reader["ApproveStatus"] != DBNull.Value ? reader["ApproveStatus"].ToString() : "";
                                }
                            }
                        }
                    }

                    PdfPTable mainLayoutTable = new PdfPTable(2);
                    mainLayoutTable.WidthPercentage = 100;
                    mainLayoutTable.SetWidths(new float[] { 1.5f, 0.8f });
                    mainLayoutTable.SpacingBefore = 5f;

                    PdfPCell leftCell = new PdfPCell();
                    leftCell.Border = iTextRectangle.NO_BORDER;
                    leftCell.Padding = 0f;

                    PdfPTable detailsTable = new PdfPTable(4);
                    detailsTable.WidthPercentage = 100;
                    detailsTable.SetWidths(new float[] { 0.22f, 0.5f, 0.24f, 0.7f });
                    detailsTable.DefaultCell.Border = iTextRectangle.NO_BORDER;
                    detailsTable.DefaultCell.Padding = 2f;
                    detailsTable.SpacingBefore = 5f;

                    detailsTable.AddCell(new Phrase("SerialNo      :", bodyFont));
                    detailsTable.AddCell(new Phrase(serialNo, bodyFont));
                    detailsTable.AddCell(new Phrase("Emp No.:", bodyFont));
                    detailsTable.AddCell(new Phrase(RequestDate.ToString("dd.MM.yyyy"), bodyFont));

                    detailsTable.AddCell(new Phrase("Requester  :", bodyFont));
                    detailsTable.AddCell(new Phrase(loggedInUser, bodyFont));
                    detailsTable.AddCell(new Phrase("Department:", bodyFont));
                    detailsTable.AddCell(new Phrase(loggedInDepart, bodyFont));

                    detailsTable.AddCell(new Phrase("Bank Name:", bodyFont));
                    detailsTable.AddCell(new Phrase(loggedInDepart, bodyFont));
                    detailsTable.AddCell(new Phrase("Account No.:", bodyFont));
                    detailsTable.AddCell(new Phrase(loggedInDepart, bodyFont));

                    detailsTable.AddCell(new Phrase("Request date:", bodyFont));
                    detailsTable.AddCell(new Phrase(RequestDate.ToString("dd.MM.yyyy"), bodyFont));

                    leftCell.AddElement(detailsTable);

                    PdfPCell rightCell = new PdfPCell();
                    rightCell.Border = iTextRectangle.NO_BORDER;
                    rightCell.Padding = 0f;

                    Paragraph checkedPara = new Paragraph();
                    checkedPara.Add(new Chunk($"Checked by : {checkStatus}", bodyFont));
                    checkedPara.SpacingBefore = 0f;
                    rightCell.AddElement(checkedPara);

                    Paragraph checkedAdminPara = new Paragraph();
                    checkedAdminPara.Add(new Chunk($"", bodyFont));
                    checkedAdminPara.SpacingBefore = 0f;
                    checkedAdminPara.SpacingAfter = 0f;
                    rightCell.AddElement(checkedAdminPara);

                    Paragraph approvedPara = new Paragraph();
                    approvedPara.Add(new Chunk($"Approved by: {approveStatus}", bodyFont));
                    approvedPara.SpacingBefore = 0f;
                    rightCell.AddElement(approvedPara);

                    Paragraph approvedHrPara = new Paragraph();
                    approvedHrPara.Add(new Chunk($"", bodyFont));
                    approvedHrPara.SpacingBefore = 0f;
                    approvedHrPara.SpacingAfter = 0f;
                    rightCell.AddElement(approvedHrPara);

                    Paragraph issuedPara = new Paragraph();
                    issuedPara.Add(new Chunk($"Received by: Canteen", bodyFont));
                    issuedPara.SpacingBefore = 0f;
                    rightCell.AddElement(issuedPara);

                    mainLayoutTable.AddCell(leftCell);
                    mainLayoutTable.AddCell(rightCell);
                    document.Add(mainLayoutTable);

                    Paragraph detailsHeading = new Paragraph("Details of the claim:", bodyFont);
                    detailsHeading.SpacingBefore = 10f;
                    detailsHeading.SpacingAfter = 5f;
                    document.Add(detailsHeading);

                    PdfPTable detailsTable2 = new PdfPTable(2);
                    detailsTable2.WidthPercentage = 100;
                    detailsTable2.SetWidths(new float[] { 0.5f, 3f });

                    document.Add(detailsTable2);

                    Paragraph footer = new Paragraph("This is a computer generated document, no signature is required | Generated on: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"), bodyFont);
                    footer.Alignment = Element.ALIGN_LEFT;
                    footer.SpacingBefore = 20f;
                    footer.Font.Color = new BaseColor(100, 100, 100);
                    document.Add(footer);

                    document.Close();
                    pdfBytes = ms.ToArray();
                    return pdfBytes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void StorePdfInDatabase(string SerialNo, byte[] pdfBytes)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO tbl_MCPdfStorage (SerialNo, PdfData, CreatedDate) VALUES (@SerialNo, @PdfData, @CreatedDate)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SerialNo", SerialNo);
                        cmd.Parameters.AddWithValue("@PdfData", pdfBytes);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error storing PDF in database: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnViewPDF_Click(object sender, EventArgs e)
        {
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