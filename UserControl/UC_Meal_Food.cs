using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HRAdmin.UserControl;
using HRAdmin.Forms;
using System.Runtime.Serialization.Formatters;
using HRAdmin.Components;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Reflection.Emit;
using System.Configuration;
using System.Data.SqlClient;
using DrawingFont = System.Drawing.Font;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Web.UI;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using iTextRectangle = iTextSharp.text.Rectangle;
using WinFormsApp = System.Windows.Forms.Application;
using System.Net.Mail;
using System.Net;
using System.Drawing.Drawing2D;

namespace HRAdmin.UserControl
{
    public partial class UC_Meal_Food : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private string EventDetails;
        private string EventTime;
        private DateTime DeliveryTime;
        private DateTime eventTime;
        private object deliveryTime;
        private string eventTime1;
        private DataTable cachedData; // Cache for data
        private bool isNetworkErrorShown = false; // Flag to prevent multiple error pop-ups
        private bool isNetworkUnavailable = false; // Track network status to avoid repeated checks

        public DateTime EventDate { get; set; }
        public string OccasionType { get; set; }
        public UC_Meal_Food(string eventDetails, string eventTime1, DateTime deliveryTime, string loggedInUser, string department)
        {
            InitializeComponent();
            EventDetails = eventDetails;
            this.eventTime1 = eventTime1;
            this.deliveryTime = deliveryTime;
            this.loggedInUser = loggedInUser;
            dtRequest.Text = DateTime.Now.ToString("dd.MM.yyyy");
            loggedInDepart = department;
            LoadUsernames();
            LoadDepartments();
            LoadData();
            cmbRequester.SelectedIndexChanged += cmbRequester_SelectedIndexChanged;
            cmbDepart.SelectedIndexChanged += cmbDepart_SelectedIndexChanged;
            cmbOS_Occasion.SelectedIndexChanged += cmbOS_Occasion_SelectedIndexChanged;
            dtpStart.ValueChanged += dtpStart_ValueChanged;
            dtpEnd.ValueChanged += dtpEnd_ValueChanged;
            cachedData = new DataTable(); // Initialize (replace with actual cache loading logic)
            isNetworkErrorShown = false;
            this.Load += UC_Food_Load;
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
        public UC_Meal_Food(string eventDetails, DateTime eventTime, string loggedInUser, string department)
        {
            InitializeComponent();
            EventDetails = eventDetails;
            this.eventTime = eventTime;
            this.loggedInUser = loggedInUser;
            loggedInDepart = department;
            dtRequest.Text = DateTime.Now.ToString("dd.MM.yyyy");
            LoadUsernames();
            LoadDepartments();
            LoadData();
            cmbRequester.SelectedIndexChanged += cmbRequester_SelectedIndexChanged;
            cmbDepart.SelectedIndexChanged += cmbDepart_SelectedIndexChanged;
            cmbOS_Occasion.SelectedIndexChanged += cmbOS_Occasion_SelectedIndexChanged;
            dtpStart.ValueChanged += dtpStart_ValueChanged;
            dtpEnd.ValueChanged += dtpEnd_ValueChanged;
            cachedData = new DataTable(); // Initialize (replace with actual cache loading logic)
            isNetworkErrorShown = false;
            this.Load += UC_Food_Load;
        }
        private void dTDay_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
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
        private void CheckUserAccess()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = "SELECT AA FROM tbl_Users WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", loggedInUser);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string AA = reader["AA"].ToString();

                                // Set check, approve button, and labels visibility: hidden if AA = 1, visible if MA = 2
                                if (AA == "1" && loggedInDepart == "HR & ADMIN")
                                {
                                    GB_Authorization.Visible = true;
                                    btnApprove.Enabled = false;
                                }
                                else if (AA == "2" && loggedInDepart == "HR & ADMIN")
                                {
                                    GB_Authorization.Visible = true;
                                }
                                else
                                {
                                    GB_Authorization.Visible = false;
                                }
                            }
                            else
                            {
                                GB_Authorization.Visible = false;
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
                            cmbRequester.Items.Add("All Users");
                            while (reader.Read())
                            {
                                string username = reader["Username"].ToString();
                                cmbRequester.Items.Add(username);
                                Debug.WriteLine($"Loaded username: {username}");
                            }
                        }
                    }
                    cmbRequester.SelectedIndex = 0;
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
                            cmbRequester.Items.Add("All Users"); // Add "All Users" option
                            while (reader.Read())
                            {
                                string username = reader["Username"].ToString();
                                cmbRequester.Items.Add(username);
                                Debug.WriteLine($"Loaded username: {username} for department: {department}");
                            }
                        }
                    }
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
                    string query = @"
                SELECT DISTINCT Department 
                FROM tbl_InternalFoodOrder 
                WHERE Department IS NOT NULL
                UNION
                SELECT DISTINCT Department 
                FROM tbl_ExternalFoodOrder 
                WHERE Department IS NOT NULL";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            cmbDepart.Items.Clear();
                            cmbDepart.Items.Add("All Departments");
                            while (reader.Read())
                            {
                                string department = reader["Department"].ToString();
                                cmbDepart.Items.Add(department);
                                Debug.WriteLine($"Loaded department: {department}");
                            }
                        }
                    }
                    cmbDepart.SelectedIndex = 0;
                    Debug.WriteLine("Departments loaded successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading departments: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error loading departments: {ex.Message}");
                }
            }
        }
        private void LoadData(string requesterID = null, string department = null, string occasionType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (dgv_OS == null)
            {
                MessageBox.Show("DataGridView is not initialized!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsNetworkAvailable())
            {
                if (!isNetworkUnavailable)
                {
                    isNetworkUnavailable = true;
                    MessageBox.Show("Network disconnected.", "Network Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (cachedData != null && cachedData.Rows.Count > 0)
                {
                    BindDataGridView(cachedData);
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Network unavailable. Displaying cached data.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Network unavailable and no cached data available.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                return;
            }

            // Set default weekly filter if no date range is provided
            DateTime today = DateTime.Today;
            DateTime weekStart = today.AddDays(-(int)today.DayOfWeek); // Start of the week (Sunday)
            DateTime weekEnd = weekStart.AddDays(7).AddTicks(-1); // End of the week (Saturday, inclusive)
            if (!startDate.HasValue)
            {
                startDate = weekStart;
            }
            if (!endDate.HasValue)
            {
                endDate = weekEnd;
            }
            else
            {
                // Normalize endDate to the end of the day
                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            // Normalize startDate to the beginning of the day
            startDate = startDate.Value.Date;

            string query = "";
            if (occasionType == "Internal")
            {
                query = @"
            SELECT 'Internal' AS OrderSource, 
                   OrderID, RequesterID, Department, OccasionType, EventDetails, RequestDate, DeliveryDate, CheckStatus,
                   CheckedBy, CheckedDate, ApproveStatus, ApprovedBy, ApprovedDate, OrderType 
            FROM tbl_InternalFoodOrder
            WHERE (@RequesterID IS NULL OR RequesterID = @RequesterID)
                  AND (@Department IS NULL OR Department = @Department)
                  AND RequestDate >= @StartDate
                  AND RequestDate <= @EndDate
            ORDER BY RequestDate ASC";
            }
            else if (occasionType == "External")
            {
                query = @"
            SELECT 'External' AS OrderSource,
                   OrderID, RequesterID, Department, OccasionType, EventDetails, RequestDate, DeliveryDate, CheckStatus,
                   CheckedBy, CheckedDate, ApproveStatus, ApprovedBy, ApprovedDate, OrderType
            FROM tbl_ExternalFoodOrder
            WHERE (@RequesterID IS NULL OR RequesterID = @RequesterID)
                  AND (@Department IS NULL OR Department = @Department)
                  AND RequestDate >= @StartDate
                  AND RequestDate <= @EndDate
            ORDER BY RequestDate ASC";
            }
            else
            {
                query = @"
            SELECT 'Internal' AS OrderSource, 
                   OrderID, RequesterID, Department, OccasionType, EventDetails, RequestDate, DeliveryDate, CheckStatus,
                   CheckedBy, CheckedDate, ApproveStatus, ApprovedBy, ApprovedDate, OrderType 
            FROM tbl_InternalFoodOrder
            WHERE (@RequesterID IS NULL OR RequesterID = @RequesterID)
                  AND (@Department IS NULL OR Department = @Department)
                  AND RequestDate >= @StartDate
                  AND RequestDate <= @EndDate
            UNION ALL
            SELECT 'External' AS OrderSource,
                   OrderID, RequesterID, Department, OccasionType, EventDetails, RequestDate, DeliveryDate, CheckStatus,
                   CheckedBy, CheckedDate, ApproveStatus, ApprovedBy, ApprovedDate, OrderType
            FROM tbl_ExternalFoodOrder
            WHERE (@RequesterID IS NULL OR RequesterID = @RequesterID)
                  AND (@Department IS NULL OR Department = @Department)
                  AND RequestDate >= @StartDate
                  AND RequestDate <= @EndDate
            ORDER BY RequestDate ASC";
            }

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Add parameters
                        cmd.Parameters.Add("@RequesterID", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(requesterID) ? (object)DBNull.Value : requesterID;
                        cmd.Parameters.Add("@Department", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(department) ? (object)DBNull.Value : department;
                        cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
                        cmd.Parameters.AddWithValue("@EndDate", endDate.Value);

                        Debug.WriteLine($"Executing LoadData with RequesterID: {(string.IsNullOrEmpty(requesterID) ? "NULL" : requesterID)}, Department: {(string.IsNullOrEmpty(department) ? "NULL" : department)}, OccasionType: {(string.IsNullOrEmpty(occasionType) ? "NULL" : occasionType)}, StartDate: {startDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}, EndDate: {endDate.Value.ToString("yyyy-MM-dd HH:mm:ss")}");

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        // Update cache
                        cachedData = dt.Copy();

                        Debug.WriteLine($"Rows retrieved: {dt.Rows.Count}");
                        foreach (DataRow row in dt.Rows)
                        {
                            Debug.WriteLine($"Row: OrderID={row["OrderID"]}, RequesterID={row["RequesterID"]}, Department={row["Department"]}, OrderSource={row["OrderSource"]}, RequestDate={row["RequestDate"]}");
                        }

                        // Bind to DataGridView
                        BindDataGridView(dt);

                        // Reset network error flags
                        isNetworkErrorShown = false;
                        isNetworkUnavailable = false;
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == -1 || ex.Number == 26) // Network-related SQL errors
                {
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Unable to connect to the database. Please check your network connection.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    if (!isNetworkErrorShown)
                    {
                        isNetworkErrorShown = true;
                        MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!isNetworkErrorShown)
                {
                    isNetworkErrorShown = true;
                    MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error loading data: {ex.Message}");
                }
            }
        }
        private void cmbRequester_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedOccasion = cmbOS_Occasion.SelectedItem?.ToString();
            DateTime? startDate = dtpStart.Value == dtpStart.MinDate ? null : (DateTime?)dtpStart.Value;
            DateTime? endDate = dtpEnd.Value == dtpEnd.MinDate ? null : (DateTime?)dtpEnd.Value;

            Debug.WriteLine($"cmbRequester selected: {selectedUsername}");
            if (selectedUsername == "All Users" || string.IsNullOrEmpty(selectedUsername))
            {
                selectedUsername = null;
                Debug.WriteLine("Loading data with no RequesterID filter.");
            }
            if (selectedDepartment == "All Departments" || string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
            }
            if (selectedOccasion == "All" || string.IsNullOrEmpty(selectedOccasion))
            {
                selectedOccasion = null;
            }

            LoadData(selectedUsername, selectedDepartment, selectedOccasion, startDate, endDate);
        }
        private void cmbDepart_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedOccasion = cmbOS_Occasion.SelectedItem?.ToString();
            DateTime? startDate = dtpStart.Value == dtpStart.MinDate ? null : (DateTime?)dtpStart.Value;
            DateTime? endDate = dtpEnd.Value == dtpEnd.MinDate ? null : (DateTime?)dtpEnd.Value;

            //Debug.WriteLine($"cmbDepart selected: {selectedDepartment}");

            // Update requester combo box based on selected department
            if (selectedDepartment == "All Departments" || string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
                LoadUsernames(); // Load all usernames
               // Debug.WriteLine("Loading all usernames for 'All Departments'.");
            }
            else
            {
                // Load usernames for the selected department
                LoadUsernamesByDepartment(selectedDepartment);
               // Debug.WriteLine($"Loading usernames for department: {selectedDepartment}");
            }

            // Reset requester selection to "All Users" to avoid invalid selections
            cmbRequester.SelectedIndex = cmbRequester.Items.Contains("All Users") ? 0 : -1;

            // Apply data filtering
            if (selectedUsername == "All Users" || string.IsNullOrEmpty(selectedUsername))
            {
                selectedUsername = null;
            }
            if (selectedOccasion == "All" || string.IsNullOrEmpty(selectedOccasion))
            {
                selectedOccasion = null;
            }

            LoadData(selectedUsername, selectedDepartment, selectedOccasion, startDate, endDate);
        }
        private void cmbOS_Occasion_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedOccasion = cmbOS_Occasion.SelectedItem?.ToString();
            DateTime? startDate = dtpStart.Value == dtpStart.MinDate ? null : (DateTime?)dtpStart.Value;
            DateTime? endDate = dtpEnd.Value == dtpEnd.MinDate ? null : (DateTime?)dtpEnd.Value;

            Debug.WriteLine($"cmbOS_Occasion selected: {selectedOccasion}");
            if (selectedUsername == "All Users" || string.IsNullOrEmpty(selectedUsername))
            {
                selectedUsername = null;
            }
            if (selectedDepartment == "All Departments" || string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
            }
            if (selectedOccasion == "All" || string.IsNullOrEmpty(selectedOccasion))
            {
                selectedOccasion = null;
            }

            LoadData(selectedUsername, selectedDepartment, selectedOccasion, startDate, endDate);
        }
        private void dtpStart_ValueChanged(object sender, EventArgs e)
        {
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedOccasion = cmbOS_Occasion.SelectedItem?.ToString();
            DateTime? startDate = dtpStart.Value == dtpStart.MinDate ? null : (DateTime?)dtpStart.Value;
            DateTime? endDate = dtpEnd.Value == dtpEnd.MinDate ? null : (DateTime?)dtpEnd.Value;

            if (selectedUsername == "All Users" || string.IsNullOrEmpty(selectedUsername))
            {
                selectedUsername = null;
            }
            if (selectedDepartment == "All Departments" || string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
            }
            if (selectedOccasion == "All" || string.IsNullOrEmpty(selectedOccasion))
            {
                selectedOccasion = null;
            }

            LoadData(selectedUsername, selectedDepartment, selectedOccasion, startDate, endDate);
        }
        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedOccasion = cmbOS_Occasion.SelectedItem?.ToString();
            DateTime? startDate = dtpStart.Value == dtpStart.MinDate ? null : (DateTime?)dtpStart.Value;
            DateTime? endDate = dtpEnd.Value == dtpEnd.MinDate ? null : (DateTime?)dtpEnd.Value;

            if (selectedUsername == "All Users" || string.IsNullOrEmpty(selectedUsername))
            {
                selectedUsername = null;
            }
            if (selectedDepartment == "All Departments" || string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
            }
            if (selectedOccasion == "All" || string.IsNullOrEmpty(selectedOccasion))
            {
                selectedOccasion = null;
            }

            LoadData(selectedUsername, selectedDepartment, selectedOccasion, startDate, endDate);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            string selectedOccasion = cmbOccasion.SelectedItem?.ToString();
            string eventText = txtEvent.Text.Trim();
            DateTime eventDate;
            if (!DateTime.TryParse(dtRequest.Text, out eventDate))
            {
                MessageBox.Show("Invalid request date format. Please use dd.MM.yyyy.", "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DateTime? deliveryTime = dtDelivery.Value;

            if (string.IsNullOrEmpty(selectedOccasion))
            {
                MessageBox.Show("Please select an occasion before proceeding.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(eventText))
            {
                MessageBox.Show("Please enter an event before proceeding.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (eventDate.Date >= deliveryTime?.Date)
            {
                MessageBox.Show("Delivery date cannot be on or before the request date.", "Invalid Date Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedOccasion == "Internal")
            {
                Form_Home.sharedLabel.Text = "Admin > Meal Request > Internal";
                Form_Home.sharedButton4.Visible = false;
                Form_Home.sharedButton5.Visible = false;
                Form_Home.sharedButton6.Visible = false;
                UC_Meal_Internal ug = new UC_Meal_Internal(eventText, eventDate, deliveryTime, loggedInUser, loggedInDepart);
                addControls(ug);
            }
            else if (selectedOccasion == "External")
            {
                Form_Home.sharedLabel.Text = "Admin > Meal Request > External";
                Form_Home.sharedButton4.Visible = false;
                Form_Home.sharedButton5.Visible = false;
                Form_Home.sharedButton6.Visible = false;
                UC_Meal_External ug = new UC_Meal_External(eventText, eventDate, deliveryTime, loggedInUser, loggedInDepart);
                addControls(ug);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "HR && Admin";
            Form_Home.sharedButton4.Visible = false;
            Form_Home.sharedButton5.Visible = false;
            Form_Home.sharedButton6.Visible = false;
            UC_A_Admin ug = new UC_A_Admin(loggedInUser, loggedInDepart);
            addControls(ug);
        }
        private void button3_Click_1(object sender, EventArgs e)
        {
            if (dgv_OS.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to check.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewCell selectedCell = dgv_OS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string orderId = selectedRow.Cells["OrderID"].Value?.ToString();
            string orderSource = selectedRow.Cells["OrderSource"].Value?.ToString();
            string checkStatus = selectedRow.Cells["CheckStatus"].Value?.ToString();
            string approveStatus = selectedRow.Cells["ApproveStatus"].Value?.ToString();

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(orderSource))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Prevent re-checking if already checked or approved
            if (!string.IsNullOrEmpty(checkStatus) && checkStatus == "Checked")
            {
                MessageBox.Show("This order has already been checked.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(approveStatus) && approveStatus == "Approved")
            {
                MessageBox.Show("This order has already been approved and cannot be checked again.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tableName = orderSource == "Internal" ? "tbl_InternalFoodOrder" : "tbl_ExternalFoodOrder";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = $@"UPDATE {tableName} 
                             SET CheckStatus = @CheckStatus, 
                                 CheckedBy = @CheckedBy, 
                                 CheckedDate = @CheckedDate, 
                                 CheckedDepartment = @CheckedDepartment
                             WHERE OrderID = @OrderID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@CheckStatus", "Checked");
                        cmd.Parameters.AddWithValue("@CheckedBy", loggedInUser);
                        cmd.Parameters.AddWithValue("@CheckedDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@CheckedDepartment", loggedInDepart);
                        cmd.Parameters.AddWithValue("@OrderID", orderId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Order status updated to Checked.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                     cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString(),
                                     cmbOS_Occasion.SelectedItem?.ToString() == "All" ? null : cmbOS_Occasion.SelectedItem?.ToString(),
                                     dtpStart.Value == dtpStart.MinDate ? null : (DateTime?)dtpStart.Value,
                                     dtpEnd.Value == dtpEnd.MinDate ? null : (DateTime?)dtpEnd.Value);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update order status.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnApprove_Click(object sender, EventArgs e)
        {
            if (dgv_OS.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to approve.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewCell selectedCell = dgv_OS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string orderId = selectedRow.Cells["OrderID"].Value?.ToString();
            string orderSource = selectedRow.Cells["OrderSource"].Value?.ToString();
            string checkStatus = selectedRow.Cells["CheckStatus"].Value?.ToString();
            string approveStatus = selectedRow.Cells["ApproveStatus"].Value?.ToString();

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(orderSource))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!string.IsNullOrEmpty(checkStatus) && checkStatus == "Rejected")
            {
                MessageBox.Show("This order has already been rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if the order has been checked
            if (string.IsNullOrEmpty(checkStatus) || checkStatus != "Checked")
            {
                MessageBox.Show("Please check the order before approving.", "Check Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Prevent re-approving if already approved
            if (!string.IsNullOrEmpty(approveStatus) && approveStatus == "Approved")
            {
                MessageBox.Show("This order has already been approved.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(checkStatus) && checkStatus == "Rejected")
            {
                MessageBox.Show("This order has already been rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(approveStatus) && approveStatus == "Rejected")
            {
                MessageBox.Show("This order has already been rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmation prompt before approving
            DialogResult result = MessageBox.Show($"Are you sure you want to approve Order ID: {orderId}?", "Confirm Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return; // Exit if user clicks "No" or closes the dialog
            }

            string tableName = orderSource == "Internal" ? "tbl_InternalFoodOrder" : "tbl_ExternalFoodOrder";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = $@"UPDATE {tableName} 
                             SET ApproveStatus = @ApproveStatus, 
                                 ApprovedBy = @ApprovedBy, 
                                 ApprovedDate = @ApprovedDate,
                                 ApprovedDepartment = @ApprovedDepartment
                             WHERE OrderID = @OrderID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@ApproveStatus", "Approved");
                        cmd.Parameters.AddWithValue("@ApprovedBy", loggedInUser);
                        cmd.Parameters.AddWithValue("@ApprovedDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@ApprovedDepartment", loggedInDepart);
                        cmd.Parameters.AddWithValue("@OrderID", orderId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Order status updated to Approved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                     cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString(),
                                     cmbOS_Occasion.SelectedItem?.ToString() == "All" ? null : cmbOS_Occasion.SelectedItem?.ToString(),
                                     dtpStart.Value == dtpStart.MinDate ? null : (DateTime?)dtpStart.Value,
                                     dtpEnd.Value == dtpEnd.MinDate ? null : (DateTime?)dtpEnd.Value);
                        }
                        else
                        {
                            MessageBox.Show("Failed to update approval status.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnReject_Click(object sender, EventArgs e)
        {
            if (dgv_OS.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to reject.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewCell selectedCell = dgv_OS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;

            string orderId = selectedRow.Cells["OrderID"].Value?.ToString();
            string orderSource = selectedRow.Cells["OrderSource"].Value?.ToString();
            string checkStatus = selectedRow.Cells["CheckStatus"].Value?.ToString();
            string approveStatus = selectedRow.Cells["ApproveStatus"].Value?.ToString(); // ✅ fixed .Value bug
            string requesterId = selectedRow.Cells["RequesterID"]?.Value?.ToString();

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(orderSource))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();

                    // Get AA value
                    string aaQuery = "SELECT AA FROM tbl_Users WHERE Username = @Username";
                    string aaValue = null;

                    using (SqlCommand cmd = new SqlCommand(aaQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", loggedInUser);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                                aaValue = reader["AA"]?.ToString();
                        }
                    }

                    if (aaValue == null)
                    {
                        MessageBox.Show("Unable to determine user role.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string tableName = orderSource == "Internal" ? "tbl_InternalFoodOrder" : "tbl_ExternalFoodOrder";
                    string statusColumn, byColumn, dateColumn, deptColumn, currentStatus;

                    if (aaValue == "1" && loggedInDepart == "HR & ADMIN")
                    {
                        statusColumn = "CheckStatus";
                        byColumn = "CheckedBy";
                        dateColumn = "CheckedDate";
                        deptColumn = "CheckedDepartment";
                        currentStatus = checkStatus;
                    }
                    else if (aaValue == "2" && loggedInDepart == "HR & ADMIN")
                    {
                        statusColumn = "ApproveStatus";
                        byColumn = "ApprovedBy";
                        dateColumn = "ApprovedDate";
                        deptColumn = "ApprovedDepartment";
                        currentStatus = approveStatus;
                    }
                    else
                    {
                        MessageBox.Show("You do not have permission to reject this order.", "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Check if already rejected or approved
                    if (!string.IsNullOrEmpty(checkStatus) && checkStatus == "Rejected")
                    {
                        MessageBox.Show("This order has already been rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (currentStatus == "Rejected")
                    {
                        MessageBox.Show("This order has already been rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (currentStatus == "Approved" || currentStatus == "Checked")
                    {
                        MessageBox.Show("This order has already been approved and cannot be rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Confirm action
                    DialogResult result = MessageBox.Show($"Are you sure you want to reject Order ID: {orderId}?", "Confirm Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result != DialogResult.Yes) return;

                    // Update query with concurrency check
                    string updateQuery = $@"
                UPDATE {tableName}
                SET {statusColumn} = @Status,
                    {byColumn} = @ByUser,
                    {dateColumn} = @Date,
                    {deptColumn} = @Department
                WHERE OrderID = @OrderID
                AND ({statusColumn} IS NULL OR {statusColumn} NOT IN ('Rejected','Approved','Checked'))";

                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                    {
                        updateCmd.Parameters.AddWithValue("@Status", "Rejected");
                        updateCmd.Parameters.AddWithValue("@ByUser", loggedInUser);
                        updateCmd.Parameters.AddWithValue("@Date", DateTime.Now);
                        updateCmd.Parameters.AddWithValue("@Department", loggedInDepart);
                        updateCmd.Parameters.AddWithValue("@OrderID", orderId);

                        int rowsAffected = updateCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Order status updated to Rejected.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData(
                                cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString(),
                                cmbOS_Occasion.SelectedItem?.ToString() == "All" ? null : cmbOS_Occasion.SelectedItem?.ToString(),
                                dtpStart.Value == dtpStart.MinDate ? null : (DateTime?)dtpStart.Value,
                                dtpEnd.Value == dtpEnd.MinDate ? null : (DateTime?)dtpEnd.Value
                            );
                        }
                        else
                        {
                            MessageBox.Show("Failed to update rejection status. The order may have been processed by someone else.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnWithdraw_Click(object sender, EventArgs e)
        {
            if (dgv_OS.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to withdraw.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewCell selectedCell = dgv_OS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string orderId = selectedRow.Cells["OrderID"].Value?.ToString();
            string orderSource = selectedRow.Cells["OrderSource"].Value?.ToString();
            string requesterID = selectedRow.Cells["RequesterID"].Value?.ToString();
            string checkStatus = selectedRow.Cells["CheckStatus"].Value?.ToString();
            string approveStatus = selectedRow.Cells["ApproveStatus"].Value?.ToString();

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(orderSource) || string.IsNullOrEmpty(requesterID))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!string.IsNullOrEmpty(checkStatus) && checkStatus == "Checked")
            {
                MessageBox.Show("This order has already been checked and cannot be withdrawn.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(approveStatus) && approveStatus == "Approved")
            {
                MessageBox.Show("This order has already been approved and cannot be withdrawn.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isHRAdmin = false;
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = "SELECT AA FROM tbl_Users WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", loggedInUser);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string AA = reader["AA"].ToString();
                                isHRAdmin = (AA == "1");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking user access: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Error checking user access: {ex.Message}");
                return;
            }

            if (requesterID != loggedInUser)
            {
                MessageBox.Show("You can only withdraw your own orders.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tableName = orderSource == "Internal" ? "tbl_InternalFoodOrder" : "tbl_ExternalFoodOrder";

            DialogResult result = MessageBox.Show($"Are you sure you want to withdraw Order ID: {orderId}?", "Confirm Withdrawal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return;
            }

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = $"DELETE FROM {tableName} WHERE OrderID = @OrderID";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Successfully withdrawn.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                     cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString(),
                                     cmbOS_Occasion.SelectedItem?.ToString() == "All" ? null : cmbOS_Occasion.SelectedItem?.ToString(),
                                     dtpStart.Value == dtpStart.MinDate ? null : (DateTime?)dtpStart.Value,
                                     dtpEnd.Value == dtpEnd.MinDate ? null : (DateTime?)dtpEnd.Value);
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
                    Debug.WriteLine($"Error withdrawing order: {ex.Message}");
                }
            }
        }
        private void btNext_Click(object sender, EventArgs e)
        {
            string selectedOccasion = cmbOccasion.SelectedItem?.ToString();
            string eventText = txtEvent.Text.Trim();
            DateTime eventDate;
            if (!DateTime.TryParse(dtRequest.Text, out eventDate))
            {
                MessageBox.Show("Invalid request date format. Please use dd.MM.yyyy.", "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DateTime? deliveryTime = dtDelivery.Value;

            if (string.IsNullOrEmpty(selectedOccasion))
            {
                MessageBox.Show("Please select an occasion before proceeding.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(eventText))
            {
                MessageBox.Show("Please enter an event before proceeding.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (eventDate.Date >= deliveryTime?.Date)
            {
                MessageBox.Show("Delivery date cannot be on or before the request date.", "Invalid Date Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (selectedOccasion == "Internal")
            {
                Form_Home.sharedLabel.Text = "Admin > Meal Request > Internal";
                Form_Home.sharedButton4.Visible = false;
                Form_Home.sharedButton5.Visible = false;
                Form_Home.sharedButton6.Visible = false;
                UC_Meal_Internal ug = new UC_Meal_Internal(eventText, eventDate, deliveryTime, loggedInUser, loggedInDepart);
                addControls(ug);
            }
            else if (selectedOccasion == "External")
            {
                Form_Home.sharedLabel.Text = "Admin > Meal Request > External";
                Form_Home.sharedButton4.Visible = false;
                Form_Home.sharedButton5.Visible = false;
                Form_Home.sharedButton6.Visible = false;
                UC_Meal_External ug = new UC_Meal_External(eventText, eventDate, deliveryTime, loggedInUser, loggedInDepart);
                addControls(ug);
            }
        }
        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (dgv_OS.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to check.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewCell selectedCell = dgv_OS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string orderId = selectedRow.Cells["OrderID"].Value?.ToString();
            string orderSource = selectedRow.Cells["OrderSource"].Value?.ToString();
            string checkStatus = selectedRow.Cells["CheckStatus"].Value?.ToString();
            string approveStatus = selectedRow.Cells["ApproveStatus"].Value?.ToString();

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(orderSource))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!string.IsNullOrEmpty(checkStatus) && checkStatus == "Checked")
            {
                MessageBox.Show("This order has already been checked.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(approveStatus) && approveStatus == "Approved")
            {
                MessageBox.Show("This order has already been approved and cannot be checked again.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(checkStatus) && checkStatus == "Rejected")
            {
                MessageBox.Show("This order has already been rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(approveStatus) && approveStatus == "Rejected")
            {
                MessageBox.Show("This order has already been rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Confirmation prompt before checking
            DialogResult result = MessageBox.Show($"Are you sure you want to check Order ID: {orderId}?", "Confirm Checking", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                return; // Exit if user clicks "No" or closes the dialog
            }

            string tableName = orderSource == "Internal" ? "tbl_InternalFoodOrder" : "tbl_ExternalFoodOrder";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = $@"UPDATE {tableName} 
                             SET CheckStatus = @CheckStatus, 
                                 CheckedBy = @CheckedBy, 
                                 CheckedDate = @CheckedDate, 
                                 CheckedDepartment = @CheckedDepartment
                             WHERE OrderID = @OrderID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@CheckStatus", "Checked");
                        cmd.Parameters.AddWithValue("@CheckedBy", loggedInUser);
                        cmd.Parameters.AddWithValue("@CheckedDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@CheckedDepartment", loggedInDepart);
                        cmd.Parameters.AddWithValue("@OrderID", orderId);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Order status updated to Checked.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                     cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString(),
                                     cmbOS_Occasion.SelectedItem?.ToString() == "All" ? null : cmbOS_Occasion.SelectedItem?.ToString(),
                                     dtpStart.Value == dtpStart.MinDate ? null : (DateTime?)dtpStart.Value,
                                     dtpEnd.Value == dtpEnd.MinDate ? null : (DateTime?)dtpEnd.Value);
                            /*

//*************************++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
              /*       

                            List<string> approverEmails = new List<string>();
                            string getApproversQuery = @"
                                                        SELECT A.Department, A.Username, B.Email, C.AccessLevel
                                                        FROM tbl_Users A
                                                        LEFT JOIN tbl_UserDetail B ON A.IndexNo = B.IndexNo
                                                        LEFT JOIN tbl_UsersLevel C ON A.Position = C.TitlePosition
                                                        WHERE Department = 'HR & ADMIN' AND AccessLevel > 1 AND AccessLevel < 3";  // First level account approver

                            using (SqlCommand cmd1 = new SqlCommand(getApproversQuery, con))
                            using (SqlDataReader reader = cmd1.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string email = reader["Email"]?.ToString();
                                    if (!string.IsNullOrWhiteSpace(email))
                                    {
                                        approverEmails.Add(email);
                                    }
                                }
                            }

                            string requesterName = "";
                            string EventDetai = "";
                            string OccasionTy = "";
                            string OrderI = "";
                            DateTime requestDate = DateTime.MinValue;
                            DateTime DelrequestDate = DateTime.MinValue;
                            

                            string getClaimDetailsQuery = $@"
                                                            SELECT A.OrderID, A.RequesterID, A.OccasionType, A.RequestDate, A.DeliveryDate, A.EventDetails, B.Email
                                                            FROM {tableName} A
                                                            LEFT JOIN tbl_UserDetail B ON A.RequesterID = B.Username
                                                            WHERE OrderID = @OrderID";


                            using (SqlCommand emailCmd = new SqlCommand(getClaimDetailsQuery, con))
                            {
                                emailCmd.Parameters.Add("@OrderID", SqlDbType.NVarChar).Value = orderId;

                                using (SqlDataReader reader = emailCmd.ExecuteReader())
                                {
                                  
                                    if (reader.Read())
                                    {
                                        requesterName = reader["RequesterID"]?.ToString();
                                        EventDetai = reader["EventDetails"]?.ToString();
                                        OccasionTy = reader["OccasionType"]?.ToString();
                                        OrderI = reader["OrderID"]?.ToString();
                                        requestDate = reader["RequestDate"] != DBNull.Value
                                            ? Convert.ToDateTime(reader["RequestDate"])
                                            : DateTime.MinValue;
                                        DelrequestDate = reader["DeliveryDate"] != DBNull.Value
                                            ? Convert.ToDateTime(reader["DeliveryDate"])
                                            : DateTime.MinValue;
                                    }
                                }
                            }

                            if (approverEmails.Count > 0)
                            {
                                string formattedDate = requestDate.ToString("dd/MM/yyyy");
                                string formattedDate1 = DelrequestDate.ToString("dd/MM/yyyy");
                                string subject = "HEM Admin Accessibility Notification: New Canteen Food Request Awaiting For Your Review And Approval";
                                string body = $@"
                                                    <p>Dear Mr./Ms. {requesterName},</p>
                                                    p>A new <strong>Canteen Food Request</strong> has been <strong>checked</strong> by <strong>Mr./Ms. {UserSession.loggedInName}</strong></p>

                    
                                                <p><u>Canteen Food Request Details:</u></p>
                                                <ul>
                                                    <li><strong>Order ID:</strong> {orderId}</li>
                                                    <li><strong>Occasion Type:</strong> {orderSource}</li>
                                                    <li><strong>Event Detail:</strong> {EventDetai}</li>
                                                    <li><strong>Request Date:</strong> {formattedDate}</li>
                                                    <li><strong>Delivery Date:</strong> {formattedDate1}</li>
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
              */
                            //++++++++++++++++++++++++++++++++++++++++++                  EMAIL FX               ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                        }
                        else
                        {
                            MessageBox.Show("Failed to update order status.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void UC_Food_Load(object sender, EventArgs e)
        {
            CheckUserAccess();
            LoadData();
        }
        private bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
        private void BindDataGridView(DataTable dt)
        {
            dgv_OS.AutoGenerateColumns = false;
            dgv_OS.Columns.Clear();

            dgv_OS.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new System.Drawing.Font("Arial", 11, FontStyle.Bold),
            };

            int fixedColumnWidth = 150;

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "OrderID",
                HeaderText = "Order ID",
                DataPropertyName = "OrderID",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "RequesterID",
                HeaderText = "Requester",
                DataPropertyName = "RequesterID",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "OrderSource",
                HeaderText = "Occasion Type",
                DataPropertyName = "OrderSource",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "OrderType",
                HeaderText = "Order Type",
                DataPropertyName = "OrderType",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "EventDetails",
                HeaderText = "Event",
                DataPropertyName = "EventDetails",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "DeliveryDate",
                HeaderText = "Delivery Date",
                DataPropertyName = "DeliveryDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "CheckStatus",
                HeaderText = "Admin Status Check",
                DataPropertyName = "CheckStatus",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "CheckedBy",
                HeaderText = "Checked By",
                DataPropertyName = "CheckedBy",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "CheckedDate",
                HeaderText = "Checked Date",
                DataPropertyName = "CheckedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ApproveStatus",
                HeaderText = "Admin HOD Status Check",
                DataPropertyName = "ApproveStatus",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ApprovedBy",
                HeaderText = "Approved By",
                DataPropertyName = "ApprovedBy",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ApprovedDate",
                HeaderText = "Approved Date",
                DataPropertyName = "ApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new System.Drawing.Font("Arial", 11)
                },
            });

            dgv_OS.DataSource = dt;
            dgv_OS.CellBorderStyle = DataGridViewCellBorderStyle.None;
            Debug.WriteLine("DataGridView updated successfully.");
        }
        private void btnView_Click(object sender, EventArgs e)
        {
            ViewExistingPDF();
        }
        private void ViewExistingPDF()
        {
            if (dgv_OS.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to view the PDF.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewCell selectedCell = dgv_OS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string orderId = selectedRow.Cells["OrderID"].Value?.ToString();
            string orderSource = selectedRow.Cells["OrderSource"].Value?.ToString();
            string requesterID = selectedRow.Cells["RequesterID"].Value?.ToString();

            if (string.IsNullOrEmpty(orderId) || string.IsNullOrEmpty(orderSource))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (requesterID != UserSession.LoggedInUser && UserSession.loggedInDepart != "HR & ADMIN")
            {
                MessageBox.Show("You can only view your own orders.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            byte[] pdfBytes = orderSource == "Internal" ? GenerateInternalPDF(orderId) : GenerateExternalPDF(orderId);
            ViewPdf(pdfBytes);
        }
        private byte[] GenerateInternalPDF(string orderId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                Dictionary<string, object> orderDetails = new Dictionary<string, object>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                     SELECT OrderID, RequesterID, Department, OccasionType, RequestDate, DeliveryDate, EventDetails,
                     Menu, Fruit, Snack, Drink1, HOTorCOLD1, Drink2, HOTorCOLD2, No_pax, DeliveryTime, Remark, DeliveryPlace,
                     CheckStatus, CheckedBy, CheckedDate, ApproveStatus, ApprovedBy, ApprovedDate, CheckedDepartment, ApprovedDepartment
                     FROM tbl_InternalFoodOrder
                     WHERE OrderID = @OrderID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orderDetails["OrderID"] = reader["OrderID"].ToString();
                                orderDetails["RequesterID"] = reader["RequesterID"].ToString();
                                orderDetails["Department"] = reader["Department"].ToString();
                                orderDetails["OccasionType"] = reader["OccasionType"].ToString();
                                orderDetails["RequestDate"] = reader["RequestDate"];
                                orderDetails["DeliveryDate"] = reader["DeliveryDate"];
                                orderDetails["EventDetails"] = reader["EventDetails"].ToString();
                                orderDetails["Menu"] = reader["Menu"] != DBNull.Value ? reader["Menu"].ToString() : "-";
                                orderDetails["Fruit"] = reader["Fruit"] != DBNull.Value ? reader["Fruit"].ToString() : "-";
                                orderDetails["Snack"] = reader["Snack"] != DBNull.Value ? reader["Snack"].ToString() : "-";
                                orderDetails["Drink1"] = reader["Drink1"] != DBNull.Value ? reader["Drink1"].ToString() : "-";
                                orderDetails["HOTorCOLD1"] = reader["HOTorCOLD1"] != DBNull.Value ? reader["HOTorCOLD1"].ToString() : "-";
                                orderDetails["Drink2"] = reader["Drink2"] != DBNull.Value ? reader["Drink2"].ToString() : "-";
                                orderDetails["HOTorCOLD2"] = reader["HOTorCOLD2"] != DBNull.Value ? reader["HOTorCOLD2"].ToString() : "-";
                                orderDetails["No_pax"] = reader["No_pax"] != DBNull.Value ? reader["No_pax"].ToString() : "-";
                                orderDetails["DeliveryTime"] = reader["DeliveryTime"] != DBNull.Value ? reader["DeliveryTime"].ToString() : "-";
                                orderDetails["DeliveryPlace"] = reader["DeliveryPlace"] != DBNull.Value ? reader["DeliveryPlace"].ToString() : "-";
                                orderDetails["Remark"] = reader["Remark"] != DBNull.Value ? reader["Remark"].ToString() : "-";
                                orderDetails["CheckStatus"] = reader["CheckStatus"] != DBNull.Value ? reader["CheckStatus"].ToString() : "";
                                orderDetails["CheckedBy"] = reader["CheckedBy"] != DBNull.Value ? reader["CheckedBy"].ToString() : "";
                                orderDetails["CheckedDate"] = reader["CheckedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CheckedDate"]).ToString("dd.MM.yyyy HH:mm") : "";
                                orderDetails["ApproveStatus"] = reader["ApproveStatus"] != DBNull.Value ? reader["ApproveStatus"].ToString() : "";
                                orderDetails["ApprovedBy"] = reader["ApprovedBy"] != DBNull.Value ? reader["ApprovedBy"].ToString() : "";
                                orderDetails["ApprovedDate"] = reader["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ApprovedDate"]).ToString("dd.MM.yyyy HH:mm") : "";
                                orderDetails["CheckedDepartment"] = reader["CheckedDepartment"] != DBNull.Value ? reader["CheckedDepartment"].ToString() : "-";
                                orderDetails["ApprovedDepartment"] = reader["ApprovedDepartment"] != DBNull.Value ? reader["ApprovedDepartment"].ToString() : "-";
                            }
                            else
                            {
                                MessageBox.Show("Order not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return null;
                            }
                        }
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4.Rotate(), 36f, 36f, 36f, 36f);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new PdfPageEventHelper();
                    writer.CloseStream = false;
                    writer.PageEvent = new WatermarkPageEvent();
                    document.Open();

                    iTextSharp.text.Font titleFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font addressFont = FontFactory.GetFont("Helvetica", 8f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font titleFont1 = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font headerFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font bodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font italicBodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.ITALIC, BaseColor.BLACK);

                    string imagePath = Path.Combine(WinFormsApp.StartupPath, "Img", "hosiden.jpg");
                    //string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo hosiden.jpg");
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
                    titlePara.Add(new Chunk("CANTEEN MEAL REQUEST FORM", titleFont));
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

                    PdfPTable detailsTable = new PdfPTable(4);
                    detailsTable.WidthPercentage = 100;
                    detailsTable.SetWidths(new float[] { 0.22f, 0.5f, 0.24f, 0.7f });
                    detailsTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    detailsTable.DefaultCell.Padding = 2f;
                    detailsTable.SpacingBefore = 5f;

                    detailsTable.AddCell(new Phrase("OrderID      :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["OrderID"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Request date:", bodyFont));
                    detailsTable.AddCell(new Phrase(Convert.ToDateTime(orderDetails["RequestDate"]).ToString("dd.MM.yyyy"), bodyFont));

                    detailsTable.AddCell(new Phrase("Requester  :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["RequesterID"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Delivery date:", bodyFont));
                    detailsTable.AddCell(new Phrase(Convert.ToDateTime(orderDetails["DeliveryDate"]).ToString("dd.MM.yyyy"), bodyFont));

                    detailsTable.AddCell(new Phrase("Department:", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["Department"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Event details:", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["EventDetails"].ToString(), bodyFont));

                    leftCell.AddElement(detailsTable);

                    PdfPCell rightCell = new PdfPCell();
                    rightCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    rightCell.Padding = 0f;

                    Paragraph checkedPara = new Paragraph();
                    string checkedBy = orderDetails["CheckedBy"].ToString();
                    string checkedDate = orderDetails["CheckedDate"].ToString();
                    string checkedDepartment = orderDetails["CheckedDepartment"].ToString();
                    if (string.IsNullOrEmpty(checkedBy))
                    {
                        checkedPara.Add(new Chunk("Checked by : Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(checkedDate))
                        {
                            checkedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                        }
                        string checkedText = string.IsNullOrEmpty(checkedDepartment) || checkedDepartment == "-"
                            ? $"Checked by : {checkedBy}   {checkedDate}"
                            : $"Checked by : {checkedBy}   {checkedDate}   \n                      {checkedDepartment}";
                        checkedPara.Add(new Chunk(checkedText, bodyFont));
                    }
                    checkedPara.SpacingBefore = 0f;
                    rightCell.AddElement(checkedPara);

                    Paragraph checkedAdminPara = new Paragraph();
                    checkedAdminPara.Add(new Chunk("", bodyFont));
                    checkedAdminPara.SpacingBefore = 0f;
                    checkedAdminPara.SpacingAfter = 0f;
                    rightCell.AddElement(checkedAdminPara);

                    Paragraph approvedPara = new Paragraph();
                    string approvedBy = orderDetails["ApprovedBy"].ToString();
                    string approvedDate = orderDetails["ApprovedDate"].ToString();
                    string approvedDepartment = orderDetails["ApprovedDepartment"].ToString();
                    if (string.IsNullOrEmpty(approvedBy))
                    {
                        approvedPara.Add(new Chunk("Approved by: Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(approvedDate))
                        {
                            approvedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                        }
                        string approvedText = string.IsNullOrEmpty(approvedDepartment) || approvedDepartment == "-"
                            ? $"Approved by: {approvedBy}   {approvedDate}"
                            : $"Approved by: {approvedBy}   {approvedDate}   \n                      {approvedDepartment}";
                        approvedPara.Add(new Chunk(approvedText, bodyFont));
                    }
                    approvedPara.SpacingBefore = 0f;
                    rightCell.AddElement(approvedPara);
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    // Add watermark with logo.png behind Account Approval name and date
                    string imagePath1 = Path.Combine(WinFormsApp.StartupPath, "Img", "logo.png");
                    if (File.Exists(imagePath1) && !string.IsNullOrEmpty(approvedBy)) // Only add watermark if approved
                    {
                        iTextSharp.text.Image watermark = iTextSharp.text.Image.GetInstance(imagePath1);
                        float xPosition = document.PageSize.Width * 0.72f; // Approximately 70% of page width for right column (e.g., ~420f for A4)
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

                    Paragraph approvedHrPara = new Paragraph();
                    approvedHrPara.Add(new Chunk("", bodyFont));
                    approvedHrPara.SpacingBefore = 0f;
                    approvedHrPara.SpacingAfter = 0f;
                    rightCell.AddElement(approvedHrPara);

                    Paragraph issuedPara = new Paragraph();
                    issuedPara.Add(new Chunk("Received by: Canteen", bodyFont));
                    issuedPara.SpacingBefore = 0f;
                    issuedPara.SpacingAfter = 0f;
                    rightCell.AddElement(issuedPara);

                    mainLayoutTable.AddCell(leftCell);
                    mainLayoutTable.AddCell(rightCell);
                    document.Add(mainLayoutTable);

                    Paragraph detailsHeading = new Paragraph("Details of the order:", bodyFont);
                    detailsHeading.SpacingBefore = 10f;
                    detailsHeading.SpacingAfter = 5f;
                    document.Add(detailsHeading);

                    PdfPTable detailsTable2 = new PdfPTable(2);
                    detailsTable2.WidthPercentage = 100;
                    detailsTable2.SetWidths(new float[] { 0.5f, 3f });

                    string mealType = "-";
                    if (!string.IsNullOrEmpty(orderDetails["Menu"].ToString()))
                    {
                        string menu = orderDetails["Menu"].ToString().ToLower();
                        if (menu.Contains("breakfast") || menu.Contains("nasi lemak") || menu.Contains("telur"))
                            mealType = "Breakfast";
                        else if (menu.Contains("lunch") || menu.Contains("ayam") || menu.Contains("ikan"))
                            mealType = "Lunch";
                        else if (menu.Contains("tea"))
                            mealType = "Tea";
                        else if (menu.Contains("dinner") || menu.Contains("goreng"))
                            mealType = "Dinner";
                    }

                    AddStyledTableRow(detailsTable2, "Meal Type:", mealType, bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Dish:", orderDetails["Menu"].ToString(), bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Fruit:", orderDetails["Fruit"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Other:", orderDetails["Snack"].ToString(), bodyFont, italicBodyFont, 0);

                    string drink1Value = orderDetails["Drink1"].ToString();
                    string hotCold1Value = orderDetails["HOTorCOLD1"].ToString();
                    string combinedDrink1Value = drink1Value;
                    if (hotCold1Value != "-" && drink1Value != "-")
                    {
                        combinedDrink1Value = $"{drink1Value} ({hotCold1Value})";
                    }
                    AddStyledTableRow(detailsTable2, "Drink1 (Hot/Cold):", combinedDrink1Value, bodyFont, italicBodyFont, 1);

                    string drink2Value = orderDetails["Drink2"].ToString();
                    string hotCold2Value = orderDetails["HOTorCOLD2"].ToString();
                    string combinedDrink2Value = drink2Value;
                    if (hotCold2Value != "-" && drink2Value != "-")
                    {
                        combinedDrink2Value = $"{drink2Value} ({hotCold2Value})";
                    }
                    AddStyledTableRow(detailsTable2, "Drink2 (Hot/Cold):", combinedDrink2Value, bodyFont, italicBodyFont, 0);

                    AddStyledTableRow(detailsTable2, "No. of Pax:", orderDetails["No_pax"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Delivery Time:", orderDetails["DeliveryTime"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Delivery Place:", orderDetails["DeliveryPlace"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Remarks:", orderDetails["Remark"].ToString(), bodyFont, italicBodyFont, 0, true);

                    document.Add(detailsTable2);

                    Paragraph footer = new Paragraph("This is a computer generated document, no signature is required | Generated on: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"), bodyFont);
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
        private byte[] GenerateExternalPDF(string orderId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                Dictionary<string, object> orderDetails = new Dictionary<string, object>();
                Dictionary<string, List<string>> menuItems = new Dictionary<string, List<string>>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
        SELECT OrderID, RequesterID, Department, OccasionType, RequestDate, DeliveryDate, EventDetails,
               B_Nofpax_P, B_DeliveryTime, L_Nofpax_B, L_Nofpax_P, L_DeliveryTime, T_Nofpax_P, T_DeliveryTime, Remark, DeliveryPlace,
               CheckStatus, CheckedBy, CheckedDate, ApproveStatus, ApprovedBy, ApprovedDate, CheckedDepartment, ApprovedDepartment, BreakfastPackage, LunchPackage, TeaPackage
        FROM tbl_ExternalFoodOrder
        WHERE OrderID = @OrderID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orderDetails["OrderID"] = reader["OrderID"].ToString();
                                orderDetails["RequesterID"] = reader["RequesterID"].ToString();
                                orderDetails["Department"] = reader["Department"].ToString();
                                orderDetails["OccasionType"] = reader["OccasionType"].ToString();
                                orderDetails["RequestDate"] = reader["RequestDate"];
                                orderDetails["DeliveryDate"] = reader["DeliveryDate"];
                                orderDetails["EventDetails"] = reader["EventDetails"].ToString();
                                orderDetails["B_Nofpax_P"] = reader["B_Nofpax_P"] != DBNull.Value ? reader["B_Nofpax_P"].ToString() : "-";
                                orderDetails["B_DeliveryTime"] = reader["B_DeliveryTime"] != DBNull.Value ? reader["B_DeliveryTime"].ToString() : "-";
                                orderDetails["L_Nofpax_B"] = reader["L_Nofpax_B"] != DBNull.Value ? reader["L_Nofpax_B"].ToString() : "-";
                                orderDetails["L_Nofpax_P"] = reader["L_Nofpax_P"] != DBNull.Value ? reader["L_Nofpax_P"].ToString() : "-";
                                orderDetails["L_DeliveryTime"] = reader["L_DeliveryTime"] != DBNull.Value ? reader["L_DeliveryTime"].ToString() : "-";
                                orderDetails["T_Nofpax_P"] = reader["T_Nofpax_P"] != DBNull.Value ? reader["T_Nofpax_P"].ToString() : "-";
                                orderDetails["T_DeliveryTime"] = reader["T_DeliveryTime"] != DBNull.Value ? reader["T_DeliveryTime"].ToString() : "-";
                                orderDetails["DeliveryPlace"] = reader["DeliveryPlace"] != DBNull.Value ? reader["DeliveryPlace"].ToString() : "-";
                                orderDetails["Remark"] = reader["Remark"] != DBNull.Value ? reader["Remark"].ToString() : "-";
                                orderDetails["CheckStatus"] = reader["CheckStatus"] != DBNull.Value ? reader["CheckStatus"].ToString() : "";
                                orderDetails["CheckedBy"] = reader["CheckedBy"] != DBNull.Value ? reader["CheckedBy"].ToString() : "";
                                orderDetails["CheckedDate"] = reader["CheckedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CheckedDate"]).ToString("dd.MM.yyyy HH:mm") : "";
                                orderDetails["ApproveStatus"] = reader["ApproveStatus"] != DBNull.Value ? reader["ApproveStatus"].ToString() : "";
                                orderDetails["ApprovedBy"] = reader["ApprovedBy"] != DBNull.Value ? reader["ApprovedBy"].ToString() : "";
                                orderDetails["ApprovedDate"] = reader["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ApprovedDate"]).ToString("dd.MM.yyyy HH:mm") : "";
                                orderDetails["CheckedDepartment"] = reader["CheckedDepartment"] != DBNull.Value ? reader["CheckedDepartment"].ToString() : "-";
                                orderDetails["ApprovedDepartment"] = reader["ApprovedDepartment"] != DBNull.Value ? reader["ApprovedDepartment"].ToString() : "-";
                                orderDetails["BreakfastPackage"] = reader["BreakfastPackage"] != DBNull.Value ? reader["BreakfastPackage"].ToString() : "-";
                                orderDetails["LunchPackage"] = reader["LunchPackage"] != DBNull.Value ? reader["LunchPackage"].ToString() : "-";
                                orderDetails["TeaPackage"] = reader["TeaPackage"] != DBNull.Value ? reader["TeaPackage"].ToString() : "-";
                            }
                            else
                            {
                                MessageBox.Show("Order not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return null;
                            }
                        }
                    }

                    string breakfastPackage = orderDetails["BreakfastPackage"].ToString();
                    string lunchPackage = orderDetails["LunchPackage"].ToString();
                    string teaPackage = orderDetails["TeaPackage"].ToString();

                    // Only fetch menu items for packages that are not "-"
                    string[] meals = { "BREAKFAST", "LUNCH", "TEA" };
                    string[] styles = { "PACKING", "BUFFET" };
                    foreach (var meal in meals)
                    {
                        foreach (var style in styles)
                        {
                            if (meal == "BREAKFAST" && style == "BUFFET") continue;
                            if (meal == "TEA" && style == "BUFFET") continue;
                            string package = meal == "BREAKFAST" ? breakfastPackage : meal == "LUNCH" ? lunchPackage : teaPackage;
                            // Skip fetching if the package is "-"
                            if (package == "-") continue;

                            string menuQuery = @"
                SELECT Menu FROM tbl_Menu
                WHERE Package = @Package AND Meal = @Meal AND Style = @Style";
                            using (SqlCommand cmd = new SqlCommand(menuQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@Package", package);
                                cmd.Parameters.AddWithValue("@Meal", meal);
                                cmd.Parameters.AddWithValue("@Style", style);
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    List<string> items = new List<string>();
                                    while (reader.Read())
                                    {
                                        items.Add(reader["Menu"].ToString());
                                    }
                                    menuItems[$"{meal}_{style}"] = items;
                                }
                            }
                        }
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4.Rotate(), 36f, 36f, 36f, 36f);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new PdfPageEventHelper();
                    writer.PageEvent = new WatermarkPageEvent();
                    document.Open();

                    iTextSharp.text.Font titleFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font addressFont = FontFactory.GetFont("Helvetica", 8f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font titleFont1 = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font headerFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font bodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font italicBodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.ITALIC, BaseColor.BLACK);
                    iTextSharp.text.Font sectionTitleFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.BOLD, new BaseColor(0, 51, 102));
                    iTextSharp.text.Font mealsHeadingFont = FontFactory.GetFont("Helvetica", 13f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

                    BaseColor lightGray = new BaseColor(240, 240, 240);
                    BaseColor darkGray = new BaseColor(150, 150, 150);

                    string imagePath = Path.Combine(WinFormsApp.StartupPath, "Img", "hosiden.jpg");
                    if (File.Exists(imagePath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imagePath);
                        logo.ScaleToFit(100f, 100f);
                        logo.Alignment = Element.ALIGN_CENTER;
                        document.Add(logo);
                    }
                    else
                    {
                        Paragraph companyPara = new Paragraph("Hosiden Electronics (M) Sdn Bhd", headerFont);
                        companyPara.Alignment = Element.ALIGN_CENTER;
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

                    PdfPTable mainLayoutTable = new PdfPTable(2);
                    mainLayoutTable.WidthPercentage = 100;
                    mainLayoutTable.SetWidths(new float[] { 1.5f, 0.8f });
                    mainLayoutTable.SpacingBefore = 10f;

                    PdfPCell leftCell = new PdfPCell();
                    leftCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    leftCell.Padding = 0f;

                    PdfPTable detailsTable = new PdfPTable(4);
                    detailsTable.WidthPercentage = 100;
                    detailsTable.SetWidths(new float[] { 0.22f, 0.5f, 0.24f, 0.7f });
                    detailsTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    detailsTable.DefaultCell.Padding = 2f;
                    detailsTable.SpacingBefore = 5f;

                    detailsTable.AddCell(new Phrase("OrderID      :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["OrderID"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Request date:", bodyFont));
                    detailsTable.AddCell(new Phrase(Convert.ToDateTime(orderDetails["RequestDate"]).ToString("dd.MM.yyyy"), bodyFont));

                    detailsTable.AddCell(new Phrase("Requester  :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["RequesterID"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Delivery date:", bodyFont));
                    detailsTable.AddCell(new Phrase(Convert.ToDateTime(orderDetails["DeliveryDate"]).ToString("dd.MM.yyyy"), bodyFont));

                    detailsTable.AddCell(new Phrase("Department:", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["Department"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Event details:", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["EventDetails"].ToString(), bodyFont));

                    leftCell.AddElement(detailsTable);

                    PdfPCell rightCell = new PdfPCell();
                    rightCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    rightCell.Padding = 0f;

                    Paragraph checkedPara = new Paragraph();
                    string checkedBy = orderDetails["CheckedBy"].ToString();
                    string checkedDate = orderDetails["CheckedDate"].ToString();
                    string checkedDepartment = orderDetails["CheckedDepartment"].ToString();
                    if (string.IsNullOrEmpty(checkedBy))
                    {
                        checkedPara.Add(new Chunk("Checked by : Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(checkedDate))
                        {
                            checkedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                        }
                        string checkedText = string.IsNullOrEmpty(checkedDepartment) || checkedDepartment == "-"
                            ? $"Checked by : {checkedBy}   {checkedDate}"
                            : $"Checked by : {checkedBy}   {checkedDate}   \n                      {checkedDepartment}";
                        checkedPara.Add(new Chunk(checkedText, bodyFont));
                    }
                    checkedPara.SpacingBefore = 0f;
                    rightCell.AddElement(checkedPara);

                    Paragraph checkedAdminPara = new Paragraph();
                    checkedAdminPara.Add(new Chunk("", bodyFont));
                    checkedAdminPara.SpacingBefore = 0f;
                    checkedAdminPara.SpacingAfter = 0f;
                    rightCell.AddElement(checkedAdminPara);

                    Paragraph approvedPara = new Paragraph();
                    string approvedBy = orderDetails["ApprovedBy"].ToString();
                    string approvedDate = orderDetails["ApprovedDate"].ToString();
                    string approvedDepartment = orderDetails["ApprovedDepartment"].ToString();
                    if (string.IsNullOrEmpty(approvedBy))
                    {
                        approvedPara.Add(new Chunk("Approved by: Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(approvedDate))
                        {
                            approvedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                        }
                        string approvedText = string.IsNullOrEmpty(approvedDepartment) || approvedDepartment == "-"
                            ? $"Approved by: {approvedBy}   {approvedDate}"
                            : $"Approved by: {approvedBy}   {approvedDate}   \n                      {approvedDepartment}";
                        approvedPara.Add(new Chunk(approvedText, bodyFont));
                    }
                    approvedPara.SpacingBefore = 0f;
                    rightCell.AddElement(approvedPara);
                    // Add watermark with logo.png behind Account Approval name and date
                    string imagePath1 = Path.Combine(WinFormsApp.StartupPath, "Img", "logo.png");
                    if (File.Exists(imagePath1) && !string.IsNullOrEmpty(approvedBy)) // Only add watermark if approved
                    {
                        iTextSharp.text.Image watermark = iTextSharp.text.Image.GetInstance(imagePath1);
                        float xPosition = document.PageSize.Width * 0.72f; // Approximately 70% of page width for right column (e.g., ~420f for A4)
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

                    Paragraph approvedHrPara = new Paragraph();
                    approvedHrPara.Add(new Chunk("", bodyFont));
                    approvedHrPara.SpacingBefore = 0f;
                    approvedHrPara.SpacingAfter = 0f;
                    rightCell.AddElement(approvedHrPara);

                    Paragraph issuedPara = new Paragraph();
                    issuedPara.Add(new Chunk("Received by: Canteen", bodyFont));
                    issuedPara.SpacingBefore = 0f;
                    issuedPara.SpacingAfter = 0f;
                    rightCell.AddElement(issuedPara);

                    mainLayoutTable.AddCell(leftCell);
                    mainLayoutTable.AddCell(rightCell);
                    document.Add(mainLayoutTable);

                    Paragraph detailsHeading = new Paragraph("Details of the Order:", bodyFont);
                    detailsHeading.SpacingBefore = 10f;
                    detailsHeading.SpacingAfter = 5f;
                    document.Add(detailsHeading);

                    string breakfastPackage = orderDetails["BreakfastPackage"].ToString();
                    string lunchPackage = orderDetails["LunchPackage"].ToString();
                    string teaPackage = orderDetails["TeaPackage"].ToString();

                    PdfPTable detailsTable2 = new PdfPTable(2);
                    detailsTable2.WidthPercentage = 100;
                    detailsTable2.SetWidths(new float[] { 0.8f, 3f });

                    AddStyledTableRow(detailsTable2, "Breakfast Package:", breakfastPackage, bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Breakfast Packing Pax:", orderDetails["B_Nofpax_P"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Breakfast Delivery Time:", orderDetails["B_DeliveryTime"].ToString(), bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Lunch Package:", lunchPackage, bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Lunch Buffet Pax:", orderDetails["L_Nofpax_B"].ToString(), bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Lunch Packing Pax:", orderDetails["L_Nofpax_P"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Lunch Delivery Time:", orderDetails["L_DeliveryTime"].ToString(), bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Tea Package:", teaPackage, bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Tea Packing Pax:", orderDetails["T_Nofpax_P"].ToString(), bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Tea Delivery Time:", orderDetails["T_DeliveryTime"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Delivery Place:", orderDetails["DeliveryPlace"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Remarks:", orderDetails["Remark"].ToString(), bodyFont, italicBodyFont, 0, true);

                    document.Add(detailsTable2);

                    // Only proceed with food items if at least one package is selected -----------------------------------------------------------------------------------------------
                    if (breakfastPackage != "-" || lunchPackage != "-" || teaPackage != "-")
                    {
                        
                        document.NewPage();
                        
                        PdfPTable foodItemsTable = new PdfPTable(2);
                        foodItemsTable.WidthPercentage = 100;
                        foodItemsTable.SetWidths(new float[] { 1f, 1f });
                        foodItemsTable.SpacingBefore = 15f;
                        //foodItemsTable.DefaultCell.BorderColor = darkGray;
                        foodItemsTable.KeepTogether = true;

                        PdfPCell headingCell = new PdfPCell(new Phrase($"Meals for Packages - Breakfast: {breakfastPackage}, Lunch: {lunchPackage}, Tea: {teaPackage}", mealsHeadingFont));
                        headingCell.Colspan = 2;
                        //headingCell.BackgroundColor = BaseColor.WHITE;
                        headingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                        //headingCell.BorderColor = darkGray;
                        headingCell.BorderWidth = 1.5f;
                        headingCell.Padding = 5f;
                        headingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        foodItemsTable.AddCell(headingCell);

                        // Conditionally add Breakfast Packing section
                        if (breakfastPackage != "-")
                        {
                            PdfPCell breakfastHeadingCell = new PdfPCell(new Phrase($"BREAKFAST PACKING: {orderDetails["B_Nofpax_P"]}", sectionTitleFont));
                            breakfastHeadingCell.Colspan = 2;
                            //breakfastHeadingCell.BackgroundColor = BaseColor.WHITE;
                            breakfastHeadingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            //breakfastHeadingCell.BorderColor = darkGray;
                            breakfastHeadingCell.BorderWidth = 1.5f;
                            breakfastHeadingCell.Padding = 5f;
                            breakfastHeadingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            foodItemsTable.AddCell(breakfastHeadingCell);

                            PdfPCell breakfastCell = new PdfPCell();
                            breakfastCell.Colspan = 2;
                            //breakfastCell.BackgroundColor = BaseColor.WHITE;
                            breakfastCell.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            //breakfastCell.BorderColor = darkGray;
                            breakfastCell.BorderWidth = 1.5f;
                            breakfastCell.Padding = 5f;
                            AddFoodItemsToCell(breakfastCell, menuItems["BREAKFAST_PACKING"], italicBodyFont);
                            foodItemsTable.AddCell(breakfastCell);
                        }

                        // Conditionally add Lunch section
                        if (lunchPackage != "-")
                        {
                            PdfPCell lunchHeadingCell = new PdfPCell(new Phrase("LUNCH", sectionTitleFont));
                            lunchHeadingCell.Colspan = 2;
                            //lunchHeadingCell.BackgroundColor = BaseColor.WHITE;
                            lunchHeadingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            //lunchHeadingCell.BorderColor = darkGray;
                            lunchHeadingCell.BorderWidth = 1.5f;
                            lunchHeadingCell.Padding = 5f;
                            lunchHeadingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            foodItemsTable.AddCell(lunchHeadingCell);

                            PdfPCell lunchBuffetCell = new PdfPCell();
                            //lunchBuffetCell.BackgroundColor = BaseColor.WHITE;
                            lunchBuffetCell.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            //lunchBuffetCell.BorderColor = darkGray;
                            lunchBuffetCell.BorderWidth = 1.5f;
                            lunchBuffetCell.Padding = 5f;
                            Paragraph lunchBuffetTitle = new Paragraph($"BUFFET: {orderDetails["L_Nofpax_B"]}", sectionTitleFont);
                            lunchBuffetTitle.SpacingAfter = 2f;
                            lunchBuffetCell.AddElement(lunchBuffetTitle);
                            AddFoodItemsToCell(lunchBuffetCell, menuItems["LUNCH_BUFFET"], italicBodyFont);
                            foodItemsTable.AddCell(lunchBuffetCell);

                            PdfPCell lunchPackingCell = new PdfPCell();
                            //lunchPackingCell.BackgroundColor = BaseColor.WHITE;
                            lunchPackingCell.Border = iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            //lunchPackingCell.BorderColor = darkGray;
                            lunchPackingCell.BorderWidth = 1.5f;
                            lunchPackingCell.Padding = 5f;
                            Paragraph lunchPackingTitle = new Paragraph($"PACKING: {orderDetails["L_Nofpax_P"]}", sectionTitleFont);
                            lunchPackingTitle.SpacingAfter = 2f;
                            lunchPackingCell.AddElement(lunchPackingTitle);
                            AddFoodItemsToCell(lunchPackingCell, menuItems["LUNCH_PACKING"], italicBodyFont);
                            foodItemsTable.AddCell(lunchPackingCell);
                        }

                        // Conditionally add Tea Packing section
                        if (teaPackage != "-")
                        {
                            PdfPCell teaHeadingCell = new PdfPCell(new Phrase($"TEA PACKING: {orderDetails["T_Nofpax_P"]}", sectionTitleFont));
                            teaHeadingCell.Colspan = 2;
                            //teaHeadingCell.BackgroundColor = BaseColor.WHITE;
                            teaHeadingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            //teaHeadingCell.BorderColor = darkGray;
                            teaHeadingCell.BorderWidth = 1.5f;
                            teaHeadingCell.Padding = 5f;
                            teaHeadingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            foodItemsTable.AddCell(teaHeadingCell);

                            PdfPCell teaCell = new PdfPCell();
                            teaCell.Colspan = 2;
                            //teaCell.BackgroundColor = BaseColor.WHITE;
                            teaCell.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            //teaCell.BorderColor = darkGray;
                            teaCell.BorderWidth = 1.5f;
                            teaCell.Padding = 5f;
                            AddFoodItemsToCell(teaCell, menuItems["TEA_PACKING"], italicBodyFont);
                            foodItemsTable.AddCell(teaCell);
                        }

                        document.Add(foodItemsTable);
                    }

                    Paragraph footer = new Paragraph("This is a computer generated document, no signature is required | Generated on: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"), bodyFont);
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
        private void AddFoodItemsToCell(PdfPCell cell, List<string> items, iTextSharp.text.Font font)
        {
            foreach (var item in items)
            {
                Paragraph p = new Paragraph($"- {item}", font);
                p.SpacingBefore = 1f;
                p.SpacingAfter = 1f;
                cell.AddElement(p);
            }
        }
        private void AddStyledTableRow(PdfPTable table, string label, string value, iTextSharp.text.Font labelFont, iTextSharp.text.Font valueFont, int rowIndex, bool multiLine = false)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont));
            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont)) { MinimumHeight = 20f };

            //labelCell.BackgroundColor = new BaseColor(255, 255, 255);
            //valueCell.BackgroundColor = new BaseColor(255, 255, 255);

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
        private void ViewPdf(byte[] pdfBytes)
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
        private void cmbOccasion_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtEvent_TextChanged(object sender, EventArgs e) { }
        private void dtDelivery_ValueChanged(object sender, EventArgs e) { }
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
    }
}