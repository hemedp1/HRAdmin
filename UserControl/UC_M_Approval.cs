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
        private DataTable serialNoData; // To store all SerialNos for ComboBox
        private bool isNetworkErrorShown;
        private bool isNetworkUnavailable;

        public UC_M_Approval(string username, string department, string emp)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            loggedInIndex = emp;
            cachedData = new DataTable(); // Initialize (replace with actual cache loading logic)
            serialNoData = new DataTable(); // Initialize SerialNo data
            serialNoData.Columns.Add("SerialNo", typeof(string));
            isNetworkErrorShown = false;
            isNetworkUnavailable = false;
            this.Load += UC_Approval_Load;
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

        private bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        private void LoadData()
        {
            LoadData(null); // Call the overload with no filter
        }

        private void LoadData(string serialNo)
        {
            if (dgvA == null)
            {
                MessageBox.Show("DataGridView is not initialized!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            m.HODApprovalStatus, 
            m.ApprovedByHOD, 
            m.HODApprovedDate, 
            m.HRApprovalStatus, 
            m.ApprovedByHR, 
            m.HRApprovedDate, 
            m.AccountApprovalStatus, 
            m.ApprovedByAccount, 
            m.AccountApprovedDate
        FROM tbl_DetailClaimForm d
        INNER JOIN tbl_MasterClaimForm m
        ON d.SerialNo = m.SerialNo";

            if (!string.IsNullOrEmpty(serialNo))
            {
                query += " WHERE d.SerialNo = @SerialNo";
            }

            query += " ORDER BY d.SerialNo ASC";

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        if (!string.IsNullOrEmpty(serialNo))
                        {
                            cmd.Parameters.AddWithValue("@SerialNo", serialNo);
                        }

                        Debug.WriteLine("Executing LoadData with joined tables.");

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        // Update cache only if no filter (initial load or full refresh)
                        if (string.IsNullOrEmpty(serialNo))
                        {
                            cachedData = dt.Copy();
                            // Update serialNoData for ComboBox
                            UpdateSerialNoData(dt);
                        }

                        Debug.WriteLine($"Rows retrieved: {dt.Rows.Count}");
                        foreach (DataRow row in dt.Rows)
                        {
                            Debug.WriteLine($"Row: SerialNo={row["SerialNo"]}, Vendor={row["Vendor"]}, ExpensesType={row["ExpensesType"]}, HODApprovalStatus={row["HODApprovalStatus"]}");
                        }

                        // Populate ComboBox with all SerialNos
                        PopulateComboBox();

                        // Bind to DataGridView
                        BindDataGridView(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!IsNetworkAvailable() && cachedData != null && cachedData.Rows.Count > 0)
                {
                    // Populate ComboBox with cached SerialNos
                    PopulateComboBox();
                    DataTable filteredData = cachedData.Copy();
                    if (!string.IsNullOrEmpty(serialNo))
                    {
                        filteredData.DefaultView.RowFilter = $"SerialNo = '{serialNo}'";
                        filteredData = filteredData.DefaultView.ToTable();
                    }
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

        private void UpdateSerialNoData(DataTable dt)
        {
            // Clear existing SerialNos
            serialNoData.Clear();
            // Add unique SerialNos from the DataTable
            foreach (DataRow row in dt.Rows)
            {
                string serial = row["SerialNo"].ToString();
                if (!serialNoData.AsEnumerable().Any(r => r.Field<string>("SerialNo") == serial))
                {
                    serialNoData.Rows.Add(serial);
                }
            }
        }

        private void PopulateComboBox()
        {
            cmbSerialNo.Items.Clear();
            foreach (DataRow row in serialNoData.Rows)
            {
                string serial = row["SerialNo"].ToString();
                if (!cmbSerialNo.Items.Contains(serial))
                {
                    cmbSerialNo.Items.Add(serial);
                }
            }
            cmbSerialNo.SelectedIndex = -1; // No item selected by default
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

            // Add columns as in the original code
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
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
                    Font = new Font("Arial", 11)
                },
            });

            dgvA.DataSource = dt;
            dgvA.CellBorderStyle = DataGridViewCellBorderStyle.None;
            Debug.WriteLine("DataGridView updated successfully.");
        }

        private void cmbSerialNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cachedData == null || cachedData.Rows.Count == 0)
            {
                return;
            }

            string selectedSerialNo = cmbSerialNo.SelectedItem?.ToString();
            LoadData(selectedSerialNo);
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
                                LoadData(cmbSerialNo.SelectedItem?.ToString());
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
                                LoadData(cmbSerialNo.SelectedItem?.ToString());
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
                                LoadData(cmbSerialNo.SelectedItem?.ToString());
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
                                LoadData(cmbSerialNo.SelectedItem?.ToString());
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
                                LoadData(cmbSerialNo.SelectedItem?.ToString());
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
                                LoadData(cmbSerialNo.SelectedItem?.ToString());
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