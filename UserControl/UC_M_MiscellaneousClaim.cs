﻿using System;
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
        private string loggedInIndex;
        private DataTable cachedData; // Declare cachedData
        private bool isNetworkErrorShown; // Declare isNetworkErrorShown
        private bool isNetworkUnavailable; // Declare isNetworkUnavailable

        public UC_M_MiscellaneousClaim(string username, string department, string emp)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            loggedInIndex = emp;
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

            UC_Acc_Account ug = new UC_Acc_Account(loggedInUser, loggedInDepart, loggedInIndex);
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

                UC_M_Work ug = new UC_M_Work(loggedInUser, loggedInDepart, selectedType, loggedInIndex);
                addControls(ug);
            }
            else if (selectedType == "Benefit")
            {
                Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim > Benefit";
                Form_Home.sharedbtnMCReport.Visible = false;
                Form_Home.sharedbtnApproval.Visible = false;

                UC_M_Work ug = new UC_M_Work(loggedInUser, loggedInDepart, selectedType, loggedInIndex);
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

                                }
                                else if (MA == "2")
                                {
                                    //btnReject.Visible = true;
                                    //btnApprove.Visible = false;
                                    // Labels remain visible as default
                                }
                                else
                                {
                                    //btnReject.Visible = false; // Default to hidden if neither condition is met
                                    //btnApprove.Visible = false; // Default to hidden if neither condition is met
                                    //label14.Visible = false; // Hide "Verification" label
                                    //label15.Visible = false; // Hide ":" symbol
                                }
                            }
                            else
                            {
                                //btnReject.Visible = false; // Hide if user not found
                                //btnApprove.Visible = false; // Hide if user not found
                                //label14.Visible = false; // Hide if user not found
                                //label15.Visible = false; // Hide if user not found
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking user access: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //btnReject.Visible = false; // Hide on error to be safe
                //btnApprove.Visible = false; // Hide on error to be safe
                //label14.Visible = false; // Hide on error to be safe
                //label15.Visible = false; // Hide on error to be safe
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

        private void LoadData(string requester = null, string department = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (dgvMS == null)
            {
                MessageBox.Show("DataGridView is not initialized!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string query = @"
                SELECT SerialNo, Requester, Department, ExpensesType, RequestDate, HODApprovalStatus, ApprovedByHOD, HODApprovedDate, 
                       HRApprovalStatus, ApprovedByHR, HRApprovedDate, AccountApprovalStatus, ApprovedByAccount, AccountApprovedDate 
                FROM tbl_MasterClaimForm
                WHERE (@StartDate IS NULL OR CAST(RequestDate AS DATE) >= @StartDate)
                      AND (@EndDate IS NULL OR CAST(RequestDate AS DATE) <= @EndDate)
                      AND (@Requester IS NULL OR Requester = @Requester)
                      AND (@Department IS NULL OR Department = @Department)
                ORDER BY RequestDate ASC";

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
                        if (startDate.HasValue && endDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@StartDate", startDate.Value.Date);
                            cmd.Parameters.AddWithValue("@EndDate", endDate.Value.Date); // Use the end date as is, no extension
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

                        Debug.WriteLine($"Executing LoadData with Requester: {(string.IsNullOrEmpty(requester) ? "NULL" : requester)}, Department: {(string.IsNullOrEmpty(department) ? "NULL" : department)}, StartDate: {(startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : "NULL")}, EndDate: {(endDate.HasValue ? endDate.Value.ToString("yyyy-MM-dd") : "NULL")}");

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
                    Font = new Font("Arial", 11),
                    Format = "dd.MM.yyyy"
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
                    Font = new Font("Arial", 11),
                    Format = "dd.MM.yyyy"
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
                    Font = new Font("Arial", 11),
                    Format = "dd.MM.yyyy"
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
                    Font = new Font("Arial", 11),
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
            if (requester != loggedInUser)
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
                    string query = "DELETE FROM tbl_MasterClaimForm WHERE SerialNo = @SerialNo";
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
    }
}