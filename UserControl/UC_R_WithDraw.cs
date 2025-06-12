using HRAdmin.Forms;
using SLRDbConnector;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace HRAdmin.UserControl
{
    public partial class UC_R_WithDraw : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        public UC_R_WithDraw(string username, string depart)
        {
            InitializeComponent();
            loggedInUser = username; // Assign username to loggedInUser
            loggedInDepart = depart;
            LoadData();
            dTDay.ValueChanged += dTDay_ValueChanged;
            dataGridView1.CellClick += dataGridView1_CellContentClick;
        }
        private void UC_WithDraw_Load(object sender, EventArgs e)
        {
            LoadData();
            dTDay.ValueChanged += dTDay_ValueChanged;
            dataGridView1.CellClick += dataGridView1_CellContentClick;
        }
        private void dataGridView1_CellContentClick(object sender, EventArgs e) 
        {
        
        }
        private void btnWithdraw_Click(object sender, EventArgs e)
        {
            int rowIndex = dataGridView1.CurrentRow.Index;
            string meetingIDStr = dataGridView1.Rows[rowIndex].Cells[0]?.Value?.ToString();
            string selectedPerson = dataGridView1.Rows[rowIndex].Cells[1]?.Value?.ToString();

            if (string.IsNullOrEmpty(selectedPerson) || string.IsNullOrEmpty(meetingIDStr))
            {
                MessageBox.Show("Error retrieving meeting details. Please check column names.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int meetingID = Convert.ToInt32(meetingIDStr);

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
                        string deleteQuery = "DELETE FROM tbl_MeetingSchedule WHERE Person = @Person AND MeetingID = @MeetingID";

                        using (SqlCommand cmd = new SqlCommand(deleteQuery, con))
                        {
                            cmd.Parameters.AddWithValue("@Person", selectedPerson);
                            cmd.Parameters.AddWithValue("@MeetingID", meetingID);
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
        private void dTDay_ValueChanged(object sender, EventArgs e)
        {
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
                    SELECT MeetingID, Person, MeetingTitle, MeetingDate, MeetingRoom, Department, 
                           CONVERT(VARCHAR(5), StartTime, 108) AS StartTime, 
                           CONVERT(VARCHAR(5), EndTime, 108) AS EndTime 
                    FROM tbl_MeetingSchedule 
                    WHERE CONVERT(date, MeetingDate) = @SelectedDate 
                          AND (Department = '' OR Department = @Depart)";

                
                          

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@SelectedDate", dTDay.Value.Date);
                        cmd.Parameters.AddWithValue("@Depart", loggedInDepart);
                        //cmd.Parameters.AddWithValue("@Depart", string.IsNullOrEmpty(cmbDepart.Text) ? "" : cmbDepart.Text);

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        dataGridView1.AutoGenerateColumns = false;
                        dataGridView1.Columns.Clear();

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
                        /*
                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                        {
                            HeaderText = "Department",
                            DataPropertyName = "Department",
                            DefaultCellStyle = new DataGridViewCellStyle
                            {
                                ForeColor = Color.MidnightBlue,
                                Font = new Font("Arial", 11)
                            },
                            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                        }); */

                        dataGridView1.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
        private void UC_WithDraw_Load_1(object sender, EventArgs e)
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
            }
            else
            {
                MessageBox.Show("Panel not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            Form_Home.sharedButton.Visible = false;         //++++    Add People
            Form_Home.sharedButtonew.Visible = true;        //++++    New Meeting
            Form_Home.sharedButtonBC.Visible = false;       //++++    Book Car
            Form_Home.sharedButton2.Visible = true;         //++++    withdraw Meeting
            Form_Home.sharedButton3.Visible = true;         //++++    Replace Ownership Meeting
            Form_Home.sharedLabel.Text = "Admin > Reserve Meeting Room && Schedule";
            UC_R_DetailsRoom ug = new UC_R_DetailsRoom(loggedInUser);
            addControls(ug);
        }
    }
}
