﻿using HRAdmin.Forms;
using SLRDbConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
//using System.Net;
//using System.Net.Mail;
using HRAdmin.Components;

namespace HRAdmin.UserControl
{
    public partial class UC_C_BookingCar : System.Windows.Forms.UserControl
    {
        DbConnector db;
        private string loggedInUser;
        private string loggedInIndex;
        private string loggedInDepart;
        public UC_C_BookingCar(string username, string index, string depart)
        {
            InitializeComponent();
            
            loggedInUser = username;
            loggedInIndex = index;
            loggedInDepart = depart;
            LoadData();
            dTDay.ValueChanged += dTDay_ValueChanged;
            PopulateTimeComboBoxes();
        }
        /*private void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("syazwanbunander1997@gmail.com"); // ✅ Sender email
                mail.To.Add(toEmail);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.Credentials = new NetworkCredential("syazwanbunander1997@gmail.com", "htam xlil vnzn kiwq"); // ✅ Must match sender
                smtp.EnableSsl = true;

                smtp.Send(mail);
                MessageBox.Show("Notification for your booking will be sent to your approver.", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //MessageBox.Show("Notification for your booking will be sent to your approver.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to send notification email: " + ex.Message);
            }
        }*/


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
                                    Form_Home.sharedbtn_verify.Visible = false;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);
                                }
                                else if  (MA == "2")
                                {
                                    Form_Home.sharedLabel.Text = "Admin > Car Reservation";
                                    Form_Home.sharedButton.Visible = false;
                                    Form_Home.sharedButtonew.Visible = false;
                                    Form_Home.sharedButton2.Visible = false;   // withdraw
                                    Form_Home.sharedButton3.Visible = false;   // replace
                                    Form_Home.sharedbtn_verify.Visible = false;
                                    Form_Home.sharedButtonbtnApp.Visible = true;
                                    Form_Home.sharedButtonBC.Visible = true;
                                    Form_Home.sharedButtonbtnWDcar.Visible = true;
                                    Form_Home.sharedbtn_Accident.Visible = true;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);
                                }
                                else 
                                {
                                    Form_Home.sharedbtn_verify.Visible = false;
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
        private void UC_C_BookingCar_Load(object sender, EventArgs e)
        {
            dTDay.ValueChanged += dTDay_ValueChanged;
            dataGridView1.CellClick += dataGridView1_CellClick;
        }
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            //MessageBox.Show($"loggedInDepart: {loggedInDepart}");
            DateTime selectedDate = dTDay.Value.Date;
            if (string.IsNullOrWhiteSpace(txtDes.Text))
            {
                MessageBox.Show("Please input the destination.");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPurpose.Text))
            {
                MessageBox.Show("Please input your purpose", "Purpose", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (cmbOut.SelectedItem == null || cmbIn.SelectedItem == null)
            {
                MessageBox.Show("Please select start and end times", "Time", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (!DateTime.TryParseExact(cmbOut.SelectedItem.ToString(), "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime time1) ||
                !DateTime.TryParseExact(cmbIn.SelectedItem.ToString(), "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime time2))
            {
                MessageBox.Show("Invalid time format. Please select a valid time", "Invalid Time Format", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (time2 <= time1)
            {
                MessageBox.Show("End time must be later than start time", "Invalid Time", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            try
            {
                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ if booked

                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string checkQuery = @"
                SELECT COUNT(*) FROM tbl_CarBookings 
                WHERE CAST(RequestDate AS DATE) = @SelectedDate
                AND (
                    (@StartDate >= StartDate AND @StartDate < EndDate) 
                    OR (@EndDate > StartDate AND @EndDate <= EndDate) 
                    OR (StartDate >= @StartDate AND EndDate <= @EndDate) -- Ensures full overlap
                )";

                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@SelectedDate", selectedDate);
                    checkCmd.Parameters.AddWithValue("@StartDate", time1.TimeOfDay); // Use only time
                    checkCmd.Parameters.AddWithValue("@EndDate", time2.TimeOfDay);   // Use only time

                    int existingBookings = (int)checkCmd.ExecuteScalar();

                    //if (existingBookings > 0)
                    //{
                    //    MessageBox.Show("The selected date and time already reserved.", "Please choose another time.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //     return;
                    //}

                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ error condition END and start not over 30m

                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ If pass error condition

                    //if (existingBookings > 0)
                    //{
                    //    MessageBox.Show("The selected date and time already reserved.","Please choose another time.", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //    return;
                    //}

                    // Insert new booking
                    string insertQuery = @"
                INSERT INTO tbl_CarBookings (DriverName, IndexNo, Depart, RequestDate, Destination, StartDate, EndDate, Purpose, StatusCheck, CheckBy, Status, ApproveBy, AssignedCar) " +
                                "VALUES (@DriverName, @IndexNo, @Depart, @RequestDate, @Destination, @StartDate, @EndDate, @Purpose, 'Pending', 'Pending', 'Pending','Pending','Pending')";




                    SqlCommand insertCmd = new SqlCommand(insertQuery, con);
                    insertCmd.Parameters.AddWithValue("@DriverName", loggedInUser);
                    insertCmd.Parameters.AddWithValue("@IndexNo", loggedInIndex);
                    insertCmd.Parameters.AddWithValue("@Depart", loggedInDepart);
                    insertCmd.Parameters.AddWithValue("@RequestDate", selectedDate);
                    insertCmd.Parameters.AddWithValue("@Destination", txtDes.Text);
                    insertCmd.Parameters.AddWithValue("@StartDate", cmbOut.Text);
                    insertCmd.Parameters.AddWithValue("@EndDate", cmbIn.Text); // Store only time
                    insertCmd.Parameters.AddWithValue("@Purpose", txtPurpose.Text);   // Store only time

                    insertCmd.ExecuteNonQuery();

                    MessageBox.Show("Booking successfully submitted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    /*
                    string destination = txtDes.Text;


                    //string requestDate = DateTime.Now.ToString("dd MMM yyyy");
                    string requestdate = dTDay.Text;
                    DateTime parsedDate = DateTime.Parse(requestdate);
                    string formattedDate = parsedDate.ToString("dd/MM/yyyy");

                    Console.WriteLine(formattedDate);

                    string purpose = txtPurpose.Text;
                    string requester = UserSession.loggedInName;

                    string subject = "HEM Admin Accessibility Notification: New Car Booking Request Awaiting Approval";

                    string body = $@"
                                    <p>Dear Approver,</p>
                                    <p>A new <strong>car booking request</strong> has been submitted by <strong>{requester}</strong>.</p>

                                    <p><u>Booking Summary:</u></p>
                                    <ul>
                                        <li><strong>Destination:</strong> {destination}</li>
                                        <li><strong>Request Date:</strong> {formattedDate}</li>
                                        <li><strong>Purpose:</strong> {purpose}</li>
                                    </ul>

                                    <p>Please log in to the system to review and approve the request.</p>

                                    <p>Thank you,<br/>HEM Admin Accessibility</p>
                                ";

                    SendEmail("hemedp1@hosiden.com", subject, body);
                    */

                    //SendEmail("hemedp1@hosiden.com", "New Request Submitted", "A new request has been submitted for your approval.");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("QAn error occurred while processing the reservation: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoadData();



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
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }
        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {

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

            string query = $@"
                SELECT COUNT(*) AS TotalAvailableCars
                FROM tbl_Cars c
                LEFT JOIN tbl_CarBookings cb
                ON c.CarPlate = cb.AssignedCar
                AND cb.RequestDate = @RequestDate
                AND cb.StartDate <= @EndTime
                AND cb.EndDate >= @StartTime
                WHERE cb.AssignedCar IS NULL";

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

                        int totalAvailableCars = (int)cmd.ExecuteScalar();
                        TotalAvailablCar.Text = totalAvailableCars.ToString(); // Display result
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void dTDay_ValueChanged(object sender, EventArgs e)   //RequestDate
        {
            LoadData();
            LoadData1();
        }
        private void cmbOut_SelectedIndexChanged(object sender, EventArgs e)  //StartDate
        {
            LoadData1();
        }
        private void cmbIn_SelectedIndexChanged(object sender, EventArgs e)  //EndDate
        {
            LoadData1();
        }
        private void TotalAvailablCar_Click(object sender, EventArgs e)
        {
            LoadData1();
        }
    }
    
}
