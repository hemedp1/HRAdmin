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
    public partial class UC_C_Verify : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInIndex;
        private string loggedInDepart;
        public string car;
        public UC_C_Verify(string username, string index, string Depart)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInIndex = index;
            loggedInDepart = Depart;
            loadUserData();
            LoadCar();
            loadInspect();
            gbCarInspect();


        }
        private void LoadCar()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    string query = @"
                SELECT TOP 1 AssignedCar
                FROM tbl_CarBookings
                WHERE DriverName = @DriverName
                  AND Depart = @Depart
                  AND Status = 'Approved'
                  AND Acknowledgement IS NULL
                  AND CompleteUseStatus IS NULL
                  AND CAST(RequestDate AS DATE) = CAST(GETDATE() AS DATE)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@DriverName", loggedInUser);
                        cmd.Parameters.AddWithValue("@Depart", loggedInDepart);

                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            car = result.ToString();
                        }
                        else
                        {
                            //label1.Text = "No car assigned.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading Assigned Car: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnAcknowledge_Click(object sender, EventArgs e)
        {

            if (dataGridView2.CurrentRow == null)
            {
                MessageBox.Show("No row selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int rowIndex = dataGridView2.CurrentRow.Index;
            string IDStr = dataGridView2.Rows[rowIndex].Cells[0]?.Value?.ToString();
            string selectedPerson = dataGridView2.Rows[rowIndex].Cells[1]?.Value?.ToString();
            string car = dataGridView2.Rows[rowIndex].Cells[15]?.Value?.ToString();


            if (string.IsNullOrEmpty(selectedPerson) || string.IsNullOrEmpty(IDStr))
            {
                MessageBox.Show("Error retrieving reservation details. Please check column names.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(IDStr, out int meetingID))
            {
                MessageBox.Show("Invalid ID format.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult confirm = MessageBox.Show($"Do you agree to these terms and conditions?",
                                                   $"Verify Confirmation",
                                                   MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    string updateQuery;
                    
                    updateQuery = "UPDATE tbl_CarBookings SET Acknowledgement = 'Acknowledged' WHERE BookingID = @BookingID";
                    
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@BookingID", meetingID);
                        cmd.Parameters.AddWithValue("@loggedInUser", loggedInUser);
                        cmd.ExecuteNonQuery();
                    }

                    dataGridView2.Refresh();

                    string updateCarQuery = "UPDATE tbl_Cars SET Status = 'Not Available' WHERE CarPlate = @CarPlate";          // update car status
                    using (SqlCommand cmd = new SqlCommand(updateCarQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@CarPlate", car);
                        cmd.ExecuteNonQuery();
                    }

                }

                loadUserData();
                MessageBox.Show("Terms and condition successfully acknowledged.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while acknowledging the terms and condition: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadUserData()
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
                                        DateChecked,
                                        Status, 
                                        ApproveBy,
                                        DateApprove,
                                        Depart,
                                        CompleteUseStatus,
                                        CONVERT(VARCHAR(5), StartDate, 108) AS StartDate, 
                                        CONVERT(VARCHAR(5), EndDate, 108) AS EndDate 
                                    FROM tbl_CarBookings 
                                    WHERE DriverName = @DriverName AND Depart = @Depart AND CompleteUseStatus IS NULL AND Status = 'Approved' AND Acknowledgement IS NULL";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@DriverName", loggedInUser);
                        cmd.Parameters.AddWithValue("@Depart", loggedInDepart);
                        cmd.Parameters.AddWithValue("RequestDate", DateTime.Now.ToString("dd.MM.yyyy")); 

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        dataGridView2.Columns.Clear();
                        dataGridView2.AutoGenerateColumns = false;

                        // Enable scrolling
                        dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        dataGridView2.ScrollBars = ScrollBars.Both; // Enable both vertical & horizontal scrolling

                        dataGridView2.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                        {
                            Font = new Font("Arial", 11, FontStyle.Bold),
                        };

                        string[] columnNames = { "BookingID", "DriverName", "Depart", "IndexNo", "RequestDate", "Destination", "Purpose", "StartDate", "EndDate", "StatusCheck", "CheckBy", "DateChecked", "Status", "ApproveBy", "DateApprove", "AssignedCar" };

                        for (int i = 0; i < columnNames.Length; i++)
                        {
                            string col = columnNames[i];
                            string headerText;

                            if (col == "BookingID")
                                headerText = "Id";

                            else if (col == "DriverName")
                                headerText = "Driver";

                            else if (col == "Depart")
                                headerText = "Department";

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
                                headerText = "Checked By";

                            else if (col == "DateChecked")
                                headerText = "Checked Date";

                            else if (col == "ApproveBy")
                                headerText = "Approved By";

                            else if (col == "DateApprove")
                                headerText = "Approved Date";

                            else if (col == "AssignedCar")
                                headerText = "Car";
                            //else if (col == "CompleteUseStatus")
                            //    headerText = "Trip Status";
                            else
                                headerText = col.Replace("_", " "); // Default formatting

                            var column = new DataGridViewTextBoxColumn()
                            {
                                HeaderText = headerText,
                                DataPropertyName = col,
                                Width = 180,
                                //AutoSizeMode = i == columnNames.Length - 1  ? DataGridViewAutoSizeColumnMode.Fill : DataGridViewAutoSizeColumnMode.None,
                                DefaultCellStyle = new DataGridViewCellStyle
                                {
                                    ForeColor = Color.MidnightBlue,
                                    Font = new Font("Arial", 11)
                                }
                            };

                            dataGridView2.Columns.Add(column);
                        }
                        dataGridView2.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void loadInspect()//private void cmbCar_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (car != null)
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    try
                    {
                        con.Open();
                        string query = "SELECT ID, Person, DateInspect, Milleage, Brakes, Signal_light, Head_light, Body, Front_Bumper, Rear_Bumper, View_Mirror, Tyres, Others FROM tbl_CarInspection WHERE CarType = @Plat";

                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Plat", car);

                            DataTable dtt = new DataTable();
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(dtt);
                            }

                            dataGridView1.Columns.Clear();
                            dataGridView1.AutoGenerateColumns = false;


                            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                            dataGridView1.ScrollBars = ScrollBars.Both; // Enable both vertical & horizontal scrolling


                            dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                            {
                                Font = new Font("Arial", 11, FontStyle.Bold),
                            };

                            string[] columnNames = { "Person", "DateInspect", "Milleage", "Brakes", "Signal_light", "Head_light", "Body", "Front_Bumper", "Rear_Bumper", "View_Mirror", "Tyres" };

                            // Optional: custom display names for headers
                            Dictionary<string, string> headerMappings = new Dictionary<string, string>
                            {
                                { "DateInspect", "Date Inspect" }
                            };

                            foreach (var col in columnNames)
                            {
                                string headerText;

                                // Use custom header if available, otherwise replace underscores
                                if (headerMappings.ContainsKey(col))
                                    headerText = headerMappings[col];
                                else
                                    headerText = col.Replace("_", " ");

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


                            // Add "Others" column (fills remaining space)
                            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                            {
                                HeaderText = "Others",
                                DataPropertyName = "Others",
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, // This will take up remaining space
                                DefaultCellStyle = new DataGridViewCellStyle
                                {
                                    ForeColor = Color.MidnightBlue,
                                    Font = new Font("Arial", 11)
                                }
                            });

                            // Bind data
                            dataGridView1.DataSource = dtt;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading inspection log: " + ex.Message);
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
        private void btnBack_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "Admin > Car Reservation > Reservation";
            Form_Home.sharedbtn_verify.Visible = true;
            UC_C_BookingCar ug = new UC_C_BookingCar(loggedInUser, loggedInDepart, loggedInDepart);
            addControls(ug);
            //sharedButtonbtnWDcar.Visible = false;
            ///btnAddpeople.Visible = false;
            //btn_New.Visible = false;
            //btnBookCar.Visible = false;
            //button2.Visible = false;
            //button3.Visible = false;
            //btnInspect.Visible = false;
            //btn_Accident.Visible = false;
            //btnCarCondition.Visible = true;
            //label1.Text = "Admin > Car Reservation > Reservation";

            //sharedButtonbtnApp = btnApp;
            //sharedButtonbtnWDcar = btnWDcar;
            //sharedbuttonInspect = btnInspect;
            //sharedbtn_Accident = btn_Accident;
            //sharedbtn_AccidentPDF = btn_AccidentPDF;
            //sharedbtn_verify = btnCarCondition;
            //sharedbtnVisitor = btnVisitor;
            //sharedbtnWithdrawEntry = btnWithdrawEntry;
            //sharedbtnNewVisitor = btnNewVisitor;
            //sharedbtnUpdate = btnUpdate;
        }

        private void gbCarInspect()
        {
            MessageBox.Show($"Terms:{car}");
            groupBox3.Text = car + " Inspection Log"; // This updates the title of the GroupBox

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
    }
}
