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
                    string query = "SELECT AA, MA FROM tbl_Users WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", loggedInUser);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string AA = reader["AA"].ToString();
                                string MA = reader["MA"].ToString();

                                // Set check, approve button, and labels visibility: hidden if AA = 1, visible if MA = 2
                                if (AA == "1")
                                {
                                    //GB_Authorization.Visible = false;
                                }
                                else if (MA == "2")
                                {
                                    //GB_Authorization.Visible = true;
                                }
                                else
                                {
                                    //GB_Authorization.Visible = false;
                                }
                            }
                            else
                            {
                                //GB_Authorization.Visible = false;
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
                    string query = "SELECT DISTINCT Department FROM tbl_Users WHERE Department IS NOT NULL";
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
            DateTime weekEnd = weekStart.AddDays(7); // End of the week (Saturday)
            if (!startDate.HasValue)
            {
                startDate = weekStart;
            }
            if (!endDate.HasValue)
            {
                endDate = weekEnd;
            }

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
                          AND RequestDate < @EndDate
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
                          AND RequestDate < @EndDate
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
                          AND RequestDate < @EndDate
                    UNION ALL
                    SELECT 'External' AS OrderSource,
                           OrderID, RequesterID, Department, OccasionType, EventDetails, RequestDate, DeliveryDate, CheckStatus,
                           CheckedBy, CheckedDate, ApproveStatus, ApprovedBy, ApprovedDate, OrderType
                    FROM tbl_ExternalFoodOrder
                    WHERE (@RequesterID IS NULL OR RequesterID = @RequesterID)
                          AND (@Department IS NULL OR Department = @Department)
                          AND RequestDate >= @StartDate
                          AND RequestDate < @EndDate
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

                        Debug.WriteLine($"Executing LoadData with RequesterID: {(string.IsNullOrEmpty(requesterID) ? "NULL" : requesterID)}, Department: {(string.IsNullOrEmpty(department) ? "NULL" : department)}, OccasionType: {(string.IsNullOrEmpty(occasionType) ? "NULL" : occasionType)}, StartDate: {startDate.Value.ToString("yyyy-MM-dd")}, EndDate: {endDate.Value.ToString("yyyy-MM-dd")}");

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

            Debug.WriteLine($"cmbDepart selected: {selectedDepartment}");

            // Update requester combo box based on selected department
            if (selectedDepartment == "All Departments" || string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
                LoadUsernames(); // Load all usernames
                Debug.WriteLine("Loading all usernames for 'All Departments'.");
            }
            else
            {
                // Load usernames for the selected department
                LoadUsernamesByDepartment(selectedDepartment);
                Debug.WriteLine($"Loading usernames for department: {selectedDepartment}");
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

            if (!isHRAdmin && requesterID != loggedInUser)
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
                Font = new Font("Arial", 11, FontStyle.Bold),
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
                },
            });

            dgv_OS.DataSource = dt;
            dgv_OS.CellBorderStyle = DataGridViewCellBorderStyle.None;
            Debug.WriteLine("DataGridView updated successfully.");
        }

        private void cmbOccasion_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtEvent_TextChanged(object sender, EventArgs e) { }
        private void dtDelivery_ValueChanged(object sender, EventArgs e) { }
    }
}