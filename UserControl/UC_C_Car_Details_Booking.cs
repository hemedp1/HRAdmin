using HRAdmin.Forms;
using System;
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
using System.Xml.Serialization;

namespace HRAdmin.UserControl
{
    public partial class UC_C_Car_Details_Booking : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        
        public UC_C_Car_Details_Booking(string username)
        {
            InitializeComponent();
            loggedInUser = username;
            cmbFilter.SelectedIndexChanged += cmbFilter_SelectedIndexChanged;
            cmbFilter.SelectedItem = "Weekly"; // Set default filter to Weekly
            ApplyFilter("Weekly"); // Load data for Weekly filter
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
        private void UC_Car_Details_Booking_Load(object sender, EventArgs e) 
        {

            cmbFilter.SelectedItem = "Weekly"; // Set default filter to Weekly
            ApplyFilter("Weekly"); // Load data for Weekly filter
            cmbFilter.SelectedIndexChanged += cmbFilter_SelectedIndexChanged;
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "HR && Admin";
            Form_Home.sharedButton.Visible = false;
            Form_Home.sharedButtonew.Visible = false;
            Form_Home.sharedButtonBC.Visible = false;
            Form_Home.sharedButton2.Visible = false;   // withdraw
            Form_Home.sharedButton3.Visible = false;  // replace
            Form_Home.sharedButtonbtnApp.Visible = false;
            Form_Home.sharedButtonbtnWDcar.Visible = false;
            Form_Home.sharedbuttonInspect.Visible = false;
            Form_Home.sharedbtn_Accident.Visible = false;
            UC_A_Admin ug = new UC_A_Admin(loggedInUser, loggedInDepart);
            addControls(ug);
        }
        private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFilter.SelectedItem != null)
            {
                ApplyFilter(cmbFilter.SelectedItem.ToString());
            }
        }
        private void ApplyFilter(string filterType)
        {
            if (string.IsNullOrEmpty(filterType)) return;

            string query = "";
            DateTime today = DateTime.Today;
            TimeSpan nowTime = DateTime.Now.TimeOfDay; // Get current time

            switch (filterType)
            {
                case "Daily":
                    query = @"
                    SELECT 
                    BookingID, 
                    DriverName, 
                    IndexNo, 
                    Destination, 
                    RequestDate, 
                    Purpose, 
                    AssignedCar, 
                    StatusCheck, 
                    CheckBy,
                    Acknowledgement,
                    AcknowledgementTime,
                    CASE 
                        WHEN DateChecked IS NULL THEN 'Pending'
                        ELSE CONVERT(VARCHAR, DateChecked, 120)
                    END AS DateChecked, 
                    Status, 
                    ApproveBy, 
                    CASE 
                        WHEN DateApprove IS NULL THEN 'Pending'
                        ELSE CONVERT(VARCHAR, DateApprove, 120)
                    END AS DateApprove,

                    CONVERT(VARCHAR(5), StartDate, 108) AS StartDate, 
                    CONVERT(VARCHAR(5), EndDate, 108) AS EndDate 
                    FROM tbl_CarBookings 
                    WHERE CAST(RequestDate AS DATE) = @Today 
                    AND (EndDate >= @NowTime OR RequestDate > @Today)
                    ORDER BY RequestDate ASC, StartDate ASC";
                    break;
                case "Weekly":
                    query = @"
                    SELECT
                    BookingID, 
                    DriverName, 
                    IndexNo, 
                    Destination, 
                    RequestDate, 
                    Purpose, 
                    AssignedCar, 
                    StatusCheck, 
                    CheckBy,
                    Acknowledgement,
                    AcknowledgementTime,
                    CASE 
                        WHEN DateChecked IS NULL THEN 'Pending'
                        ELSE CONVERT(VARCHAR, DateChecked, 120)
                    END AS DateChecked, 
                    Status, 
                    ApproveBy, 
                    CASE 
                        WHEN DateApprove IS NULL THEN 'Pending'
                        ELSE CONVERT(VARCHAR, DateApprove, 120)
                    END AS DateApprove,
                    CONVERT(VARCHAR(5), StartDate, 108) AS StartDate, 
                    CONVERT(VARCHAR(5), EndDate, 108) AS EndDate 
                    FROM tbl_CarBookings 
                    WHERE DATEDIFF(WEEK, 0, RequestDate) = DATEDIFF(WEEK, 0, @Today)
                    AND YEAR(RequestDate) = YEAR(@Today) 
                    AND (EndDate >= @NowTime OR RequestDate > @Today)
                    ORDER BY RequestDate ASC, StartDate ASC";
                    break;
                case "Monthly":
                    query = @"
                    SELECT
                    BookingID, 
                    DriverName, 
                    IndexNo, 
                    Destination, 
                    RequestDate, 
                    Purpose, 
                    AssignedCar, 
                    StatusCheck, 
                    CheckBy,
                    Acknowledgement,
                    AcknowledgementTime,
                    CASE 
                        WHEN DateChecked IS NULL THEN 'Pending'
                        ELSE CONVERT(VARCHAR, DateChecked, 120)
                    END AS DateChecked, 
                    Status, 
                    ApproveBy, 
                    CASE 
                        WHEN DateApprove IS NULL THEN 'Pending'
                        ELSE CONVERT(VARCHAR, DateApprove, 120)
                    END AS DateApprove,
                    CONVERT(VARCHAR(5), StartDate, 108) AS StartDate, 
                    CONVERT(VARCHAR(5), EndDate, 108) AS EndDate 
                    FROM tbl_CarBookings 
                    WHERE MONTH(RequestDate) = MONTH(@Today) 
                    AND YEAR(RequestDate) = YEAR(@Today) 
                    AND (EndDate >= @NowTime OR RequestDate > @Today)
                    ORDER BY RequestDate ASC, StartDate ASC";
                    break;
                default:
                    return; // Exit if filter type is invalid
            }

            LoadFilteredData(query, today, nowTime);
        }
        private void LoadFilteredData(string query, DateTime today, TimeSpan nowTime)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Today", today);
                        cmd.Parameters.AddWithValue("@NowTime", nowTime);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        dataGridView1.Columns.Clear();
                        dataGridView1.AutoGenerateColumns = false;

                        // Enable scrolling
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        dataGridView1.ScrollBars = ScrollBars.Both; // Enable both vertical & horizontal scrolling

                        dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                        {
                            Font = new Font("Arial", 11, FontStyle.Bold),
                        };

                        string[] columnNames = { "DriverName", "IndexNo", "RequestDate", "Destination", "Purpose", "StartDate", "EndDate", "StatusCheck", "CheckBy", "DateChecked", "Status", "ApproveBy", "DateApprove", "AssignedCar", "Acknowledgement", "AcknowledgementTime" };

                        for (int i = 0; i < columnNames.Length; i++)
                        {
                            string col = columnNames[i];
                            string headerText;

                            if (col == "DriverName")
                                headerText = "Driver";
                            else if (col == "IndexNo")
                                headerText = "Index No";
                            else if (col == "RequestDate")
                                headerText = "Request Date";
                            else if (col == "Destination")
                                headerText = "Destination";
                            else if (col == "StartDate")
                                headerText = "Start Time";
                            else if (col == "EndDate")
                                headerText = "End Time";
                            else if (col == "Status")
                                headerText = "Admin Status Approval";
                            else if (col == "Purpose")
                                headerText = "Purpose";
                            else if (col == "StatusCheck")
                                headerText = "HOD Status Check";
                            else if (col == "CheckBy")
                                headerText = "Check By";
                            else if (col == "DateChecked")
                                headerText = "Checked Date";
                            else if (col == "ApproveBy")
                                headerText = "Approve By";
                            else if (col == "DateApprove")
                                headerText = "Approved Date";
                            else if (col == "AssignedCar")
                                headerText = "Car";
                            else if (col == "Acknowledgement")
                                headerText = "Acknowledgement";
                            else if (col == "AcknowledgementTime")
                                headerText = "Acknowledged Time";
                            else
                                headerText = col.Replace("_", " "); // Default formatting

                            var column = new DataGridViewTextBoxColumn()
                            {
                                HeaderText = headerText,
                                DataPropertyName = col,
                                Width = 130,
                                //AutoSizeMode = i == columnNames.Length - 1  ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.None,
                                DefaultCellStyle = new DataGridViewCellStyle
                                {
                                    ForeColor = Color.MidnightBlue,
                                    Font = new Font("Arial", 11)
                                }
                            };

                            dataGridView1.Columns.Add(column);
                        }
                        dataGridView1.DataSource = dt;
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
