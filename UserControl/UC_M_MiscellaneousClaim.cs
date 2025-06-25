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

namespace HRAdmin.UserControl
{
    public partial class UC_M_MiscellaneousClaim : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private DataTable cachedData; // Declare cachedData
        private bool isNetworkErrorShown; // Declare isNetworkErrorShown
        private bool isNetworkUnavailable; // Declare isNetworkUnavailable

        public UC_M_MiscellaneousClaim(string username, string department)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            dtRequest.Text = DateTime.Now.ToString("dd.MM.yyyy");
            LoadUsernames();
            LoadDepartments();
            LoadData();
            cmbRequester.SelectedIndexChanged += cmbRequester_SelectedIndexChanged;
            cmbDepart.SelectedIndexChanged += cmbDepart_SelectedIndexChanged;
            cachedData = new DataTable(); // Initialize (replace with actual cache loading logic)
            isNetworkErrorShown = false;
            isNetworkUnavailable = false;
            this.Load += UC_Miscellaneous_Load;
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

            UC_Acc_Account ug = new UC_Acc_Account(loggedInUser, loggedInDepart);
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
                UC_M_Work ug = new UC_M_Work(loggedInUser, loggedInDepart, selectedType);
                addControls(ug);
            }
            else if (selectedType == "Benefit")
            {
                Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Benefit";
                UC_M_Benefit ug = new UC_M_Benefit(loggedInUser, loggedInDepart, selectedType);
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
                                    btnReject.Visible = true;
                                    btnApprove.Visible = true;
                                    // Labels remain visible unless neither condition is met
                                }
                                else if (MA == "2")
                                {
                                    btnReject.Visible = true;
                                    btnApprove.Visible = false;
                                    // Labels remain visible as default
                                }
                                else
                                {
                                    btnReject.Visible = false; // Default to hidden if neither condition is met
                                    btnApprove.Visible = false; // Default to hidden if neither condition is met
                                    label14.Visible = false; // Hide "Verification" label
                                    label15.Visible = false; // Hide ":" symbol
                                }
                            }
                            else
                            {
                                btnReject.Visible = false; // Hide if user not found
                                btnApprove.Visible = false; // Hide if user not found
                                label14.Visible = false; // Hide if user not found
                                label15.Visible = false; // Hide if user not found
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking user access: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnReject.Visible = false; // Hide on error to be safe
                btnApprove.Visible = false; // Hide on error to be safe
                label14.Visible = false; // Hide on error to be safe
                label15.Visible = false; // Hide on error to be safe
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

        private void LoadData(string requester = null, string department = null)
        {
            if (dgvMS == null)
            {
                MessageBox.Show("DataGridView is not initialized!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string filterType = cmbPeriod.SelectedItem?.ToString() ?? "Weekly"; // Default to Weekly if null
            string query = "";
            DateTime today = DateTime.Today;

            switch (filterType)
            {
                case "Daily":
                    query = @"
                SELECT SerialNo, Requester, Department, ExpensesType, RequestDate, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice, 
	                   HODApprovalStatus, ApprovedByHOD, HODApprovedDate, HRApprovalStatus, ApprovedByHR, HRApprovedDate, AccountApprovalStatus, 
	                   ApprovedByAccount, AccountApprovedDate 
                FROM tbl_MiscellaneousClaim
                WHERE CAST(RequestDate AS DATE) = @Today
                      AND (@Requester IS NULL OR Requester = @Requester)
                      AND (@Department IS NULL OR Department = @Department)
                ORDER BY RequestDate ASC";
                    break;

                case "Weekly":
                    query = @"
                SELECT SerialNo, Requester, Department, ExpensesType, RequestDate, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice, 
	                   HODApprovalStatus, ApprovedByHOD, HODApprovedDate, HRApprovalStatus, ApprovedByHR, HRApprovedDate, AccountApprovalStatus, 
	                   ApprovedByAccount, AccountApprovedDate 
                FROM tbl_MiscellaneousClaim
                WHERE RequestDate >= @WeekStart AND RequestDate < @WeekEnd
                      AND (@Requester IS NULL OR Requester = @Requester)
                      AND (@Department IS NULL OR Department = @Department)
                ORDER BY RequestDate ASC";

                    break;

                case "Monthly":
                    query = @"
                SELECT SerialNo, Requester, Department, ExpensesType, RequestDate, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice, 
	                   HODApprovalStatus, ApprovedByHOD, HODApprovedDate, HRApprovalStatus, ApprovedByHR, HRApprovedDate, AccountApprovalStatus, 
	                   ApprovedByAccount, AccountApprovedDate 
                FROM tbl_MiscellaneousClaim
                WHERE YEAR(RequestDate) = @Year AND MONTH(RequestDate) = @Month
                      AND (@Requester IS NULL OR Requester = @Requester)
                      AND (@Department IS NULL OR Department = @Department)
                ORDER BY RequestDate ASC";

                    break;

                default:
                    return; // Exit if filter type is invalid
            }

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Add requester and department parameters
                        cmd.Parameters.Add("@Requester", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(requester) ? (object)DBNull.Value : requester;
                        cmd.Parameters.Add("@Department", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(department) ? (object)DBNull.Value : department;

                        // Add date filter parameters
                        if (filterType == "Daily")
                        {
                            cmd.Parameters.AddWithValue("@Today", today);
                        }
                        else if (filterType == "Weekly")
                        {
                            DateTime weekStart = today.AddDays(-(int)today.DayOfWeek); // Start of the week (Sunday)
                            DateTime weekEnd = weekStart.AddDays(7); // End of the week
                            cmd.Parameters.AddWithValue("@WeekStart", weekStart);
                            cmd.Parameters.AddWithValue("@WeekEnd", weekEnd);
                        }
                        else if (filterType == "Monthly")
                        {
                            cmd.Parameters.AddWithValue("@Year", today.Year);
                            cmd.Parameters.AddWithValue("@Month", today.Month);
                        }

                        Debug.WriteLine($"Executing LoadData with Requester: {(string.IsNullOrEmpty(requester) ? "NULL" : requester)}, Department: {(string.IsNullOrEmpty(department) ? "NULL" : department)}, Filter: {filterType}");

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
                            Debug.WriteLine($"Row: SerialNo={row["SerialNo"]}, Requester={row["Requester"]}, Department={row["Department"]}");
                        }

                        // Bind to DataGridView
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
            Debug.WriteLine($"cmbRequester selected: {selectedUsername}");
            if (selectedUsername == "All Users" || string.IsNullOrEmpty(selectedUsername))
            {
                selectedUsername = null;
                Debug.WriteLine("Loading data with no Requester filter.");
            }
            if (selectedDepartment == "All Departments" || string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = null;
            }
            LoadData(selectedUsername, selectedDepartment);
        }

        private void cmbDepart_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedDepartment = cmbDepart.SelectedItem?.ToString();
            string selectedUsername = cmbRequester.SelectedItem?.ToString();
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
            LoadData(selectedUsername, selectedDepartment);
        }

        private void cmbPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPeriod.SelectedItem != null)
            {
                ApplyFilter(cmbPeriod.SelectedItem.ToString());
            }
        }

        private void UC_Miscellaneous_Load(object sender, EventArgs e)
        {
            cmbPeriod.SelectedItem = "Weekly"; // Set default filter to Weekly
            ApplyFilter("Weekly"); // Load data for Weekly filter
            cmbPeriod.SelectedIndexChanged += cmbPeriod_SelectedIndexChanged;
            CheckUserAccess(); // Check user access to set button visibility
        }

        private bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        private void ApplyFilter(string filterType)
        {
            if (string.IsNullOrEmpty(filterType)) return;

            string query = "";
            DateTime today = DateTime.Today;

            switch (filterType)
            {
                case "Daily":
                    query = @"
                SELECT SerialNo, Requester, Department, ExpensesType, RequestDate, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice, 
	                   HODApprovalStatus, ApprovedByHOD, HODApprovedDate, HRApprovalStatus, ApprovedByHR, HRApprovedDate, AccountApprovalStatus, 
	                   ApprovedByAccount, AccountApprovedDate 
                FROM tbl_MiscellaneousClaim
                WHERE CAST(RequestDate AS DATE) = @Today
                ORDER BY RequestDate ASC";

                    break;

                case "Weekly":
                    query = @"
                SELECT SerialNo, Requester, Department, ExpensesType, RequestDate, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice, 
	                   HODApprovalStatus, ApprovedByHOD, HODApprovedDate, HRApprovalStatus, ApprovedByHR, HRApprovedDate, AccountApprovalStatus, 
	                   ApprovedByAccount, AccountApprovedDate 
                FROM tbl_MiscellaneousClaim
                WHERE RequestDate >= @WeekStart AND RequestDate < @WeekEnd
                ORDER BY RequestDate ASC";

                    break;

                case "Monthly":
                    query = @"
                SELECT SerialNo, Requester, Department, ExpensesType, RequestDate, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice, 
	                   HODApprovalStatus, ApprovedByHOD, HODApprovedDate, HRApprovalStatus, ApprovedByHR, HRApprovedDate, AccountApprovalStatus, 
	                   ApprovedByAccount, AccountApprovedDate 
                FROM tbl_MiscellaneousClaim
                WHERE YEAR(RequestDate) = @Year AND MONTH(RequestDate) = @Month
                ORDER BY RequestDate ASC";

                    break;

                default:
                    return; // Exit if filter type is invalid
            }

            LoadFilteredData(query, today);
        }

        private void LoadFilteredData(string query, DateTime today)
        {
            if (dgvMS == null)
            {
                MessageBox.Show("DataGridView is not initialized!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!IsNetworkAvailable())
            {
                if (!isNetworkUnavailable) // Only show the message once
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
                        // Add parameters based on filter type
                        if (query.Contains("@Today"))
                        {
                            cmd.Parameters.AddWithValue("@Today", today);
                        }
                        else if (query.Contains("@WeekStart"))
                        {
                            DateTime weekStart = today.AddDays(-(int)today.DayOfWeek); // Start of the week (Sunday)
                            DateTime weekEnd = weekStart.AddDays(7); // End of the week
                            cmd.Parameters.AddWithValue("@WeekStart", weekStart);
                            cmd.Parameters.AddWithValue("@WeekEnd", weekEnd);
                        }
                        else if (query.Contains("@Year"))
                        {
                            cmd.Parameters.AddWithValue("@Year", today.Year);
                            cmd.Parameters.AddWithValue("@Month", today.Month);
                        }

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Update cache
                        cachedData = dt.Copy();

                        // Bind to DataGridView
                        BindDataGridView(dt);

                        // Reset error flags since we successfully connected
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
            dgvMS.AutoGenerateColumns = false;
            dgvMS.Columns.Clear();

            dgvMS.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Arial", 11, FontStyle.Bold),
            };

            int fixedColumnWidth = 150;

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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Vendor",
                HeaderText = "Vendor",
                DataPropertyName = "Vendor",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Item",
                HeaderText = "Item",
                DataPropertyName = "Item",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "InvoiceAmount",
                HeaderText = "Invoice Amount",
                DataPropertyName = "InvoiceAmount",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "InvoiceNo",
                HeaderText = "Invoice No",
                DataPropertyName = "InvoiceNo",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "Invoice",
                HeaderText = "Invoice",
                DataPropertyName = "Invoice",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "HODApprovalStatus",
                HeaderText = "HOD Approval Status",
                DataPropertyName = "HODApprovalStatus",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ApprovedByHOD",
                HeaderText = "Approved By HOD",
                DataPropertyName = "ApprovedByHOD",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "HODApprovedDate",
                HeaderText = "HOD Approved Date",
                DataPropertyName = "HODApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "HRApprovalStatus",
                HeaderText = "HR Approval Status",
                DataPropertyName = "HRApprovalStatus",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ApprovedByHR",
                HeaderText = "Approved By HR",
                DataPropertyName = "ApprovedByHR",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "HRApprovedDate",
                HeaderText = "HR Approved Date",
                DataPropertyName = "HRApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "AccountApprovalStatus",
                HeaderText = "Account Approval Status",
                DataPropertyName = "AccountApprovalStatus",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "ApprovedByAccount",
                HeaderText = "Approved By Account",
                DataPropertyName = "ApprovedByAccount",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "AccountApprovedDate",
                HeaderText = "Account Approved Date",
                DataPropertyName = "AccountApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11)
                },
            });

            dgvMS.DataSource = dt;
            dgvMS.CellBorderStyle = DataGridViewCellBorderStyle.None; // Add this line to remove cell borders
            Debug.WriteLine("DataGridView updated successfully.");
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
            string department = selectedRow.Cells["Department"].Value?.ToString();
            string hodApprovalStatus = selectedRow.Cells["HODApprovalStatus"].Value?.ToString();
            string hrApprovalStatus = selectedRow.Cells["HRApprovalStatus"].Value?.ToString();
            string accountApprovalStatus = selectedRow.Cells["AccountApprovalStatus"].Value?.ToString();

            // Validate the selection
            if (string.IsNullOrEmpty(serialNo) || string.IsNullOrEmpty(requester) || string.IsNullOrEmpty(department))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if HRApprovalStatus is Rejected
            if (hrApprovalStatus == "Rejected")
            {
                MessageBox.Show("This order cannot be approved because HR has rejected it.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if AccountApprovalStatus is Rejected
            if (accountApprovalStatus == "Rejected")
            {
                MessageBox.Show("This order cannot be approved because Account has rejected it.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Handle ACCOUNT department approval
            if (loggedInDepart == "ACCOUNT")
            {
                // Check if HODApprovalStatus is Rejected or Pending
                if (hodApprovalStatus == "Rejected" || hodApprovalStatus == "Pending")
                {
                    MessageBox.Show($"This order cannot be approved by Account because HOD approval is {hodApprovalStatus}.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if HRApprovalStatus is Pending
                if (hrApprovalStatus == "Pending")
                {
                    MessageBox.Show("This order cannot be approved by Account because HR approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the order is already approved by Account
                if (accountApprovalStatus == "Approved")
                {
                    MessageBox.Show("This order has already been approved by Account.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm Account approval with the user
                DialogResult result = MessageBox.Show($"Are you sure you want to approve Serial No: {serialNo} as Account?", "Confirm Account Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Update the database for Account approval
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                    {
                        con.Open();
                        string query = @"
                    UPDATE tbl_MiscellaneousClaim 
                    SET AccountApprovalStatus = @AccountApprovalStatus, 
                        ApprovedByAccount = @ApprovedByAccount, 
                        AccountApprovedDate = @AccountApprovedDate 
                    WHERE SerialNo = @SerialNo AND AccountApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@AccountApprovalStatus", "Approved");
                            cmd.Parameters.AddWithValue("@ApprovedByAccount", loggedInUser);
                            cmd.Parameters.AddWithValue("@AccountApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order approved successfully by Account.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                         cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString());
                            }
                            else
                            {
                                MessageBox.Show("Failed to approve the order. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error approving order: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error approving order: {ex.Message}");
                }
            }
            // Handle HR & ADMIN department approval
            else if (loggedInDepart == "HR & ADMIN")
            {
                // Check if HODApprovalStatus is Rejected or Pending
                if (hodApprovalStatus == "Rejected" || hodApprovalStatus == "Pending")
                {
                    MessageBox.Show($"This order cannot be approved by HR because HOD approval is {hodApprovalStatus}.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the order is already approved by HR
                if (hrApprovalStatus == "Approved")
                {
                    MessageBox.Show("This order has already been approved by HR.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm HR approval with the user
                DialogResult result = MessageBox.Show($"Are you sure you want to approve Serial No: {serialNo} as HR?", "Confirm HR Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                    UPDATE tbl_MiscellaneousClaim 
                    SET HRApprovalStatus = @HRApprovalStatus, 
                        ApprovedByHR = @ApprovedByHR, 
                        HRApprovedDate = @HRApprovedDate 
                    WHERE SerialNo = @SerialNo AND HRApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@HRApprovalStatus", "Approved");
                            cmd.Parameters.AddWithValue("@ApprovedByHR", loggedInUser);
                            cmd.Parameters.AddWithValue("@HRApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order approved successfully by HR.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                         cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString());
                            }
                            else
                            {
                                MessageBox.Show("Failed to approve the order. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error approving order: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error approving order: {ex.Message}");
                }
            }
            // Handle HOD approval for non-HR & ADMIN, non-ACCOUNT departments
            else
            {
                // Check if the order is already approved by HOD
                if (hodApprovalStatus == "Approved")
                {
                    MessageBox.Show("This order has already been approved by HOD.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the logged-in user's department matches the requester's department
                if (department != loggedInDepart)
                {
                    MessageBox.Show("You can only approve orders from your own department.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm HOD approval with the user
                DialogResult result = MessageBox.Show($"Are you sure you want to approve Serial No: {serialNo} as HOD?", "Confirm HOD Approval", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Update the database for HOD approval
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                    {
                        con.Open();
                        string query = @"
                    UPDATE tbl_MiscellaneousClaim 
                    SET HODApprovalStatus = @HODApprovalStatus, 
                        ApprovedByHOD = @ApprovedByHOD, 
                        HODApprovedDate = @HODApprovedDate 
                    WHERE SerialNo = @SerialNo";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@HODApprovalStatus", "Approved");
                            cmd.Parameters.AddWithValue("@ApprovedByHOD", loggedInUser);
                            cmd.Parameters.AddWithValue("@HODApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order approved successfully by HOD.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                         cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString());
                            }
                            else
                            {
                                MessageBox.Show("Failed to approve the order. It may not exist or has already been processed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error approving order: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error approving order: {ex.Message}");
                }
            }
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            // Check if a cell is selected in the DataGridView
            if (dgvMS.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to reject.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the selected row
            DataGridViewCell selectedCell = dgvMS.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string serialNo = selectedRow.Cells["SerialNo"].Value?.ToString();
            string requester = selectedRow.Cells["Requester"].Value?.ToString();
            string department = selectedRow.Cells["Department"].Value?.ToString();
            string hodApprovalStatus = selectedRow.Cells["HODApprovalStatus"].Value?.ToString();
            string hrApprovalStatus = selectedRow.Cells["HRApprovalStatus"].Value?.ToString();
            string accountApprovalStatus = selectedRow.Cells["AccountApprovalStatus"].Value?.ToString();

            // Validate the selection
            if (string.IsNullOrEmpty(serialNo) || string.IsNullOrEmpty(requester) || string.IsNullOrEmpty(department))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Handle rejection based on department
            if (loggedInDepart == "ACCOUNT")
            {
                // Check if HODApprovalStatus is Rejected or Pending
                if (hodApprovalStatus == "Rejected" || hodApprovalStatus == "Pending")
                {
                    MessageBox.Show($"This order cannot be rejected by Account because HOD approval is {hodApprovalStatus}.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if HRApprovalStatus is Rejected or Pending
                if (hrApprovalStatus == "Rejected" || hrApprovalStatus == "Pending")
                {
                    MessageBox.Show($"This order cannot be rejected by Account because HR approval is {hrApprovalStatus}.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the order is already approved by Account
                if (accountApprovalStatus == "Approved")
                {
                    MessageBox.Show("This order cannot be rejected by Account because it has already been approved.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the order is already rejected by Account
                if (accountApprovalStatus == "Rejected")
                {
                    MessageBox.Show("This order has already been rejected by Account.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm Account rejection with the user
                DialogResult result = MessageBox.Show($"Are you sure you want to reject Serial No: {serialNo} as Account?", "Confirm Account Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Update the database for Account rejection
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                    {
                        con.Open();
                        string query = @"
                            UPDATE tbl_MiscellaneousClaim 
                            SET AccountApprovalStatus = @AccountApprovalStatus, 
                                ApprovedByAccount = @ApprovedByAccount, 
                                AccountApprovedDate = @AccountApprovedDate 
                            WHERE SerialNo = @SerialNo AND AccountApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@AccountApprovalStatus", "Rejected");
                            cmd.Parameters.AddWithValue("@ApprovedByAccount", loggedInUser);
                            cmd.Parameters.AddWithValue("@AccountApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order rejected successfully by Account.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                         cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString());
                            }
                            else
                            {
                                MessageBox.Show("Failed to reject the order. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error rejecting order: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error rejecting order: {ex.Message}");
                }
            }
            else if (loggedInDepart == "HR & ADMIN")
            {
                // Check if HODApprovalStatus is Rejected or Pending
                if (hodApprovalStatus == "Rejected" || hodApprovalStatus == "Pending")
                {
                    MessageBox.Show($"This order cannot be rejected by HR because HOD approval is {hodApprovalStatus}.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the order is already approved by HR
                if (hrApprovalStatus == "Approved")
                {
                    MessageBox.Show("This order cannot be rejected by HR because it has already been approved.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the order is already rejected by HR
                if (hrApprovalStatus == "Rejected")
                {
                    MessageBox.Show("This order has already been rejected by HR.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm HR rejection with the user
                DialogResult result = MessageBox.Show($"Are you sure you want to reject Serial No: {serialNo} as HR?", "Confirm HR Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Update the database for HR rejection
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                    {
                        con.Open();
                        string query = @"
                            UPDATE tbl_MiscellaneousClaim 
                            SET HRApprovalStatus = @HRApprovalStatus, 
                                ApprovedByHR = @ApprovedByHR, 
                                HRApprovedDate = @HRApprovedDate 
                            WHERE SerialNo = @SerialNo AND HRApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@HRApprovalStatus", "Rejected");
                            cmd.Parameters.AddWithValue("@ApprovedByHR", loggedInUser);
                            cmd.Parameters.AddWithValue("@HRApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order rejected successfully by HR.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                         cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString());
                            }
                            else
                            {
                                MessageBox.Show("Failed to reject the order. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error rejecting order: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error rejecting order: {ex.Message}");
                }
            }
            else
            {
                // Check if the order is already rejected or approved by HOD
                if (hodApprovalStatus == "Rejected")
                {
                    MessageBox.Show("This order has already been rejected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (hodApprovalStatus == "Approved")
                {
                    MessageBox.Show("This order has already been approved and cannot be rejected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the logged-in user's department matches the requester's department
                if (department != loggedInDepart)
                {
                    MessageBox.Show("You can only reject orders from your own department.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Confirm rejection with the user
                DialogResult result = MessageBox.Show($"Are you sure you want to reject Serial No: {serialNo}?", "Confirm Rejection", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Update the database for HOD rejection
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                    {
                        con.Open();
                        string query = @"
                            UPDATE tbl_MiscellaneousClaim 
                            SET HODApprovalStatus = @HODApprovalStatus, 
                                ApprovedByHOD = @ApprovedByHOD, 
                                HODApprovedDate = @HODApprovedDate 
                            WHERE SerialNo = @SerialNo AND HODApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@HODApprovalStatus", "Rejected");
                            cmd.Parameters.AddWithValue("@ApprovedByHOD", loggedInUser);
                            cmd.Parameters.AddWithValue("@HODApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order rejected successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                         cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString());
                            }
                            else
                            {
                                MessageBox.Show("Failed to reject the order. It may not be pending or does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error rejecting order: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error rejecting order: {ex.Message}");
                }
            }
        }

        private void btnWithdraw_Click(object sender, EventArgs e)
        {
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

            // Check if the order has been approved by any department
            if (hodApprovalStatus == "Approved" || hrApprovalStatus == "Approved" || accountApprovalStatus == "Approved")
            {
                MessageBox.Show("This order cannot be withdrawn because it has been approved.", "Withdrawal Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if the user has HR & Admin access (AA = 1) to delete any order
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

            // Restrict non-HR & Admin users to only deleting their own orders
            if (!isHRAdmin && requester != loggedInUser)
            {
                MessageBox.Show("You can only withdraw your own orders.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    string query = $"DELETE FROM tbl_MiscellaneousClaim WHERE SerialNo = @SerialNo";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SerialNo", serialNo);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Successfully withdrawn.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Refresh the DataGridView
                            LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                     cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString());
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
    }
}