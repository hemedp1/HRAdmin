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

namespace HRAdmin.UserControl
{
    public partial class UC_C_CarCheckFromManager : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        public UC_C_CarCheckFromManager(string username, string Depart)
        {
            InitializeComponent();
            
            loggedInDepart = Depart;
            loggedInUser = username;
            //MessageBox.Show($"Error on Report ID Selection: {loggedInDepart}");
            LoadPendingBookings();
            LoadData();
            dTDay.ValueChanged += dTDay_ValueChanged;
            LoadPendingBookings();
        }
        private void UC_C_CarCheckFromManager_Load(object sender, EventArgs e)
        {
            LoadData();
            dTDay.ValueChanged += dTDay_ValueChanged;
        }
        private void LoadPendingBookings()
        {
           
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = @"
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
                WHERE StatusCheck = 'Pending' AND Depart = @Depart";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Check the value before assigning
                        if (string.IsNullOrWhiteSpace(loggedInDepart))
                        {
                            //MessageBox.Show("loggedInDepart is null or empty.");
                            return;
                        }

                        cmd.Parameters.AddWithValue("@Depart", loggedInDepart);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            dataGridView1.DataSource = dt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a booking to verify.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rowIndex = dataGridView1.CurrentRow.Index;
            DateTime selectedDate = dTDay.Value.Date;
            string bookingIDStr = dataGridView1.Rows[rowIndex].Cells[0]?.Value?.ToString();

            int bookingID;
            if (!int.TryParse(bookingIDStr, out bookingID))
            {
                MessageBox.Show("Invalid Booking ID format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to verify this reservation?", "Verify Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    /////   The PIC must same department
                    string checkdepart = "SELECT DriverName, Depart FROM tbl_CarBookings WHERE BookingID = @BookingID";
                    //string check = "select a.Username,a.Department,a.Position, b.AccessLevel from tbl_Users a left join tbl_UsersLevel b ON a.Position = b.TitlePosition";
                    using (SqlCommand checkCmd = new SqlCommand(checkdepart, con))
                    {
                        checkCmd.Parameters.AddWithValue("@BookingID", bookingID);
                        object resultt = checkCmd.ExecuteScalar();
                        string department = resultt?.ToString();

                        //MessageBox.Show($"department: {department}");
                        //MessageBox.Show($"loggedInDepart: {loggedInDepart}");

                        if (string.IsNullOrEmpty(department) || department != loggedInDepart)
                        {
                            MessageBox.Show("Cannot proceed. Must be on the same department.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }


                    //      Pass all case, verify execute
                    string query = "UPDATE tbl_CarBookings SET DateChecked = @DateChecked, StatusCheck = 'Checked', CheckBy = @loggedInUser WHERE BookingID = @BookingID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@BookingID", bookingID);
                        cmd.Parameters.AddWithValue("@DateChecked", selectedDate);
                        cmd.Parameters.AddWithValue("@loggedInUser", loggedInUser);

                        try
                        {
                            
                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Reservation verified successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadPendingBookings();
                                //LoadData(); // Refresh DataGridView
                            }
                            else
                            {
                                MessageBox.Show("No record updated. Please check the Booking ID.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
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
        private void CheckUserAccess(string username)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = "SELECT AA, MA FROM tbl_Users WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string AA = reader["AA"].ToString();
                                string MA = reader["MA"].ToString();

                                if (AA == "1")
                                {
                                    Form_Home.sharedLabel.Text = "Admin > Car Reservation";
                                    Form_Home.sharedButton.Visible = false;
                                    Form_Home.sharedButtonew.Visible = false;
                                    Form_Home.sharedButton2.Visible = false;   // withdraw
                                    Form_Home.sharedButton3.Visible = false;   // replace
                                    Form_Home.sharedButtonBC.Visible = true;
                                    Form_Home.sharedButtonbtnApp.Visible = true;
                                    Form_Home.sharedButtonbtnWDcar.Visible = true;
                                    Form_Home.sharedbuttonInspect.Visible = true;
                                    Form_Home.sharedbtn_Accident.Visible = true;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);
                                }
                                else if (MA == "2")
                                {
                                    Form_Home.sharedLabel.Text = "Admin > Car Reservation";
                                    Form_Home.sharedButton.Visible = false;
                                    Form_Home.sharedButtonew.Visible = false;
                                    Form_Home.sharedButton2.Visible = false;   // withdraw
                                    Form_Home.sharedButton3.Visible = false;   // replace
                                    Form_Home.sharedButtonbtnApp.Visible = true;
                                    Form_Home.sharedButtonBC.Visible = true;
                                    Form_Home.sharedButtonbtnWDcar.Visible = true;
                                    Form_Home.sharedbtn_Accident.Visible = true;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);
                                }
                                else
                                {
                                    Form_Home.sharedLabel.Text = "Admin > Car Reservation";
                                    Form_Home.sharedButton.Visible = false;
                                    Form_Home.sharedButtonew.Visible = false;
                                    Form_Home.sharedButton2.Visible = false;   // withdraw
                                    Form_Home.sharedButton3.Visible = false;   // replace
                                    Form_Home.sharedbuttonInspect.Visible = false;
                                    Form_Home.sharedButtonbtnApp.Visible = false;
                                    Form_Home.sharedButtonBC.Visible = true;
                                    Form_Home.sharedButtonbtnWDcar.Visible = true;
                                    Form_Home.sharedbtn_Accident.Visible = true;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);
                                }
                            }
                            else
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            CheckUserAccess(loggedInUser);
        }
        private void LoadData()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = @"
                    SELECT BookingID, DriverName, IndexNo, RequestDate, Destination, Purpose, AssignedCar, Status, ApproveBy, DateApprove, StatusCheck, CheckBy, DateChecked,
                           CONVERT(VARCHAR(5), StartDate, 108) AS StartDate, 
                           CONVERT(VARCHAR(5), EndDate, 108) AS EndDate 
                    FROM tbl_CarBookings 
                    WHERE CONVERT(date, RequestDate) = @SelectedDate AND StatusCheck = 'Pending'";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SelectedDate", dTDay.Value.Date);
                        //cmd.Parameters.AddWithValue("@Depart", string.IsNullOrEmpty(cmbDepart.Text) ? "" : cmbDepart.Text);

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        dataGridView1.Columns.Clear();
                        dataGridView1.AutoGenerateColumns = false;

                        // Enable scrolling

                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        dataGridView1.ScrollBars = ScrollBars.Both; // Enable both vertical & horizontal scrolling


                        dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                        {
                            Font = new Font("Arial", 11, FontStyle.Bold),
                        };

                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "ID",
                            DataPropertyName = "BookingID",
                            Width = 80,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                ForeColor = Color.MidnightBlue,
                                Font = new Font("Arial", 11)
                            }
                        });

                        string[] columnNames = { "DriverName", "IndexNo", "RequestDate", "Destination", "Purpose", "StartDate", "EndDate", "StatusCheck", "CheckBy", "DateChecked", "Status", "ApproveBy", "DateApprove", "AssignedCar" };

                        foreach (var col in columnNames)
                        {
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
                            else if (col == "AssignedCar")
                                headerText = "Car";
                            else if (col == "ApproveBy")
                                headerText = "Approve By";
                            else if (col == "DateApprove")
                                headerText = "Approved Date";
                            else if (col == "StatusCheck")
                                headerText = "HOD Status Check";
                            else if (col == "CheckBy")
                                headerText = "Check By";
                            else if (col == "DateChecked")
                                headerText = "Checked Date";
                            else
                                headerText = col.Replace("_", " "); // Default formatting

                            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                            {
                                HeaderText = headerText,
                                DataPropertyName = col,
                                Width = 120,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                                DefaultCellStyle = new DataGridViewCellStyle
                                {
                                    ForeColor = Color.MidnightBlue,
                                    Font = new Font("Arial", 11)
                                }
                            });

                        }
                        dataGridView1.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dTDay_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }
    }
}
