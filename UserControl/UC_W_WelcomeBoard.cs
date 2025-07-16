using HRAdmin.Forms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using DrawingFont = System.Drawing.Font;

namespace HRAdmin.UserControl
{
    public partial class UC_W_WelcomeBoard : System.Windows.Forms.UserControl
    {
        // Member variables
        private string loggedInUser;
        private string loggedInDepart;
        private List<(string Company, string Purpose, DateTime? StartDate, DateTime? EndDate)> companyInfo =
            new List<(string, string, DateTime?, DateTime?)>();
        private int currentCompanyIndex = 0;

        // Timers
        private Timer companySwapTimer;
        private Timer _refreshTimer;
        private Timer networkStatusTimer;

        // Network status
        private bool _networkAvailable = true;
        private DateTime _lastNetworkCheck = DateTime.MinValue;
        private const int NetworkCheckInterval = 5000;
        private bool lastNetworkStatus = true;

        // UI elements
        private Panel networkStatusPanel;
        private Label networkStatusLabel;

        public UC_W_WelcomeBoard(string username, string department)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;

            // Initialize components container if null
            components = components ?? new System.ComponentModel.Container();

            InitializeWelcomeBoardUI();
            InitializeNetworkStatusUI();
            InitializeTimers();

            // Load initial data
            LoadVisitorNamesToDataGridView();
        }

        private void InitializeWelcomeBoardUI()
        {
            // Configure company info labels
            lblCompany.Font = new Font("Arial", 28, FontStyle.Bold);
            lblCompany.ForeColor = Color.White;
            lblCompany.TextAlign = ContentAlignment.MiddleCenter;

            lblPurpose.Font = new Font("Arial", 20, FontStyle.Regular);
            lblPurpose.ForeColor = Color.White;
            lblPurpose.TextAlign = ContentAlignment.MiddleCenter;

            lblStartDate.Font = new Font("Arial", 16, FontStyle.Regular);
            lblStartDate.ForeColor = Color.White;
            lblStartDate.TextAlign = ContentAlignment.MiddleCenter;

            // Configure DataGridView
            //dgv_Visitor.BackgroundColor = Color.FromArgb(240, 240, 240);
            dgv_Visitor.BorderStyle = BorderStyle.None;
            dgv_Visitor.EnableHeadersVisualStyles = false;
            dgv_Visitor.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        }

        private void InitializeNetworkStatusUI()
        {
            // Create and configure network status panel
            networkStatusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.Transparent,
                Visible = false
            };

            // Create and configure status label
            networkStatusLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.White
            };

            networkStatusPanel.Controls.Add(networkStatusLabel);
            this.Controls.Add(networkStatusPanel);
            this.Controls.SetChildIndex(networkStatusPanel, 0);

            // Setup timer to auto-hide notifications
            networkStatusTimer = new Timer { Interval = 2000 };
            networkStatusTimer.Tick += (s, e) =>
            {
                networkStatusPanel.Visible = false;
                networkStatusTimer.Stop();
            };
        }

        private void InitializeTimers()
        {
            // Company rotation timer
            companySwapTimer = new Timer(components)
            {
                Interval = 5000 // 5 seconds
            };
            companySwapTimer.Tick += CompanySwapTimer_Tick;

            // Data refresh timer
            _refreshTimer = new Timer(components)
            {
                Interval = 30000 // 30 seconds
            };
            _refreshTimer.Tick += (s, e) => LoadVisitorNamesToDataGridView();
            _refreshTimer.Start();
        }

        private void ShowNetworkNotification(string message, bool isError)
        {
            if (networkStatusPanel.InvokeRequired)
            {
                networkStatusPanel.Invoke(new Action(() => ShowNetworkNotification(message, isError)));
                return;
            }

            networkStatusPanel.BackColor = isError ? Color.FromArgb(220, 50, 50) : Color.FromArgb(50, 150, 50);
            networkStatusLabel.Text = message;
            networkStatusPanel.Visible = true;
            networkStatusPanel.BringToFront();

            networkStatusTimer.Stop();
            networkStatusTimer.Start();
        }

        private bool CheckNetworkAvailable()
        {
            if ((DateTime.Now - _lastNetworkCheck).TotalMilliseconds < NetworkCheckInterval)
                return _networkAvailable;

            _lastNetworkCheck = DateTime.Now;
            try
            {
                bool currentStatus = NetworkInterface.GetIsNetworkAvailable();
                if (currentStatus != lastNetworkStatus)
                {
                    lastNetworkStatus = currentStatus;
                    ShowNetworkNotification(
                        currentStatus ? "Network connection restored" : "Network connection lost",
                        !currentStatus);
                }
                _networkAvailable = currentStatus;
                return _networkAvailable;
            }
            catch
            {
                _networkAvailable = false;
                return false;
            }
        }

        private void CompanySwapTimer_Tick(object sender, EventArgs e)
        {
            if (companyInfo.Count > 0)
            {
                currentCompanyIndex = (currentCompanyIndex + 1) % companyInfo.Count;
                UpdateCompanyDisplay(companyInfo[currentCompanyIndex]);
            }
            else
            {
                ShowNoVisitorsMessage();
            }
        }

        private void UpdateCompanyDisplay((string Company, string Purpose, DateTime? StartDate, DateTime? EndDate) info)
        {
            lblCompany.Text = info.Company;
            lblPurpose.Text = info.Purpose;
            lblStartDate.Text = $"From {info.StartDate?.ToString("dd.MM.yyyy") ?? ""}  To {info.EndDate?.ToString("dd.MM.yyyy") ?? ""}";
            LoadVisitorsForCompany(info.Company, info.Purpose);
        }

        private void ShowNoVisitorsMessage()
        {
            lblCompany.Text = "No Visitors Today";
            lblPurpose.Text = "";
            lblStartDate.Text = "";
            dgv_Visitor.DataSource = null;
        }

        public void LoadVisitorNamesToDataGridView()
        {
            if (!CheckNetworkAvailable())
            {
                ShowNetworkNotification("No network connection - using cached data", true);
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    string companyQuery = @"
                        SELECT Company, Purpose, MIN(StartDate) AS StartDate, MAX(EndDate) AS EndDate
                        FROM tbl_WelcomeBoard
                        WHERE CAST(GETDATE() AS DATE) BETWEEN CAST(StartDate AS DATE) AND CAST(EndDate AS DATE)
                        AND Company IS NOT NULL AND Company != ''
                        GROUP BY Company, Purpose";

                    using (SqlCommand cmd = new SqlCommand(companyQuery, con))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            companyInfo.Clear();
                            while (reader.Read())
                            {
                                companyInfo.Add((
                                    reader["Company"].ToString(),
                                    reader["Purpose"].ToString(),
                                    reader["StartDate"] != DBNull.Value ? (DateTime?)reader["StartDate"] : null,
                                    reader["EndDate"] != DBNull.Value ? (DateTime?)reader["EndDate"] : null
                                ));
                            }
                        }
                    }

                    if (companyInfo.Count > 0)
                    {
                        currentCompanyIndex = 0;
                        UpdateCompanyDisplay(companyInfo[currentCompanyIndex]);
                        companySwapTimer.Enabled = companyInfo.Count > 1;
                    }
                    else
                    {
                        ShowNoVisitorsMessage();
                        companySwapTimer.Stop();
                    }
                }
                catch (Exception ex)
                {
                    ShowNetworkNotification("Error loading visitor data", true);
                    Console.WriteLine($"Error loading visitor data: {ex.Message}");
                }
            }
        }

        private void LoadVisitorsForCompany(string company, string purpose)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    string visitorQuery = @"
                        SELECT Visitor AS VisitorName
                        FROM tbl_WelcomeBoard
                        WHERE Company = @Company
                        AND Purpose = @Purpose
                        AND CAST(GETDATE() AS DATE) BETWEEN CAST(StartDate AS DATE) AND CAST(EndDate AS DATE)
                        AND Visitor IS NOT NULL AND Visitor != ''
                        ORDER BY Visitor";

                    using (SqlCommand cmd = new SqlCommand(visitorQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Company", company);
                        cmd.Parameters.AddWithValue("@Purpose", purpose);

                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);

                        if (dgv_Visitor.InvokeRequired)
                        {
                            dgv_Visitor.Invoke(new Action(() =>
                            {
                                dgv_Visitor.DataSource = dt;
                                StyleDataGridView(dgv_Visitor);
                            }));
                        }
                        else
                        {
                            dgv_Visitor.DataSource = dt;
                            StyleDataGridView(dgv_Visitor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowNetworkNotification("Error loading visitor list", true);
                    Console.WriteLine($"Error loading visitors: {ex.Message}");
                }
            }
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            if (dgv.InvokeRequired)
            {
                dgv.Invoke(new Action(() => StyleDataGridView(dgv)));
                return;
            }

            dgv.ColumnHeadersVisible = false;
            dgv.Dock = DockStyle.Fill;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.RowTemplate.Height = 37;
            dgv.ReadOnly = true;
            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.Black,
                    Font = new DrawingFont("Arial", 25),
                    BackColor = Color.Gainsboro,
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    SelectionBackColor = Color.Gainsboro,
                    SelectionForeColor = Color.Black
                };
                column.Resizable = DataGridViewTriState.False;
            }

            foreach (DataGridViewRow row in dgv.Rows)
            {
                row.Height = 37;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Form_Home.sharedPanel != null)
            {
                Form_Home.sharedLabel.Text = "HR && Admin";
                Form_Home.sharedButton4.Visible = false;
                Form_Home.sharedButton5.Visible = false;
                Form_Home.sharedButton6.Visible = false;
                Form_Home.sharedbtnVisitor.Visible = false;
                Form_Home.sharedbtnWithdrawEntry.Visible = false;
                Form_Home.sharedbtnNewVisitor.Visible = false;
                Form_Home.sharedbtnUpdate.Visible = false;

                var adminControl = new UC_A_Admin(loggedInUser, loggedInDepart);
                Form_Home.sharedPanel.Controls.Clear();
                adminControl.Dock = DockStyle.Fill;
                Form_Home.sharedPanel.Controls.Add(adminControl);
                adminControl.BringToFront();
            }
            else
            {
                MessageBox.Show("Panel not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void UC_WelcomeBoard_Load(object sender, EventArgs e)
        {
            LoadVisitorNamesToDataGridView();
        }

        // Empty event handlers for designer-generated events
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void dgv_Visitor_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}