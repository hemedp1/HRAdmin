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
    public partial class UC_M_Approval : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private string loggedInIndex;
        private DataTable cachedData; // For caching data
        private bool isNetworkErrorShown;
        private bool isNetworkUnavailable;

        public UC_M_Approval(string username, string department, string emp)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            loggedInIndex = emp;
            cachedData = new DataTable(); // Initialize (replace with actual cache loading logic)
            isNetworkErrorShown = false;
            isNetworkUnavailable = false;
            this.Load += UC_Approval_Load;
            // Add event handlers for DateTimePickers and ComboBoxes
            dtpStart.ValueChanged += dtpStart_ValueChanged;
            dtpEnd.ValueChanged += dtpEnd_ValueChanged;
            cmbDepartment.SelectedIndexChanged += cmbDepartment_SelectedIndexChanged;
            cmbRequester.SelectedIndexChanged += cmbRequester_SelectedIndexChanged;
            LoadDepartments(); // Populate departments
            LoadUsernames(); // Populate requesters
            LoadData(); // Load data on initialization
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

        private void UC_Approval_Load(object sender, EventArgs e)
        {
            LoadData(); // Load data on form load
        }

        private void dtpStart_ValueChanged(object sender, EventArgs e)
        {
            if (dtpEnd.Value < dtpStart.Value)
            {
                MessageBox.Show("End date cannot be earlier than start date.", "Invalid Date Range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpStart.Value = dtpEnd.Value; // Reset start date to match end date
                return;
            }
            LoadData(); // Reload data when start date changes
        }

        private void dtpEnd_ValueChanged(object sender, EventArgs e)
        {
            if (dtpEnd.Value < dtpStart.Value)
            {
                MessageBox.Show("End date cannot be earlier than start date.", "Invalid Date Range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtpEnd.Value = dtpStart.Value; // Reset end date to match start date
                return;
            }
            LoadData(); // Reload data when end date changes
        }

        private void cmbDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDepartment.SelectedIndex != -1)
            {
                string selectedDepartment = cmbDepartment.SelectedItem.ToString();
                LoadUsernamesByDepartment(selectedDepartment); // Load usernames for selected department
            }
            else
            {
                LoadUsernames(); // Load all usernames if no department selected
            }
            LoadData(); // Reload data with current filters
        }

        private void cmbRequester_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData(); // Reload data when requester changes
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
                            cmbDepartment.Items.Clear();
                            while (reader.Read())
                            {
                                string department = reader["Department"].ToString();
                                cmbDepartment.Items.Add(department);
                                Debug.WriteLine($"Loaded department: {department}");
                            }
                        }
                    }
                    cmbDepartment.SelectedIndex = -1; // No department selected initially
                    if (cmbDepartment.Items.Count > 0)
                    {
                        Debug.WriteLine("Departments loaded successfully.");
                    }
                    else
                    {
                        Debug.WriteLine("No departments found.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading departments: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error loading departments: {ex.Message}");
                }
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
                            while (reader.Read())
                            {
                                string username = reader["Username"].ToString();
                                cmbRequester.Items.Add(username);
                                Debug.WriteLine($"Loaded username: {username}");
                            }
                        }
                    }
                    cmbRequester.SelectedIndex = -1; // No requester selected initially
                    if (cmbRequester.Items.Count > 0)
                    {
                        Debug.WriteLine("Usernames loaded successfully.");
                    }
                    else
                    {
                        Debug.WriteLine("No usernames found.");
                    }
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
                    cmbRequester.SelectedIndex = -1; // No requester selected initially
                    if (cmbRequester.Items.Count > 0)
                    {
                        Debug.WriteLine($"Usernames loaded successfully for department: {department}");
                    }
                    else
                    {
                        Debug.WriteLine($"No usernames found for department: {department}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading usernames: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Debug.WriteLine($"Error loading usernames for department {department}: {ex.Message}");
                }
            }
        }

        private bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        private void LoadData()
        {
            if (dgvA == null)
            {
                MessageBox.Show("DataGridView is not initialized!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Require a valid date range to display any data
            bool applyDateFilter = dtpStart.Value != null && dtpEnd.Value != null && dtpEnd.Value >= dtpStart.Value;
            if (!applyDateFilter)
            {
                dgvA.DataSource = null;
                Debug.WriteLine("No data loaded due to invalid or missing date range.");
                return;
            }

            string query = @"
        SELECT 
            d.SerialNo, 
            d.ExpensesType, 
            d.Vendor, 
            d.Item, 
            d.InvoiceAmount, 
            d.InvoiceNo, 
            d.Invoice, 
            m.RequestDate,
            m.HODApprovalStatus, 
            m.ApprovedByHOD, 
            m.HODApprovedDate, 
            m.HRApprovalStatus, 
            m.ApprovedByHR, 
            m.HRApprovedDate, 
            m.AccountApprovalStatus, 
            m.ApprovedByAccount, 
            m.AccountApprovedDate,
            m.Requester
        FROM tbl_DetailClaimForm d
        INNER JOIN tbl_MasterClaimForm m
        ON d.SerialNo = m.SerialNo";

            // Build WHERE clause for filters
            bool applyDepartmentFilter = cmbDepartment.SelectedIndex != -1;
            bool applyRequesterFilter = cmbRequester.SelectedIndex != -1;
            List<string> conditions = new List<string>();
            List<SqlParameter> parameters = new List<SqlParameter>();

            // Apply date filter with explicit end time
            conditions.Add("m.RequestDate >= @StartDate AND m.RequestDate < @EndDate");
            parameters.Add(new SqlParameter("@StartDate", dtpStart.Value.Date));
            parameters.Add(new SqlParameter("@EndDate", dtpEnd.Value.Date.AddDays(1))); // Exclusive end date

            if (applyDepartmentFilter)
            {
                conditions.Add("d.SerialNo LIKE @Department + '_%'");
                parameters.Add(new SqlParameter("@Department", cmbDepartment.SelectedItem.ToString()));
            }

            if (applyRequesterFilter)
            {
                conditions.Add("m.Requester = @Requester");
                parameters.Add(new SqlParameter("@Requester", cmbRequester.SelectedItem.ToString()));
            }

            if (conditions.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", conditions);
            }

            query += " ORDER BY d.SerialNo ASC";

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                        Debug.WriteLine($"Executing LoadData with query: {query}");
                        Debug.WriteLine($"Parameters: StartDate={dtpStart.Value}, EndDate={dtpEnd.Value.AddDays(1)}");

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
                            Debug.WriteLine($"Row: SerialNo={row["SerialNo"]}, RequestDate={row["RequestDate"]}");
                        }

                        // Bind to DataGridView
                        BindDataGridView(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!IsNetworkAvailable() && cachedData != null && cachedData.Rows.Count > 0)
                {
                    DataTable filteredData = cachedData.Copy();
                    List<string> filterConditions = new List<string>();

                    filterConditions.Add($"RequestDate >= #{dtpStart.Value.Date:MM/dd/yyyy}# AND RequestDate < #{dtpEnd.Value.Date.AddDays(1):MM/dd/yyyy}#");

                    if (applyDepartmentFilter)
                    {
                        filterConditions.Add($"SerialNo LIKE '{SqlServerEscape(cmbDepartment.SelectedItem.ToString())}%'");
                    }

                    if (applyRequesterFilter)
                    {
                        filterConditions.Add($"Requester = '{SqlServerEscape(cmbRequester.SelectedItem.ToString())}'");
                    }

                    filteredData.DefaultView.RowFilter = string.Join(" AND ", filterConditions);
                    filteredData = filteredData.DefaultView.ToTable();

                    BindDataGridView(filteredData);
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
                        MessageBox.Show("Error loading data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    Debug.WriteLine($"Error loading data: {ex.Message}");
                }
            }
        }

        private string SqlServerEscape(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input.Replace("'", "''");
        }

        private void BindDataGridView(DataTable dt)
        {
            dgvA.AutoGenerateColumns = false;
            dgvA.Columns.Clear();

            dgvA.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Arial", 11, FontStyle.Bold),
            };

            int fixedColumnWidth = 150;

            // Add columns
            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "RequestDate",
                HeaderText = "Request date",
                DataPropertyName = "RequestDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11),
                    Format = "dd.MM.yyyy"
                },
            });

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "HODApprovedDate",
                HeaderText = "HOD Approved Date",
                DataPropertyName = "HODApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11),
                    Format = "dd.MM.yyyy   HH:mm"
                },
            });

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "HRApprovedDate",
                HeaderText = "HR Approved Date",
                DataPropertyName = "HRApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11),
                    Format = "dd.MM.yyyy   HH:mm"
                },
            });

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
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

            dgvA.Columns.Add(new DataGridViewTextBoxColumn()
            {
                Name = "AccountApprovedDate",
                HeaderText = "Account Approved Date",
                DataPropertyName = "AccountApprovedDate",
                Width = fixedColumnWidth,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.MidnightBlue,
                    Font = new Font("Arial", 11),
                    Format = "dd.MM.yyyy   HH:mm"
                },
            });

            dgvA.DataSource = dt;
            dgvA.CellBorderStyle = DataGridViewCellBorderStyle.None;
            Debug.WriteLine("DataGridView updated successfully.");
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            // Check if a cell is selected in the DataGridView
            if (dgvA.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to approve.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the selected row
            DataGridViewCell selectedCell = dgvA.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string serialNo = selectedRow.Cells["SerialNo"].Value?.ToString();
            string hodApprovalStatus = selectedRow.Cells["HODApprovalStatus"].Value?.ToString();
            string hrApprovalStatus = selectedRow.Cells["HRApprovalStatus"].Value?.ToString();
            string accountApprovalStatus = selectedRow.Cells["AccountApprovalStatus"].Value?.ToString();

            // Validate the selection
            if (string.IsNullOrEmpty(serialNo))
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
            UPDATE tbl_MasterClaimForm 
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
                            cmd.Parameters.AddWithValue("@ApprovedByHR", loggedInUser);
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
                            cmd.Parameters.AddWithValue("@ApprovedByHOD", loggedInUser);
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
            if (dgvA.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell in the order row to reject.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the selected row
            DataGridViewCell selectedCell = dgvA.SelectedCells[0];
            DataGridViewRow selectedRow = selectedCell.OwningRow;
            string serialNo = selectedRow.Cells["SerialNo"].Value?.ToString();
            string hodApprovalStatus = selectedRow.Cells["HODApprovalStatus"].Value?.ToString();
            string hrApprovalStatus = selectedRow.Cells["HRApprovalStatus"].Value?.ToString();
            string accountApprovalStatus = selectedRow.Cells["AccountApprovalStatus"].Value?.ToString();

            // Validate the selection
            if (string.IsNullOrEmpty(serialNo))
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
                    UPDATE tbl_MasterClaimForm 
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
                            cmd.Parameters.AddWithValue("@ApprovedByHR", loggedInUser);
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
                            cmd.Parameters.AddWithValue("@ApprovedByHOD", loggedInUser);
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
    }
}