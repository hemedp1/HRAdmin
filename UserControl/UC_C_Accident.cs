using HRAdmin.Components;
using HRAdmin.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Description;
using System.Windows.Forms;
using System.Xml.Linq;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace HRAdmin.UserControl
{

    public partial class UC_C_Accident : System.Windows.Forms.UserControl
    {
        public static GroupBox Grp6;
        private string loggedInUser;
        private string loggedInIndex;
        private string loggedInDepart;
        private string loggedInName;
        public UC_C_Accident(string username, string Index, string Depart, string Name)
        {
            InitializeComponent();
            this.Load += UC_C_Accident_Load;
            loggedInUser = username;
            loggedInIndex = Index;
            loggedInDepart = Depart;
            loggedInName = Name;
            
            groupBox2.Visible = false;
            Grp6 = groupBox6;
            AutoScroll = true;

            LoadDepartments();
            dTDay.ValueChanged += dTDay_ValueChanged;
           // cmnRepID.SelectedIndexChanged += cmnRepID_SelectedIndexChanged;
            CheckUserAccess1(loggedInUser);
            loadCars();
            
        }
        private void UC_C_Accident_Load(object sender, EventArgs e)
        {
            dtp_filter.ShowCheckBox = true;
            dtp_filter.Checked = false;
            loadListNotApp();
            dTDay.ValueChanged += dTDay_ValueChanged;
        }
        private void rB_Rej_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Visible = false;
        }
        private void rB_App_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Visible = true;
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
        private void CheckUserAccess1(string username)    // Access Car accident
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = "SELECT AA, MA, AC FROM tbl_Users WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string AA = reader["AA"].ToString();
                                string MA = reader["MA"].ToString();
                                string AC = reader["AC"].ToString();

                                if (AA == "1")
                                {
                                    Form_Home.sharedLabel.Text = "Admin > Car Reservation > Accident";
                                    Form_Home.sharedButton.Visible = false;
                                    Form_Home.sharedButtonew.Visible = false;
                                    Form_Home.sharedButton2.Visible = false;   // withdraw
                                    Form_Home.sharedButton3.Visible = false;   // replace
                                    Form_Home.sharedButtonBC.Visible = false;
                                    Form_Home.sharedButtonbtnApp.Visible = false;
                                    Form_Home.sharedButtonbtnWDcar.Visible = false;
                                    Form_Home.sharedbuttonInspect.Visible = false;
                                    Form_Home.sharedbtn_Accident.Visible = false;
                                    groupBox6.Visible = true;
                                    submit.Visible = false;
                                    cmbCar.Visible = false;
                                    cmbTimeRep.Visible = false;
                                    dtRep.Visible = false;
                                    //btnChecked.Visible = false;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);

                                    MakeUserSectionReadOnly(groupBox1);
                                    MakeUserSectionReadOnly(groupBox2);
                                    MakeUserSectionReadOnly(groupBox3);
                                    MakeUserSectionReadOnly(groupBox4);
                                    MakeUserSectionReadOnly(groupBox5);
                                    rB_App.Enabled = false;
                                    rB_Rej.Enabled = false;
                                    cmbCar.Enabled = false;
                                    txtCarr.ReadOnly = true;
                                    txtDes.ReadOnly = true; 
                                    txtIndex.ReadOnly = true;
                                    txtHEMdriver.ReadOnly = true;
                                    txtDepart.ReadOnly = true;


                                }
                                else if (AC == "11" || AC == "12")
                                {
                                    Form_Home.sharedLabel.Text = "Admin > Car Reservation > Accident";
                                    Form_Home.sharedButton.Visible = false;
                                    Form_Home.sharedButtonew.Visible = false;
                                    Form_Home.sharedButton2.Visible = false;   // withdraw
                                    Form_Home.sharedButton3.Visible = false;   // replace
                                    Form_Home.sharedButtonBC.Visible = false;
                                    Form_Home.sharedButtonbtnApp.Visible = false;
                                    Form_Home.sharedButtonbtnWDcar.Visible = false;
                                    Form_Home.sharedbuttonInspect.Visible = false;
                                    Form_Home.sharedbtn_Accident.Visible = false;
                                    groupBox6.Visible = true;
                                    submit.Visible = false;
                                    cmbCar.Visible = false;
                                    cmbTimeRep.Visible = false;
                                    dtRep.Visible = false;
                                    //btnApp_Admin.Visible = false;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);

                                    MakeUserSectionReadOnly(groupBox1);
                                    MakeUserSectionReadOnly(groupBox2);
                                    MakeUserSectionReadOnly(groupBox3);
                                    MakeUserSectionReadOnly(groupBox4);
                                    MakeUserSectionReadOnly(groupBox5);
                                }
                                else if (MA == "2")
                                {
                                    Form_Home.sharedLabel.Text = "Admin > Car Reservation > Accident";
                                    Form_Home.sharedButton.Visible = false;
                                    Form_Home.sharedButtonew.Visible = false;
                                    Form_Home.sharedButton2.Visible = false;   // withdraw
                                    Form_Home.sharedButton3.Visible = false;   // replace
                                    Form_Home.sharedbuttonInspect.Visible = false;
                                    Form_Home.sharedButtonbtnApp.Visible = false;
                                    Form_Home.sharedButtonBC.Visible = false;
                                    Form_Home.sharedButtonbtnWDcar.Visible = false;
                                    Form_Home.sharedbtn_Accident.Visible = false;
                                    groupBox6.Visible = false;
                                    txtCarr.Visible = false;
                                    txtrepTime.Visible = false;
                                    txtRepdatee.Visible = false;
                                    label21.Visible = false;
                                    label1.Visible = false;
                                    label5.Visible = false;
                                    label23.Visible = false;
                                    label24.Visible = false;
                                    label8.Visible = false;
                                    label2.Visible = false;
                                    label22.Visible = false;
                                    txtIndex.Visible = false;
                                    txtHEMdriver.Visible = false;
                                    txtDepart.Visible = false;
                                    cmnRepID.Visible = false;
                                    rB_App.Location = new Point(rB_App.Location.X, 78);
                                    rB_Rej.Location = new Point(rB_Rej.Location.X, 110);
                                    //rB_App.Enabled = true;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);

                                   
                                }
                                else
                                {
                                    Form_Home.sharedLabel.Text = "Admin > Car Reservation > Accident";
                                    Form_Home.sharedButton.Visible = false;
                                    Form_Home.sharedButtonew.Visible = false;
                                    Form_Home.sharedButton2.Visible = false;   // withdraw
                                    Form_Home.sharedButton3.Visible = false;   // replace
                                    Form_Home.sharedbuttonInspect.Visible = false;
                                    Form_Home.sharedButtonbtnApp.Visible = false;
                                    Form_Home.sharedButtonBC.Visible = false;
                                    Form_Home.sharedButtonbtnWDcar.Visible = false;
                                    Form_Home.sharedbtn_Accident.Visible = false;
                                    groupBox6.Visible = false;
                                    txtCarr.Visible = false;
                                    txtrepTime.Visible = false;
                                    txtRepdatee.Visible = false;
                                    label21.Visible = false;
                                    label1.Visible = false;
                                    label5.Visible = false;
                                    txtIndex.Visible = false;
                                    txtHEMdriver.Visible = false;
                                    txtDepart.Visible = false;
                                    label23.Visible = false;
                                    label24.Visible = false;
                                    label8.Visible = false;
                                    label2.Visible = false;
                                    label22.Visible = false;
                                    cmnRepID.Visible = false;
                                    rB_App.Location = new Point(rB_App.Location.X, 80);
                                    rB_Rej.Location = new Point(rB_Rej.Location.X, 110);
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);
                                }
                            }
                            else
                            {
                                groupBox6.Visible = false;
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
        private void CheckUserAccess(string username)     //USE AT BACK BUTTON
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = "SELECT AA, MA, AC FROM tbl_Users WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string AA = reader["AA"].ToString();
                                string MA = reader["MA"].ToString();
                                string AC = reader["AC"].ToString();

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
                                    Form_Home.sharedbtn_AccidentPDF.Visible = false;
                                    groupBox6.Visible = true;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);
                                }
                                else if (AC == "11" || AC == "12")
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
                                    Form_Home.sharedbtn_AccidentPDF.Visible = false;
                                    groupBox6.Visible = true;
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
                                    Form_Home.sharedbuttonInspect.Visible = false;
                                    Form_Home.sharedButtonbtnApp.Visible = true;
                                    Form_Home.sharedButtonBC.Visible = true;
                                    Form_Home.sharedButtonbtnWDcar.Visible = true;
                                    Form_Home.sharedbtn_Accident.Visible = true;
                                    Form_Home.sharedbtn_AccidentPDF.Visible = false;
                                    groupBox6.Visible = false;
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
                                    Form_Home.sharedbtn_AccidentPDF.Visible = false;
                                    groupBox6.Visible = false;
                                    UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
                                    addControls(ug);
                                }
                            }
                            else
                            {
                                groupBox6.Visible = false;
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
        private void loadCars()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    // Load Room Data
                    string query = "SELECT DISTINCT CarPlate FROM tbl_Cars where Status = 'Available'";
                    SqlDataAdapter da = new SqlDataAdapter(query, con);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Debugging: Check if data exists
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No Car found in the database!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        private void DateReportAcc()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    string query = "SELECT DISTINCT ReportID FROM tbl_AccidentCar WHERE CAST(DateReport AS DATE) = @date";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@date", dTDay.Value.Date);

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Insert placeholder row at the top
                        DataRow newRow = dt.NewRow();
                        newRow["ReportID"] = "    -- Select Report ID --";
                        dt.Rows.InsertAt(newRow, 0);

                        cmnRepID.DataSource = dt;
                        cmnRepID.DisplayMember = "ReportID";
                        cmnRepID.ValueMember = "ReportID";
                        cmnRepID.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error on Report ID Selection: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        ///MessageBox.Show($"DDSDSDDWDWWD: {time1}"); 
        private void submit_Click(object sender, EventArgs e)
        {
            DateTime selectedDate = dTDay.Value.Date;
            DateTime dateRep = dtRep.Value.Date;                        //+++++++++++++                                              Driver Details
            if (string.IsNullOrWhiteSpace(cmbCar.Text))
            {
                MessageBox.Show("Please select car", "Car", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDes.Text))
            {
                MessageBox.Show("Please input your destination", "Purpose", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (rB_App.Checked)
            {
                if (string.IsNullOrWhiteSpace(txtDriverName.Text))                          //+++++++++++++                                              Driver Details
                {
                    MessageBox.Show("Please input the driver name", "Driver Name", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtNoKeretaInvolve.Text) || !double.TryParse(txtNoKeretaInvolve.Text, out _))
                {
                    MessageBox.Show("Please enter a valid number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtRegistrationNo.Text))
                {
                    MessageBox.Show("Please input the registration no", "No plat", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtTypVehicle.Text))
                {
                    MessageBox.Show("Please input vehicle type", "Vehicle", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtIC.Text) || !double.TryParse(txtIC.Text, out _))
                {
                    MessageBox.Show("Please input IC driver without unique character", "IC", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtTel.Text) || !double.TryParse(txtTel.Text, out _))
                {
                    MessageBox.Show("Please enter a valid no tel without unique character", "Telephone No", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtAddress.Text))
                {
                    MessageBox.Show("Please input driver address", "Address", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }
            }
            if (string.IsNullOrWhiteSpace(txtPlaceRep.Text))
            {
                MessageBox.Show("Please input location of the accident occurred", "Place of Accident", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (string.IsNullOrWhiteSpace(cmbTimeRep.Text))
            {
                MessageBox.Show("Please input time of the accident occurred", "Time of Accident", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtExplanantion.Text))                          //+++++++++++++                                              PM
            {
                MessageBox.Show("Please input your explanation how accident occurred", "Explanation", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtPM.Text))                          //+++++++++++++                                              PM
            {
                MessageBox.Show("Please input your preventive measure", "Preventive Measure", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }
            if (selectedDate < dateRep)
            {
                MessageBox.Show("The report date cannot be before the accident date.", "Date Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string checkQuery = @"
                    SELECT COUNT(*) FROM tbl_AccidentCar 
                    WHERE CAST(DateReport AS DATE) = @SelectedDate
                    AND DriverInternal = @DrivName AND IndexNo = @IndexNo AND Dept = @Dept";

                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    
                    checkCmd.Parameters.AddWithValue("@SelectedDate", selectedDate);
                    checkCmd.Parameters.AddWithValue("@DrivName", loggedInUser);
                    checkCmd.Parameters.AddWithValue("@IndexNo", loggedInIndex);
                    checkCmd.Parameters.AddWithValue("@Dept", loggedInDepart);

                    int existingBookings = (int)checkCmd.ExecuteScalar();

                    if (existingBookings > 0)  //////////////////////////////////////////////////////+++++++++++++          Update record
                    {   
                        string checkQuery1 = @"SELECT CheckStatus FROM tbl_AccidentCar 
                                            WHERE CAST(DateReport AS DATE) = @SelectedDate
                                            AND DriverInternal = @DrivName AND IndexNo = @IndexNo AND Dept = @Dept";

                        using (SqlCommand checkCmd1 = new SqlCommand(checkQuery1, con))
                        {
                            checkCmd1.Parameters.AddWithValue("@SelectedDate", selectedDate);
                            checkCmd1.Parameters.AddWithValue("@DrivName", loggedInUser);
                            checkCmd1.Parameters.AddWithValue("@IndexNo", loggedInIndex);
                            checkCmd1.Parameters.AddWithValue("@Dept", loggedInDepart);
                            object result = checkCmd1.ExecuteScalar();
                            string statusCheck = result?.ToString();
                            MessageBox.Show($"statusCheck: {statusCheck}");

                            if ( statusCheck == "Checked")
                            {
                                MessageBox.Show("Cannot modify. Record already verified by admin.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                        }
                        DialogResult confirm = MessageBox.Show("Are you sure? you want to edit the current record?", "Edit Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (confirm == DialogResult.Yes)
                        {
                            string updateQuery = @"UPDATE tbl_AccidentCar SET DateReport = @DateReport, DriverInternal = @DriverInternal, IndexNo = @IndexNo,
                                                Dept = @Dept, Car = @Car, Destination = @Destination, DriverExternal = @DriverExternal, NoofVehicle = @NoofVehicle,
                                                PlatNo = @PlatNo, VehicleType = @VehicleType, InsuranceClass = @InsuranceClass, InsuranceComp = @InsuranceComp,
                                            IC = @IC,Tel = @Tel, PolicyNo = @PolicyNo, Address = @Address, DateofAccident = @DateofAccident, Place = @Place,
                                            Time = @Time, PM = @PM, PoliceStation = @PoliceStation, ReportNo = @ReportNo, Explanation = @Explanation
                                            WHERE ReportID = @ReportID";

                            SqlCommand updateCmd = new SqlCommand(updateQuery, con);
                            updateCmd.Parameters.AddWithValue("@DateReport", selectedDate);
                            updateCmd.Parameters.AddWithValue("@DriverInternal", loggedInUser);
                            updateCmd.Parameters.AddWithValue("@IndexNo", loggedInIndex);
                            updateCmd.Parameters.AddWithValue("@Dept", loggedInDepart);
                            updateCmd.Parameters.AddWithValue("@Car", cmbCar.Text);
                            updateCmd.Parameters.AddWithValue("@Destination", txtDes.Text);
                            updateCmd.Parameters.AddWithValue("@DriverExternal", txtDriverName.Text);
                            updateCmd.Parameters.AddWithValue("@NoofVehicle", txtNoKeretaInvolve.Text);
                            updateCmd.Parameters.AddWithValue("@PlatNo", txtRegistrationNo.Text);
                            updateCmd.Parameters.AddWithValue("@VehicleType", txtTypVehicle.Text);
                            updateCmd.Parameters.AddWithValue("@InsuranceClass", txtInsurantClass.Text);
                            updateCmd.Parameters.AddWithValue("@InsuranceComp", txtInsurance.Text);
                            updateCmd.Parameters.AddWithValue("@IC", txtIC.Text);
                            updateCmd.Parameters.AddWithValue("@Tel", txtTel.Text);
                            updateCmd.Parameters.AddWithValue("@PolicyNo", txtPolicyNo.Text);
                            updateCmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                            updateCmd.Parameters.AddWithValue("@DateofAccident", dateRep);
                            updateCmd.Parameters.AddWithValue("@Place", txtPlaceRep.Text);
                            updateCmd.Parameters.AddWithValue("@Time", cmbTimeRep.Text);
                            updateCmd.Parameters.AddWithValue("@PM", txtPM.Text);
                            updateCmd.Parameters.AddWithValue("@PoliceStation", txtPolis.Text);
                            updateCmd.Parameters.AddWithValue("@ReportNo", txtRepNo.Text);
                            updateCmd.Parameters.AddWithValue("@ReportID", loggedInUser + "_" + loggedInIndex + "_" + selectedDate.ToString("yyyy-MM-dd"));
                            updateCmd.Parameters.AddWithValue("@Explanation", txtExplanantion.Text);

                            updateCmd.ExecuteNonQuery();

                            MessageBox.Show("Record successfully modified"); /////////
                        }

                        return;

                    }

                    string insertQuery = @"
                    INSERT INTO tbl_AccidentCar (DateReport, DriverInternal, IndexNo, Dept, Car, Destination, DriverExternal, NoofVehicle, PlatNo, VehicleType, InsuranceClass, InsuranceComp, IC, Tel, PolicyNo, Address, DateofAccident, Place, Time, PM, PoliceStation, ReportNo, ReportID, Explanation, CheckStatus, ApproveStatus) " +
                               "VALUES (@DateReport, @DriverInternal, @IndexNo, @Dept, @Car, @Destination, @DriverExternal, @NoofVehicle, @PlatNo, @VehicleType, @InsuranceClass, @InsuranceComp, @IC, @Tel, @PolicyNo, @Address, @DateofAccident, @Place, @Time, @PM, @PoliceStation, @ReportNo, @ReportID, @Explanation, 'Pending', 'Pending')";


                    //08/04/2025  WO25002925 @Tel, @PolicyNo, @Address, @DateofAccident, @Place, @Time, @DestinationRep, @PlatRep, @PM, @PoliceStation, @ReportNo)";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, con);
                    insertCmd.Parameters.AddWithValue("@DateReport", selectedDate);
                    insertCmd.Parameters.AddWithValue("@DriverInternal", loggedInUser);
                    insertCmd.Parameters.AddWithValue("@IndexNo", loggedInIndex);
                    insertCmd.Parameters.AddWithValue("@Dept", loggedInDepart);
                    insertCmd.Parameters.AddWithValue("@Car", cmbCar.Text);
                    insertCmd.Parameters.AddWithValue("@Destination", txtDes.Text);
                    insertCmd.Parameters.AddWithValue("@DriverExternal", txtDriverName.Text);
                    insertCmd.Parameters.AddWithValue("@NoofVehicle", txtNoKeretaInvolve.Text);
                    insertCmd.Parameters.AddWithValue("@PlatNo", txtRegistrationNo.Text);
                    insertCmd.Parameters.AddWithValue("@VehicleType", txtTypVehicle.Text);
                    insertCmd.Parameters.AddWithValue("@InsuranceClass", txtInsurantClass.Text);
                    insertCmd.Parameters.AddWithValue("@InsuranceComp", txtInsurance.Text);
                    insertCmd.Parameters.AddWithValue("@IC", txtIC.Text);
                    insertCmd.Parameters.AddWithValue("@Tel", txtTel.Text);
                    insertCmd.Parameters.AddWithValue("@PolicyNo", txtPolicyNo.Text);
                    insertCmd.Parameters.AddWithValue("@Address", txtAddress.Text);
                    insertCmd.Parameters.AddWithValue("@DateofAccident", dateRep);
                    insertCmd.Parameters.AddWithValue("@Place", txtPlaceRep.Text);
                    insertCmd.Parameters.AddWithValue("@Time", cmbTimeRep.Text);
                    insertCmd.Parameters.AddWithValue("@PM", txtPM.Text);
                    insertCmd.Parameters.AddWithValue("@PoliceStation", txtPolis.Text);
                    insertCmd.Parameters.AddWithValue("@ReportNo", txtRepNo.Text);
                    insertCmd.Parameters.AddWithValue("@ReportID", loggedInUser + "_" + loggedInIndex + "_" + selectedDate.ToString("yyyy-MM-dd"));
                    insertCmd.Parameters.AddWithValue("@Explanation", txtExplanantion.Text);

                    insertCmd.ExecuteNonQuery();

                    MessageBox.Show("Report details successfully submitted!", "Report Submmitted");

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("QAn error occurred while submitting the report: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadUserDataForDate()
        {
            if (cmnRepID.Text == "    -- Select Report ID --")
            {
                //MessageBox.Show("Please select a valid Report ID before proceeding.", "Missing Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {


                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
            SELECT TOP 1 * 
            FROM tbl_AccidentCar 
            WHERE CAST(DateReport AS DATE) = @Date 
            AND ReportID = @ReportID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@Date", SqlDbType.Date).Value = dTDay.Value.Date;
                        cmd.Parameters.Add("@ReportID", SqlDbType.VarChar).Value = cmnRepID.Text;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate fields...
                                txtHEMdriver.Text = reader["DriverInternal"]?.ToString();
                                txtDepart.Text = reader["Dept"]?.ToString();
                                txtIndex.Text = reader["IndexNo"]?.ToString();
                                txtCarr.Text = reader["Car"]?.ToString();
                                txtDes.Text = reader["Destination"]?.ToString();
                                txtDriverName.Text = reader["DriverExternal"]?.ToString();
                                txtNoKeretaInvolve.Text = reader["NoofVehicle"]?.ToString();
                                txtRegistrationNo.Text = reader["PlatNo"]?.ToString();
                                txtTypVehicle.Text = reader["VehicleType"]?.ToString();
                                txtInsurantClass.Text = reader["InsuranceClass"]?.ToString();
                                txtIC.Text = reader["IC"]?.ToString();
                                txtTel.Text = reader["Tel"]?.ToString();
                                txtInsurance.Text = reader["InsuranceComp"]?.ToString();
                                txtPolicyNo.Text = reader["PolicyNo"]?.ToString();
                                txtAddress.Text = reader["Address"]?.ToString();

                                if (DateTime.TryParse(reader["DateofAccident"]?.ToString(), out DateTime accidentDate))
                                {
                                    txtRepdatee.Text = accidentDate.ToString("dd/MM/yyyy");
                                }
                                else
                                {
                                    txtRepdatee.Text = string.Empty;
                                }

                                txtPlaceRep.Text = reader["Place"]?.ToString();
                                txtrepTime.Text = reader["Time"]?.ToString();
                                txtPM.Text = reader["PM"]?.ToString();
                                txtPolis.Text = reader["PoliceStation"]?.ToString();
                                txtRepNo.Text = reader["ReportNo"]?.ToString();
                                txtExplanantion.Text = reader["Explanation"]?.ToString();
                                txtRemarksAdmin.Text = reader["Remarks"]?.ToString();
                                reader.Close();
                                string query1 = "SELECT AA, MA, AC FROM tbl_Users WHERE Username = @Username";
                                using (SqlCommand cmd1 = new SqlCommand(query1, con))
                                {
                                    cmd1.Parameters.AddWithValue("@Username", loggedInUser);

                                    using (SqlDataReader reader1 = cmd1.ExecuteReader())
                                    {
                                        if (reader1.Read())
                                        {
                                            string AA = reader1["AA"].ToString();
                                            string MA = reader1["MA"].ToString();
                                            string AC = reader1["AC"].ToString();

                                            if (AA == "1")
                                            {
                                                //if (!string.IsNullOrWhiteSpace(txtRemarksAdmin.Text))
                                                //{
                                                    //MakeUserSectionReadOnly(groupBox6);
                                                    //btnApp_Admin.Enabled = true;
                                                //}
                                                //else
                                                //{
                                                    //MakeUserSectionReadOnly(groupBox6);
                                                    btnApp_Admin.Enabled = true;
                                                //}
                                                
                                            }
                                            else if (AC == "11")
                                            {
                                                //txtRemarksAdmin.Enabled = true;
                                            }
                                            else if (MA == "2")
                                            {
                                                   

                                            }

                                            else
                                            {
                                               
                                            }
                                        }
                                        else
                                        {

                                        }
                                    }
                                }
                            }
                            else
                            {
                                //MessageBox.Show("No entries found for the selected date and report.");
                            
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading datap: " + ex.Message);
            }

        }
        private void LoadEditForUser() 
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"SELECT TOP 1 * FROM tbl_AccidentCar WHERE CAST(DateReport AS DATE) = @Date AND DriverInternal = @DriverInternal AND IndexNo = @IndexNo";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@Date", SqlDbType.Date).Value = dTDay.Value.Date;
                        cmd.Parameters.Add("@DriverInternal", SqlDbType.VarChar).Value = loggedInUser;
                        cmd.Parameters.Add("@IndexNo", SqlDbType.VarChar).Value = loggedInIndex;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtHEMdriver.Text = reader["DriverInternal"]?.ToString();
                                txtDepart.Text = reader["Dept"]?.ToString();
                                txtIndex.Text = reader["IndexNo"]?.ToString();
                                cmbCar.Text = reader["Car"]?.ToString();
                                txtDes.Text = reader["Destination"]?.ToString();

                                txtDriverName.Text = reader["DriverExternal"]?.ToString();

                                if (!string.IsNullOrWhiteSpace(txtDriverName.Text))
                                {
                                    rB_App.Checked = true;
                                }

                                txtNoKeretaInvolve.Text = reader["NoofVehicle"]?.ToString();
                                txtRegistrationNo.Text = reader["PlatNo"]?.ToString();
                                txtTypVehicle.Text = reader["VehicleType"]?.ToString();
                                txtInsurantClass.Text = reader["InsuranceClass"]?.ToString();
                                txtIC.Text = reader["IC"]?.ToString();
                                txtTel.Text = reader["Tel"]?.ToString();
                                txtInsurance.Text = reader["InsuranceComp"]?.ToString();
                                txtPolicyNo.Text = reader["PolicyNo"]?.ToString();
                                txtAddress.Text = reader["Address"]?.ToString();

                                if (DateTime.TryParse(reader["DateofAccident"]?.ToString(), out DateTime accidentDate))
                                {
                                    txtRepdatee.Text = accidentDate.ToString("dd/MM/yyyy");
                                }
                                else
                                {
                                    txtRepdatee.Text = string.Empty;
                                }

                                txtPlaceRep.Text = reader["Place"]?.ToString();
                                txtrepTime.Text = reader["Time"]?.ToString();        // Setting time to a textbox
                                txtPM.Text = reader["PM"]?.ToString();
                                txtPolis.Text = reader["PoliceStation"]?.ToString();
                                txtRepNo.Text = reader["ReportNo"]?.ToString();
                                txtExplanantion.Text = reader["Explanation"]?.ToString();
                                string timeValue = reader["Time"]?.ToString()?.Trim();

                                if (!string.IsNullOrEmpty(timeValue))
                                {
                                    // Ensure same format (e.g., leading zero)
                                    if (timeValue.Length == 4) timeValue = "0" + timeValue; // "8:00" -> "08:00"

                                    if (cmbTimeRep.Items.Contains(timeValue))
                                    {
                                        cmbTimeRep.Text = timeValue;
                                    }
                                    else
                                    {
                                        cmbTimeRep.Text = timeValue;
                                        MessageBox.Show("Value not in list: " + timeValue);
                                    }
                                }

                            }
                            else
                            {
                                //MessageBox.Show("No entries found for the selected date and report.");

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading data: " + ex.Message);
            }

        }
        private void MakeUserSectionReadOnly(GroupBox groupBox)
        {
            foreach (Control ctrl in groupBox.Controls)
            {
                if (ctrl is Label)
                {
                    continue; // Skip labels
                }
                else if (ctrl is TextBox tb)
                {
               
                    tb.ReadOnly = true;
                }
                else if (ctrl is RadioButton rb)
                {
                    rb.Enabled = false;
                }
                else if (ctrl is ComboBox comboBox)
                {
                    comboBox.Enabled = false;
                }
                else if (ctrl is Panel panel && panel.Name == "panel3")
                {
                    foreach (Control child in panel.Controls)
                    {
                        if (child is Label)
                        {
                            continue; // Skip labels inside panel3
                        }
                        else if (child is TextBox tb2)
                        {
                            tb2.ReadOnly = true;
                        }
                        else if (child is ComboBox cb2)
                        {
                            cb2.Enabled = false;
                        }
                        else if (child is RadioButton rb2)
                        {
                            rb2.Enabled = false;
                        }
                        else
                        {
                            child.Enabled = false;
                        }
                    }
                }
            }

            // Keep these controls active
            rB_App.Checked = true;
            cmnRepID.Enabled = true;
        }
        private void cmnRepID_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadUserDataForDate();
        }
        private void dTDay_ValueChanged(object sender, EventArgs e)
        {
            
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = "SELECT AA, MA, AC FROM tbl_Users WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", loggedInUser);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string AA = reader["AA"].ToString();
                                string MA = reader["MA"].ToString();
                                string AC = reader["AC"].ToString();

                                if (AA == "1")
                                {
                                    DateReportAcc();
                                }
                                else if (AC == "11" || AC == "12")
                                {
                                    DateReportAcc();
                                }
                                else if (MA == "2")
                                {
                                    LoadEditForUser();

                                }

                                else
                                {
                                    LoadEditForUser();
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
        private void subadmin_Click(object sender, EventArgs e)
        {

        }
        private void btnAttachemnt_Click(object sender, EventArgs e)
        {

        }
        private void btnChecked_Click(object sender, EventArgs e)
        {
            string selectedDate = dTDay.Value.Date.ToString("yyyy-MM-dd");

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                con.Open();

                string checkQuery = @"SELECT CheckStatus FROM tbl_AccidentCar WHERE CAST(DateReport AS DATE) = @Date 
                                    AND ReportID = @ReportID";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.Add("@Date", SqlDbType.Date).Value = selectedDate;
                    checkCmd.Parameters.Add("@ReportID", SqlDbType.VarChar).Value = cmnRepID.Text.Trim();
                    object statusresult = checkCmd.ExecuteScalar();
                    string statusCheck = statusresult?.ToString();
                    //MessageBox.Show($"statusresult : {statusCheck}");
                    if (statusCheck == "Checked")
                    {
                        MessageBox.Show("This report has already been checked.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (statusCheck == "Rejected")
                    {
                        MessageBox.Show("This accident record has already been rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string checkQuery1 = @"SELECT AC FROM tbl_Users WHERE Department = @Depart AND Username = @Username";

                using (SqlCommand checkCmd1 = new SqlCommand(checkQuery1, con))
                {
                    checkCmd1.Parameters.Add("@Username", SqlDbType.VarChar).Value = UserSession.LoggedInUser;
                    checkCmd1.Parameters.Add("@Depart", SqlDbType.VarChar).Value = UserSession.loggedInDepart;
                    object AC = checkCmd1.ExecuteScalar();

                    string AClvl = AC?.ToString();

                    if (AClvl == "12")
                    {
                        MessageBox.Show("This action is not available for approvers.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                DialogResult result = MessageBox.Show("Are you sure you want to verify this accident record?", "Verify Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string query = @"UPDATE tbl_AccidentCar 
                                    SET 
                                        DateCheck = @DateCheck, 
                                        CheckStatus = 'Checked', 
                                        CheckedBy = @CheckedBy, 
                                        Remarks = @Remarks,
                                        CheckedByDepartment = @CheckedByDepartment
                                    WHERE 
                                        CAST(DateReport AS DATE) = @Date 
                                        AND ReportID = @ReportID";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@CheckedBy", loggedInUser);
                        cmd.Parameters.Add("@DateCheck", SqlDbType.Date).Value = selectedDate;
                        cmd.Parameters.Add("@Date", SqlDbType.Date).Value = selectedDate;
                        cmd.Parameters.AddWithValue("@CheckedByDepartment", loggedInDepart);
                        cmd.Parameters.Add("@ReportID", SqlDbType.VarChar).Value = cmnRepID.Text.Trim();
                        cmd.Parameters.Add("@Remarks", SqlDbType.VarChar).Value = txtRemarksAdmin.Text.Trim();


                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Accident record verified successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            
        }
        private void btnApp_Admin_Click(object sender, EventArgs e)
        {
            string selectedDate = dTDay.Value.Date.ToString("yyyy-MM-dd");
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();

                    string checkQuery1 = @"SELECT AC FROM tbl_Users WHERE Department = @Depart AND Username = @Username";

                    using (SqlCommand checkCmd1 = new SqlCommand(checkQuery1, con))
                    {
                        checkCmd1.Parameters.Add("@Username", SqlDbType.VarChar).Value = UserSession.LoggedInUser;
                        checkCmd1.Parameters.Add("@Depart", SqlDbType.VarChar).Value = UserSession.loggedInDepart;
                        object AC = checkCmd1.ExecuteScalar();

                        string AClvl = AC?.ToString();

                        if (AClvl == "11")
                        {
                            MessageBox.Show("This action is not available for checkers.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string checkQuery = @"SELECT CheckStatus, ApproveStatus FROM tbl_AccidentCar WHERE CAST(DateReport AS DATE) = @Date 
                                        AND ReportID = @ReportID";

                    string statusCheck = null;
                    string approveStatus = null;

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.Add("@Date", SqlDbType.Date).Value = selectedDate;
                        checkCmd.Parameters.Add("@ReportID", SqlDbType.VarChar).Value = cmnRepID.Text.Trim();

                        using (SqlDataReader reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                statusCheck = reader["CheckStatus"]?.ToString();
                                approveStatus = reader["ApproveStatus"]?.ToString();
                            }
                        }
                    }

                    if (statusCheck == "Pending")
                    {
                        MessageBox.Show("Cannot proceed. Status Check must be 'Checked'.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (statusCheck == "Rejected")
                    {
                        MessageBox.Show("This accident record has already been rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (approveStatus == "Rejected")
                    {
                        MessageBox.Show("This accident record has already been rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (approveStatus == "Approved")
                    {
                        MessageBox.Show("This report has already been Approved.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                   

                    DialogResult result = MessageBox.Show("Are you sure you want to verify this accident record?", "Verify Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        {
                          
                            string query = @"UPDATE tbl_AccidentCar 
                                   SET 
                                      DateApprove = @DateApprove, 
                                      ApproveStatus = 'Approved', 
                                      ApproveBy = @ApproveBy, 
                                      Remarks = @Remarks,
                                      ApproveByDepartment = @ApproveByDepartment
                                   WHERE 
                                      CAST(DateReport AS DATE) = @Date 
                                      AND ReportID = @ReportID";

                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                

                                cmd.Parameters.AddWithValue("@ApproveBy", loggedInUser);
                                cmd.Parameters.Add("@DateApprove", SqlDbType.Date).Value = selectedDate;
                                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = selectedDate;
                                cmd.Parameters.AddWithValue("@ApproveByDepartment", loggedInDepart);
                                cmd.Parameters.Add("@ReportID", SqlDbType.VarChar).Value = cmnRepID.Text.Trim();
                                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar).Value = txtRemarksAdmin.Text.Trim();


                                cmd.ExecuteNonQuery();

                                MessageBox.Show("Accident record approved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while approving the record: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private void panel6_Paint(object sender, PaintEventArgs e)
        {

        }
        private void LoadDepartments()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                string query = "SELECT DISTINCT Dept FROM tbl_AccidentCar";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);


                // Insert placeholder row at the top
                DataRow newRow = dt.NewRow();
                newRow["Dept"] = "-- Select Department --";
                dt.Rows.InsertAt(newRow, 0);

                
                cmbdep.DisplayMember = "Dept";
                cmbdep.ValueMember = "Dept";
                cmbdep.DataSource = dt;
                cmbdep.SelectedIndex = 0;
            }
        }
        private void cmbdep_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadListNotApp();
            if (cmbdep.SelectedIndex >= 1)
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    string query = "SELECT DISTINCT DriverInternal FROM tbl_AccidentCar WHERE Dept = @Dept";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Dept", cmbdep.SelectedValue.ToString());

                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Insert placeholder row at the top
                        DataRow newRow = dt.NewRow();
                        newRow["DriverInternal"] = "    -- Select Driver --";
                        dt.Rows.InsertAt(newRow, 0);

                        cmbdriver.DataSource = dt;
                        cmbdriver.DisplayMember = "DriverInternal";
                        cmbdriver.ValueMember = "DriverInternal";
                        cmbdriver.SelectedIndex = 0;
                    }
                }
            }
        }
        private void loadListNotApp()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();

                    string query = @"
                SELECT 
                    DateReport,
                    DriverInternal,
                    IndexNo,
                    Dept,
                    Car,
                    CheckStatus,
                    ISNULL(CheckedBy, 'Pending') AS CheckedBy,
                    ISNULL(CONVERT(varchar, DateCheck, 23), 'Pending') AS DateCheck,
                    ApproveStatus,
                    ISNULL(ApproveBy, 'Pending') AS ApproveBy,
                    ISNULL(CONVERT(varchar, DateApprove, 23), 'Pending') AS DateApprove
                FROM tbl_AccidentCar 
                WHERE ApproveStatus = 'Pending'";

                    List<SqlParameter> parameters = new List<SqlParameter>();

                    if (dtp_filter.Checked)
                    {
                        query += " AND DateReport = @DateReport";
                        parameters.Add(new SqlParameter("@DateReport", dtp_filter.Value.ToString("yyyy/MM/dd")));
                    }

                    if (cmbdep.SelectedValue != null && cmbdep.SelectedValue.ToString().Trim() != "-- Select Department --")
                    {
                        query += " AND Dept = @Dept";
                        parameters.Add(new SqlParameter("@Dept", cmbdep.SelectedValue.ToString()));
                    }

                    if (cmbdriver.SelectedValue != null && cmbdriver.SelectedValue.ToString().Trim() != "-- Select Driver --")
                    {
                        query += " AND DriverInternal = @DriverInternal";
                        parameters.Add(new SqlParameter("@DriverInternal", cmbdriver.SelectedValue.ToString()));
                    }
            


                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.Add(param);
                        }

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        dataGridView1.Columns.Clear();
                        dataGridView1.AutoGenerateColumns = false;
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        dataGridView1.ScrollBars = ScrollBars.Both;

                        dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                        {
                            Font = new Font("Arial", 11, FontStyle.Bold),
                        };

                        string[] columnNames = { "DateReport", "DriverInternal", "IndexNo", "Dept", "Car", "CheckStatus", "CheckedBy", "DateCheck", "ApproveStatus", "ApproveBy", "DateApprove" };

                        foreach (var col in columnNames)
                        {
                            string headerText;
                            switch (col)
                            {
                                case "DateReport":
                                    headerText = "Date Report";
                                    break;
                                case "DriverInternal":
                                    headerText = "Driver";
                                    break;
                                case "IndexNo":
                                    headerText = "Index No";
                                    break;
                                case "Dept":
                                    headerText = "Department";
                                    break;
                                case "Car":
                                    headerText = "Car";
                                    break;
                                case "CheckStatus":
                                    headerText = "Admin Status Check";
                                    break;
                                case "CheckedBy":
                                    headerText = "Checked By";
                                    break;
                                case "DateCheck":
                                    headerText = "Checked Date";
                                    break;
                                case "ApproveStatus":
                                    headerText = "Admin HOD Status Check";
                                    break;
                                case "ApproveBy":
                                    headerText = "Approve By";
                                    break;
                                case "DateApprove":
                                    headerText = "Approve Date";
                                    break;
                                default:
                                    headerText = col.Replace("_", " ");
                                    break;
                            }

                            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                            {
                                HeaderText = headerText,
                                DataPropertyName = col,
                                Width = 170,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                                SortMode = DataGridViewColumnSortMode.Automatic,
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
                    MessageBox.Show("Error loading accident reports: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void cmbdriver_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadListNotApp(); // Refresh when driver changes
        }
        private void dtp_filter_ValueChanged(object sender, EventArgs e)
        {
            loadListNotApp();
        }
        private void button1_Click(object sender, EventArgs e)
        {

            loadListNotApp();
            cmbdep.SelectedIndex = 0;
            cmbdriver.DataSource = null;
            dtp_filter.Checked = false;
        }
        private void btnRej_Admin_Click(object sender, EventArgs e)
        {
            string selectedDate = dTDay.Value.Date.ToString("yyyy-MM-dd");

            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                con.Open();

                string checkQuery = @"SELECT CheckStatus, ApproveStatus 
                              FROM tbl_AccidentCar 
                              WHERE CAST(DateReport AS DATE) = @Date 
                              AND ReportID = @ReportID";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.Add("@Date", SqlDbType.Date).Value = selectedDate;
                    checkCmd.Parameters.Add("@ReportID", SqlDbType.VarChar).Value = cmnRepID.Text.Trim();

                    string statusCheck = null;
                    string approveStatus = null;

                    using (SqlDataReader reader = checkCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            statusCheck = reader["CheckStatus"]?.ToString();
                            approveStatus = reader["ApproveStatus"]?.ToString();
                        }
                    } // ✅ reader closed here

                    if (statusCheck == null) return; // no record found

                    // approve : pen → rej
                    if (statusCheck == "Checked" && approveStatus == "Pending")
                    {
                        if (MessageBox.Show("Are you sure you want to reject this accident record?",
                            "Verify Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            string query = @"UPDATE tbl_AccidentCar 
                                     SET DateApprove = @DateApprove, 
                                         ApproveStatus = 'Rejected', 
                                         ApproveBy = @ApproveBy, 
                                         Remarks = @Remarks,
                                         ApproveByDepartment = @ApproveByDepartment
                                     WHERE CAST(DateReport AS DATE) = @Date 
                                       AND ReportID = @ReportID";

                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.Add("@DateApprove", SqlDbType.Date).Value = selectedDate;
                                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = selectedDate;
                                cmd.Parameters.Add("@ReportID", SqlDbType.VarChar).Value = cmnRepID.Text.Trim();
                                cmd.Parameters.Add("@ApproveBy", SqlDbType.VarChar).Value = loggedInUser;
                                cmd.Parameters.Add("@ApproveByDepartment", SqlDbType.VarChar).Value = loggedInDepart;
                                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar).Value = txtRemarksAdmin.Text.Trim();

                                cmd.ExecuteNonQuery();
                                MessageBox.Show("Accident record rejected successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    // chck : pen → rej
                    else if (statusCheck == "Pending" && approveStatus == "Pending")
                    {
                        if (MessageBox.Show("Are you sure you want to reject this accident record?",
                            "Verify Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            string query = @"UPDATE tbl_AccidentCar 
                                     SET DateCheck = @DateCheck, 
                                         CheckStatus = 'Rejected', 
                                         CheckedBy = @CheckedBy, 
                                         Remarks = @Remarks,
                                         CheckedByDepartment = @CheckedByDepartment
                                     WHERE CAST(DateReport AS DATE) = @Date 
                                       AND ReportID = @ReportID";

                            using (SqlCommand cmd = new SqlCommand(query, con))
                            {
                                cmd.Parameters.Add("@DateCheck", SqlDbType.Date).Value = selectedDate;
                                cmd.Parameters.Add("@Date", SqlDbType.Date).Value = selectedDate;
                                cmd.Parameters.Add("@ReportID", SqlDbType.VarChar).Value = cmnRepID.Text.Trim();
                                cmd.Parameters.Add("@CheckedBy", SqlDbType.VarChar).Value = loggedInUser;
                                cmd.Parameters.Add("@CheckedByDepartment", SqlDbType.VarChar).Value = loggedInDepart;
                                cmd.Parameters.Add("@Remarks", SqlDbType.VarChar).Value = txtRemarksAdmin.Text.Trim();

                                cmd.ExecuteNonQuery();
                                MessageBox.Show("Accident record rejected successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    // prevent reject again
                    else if ((statusCheck == "Rejected" && approveStatus == "Pending") ||
                             (statusCheck == "Checked" && approveStatus == "Rejected"))
                    {
                        MessageBox.Show("This accident record has already been rejected.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (statusCheck == "Checked" && approveStatus == "Approved") 
                    {
                        MessageBox.Show("This accident record has already been approved.", "Action Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }


    }
}
