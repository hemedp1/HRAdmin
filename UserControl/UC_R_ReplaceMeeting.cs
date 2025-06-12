using HRAdmin.Forms;
using SLRDbConnector;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace HRAdmin.UserControl
{
    public partial class UC_R_ReplaceMeeting : System.Windows.Forms.UserControl
    {
        DbConnector db;
        private string loggedInUser;
        public UC_R_ReplaceMeeting(string username)
        {
            InitializeComponent();
            loggedInUser = username;
            db = new DbConnector();
            dTDay.ValueChanged += dTDay_ValueChanged;
            LoadData();
            loadUsers();
            cmbDepart.SelectedIndexChanged += cmbDepart_SelectedIndexChanged;
        }
        private void UC_ReplaceMeeting_Load(object sender, EventArgs e)
        {
            cmbDepart.SelectedIndexChanged += cmbDepart_SelectedIndexChanged;
            //dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView1_CellFormatting);
            //cmbRoom.SelectedIndexChanged += cmbRoom_SelectedIndexChanged;
            //LoadRooms();
            loadUsers();
            LoadData();
            dTDay.ValueChanged += dTDay_ValueChanged;
            //dataGridView1.CellClick += dataGridView1_CellClick;

        }
        private void loadUsers()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    // Load Room Data
                    string query = "SELECT DISTINCT Username FROM tbl_Users";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Debugging: Check if data exists
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No rooms found in the database!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    cmbAssign.DataSource = dt;
                    cmbAssign.DisplayMember = "Username";
                    cmbAssign.ValueMember = "Username";

                    cmbAssign.SelectedIndex = -1; // Ensure nothing is pre-selected


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error on Room Selection: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    SELECT MeetingID, Person, MeetingTitle, MeetingDate, MeetingRoom, 
                           CONVERT(VARCHAR(5), StartTime, 108) AS StartTime, 
                           CONVERT(VARCHAR(5), EndTime, 108) AS EndTime 
                    FROM tbl_MeetingSchedule 
                    WHERE CONVERT(date, MeetingDate) = @SelectedDate";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SelectedDate", dTDay.Value.Date);
                        //cmd.Parameters.AddWithValue("@BILIK", string.IsNullOrEmpty(cmbRoom.Text) ? "" : cmbRoom.Text);




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
                            Width = 340,

                        });

                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "Room",
                            DataPropertyName = "MeetingRoom",
                            Width = 240,
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
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                ForeColor = Color.MidnightBlue, // Correct syntax
                                Font = new Font("Arial", 11)
                            },
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        });

                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "Start Time",
                            DataPropertyName = "StartTime",
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
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                Format = "HH:mm",
                                ForeColor = Color.MidnightBlue,
                                Font = new Font("Arial", 11)
                            },
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        });

                        //string columnNames = "Columns in DataTable:\n";
                        //foreach (DataColumn col in dt.Columns)
                        //{
                        //    columnNames += col.ColumnName + "\n";
                        //}
                        //MessageBox.Show(columnNames, "Debug: DataTable Columns");

                        // Bind the data
                        dataGridView1.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string depart = cmbDepart.Text.Trim();
            //MessageBox.Show($"roomId: {depart}");
            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select your department.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dataGridView1.CurrentRow == null)
            {
                MessageBox.Show("Please select a row from the booking list.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rowIndex = dataGridView1.CurrentRow.Index;
            string meetingIDStr = dataGridView1.Rows[rowIndex].Cells[0]?.Value?.ToString();
            string selectedPerson = dataGridView1.Rows[rowIndex].Cells[1]?.Value?.ToString();
            
            //MessageBox.Show($"DDSDSDDWDWWD: {meetingIDStr}");
            if (string.IsNullOrEmpty(selectedPerson) || string.IsNullOrEmpty(meetingIDStr))
            {
                MessageBox.Show("Error retrieving meeting details. Please check column names.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int meetingID = Convert.ToInt32(meetingIDStr);

            //MessageBox.Show($"Person: {selectedPerson}, Meeting ID: {meetingID}");

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                con.Open();

                string checkQuery = "SELECT COUNT(*) FROM tbl_Users WHERE Department = @depart AND Username = @NewPerson";


                SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                checkCmd.Parameters.AddWithValue("@depart", cmbDepart.Text);
                checkCmd.Parameters.AddWithValue("@NewPerson", cmbAssign.Text);

                int existingBookings = (int)checkCmd.ExecuteScalar();
                //MessageBox.Show($"roomId: {existingBookings}");
                if (existingBookings < 1)
                {
                    MessageBox.Show("The combination of user and section entered does not exist", "Confirm the new owner’s details", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }
            }

            if (selectedPerson != loggedInUser)
            {
                MessageBox.Show("You can only replace your own reservation.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }




            DialogResult confirm = MessageBox.Show("Are you sure you want to change the ownership of this reservation?", "Confirm replace", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                    {
                        con.Open();
                        /*
                        string checkQuery = "SELECT COUNT(*) FROM tbl_Users WHERE Department = @depart AND Username = @NewPerson";


                        SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                        checkCmd.Parameters.AddWithValue("@depart", cmbDepart.Text);
                        checkCmd.Parameters.AddWithValue("@NewPerson", cmbAssign.Text);

                        int existingBookings = (int)checkCmd.ExecuteScalar();

                        if (existingBookings < 0)
                        {
                            MessageBox.Show("This ", "Please verify with the requester.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                           
                            return;
                        }
                        */
                        string deleteQuery = "UPDATE tbl_MeetingSchedule SET Person = @NewPerson, MeetingTitle = @NewTitle, Department = @depart WHERE MeetingID = @MeetingID AND Person = @OldPerson";

                        using (SqlCommand cmd = new SqlCommand(deleteQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@OldPerson", selectedPerson);
                            cmd.Parameters.AddWithValue("@NewPerson", cmbAssign.SelectedValue?.ToString()); // New assigned user
                            cmd.Parameters.AddWithValue("@NewTitle", txtMeeting.Text); // New assigned user
                            cmd.Parameters.AddWithValue("@MeetingID", meetingID);
                            cmd.Parameters.AddWithValue("@depart", cmbDepart.Text.Trim());


                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Ownership successfully replaced", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData(); // Refresh DataGridView
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error replacing reservation: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dTDay_ValueChanged(object sender, EventArgs e)
        {
            LoadData();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void cmbAssign_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

       
        private void addControls(System.Windows.Forms.UserControl userControl)
        {
            if (Form_Home.sharedPanel != null && Form_Home.sharedLabel != null) // Ensure panel4 exists
            {
                Form_Home.sharedPanel.Controls.Clear();
                userControl.Dock = DockStyle.Fill;
                Form_Home.sharedPanel.Controls.Add(userControl);
                userControl.BringToFront();
                // Update label text



            }
            else
            {
                MessageBox.Show("Panel not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            Form_Home.sharedButton.Visible = false;
            Form_Home.sharedButtonew.Visible = true;  //new
            Form_Home.sharedButtonBC.Visible = false;
            Form_Home.sharedButton2.Visible = true;   //withdra
            Form_Home.sharedButton3.Visible = true;  //replace
            Form_Home.sharedLabel.Text = "Admin > Reserve Meeting Room && Schedule";
            UC_R_DetailsRoom ug = new UC_R_DetailsRoom(loggedInUser);
            addControls(ug);
        }

        private void cmbDepart_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
