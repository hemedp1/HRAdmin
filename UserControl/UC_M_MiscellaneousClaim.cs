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
using HRAdmin.Components;

namespace HRAdmin.UserControl
{
    public partial class UC_M_MiscellaneousClaim : System.Windows.Forms.UserControl
    {
        private string LoggedInUser;
        private string loggedInDepart;
        private string loggedInIndex;
        private string LoggedInBank;
        private string logginInUserAccessLevel;
        private string LoggedInAccNo;
        private DataTable cachedData; // Declare cachedData
        private bool isNetworkErrorShown; // Declare isNetworkErrorShown
        private bool isNetworkUnavailable; // Declare isNetworkUnavailable
        private byte[] pdfBytes;

        public UC_M_MiscellaneousClaim(string username, string department, string emp,string bank, string accountNo, string UL)
        {
            InitializeComponent();
            //string gg = UserSession.LoggedInBank;
            LoggedInUser = username;
            loggedInDepart = department;
            loggedInIndex = emp;
            LoggedInBank = bank;
            LoggedInAccNo = accountNo;
            logginInUserAccessLevel = UL;


            dtRequest.Text = DateTime.Now.ToString("dd.MM.yyyy");
            LoadUsernames();
            LoadDepartments();
            LoadData(); // Initial load with default weekly filter
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
            Form_Home.sharedbtnApproval.Visible = false;

            UC_Acc_Account ug = new UC_Acc_Account(LoggedInUser, loggedInDepart, loggedInIndex, LoggedInBank, LoggedInAccNo, logginInUserAccessLevel);
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

                UC_M_Work ug = new UC_M_Work(LoggedInUser, loggedInDepart, selectedType, loggedInIndex, LoggedInBank, LoggedInAccNo, logginInUserAccessLevel);
                addControls(ug);
            }
            else if (selectedType == "Benefit")
            {
                Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Benefit";
                Form_Home.sharedbtnMCReport.Visible = false;
                Form_Home.sharedbtnApproval.Visible = false;

                UC_M_Work ug = new UC_M_Work(LoggedInUser, loggedInDepart, selectedType, loggedInIndex, LoggedInBank, LoggedInAccNo, logginInUserAccessLevel);
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
                        cmd.Parameters.AddWithValue("@Username", LoggedInUser);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string AA = reader["AA"].ToString();
                                string MA = reader["MA"].ToString();

                                // Set check, approve button, and labels visibility: hidden if AA = 1, visible if MA = 2
                                if (AA == "1")
                                {
                                    //P_Authorization.Visible = false;
                                }
                                else if (MA == "2")
                                {
                                    //P_Authorization.Visible = true; 
                                }
                                else
                                {
                                    //P_Authorization.Visible = false;
                                }
                            }
                            else
                            {
                                //P_Authorization.Visible = false;
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
                    string query = "SELECT Name1 FROM tbl_Users";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            cmbRequester.Items.Clear();
                            cmbRequester.Items.Add("All Users");
                            while (reader.Read())
                            {
                                string Name1 = reader["Name1"].ToString();
                                cmbRequester.Items.Add(Name1);
                                Debug.WriteLine($"Loaded username: {Name1}");
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
                    // Modified query to select distinct departments from tbl_MasterClaimForm
                    string query = "SELECT DISTINCT Department FROM tbl_MasterClaimForm WHERE Department IS NOT NULL";
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
                                Debug.WriteLine($"Loaded department from tbl_MasterClaimForm: {department}");
                            }
                        }
                    }
                    cmbDepart.SelectedIndex = 0;
                    Debug.WriteLine("Departments loaded successfully from tbl_MasterClaimForm.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading departments: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error loading departments from tbl_MasterClaimForm: {ex.Message}");
                }
            }
        }

        private void LoadData(string requester = null, string department = null, DateTime? startDate = null, DateTime? endDate = null, string expensesType = null)
        {
            if (dgvMS == null)
            {
                MessageBox.Show("DataGridView is not initialized!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query = @"
        SELECT SerialNo, Requester, Department, ExpensesType, RequestDate, HODApprovalStatus, ApprovedByHOD, HODApprovedDate, 
               HRApprovalStatus, ApprovedByHR, HRApprovedDate, AccountApprovalStatus, ApprovedByAccount, AccountApprovedDate,
               Account2ApprovalStatus, ApprovedByAccount2, Account2ApprovedDate, Account3ApprovalStatus, ApprovedByAccount3, Account3ApprovedDate
        FROM tbl_MasterClaimForm
        WHERE (@StartDate IS NULL OR CAST(RequestDate AS DATE) >= @StartDate)
              AND (@EndDate IS NULL OR CAST(RequestDate AS DATE) <= @EndDate)
              AND (@Requester IS NULL OR Requester = @Requester)
              AND (@Department IS NULL OR Department = @Department)
              AND (@ExpensesType IS NULL OR ExpensesType = @ExpensesType)
        ORDER BY RequestDate ASC";

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Add parameters
                        cmd.Parameters.Add("@Requester", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(requester) ? (object)DBNull.Value : requester;
                        cmd.Parameters.Add("@Department", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(department) ? (object)DBNull.Value : department;
                        cmd.Parameters.Add("@ExpensesType", SqlDbType.NVarChar).Value = string.IsNullOrEmpty(expensesType) ? (object)DBNull.Value : expensesType;

                        // Add date filter parameters
                        if (startDate.HasValue && endDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
                            cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date);
                        }
                        else
                        {
                            // Default to weekly filter if no dates are provided
                            DateTime today = DateTime.Today;
                            DateTime weekStart = today.AddDays(-(int)today.DayOfWeek); // Start of the week (Sunday)
                            DateTime weekEnd = weekStart.AddDays(7).AddTicks(-1); // End of the week
                            cmd.Parameters.AddWithValue("@StartDate", weekStart);
                            cmd.Parameters.AddWithValue("@EndDate", weekEnd);
                        }

                        Debug.WriteLine($"Executing LoadData with Requester: {(string.IsNullOrEmpty(requester) ? "NULL" : requester)}, Department: {(string.IsNullOrEmpty(department) ? "NULL" : department)}, StartDate: {(startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "NULL")}, EndDate: {(endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "NULL")}, ExpensesType: {(string.IsNullOrEmpty(expensesType) ? "NULL" : expensesType)}");

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
                            Debug.WriteLine($"Row: SerialNo={row["SerialNo"]}, Requester={row["Requester"]}, Department={row["Department"]}, ExpensesType={row["ExpensesType"]}");
                        }

                        // Bind to DataGridView
                        BindDataGridView(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!isNetworkErrorShown)
                {
                    isNetworkErrorShown = true;
                    MessageBox.Show("Error loading data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error loading data: {ex.Message}");
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
            }
        }

        private void cmbECtype_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedExpensesType = cmbECtype.SelectedItem?.ToString();
            if (selectedExpensesType == "All") // Optional: Add an "All" option to show both Work and Benefit
            {
                selectedExpensesType = null;
            }
            string selectedUsername = cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString();
            LoadData(selectedUsername, selectedDepartment, dtpStart.Value, dtpEnd.Value, selectedExpensesType);
        }

        private void cmbRequester_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedUsername = cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString();
            LoadData(selectedUsername, selectedDepartment, dtpStart.Value, dtpEnd.Value);
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
            LoadData(selectedUsername, selectedDepartment, dtpStart.Value, dtpEnd.Value);
        }

        private void UC_Miscellaneous_Load(object sender, EventArgs e)
        {
            LoadData(); // Load data with default weekly filter
            CheckUserAccess(); // Check user access to set button visibility
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

            dgvMS.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new System.Drawing.Font("Arial", 11, FontStyle.Bold),
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
                HeaderText = "Account1 Status Check",
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
                HeaderText = "Account3 Status Check",
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

            Debug.WriteLine("DataGridView updated successfully.");
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

            // Restrict withdrawal to only the user's own orders
            if (requester != LoggedInUser)
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
                    string query = "BEGIN TRANSACTION;\r\nDELETE FROM tbl_DetailClaimForm WHERE SerialNo = @SerialNo;\r\nDELETE FROM tbl_MasterClaimForm WHERE SerialNo = @SerialNo;\r\nCOMMIT;";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SerialNo", serialNo);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Successfully withdrawn.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Refresh the DataGridView
                            LoadData(cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString(),
                                     cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString(),
                                     dtpStart.Value, dtpEnd.Value);
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

        private void dtpStart_ValueChanged(object sender, EventArgs e)
        {
            string selectedUsername = cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString();
            LoadData(selectedUsername, selectedDepartment, dtpStart.Value, dtpEnd.Value);
        }

        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            string selectedUsername = cmbRequester.SelectedItem?.ToString() == "All Users" ? null : cmbRequester.SelectedItem?.ToString();
            string selectedDepartment = cmbDepart.SelectedItem?.ToString() == "All Departments" ? null : cmbDepart.SelectedItem?.ToString();
            LoadData(selectedUsername, selectedDepartment, dtpStart.Value, dtpEnd.Value);
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
            string accountApprovalStatus = selectedRow.Cells["AccountApprovalStatus"].Value?.ToString();
            string expensesType = selectedRow.Cells["ExpensesType"].Value?.ToString();

            // Validate the selection
            if (string.IsNullOrEmpty(serialNo) || string.IsNullOrEmpty(requester))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check if any department has rejected the order
            if (hodApprovalStatus == "Rejected" || hrApprovalStatus == "Rejected" || accountApprovalStatus == "Rejected")
            {
                MessageBox.Show("This order cannot be approved because it has been rejected by one or more departments.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Handle ACCOUNT department approval
            if (loggedInDepart == "ACCOUNT")
            {
                // Check if HODApprovalStatus is Pending
                if (hodApprovalStatus == "Pending")
                {
                    MessageBox.Show("This order cannot be approved by Account because HOD approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if HRApprovalStatus is Pending and ExpensesType is not Work
                if (hrApprovalStatus == "Pending" && expensesType != "Work")
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
            UPDATE tbl_MasterClaimForm 
            SET AccountApprovalStatus = @AccountApprovalStatus, 
                ApprovedByAccount = @ApprovedByAccount, 
                AccountApprovedDate = @AccountApprovedDate 
            WHERE SerialNo = @SerialNo AND AccountApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@AccountApprovalStatus", "Approved");
                            cmd.Parameters.AddWithValue("@ApprovedByAccount", LoggedInUser);
                            cmd.Parameters.AddWithValue("@AccountApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order approved successfully by Account.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData();
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
                // Check if the ExpensesType is Work
                if (expensesType == "Work")
                {
                    MessageBox.Show("HR & ADMIN cannot approve Work-related expenses.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if HODApprovalStatus is Pending
                if (hodApprovalStatus == "Pending")
                {
                    MessageBox.Show("This order cannot be approved by HR because HOD approval is Pending.", "Approval Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            UPDATE tbl_MasterClaimForm 
            SET HRApprovalStatus = @HRApprovalStatus, 
                ApprovedByHR = @ApprovedByHR, 
                HRApprovedDate = @HRApprovedDate 
            WHERE SerialNo = @SerialNo AND HRApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@HRApprovalStatus", "Approved");
                            cmd.Parameters.AddWithValue("@ApprovedByHR", LoggedInUser);
                            cmd.Parameters.AddWithValue("@HRApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order approved successfully by HR.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData();
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
            else
            {
                // Check if the user is trying to approve their own claim
                if (requester == LoggedInUser)
                {
                    MessageBox.Show("You cannot approve your own claim.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Extract requester's department from SerialNo (e.g., "HR & ADMIN" from "HR & ADMIN_02072025_3")
                string requesterDepartment = serialNo.Split('_')[0].Trim();
                if (loggedInDepart != requesterDepartment)
                {
                    MessageBox.Show($"You are not authorized to approve this order. Only HOD from {requesterDepartment} department can approve.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if the order is already approved by HOD
                if (hodApprovalStatus == "Approved")
                {
                    MessageBox.Show("This order has already been approved by HOD.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            UPDATE tbl_MasterClaimForm 
            SET HODApprovalStatus = @HODApprovalStatus, 
                ApprovedByHOD = @ApprovedByHOD, 
                HODApprovedDate = @HODApprovedDate 
            WHERE SerialNo = @SerialNo AND HODApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@HODApprovalStatus", "Approved");
                            cmd.Parameters.AddWithValue("@ApprovedByHOD", LoggedInUser);
                            cmd.Parameters.AddWithValue("@HODApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order approved successfully by HOD.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData();
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
            string hodApprovalStatus = selectedRow.Cells["HODApprovalStatus"].Value?.ToString();
            string hrApprovalStatus = selectedRow.Cells["HRApprovalStatus"].Value?.ToString();
            string accountApprovalStatus = selectedRow.Cells["AccountApprovalStatus"].Value?.ToString();
            string expensesType = selectedRow.Cells["ExpensesType"].Value?.ToString();

            // Validate the selection
            if (string.IsNullOrEmpty(serialNo) || string.IsNullOrEmpty(requester))
            {
                MessageBox.Show("Invalid order selection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Handle ACCOUNT department rejection
            if (loggedInDepart == "ACCOUNT")
            {
                // Check if HODApprovalStatus is Rejected or Pending
                if (hodApprovalStatus == "Rejected" || hodApprovalStatus == "Pending")
                {
                    MessageBox.Show($"This order cannot be rejected by Account because HOD approval is {hodApprovalStatus}.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if HRApprovalStatus is Rejected or Pending and ExpensesType is not Work
                if ((hrApprovalStatus == "Rejected" || hrApprovalStatus == "Pending") && expensesType != "Work")
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
                    UPDATE tbl_MasterClaimForm 
                    SET AccountApprovalStatus = @AccountApprovalStatus, 
                        ApprovedByAccount = @ApprovedByAccount, 
                        AccountApprovedDate = @AccountApprovedDate 
                    WHERE SerialNo = @SerialNo AND AccountApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@AccountApprovalStatus", "Rejected");
                            cmd.Parameters.AddWithValue("@ApprovedByAccount", LoggedInUser);
                            cmd.Parameters.AddWithValue("@AccountApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order rejected successfully by Account.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData();
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
                // Check if the ExpensesType is Work
                if (expensesType == "Work")
                {
                    MessageBox.Show("HR & ADMIN cannot reject Work-related expenses.", "Rejection Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

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
                    UPDATE tbl_MasterClaimForm 
                    SET HRApprovalStatus = @HRApprovalStatus, 
                        ApprovedByHR = @ApprovedByHR, 
                        HRApprovedDate = @HRApprovedDate 
                    WHERE SerialNo = @SerialNo AND HRApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@HRApprovalStatus", "Rejected");
                            cmd.Parameters.AddWithValue("@ApprovedByHR", LoggedInUser);
                            cmd.Parameters.AddWithValue("@HRApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order rejected successfully by HR.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData();
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
                // Check if the user is trying to reject their own claim
                if (requester == LoggedInUser)
                {
                    MessageBox.Show("You cannot reject your own claim.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Extract requester's department from SerialNo (e.g., "HR & ADMIN" from "HR & ADMIN_02072025_3")
                string requesterDepartment = serialNo.Split('_')[0].Trim();
                if (loggedInDepart != requesterDepartment)
                {
                    MessageBox.Show($"You are not authorized to reject this order. Only HOD from {requesterDepartment} department can reject.", "Unauthorized", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

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
                    UPDATE tbl_MasterClaimForm 
                    SET HODApprovalStatus = @HODApprovalStatus, 
                        ApprovedByHOD = @ApprovedByHOD, 
                        HODApprovedDate = @HODApprovedDate 
                    WHERE SerialNo = @SerialNo AND HODApprovalStatus = 'Pending'";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@HODApprovalStatus", "Rejected");
                            cmd.Parameters.AddWithValue("@ApprovedByHOD", LoggedInUser);
                            cmd.Parameters.AddWithValue("@HODApprovedDate", DateTime.Now);
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Order rejected successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                // Refresh the DataGridView
                                LoadData();
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

            // Validate the selection
            if (string.IsNullOrEmpty(serialNo) || string.IsNullOrEmpty(requester))
            {
                MessageBox.Show("Invalid order selection: SerialNo or Requester is missing.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Proceed with generating and viewing the PDF
            Debug.WriteLine($"Selected SerialNo: {serialNo}");
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
                           m.AccountApprovalStatus, m.ApprovedByAccount, m.AccountApprovedDate
                    FROM tbl_MasterClaimForm m
                    JOIN tbl_Users u ON m.EmpNo = u.IndexNo
                    JOIN tbl_UserDetail ud ON u.IndexNo = ud.IndexNo
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