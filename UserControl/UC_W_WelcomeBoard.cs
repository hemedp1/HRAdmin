using HRAdmin.Forms;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using DrawingFont = System.Drawing.Font;

namespace HRAdmin.UserControl  //UC_W_WelcomeBoard
{
    public partial class UC_W_WelcomeBoard : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private List<(string Company, string Purpose, DateTime? StartDate, DateTime? EndDate)> companyInfo = new List<(string, string, DateTime?, DateTime?)>();
        private int currentCompanyIndex = 0;
        private Timer companySwapTimer;

        public UC_W_WelcomeBoard(string username, string department)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;

            // Ensure components is initialized
            if (components == null)
            {
                components = new System.ComponentModel.Container();
            }

            // Initialize the timer and add to components for automatic disposal
            companySwapTimer = new Timer(components);
            companySwapTimer.Interval = 5000;
            companySwapTimer.Tick += CompanySwapTimer_Tick;
        }

        private void CompanySwapTimer_Tick(object sender, EventArgs e)
        {
            if (companyInfo.Count > 0)
            {
                currentCompanyIndex = (currentCompanyIndex + 1) % companyInfo.Count;
                var currentCompanyInfo = companyInfo[currentCompanyIndex];
                lblCompany.Text = currentCompanyInfo.Company;
                lblPurpose.Text = currentCompanyInfo.Purpose;
                lblStartDate.Text = currentCompanyInfo.StartDate?.ToString("dd.MM.yyyy") ?? "";
                lblEndDate.Text = currentCompanyInfo.EndDate?.ToString("dd.MM.yyyy") ?? "";
                LoadVisitorsForCompany(currentCompanyInfo.Company, currentCompanyInfo.Purpose); // Update DataGridView with visitors for the current company and purpose
            }
            else
            {
                lblCompany.Text = "No Visitors Today";
                lblPurpose.Text = "";
                lblStartDate.Text = "";
                lblEndDate.Text = "";
                dgv_Visitor.DataSource = null; // Clear DataGridView if no companies
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

        private void button2_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "HR && Admin";
            Form_Home.sharedButton4.Visible = false;
            Form_Home.sharedButton5.Visible = false;
            Form_Home.sharedButton6.Visible = false;
            Form_Home.sharedbtnVisitor.Visible = false;
            Form_Home.sharedbtnWithdrawEntry.Visible = false;
            Form_Home.sharedbtnNewVisitor.Visible = false;
            Form_Home.sharedbtnUpdate.Visible = false;
            UC_A_Admin ug = new UC_A_Admin(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        public void LoadVisitorNamesToDataGridView()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // Query to fetch distinct company names, purpose, earliest StartDate, and EndDate for valid date ranges
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
                                string company = reader["Company"].ToString();
                                string purpose = reader["Purpose"].ToString();
                                DateTime? startDate = reader["StartDate"] != DBNull.Value ? (DateTime?)reader["StartDate"] : null;
                                DateTime? endDate = reader["EndDate"] != DBNull.Value ? (DateTime?)reader["EndDate"] : null;
                                companyInfo.Add((company, purpose, startDate, endDate));
                            }
                        }
                    }

                    // Update lblCompany, lblPurpose, lblStartDate, lblEndDate, and DataGridView
                    if (companyInfo.Count > 0)
                    {
                        currentCompanyIndex = 0;
                        var currentCompanyInfo = companyInfo[currentCompanyIndex];
                        lblCompany.Text = currentCompanyInfo.Company;
                        lblPurpose.Text = currentCompanyInfo.Purpose;
                        lblStartDate.Text = currentCompanyInfo.StartDate?.ToString("dd.MM.yyyy") ?? "";
                        lblEndDate.Text = currentCompanyInfo.EndDate?.ToString("dd.MM.yyyy") ?? "";
                        LoadVisitorsForCompany(currentCompanyInfo.Company, currentCompanyInfo.Purpose); // Load visitors for the first company and purpose
                        if (companyInfo.Count > 1)
                        {
                            companySwapTimer.Start(); // Start timer if multiple companies
                        }
                        else
                        {
                            companySwapTimer.Stop(); // Stop timer if only one company
                        }
                    }
                    else
                    {
                        lblCompany.Text = "No Visitors Today";
                        lblPurpose.Text = "";
                        lblStartDate.Text = "";
                        lblEndDate.Text = "";
                        dgv_Visitor.DataSource = null; // Clear DataGridView
                        companySwapTimer.Stop();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                    // Query to fetch visitors for the specified company and purpose within valid date range
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
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgv_Visitor.DataSource = dt;

                            // Configure DataGridView properties
                            dgv_Visitor.ReadOnly = true;
                            dgv_Visitor.AllowUserToAddRows = false;

                            // Apply styling
                            StyleDataGridView(dgv_Visitor);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading visitors: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.ColumnHeadersVisible = false;
            dgv.Dock = DockStyle.Fill;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.RowTemplate.Height = 43; // Increase to 40 pixels (default is typically 22 pixels)
            dgv.ReadOnly = true;

            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.Black,
                    Font = new DrawingFont("Arial", 25),
                    BackColor = Color.Gainsboro,
                    Alignment = DataGridViewContentAlignment.MiddleCenter // Center all cell content
                };

                column.DefaultCellStyle.SelectionBackColor = Color.Gainsboro; // Change selection background to a visible color
                column.DefaultCellStyle.SelectionForeColor = Color.Black; // Ensure text remains visible on selection
                column.Resizable = DataGridViewTriState.False;
                column.ReadOnly = true;
            }
        }

        private void UC_WelcomeBoard_Load(object sender, EventArgs e)
        {
            LoadVisitorNamesToDataGridView();
        }

        private void lblStartDate_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void dgv_Visitor_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}