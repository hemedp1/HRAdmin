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
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.NetworkInformation;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using iTextRectangle = iTextSharp.text.Rectangle;
using WinFormsApp = System.Windows.Forms.Application;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Reflection;
using System.Net.Mail;
using System.Net;
using HRAdmin.Components;
using static iTextSharp.text.pdf.PdfDocument;
using System.Threading;
using System.Drawing.Drawing2D;
using Org.BouncyCastle.Asn1.Ocsp;
using Rectangle = System.Drawing.Rectangle;


namespace HRAdmin.UserControl
{
    public partial class UC_M_MiscellaneousClaim : System.Windows.Forms.UserControl
    {
        private string LoggedInUser;
        private string loggedInDepart;
        private string loggedInIndex;
        private string LoggedInBank;
        private string LoggedInAccNo;
        private string loggedInName;
        private DataTable cachedData; // Declare cachedData
        private bool isNetworkErrorShown; // Declare isNetworkErrorShown
        private bool isNetworkUnavailable; // Declare isNetworkUnavailable
        private byte[] pdfBytes;

        private DataTable masterData; // Stores all loaded data
        private BindingSource bs = new BindingSource(); // For filtering

        public UC_M_MiscellaneousClaim(string username, string department, string emp, string bank, string accountNo, string fullname)
        {
            InitializeComponent();

            //string gg = UserSession.LoggedInBank;
            LoggedInUser = username;
            loggedInDepart = department;
            loggedInIndex = emp;
            LoggedInBank = bank;
            LoggedInAccNo = accountNo;
            loggedInName = fullname;
            dtRequest.Text = DateTime.Now.ToString("dd.MM.yyyy");
            string userlevel = UserSession.logginInUserAccessLevel;
            string userfullName = UserSession.loggedInName;
            cmbRequester.SelectedIndexChanged += cmbRequester_SelectedIndexChanged;
            cmbDepart.SelectedIndexChanged += cmbDepart_SelectedIndexChanged;
            cachedData = new DataTable(); // Initialize (replace with actual cache loading logic)
            isNetworkErrorShown = false;
            isNetworkUnavailable = false;
            this.Load += UC_Miscellaneous_Load;
            //MessageBox.Show($"loggedInName: {UserSession.loggedInName}");
            //MessageBox.Show($"LoggedInUser: {UserSession.LoggedInUser}");
        }
        private void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                // Connection string (replace with your actual connection string)
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Mail, Password, Port, SmtpClient FROM tbl_Administrator WHERE ID = 1";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string fromEmail = reader["Mail"].ToString();
                                string password = reader["Password"].ToString();
                                int port = Convert.ToInt32(reader["Port"]);
                                string smtpClient = reader["SmtpClient"].ToString();

                                MailMessage mail = new MailMessage();
                                mail.From = new MailAddress(fromEmail);
                                mail.To.Add(toEmail);
                                mail.Subject = subject;
                                mail.Body = body;
                                mail.IsBodyHtml = true;

                                SmtpClient smtp = new SmtpClient(smtpClient, port);
                                smtp.Credentials = new NetworkCredential(fromEmail, password);
                                smtp.EnableSsl = false;

                                smtp.Send(mail);

                                //MessageBox.Show("Notification for your booking will be sent to your approver.",
                                //    "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (SmtpException smtpEx)
            {
                MessageBox.Show($"SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}\n\nFull Details:\n{smtpEx.ToString()}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"General Error: {ex.Message}\n\nFull Details:\n{ex.ToString()}");
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
                // Update label text if needed
            }
            else
            {
                MessageBox.Show("Panel not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "Account";
            Form_Home.sharedbtnMCReport.Visible = false;
            Form_Home.sharedbtnApproval.Visible = false;

            UC_Acc_Account ug = new UC_Acc_Account(LoggedInUser, loggedInDepart, loggedInIndex, LoggedInBank, LoggedInAccNo);
            addControls(ug);
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            string selectedType = cmbType.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedType))
            {
                MessageBox.Show("Please select an Expenses claim type before proceeding.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedType == "Work")
            {
                Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Work";
                Form_Home.sharedbtnMCReport.Visible = false;
                Form_Home.sharedbtnApproval.Visible = false;

                UC_M_Work ug = new UC_M_Work(LoggedInUser, loggedInDepart, selectedType, loggedInIndex, LoggedInBank, LoggedInAccNo);
                addControls(ug);
            }
            else if (selectedType == "Benefit")
            {
                Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Benefit";
                Form_Home.sharedbtnMCReport.Visible = false;
                Form_Home.sharedbtnApproval.Visible = false;

                UC_M_Work ug = new UC_M_Work(LoggedInUser, loggedInDepart, selectedType, loggedInIndex, LoggedInBank, LoggedInAccNo);
                addControls(ug);
            }
        }
        private void CheckUserAccess()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = "SELECT a.Username, a.Name1, a.Position, b.TitlePosition, b.AccessLevel FROM tbl_Users a LEFT JOIN tbl_UsersLevel b ON a.Position = b.TitlePosition WHERE a.Username = @Username";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", UserSession.LoggedInUser);
                        using (SqlDataReader reader2 = cmd.ExecuteReader())
                        {
                            if (reader2.Read())
                            {
                                int accessLevel = Convert.ToInt32(reader2["AccessLevel"]);

                                if (accessLevel >= 1 && accessLevel <= 9 && UserSession.loggedInDepart != "ACCOUNT")
                                {
                                    if (accessLevel == 1 && UserSession.loggedInDepart == "HR & ADMIN")  // exclude zarawi
                                    {
                                        groupBox3.Visible = false;
                                        label19.Visible = false;
                                        label20.Visible = false;
                                        txtSearchSn.Visible = false;
                                        btnSearch.Visible = false;
                                    }
                                    else
                                    {
                                        groupBox3.Visible = true;
                                    }
                                    
                                }
                                else if (accessLevel == 99 && UserSession.loggedInDepart == "ACCOUNT")
                                {
                                    groupBox3.Visible = true;
                                }
                                else if (accessLevel >= 1 && accessLevel <= 9 && UserSession.loggedInDepart == "ACCOUNT")
                                {
                                    groupBox3.Visible = true;
                                }
                                else
                                {
                                    groupBox3.Visible = false;
                                    label19.Visible = false;
                                    label20.Visible = false;
                                    txtSearchSn.Visible = false;
                                    btnSearch.Visible = false;
                                }

                            }
                            else
                            {
                                groupBox3.Visible = false;
                                label19.Visible = false;
                                label20.Visible = false;
                                txtSearchSn.Visible = false;
                                btnSearch.Visible = false;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking user access: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void cmbECtype_SelectedIndexChanged(object sender, EventArgs e)
        {
            string expensesType = cmbECtype.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(expensesType) || masterData == null)
                return;

            if (expensesType == "-- All --")
            {
                BindDataGridView(masterData); // Show all
            }
            else
            {
                var rows = masterData.AsEnumerable()
                    .Where(r => r.Field<string>("ExpensesType") == expensesType);

                if (rows.Any())
                {
                    BindDataGridView(rows.CopyToDataTable());
                }
                else
                {
                    // no match → show empty grid
                    BindDataGridView(masterData.Clone());
                }
            }
        }
        private void dtpStart_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        private void cmbRequester_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedReq = cmbRequester.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(selectedReq))
            {
                if (selectedReq == "-- All --")
                {
                    BindDataGridView(masterData); // Show all
                }
                else
                {
                    var filtered = masterData.AsEnumerable()
                        .Where(r => r.Field<string>("Requester") == selectedReq)
                        .CopyToDataTable();

                    BindDataGridView(filtered);
                }
            }
        }
        private void LoadData()
        {
            //int showOwnRecords = int.TryParse(UserSession.logginInUserAccessLevel, out int accessLevel) && accessLevel < 1;
            //bool showOwnRecords = int.TryParse(UserSession.logginInUserAccessLevel, out int accessLevel) && accessLevel < 1 && accessLevel < 8;

            
            string query = "";
            //MessageBox.Show($"LoggedInUser: {UserSession.logginInUserAccessLevel}");
            if (int.TryParse(UserSession.logginInUserAccessLevel, out int accessLevel) &&
            (
                accessLevel == 0 ||
                (accessLevel == 99 && UserSession.loggedInDepart != "ACCOUNT") ||
                (accessLevel == 1 && UserSession.loggedInDepart == "HR & ADMIN")
            ))
            {
                // Query 1: Show only logged-in user's own records
                query = @"
            SELECT 
                a.SerialNo, a.Requester, a.Department, a.ExpensesType, a.RequestDate, 
                a.HODApprovalStatus, 
                ISNULL(CONVERT(VARCHAR, a.ApprovedByHOD, 120), 'Pending') AS ApprovedByHOD, 
                ISNULL(CONVERT(VARCHAR, a.HODApprovedDate, 120), 'Pending') AS HODApprovedDate, 
                a.HRApprovalStatus,
                ISNULL(CONVERT(VARCHAR, a.ApprovedByHR, 120), 'Pending') AS ApprovedByHR,
                ISNULL(CONVERT(VARCHAR, a.HRApprovedDate, 120), 'Pending') AS HRApprovedDate,
                a.AccountApprovalStatus, 
                ISNULL(CONVERT(VARCHAR, a.ApprovedByAccount, 120), 'Pending') AS ApprovedByAccount,
                ISNULL(CONVERT(VARCHAR, a.AccountApprovedDate, 120), 'Pending') AS AccountApprovedDate,
                ISNULL(CONVERT(VARCHAR, a.Account2ApprovalStatus, 120), 'Pending') AS Account2ApprovalStatus,
                ISNULL(CONVERT(VARCHAR, a.ApprovedByAccount2, 120), 'Pending') AS ApprovedByAccount2,
                ISNULL(CONVERT(VARCHAR, a.Account2ApprovedDate, 120), 'Pending') AS Account2ApprovedDate,
                ISNULL(CONVERT(VARCHAR, a.Account3ApprovalStatus, 120), 'Pending') AS Account3ApprovalStatus,
                ISNULL(CONVERT(VARCHAR, a.ApprovedByAccount3, 120), 'Pending') AS ApprovedByAccount3,
                ISNULL(CONVERT(VARCHAR, a.Account3ApprovedDate, 120), 'Pending') AS Account3ApprovedDate,
                b.Username, c.AccessLevel, b.SuperApprover, d.Department1
            FROM 
                tbl_MasterClaimForm a
            LEFT JOIN tbl_Users b ON a.EmpNo = b.IndexNo
            LEFT JOIN tbl_UsersLevel c ON b.Position = c.TitlePosition
            LEFT JOIN tbl_Department d ON b.Department = d.Department0
            WHERE
                b.Name1 = @LoggedInUser
                AND (@StartDate IS NULL OR CAST(a.RequestDate AS DATE) >= @StartDate)
                AND (@EndDate IS NULL OR CAST(a.RequestDate AS DATE) <= @EndDate)
            ORDER BY a.RequestDate ASC";
            }
            else
            {
                // Query 2: Original query (department filtering)
                query = @"
            SELECT 
    a.SerialNo, a.Requester, a.Department, a.ExpensesType, a.RequestDate, 
    a.HODApprovalStatus, 
    ISNULL(CONVERT(VARCHAR, a.ApprovedByHOD, 120), 'Pending') AS ApprovedByHOD, 
    ISNULL(CONVERT(VARCHAR, a.HODApprovedDate, 120), 'Pending') AS HODApprovedDate, 
    a.HRApprovalStatus,
    ISNULL(CONVERT(VARCHAR, a.ApprovedByHR, 120), 'Pending') AS ApprovedByHR,
    ISNULL(CONVERT(VARCHAR, a.HRApprovedDate, 120), 'Pending') AS HRApprovedDate,
    a.AccountApprovalStatus, 
    ISNULL(CONVERT(VARCHAR, a.ApprovedByAccount, 120), 'Pending') AS ApprovedByAccount,
    ISNULL(CONVERT(VARCHAR, a.AccountApprovedDate, 120), 'Pending') AS AccountApprovedDate,
    ISNULL(CONVERT(VARCHAR, a.Account2ApprovalStatus, 120), 'Pending') AS Account2ApprovalStatus,
    ISNULL(CONVERT(VARCHAR, a.ApprovedByAccount2, 120), 'Pending') AS ApprovedByAccount2,
    ISNULL(CONVERT(VARCHAR, a.Account2ApprovedDate, 120), 'Pending') AS Account2ApprovedDate,
    ISNULL(CONVERT(VARCHAR, a.Account3ApprovalStatus, 120), 'Pending') AS Account3ApprovalStatus,
    ISNULL(CONVERT(VARCHAR, a.ApprovedByAccount3, 120), 'Pending') AS ApprovedByAccount3,
    ISNULL(CONVERT(VARCHAR, a.Account3ApprovedDate, 120), 'Pending') AS Account3ApprovedDate,
    b.Username, c.AccessLevel, b.SuperApprover, d.Department1
FROM 
    tbl_MasterClaimForm a
LEFT JOIN tbl_Users b ON a.EmpNo = b.IndexNo
LEFT JOIN tbl_UsersLevel c ON b.Position = c.TitlePosition
LEFT JOIN tbl_Department d ON b.Department = d.Department0
WHERE 
    (@StartDate IS NULL OR CAST(a.RequestDate AS DATE) >= @StartDate)
    AND (@EndDate IS NULL OR CAST(a.RequestDate AS DATE) <= @EndDate)
    AND (
        EXISTS (
            SELECT 1 
            FROM tbl_Department d2
            WHERE d2.Department3 = @LoggedInDept 
               OR d2.Department4 = @LoggedInDept 
               OR d2.Department5 = @LoggedInDept
        )
        OR a.Department IN (
            SELECT Department0
            FROM tbl_Department
            WHERE Department1 = @LoggedInDept
        )
        OR a.Department = @LoggedInDept
    )
    AND (
    -- Case 1: Logged in user is level 99 → see all
    @LoggedInUserAccessLevel = 99

    -- Case 2: Always see own record
    OR b.Name1 = @LoggedInUser

    -- Case 3: Department rule
    OR a.Department <> @LoggedInDept

    OR c.AccessLevel < @LoggedInUserAccessLevel

OR (a.Department = @LoggedInDept AND c.AccessLevel = 99)


)

ORDER BY a.RequestDate ASC";
            }

            SqlConnection con = null;

            try
            {
                con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString);
                con.Open();

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // Parameters
                    cmd.Parameters.AddWithValue("@LoggedInDept", UserSession.loggedInDepart);
                    cmd.Parameters.AddWithValue("@LoggedInUser", UserSession.loggedInName); // <-- Add user parameter
                    cmd.Parameters.AddWithValue("@LoggedInUserAccessLevel", accessLevel);


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

                    // Fill tables
                    DataTable dt = new DataTable();
                    masterData = new DataTable();

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(masterData);
                        da.Fill(dt);
                    }

                    // ComboBox loading
                    cmbDepart.SelectedIndexChanged -= cmbDepart_SelectedIndexChanged;
                    cmbRequester.SelectedIndexChanged -= cmbRequester_SelectedIndexChanged;
                    cmbECtype.SelectedIndexChanged -= cmbECtype_SelectedIndexChanged;

                    var uniqueDepts = masterData.AsEnumerable()
                        .Select(r => r.Field<string>("Department"))
                        .Where(d => !string.IsNullOrEmpty(d))
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();

                    cmbDepart.Items.Clear();
                    cmbDepart.Items.Add("-- All --");
                    cmbDepart.Items.AddRange(uniqueDepts.ToArray());
                    cmbDepart.SelectedIndex = 0;

                    var requesters = masterData.AsEnumerable()
                        .Select(r => r.Field<string>("Requester"))
                        .Where(d => !string.IsNullOrEmpty(d))
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();

                    cmbRequester.Items.Clear();
                    cmbRequester.Items.Add("-- All --");
                    cmbRequester.Items.AddRange(requesters.ToArray());
                    cmbRequester.SelectedIndex = 0;

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

                    cmbDepart.SelectedIndexChanged += cmbDepart_SelectedIndexChanged;
                    cmbRequester.SelectedIndexChanged += cmbRequester_SelectedIndexChanged;
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
        private void cmbDepart_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Prevent recursive calls during programmatic changes
            cmbRequester.SelectedIndexChanged -= cmbRequester_SelectedIndexChanged;

            // Get the selected department
            string selectedDept = cmbDepart.SelectedItem?.ToString();

            // Filter requesters based on the selected department
            var requesters = masterData.AsEnumerable()
                .Where(r => selectedDept == "-- All --" || r.Field<string>("Department") == selectedDept)
                .Select(r => r.Field<string>("Requester"))
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            // Update cmbRequester
            cmbRequester.Items.Clear();
            cmbRequester.Items.Add("-- All --");
            cmbRequester.Items.AddRange(requesters.ToArray());
            cmbRequester.SelectedIndex = 0;

            // Reattach the event handler
            cmbRequester.SelectedIndexChanged += cmbRequester_SelectedIndexChanged;

            // Filter DataGridView (preserve existing behavior)
            if (!string.IsNullOrEmpty(selectedDept))
            {
                if (selectedDept == "-- All --")
                {
                    BindDataGridView(masterData); // Show all
                }
                else
                {
                    var filtered = masterData.AsEnumerable()
                        .Where(r => r.Field<string>("Department") == selectedDept)
                        .CopyToDataTable();

                    BindDataGridView(filtered);
                }
            }
        }
        private void HandleRowSelection()
        {
            if (dgvMS.SelectedCells.Count == 0)
                return; // no row selected yet

            DataGridViewCell selectedCell = dgvMS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;

            string serialNo = selectedRow.Cells["SerialNo"].Value?.ToString();
            int accessLevelRequester = -1;

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    // 🔹 First query: requester info
                    string query = @"
                SELECT C.AccessLevel, A.Requester, A.Department, A.ExpensesType, A.HODApprovalStatus,A.HRApprovalStatus, A.Account2ApprovalStatus,A.Account3ApprovalStatus, A.AccountApprovalStatus, B.Position
                FROM tbl_MasterClaimForm A
                LEFT JOIN tbl_Users B ON A.Requester = B.Name1
                LEFT JOIN tbl_UsersLevel C ON B.Position = C.TitlePosition
                WHERE A.SerialNo = @SerialNo";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@SerialNo", SqlDbType.VarChar).Value = serialNo;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                groupBox3.Visible = false;
                                return;
                            }

                            accessLevelRequester = reader["AccessLevel"] != DBNull.Value
                                ? Convert.ToInt32(reader["AccessLevel"])
                                : 0;
                            string requester = reader["Requester"]?.ToString() ?? string.Empty;
                            string expensesType = reader["ExpensesType"]?.ToString() ?? string.Empty;
                            string RequesterTitlePosition = reader["Position"]?.ToString() ?? string.Empty;
                            string GA = reader["AccountApprovalStatus"]?.ToString() ?? string.Empty;
                            string HR = reader["HRApprovalStatus"]?.ToString() ?? string.Empty;
                            string HOD = reader["HODApprovalStatus"]?.ToString() ?? string.Empty;
                            string depart = reader["Department"]?.ToString() ?? string.Empty;
                            reader.Close(); // ✅ close before running second query
                                            // ---------------------------------------------------------

                            // 🔹 Second query: logged-in user info
                            string query1 = @"
                        SELECT a.Username, a.Name1, a.Position, b.TitlePosition, b.AccessLevel 
                        FROM tbl_Users a 
                        LEFT JOIN tbl_UsersLevel b ON a.Position = b.TitlePosition 
                        WHERE a.Username = @Username";

                            using (SqlCommand cmd1 = new SqlCommand(query1, con))
                            {
                                cmd1.Parameters.AddWithValue("@Username", UserSession.LoggedInUser);

                                using (SqlDataReader reader2 = cmd1.ExecuteReader()) // ✅ use cmd1
                                {
                                    if (!reader2.Read())
                                    {
                                        groupBox3.Visible = false;
                                        return;
                                    }

                                    int accessLevel = Convert.ToInt32(reader2["AccessLevel"]);

                                    // 🔹 Your condition logic
                                    if (accessLevel >= 0 && accessLevel <= 9 && UserSession.loggedInDepart != "ACCOUNT")
                                    {
                                        
                                        if (int.TryParse(UserSession.logginInUserAccessLevel, out int currentAccessLevel))
                                        {
                                            if (currentAccessLevel < accessLevelRequester && accessLevelRequester != 99 && UserSession.loggedInDepart != "ACCOUNT")
                                            {
                                                //
                                                if (UserSession.loggedInDepart == "HR & ADMIN" && expensesType != "Work" && currentAccessLevel > accessLevelRequester && depart == "HR & ADMIN")
                                                {
                                                    groupBox3.Visible = true;
                                                }
                                                else if (UserSession.loggedInDepart == "GENERAL AFFAIRS")
                                                {
                                                    groupBox3.Visible = true;
                                                }
                                                else 
                                                {
                                                    if (depart == "HR & ADMIN")
                                                    {
                                                        groupBox3.Visible = false;
                                                    }
                                                    else
                                                    {
                                                        groupBox3.Visible = true; //tukar sebab jega benefit tak sho
                                                    }
                                                    
                                                }
                                            }
                                            else if (RequesterTitlePosition == "MANAGING DIRECTOR")
                                            {
                                                //if md gb tak show
                                                groupBox3.Visible = false;
                                            }
                                            else if ((currentAccessLevel > accessLevelRequester || accessLevelRequester == 99) && expensesType == "Benefit")
                                            {
                                                if(depart == "HR & ADMIN")
                                                {
                                                    if(UserSession.loggedInDepart == "GENERAL AFFAIRS" && accessLevelRequester < 2)
                                                    {
                                                        groupBox3.Visible = false;
                                                    }
                                                    else 
                                                    {
                                                        groupBox3.Visible = true;
                                                    }
                                                    
                                                }
                                                else
                                                {
                                                    groupBox3.Visible = true;
                                                }
                                                
                                            }
                                            else if ((currentAccessLevel > accessLevelRequester || accessLevelRequester == 99) && expensesType == "Work"  && depart != "HR & ADMIN")
                                            {
                                                if(depart == "HR & ADMIN" && UserSession.loggedInDepart != "HR & ADMIN")
                                                {
                                                    
                                                    groupBox3.Visible = true;
                                                }
                                                else
                                                {
                                               
                                                    groupBox3.Visible = false;
                                                }
                                                
                                            }
                                            else if ((currentAccessLevel > accessLevelRequester || accessLevelRequester == 99) && expensesType == "Work" && depart == "HR & ADMIN")
                                            {
                                                if (UserSession.loggedInDepart == "GENERAL AFFAIRS" && accessLevelRequester < 2)
                                                {
                                                    groupBox3.Visible = false;
                                                }
                                                else
                                                {
                                                    groupBox3.Visible = true;
                                                }
                                            }
                                            else if (UserSession.loggedInName == requester)
                                            {
                                               
                                                //if(expensesType =="")
                                                if (HOD == "Approved" && requester != "Normala")
                                                {
                                                    if(depart == "HR & ADMIN" || depart == "ACCOUNT" || depart == "GENERAL AFFAIRS")
                                                    {
                                                        //MessageBox.Show("ddd");

                                                        groupBox3.Visible = true;  
                                                    }
                                                    else
                                                    {
                                                        groupBox3.Visible = false;
                                                    }
                                                    
                                                }
                                                else
                                                {
                                                    groupBox3.Visible = false;
                                                }
                                                
                                            }
                                            else
                                            {
                                                groupBox3.Visible = false;
                                            }
                                        }
                                    }
                                    else if (accessLevel == 99 && UserSession.loggedInDepart == "ACCOUNT")
                                    {
                                        groupBox3.Visible = true;
                                    }
                                    else
                                    {
                                        groupBox3.Visible = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                groupBox3.Visible = false;
            }
        }
        private void UC_Miscellaneous_Load(object sender, EventArgs e)
        {
            dgvMS.SelectionChanged += dgvMS_SelectionChanged;
            cmbDepart.SelectedIndexChanged += cmbDepart_SelectedIndexChanged;
            //LoadData(); // Load data with default weekly filter
            CheckUserAccess(); // Check user access to set button visibility
        }
        private void dgvMS_SelectionChanged(object sender, EventArgs e)
        {
           // HandleRowSelection(); 
        }
        private bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
        private void BindDataGridView(DataTable dt)
        {
            dgvMS.AutoGenerateColumns = false;
            dgvMS.Columns.Clear();
            dgvMS.ReadOnly = true;
            //dgvMS.DataSource = dt;
            dgvMS.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new System.Drawing.Font("Arial", 11, FontStyle.Bold),
            };

            int fixedColumnWidth = 180;

            // Add columns as in the original code
            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "HODApprovalStatus",
                HeaderText = "HOD Status Check",
                DataPropertyName = "HODApprovalStatus",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ApprovedByHOD",
                HeaderText = "Approved By",
                DataPropertyName = "ApprovedByHOD",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "HODApprovedDate",
                HeaderText = "Approved Date",
                DataPropertyName = "HODApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11),
                    Format = "dd.MM.yyyy"
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "HRApprovalStatus",
                HeaderText = "HR Status Check",
                DataPropertyName = "HRApprovalStatus",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ApprovedByHR",
                HeaderText = "Approved By",
                DataPropertyName = "ApprovedByHR",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "HRApprovedDate",
                HeaderText = "Approved Date",
                DataPropertyName = "HRApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11),
                    Format = "dd.MM.yyyy"
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Account2ApprovalStatus",
                HeaderText = "Account 1 Status Check",
                DataPropertyName = "Account2ApprovalStatus",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ApprovedByAccount2",
                HeaderText = "Approved By",
                DataPropertyName = "ApprovedByAccount2",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Account2ApprovedDate",
                HeaderText = "Approved Date",
                DataPropertyName = "Account2ApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11),
                    Format = "dd.MM.yyyy"
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Account3ApprovalStatus",
                HeaderText = "Account 2 Status Check",
                DataPropertyName = "Account3ApprovalStatus",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ApprovedByAccount3",
                HeaderText = "Approved By",
                DataPropertyName = "ApprovedByAccount3",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Account3ApprovedDate",
                HeaderText = "Approved Date",
                DataPropertyName = "Account3ApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11),
                    Format = "dd.MM.yyyy"
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "AccountApprovalStatus",
                HeaderText = "General Affair Status Check",
                DataPropertyName = "AccountApprovalStatus",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ApprovedByAccount",
                HeaderText = "Approved By",
                DataPropertyName = "ApprovedByAccount",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "AccountApprovedDate",
                HeaderText = "Approved Date",
                DataPropertyName = "AccountApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11),
                    Format = "dd.MM.yyyy"
                },
            });

            // Create a new DataTable to modify HR-related columns (except HRApprovedDate)
            DataTable modifiedDt = dt.Copy();
            foreach (DataRow row in modifiedDt.Rows)
            {
                if (row["ExpensesType"]?.ToString() == "Work")
                {
                    row["HRApprovalStatus"] = "-";
                    row["ApprovedByHR"] = "-";
                    // Do not modify HRApprovedDate in the DataTable to avoid type mismatch
                }
            }

            // Set the DataSource
            dgvMS.DataSource = modifiedDt;
            dgvMS.CellBorderStyle = DataGridViewCellBorderStyle.None;

            // Attach CellFormatting event handler to handle HRApprovedDate display
            dgvMS.CellFormatting += dgvMS_CellFormatting;

            //Debug.WriteLine("DataGridView updated successfully.");
        }
        // CellFormatting event handler to display "-" for HRApprovedDate when ExpensesType is "Work"
        private void dgvMS_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvMS.Columns[e.ColumnIndex].Name == "HRApprovedDate" && e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvMS.Rows[e.RowIndex];
                if (row.Cells["ExpensesType"].Value?.ToString() == "Work")
                {
                    e.Value = "-";
                    e.FormattingApplied = true;
                }
            }
        }
        private void btnWithdraw_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(UserSession.loggedInName);
            // Check if a cell is selected in the DataGridView
            if (dgvMS.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to withdraw.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the selected row
            DataGridViewCell selectedCell = dgvMS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string serialNo = selectedRow.Cells["SerialNo"].Value?.ToString();
            string requester = selectedRow.Cells["Requester"].Value?.ToString();
            string hodApprovalStatus = selectedRow.Cells["HODApprovalStatus"].Value?.ToString();
            string hrApprovalStatus = selectedRow.Cells["HRApprovalStatus"].Value?.ToString();
            string accountApprovalStatus = selectedRow.Cells["AccountApprovalStatus"].Value?.ToString();

            // Validate the selection
            if (string.IsNullOrEmpty(serialNo) || string.IsNullOrEmpty(requester))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Restrict withdrawal to only the user's own orders
            if (requester != UserSession.loggedInName)
            {
                MessageBox.Show("You can only withdraw your own orders.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if the order has been approved by any department
            if (hodApprovalStatus == "Approved" || hrApprovalStatus == "Approved" || accountApprovalStatus == "Approved")
            {
                MessageBox.Show("This order cannot be withdrawn because it has been approved.", "Withdrawal Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            

            // Confirm deletion with the user
            DialogResult result = MessageBox.Show($"Are you sure you want to withdraw Serial No: {serialNo}?", "Confirm Withdrawal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            // Delete the order from the database
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = "BEGIN TRANSACTION;\r\nDELETE FROM tbl_DetailClaimForm WHERE SerialNo = @SerialNo;\r\nDELETE FROM tbl_MasterClaimForm WHERE SerialNo = @SerialNo;\r\nCOMMIT;";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SerialNo", serialNo);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Successfully withdrawn.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Refresh the DataGridView
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Failed to withdraw the order. It may have already been processed or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error withdrawing order: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //Debug.WriteLine($"Error withdrawing order: {ex.Message}");
                }
            }
        }
        private void btnApprove_Click(object sender, EventArgs e)
        {
            // Check if a cell is selected in the DataGridView
            if (dgvMS.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to approve.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the selected row
            DataGridViewCell selectedCell = dgvMS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string serialNo = selectedRow.Cells["SerialNo"].Value?.ToString();
            string requester = selectedRow.Cells["Requester"].Value?.ToString();
            string hodApprovalStatus = selectedRow.Cells["HODApprovalStatus"].Value?.ToString();
            string hrApprovalStatus = selectedRow.Cells["HRApprovalStatus"].Value?.ToString();
            string account2ApprovalStatus = selectedRow.Cells["Account2ApprovalStatus"].Value?.ToString();
            string account3ApprovalStatus = selectedRow.Cells["Account3ApprovalStatus"].Value?.ToString();
            string accountApprovalStatus = selectedRow.Cells["AccountApprovalStatus"].Value?.ToString();
            string expensesType = selectedRow.Cells["ExpensesType"].Value?.ToString();
            string department = selectedRow.Cells["Department"].Value?.ToString();

            ///////////////////     ACCESSS LEVEL
            //
            /*

            int accessLevelRequester = -1; // default value

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = @"
                    SELECT C.AccessLevel, A.Requester, A.Department, A.ExpensesType, A.HODApprovalStatus,A.HRApprovalStatus, A.Account2ApprovalStatus,A.Account3ApprovalStatus, A.AccountApprovalStatus, B.Position
                    FROM tbl_MasterClaimForm A
                    LEFT JOIN tbl_Users B ON A.Requester = B.Name1
                    LEFT JOIN tbl_UsersLevel C ON B.Position = C.TitlePosition
                    WHERE A.SerialNo = @SerialNo";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SerialNo", serialNo);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            accessLevelRequester = Convert.ToInt32(result);
                        }
                        else
                        {
                            MessageBox.Show("User access level not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving user access level: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Parse current user access level safely
            if (int.TryParse(UserSession.logginInUserAccessLevel, out int currentAccessLevel))
            {
                if (currentAccessLevel < accessLevelRequester && accessLevelRequester != 99 && UserSession.loggedInDepart != "ACCOUNT")
                {
                    if (UserSession.loggedInDepart == "HR & ADMIN")
                    {
                        MessageBox.Show("You are not authorized to approve this miscellaneous claim.",
                                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        MessageBox.Show("You are not authorized to approve this miscellaneous claim.",
                                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else if (currentAccessLevel < accessLevelRequester &&
                         UserSession.loggedInDepart == "HR & ADMIN" &&
                         department == "HR & ADMIN" && accessLevelRequester != 99)
                {
                    MessageBox.Show("You cannot approve this miscellaneous claim because the requester has a higher access level within HR & ADMIN.",
                                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (UserSession.loggedInDepart != department &&
                         UserSession.loggedInDepart == "HR & ADMIN" &&
                         expensesType == "Work")
                {
                    MessageBox.Show("You are not authorized to approve this miscellaneous claim.",
                                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


            }
            else
            {
                MessageBox.Show("Invalid access level for logged-in user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            */
                //////////////////////////


            // Validate the selection
            if (string.IsNullOrEmpty(serialNo) || string.IsNullOrEmpty(requester))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if any department has rejected the order
            if (hodApprovalStatus == "Rejected" || hrApprovalStatus == "Rejected" ||
                account2ApprovalStatus == "Rejected" || account3ApprovalStatus == "Rejected" ||
                accountApprovalStatus == "Rejected")
            {
                MessageBox.Show("This Miscellaneous Claim cannot be approved because it has been rejected by one or more departments.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get user access level from tbl_UsersLevel by joining with tbl_Users
            int userAccessLevel = 2;
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = @"
                                    SELECT ul.AccessLevel 
                                    FROM tbl_UsersLevel ul
                                    LEFT JOIN tbl_Users u ON ul.TitlePosition = u.Position
                                    WHERE u.Name1 = @Name1";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Name1", UserSession.loggedInName);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            userAccessLevel = Convert.ToInt32(result);
                        }
                        else
                        {
                            MessageBox.Show("User access level not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving user access level: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ///+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  ACCOUNT                 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            // Handle ACCOUNT department approvals
            if (UserSession.loggedInDepart == "ACCOUNT" || UserSession.loggedInDepart == "GENERAL AFFAIRS" && hodApprovalStatus == "Approved")
            {
                // Handle Account2ApprovalStatus (AccessLevel 0)
                if (userAccessLevel == 99)
                {
                    // Check if HODApprovalStatus is Pending or Rejected for Work expenses
                    if (expensesType == "Work")
                    {
                        if (hodApprovalStatus == "Pending")
                        {
                            MessageBox.Show("This Miscellaneous Claim cannot be approved by 1st-Level Account because HOD approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (hodApprovalStatus == "Rejected")
                        {
                            MessageBox.Show("This Miscellaneous Claim cannot be approved by 1st-Level Account because HOD approval was Rejected.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                    }

                    if (expensesType == "Benefit")
                    {
                        if (hodApprovalStatus == "Pending")
                        {
                            MessageBox.Show("This Miscellaneous Claim cannot be approved by 1st-Level Account because HOD approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (hodApprovalStatus == "Rejected")
                        {
                            MessageBox.Show("This Miscellaneous Claim cannot be approved by 1st-Level Account because HOD approval was Rejected.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (hrApprovalStatus == "Pending")
                        {
                            MessageBox.Show("This Miscellaneous Claim cannot be approved by 1st-Level Account because HR & ADMIN approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        if (hrApprovalStatus == "Rejected")
                        {
                            MessageBox.Show("This Miscellaneous Claim cannot be approved by 1st-Level Account because HR & ADMIN approval was Rejected.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    // Check if already approved
                    if (account2ApprovalStatus == "Approved")
                    {
                        MessageBox.Show("This Miscellaneous Claim has already been approved by 1st-Level Account.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Confirm Account2 approval
                    DialogResult result = MessageBox.Show($"Are you sure you want to approve Miscellaneous Claim  for Serial No: {serialNo} as 1st-Level Account?", "Confirm Account Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    // Update database for Account2 approval
                    try
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                        {
                            con.Open();
                            string query = @"
                UPDATE tbl_MasterClaimForm 
                SET Account2ApprovalStatus = @Account2ApprovalStatus,
                    ApprovedByAccount2 = @ApprovedByAccount2,
                    Account2ApprovedDate = @Account2ApprovedDate 
                WHERE SerialNo = @SerialNo AND Account2ApprovalStatus = 'Pending'";
                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@Account2ApprovalStatus", "Approved");
                                cmd.Parameters.AddWithValue("@ApprovedByAccount2", UserSession.loggedInName);
                                cmd.Parameters.AddWithValue("@Account2ApprovedDate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Miscellaneous Claim approved successfully by 1st-Level Account.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();


                                    ///+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  Send  Notification to Account 2 approver                 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                    // Step 2: Get 2nd level Account approver(s)
                                    List<string> accountApproverEmails = new List<string>();
                                    string getApproversQuery = @"
                                                                SELECT B.Email 
                                                                FROM tbl_Users A
                                                                LEFT JOIN tbl_UserDetail B ON A.IndexNo = B.IndexNo
                                                                LEFT JOIN tbl_UsersLevel C ON A.Position = C.TitlePosition
                                                                WHERE A.Department = 'ACCOUNT' AND C.AccessLevel = '1'";  // First level account approver

                                    using (SqlCommand cmd2 = new SqlCommand(getApproversQuery, con))
                                    using (SqlDataReader reader = cmd2.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            string email = reader["Email"]?.ToString();
                                            if (!string.IsNullOrWhiteSpace(email))
                                            {
                                                accountApproverEmails.Add(email);
                                            }
                                        }
                                    }

                                    // Step 3: Get claim details for email body
                                    string requesterName = "";
                                    string expenType = "";
                                    DateTime requestDate = DateTime.MinValue;

                                    string getClaimDetailsQuery = @"SELECT Requester, ExpensesType, RequestDate 
                                        FROM tbl_MasterClaimForm 
                                        WHERE SerialNo = @SerialNo";

                                    using (SqlCommand cmd3 = new SqlCommand(getClaimDetailsQuery, con))
                                    {
                                        cmd3.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                        using (SqlDataReader reader = cmd3.ExecuteReader())
                                        {
                                            if (reader.Read())
                                            {
                                                requesterName = reader["Requester"]?.ToString();
                                                expenType = reader["ExpensesType"]?.ToString();
                                                requestDate = reader["RequestDate"] != DBNull.Value
                                                    ? Convert.ToDateTime(reader["RequestDate"])
                                                    : DateTime.MinValue;
                                            }
                                        }
                                    }


                                    // Step 4: Send email to 1st level Account approvers   hemacc1@hosiden.com
                                    if (accountApproverEmails.Count > 0)
                                    {
                                        string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                        string subject = "HEM Admin Accessibility Notification: New Miscellaneous Claim Awaiting For Your Review And Approval";

                                        string body = $@"
                                            <p>Dear Approver - Account,</p>
                                            <p>The following <strong>Miscellaneous Claim</strong> has been approved by <strong>1st-Level Account</strong> and is now awaiting your review.</p>

                                            <p><u>Claim Details:</u></p>
                                            <ul>
                                                <li><strong>Requester:</strong> {requesterName}</li>
                                                <li><strong>Claim Type:</strong> {expenType}</li>
                                                <li><strong>Serial No:</strong> {serialNo}</li>
                                                <li><strong>Submission Date:</strong> {formattedDate}</li>
                                            </ul>

                                            <p>Please log in to the system to <strong>approve</strong> or <strong>reject</strong> this claim.</p>

                                            <p>Thank you,<br/>HEM Admin Accessibility</p>
                                        ";

                                        foreach (var email in accountApproverEmails)///hemacc2@hosiden.com
                                        {
                                            SendEmail(email, subject, body);
                                        }

                                        MessageBox.Show("Notification sent to 2nd-Level Accounts approver.", "Notification Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }

                                }
                                else
                                {
                                    MessageBox.Show("Failed to approve the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error approving Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                // Handle Account3ApprovalStatus (AccessLevel 1)
                else if (userAccessLevel == 1)
                {
                    // Check if Account2ApprovalStatus is Pending or Rejected
                    if (account2ApprovalStatus == "Pending")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be approved by 2nd-Level Account because 1st-Level Account approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (account2ApprovalStatus == "Rejected")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be approved by 2nd-Level Account because 1st-Level Account approval was Rejected.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if already approved
                    if (account3ApprovalStatus == "Approved")
                    {
                        MessageBox.Show("This Miscellaneous Claim has already been approved by 2nd-Level Account.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Confirm Account3 approval
                    DialogResult result = MessageBox.Show($"Are you sure you want to approve Miscellaneous Claim for Serial No: {serialNo} as 2nd-Level Account?", "Confirm Account Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    // Update database for Account3 approval
                    try
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                        {
                            con.Open();
                            string query = @"
                UPDATE tbl_MasterClaimForm 
                SET Account3ApprovalStatus = @Account3ApprovalStatus, 
                    ApprovedByAccount3 = @ApprovedByAccount3, 
                    Account3ApprovedDate = @Account3ApprovedDate 
                WHERE SerialNo = @SerialNo AND Account3ApprovalStatus = 'Pending'";
                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@Account3ApprovalStatus", "Approved");
                                cmd.Parameters.AddWithValue("@ApprovedByAccount3", UserSession.loggedInName);
                                cmd.Parameters.AddWithValue("@Account3ApprovedDate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Miscellaneous Claim approved successfully by 2nd-Level Account.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();

                                    ///+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  Send  Notification to GA (Account HOD 2 approver                 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                    // Step 2: Get 2nd level Account approver(s)
                                    List<string> accountApproverEmails = new List<string>();
                                    string getApproversQuery = @"
                                                                SELECT A.Department, A.Username,  B.Email, C.AccessLevel
                                                                FROM
                                                                tbl_Users A
                                                                LEFT JOIN tbl_UserDetail B ON A.IndexNo = B.IndexNo
                                                                LEFT JOIN tbl_UsersLevel C ON A.Position = C.TitlePosition

                                                                WHERE Department= 'GENERAL AFFAIRS'";  // last level account approver

                                    using (SqlCommand cmd2 = new SqlCommand(getApproversQuery, con))
                                    using (SqlDataReader reader = cmd2.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            string email = reader["Email"]?.ToString();
                                            if (!string.IsNullOrWhiteSpace(email))
                                            {
                                                accountApproverEmails.Add(email);
                                            }
                                        }
                                    }

                                    // Step 3: Get claim details for email body
                                    string requesterName = "";
                                    string expenType = "";
                                    DateTime requestDate = DateTime.MinValue;

                                    string getClaimDetailsQuery = @"SELECT Requester, ExpensesType, RequestDate 
                                        FROM tbl_MasterClaimForm 
                                        WHERE SerialNo = @SerialNo";

                                    using (SqlCommand cmd3 = new SqlCommand(getClaimDetailsQuery, con))
                                    {
                                        cmd3.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                        using (SqlDataReader reader = cmd3.ExecuteReader())
                                        {
                                            if (reader.Read())
                                            {
                                                requesterName = reader["Requester"]?.ToString();
                                                expenType = reader["ExpensesType"]?.ToString();
                                                requestDate = reader["RequestDate"] != DBNull.Value
                                                    ? Convert.ToDateTime(reader["RequestDate"])
                                                    : DateTime.MinValue;
                                            }
                                        }
                                    }


                                    // Step 4: Send email to 1st level Account approvers   hemacc1@hosiden.com
                                    if (accountApproverEmails.Count > 0)
                                    {
                                        string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                        string subject = "HEM Admin Accessibility Notification: New Miscellaneous Claim Awaiting For Your Review And Approval";

                                        string body = $@"
                                            <p>Dear Approver - Account,</p>
                                            <p>The following <strong>Miscellaneous Claim</strong> has been approved by <strong>2nd-Level Account</strong> and is now awaiting your review.</p>

                                            <p><u>Claim Details:</u></p>
                                            <ul>
                                                <li><strong>Requester:</strong> {requesterName}</li>
                                                <li><strong>Claim Type:</strong> {expenType}</li>
                                                <li><strong>Serial No:</strong> {serialNo}</li>
                                                <li><strong>Submission Date:</strong> {formattedDate}</li>
                                            </ul>

                                            <p>Please log in to the system to <strong>approve</strong> or <strong>reject</strong> this claim.</p>

                                            <p>Thank you,<br/>HEM Admin Accessibility</p>
                                        ";

                                        foreach (var email in accountApproverEmails)///hemacc2@hosiden.com
                                        {
                                            SendEmail(email, subject, body);
                                        }

                                        MessageBox.Show("Notification sent to 3rd-Level Account approvers.", "Notification Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Failed to approve the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error approving Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                // Handle AccountApprovalStatus (AccessLevel 3)
                else if (userAccessLevel == 3)
                {
                    // Check if Account3ApprovalStatus is Pending or Rejected
                    if (account3ApprovalStatus == "Pending")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be approved by Account because 2nd-Level Account approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (account3ApprovalStatus == "Rejected")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be approved by Account because 2nd-Level Account approval was Rejected.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    // Check if loggedInDepart is GENERAL AFFAIRS
                    if (UserSession.loggedInDepart != "GENERAL AFFAIRS")
                    {
                        MessageBox.Show("Only users from GENERAL AFFAIRS can approve this Miscellaneous Claim.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    // Check if already approved
                    if (accountApprovalStatus == "Approved")
                    {
                        MessageBox.Show("This Miscellaneous Claim has already been approved by 3rd-Level Account.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    // Confirm Account approval
                    DialogResult result = MessageBox.Show($"Are you sure you want to approve Miscellaneous Claim for Serial No: {serialNo} as 3rd-Level Account?", "Confirm Account Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    // Update database for Account approval
                    try
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                        {
                            con.Open();
                            string query = @"
                UPDATE tbl_MasterClaimForm 
                SET AccountApprovalStatus = @AccountApprovalStatus, 
                    ApprovedByAccount = @ApprovedByAccount, 
                    AccountApprovedDate = @AccountApprovedDate 
                WHERE SerialNo = @SerialNo AND AccountApprovalStatus = 'Pending'";
                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@AccountApprovalStatus", "Approved");
                                cmd.Parameters.AddWithValue("@ApprovedByAccount", UserSession.loggedInName);
                                cmd.Parameters.AddWithValue("@AccountApprovedDate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Miscellaneous Claim approved successfully by 3rd-Level Account.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();

                                    //++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                    string requesterName = "";
                                    string expenType = "";
                                    string indexNum = "";
                                    string userEmails = "";
                                    DateTime requestDate = DateTime.MinValue;

                                    string getClaimDetailsQuery = @"
                                                                    SELECT A.Requester, A.ExpensesType, A.RequestDate, A.EmpNo, B.Email
                                                                    FROM tbl_MasterClaimForm A
                                                                    LEFT JOIN tbl_UserDetail B ON A.EmpNo = B.IndexNo
                                                                    WHERE SerialNo = @SerialNo";
                                    List<string> approverEmails = new List<string>();

                                    using (SqlCommand emailCmd = new SqlCommand(getClaimDetailsQuery, con))
                                    {
                                        emailCmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                        using (SqlDataReader reader = emailCmd.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                string email = reader["Email"]?.ToString();
                                                if (!string.IsNullOrEmpty(email))
                                                {
                                                    approverEmails.Add(email);
                                                }
                                                requesterName = reader["Requester"]?.ToString();
                                                expenType = reader["ExpensesType"]?.ToString();
                                                indexNum = reader["EmpNo"]?.ToString();
                                                userEmails = reader["Email"]?.ToString();
                                                requestDate = reader["RequestDate"] != DBNull.Value
                                                    ? Convert.ToDateTime(reader["RequestDate"])
                                                    : DateTime.MinValue;
                                            }
                                        }
                                    }

                                    if (approverEmails.Count > 0)
                                    {
                                        string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                        string subject = "HEM Admin Accessibility Notification: Your Miscellaneous Claim Has Been Approved!";
                                        string body = $@"
                                                    <p>Dear Mr./Ms. {requesterName},</p>
                                                    <p>Your <strong>Miscellaneous Claim</strong> has been <strong>approved</strong> by Mr./Ms. <strong>{UserSession.loggedInName}</strong></p>

                    
                                                <p><u>Claim Details:</u></p>
                                                <ul>
                                                    <li><strong>Requester:</strong> {requesterName}</li>
                                                    <li><strong>Claim Type:</strong> {expenType}</li>
                                                    <li><strong>Serial No:</strong> {serialNo}</li>
                                                    <li><strong>Submission Date:</strong> {formattedDate}</li>
                                                </ul>

                                                    <p>The approved amount will be processed on the <strong>15th</strong> and <strong>30th</strong> of each month. If either date falls on a non-working day, payment will be made on the <strong>next</strong> or <strong>before</strong> working day.</p>
                                                    <p>Thank you,<br/>HEM Admin Accessibility</p>
                                                ";

                                        foreach (var email in approverEmails)
                                        {
                                            SendEmail(email, subject, body);
                                        }

                                        MessageBox.Show(
                                            "Notification has been sent to the requester confirming the claim status.",
                                            "Notification Sent",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information
                                        );
                                    }

                                    //++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


                                }
                                else
                                {
                                    MessageBox.Show("Failed to approve the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error approving Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("You do not have the required access level to approve this Miscellaneous Claim.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            ///+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  HR & ADMIN                 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            // Handle HR & ADMIN department approval  -- CHANGE ON 21/1/26 ADD FACILITY
            else if (UserSession.loggedInDepart == "HR & ADMIN" && department != "ISO" && department != "FACILITY")
            {
                //MessageBox.Show(hrApprovalStatus);
                // Check if the ExpensesType is Work    -- CHANGE ON 21/1/26 ADD FACILITY
                if (expensesType == "Work" && department != "HR & ADMIN" && department != "ISO"  && department != "FACILITY")
                {
                    MessageBox.Show("HR & ADMIN cannot approve Work-related expenses.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (hrApprovalStatus == "-" && department == "HR & ADMIN" && expensesType == "Work" && hodApprovalStatus == "Approved")//hr
                {
                    MessageBox.Show("HR & ADMIN cannot approve Work-related expenses.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (requester == UserSession.loggedInName)
                {
                    MessageBox.Show("You cannot approve your own Miscellaneous Claim.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Check if HODApprovalStatus is Pending 
                if (hodApprovalStatus == "Pending" && department != "HR & ADMIN")
                {
                    MessageBox.Show("This Miscellaneous Claim cannot be approved by HR because HOD approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (hodApprovalStatus == "Pending" && department == "HR & ADMIN")
                {
                    // Confirm HR approval with the user
                    DialogResult result = MessageBox.Show($"Are you sure you want to approve Miscellaneous Claim for Serial No: {serialNo} as HOD?", "Confirm HOD Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    // Update the database for HR approval
                    try
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                        {
                            con.Open();
                            string query = @"
                UPDATE tbl_MasterClaimForm 
                SET HODApprovalStatus = @HODApprovalStatus, 
                    ApprovedByHOD = @ApprovedByHOD, 
                    HODApprovedDate = @HODApprovedDate 
                WHERE SerialNo = @SerialNo AND HODApprovalStatus = 'Pending'";
                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@HODApprovalStatus", "Approved");
                                cmd.Parameters.AddWithValue("@ApprovedByHOD", UserSession.loggedInName);
                                cmd.Parameters.AddWithValue("@HODApprovedDate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Miscellaneous claim approved successfully by HOD.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                    // Step 2: Get 1st level Account approver(s)
                                    List<string> accountApproverEmails = new List<string>();
                                    string getApproversQuery = @"
                                                    SELECT B.Email 
                                                    FROM tbl_Users A
                                                    LEFT JOIN tbl_UserDetail B ON A.IndexNo = B.IndexNo
                                                    LEFT JOIN tbl_UsersLevel C ON A.Position = C.TitlePosition
                                                    WHERE A.Department = 'ACCOUNT' AND C.AccessLevel = '99'";  // First level account approver

                                    using (SqlCommand cmd5 = new SqlCommand(getApproversQuery, con))
                                    using (SqlDataReader reader = cmd5.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            string email = reader["Email"]?.ToString();
                                            if (!string.IsNullOrWhiteSpace(email))
                                            {
                                                accountApproverEmails.Add(email);
                                            }
                                        }
                                    }

                                    // Step 3: Get claim details for email body
                                    string requesterName = "";
                                    string expenType = "";
                                    DateTime requestDate = DateTime.MinValue;

                                    string getClaimDetailsQuery = @"SELECT Requester, ExpensesType, RequestDate 
                                        FROM tbl_MasterClaimForm 
                                        WHERE SerialNo = @SerialNo";

                                    using (SqlCommand cmd6 = new SqlCommand(getClaimDetailsQuery, con))
                                    {
                                        cmd6.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                        using (SqlDataReader reader = cmd6.ExecuteReader())
                                        {
                                            if (reader.Read())
                                            {
                                                requesterName = reader["Requester"]?.ToString();
                                                expenType = reader["ExpensesType"]?.ToString();
                                                requestDate = reader["RequestDate"] != DBNull.Value
                                                    ? Convert.ToDateTime(reader["RequestDate"])
                                                    : DateTime.MinValue;
                                            }
                                        }
                                    }

                                    // Step 4: Send email to 1st level Account approvers
                                    if (accountApproverEmails.Count > 0)
                                    {
                                        string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                        string subject = "HEM Admin Accessibility Notification: New Miscellaneous Claim Awaiting For Your Review And Approval";

                                        string body = $@"
                                            <p>Dear Approver - Account,</p>
                                            <p>The following <strong>Miscellaneous Claim</strong> has been approved by <strong>HR & ADMIN</strong> and is now awaiting your review.</p>

                                            <p><u>Claim Details:</u></p>
                                            <ul>
                                                <li><strong>Requester:</strong> {requesterName}</li>
                                                <li><strong>Claim Type:</strong> {expenType}</li>
                                                <li><strong>Serial No:</strong> {serialNo}</li>
                                                <li><strong>Submission Date:</strong> {formattedDate}</li>
                                            </ul>

                                            <p>Please log in to the system to <strong>approve</strong> or <strong>reject</strong> this claim.</p>

                                            <p>Thank you,<br/>HEM Admin Accessibility</p>
                                        ";

                                        foreach (var email in accountApproverEmails)///hemacc2@hosiden.com
                                        {
                                            SendEmail(email, subject, body);
                                        }

                                        MessageBox.Show("Notification sent to First-Level Account approvers.", "Notification Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Failed to approve the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error approving Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }

                // Check if the order is already approved by HR
                if (hrApprovalStatus == "Approved")
                {
                    MessageBox.Show("This Miscellaneous Claim has already been approved by HR.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm HR approval with the user
                DialogResult resultHR = MessageBox.Show($"Are you sure you want to approve Miscellaneous Claim for Serial No: {serialNo} as HR?", "Confirm HR Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resultHR != DialogResult.Yes)
                {
                    return;
                }

                // Update the database for HR approval
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                    {
                        con.Open();
                        string query = @"
                                        UPDATE tbl_MasterClaimForm 
                                        SET HRApprovalStatus = @HRApprovalStatus, 
                                            ApprovedByHR = @ApprovedByHR, 
                                            HRApprovedDate = @HRApprovedDate 
                                        WHERE SerialNo = @SerialNo AND HRApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@HRApprovalStatus", "Approved");
                            cmd.Parameters.AddWithValue("@ApprovedByHR", UserSession.loggedInName);
                            cmd.Parameters.AddWithValue("@HRApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Miscellaneous Claim approved successfully by HR.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadData();

                                // Step 2: Get 1st level Account approver(s)
                                List<string> accountApproverEmails = new List<string>();
                                string getApproversQuery = @"
                                                    SELECT B.Email 
                                                    FROM tbl_Users A
                                                    LEFT JOIN tbl_UserDetail B ON A.IndexNo = B.IndexNo
                                                    LEFT JOIN tbl_UsersLevel C ON A.Position = C.TitlePosition
                                                    WHERE A.Department = 'ACCOUNT' AND C.AccessLevel = '99'";  // First level account approver

                                using (SqlCommand cmd5 = new SqlCommand(getApproversQuery, con))
                                using (SqlDataReader reader = cmd5.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string email = reader["Email"]?.ToString();
                                        if (!string.IsNullOrWhiteSpace(email))
                                        {
                                            accountApproverEmails.Add(email);
                                        }
                                    }
                                }

                                // Step 3: Get claim details for email body
                                string requesterName = "";
                                string expenType = "";
                                DateTime requestDate = DateTime.MinValue;

                                string getClaimDetailsQuery = @"SELECT Requester, ExpensesType, RequestDate 
                                        FROM tbl_MasterClaimForm 
                                        WHERE SerialNo = @SerialNo";

                                using (SqlCommand cmd6 = new SqlCommand(getClaimDetailsQuery, con))
                                {
                                    cmd6.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                    using (SqlDataReader reader = cmd6.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            requesterName = reader["Requester"]?.ToString();
                                            expenType = reader["ExpensesType"]?.ToString();
                                            requestDate = reader["RequestDate"] != DBNull.Value
                                                ? Convert.ToDateTime(reader["RequestDate"])
                                                : DateTime.MinValue;
                                        }
                                    }
                                }

                                // Step 4: Send email to 1st level Account approvers
                                if (accountApproverEmails.Count > 0)
                                {
                                    string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                    string subject = "HEM Admin Accessibility Notification: New Miscellaneous Claim Awaiting For Your Review And Approval";

                                    string body = $@"
                                            <p>Dear Approver - Account,</p>
                                            <p>The following <strong>Miscellaneous Claim</strong> has been approved by <strong>HR & ADMIN</strong> and is now awaiting your review.</p>

                                            <p><u>Claim Details:</u></p>
                                            <ul>
                                                <li><strong>Requester:</strong> {requesterName}</li>
                                                <li><strong>Claim Type:</strong> {expenType}</li>
                                                <li><strong>Serial No:</strong> {serialNo}</li>
                                                <li><strong>Submission Date:</strong> {formattedDate}</li>
                                            </ul>

                                            <p>Please log in to the system to <strong>approve</strong> or <strong>reject</strong> this claim.</p>

                                            <p>Thank you,<br/>HEM Admin Accessibility</p>
                                        ";

                                    foreach (var email in accountApproverEmails)///hemacc2@hosiden.com
                                    {
                                        SendEmail(email, subject, body);
                                    }

                                    MessageBox.Show("Notification sent to First-Level Account approvers.", "Notification Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Failed to approve the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error approving Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            ///+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  HOD                 +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            else
            {
                // Check if the user is trying to approve their own claim
                if (requester == UserSession.loggedInName)
                {
                    MessageBox.Show("You cannot approve your own Miscellaneous Claim.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Extract requester's department from SerialNo (e.g., "HR & ADMIN" from "HR & ADMIN_02072025_3")
                string requesterDepartment = serialNo.Split('_')[0].Trim();
                if (((UserSession.loggedInDepart != requesterDepartment) && requesterDepartment != "EDP" && requesterDepartment != "HR & ADMIN" && requesterDepartment != "ACCOUNT" && requesterDepartment != "FACILITY") && (UserSession.loggedInDepart == "GENERAL AFFAIRS" && UserSession.loggedInDepart != requesterDepartment))
                {
                    MessageBox.Show($"You are not authorized to approve this Miscellaneous Claim. Only HOD from {requesterDepartment} department can approve.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the order is already approved by HOD
                if (hodApprovalStatus == "Approved")
                {
                    MessageBox.Show("This Miscellaneous Claim has already been approved by HOD.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm HOD approval with the user
                DialogResult result = MessageBox.Show($"Are you sure you want to approve Miscellaneous Claim for Serial No: {serialNo} as HOD?", "Confirm HOD Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                    {
                        con.Open();

                        // Step 1: Update the HOD approval status
                        string updateQuery = "";
                        if (requester == "Normala")
                        {
                            updateQuery = @"
                                            UPDATE tbl_MasterClaimForm 
                                            SET HODApprovalStatus = @HODApprovalStatus, 
                                                ApprovedByHOD = @ApprovedByHOD, 
                                                HODApprovedDate = @HODApprovedDate,
                                                HRApprovalStatus = @HRApprovalStatus, 
                                                ApprovedByHR = @ApprovedByHR, 
                                                HRApprovedDate = @HRApprovedDate 

                                            WHERE SerialNo = @SerialNo AND HODApprovalStatus = 'Pending'";
                        }
                        else
                        {
                            updateQuery = @"
                                            UPDATE tbl_MasterClaimForm 
                                            SET HODApprovalStatus = @HODApprovalStatus, 
                                                ApprovedByHOD = @ApprovedByHOD, 
                                                HODApprovedDate = @HODApprovedDate 

                                            WHERE SerialNo = @SerialNo AND HODApprovalStatus = 'Pending'";
                        }
                        

                        using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@HODApprovalStatus", "Approved");
                            cmd.Parameters.AddWithValue("@ApprovedByHOD", UserSession.loggedInName);
                            cmd.Parameters.AddWithValue("@HODApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            if (requester == "Normala")
                            {
                                cmd.Parameters.AddWithValue("@HRApprovalStatus", "Approved");
                                cmd.Parameters.AddWithValue("@ApprovedByHR", requester);
                                cmd.Parameters.AddWithValue("@HRApprovedDate", DateTime.Now);
                            }

                                


                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected <= 0)
                            {
                                MessageBox.Show("Failed to approve the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        MessageBox.Show("Miscellaneous claim approved successfully by HOD.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();

                        //////////////////////// +++++++++++++++++++++++++++++++++++++++++++++++++++++           Email Noti HOD to Acc           +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++  
                        if (expensesType == "Work")
                        {
                            // Step 2: Get 1st level Account approver(s)
                            List<string> accountApproverEmails = new List<string>();
                            string getApproversQuery = @"
                                                    SELECT B.Email 
                                                    FROM tbl_Users A
                                                    LEFT JOIN tbl_UserDetail B ON A.IndexNo = B.IndexNo
                                                    LEFT JOIN tbl_UsersLevel C ON A.Position = C.TitlePosition
                                                    WHERE A.Department = 'ACCOUNT' AND C.AccessLevel = '99'";  // First level account approver

                            using (SqlCommand cmd = new SqlCommand(getApproversQuery, con))
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string email = reader["Email"]?.ToString();
                                    if (!string.IsNullOrWhiteSpace(email))
                                    {
                                        accountApproverEmails.Add(email);
                                    }
                                }
                            }

                            // Step 3: Get claim details for email body
                            string requesterName = "";
                            string expenType = "";
                            DateTime requestDate = DateTime.MinValue;

                            string getClaimDetailsQuery = @"SELECT Requester, ExpensesType, RequestDate 
                                        FROM tbl_MasterClaimForm 
                                        WHERE SerialNo = @SerialNo";

                            using (SqlCommand cmd = new SqlCommand(getClaimDetailsQuery, con))
                            {
                                cmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        requesterName = reader["Requester"]?.ToString();
                                        expenType = reader["ExpensesType"]?.ToString();
                                        requestDate = reader["RequestDate"] != DBNull.Value
                                            ? Convert.ToDateTime(reader["RequestDate"])
                                            : DateTime.MinValue;
                                    }
                                }
                            }

                            // Step 4: Send email to 1st level Account approvers
                            if (accountApproverEmails.Count > 0)
                            {
                                string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                string subject = "HEM Admin Accessibility Notification: New Miscellaneous Claim Awaiting For Your Review And Approval";

                                string body = $@"
                                            <p>Dear Approver - Account,</p>
                                            <p>The following <strong>Miscellaneous Claim</strong> has been approved by <strong>HOD</strong> and is now awaiting your review.</p>

                                            <p><u>Claim Details:</u></p>
                                            <ul>
                                                <li><strong>Requester:</strong> {requesterName}</li>
                                                <li><strong>Claim Type:</strong> {expenType}</li>
                                                <li><strong>Serial No:</strong> {serialNo}</li>
                                                <li><strong>Submission Date:</strong> {formattedDate}</li>
                                            </ul>

                                            <p>Please log in to the system to <strong>approve</strong> or <strong>reject</strong> this claim.</p>

                                            <p>Thank you,<br/>HEM Admin Accessibility</p>
                                        ";

                                foreach (var email in accountApproverEmails)///hemacc2@hosiden.com
                                {
                                    SendEmail(email, subject, body);
                                }

                                MessageBox.Show("Notification sent to 1st-Level Account approvers.", "Notification Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                        else if (expensesType == "Benefit")
                        {
                            // Step 2: Get 1st level Account approver(s)
                            List<string> accountApproverEmails = new List<string>();
                            string getApproversQuery = @"
                                                        SELECT A.Department, A.Username, B.Email, C.AccessLevel
                                                        FROM tbl_Users A
                                                        LEFT JOIN tbl_UserDetail B ON A.IndexNo = B.IndexNo
                                                        LEFT JOIN tbl_UsersLevel C ON A.Position = C.TitlePosition
                                                        WHERE Department = 'HR & ADMIN' AND AccessLevel > 1 AND AccessLevel < 7";   // First level account approver normala@hosiden.com.my  NurSyahir@hosiden.com.my

                            using (SqlCommand cmd = new SqlCommand(getApproversQuery, con))
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string email = reader["Email"]?.ToString();
                                    if (!string.IsNullOrWhiteSpace(email))
                                    {
                                        accountApproverEmails.Add(email);
                                    }
                                }
                            }

                            // Step 3: Get claim details for email body
                            string requesterName = "";
                            string expenType = "";
                            DateTime requestDate = DateTime.MinValue;

                            string getClaimDetailsQuery = @"SELECT Requester, ExpensesType, RequestDate 
                                        FROM tbl_MasterClaimForm 
                                        WHERE SerialNo = @SerialNo";

                            using (SqlCommand cmd = new SqlCommand(getClaimDetailsQuery, con))
                            {
                                cmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        requesterName = reader["Requester"]?.ToString();
                                        expenType = reader["ExpensesType"]?.ToString();
                                        requestDate = reader["RequestDate"] != DBNull.Value
                                            ? Convert.ToDateTime(reader["RequestDate"])
                                            : DateTime.MinValue;
                                    }
                                }
                            }

                            //
                            if (accountApproverEmails.Count > 0)
                            {
                                string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                string subject = "HEM Admin Accessibility Notification: New Miscellaneous Claim Awaiting For Your Review And Approval";
                                string body = "";

                                if(requesterName == "Normala")
                                {
                                     body = $@"
                                            <p>Dear Approver - Account,</p>
                                            <p>The following <strong>Miscellaneous Claim</strong> has been approved by <strong>HR & ADMIN</strong> and is now awaiting your review.</p>

                                            <p><u>Claim Details:</u></p>
                                            <ul>
                                                <li><strong>Requester:</strong> {requesterName}</li>
                                                <li><strong>Claim Type:</strong> {expenType}</li>
                                                <li><strong>Serial No:</strong> {serialNo}</li>
                                                <li><strong>Submission Date:</strong> {formattedDate}</li>
                                            </ul>

                                          <p>Please log in to the system to <strong>approve</strong> or <strong>reject</strong> this claim.</p>

                                            <p>Thank you,<br/>HEM Admin Accessibility</p>
                                        ";

                                    foreach (var email in accountApproverEmails)
                                    {
                                        SendEmail("k-sumi@hosiden.com", subject, body);
                                       // SendEmail("syazwanbunander1997@gmail.com", subject, body);
                                    }

                                    MessageBox.Show("Notification sent to First-Level Account approvers.", "Notification Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    body = $@"
                                            <p>Dear Approver - HR,</p>
                                            <p>The following <strong>Miscellaneous Claim</strong> has been approved by <strong>HOD</strong> and is now awaiting your review.</p>

                                            <p><u>Claim Details:</u></p>
                                            <ul>
                                                <li><strong>Requester:</strong> {requesterName}</li>
                                                <li><strong>Claim Type:</strong> {expenType}</li>
                                                <li><strong>Serial No:</strong> {serialNo}</li>
                                                <li><strong>Submission Date:</strong> {formattedDate}</li>
                                            </ul>

                                            <p>Please log in to the system to <strong>approve</strong> or <strong>reject</strong> this claim.</p>

                                            <p>Thank you,<br/>HEM Admin Accessibility</p>
                                        ";

                                    foreach (var email in accountApproverEmails)
                                    {
                                        SendEmail(email, subject, body);
                                    }

                                    MessageBox.Show("Notification sent to HR & ADMIN approvers.", "Notification Sent", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                }

                            }
                        }


                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error approving Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }
        private void btnReject_Click(object sender, EventArgs e)
        {
            // Check if a cell is selected in the DataGridView
            if (dgvMS.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the Miscellaneous Claim row to reject.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the selected row
            DataGridViewCell selectedCell = dgvMS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string serialNo = selectedRow.Cells["SerialNo"].Value?.ToString();
            string requester = selectedRow.Cells["Requester"].Value?.ToString();
            string hodApprovalStatus = selectedRow.Cells["HODApprovalStatus"].Value?.ToString();
            string hrApprovalStatus = selectedRow.Cells["HRApprovalStatus"].Value?.ToString();

            string account2ApprovalStatus = selectedRow.Cells["Account2ApprovalStatus"].Value?.ToString();
            string account3ApprovalStatus = selectedRow.Cells["Account3ApprovalStatus"].Value?.ToString();
            string accountApprovalStatus = selectedRow.Cells["AccountApprovalStatus"].Value?.ToString();

            string expensesType = selectedRow.Cells["ExpensesType"].Value?.ToString();
            string department = selectedRow.Cells["Department"].Value?.ToString();

            // Validate the selection
            if (string.IsNullOrEmpty(serialNo) || string.IsNullOrEmpty(requester))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if AccountApprovalStatus is Approved (no one can reject if Account has approved)
            if (accountApprovalStatus == "Approved")
            {
                MessageBox.Show("This Miscellaneous Claim cannot be rejected because it has been approved by Account.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (requester == UserSession.loggedInName)
            {
                MessageBox.Show("You cannot reject your own Miscellaneous Claim.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Get user access level from tbl_UsersLevel by joining with tbl_Users
            int userAccessLevel = 2;
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = @"
                                    SELECT ul.AccessLevel 
                                    FROM tbl_UsersLevel ul
                                    LEFT JOIN tbl_Users u ON ul.TitlePosition = u.Position
                                    WHERE u.Name1 = @Name1";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Name1", UserSession.loggedInName);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            userAccessLevel = Convert.ToInt32(result);
                        }
                        else
                        {
                            MessageBox.Show("User access level not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving user access level: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Handle ACCOUNT department rejection
            if (UserSession.loggedInDepart == "ACCOUNT" || UserSession.loggedInDepart == "GENERAL AFFAIRS" && hodApprovalStatus == "Approved")
          //if ((UserSession.loggedInDepart == "ACCOUNT" || UserSession.loggedInDepart == "GENERAL AFFAIRS"))
            {
                // Handle Account2ApprovalStatus (AccessLevel 99)
                if (userAccessLevel == 99)
                {

                    // Check if  HOD ApprovalStatus is Approved   -- Add on 19/11/25
                    if (hodApprovalStatus == "Pending")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be rejected because HOD approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if HODApprovalStatus is Rejected (no need to reject if already rejected)
                    if (hodApprovalStatus == "Rejected" || hrApprovalStatus == "Rejected")
                    {
                        MessageBox.Show("This Miscellaneous Claim has already been rejected by a previous department.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if Account2ApprovalStatus is Approved
                    if (account2ApprovalStatus == "Approved")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be rejected by 1st-Level Account because it has already been approved by 1st-Level Account.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (hrApprovalStatus == "Pending")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be rejected by 1st-Level Account because HR & ADMIN approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                    if (hrApprovalStatus == "Rejected")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be approved by 1st-Level Account because HR & ADMIN approval was Rejected.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    // Check if Account2ApprovalStatus is Rejected
                    if (account2ApprovalStatus == "Rejected")
                    {
                        MessageBox.Show("This Miscellaneous Claim has already been rejected by 1st-Level Account.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Confirm Account2 rejection
                    DialogResult result = MessageBox.Show($"Are you sure you want to reject Miscellaneous Claim for Serial No: {serialNo} as 1st-Level Account?", "Confirm 1st-Level Account Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    // Update database for Account2 rejection
                    try
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                        {
                            con.Open();
                            string query = @"
                                        UPDATE tbl_MasterClaimForm 
                                        SET Account2ApprovalStatus = @Account2ApprovalStatus, 
                                            ApprovedByAccount2 = @ApprovedByAccount2, 
                                            Account2ApprovedDate = @Account2ApprovedDate 
                                        WHERE SerialNo = @SerialNo AND Account2ApprovalStatus = 'Pending'";
                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@Account2ApprovalStatus", "Rejected");
                                cmd.Parameters.AddWithValue("@ApprovedByAccount2", UserSession.loggedInName);
                                cmd.Parameters.AddWithValue("@Account2ApprovedDate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Miscellaneous Claim rejected successfully by 1st-Level Account.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                    //=+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                    string requesterName = "";
                                    string expenType = "";
                                    string indexNum = "";
                                    string userEmails = "";
                                    DateTime requestDate = DateTime.MinValue;

                                    string getClaimDetailsQuery = @"
                                                                    SELECT A.Requester, A.ExpensesType, A.RequestDate, A.EmpNo, B.Email
                                                                    FROM tbl_MasterClaimForm A
                                                                    LEFT JOIN tbl_UserDetail B ON A.EmpNo = B.IndexNo
                                                                    WHERE SerialNo = @SerialNo";
                                    List<string> approverEmails = new List<string>();

                                    using (SqlCommand emailCmd = new SqlCommand(getClaimDetailsQuery, con))
                                    {
                                        emailCmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                        using (SqlDataReader reader = emailCmd.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                string email = reader["Email"]?.ToString();
                                                if (!string.IsNullOrEmpty(email))
                                                {
                                                    approverEmails.Add(email);
                                                }
                                                requesterName = reader["Requester"]?.ToString();
                                                expenType = reader["ExpensesType"]?.ToString();
                                                indexNum = reader["EmpNo"]?.ToString();
                                                userEmails = reader["Email"]?.ToString();
                                                requestDate = reader["RequestDate"] != DBNull.Value
                                                    ? Convert.ToDateTime(reader["RequestDate"])
                                                    : DateTime.MinValue;
                                            }
                                        }
                                    }

                                    if (approverEmails.Count > 0)
                                    {
                                        string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                        string subject = "HEM Admin Accessibility Notification: Your Miscellaneous Claim Has Been Rejected!";
                                        string body = $@"
                                                    <p>Dear Mr./Ms. {requesterName},</p>
                                                    <p>We regret to inform you that your <strong>Miscellaneous Claim</strong> 
                                                    (submitted on <strong>{formattedDate}</strong>) has been <strong>rejected</strong> 
                                                    by Mr./Ms. <strong>{UserSession.loggedInName}</strong>.</p>
                    
                                                <p><u>Claim Details:</u></p>
                                                <ul>
                                                    <li><strong>Requester:</strong> {requesterName}</li>
                                                    <li><strong>Claim Type:</strong> {expenType}</li>
                                                    <li><strong>Serial No:</strong> {serialNo}</li>
                                                    <li><strong>Submission Date:</strong> {formattedDate}</li>
                                                </ul>

                                                <p>For more details, you may reach out to <strong>{UserSession.loggedInName}</strong> from the <strong>{UserSession.loggedInDepart}</strong> Department.</p>
   

                                                    <p>Thank you,<br/>HEM Admin Accessibility</p>
                                                ";

                                        foreach (var email in approverEmails)
                                        {
                                            SendEmail(email, subject, body);
                                        }

                                        MessageBox.Show(
                                            "Notification has been sent to the requester confirming the Miscellaneous Claim status.",
                                            "Notification Sent",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information
                                        );
                                    }

                                    //=++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                }
                                else
                                {
                                    MessageBox.Show("Failed to reject the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error rejecting Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                // Handle Account3ApprovalStatus (AccessLevel 1)
                else if (userAccessLevel == 1)
                {

                    // Check if  HOD ApprovalStatus is Approved    -- Add on 19/11/25
                    if (hodApprovalStatus == "Pending")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be rejected because HOD approval is Pending.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }


                    // Check if HODApprovalStatus or Account2ApprovalStatus is Rejected
                    if (hodApprovalStatus == "Rejected" || hrApprovalStatus == "Rejected" || account2ApprovalStatus == "Rejected")
                    {
                        MessageBox.Show("This Miscellaneous Claim has already been rejected by a previous department.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if Account3ApprovalStatus is Approved
                    if (account3ApprovalStatus == "Approved")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be rejected because it has already been approved by 2nd-Level Account.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if Account3ApprovalStatus is Rejected
                    if (account3ApprovalStatus == "Rejected")
                    {
                        MessageBox.Show("This Miscellaneous Claim has already been rejected by 2nd-Level Account.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Confirm Account3 rejection
                    DialogResult result = MessageBox.Show($"Are you sure you want to reject Miscellaneous Claim for Serial No: {serialNo} as 2nd-Level Account?", "Confirm 2nd-Level Account Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    // Update database for Account3 rejection
                    try
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                        {
                            con.Open();
                            string query = @"
                                            UPDATE tbl_MasterClaimForm 
                                            SET Account3ApprovalStatus = @Account3ApprovalStatus, 
                                                ApprovedByAccount3 = @ApprovedByAccount3, 
                                                Account3ApprovedDate = @Account3ApprovedDate 
                                            WHERE SerialNo = @SerialNo AND Account3ApprovalStatus = 'Pending'";
                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@Account3ApprovalStatus", "Rejected");
                                cmd.Parameters.AddWithValue("@ApprovedByAccount3", UserSession.loggedInName);
                                cmd.Parameters.AddWithValue("@Account3ApprovedDate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Miscellaneous Claim rejected successfully by 2nd-Level Account.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();

                                    //=+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                    string requesterName = "";
                                    string expenType = "";
                                    string indexNum = "";
                                    string userEmails = "";
                                    DateTime requestDate = DateTime.MinValue;

                                    string getClaimDetailsQuery = @"
                                                                    SELECT A.Requester, A.ExpensesType, A.RequestDate, A.EmpNo, B.Email
                                                                    FROM tbl_MasterClaimForm A
                                                                    LEFT JOIN tbl_UserDetail B ON A.EmpNo = B.IndexNo
                                                                    WHERE SerialNo = @SerialNo";
                                    List<string> approverEmails = new List<string>();

                                    using (SqlCommand emailCmd = new SqlCommand(getClaimDetailsQuery, con))
                                    {
                                        emailCmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                        using (SqlDataReader reader = emailCmd.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                string email = reader["Email"]?.ToString();
                                                if (!string.IsNullOrEmpty(email))
                                                {
                                                    approverEmails.Add(email);
                                                }
                                                requesterName = reader["Requester"]?.ToString();
                                                expenType = reader["ExpensesType"]?.ToString();
                                                indexNum = reader["EmpNo"]?.ToString();
                                                userEmails = reader["Email"]?.ToString();
                                                requestDate = reader["RequestDate"] != DBNull.Value
                                                    ? Convert.ToDateTime(reader["RequestDate"])
                                                    : DateTime.MinValue;
                                            }
                                        }
                                    }

                                    if (approverEmails.Count > 0)
                                    {
                                        string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                        string subject = "HEM Admin Accessibility Notification: Your Miscellaneous Claim Has Been Rejected!";
                                        string body = $@"
                                                    <p>Dear Mr./Ms. {requesterName},</p>
                                                    <p>We regret to inform you that your <strong>Miscellaneous Claim</strong> 
                                                    (submitted on <strong>{formattedDate}</strong>) has been <strong>rejected</strong> 
                                                    by Mr./Ms. <strong>{UserSession.loggedInName}</strong>.</p>
                    
                                                <p><u>Claim Details:</u></p>
                                                <ul>
                                                    <li><strong>Requester:</strong> {requesterName}</li>
                                                    <li><strong>Claim Type:</strong> {expenType}</li>
                                                    <li><strong>Serial No:</strong> {serialNo}</li>
                                                    <li><strong>Submission Date:</strong> {formattedDate}</li>
                                                </ul>

                                                <p>For more details, you may reach out to <strong>{UserSession.loggedInName}</strong> from the <strong>{UserSession.loggedInDepart}</strong> Department.</p>
   

                                                    <p>Thank you,<br/>HEM Admin Accessibility</p>
                                                ";

                                        foreach (var email in approverEmails)
                                        {
                                            SendEmail(email, subject, body);
                                        }

                                        MessageBox.Show(
                                            "Notification has been sent to the requester confirming the Miscellaneous Claim status.",
                                            "Notification Sent",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information
                                        );
                                    }

                                    //=++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                }
                                else
                                {
                                    MessageBox.Show("Failed to reject the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error rejecting Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                // Handle AccountApprovalStatus (AccessLevel 3)
                else if (userAccessLevel == 3)
                {
                    // Check if loggedInDepart is GENERAL AFFAIRS
                    if (UserSession.loggedInDepart != "GENERAL AFFAIRS")
                    {
                        MessageBox.Show("Only users from GENERAL AFFAIRS can reject this Miscellaneous Claim.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //----Add on 19 / 11 / 25
                    if (hodApprovalStatus == "Pending")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be rejected because HOD approval is Pending.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if HODApprovalStatus, HRApprovalStatus, Account2ApprovalStatus, or Account3ApprovalStatus is Rejected
                    if (hodApprovalStatus == "Rejected" || hrApprovalStatus == "Rejected" ||
                        account2ApprovalStatus == "Rejected" || account3ApprovalStatus == "Rejected")
                    {
                        MessageBox.Show("This Miscellaneous Claim has already been rejected by a previous department.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if AccountApprovalStatus is Approved
                    if (accountApprovalStatus == "Approved")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be rejected by Account because it has already been approved by 3rd-Level Account.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if Account 2 ApprovalStatus is pending
                    if (account2ApprovalStatus == "Pending")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be rejected by Account because 2nd-Level Account still did not verify approve/reject.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }


                    // Check if AccountApprovalStatus is Rejected
                    if (accountApprovalStatus == "Rejected")
                    {
                        MessageBox.Show("This Miscellaneous Claim has already been rejected by 3rd-Level Account.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Confirm Account rejection
                    DialogResult result = MessageBox.Show($"Are you sure you want to reject Miscellaneous Claim for Serial No: {serialNo} as 3rd-Level Account?", "Confirm 3rd-Level Account Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    // Update database for Account rejection
                    try
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                        {
                            con.Open();
                            string query = @"
                                            UPDATE tbl_MasterClaimForm 
                                            SET AccountApprovalStatus = @AccountApprovalStatus, 
                                                ApprovedByAccount = @ApprovedByAccount, 
                                                AccountApprovedDate = @AccountApprovedDate 
                                            WHERE SerialNo = @SerialNo AND AccountApprovalStatus = 'Pending'";
                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.AddWithValue("@AccountApprovalStatus", "Rejected");
                                cmd.Parameters.AddWithValue("@ApprovedByAccount", UserSession.loggedInName);
                                cmd.Parameters.AddWithValue("@AccountApprovedDate", DateTime.Now);
                                cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Miscellaneous Claim rejected successfully by 3rd-Level Account.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                    //=+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                    string requesterName = "";
                                    string expenType = "";
                                    string indexNum = "";
                                    string userEmails = "";
                                    DateTime requestDate = DateTime.MinValue;

                                    string getClaimDetailsQuery = @"
                                                                    SELECT A.Requester, A.ExpensesType, A.RequestDate, A.EmpNo, B.Email
                                                                    FROM tbl_MasterClaimForm A
                                                                    LEFT JOIN tbl_UserDetail B ON A.EmpNo = B.IndexNo
                                                                    WHERE SerialNo = @SerialNo";
                                    List<string> approverEmails = new List<string>();

                                    using (SqlCommand emailCmd = new SqlCommand(getClaimDetailsQuery, con))
                                    {
                                        emailCmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                        using (SqlDataReader reader = emailCmd.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                string email = reader["Email"]?.ToString();
                                                if (!string.IsNullOrEmpty(email))
                                                {
                                                    approverEmails.Add(email);
                                                }
                                                requesterName = reader["Requester"]?.ToString();
                                                expenType = reader["ExpensesType"]?.ToString();
                                                indexNum = reader["EmpNo"]?.ToString();
                                                userEmails = reader["Email"]?.ToString();
                                                requestDate = reader["RequestDate"] != DBNull.Value
                                                    ? Convert.ToDateTime(reader["RequestDate"])
                                                    : DateTime.MinValue;
                                            }
                                        }
                                    }

                                    if (approverEmails.Count > 0)
                                    {
                                        string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                        string subject = "HEM Admin Accessibility Notification: Your Miscellaneous Claim Has Been Rejected!";
                                        string body = $@"
                                                    <p>Dear Mr./Ms. {requesterName},</p>
                                                    <p>We regret to inform you that your <strong>Miscellaneous Claim</strong> 
                                                    (submitted on <strong>{formattedDate}</strong>) has been <strong>rejected</strong> 
                                                    by Mr./Ms. <strong>{UserSession.loggedInName}</strong>.</p>
                    
                                                <p><u>Claim Details:</u></p>
                                                <ul>
                                                    <li><strong>Requester:</strong> {requesterName}</li>
                                                    <li><strong>Claim Type:</strong> {expenType}</li>
                                                    <li><strong>Serial No:</strong> {serialNo}</li>
                                                    <li><strong>Submission Date:</strong> {formattedDate}</li>
                                                </ul>

                                                <p>For more details, you may reach out to <strong>{UserSession.loggedInName}</strong> from the <strong>{UserSession.loggedInDepart}</strong> Department.</p>
   

                                                    <p>Thank you,<br/>HEM Admin Accessibility</p>
                                                ";

                                        foreach (var email in approverEmails)
                                        {
                                            SendEmail(email, subject, body);
                                        }

                                        MessageBox.Show(
                                            "Notification has been sent to the requester confirming the Miscellaneous Claim status.",
                                            "Notification Sent",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information
                                        );
                                    }

                                    //=++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                }
                                else
                                {
                                    MessageBox.Show("Failed to reject the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error rejecting Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("You do not have the required access level to reject this Miscellaneous Claim.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            // Handle HR & ADMIN department rejection
            else if (UserSession.loggedInDepart == "HR & ADMIN")
            {
  
                // Check if ExpensesType is Work
                if (expensesType == "Work" && department != "HR & ADMIN")
                {
                    MessageBox.Show("HR & ADMIN cannot reject Work-related expenses.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Check if HODApprovalStatus is Pending
                if (hodApprovalStatus == "Pending" && department != "HR & ADMIN")
                {
                    MessageBox.Show("This Miscellaneous Claim cannot be rejected by HR because HOD approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Check if HODApprovalStatus is Rejected
                if (hodApprovalStatus == "Rejected")
                {
                    MessageBox.Show("This Miscellaneous Claim has already been rejected by HOD.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Handle HR & ADMIN acting as HOD for their own department
                if (department == "HR & ADMIN" && hodApprovalStatus == "Pending" && (expensesType == "Work" || expensesType == "Benefit"))
                {
                    // Check if HRApprovalStatus is Approved (HR & ADMIN also handles HR approval)
                    if (hrApprovalStatus == "Approved")
                    {
                        MessageBox.Show("This Miscellaneous Claim cannot be rejected by HR because it has already been approved by HR.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if HRApprovalStatus is Rejected
                    if (hrApprovalStatus == "Rejected")
                    {
                        MessageBox.Show("This Miscellaneous Claim has already been rejected by HR.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Confirm HR rejection
                    DialogResult result = MessageBox.Show($"Are you sure you want to reject Miscellaneous Claim for Serial No: {serialNo} as HOD?", "Confirm HOD Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes)
                    {
                        return;
                    }

                    // Update database for HR rejection (also updates HOD fields for HR & ADMIN department)
                    try
                    {
                        using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                        {
                            con.Open();
                            string query11 = @"
                                            UPDATE tbl_MasterClaimForm 
                                            SET HODApprovalStatus = @HODApprovalStatus, 
                                                ApprovedByHOD = @ApprovedByHOD, 
                                                HODApprovedDate = @HODApprovedDate
                                            WHERE SerialNo = @SerialNo AND HODApprovalStatus = 'Pending' AND HRApprovalStatus = 'Pending'";
                            using (SqlCommand cmd11 = new SqlCommand(query11, con))
                            {
                                cmd11.Parameters.AddWithValue("@HODApprovalStatus", "Rejected");
                                cmd11.Parameters.AddWithValue("@ApprovedByHOD", UserSession.loggedInName);
                                cmd11.Parameters.AddWithValue("@HODApprovedDate", DateTime.Now);
                                //cmd11.Parameters.AddWithValue("@HRApprovalStatus", "Rejected");
                                //cmd11.Parameters.AddWithValue("@ApprovedByHR", LoggedInUser);
                                //cmd11.Parameters.AddWithValue("@HRApprovedDate", DateTime.Now);
                                cmd11.Parameters.AddWithValue("@SerialNo", serialNo);

                                int rowsAffected = cmd11.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Miscellaneous Claim rejected successfully by HOD.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    LoadData();
                                    //=+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                    string requesterName = "";
                                    string expenType = "";
                                    string indexNum = "";
                                    string userEmails = "";
                                    DateTime requestDate = DateTime.MinValue;

                                    string getClaimDetailsQuery = @"
                                                                    SELECT A.Requester, A.ExpensesType, A.RequestDate, A.EmpNo, B.Email
                                                                    FROM tbl_MasterClaimForm A
                                                                    LEFT JOIN tbl_UserDetail B ON A.EmpNo = B.IndexNo
                                                                    WHERE SerialNo = @SerialNo";
                                    List<string> approverEmails = new List<string>();

                                    using (SqlCommand emailCmd = new SqlCommand(getClaimDetailsQuery, con))
                                    {
                                        emailCmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                        using (SqlDataReader reader = emailCmd.ExecuteReader())
                                        {
                                            while (reader.Read())
                                            {
                                                string email = reader["Email"]?.ToString();
                                                if (!string.IsNullOrEmpty(email))
                                                {
                                                    approverEmails.Add(email);
                                                }
                                                requesterName = reader["Requester"]?.ToString();
                                                expenType = reader["ExpensesType"]?.ToString();
                                                indexNum = reader["EmpNo"]?.ToString();
                                                userEmails = reader["Email"]?.ToString();
                                                requestDate = reader["RequestDate"] != DBNull.Value
                                                    ? Convert.ToDateTime(reader["RequestDate"])
                                                    : DateTime.MinValue;
                                            }
                                        }
                                    }

                                    if (approverEmails.Count > 0)
                                    {
                                        string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                        string subject = "HEM Admin Accessibility Notification: Your Miscellaneous Claim Has Been Rejected!";
                                        string body = $@"
                                                    <p>Dear Mr./Ms. {requesterName},</p>
                                                    <p>We regret to inform you that your <strong>Miscellaneous Claim</strong> 
                                                    (submitted on <strong>{formattedDate}</strong>) has been <strong>rejected</strong> 
                                                    by Mr./Ms. <strong>{UserSession.loggedInName}</strong>.</p>
                    
                                                <p><u>Claim Details:</u></p>
                                                <ul>
                                                    <li><strong>Requester:</strong> {requesterName}</li>
                                                    <li><strong>Claim Type:</strong> {expenType}</li>
                                                    <li><strong>Serial No:</strong> {serialNo}</li>
                                                    <li><strong>Submission Date:</strong> {formattedDate}</li>
                                                </ul>

                                                <p>For more details, you may reach out to <strong>{UserSession.loggedInName}</strong> from the <strong>{UserSession.loggedInDepart}</strong> Department.</p>
   

                                                    <p>Thank you,<br/>HEM Admin Accessibility</p>
                                                ";

                                        foreach (var email in approverEmails)
                                        {
                                            SendEmail(email, subject, body);
                                        }

                                        MessageBox.Show(
                                            "Notification has been sent to the requester confirming the Miscellaneous Claim status.",
                                            "Notification Sent",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information
                                        );
                                    }

                                    //=++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                }
                                else
                                {
                                    MessageBox.Show("Failed to reject the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error rejecting Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return;
                }

                // Check if HRApprovalStatus is Approved
                if (hrApprovalStatus == "Approved")
                {
                    MessageBox.Show("This Miscellaneous Claim cannot be rejected by HR because it has already been approved by HR.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (expensesType == "Work" && department == "HR & ADMIN" && (hodApprovalStatus == "Approved" || hodApprovalStatus == "Rejected"))
                {
                    MessageBox.Show("HR & ADMIN cannot reject Work-related expenses.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Check if HRApprovalStatus is Rejected
                if (hrApprovalStatus == "Rejected")
                {
                    MessageBox.Show("This Miscellaneous Claim has already been rejected by HR.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm HR rejection
                DialogResult resultHR = MessageBox.Show($"Are you sure you want to reject Miscellaneous Claim for Serial No: {serialNo} as HR?", "Confirm HR Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (resultHR != DialogResult.Yes)
                {
                    return;
                }

                // Update database for HR rejection
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                    {
                        con.Open();
                        string query = @"
                            UPDATE tbl_MasterClaimForm 
                            SET HRApprovalStatus = @HRApprovalStatus, 
                                ApprovedByHR = @ApprovedByHR, 
                                HRApprovedDate = @HRApprovedDate 
                            WHERE SerialNo = @SerialNo AND HRApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@HRApprovalStatus", "Rejected");
                            cmd.Parameters.AddWithValue("@ApprovedByHR", UserSession.LoggedInUser);
                            cmd.Parameters.AddWithValue("@HRApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Miscellaneous Claim rejected successfully by HR.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Failed to reject the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error rejecting Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                //MessageBox.Show("HR & ADMIN2");
                // Check if the user is trying to reject their own claim
                if (requester == UserSession.LoggedInUser)
                {
                    MessageBox.Show("You cannot reject your own Miscellaneous Claim.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Extract requester's department from SerialNo (e.g., "HR & ADMIN" from "HR & ADMIN_02072025_3")
                string requesterDepartment = serialNo.Split('_')[0].Trim();
                //if (UserSession.loggedInDepart != requesterDepartment)
                if (((UserSession.loggedInDepart != requesterDepartment) && requesterDepartment != "EDP" && requesterDepartment != "HR & ADMIN" && requesterDepartment != "ACCOUNT" && requesterDepartment != "FACILITY") && (UserSession.loggedInDepart == "GENERAL AFFAIRS" && UserSession.loggedInDepart != requesterDepartment))
                {
                    MessageBox.Show($"You are not authorized to reject this Miscellaneous Claim. Only HOD from {requesterDepartment} department can reject.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if HODApprovalStatus is Approved
                if (hodApprovalStatus == "Approved")
                {
                    MessageBox.Show("This Miscellaneous Claim cannot be rejected because it has already been approved by HOD.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if HODApprovalStatus is Rejected
                if (hodApprovalStatus == "Rejected")
                {
                    MessageBox.Show("This Miscellaneous Claim has already been rejected by HOD.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm HOD rejection
                DialogResult result = MessageBox.Show($"Are you sure you want to reject Miscellaneous Claim for Serial No: {serialNo} as HOD?", "Confirm HOD Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Update database for HOD rejection
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                    {
                        con.Open();
                        string query = @"
                                        UPDATE tbl_MasterClaimForm 
                                        SET HODApprovalStatus = @HODApprovalStatus, 
                                            ApprovedByHOD = @ApprovedByHOD, 
                                            HODApprovedDate = @HODApprovedDate 
                                        WHERE SerialNo = @SerialNo AND HODApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@HODApprovalStatus", "Rejected");
                            cmd.Parameters.AddWithValue("@ApprovedByHOD", UserSession.loggedInName);
                            cmd.Parameters.AddWithValue("@HODApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Miscellaneous Claim rejected successfully by HOD.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadData();
                                //=+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                                string requesterName = "";
                                string expenType = "";
                                string indexNum = "";
                                string userEmails = "";
                                DateTime requestDate = DateTime.MinValue;

                                string getClaimDetailsQuery = @"
                                                                    SELECT A.Requester, A.ExpensesType, A.RequestDate, A.EmpNo, B.Email
                                                                    FROM tbl_MasterClaimForm A
                                                                    LEFT JOIN tbl_UserDetail B ON A.EmpNo = B.IndexNo
                                                                    WHERE SerialNo = @SerialNo";
                                List<string> approverEmails = new List<string>();

                                using (SqlCommand emailCmd = new SqlCommand(getClaimDetailsQuery, con))
                                {
                                    emailCmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar).Value = serialNo;

                                    using (SqlDataReader reader = emailCmd.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            string email = reader["Email"]?.ToString();
                                            if (!string.IsNullOrEmpty(email))
                                            {
                                                approverEmails.Add(email);
                                            }
                                            requesterName = reader["Requester"]?.ToString();
                                            expenType = reader["ExpensesType"]?.ToString();
                                            indexNum = reader["EmpNo"]?.ToString();
                                            userEmails = reader["Email"]?.ToString();
                                            requestDate = reader["RequestDate"] != DBNull.Value
                                                ? Convert.ToDateTime(reader["RequestDate"])
                                                : DateTime.MinValue;
                                        }
                                    }
                                }

                                if (approverEmails.Count > 0)
                                {
                                    string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                    string subject = "HEM Admin Accessibility Notification: Your Miscellaneous Claim Has Been Rejected!";
                                    string body = $@"
                                                    <p>Dear Mr./Ms. {requesterName},</p>
                                                    <p>We regret to inform you that your <strong>Miscellaneous Claim</strong> 
                                                    (submitted on <strong>{formattedDate}</strong>) has been <strong>rejected</strong> 
                                                    by Mr./Ms. <strong>{UserSession.loggedInName}</strong>.</p>
                    
                                                <p><u>Claim Details:</u></p>
                                                <ul>
                                                    <li><strong>Requester:</strong> {requesterName}</li>
                                                    <li><strong>Claim Type:</strong> {expenType}</li>
                                                    <li><strong>Serial No:</strong> {serialNo}</li>
                                                    <li><strong>Submission Date:</strong> {formattedDate}</li>
                                                </ul>

                                                <p>For more details, you may reach out to Mr./Ms. <strong>{UserSession.loggedInName}</strong> from the <strong>{UserSession.loggedInDepart}</strong> Department.</p>
   

                                                    <p>Thank you,<br/>HEM Admin Accessibility</p>
                                                ";

                                    foreach (var email in approverEmails)
                                    {
                                        SendEmail(email, subject, body);
                                    }

                                    MessageBox.Show(
                                        "Notification has been sent to the requester confirming the Miscellaneous Claim status.",
                                        "Notification Sent",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information
                                    );
                                }

                                //=++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                            }
                            else
                            {
                                MessageBox.Show("Failed to reject the Miscellaneous Claim. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error rejecting Miscellaneous Claim: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnViewInvoice_Click(object sender, EventArgs e)
        {
            // Check if a cell is selected in the DataGridView
            if (dgvMS.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to view.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Get the selected row
            DataGridViewCell selectedCell = dgvMS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string serialNo = selectedRow.Cells["SerialNo"].Value?.ToString();
            string requester = selectedRow.Cells["Requester"].Value?.ToString();
            string requesterDepartment = selectedRow.Cells["Department"].Value?.ToString();
            string expensesType = selectedRow.Cells["ExpensesType"].Value?.ToString();



            int accessLevelRequester = -1; // default value

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = @"
                    SELECT C.AccessLevel, A.Requester, A.Department, A.ExpensesType, A.HODApprovalStatus,A.HRApprovalStatus, A.Account2ApprovalStatus,A.Account3ApprovalStatus, A.AccountApprovalStatus, B.Position
                    FROM tbl_MasterClaimForm A
                    LEFT JOIN tbl_Users B ON A.Requester = B.Name1
                    LEFT JOIN tbl_UsersLevel C ON B.Position = C.TitlePosition
                    WHERE A.SerialNo = @SerialNo";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SerialNo", serialNo);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            accessLevelRequester = Convert.ToInt32(result);
                        }
                        else
                        {
                            MessageBox.Show("User access level not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving user access level: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Parse current user access level safely
            if (int.TryParse(UserSession.logginInUserAccessLevel, out int currentAccessLevel))
            {
                if (currentAccessLevel < accessLevelRequester && accessLevelRequester != 99 && UserSession.loggedInDepart != "ACCOUNT")
                {
                    if (UserSession.loggedInDepart == "HR & ADMIN" && requesterDepartment == "HR & ADMIN")
                    {
                        MessageBox.Show("You are not authorized to view this report.",
                                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        if (UserSession.loggedInDepart == "GENERAL AFFAIRS")
                        {

                        }
                        else
                        {
                            if (UserSession.loggedInDepart == "HR & ADMIN")
                            {
                                if (requesterDepartment != UserSession.loggedInDepart && expensesType == "Work")
                                {
                                    MessageBox.Show("You are not authorized to view this report.",
                                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                MessageBox.Show("You are not authorized to view this report.",
                                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            
                        }
                            
                    }
                }
                else if (currentAccessLevel < accessLevelRequester &&
                         UserSession.loggedInDepart == "HR & ADMIN" &&
                         requesterDepartment == "HR & ADMIN" && accessLevelRequester != 99)
                {
                    MessageBox.Show("You are not authorized to view this report.",
                                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else if (UserSession.loggedInDepart != requesterDepartment &&
                         UserSession.loggedInDepart == "HR & ADMIN" &&
                         expensesType == "Work")
                {
                    if(requesterDepartment == "ISO")
                    {

                    }
                    else if (requesterDepartment == "FACILITY")
                    {
                        
                    }
                    else
                    {
                        MessageBox.Show("You are not authorized to view this report1.",
                                    "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    
                }


            }
            else
            {
                MessageBox.Show("Invalid access level for logged-in user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Validate the selection
            if (string.IsNullOrEmpty(serialNo) || string.IsNullOrEmpty(requester))
            {
                MessageBox.Show("Invalid order selection: SerialNo or Requester is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // Proceed with generating and viewing the PDF
            //string selectedMeal = cmbType.SelectedItem?.ToString() ?? "DefaultMeal";
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
            SELECT m.SerialNo, u.Name AS Requester, m.EmpNo, m.Department, ud.BankName, ud.AccountNo, m.ExpensesType, m.RequestDate, 
                   m.HODApprovalStatus, m.ApprovedByHOD, m.HODApprovedDate, 
                   m.HRApprovalStatus, m.ApprovedByHR, m.HRApprovedDate, 
	               m.Account2ApprovalStatus, m.ApprovedByAccount2 ,m.Account2ApprovedDate,
	               m.Account3ApprovalStatus, m.ApprovedByAccount3 ,m.Account3ApprovedDate,
                   m.AccountApprovalStatus, m.ApprovedByAccount, m.AccountApprovedDate
            FROM tbl_MasterClaimForm m
            LEFT JOIN tbl_Users u ON m.EmpNo = u.IndexNo
            LEFT JOIN tbl_UserDetail ud ON u.IndexNo = ud.IndexNo
            WHERE m.SerialNo = @SerialNo";
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

                                //1st App Acc
                                orderDetails["Account2ApprovalStatus"] = reader["Account2ApprovalStatus"] != DBNull.Value ? reader["Account2ApprovalStatus"].ToString() : "";
                                orderDetails["ApprovedByAccount2"] = reader["ApprovedByAccount2"] != DBNull.Value ? reader["ApprovedByAccount2"].ToString() : "";
                                orderDetails["Account2ApprovedDate"] = reader["Account2ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["Account2ApprovedDate"]).ToString("dd.MM.yyyy") : "";


                                //2nd App Acc
                                orderDetails["Account3ApprovalStatus"] = reader["Account3ApprovalStatus"] != DBNull.Value ? reader["Account3ApprovalStatus"].ToString() : "";
                                orderDetails["ApprovedByAccount3"] = reader["ApprovedByAccount3"] != DBNull.Value ? reader["ApprovedByAccount3"].ToString() : "";
                                orderDetails["Account3ApprovedDate"] = reader["Account3ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["Account3ApprovedDate"]).ToString("dd.MM.yyyy") : "";

                                //final App Acc
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

                    // Custom page event handler for watermark
                    writer.PageEvent = new WatermarkPageEvent();
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

                    PdfPTable detailsTable = new PdfPTable(3); // 3 columns: Label, Colon, Value
                    detailsTable.WidthPercentage = 100;
                    detailsTable.SetWidths(new float[] { 0.15f, 0.05f, 0.55f }); // Adjust widths as needed
                    detailsTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                    detailsTable.DefaultCell.Padding = 2f;
                    detailsTable.SpacingBefore = 5f;

                    AddDetailRow(detailsTable, "Requester", orderDetails["Requester"].ToString(), bodyFont);
                    AddDetailRow(detailsTable, "Emp No.", orderDetails["EmpNo"].ToString(), bodyFont);
                    AddDetailRow(detailsTable, "Department", orderDetails["Department"].ToString(), bodyFont);
                    AddDetailRow(detailsTable, "Bank name", orderDetails["BankName"].ToString(), bodyFont);
                    AddDetailRow(detailsTable, "Account No.", orderDetails["AccountNo"].ToString(), bodyFont);
                    AddDetailRow(detailsTable, "Request date", Convert.ToDateTime(orderDetails["RequestDate"]).ToString("dd.MM.yyyy"), bodyFont);

                    leftCell.AddElement(detailsTable);

                    PdfPCell rightCell = new PdfPCell();
                    rightCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    rightCell.Padding = 0f;

                    // Create a table for approval sections with proper alignment
                    PdfPTable approvalTable = new PdfPTable(3); // 3 columns: Label, Colon, Value
                    approvalTable.WidthPercentage = 100;
                    approvalTable.SpacingBefore = 10f;

                    // Set column widths to align colons properly
                    float[] columnWidths = new float[] { 50f, 5f, 50f }; // Adjust widths as needed
                    approvalTable.SetWidths(columnWidths);

                    // HOD Approval
                    string ApprovedByHOD = orderDetails["ApprovedByHOD"].ToString();
                    string HODApprovedDate = orderDetails["HODApprovedDate"].ToString();
                    string HODstatus = orderDetails["HODApprovalStatus"].ToString();

                    string hodValue = HODstatus == "Rejected" ?
                        $"Rejected   {(string.IsNullOrEmpty(HODApprovedDate) ? DateTime.Now.ToString("dd.MM.yyyy") : HODApprovedDate)}" :
                        HODstatus == "Approved" ?
                        $"{(string.IsNullOrEmpty(ApprovedByHOD) ? "Approved" : ApprovedByHOD)}   {(string.IsNullOrEmpty(HODApprovedDate) ? DateTime.Now.ToString("dd.MM.yyyy") : HODApprovedDate)}" :
                        "Pending";

                    AddApprovalRow(approvalTable, "HOD Approval", hodValue, bodyFont);

                    // HR Approval (conditionally added)
                    if (orderDetails["ExpensesType"].ToString().ToLower() != "work")
                    {
                        string ApprovedByHR = orderDetails["ApprovedByHR"].ToString();
                        string HRApprovedDate = orderDetails["HRApprovedDate"].ToString();
                        string hrStatus = orderDetails["HRApprovalStatus"].ToString();

                        string hrValue = hrStatus == "Rejected" ?
                            $"Rejected   {(string.IsNullOrEmpty(HRApprovedDate) ? DateTime.Now.ToString("dd.MM.yyyy") : HRApprovedDate)}" :
                            hrStatus == "Approved" ?
                            $"{(string.IsNullOrEmpty(ApprovedByHR) ? "Approved" : ApprovedByHR)}   {(string.IsNullOrEmpty(HRApprovedDate) ? DateTime.Now.ToString("dd.MM.yyyy") : HRApprovedDate)}" :
                            "Pending";

                        AddApprovalRow(approvalTable, "HR Approval", hrValue, bodyFont);
                    }

                    /// 1st Account Approval
                    string ApprovedByAccount1 = orderDetails["ApprovedByAccount2"].ToString();
                    string AccountApprovedDate1 = orderDetails["Account2ApprovedDate"].ToString();
                    string accStatus1 = orderDetails["Account2ApprovalStatus"].ToString();

                    // 2nd Account Approval
                    string ApprovedByAccount2 = orderDetails["ApprovedByAccount3"].ToString();
                    string AccountApprovedDate2 = orderDetails["Account3ApprovedDate"].ToString();
                    string accStatus2 = orderDetails["Account3ApprovalStatus"].ToString();

                    // Final Account Approval
                    string ApprovedByAccount = orderDetails["ApprovedByAccount"].ToString();
                    string AccountApprovedDate = orderDetails["AccountApprovedDate"].ToString();
                    string accStatus = orderDetails["AccountApprovalStatus"].ToString();

                    string displayName = "";
                    string displayDate = "";
                    string displayStatus = "";

                    // --- Determine which stage to show ---
                    if (accStatus1 == "Rejected")
                    {
                        displayStatus = accStatus1;
                        displayName = ApprovedByAccount1;
                        displayDate = AccountApprovedDate1;
                    }
                    else if (accStatus2 == "Rejected")
                    {
                        displayStatus = accStatus2;
                        displayName = ApprovedByAccount2;
                        displayDate = AccountApprovedDate2;
                    }
                    else if (accStatus == "Rejected" || accStatus == "Approved")
                    {
                        displayStatus = accStatus;
                        displayName = ApprovedByAccount;
                        displayDate = AccountApprovedDate;
                    }
                    else
                    {
                        displayStatus = "Pending";
                    }

                    // --- Build the display value ---
                    string accountValue = displayStatus == "Rejected"
                        ? $"Rejected   {(string.IsNullOrEmpty(displayDate) ? DateTime.Now.ToString("dd.MM.yyyy") : displayDate)}"
                        : displayStatus == "Approved"
                            ? $"{(string.IsNullOrEmpty(displayName) ? "Approved" : displayName)}   {(string.IsNullOrEmpty(displayDate) ? DateTime.Now.ToString("dd.MM.yyyy") : displayDate)}"
                            : "Pending";

                    // --- Add row ---
                    AddApprovalRow(approvalTable, "Account Approval", accountValue, bodyFont);

                    // Add the approval table to your document
                    rightCell.AddElement(approvalTable);
                    //AccountApprovalPara.Add(new Chunk($"Account Approval : {(string.IsNullOrEmpty(ApprovedByAccount) ? "Pending" : $"{ApprovedByAccount}   {(string.IsNullOrEmpty(AccountApprovedDate) ? DateTime.Now.ToString("dd.MM.yyyy") : AccountApprovedDate)}")}", bodyFont));


                    // AccountApprovalPara.SpacingBefore = 0f;
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    // Add watermark with logo.png behind Account Approval name and date
                    string imagePath1 = Path.Combine(WinFormsApp.StartupPath, "Img", "logo.png");
                    if (File.Exists(imagePath1) && !string.IsNullOrEmpty(ApprovedByAccount)) // Only add watermark if approved
                    {
                        iTextSharp.text.Image watermark = iTextSharp.text.Image.GetInstance(imagePath1);
                        float xPosition = document.PageSize.Width * 0.75f; // Approximately 70% of page width for right column (e.g., ~420f for A4)
                        float yPosition = document.PageSize.Height - 217f; // Approximate Y position near top of approvals (e.g., ~700f for A4)
                        float width = 80f; // Width to fit behind the text
                        float height = 80f; // Height to fit behind the text
                        watermark.SetAbsolutePosition(xPosition, yPosition);
                        watermark.ScaleToFit(width, height); // Scale to fit behind the text area

                        PdfContentByte under = writer.DirectContentUnder;
                        PdfGState gState = new PdfGState();
                        gState.FillOpacity = 0.05f; // Set opacity to 5% (0.0f to 1.0f)
                        under.SetGState(gState);
                        under.AddImage(watermark);
                    }
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    //rightCell.AddElement(AccountApprovalPara);

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
                        invoiceCell.Padding = 5f; // Add padding to give space around the text

                        string invoicePath = item["Invoice"] as string;
                        if (!string.IsNullOrEmpty(invoicePath) && File.Exists(invoicePath))
                        {
                            Phrase linkPhrase = new Phrase();
                            Anchor invoiceLink = new Anchor("View", linkFont);
                            invoiceLink.Reference = $"file:///{invoicePath.Replace("\\", "/")}";
                            linkPhrase.Add(invoiceLink);
                            invoiceCell.AddElement(linkPhrase); // Add the phrase to the cell
                        }
                        else
                        {
                            invoiceCell.AddElement(new Phrase("No Invoice", bodyFont));
                        }
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
        // Custom page event handler for adding watermark on all pages
        public class WatermarkPageEvent : PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                string imagePath = Path.Combine(WinFormsApp.StartupPath, "Img", "logo.png");
                if (File.Exists(imagePath))
                {
                    iTextSharp.text.Image watermark = iTextSharp.text.Image.GetInstance(imagePath);
                    float pageWidth = document.PageSize.Width;
                    float pageHeight = document.PageSize.Height;
                    float scaleFactor = 0.7f; // Reduce size to 70% of the page dimensions
                    watermark.ScaleToFit(pageWidth * scaleFactor, pageHeight * scaleFactor); // Scale to a smaller size

                    watermark.RotationDegrees = 0; // Rotate for watermark effect

                    // Center the watermark
                    float x = (pageWidth - watermark.ScaledWidth) / 2;
                    float y = (pageHeight - watermark.ScaledHeight) / 2;
                    watermark.SetAbsolutePosition(x, y);

                    PdfContentByte under = writer.DirectContentUnder;
                    PdfGState gState = new PdfGState();
                    gState.FillOpacity = 0.05f; // Set opacity to 5% (0.0f to 1.0f)
                    under.SetGState(gState);
                    under.AddImage(watermark);
                }
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
        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
        void columnSetup1(PdfPTable table, string label1, string value1, string label2, string value2, iTextSharp.text.Font font)
        {
            AddLabelValuePair1(table, label1, value1, font);
            AddLabelValuePair1(table, label2, value2, font);
        }

        void AddLabelValuePair1(PdfPTable table, string label, string value, iTextSharp.text.Font font)
        {
            if (!string.IsNullOrEmpty(label))
            {
                // Label
                PdfPCell labelCell = new PdfPCell(new Phrase(label, font));
                labelCell.HorizontalAlignment = Element.ALIGN_LEFT;
                labelCell.Border = PdfPCell.NO_BORDER;
                labelCell.PaddingRight = 5f;
                table.AddCell(labelCell);

                // Colon
                PdfPCell colonCell = new PdfPCell(new Phrase(":", font));
                colonCell.HorizontalAlignment = Element.ALIGN_CENTER;
                colonCell.Border = PdfPCell.NO_BORDER;
                table.AddCell(colonCell);
            }
            else
            {
                // Empty label and colon cells
                table.AddCell(new PdfPCell(new Phrase("", font)) { Border = PdfPCell.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase("", font)) { Border = PdfPCell.NO_BORDER });
            }

            // Value
            PdfPCell valueCell = new PdfPCell(new Phrase(value ?? "", font));
            valueCell.HorizontalAlignment = Element.ALIGN_LEFT;
            valueCell.Border = PdfPCell.NO_BORDER;
            valueCell.PaddingLeft = 5f;
            table.AddCell(valueCell);
        }
        void AddApprovalRow(PdfPTable table, string label, string value, iTextSharp.text.Font font)
        {
            // Label cell
            PdfPCell labelCell = new PdfPCell(new Phrase(label, font));
            labelCell.HorizontalAlignment = Element.ALIGN_LEFT;
            labelCell.Border = PdfPCell.NO_BORDER;
            labelCell.PaddingRight = 5f;
            table.AddCell(labelCell);

            // Colon cell
            PdfPCell colonCell = new PdfPCell(new Phrase(":", font));
            colonCell.HorizontalAlignment = Element.ALIGN_LEFT;
            colonCell.Border = PdfPCell.NO_BORDER;
            colonCell.PaddingRight = 5f;
            table.AddCell(colonCell);

            // Value cell
            PdfPCell valueCell = new PdfPCell(new Phrase(value ?? "", font));
            valueCell.HorizontalAlignment = Element.ALIGN_LEFT;
            valueCell.Border = PdfPCell.NO_BORDER;
            valueCell.PaddingLeft = 5f;
            table.AddCell(valueCell);
        }
        void AddDetailRow(PdfPTable table, string label, string value, iTextSharp.text.Font font)
        {
            // Label cell
            PdfPCell labelCell = new PdfPCell(new Phrase(label, font));
            labelCell.HorizontalAlignment = Element.ALIGN_LEFT;
            labelCell.Border = PdfPCell.NO_BORDER;
            labelCell.PaddingRight = 5f;
            table.AddCell(labelCell);

            // Colon cell
            PdfPCell colonCell = new PdfPCell(new Phrase(":", font));
            colonCell.HorizontalAlignment = Element.ALIGN_LEFT;
            colonCell.Border = PdfPCell.NO_BORDER;
            colonCell.PaddingRight = 5f;
            table.AddCell(colonCell);

            // Value cell
            PdfPCell valueCell = new PdfPCell(new Phrase(value ?? "", font));
            valueCell.HorizontalAlignment = Element.ALIGN_LEFT;
            valueCell.Border = PdfPCell.NO_BORDER;
            valueCell.PaddingLeft = 5f;
            table.AddCell(valueCell);
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchValue = txtSearchSn.Text.Trim();

            if (string.IsNullOrEmpty(searchValue))
            {
                // Clear DataGridView or do nothing
                dgvMS.DataSource = null;
                return;
            }

            using (SqlConnection con = new SqlConnection(
                ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                con.Open();

                string query = @"select 
                                        SerialNo,
                                        Requester,
                                        Department,
                                        ExpensesType,
                                        RequestDate,
                                        HODApprovalStatus,
                                        ApprovedByHOD,
                                        HODApprovedDate,
                                        HRApprovalStatus,
                                        ApprovedByHR,
                                        HRApprovedDate,
                                        Account2ApprovalStatus,
                                        ApprovedByAccount2,
                                        Account2ApprovedDate,
                                        Account3ApprovalStatus,
                                        ApprovedByAccount3,
                                        Account3ApprovedDate,
                                        AccountApprovalStatus,
                                        ApprovedByAccount,
                                        AccountApprovedDate from tbl_MasterClaimForm where SerialNo LIKE @SerialNo;";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SerialNo", "%" + searchValue + "%");

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvMS.DataSource = dt;
                    BindDataGridView(dt);
                }
            }
        }
        private void dgvMS_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}