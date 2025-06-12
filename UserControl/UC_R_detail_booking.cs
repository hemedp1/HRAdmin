using HRAdmin.Forms;
using SLRDbConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace HRAdmin.UserControl
{
    public partial class UC_R_detail_booking : System.Windows.Forms.UserControl
    {
        DbConnector db;
        private string loggedInUser;
        private string loggedInDepart;
        public UC_R_detail_booking(string username, string depart)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = depart;
            db = new DbConnector();
            dTDay.ValueChanged += dTDay_ValueChanged;
            
            cmbRoom.SelectedIndexChanged += cmbRoom_SelectedIndexChanged;
            LoadRooms();
            LoadData();
            dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView1_CellFormatting);
        }
        private void dTDay_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }
        private void LoadData()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {

                //MessageBox.Show($"roomId: {dTDay.Value.Date}");
                try
                {
                    con.Open();
                    string query = @"
                    SELECT MeetingID, Person, MeetingTitle, MeetingDate, MeetingRoom, Department, 
                           CONVERT(VARCHAR(5), StartTime, 108) AS StartTime, 
                           CONVERT(VARCHAR(5), EndTime, 108) AS EndTime 
                    FROM tbl_MeetingSchedule 
                    WHERE CONVERT(date, MeetingDate) = @SelectedDate 
                          AND (@BILIK = '' OR MeetingRoom = @BILIK)";
                          //AND (@Department = '' OR Department = @Department)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SelectedDate", dTDay.Value.Date);
                        cmd.Parameters.AddWithValue("@BILIK", string.IsNullOrEmpty(cmbRoom.Text) ? "" : cmbRoom.Text);
                        //cmd.Parameters.AddWithValue("@Department", string.IsNullOrEmpty(cmbDepart.Text) ? "" : cmbDepart.Text);




                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        // Clear existing columns if they exist
                        dataGridView1.Columns.Clear();
                        dataGridView1.AutoGenerateColumns = false;


                        dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                        {
                            //ForeColor = Color.Blue, // Text color
                            Font = new Font("Arial", 11, FontStyle.Bold), // Font: Arial, size 11, Bold
                            //BackColor = Color.LightGray
                        };
                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "ID",
                            DataPropertyName = "MeetingID",
                            Width = 50,
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                ForeColor = Color.MidnightBlue, // Correct syntax
                                Font = new Font("Arial", 11)
                            },

                        });
                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "Person",
                            DataPropertyName = "Person",
                            Width = 150,
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                ForeColor = Color.MidnightBlue, // Correct syntax
                                Font = new Font("Arial", 11)
                            },

                        });


                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "Title",
                            DataPropertyName = "MeetingTitle",
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                ForeColor = Color.MidnightBlue, // Correct syntax
                                Font = new Font("Arial", 11)
                            },
                            Width = 290,

                        });

                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "Room",
                            DataPropertyName = "MeetingRoom",
                            Width = 210,
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                ForeColor = Color.MidnightBlue, // Correct syntax
                                Font = new Font("Arial", 11)
                            },

                        });

                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "Meeting Date",
                            DataPropertyName = "MeetingDate",
                            //Width = 100,
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                ForeColor = Color.MidnightBlue, // Correct syntax
                                Font = new Font("Arial", 11)
                            },
                        }); ;

                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "Start Time",
                            DataPropertyName = "StartTime",
                            //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                            //Width = 100,
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                Format = "HH:mm",
                                ForeColor = Color.MidnightBlue,
                                Font = new Font("Arial", 11)
                            },
                            //DefaultCellStyle = new DataGridViewCellStyle { Format = "HH:mm" }, // Format to HH:mm
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        });

                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "End Time",
                            DataPropertyName = "EndTime",
                            //Width = 100,
                            //AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                Format = "HH:mm",
                                ForeColor = Color.MidnightBlue,
                                Font = new Font("Arial", 11)
                            },
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        });

                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "Section",
                            DataPropertyName = "Department",
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                ForeColor = Color.MidnightBlue,
                                Font = new Font("Arial", 11)
                            },
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        });
                        
                        dataGridView1.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView1.Columns[e.ColumnIndex].Name == "StartTime" || dataGridView1.Columns[e.ColumnIndex].Name == "EndTime")
            {
                if (e.Value != null && e.Value is TimeSpan)
                {
                    TimeSpan timeValue = (TimeSpan)e.Value;
                    e.Value = timeValue.ToString(@"hh\:mm");
                    e.FormattingApplied = true;
                }
            }
        }
        private void UC_detail_booking_Load(object sender, EventArgs e)
        {
            //LoadData();
            dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView1_CellFormatting);
            cmbRoom.SelectedIndexChanged += cmbRoom_SelectedIndexChanged;
            LoadRooms();
            dataGridView1.CellClick += dataGridView1_CellClick;
            

        }
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string roomId = cmbRoom.SelectedValue?.ToString();
            DateTime selectedDate = dTDay.Value.Date; // Ensure only DATE part is considered
            //string depart = cmbDepart.SelectedValue?.ToString() ?? cmbDepart.Text;



            if (string.IsNullOrWhiteSpace(txtMeeting.Text)) // Fixes the null check
            {
                MessageBox.Show("Please input the title of the meeting.");
                return;
            }

            if (cmbStart.SelectedItem == null || cmbend.SelectedItem == null)
            {
                MessageBox.Show("Please select start and end times.");
                return;
            }

            if (cmbRoom.SelectedItem == null || string.IsNullOrEmpty(roomId))
            {
                MessageBox.Show("Please select a valid room.");
                return;
            }


            // Try parsing selected times safely
            if (!DateTime.TryParseExact(cmbStart.SelectedItem.ToString(), "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime time1) ||
                !DateTime.TryParseExact(cmbend.SelectedItem.ToString(), "HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime time2))
            {
                MessageBox.Show("Invalid time format. Please select a valid time.");
                return;
            }
            //MessageBox.Show($"DDSDSDDWDWWD: {time1}");
            // Ensure end time is after start time
            if (time2 <= time1)
            {
                MessageBox.Show("End time must be later than start time.", "Invalid Time Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ if booked

                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Check if the room is already booked
                    string checkQuery = @"
                SELECT COUNT(*) FROM tbl_MeetingSchedule 
                WHERE MeetingRoom = @RoomID 
                AND CAST(MeetingDate AS DATE) = @SelectedDate
                AND (
                    (@StartTime >= StartTime AND @StartTime < EndTime) 
                    OR (@EndTime > StartTime AND @EndTime <= EndTime) 
                    OR (StartTime >= @StartTime AND EndTime <= @EndTime) -- Ensures full overlap
                )";

                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@RoomID", roomId);
                    checkCmd.Parameters.AddWithValue("@SelectedDate", selectedDate);
                    checkCmd.Parameters.AddWithValue("@StartTime", time1.TimeOfDay); // Use only time
                    checkCmd.Parameters.AddWithValue("@EndTime", time2.TimeOfDay);   // Use only time

                    int existingBookings = (int)checkCmd.ExecuteScalar();

                    if (existingBookings > 0)
                    {
                        MessageBox.Show("This room is already reserved for the selected date and time.", "Please choose another time", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ error condition END and start not over 30m

                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++ If pass error condition

                    if (existingBookings > 0)
                    {
                        MessageBox.Show("This room is already reserved for the selected date and time."," Please choose another time.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Insert new booking
                    string insertQuery = @"
                INSERT INTO tbl_MeetingSchedule (Person, MeetingRoom, MeetingTitle, MeetingDate, StartTime, EndTime, Department) 
                VALUES (@Person, @MeetingRoom, @MeetingTitle, @MeetingDate, @StartTime, @EndTime, @Department)";




                    SqlCommand insertCmd = new SqlCommand(insertQuery, con);
                    insertCmd.Parameters.AddWithValue("@Person", loggedInUser);
                    insertCmd.Parameters.AddWithValue("@MeetingRoom", roomId);
                    insertCmd.Parameters.AddWithValue("@MeetingTitle", txtMeeting.Text);
                    insertCmd.Parameters.AddWithValue("@MeetingDate", selectedDate);
                    //insertCmd.Parameters.AddWithValue("@StartTime", time1.ToString("HH:mm"));
                    //insertCmd.Parameters.AddWithValue("@EndTime", time2.ToString("HH:mm"));

                    insertCmd.Parameters.AddWithValue("@StartTime", time1); // Store only time
                    insertCmd.Parameters.AddWithValue("@EndTime", time2);   // Store only time
                    insertCmd.Parameters.AddWithValue("@Department", loggedInDepart);


                    insertCmd.ExecuteNonQuery();

                    MessageBox.Show("Reservation successfull!" , "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while processing the reservation: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            LoadData();
        }
        private void LoadRooms()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    // Load Room Data
                    string query = "SELECT DISTINCT Room FROM tbl_MeetingRoom";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Debugging: Check if data exists
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No rooms found in the database!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    cmbRoom.DataSource = dt;
                    cmbRoom.DisplayMember = "Room";
                    cmbRoom.ValueMember = "Room";

                    cmbRoom.SelectedIndex = -1; // Ensure nothing is pre-selected


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error on Room Selection: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void cmbRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbRoom.SelectedValue != null)
            {
                string selectedRoom = cmbRoom.SelectedValue.ToString(); // Use SelectedValue

                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    try
                    {
                        con.Open();
                        string query = "SELECT Img FROM tbl_MeetingRoom WHERE Room = @Room";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Room", selectedRoom);

                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                byte[] imgData = (byte[])result;
                                pictureBox1.Image = ByteArrayToImage(imgData); // Convert and display image
                                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom; // Adjust image display
                            }
                            else
                            {
                                //pictureBox1.Image = null; // No image found
                                //MessageBox.Show("No image found for this room.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading image: " + ex.Message);
                    }
                }
            }
            LoadData();
        }
        private Image ByteArrayToImage(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }
        private string selectedPerson = ""; // Store the booking creator
        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
        private void btnWithdraw_Click_1(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "New Meeting ➢ Withdraw";
            UC_R_WithDraw ug = new UC_R_WithDraw(loggedInUser, loggedInDepart);
            addControls(ug);
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
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "Admin > Reserve Meeting Room && Schedule";
            Form_Home.sharedButton.Visible = false;
            Form_Home.sharedButtonew.Visible = true;  //new
            Form_Home.sharedButtonBC.Visible = false;
            Form_Home.sharedButton2.Visible = true;   //withdra
            Form_Home.sharedButton3.Visible = true;  //replace
            UC_R_DetailsRoom ug = new UC_R_DetailsRoom(loggedInUser);
            addControls(ug);
        }

        private void cmbDepart_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
