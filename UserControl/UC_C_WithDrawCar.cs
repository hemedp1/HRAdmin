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
 
    public partial class UC_C_WithDrawCar : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInIndex;
        public UC_C_WithDrawCar(string username, string index)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInIndex = index;
            LoadData();
            dTDay.ValueChanged += dTDay_ValueChanged;
        }
        private void dTDay_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        private void btnWithdraw_Click(object sender, EventArgs e)
        {
            int rowIndex = dataGridView1.CurrentRow.Index;
            string carIDStr = dataGridView1.Rows[rowIndex].Cells[0]?.Value?.ToString();
            string selectedPerson = dataGridView1.Rows[rowIndex].Cells[1]?.Value?.ToString();

            if (string.IsNullOrEmpty(selectedPerson) || string.IsNullOrEmpty(carIDStr))
            {
                MessageBox.Show("Error retrieving meeting details. Please check column names.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }



            int bookingID = Convert.ToInt32(carIDStr);

            if (selectedPerson != loggedInUser)
            {
                MessageBox.Show("You can only withdraw your own reservation", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirm = MessageBox.Show("Are you sure you want to withdraw this reservation?", "Withdrawal Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                    {
                        con.Open();
                        string deleteQuery = "DELETE FROM tbl_CarBookings WHERE DriverName = @DriverName AND BookingID = @BookingID";

                        using (SqlCommand cmd = new SqlCommand(deleteQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@DriverName", selectedPerson);
                            cmd.Parameters.AddWithValue("@BookingID", bookingID);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Reservation withdrawn successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData(); // Refresh DataGridView
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error withdrawing reservation: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }
        private void LoadData()
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
                                    WHERE CONVERT(date, RequestDate) = @SelectedDate";
                    //AND (@Department = '' OR Department = @Department)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SelectedDate", dTDay.Value.Date);

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
                        // Add "ID" column (fixed 80px)
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
                                headerText = "Approved Status";
                            else if (col == "Purpose")
                                headerText = "Purpose";
                            else if (col == "StatusCheck")
                                headerText = "Check Status";
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
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void addControls(System.Windows.Forms.UserControl userControl)
        {
            if (Form_Home.sharedPanel != null && Form_Home.sharedLabel != null) // Ensure panel4 exists
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

                        using (SqlDataReader reader = cmd.ExecuteReader())  // Use SqlDataReader
                        {
                            if (reader.Read())
                            {
                                string AA = reader["AA"].ToString();
                                string MA = reader["MA"].ToString();


                                if (AA == "1")
                                {
                                    Form_Home.sharedButtonbtnApp.Visible = true;
                                    Form_Home.sharedbuttonInspect.Visible = true;
                                }
                                else if (MA == "2")
                                {
                                    Form_Home.sharedButtonbtnApp.Visible = true;
                                }
                                else
                                {
                                    Form_Home.sharedButtonbtnApp.Visible = false;
                                }
                            }
                            else
                            {
                                Form_Home.sharedButtonbtnApp.Visible = false;
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
            Form_Home.sharedButton.Visible = false;         //++++    Add People
            Form_Home.sharedButtonew.Visible = false;       //++++    New Meeting
            Form_Home.sharedButton2.Visible = false;        //++++    Withdraw Meeting
            Form_Home.sharedButton3.Visible = false;        //++++    Replace Ownership Meeting
            Form_Home.sharedButtonBC.Visible = true;        //++++    Book Car
            Form_Home.sharedButtonbtnWDcar.Visible = true;  //++++    Withdraw Car
            Form_Home.sharedbtn_Accident.Visible = true;
            //Form_Home.sharedbuttonInspect.Visible = true;

            Form_Home.sharedLabel.Text = "Admin > Car Reservation";
            UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
            addControls(ug);
        }
    }
}
