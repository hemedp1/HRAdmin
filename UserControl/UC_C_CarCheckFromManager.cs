using HRAdmin.Forms;
using System;
using HRAdmin.Components;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace HRAdmin.UserControl
{
    public partial class UC_C_CarCheckFromManager : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private string logginInUserAccessLevel;
        public UC_C_CarCheckFromManager(string username, string Depart, string UL)
        {
            InitializeComponent();

            string logginDepart1stLvl = UserSession.logginDepart0Lvl;
            loggedInDepart = Depart;
            loggedInUser = username;
            logginInUserAccessLevel = UL;
            //MessageBox.Show($"logginInUserAccessLevesssssl: {logginInUserAccessLevel}");
            LoadPendingBookings();
            LoadData();
            dTDay.ValueChanged += dTDay_ValueChanged;
            LoadPendingBookings();
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
        private void UC_C_CarCheckFromManager_Load(object sender, EventArgs e)
        {
            LoadData();
            dTDay.ValueChanged += dTDay_ValueChanged;
        }
        private void LoadPendingBookings()
        {
            //MessageBox.Show($"logginInUserAccessLevel: {logginInUserAccessLevel}");
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();

                    // 1. Get all departments this user can approve
                    List<string> approvableDepartments = new List<string>();

                    // Check parent departments (like GENERAL AFFAIRS overseeing others)
                    string departmentQuery = @"SELECT Department0 FROM tbl_Department 
                                    WHERE Department1 = @UserDepartment";

                    using (SqlCommand deptCmd = new SqlCommand(departmentQuery, con))
                    {
                        deptCmd.Parameters.AddWithValue("@UserDepartment", UserSession.loggedInDepart);

                        using (SqlDataReader reader = deptCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                approvableDepartments.Add(reader["Department0"].ToString());
                            }
                        }
                    }

                    // If no special approval rights, just show own department
                    if (approvableDepartments.Count == 0)
                    {
                        approvableDepartments.Add(UserSession.loggedInDepart);
                    }

                    // 2. Build the main query with dynamic department filtering
                    string baseQuery = @"
                SELECT 
                    a.BookingID, 
                    a.DriverName, 
                    a.IndexNo, 
                    a.Destination, 
                    a.RequestDate, 
                    a.Purpose, 
                    a.AssignedCar, 
                    a.StatusCheck, 
                    a.CheckBy, 
                    ud.Department0,
                    ud.Department1,
                    CASE 
                        WHEN a.DateChecked IS NULL THEN 'Pending'
                        ELSE CONVERT(VARCHAR, a.DateChecked, 120)
                    END AS DateChecked, 
                    a.Status, 
                    a.ApproveBy, 
                    CASE 
                        WHEN a.DateApprove IS NULL THEN 'Pending'
                        ELSE CONVERT(VARCHAR, a.DateApprove, 120)
                    END AS DateApprove,
                    CONVERT(VARCHAR(5), a.StartDate, 108) AS StartDate, 
                    CONVERT(VARCHAR(5), a.EndDate, 108) AS EndDate 
                FROM tbl_CarBookings a
                LEFT JOIN tbl_Department ud ON a.Depart = ud.Department0
                WHERE a.StatusCheck = 'Pending'";

                    // Add department filter
                    string departmentFilter = " AND a.Depart IN (";
                    var parameters = new List<SqlParameter>();

                    for (int i = 0; i < approvableDepartments.Count; i++)
                    {
                        if (i > 0) departmentFilter += ",";
                        string paramName = "@dept" + i;
                        departmentFilter += paramName;
                        parameters.Add(new SqlParameter(paramName, approvableDepartments[i]));
                    }
                    departmentFilter += ")";

                    // Add current user exclusion
                    string excludeCurrentUser = " AND a.DriverName <> @CurrentUser";
                    parameters.Add(new SqlParameter("@CurrentUser", UserSession.LoggedInUser));

                    // Combine all query parts
                    string fullQuery = baseQuery + departmentFilter + excludeCurrentUser;

                    using (SqlCommand cmd = new SqlCommand(fullQuery, con))
                    {
                        // Add all parameters
                        cmd.Parameters.AddRange(parameters.ToArray());

                        // Debug output
                        Debug.WriteLine($"User can approve departments: {string.Join(", ", approvableDepartments)}");
                        Debug.WriteLine($"Excluding user: {UserSession.LoggedInUser}");

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            dataGridView1.DataSource = dt;

                            // Additional debug
                            Debug.WriteLine($"Found {dt.Rows.Count} pending bookings");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading pending bookings: " + ex.Message);
                    Debug.WriteLine($"Error: {ex.Message}\n{ex.StackTrace}");
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
        private void CheckUserAccess(string username)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = "SELECT AA, MA FROM tbl_Users WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string AA = reader["AA"].ToString();
                                string MA = reader["MA"].ToString();

                                if (AA == "1")
                                {
                                    Form_Home.sharedLabel.Text = "Admin > Car Reservation";
                                    Form_Home.sharedButton.Visible = false;
                                    Form_Home.sharedButtonew.Visible = false;
                                    Form_Home.sharedButton2.Visible = false;   // withdraw
                                    Form_Home.sharedButton3.Visible = false;   // replace
                                    Form_Home.sharedButtonBC.Visible = true;
                                    Form_Home.sharedButtonbtnApp.Visible = true;
                                    Form_Home.sharedButtonbtnWDcar.Visible = true;
                                    Form_Home.sharedbuttonInspect.Visible = true;
                                    Form_Home.sharedbtn_Accident.Visible = true;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);
                                }
                                else if (MA == "2")
                                {
                                    Form_Home.sharedLabel.Text = "Admin > Car Reservation";
                                    Form_Home.sharedButton.Visible = false;
                                    Form_Home.sharedButtonew.Visible = false;
                                    Form_Home.sharedButton2.Visible = false;   // withdraw
                                    Form_Home.sharedButton3.Visible = false;   // replace
                                    Form_Home.sharedButtonbtnApp.Visible = true;
                                    Form_Home.sharedButtonBC.Visible = true;
                                    Form_Home.sharedButtonbtnWDcar.Visible = true;
                                    Form_Home.sharedbtn_Accident.Visible = true;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);
                                }
                                else
                                {
                                    Form_Home.sharedLabel.Text = "Admin > Car Reservation";
                                    Form_Home.sharedButton.Visible = false;
                                    Form_Home.sharedButtonew.Visible = false;
                                    Form_Home.sharedButton2.Visible = false;   // withdraw
                                    Form_Home.sharedButton3.Visible = false;   // replace
                                    Form_Home.sharedbuttonInspect.Visible = false;
                                    Form_Home.sharedButtonbtnApp.Visible = false;
                                    Form_Home.sharedButtonBC.Visible = true;
                                    Form_Home.sharedButtonbtnWDcar.Visible = true;
                                    Form_Home.sharedbtn_Accident.Visible = true;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            CheckUserAccess(loggedInUser);
        }
        private void LoadData()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();

                    // 1. Get approvable departments
                    List<string> approvableDepartments = new List<string>();

                    // Check if user has special approval rights
                    string approverQuery = @"SELECT Department0 FROM tbl_Department 
                                   WHERE Department1 = @UserDepartment";

                    using (SqlCommand approverCmd = new SqlCommand(approverQuery, con))
                    {
                        approverCmd.Parameters.AddWithValue("@UserDepartment", UserSession.loggedInDepart);

                        using (SqlDataReader reader = approverCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                approvableDepartments.Add(reader["Department0"].ToString());
                            }
                        }
                    }

                    // If no special approval rights, just show own department
                    if (approvableDepartments.Count == 0)
                    {
                        approvableDepartments.Add(UserSession.loggedInDepart);
                    }

                    // 2. Build and execute main query with filters
                    string baseQuery = @"
                SELECT 
                    BookingID, DriverName, IndexNo, RequestDate, 
                    Destination, Purpose, AssignedCar, Status, 
                    ApproveBy, DateApprove, StatusCheck, CheckBy, DateChecked,
                    CONVERT(VARCHAR(5), StartDate, 108) AS StartDate, 
                    CONVERT(VARCHAR(5), EndDate, 108) AS EndDate 
                FROM tbl_CarBookings 
                WHERE CONVERT(date, RequestDate) = @SelectedDate 
                AND StatusCheck = 'Pending'";

                    // Add department filter
                    string departmentFilter = " AND Depart IN (";
                    var parameters = new List<SqlParameter>();

                    for (int i = 0; i < approvableDepartments.Count; i++)
                    {
                        if (i > 0) departmentFilter += ",";
                        string paramName = "@dept" + i;
                        departmentFilter += paramName;
                        parameters.Add(new SqlParameter(paramName, approvableDepartments[i]));
                    }
                    departmentFilter += ")";

                    // Add current user exclusion
                    string excludeCurrentUser = " AND DriverName <> @CurrentUser";
                    parameters.Add(new SqlParameter("@CurrentUser", UserSession.LoggedInUser));

                    // Combine all query parts
                    string fullQuery = baseQuery + departmentFilter + excludeCurrentUser;

                    using (SqlCommand cmd = new SqlCommand(fullQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@SelectedDate", dTDay.Value.Date);
                        cmd.Parameters.AddRange(parameters.ToArray());

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        // Configure DataGridView
                        ConfigureDataGridView(dt);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message);
                }
            }
        }
        private void ConfigureDataGridView(DataTable dt)
        {
            dataGridView1.Columns.Clear();
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dataGridView1.ScrollBars = ScrollBars.Both;

            dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Arial", 11, FontStyle.Bold),
            };

            // Add ID column
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
            {
                HeaderText = "ID",
                DataPropertyName = "BookingID",
                Width = 80,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                }
            });

            // Column configurations
            var columnConfigs = new Dictionary<string, string>()
            {
                ["DriverName"] = "Driver",
                ["IndexNo"] = "Index No",
                ["RequestDate"] = "Request Date",
                ["Destination"] = "Destination",
                ["StartDate"] = "Start Time",
                ["EndDate"] = "End Time",
                ["Status"] = "Admin Status Approval",
                ["Purpose"] = "Purpose",
                ["AssignedCar"] = "Car",
                ["ApproveBy"] = "Approve By",
                ["DateApprove"] = "Approved Date",
                ["StatusCheck"] = "HOD Status Check",
                ["CheckBy"] = "Check By",
                ["DateChecked"] = "Checked Date"
            };

            // Add remaining columns
            foreach (var config in columnConfigs)
            {
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                {
                    HeaderText = config.Value,
                    DataPropertyName = config.Key,
                    Width = 120,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        ForeColor = Color.MidnightBlue,
                        Font = new Font("Arial", 11)
                    }
                });
            }

            dataGridView1.DataSource = dt;
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void dTDay_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a booking to verify.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rowIndex = dataGridView1.CurrentRow.Index;
            DateTime selectedDate = dTDay.Value.Date;
            string bookingIDStr = dataGridView1.Rows[rowIndex].Cells[0]?.Value?.ToString();

            int bookingID;
            if (!int.TryParse(bookingIDStr, out bookingID))
            {
                MessageBox.Show("Invalid Booking ID format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to verify this reservation?", "Verify Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    string checkdepart = @"
            SELECT q.DriverName, q.Depart, q.StartDate, q.EndDate, q.Purpose, q.RequestDate, q.Destination, u.Position, l.AccessLevel 
            FROM tbl_CarBookings q 
            LEFT JOIN tbl_Users u ON q.DriverName = u.Username 
            LEFT JOIN tbl_UsersLevel l ON u.Position = l.TitlePosition 
            WHERE BookingID = @BookingID";

                    string Username = string.Empty;
                    string tujuan = string.Empty;
                    string tempat = string.Empty;
                    string timeOutFormatted = string.Empty;
                    string timeInFormatted = string.Empty;
                    string bookingDateFormatted = string.Empty;
                    int requestorAccessLevel = -1;

                    // First query to check
                    using (SqlCommand checkCmd = new SqlCommand(checkdepart, con))
                    {
                        checkCmd.Parameters.AddWithValue("@BookingID", bookingID);

                        using (SqlDataReader reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Username = reader["DriverName"]?.ToString();
                                tujuan = reader["Purpose"]?.ToString();
                                tempat = reader["Destination"]?.ToString();
                                DateTime? bookingDate = reader["RequestDate"] != DBNull.Value
                                ? Convert.ToDateTime(reader["RequestDate"])
                                : (DateTime?)null;
                                bookingDateFormatted = bookingDate?.ToString("dd/MM/yyyy");


                                TimeSpan? timeOut = reader["StartDate"] != DBNull.Value
                                    ? (TimeSpan)reader["StartDate"]
                                    : (TimeSpan?)null;
                                timeOutFormatted = timeOut?.ToString(@"hh\:mm");

                                TimeSpan? timeIn = reader["EndDate"] != DBNull.Value
                                    ? (TimeSpan)reader["EndDate"]
                                    : (TimeSpan?)null;
                                timeInFormatted = timeIn?.ToString(@"hh\:mm");


                                if (!int.TryParse(reader["AccessLevel"]?.ToString(), out requestorAccessLevel) ||
                                    !int.TryParse(logginInUserAccessLevel, out int currentUserAccessLevel))
                                {
                                    MessageBox.Show("Access level data is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                if (currentUserAccessLevel == requestorAccessLevel)
                                {
                                    MessageBox.Show("Cannot proceed. Only your superior can approve this action.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Show("No matching booking found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        } // Reader closes here
                    } // checkCmd closes here

                    // Now run the update safely
                    string query = @"
            UPDATE tbl_CarBookings 
            SET DateChecked = @DateChecked, StatusCheck = 'Checked', CheckBy = @loggedInUser 
            WHERE BookingID = @BookingID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@BookingID", bookingID);
                        cmd.Parameters.AddWithValue("@DateChecked", selectedDate);
                        cmd.Parameters.AddWithValue("@loggedInUser", loggedInUser);

                        try
                        {
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Reservation verified successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadPendingBookings();


//++++++++++++++++++++++++++++++++++++++         Email Fx        ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                                
                                string query1 = @"SELECT TOP 1 A.Department, A.AA, B.Email
                                                FROM 
                                                tbl_Users A
                                                LEFT JOIN tbl_UserDetail B ON A.IndexNo = B.IndexNo
                                                WHERE A.Department = 'HR & ADMIN' AND AA = '1'";
                                List<string> approverEmails = new List<string>();

                                using (SqlCommand emailCmd = new SqlCommand(query1, con))
                                {
                                    //emailCmd.Parameters.AddWithValue("@Username", Username);

                                    using (SqlDataReader reader = emailCmd.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            string email = reader["Email"]?.ToString();
                                            if (!string.IsNullOrEmpty(email))
                                            {
                                                approverEmails.Add(email);
                                            }
                                        }
                                    }
                                }

                                if (approverEmails.Count > 0)
                                {
                                    string destination = tempat;//txtDes.Text;
                                    string purpose = tujuan; //txtPurpose.Text;

                                    string subject = "HEM Admin Accessibility Notification: Car Booking Request Awaiting Final Approval";

                                    string body = $@"
                                                    <p>Dear Approver - HR & ADMIN,</p>
                                                    <p>
                                                      A <strong>car booking request</strong> for 
                                                      Mr./Ms. <strong>{Username}</strong> has been 
                                                      approved by Mr./Ms. <strong>{UserSession.loggedInName}</strong> 
                                                      and is now awaiting your final approval.
                                                    </p>

                                                    <p><u>Booking Details:</u></p>
                                                    <ul>
                                                        <li><strong>Purpose:</strong> {purpose}</li>
                                                        <li><strong>Destination:</strong> {destination}</li>
                                                        <li><strong>Request Date:</strong> {bookingDateFormatted}</li>
                                                        <li><strong>Time Out:</strong> {timeOutFormatted}</li>
                                                        <li><strong>Time In:</strong> {timeInFormatted}</li>
                                                        
                                                    </ul>

                                                     <p>Please log in to the system to <strong>review</strong> and <strong>approve</strong> or <strong>reject</strong> the request.</p>

                                                    <p>Thank you,<br/>HEM Admin Accessibility</p>
                                                ";

                                    foreach (var email in approverEmails)
                                    {
                                        SendEmail(email, subject, body);
                                    }

                                    MessageBox.Show(
                                        "A booking approval notification has been sent to approver HR & ADMIN.",
                                        "Notification Sent",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information
                                    );
                                }
                                else
                                {
                                    MessageBox.Show("No record updated. Please check the Booking ID.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            //MessageBox.Show($"Username: {Username}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    } // cmd closes here
                } // connection closes here
            }
        }
        private void btnRej_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a booking to reject.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rowIndex = dataGridView1.CurrentRow.Index;
            DateTime selectedDate = dTDay.Value.Date;
            string bookingIDStr = dataGridView1.Rows[rowIndex].Cells[0]?.Value?.ToString();

            int bookingID;
            if (!int.TryParse(bookingIDStr, out bookingID))
            {
                MessageBox.Show("Invalid Booking ID format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to reject this reservation?", "Reject Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    string checkdepart = @"
                                        SELECT q.DriverName, q.Depart, q.StartDate, q.EndDate, q.Purpose, q.RequestDate, q.Destination, u.Position, l.AccessLevel 
                                        FROM tbl_CarBookings q 
                                        LEFT JOIN tbl_Users u ON q.DriverName = u.Username 
                                        LEFT JOIN tbl_UsersLevel l ON u.Position = l.TitlePosition 
                                        WHERE BookingID = @BookingID";

                    string Username = string.Empty;
                    string tujuan = string.Empty;
                    string tempat = string.Empty;
                    string timeOutFormatted = string.Empty;
                    string timeInFormatted = string.Empty;
                    string bookingDateFormatted = string.Empty;
                    int requestorAccessLevel = -1;

                    // First query to check
                    using (SqlCommand checkCmd = new SqlCommand(checkdepart, con))
                    {
                        checkCmd.Parameters.AddWithValue("@BookingID", bookingID);

                        using (SqlDataReader reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Username = reader["DriverName"]?.ToString();
                                tujuan = reader["Purpose"]?.ToString();
                                tempat = reader["Destination"]?.ToString();
                                DateTime? bookingDate = reader["RequestDate"] != DBNull.Value
                                ? Convert.ToDateTime(reader["RequestDate"])
                                : (DateTime?)null;
                                bookingDateFormatted = bookingDate?.ToString("dd/MM/yyyy");


                                TimeSpan? timeOut = reader["StartDate"] != DBNull.Value
                                    ? (TimeSpan)reader["StartDate"]
                                    : (TimeSpan?)null;
                                timeOutFormatted = timeOut?.ToString(@"hh\:mm");

                                TimeSpan? timeIn = reader["EndDate"] != DBNull.Value
                                    ? (TimeSpan)reader["EndDate"]
                                    : (TimeSpan?)null;
                                timeInFormatted = timeIn?.ToString(@"hh\:mm");

                                if (!int.TryParse(reader["AccessLevel"]?.ToString(), out requestorAccessLevel) ||
                                    !int.TryParse(logginInUserAccessLevel, out int currentUserAccessLevel))
                                {
                                    MessageBox.Show("Access level data is invalid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }

                                if (currentUserAccessLevel == requestorAccessLevel)
                                {
                                    MessageBox.Show("Cannot proceed. Only your superior can reject this action.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Show("No matching booking found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        } // Reader closes here
                    } // checkCmd closes here

                    // Now run the update safely     //  Checked
                    string query = @"
                                    UPDATE tbl_CarBookings 
                                    SET DateChecked = @DateChecked, StatusCheck = 'Rejected', CheckBy = @loggedInUser 
                                    WHERE BookingID = @BookingID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@BookingID", bookingID);
                        cmd.Parameters.AddWithValue("@DateChecked", selectedDate);
                        cmd.Parameters.AddWithValue("@loggedInUser", loggedInUser);

                        try
                        {
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Reservation rejected successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadPendingBookings();

                                //+++++++++++++++++         Email Fx        ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                                string query11 = @"SELECT A.Email, B.Name1 
                                                  FROM tbl_UserDetail A
                                                  LEFT JOIN tbl_Users B ON A.Username = B.Username 
                                                  WHERE A.Username = @Username";

                                List<string> UserEmails = new List<string>();
                                string namev = "";

                                using (SqlCommand emailCmd1 = new SqlCommand(query11, con))
                                {
                                    emailCmd1.Parameters.AddWithValue("@Username", Username);

                                    using (SqlDataReader reader = emailCmd1.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            string email1 = reader["Email"]?.ToString();
                                            if (!string.IsNullOrEmpty(email1))
                                            {
                                                UserEmails.Add(email1);
                                            }

                                            namev = reader["Name1"]?.ToString();
                                        }
                                    }
                                }

                                if (UserEmails.Count > 0)
                                {
                                    string destination = tempat;   // txtDes.Text;
                                    string purpose = tujuan;       // txtPurpose.Text;

                                    string subject = "HEM Admin Accessibility Notification: Car Booking Request Rejected";

                                    string body = $@"
                                                    <p>Dear Mr./Ms. {namev}</p>
                                                    <p>
                                                      Your <strong>car booking request</strong> has been 
                                                      <strong>rejected</strong> by Mr./Ms. <strong>{UserSession.loggedInName}</strong>.
                                                    </p>

                                                    <p><u>Booking Details:</u></p>
                                                    <ul>
                                                        <li><strong>Purpose:</strong> {purpose}</li>
                                                        <li><strong>Destination:</strong> {destination}</li>
                                                        <li><strong>Request Date:</strong> {bookingDateFormatted}</li>
                                                        <li><strong>Time Out:</strong> {timeOutFormatted}</li>
                                                        <li><strong>Time In:</strong> {timeInFormatted}</li>
                                                    </ul>

                                                    <p>If you have any questions, please contact your HOD</strong>.</p>

                                                    <p>Thank you,<br/>HEM Admin Accessibility</p>
                                                ";

                                    foreach (var email in UserEmails)
                                    {
                                        SendEmail(email, subject, body);
                                    }

                                    MessageBox.Show(
                                        "A booking rejection notification has been sent to the requester.",
                                        "Notification Sent",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information
                                    );
                                }

                                else
                                {
                                    MessageBox.Show("No record updated. Please check the Booking ID.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            //MessageBox.Show($"Username: {Username}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    } // cmd closes here
                } // connection closes here
            }
        }
        
    }
}
/*

SELECT TOP 1 A.Name,A.Department, A.AA, B.Email,C.AccessLevel
    FROM 
    tbl_Users A
    LEFT JOIN tbl_UserDetail B ON A.IndexNo = B.IndexNo
	LEFT JOIN tbl_UsersLevel C ON A.Position = C.TitlePosition
    WHERE A.Department = 'HR & ADMIN' AND AA = '1' AND AccessLevel = '0'

SELECT A.Department, A.Username, B.Email, C.AccessLevel
FROM tbl_Users A
LEFT JOIN tbl_UserDetail B ON A.IndexNo = B.IndexNo
LEFT JOIN tbl_UsersLevel C ON A.Position = C.TitlePosition
WHERE Department = 'HR & ADMIN' AND AccessLevel  < 1
*/