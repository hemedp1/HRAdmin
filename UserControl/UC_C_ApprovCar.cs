﻿using HRAdmin.Forms;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace HRAdmin.UserControl
{
    

    public partial class UC_C_ApprovCar : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private bool isFirstLoad = true;
        public UC_C_ApprovCar(string username)
        {
            InitializeComponent();
            loggedInUser = username;

            dTDay.ValueChanged += dTDay_ValueChanged;
            cmbCarSelection.SelectedIndexChanged += cmbCarSelection_SelectedIndexChanged;

            PopulateTimeComboBoxes();
            LoadData();                // Prepare grid structure
            dTDay_ValueChanged(null, null);  // Trigger correct first-time load
        }

        private void LoadPendingBookings(DateTime selectedDate)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                string query = @"SELECT BookingID, DriverName, IndexNo, RequestDate, Destination, Purpose, AssignedCar, Status, ApproveBy, DateApprove, StatusCheck, CheckBy, DateChecked,
                   CONVERT(VARCHAR(5), StartDate, 108) AS StartDate,
                   CONVERT(VARCHAR(5), EndDate, 108) AS EndDate
            FROM tbl_CarBookings
            WHERE CONVERT(date, RequestDate) = @SelectedDate AND Status = 'Pending'";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SelectedDate", selectedDate.Date); // Pass only the date part

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
        }
        private void LoadPendingBookingsall()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
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
                                WHERE Status = 'Pending'";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
        }
        private void btnApprove_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("No row selected. Please select a booking to approve or reject.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!rB_App.Checked && !rB_Rej.Checked)
            {
                MessageBox.Show("Please select either 'Approve' or 'Reject' before proceeding.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (rB_App.Checked && string.IsNullOrWhiteSpace(cmbCarSelection.Text))
            {
                MessageBox.Show("Please select a car before approving the booking.", "Missing Car Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rowIndex = dataGridView1.CurrentRow.Index;
            string selectedCar = cmbCarSelection.Text;
            DateTime selectedDate = dTDay.Value.Date;
            string meetingIDStr = dataGridView1.Rows[rowIndex].Cells[0]?.Value?.ToString();
            string selectedPerson = dataGridView1.Rows[rowIndex].Cells[1]?.Value?.ToString();

            if (string.IsNullOrEmpty(selectedPerson) || string.IsNullOrEmpty(meetingIDStr))
            {
                MessageBox.Show("Error retrieving reservation details. Please check column names.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(meetingIDStr, out int meetingID))
            {
                MessageBox.Show("Invalid Booking ID format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string action = rB_App.Checked ? "approve" : "reject";
            DialogResult confirm = MessageBox.Show($"Are you sure you want to {action} this reservation?",
                                                   $"Verify Confirmation",
                                                   MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    // Check if the booking status is 'Checked' before approving
                    string checkQuery = "SELECT StatusCheck FROM tbl_CarBookings WHERE BookingID = @BookingID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@BookingID", meetingID);
                        object result = checkCmd.ExecuteScalar();
                        string statusCheck = result?.ToString();

                        if (string.IsNullOrEmpty(statusCheck) || statusCheck != "Checked")
                        {
                            MessageBox.Show("Cannot proceed. Status Check must be 'Checked'.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                   
                    string updateQuery;
                    if (rB_App.Checked)
                    {
                        updateQuery = "UPDATE tbl_CarBookings SET AssignedCar = @Car, Status = 'Approved', ApproveBy = @loggedInUser, DateApprove = @selectedDate WHERE BookingID = @BookingID";
                    }
                    else // If Reject is selected
                    {
                        updateQuery = "UPDATE tbl_CarBookings SET AssignedCar = 'Not Available', Status = 'Rejected', ApproveBy = @loggedInUser, DateApprove = @selectedDate WHERE BookingID = @BookingID";
                    }

                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Car", selectedCar);
                        cmd.Parameters.AddWithValue("@BookingID", meetingID);
                        cmd.Parameters.AddWithValue("@loggedInUser", loggedInUser); 
                        cmd.Parameters.AddWithValue("@selectedDate", selectedDate);
                        cmd.ExecuteNonQuery();
                    }

                    LoadPendingBookings(selectedDate); // Refresh DataGridView
                    dataGridView1.Refresh();

                }
                LoadPendingBookingsall();
                //loadCars();
                MessageBox.Show("Car booking approved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                cmbCarSelection.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error {action}ing reservation: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            CheckUserAccess(loggedInUser);
            Form_Home.sharedLabel.Text = "Admin > Car Reservation";
            Form_Home.sharedButton.Visible = false;
            Form_Home.sharedButtonew.Visible = false;
            Form_Home.sharedButtonBC.Visible = true;
            Form_Home.sharedButton2.Visible = false;   // withdraw
            Form_Home.sharedButton3.Visible = false;  // replace
            Form_Home.sharedButtonbtnApp.Visible = true;
            Form_Home.sharedButtonbtnWDcar.Visible = true;
            Form_Home.sharedbtn_Accident.Visible = true;
            UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
            addControls(ug);
        }
        private void LoadData()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = @"
                    SELECT BookingID, DriverName, IndexNo, RequestDate, Destination, Purpose, AssignedCar, Status, ApproveBy, DateApprove,StatusCheck, CheckBy, DateChecked,
                           CONVERT(VARCHAR(5), StartDate, 108) AS StartDate, 
                           CONVERT(VARCHAR(5), EndDate, 108) AS EndDate 
                    FROM tbl_CarBookings 
                    WHERE CONVERT(date, RequestDate) = @SelectedDate AND Status = 'Pending'";

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
                            Width = 70,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                ForeColor = Color.MidnightBlue,
                                Font = new Font("Arial", 11)
                            }
                        });

                        string[] columnNames = { "DriverName", "IndexNo", "RequestDate", "Destination", "StartDate", "EndDate", "Status", "Purpose", "AssignedCar", "ApproveBy", "DateApprove", "StatusCheck", "CheckBy", "DateChecked" };

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
        private void cmbCarSelection_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void rB_App_CheckedChanged(object sender, EventArgs e)
        {
            cmbCarSelection.Visible = true;
            label2.Visible = true;
            label3.Visible = true;
            panel9.Visible = true;
          
        }

        private void rB_Rej_CheckedChanged(object sender, EventArgs e)
        {
            cmbCarSelection.Visible = false;
            label2.Visible = false;
            label3.Visible = false;
            panel9.Visible = false;
            
        }
        private void PopulateTimeComboBoxes()
        {
            for (int hour = 8; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute += 15) // 15-minute intervals
                {
                    string time = $"{hour:00}:{minute:00}";
                    cmbOut.Items.Add(time);
                    cmbIn.Items.Add(time);
                }
            }
            cmbOut.SelectedIndex = cmbOut.FindStringExact(""); // Default start time
            cmbIn.SelectedIndex = cmbIn.FindStringExact("");  // Default end time
            dTDay.Value = DateTime.Today; // Default to current date (2025-06-23)
        }
        private void LoadData1()
        {
            string requestDate = dTDay.Value.ToString("yyyy-MM-dd");
            string startTime = cmbOut.SelectedItem?.ToString() ?? "11:00";
            string endTime = cmbIn.SelectedItem?.ToString() ?? "15:00";

            string query = @"
        SELECT c.CarPlate
        FROM tbl_Cars c
        LEFT JOIN tbl_CarBookings cb
            ON c.CarPlate = cb.AssignedCar
            AND cb.RequestDate = @RequestDate
            AND cb.StartDate <= @StartTime
            AND cb.EndDate >= @EndTime
        WHERE cb.AssignedCar IS NULL AND c.Status = 'Available'";

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@RequestDate", requestDate);
                        cmd.Parameters.AddWithValue("@StartTime", startTime);
                        cmd.Parameters.AddWithValue("@EndTime", endTime);

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }

                        cmbCarSelection.DataSource = null; // Reset before rebinding
                        cmbCarSelection.DisplayMember = "CarPlate";
                        cmbCarSelection.ValueMember = "CarPlate";
                        cmbCarSelection.DataSource = dt;
                        cmbCarSelection.SelectedIndex = -1; 
                        if (dt.Rows.Count == 0)
                        {
                            cmbCarSelection.Text = "No cars available";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void dTDay_ValueChanged(object sender, EventArgs e)
        {
            if (isFirstLoad)
            {
                LoadPendingBookingsall();  // Show all on first load
                isFirstLoad = false;
            }
            else
            {
                LoadPendingBookings(dTDay.Value.Date);  // Apply filter after first load
            }

            LoadData();  // Keeps columns and formatting intact
            LoadData1(); // Updates car availability
        }

        private void cmbOut_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData1();
        }
        private void cmbIn_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadData1();
        }
        private void button1_Click(object sender, EventArgs e) //refresh button
        {
            LoadPendingBookingsall();
        }
    }
}
