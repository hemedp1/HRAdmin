using HRAdmin.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace HRAdmin.UserControl
{
    public partial class UC_C_Inspection : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        public UC_C_Inspection(string username)
        {
            InitializeComponent();
            loggedInUser = username;
            loadCar();
            LoadData();
        }
        private void loadCar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    // Load Room Data
                    string query = "SELECT DISTINCT CarPlate FROM tbl_Cars where Status = 'Not Available'";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Debugging: Check if data exists
                    if (dt.Rows.Count == 0)
                    {
                        //MessageBox.Show("No Car found in the database!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    cmbCar.DataSource = dt;
                    cmbCar.DisplayMember = "CarPlate";
                    cmbCar.ValueMember = "CarPlate";

                    cmbCar.SelectedIndex = -1; // Ensure nothing is pre-selected


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error on Room Selection: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            DateTime selectedDate = dTDay.Value.Date;
            if (string.IsNullOrWhiteSpace(cmbCar.Text))
            {
                MessageBox.Show("Please select car.", "Car Selection", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return; 
            }
            if(string.IsNullOrWhiteSpace(cmbIn.Text)) 
            {
                MessageBox.Show("Please select time.", "Time Selection", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if(string.IsNullOrWhiteSpace(txtMilleage.Text))
            {
                MessageBox.Show("Please input mileage.", "Input Mileage", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if(!double.TryParse(txtMilleage.Text, out _))
            {
                MessageBox.Show("Please enter a valid mileage.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if(string.IsNullOrWhiteSpace(cmbBrakes.Text)) 
            {
                MessageBox.Show("Please select brakes condition.", "Brakes Condition", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if(string.IsNullOrWhiteSpace(cmbSignal_light.Text)) 
            {
                MessageBox.Show("Please select signal condition.", "Signal Condition", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if(string.IsNullOrWhiteSpace(cmbHead_light.Text)) 
            {
                MessageBox.Show("Please select head light condition.", "Head Light Condition", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if(string.IsNullOrWhiteSpace(cmbBody.Text)) 
            {
                MessageBox.Show("Please select body condition.", "Body Condition", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if(string.IsNullOrWhiteSpace(cmbFront_Bumper.Text)) 
            {
                MessageBox.Show("Please select front bumper condition.", "Front Bumper Condition", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if(string.IsNullOrWhiteSpace(cmbRear_Bumper.Text)) 
            {
                MessageBox.Show("Please select rear rumper condition.", "Rear Bumper Condition", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if(string.IsNullOrWhiteSpace(cmbView_Mirror.Text)) 
            {
                MessageBox.Show("Please select view mirror condition.", "View Mirror Condition", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if(string.IsNullOrWhiteSpace(cmbTyres.Text)) 
            {
                MessageBox.Show("Please select tyres condition.", "Tyres Condition", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Check if the car meets the condition where it should NOT be updated
                    string checkQuery = @"SELECT COUNT(*) 
                              FROM tbl_CarBookings cb
                              JOIN tbl_Cars c ON cb.AssignedCar = c.CarPlate
                              WHERE 
                                  CAST(cb.RequestDate AS DATE) >= CAST(GETDATE() AS DATE)  
                                  AND CAST(GETDATE() AS TIME) <= CAST(cb.EndDate AS TIME) 
                                  AND c.Status = 'Not Available'
                                  AND cb.AssignedCar = @CarPlate";  // Filter by the selected car

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@CarPlate", cmbCar.Text);
                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("This car is still in an active booking and inspection cannot be done.",
                                            "Update Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            return;  // Exit the function to prevent the update
                        }
                    }
                    DialogResult confirm = MessageBox.Show("Are you sure you want to confirm this inspection record?", "Inspection Record Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirm == DialogResult.Yes) 
                    {
                        // Proceed with inserting inspection data
                        string insQuery = @"INSERT INTO tbl_CarInspection(Person, CarType, DateInspect, Actual_TimeIN, Milleage, Brakes, Signal_light, Head_light, Body, Front_Bumper, Rear_Bumper, View_Mirror, Tyres, Others) 
                           VALUES (@Person, @CarType, @DateInspect, @Actual_TimeIN, @Milleage, @Brakes, @Signal_light, @Head_light, @Body, @Front_Bumper, @Rear_Bumper, @View_Mirror, @Tyres, @Others)";

                        using (SqlCommand Insertcmd = new SqlCommand(insQuery, con))
                        {
                            Insertcmd.Parameters.AddWithValue("@Person", loggedInUser);
                            Insertcmd.Parameters.AddWithValue("@CarType", cmbCar.Text);
                            Insertcmd.Parameters.AddWithValue("@DateInspect", dTDay.Value.Date);
                            Insertcmd.Parameters.AddWithValue("@Actual_TimeIN", cmbIn.Text);
                            Insertcmd.Parameters.AddWithValue("@Milleage", txtMilleage.Text);
                            Insertcmd.Parameters.AddWithValue("@Brakes", cmbBrakes.Text);
                            Insertcmd.Parameters.AddWithValue("@Signal_light", cmbSignal_light.Text);
                            Insertcmd.Parameters.AddWithValue("@Head_light", cmbHead_light.Text);
                            Insertcmd.Parameters.AddWithValue("@Body", cmbBody.Text);
                            Insertcmd.Parameters.AddWithValue("@Front_Bumper", cmbFront_Bumper.Text);
                            Insertcmd.Parameters.AddWithValue("@Rear_Bumper", cmbRear_Bumper.Text);
                            Insertcmd.Parameters.AddWithValue("@View_Mirror", cmbView_Mirror.Text);
                            Insertcmd.Parameters.AddWithValue("@Tyres", cmbTyres.Text);
                            Insertcmd.Parameters.AddWithValue("@Others", txtRemarks.Text);

                            Insertcmd.ExecuteNonQuery();
                        }

                        // Update Use Status to 'Complete'
                        string updateStatusUse = "UPDATE tbl_CarBookings SET CompleteUseStatus = 'Complete' WHERE AssignedCar = @CarPlate";

                        using (SqlCommand StatusupdateCmd = new SqlCommand(updateStatusUse, con))
                        {
                            StatusupdateCmd.Parameters.AddWithValue("@CarPlate", cmbCar.Text);
                            StatusupdateCmd.ExecuteNonQuery();
                        }



                        // Update Car Status to 'Available' only if the car is not still booked
                        string updateCarQuery = "UPDATE tbl_Cars SET Status = 'Available' WHERE CarPlate = @CarPlate";

                        using (SqlCommand updateCmd = new SqlCommand(updateCarQuery, con))
                        {
                            updateCmd.Parameters.AddWithValue("@CarPlate", cmbCar.Text);
                            updateCmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Car inspection record successfully updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        loadCar();
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while processing the inspection: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            LoadData();
            cmbCar.SelectedIndex = -1;
            cmbBody.SelectedIndex = -1;
            cmbBrakes.SelectedIndex = -1;
            cmbCar.SelectedIndex = -1;
            cmbFront_Bumper.SelectedIndex = -1;
            cmbHead_light.SelectedIndex = -1;
            cmbIn.SelectedIndex = -1;
            cmbRear_Bumper.SelectedIndex = -1;
            cmbSignal_light.SelectedIndex = -1;
            cmbView_Mirror.SelectedIndex = -1;
            cmbTyres.SelectedIndex = -1;
            txtMilleage.Clear();
            txtRemarks.Clear();
            //cmbCar.Text = - 1;cmb
        }
        private void LoadData()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = @"
            SELECT ID, Person, CarType, DateInspect, Actual_TimeIN, Milleage, Brakes, Signal_light, Head_light, Body, 
                   Front_Bumper, Rear_Bumper, View_Mirror, Tyres, Others
            FROM tbl_CarInspection 
            WHERE CONVERT(date, DateInspect) = @SelectedDate";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@SelectedDate", SqlDbType.Date).Value = dTDay.Value.Date;

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        dataGridView1.Columns.Clear();
                        dataGridView1.AutoGenerateColumns = false;

                       
                        DataGridViewCellStyle commonStyle = new DataGridViewCellStyle
                        {
                            ForeColor = Color.MidnightBlue,
                            Font = new Font("Arial", 11)
                        };

                        dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                        {
                            Font = new Font("Arial", 11, FontStyle.Bold)
                        };

                        string[] columnNames = { "ID", "Person", "CarType", "DateInspect", "Actual_TimeIN", "Milleage", "Brakes",
                                         "Signal_light", "Head_light", "Body", "Front_Bumper", "Rear_Bumper",
                                         "View_Mirror", "Tyres", "Others" };
                        string[] headers = { "ID", "Person", "Car", "Date", "Time", "Milleage", "Brakes",
                                     "Signal light", "Head light", "Body", "Front Bumper", "Rear Bumper",
                                     "View Mirror", "Tyres", "Others" };

                        for (int i = 0; i < columnNames.Length; i++)
                        {
                            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                            {
                                HeaderText = headers[i],
                                DataPropertyName = columnNames[i],
                                DefaultCellStyle = commonStyle,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.None, // Keep all columns fixed size
                                Width = i == 0 ? 80 : 120 // ID column (index 0) is 80px, others are 120px

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
    }

}
