using HRAdmin.Components;
using HRAdmin.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace HRAdmin.UserControl
{
    public partial class UC_A_Admin : System.Windows.Forms.UserControl
    {  //loggedInUser, loggedInDepart);
        private string loggedInUser;
        private string loggedInDepart;
        public static Panel sharedPanel;
        public static Panel sharedPanele;
        public static Button sharedBtnTest;
        private string EventDetails;
        private DateTime EventTime;

        public UC_A_Admin(string username, string Depart)
        {
            loggedInUser = username;
            loggedInDepart = Depart;
            //MessageBox.Show($"DDSDSDDWDWWD: {username}");
            InitializeComponent();
            
        }
        private void addControls(System.Windows.Forms.UserControl userControl)
        {
            if (Form_Home.sharedPanel != null && Form_Home.sharedLabel != null)
            {
                Form_Home.sharedPanel.Controls.Clear();
                userControl.Dock = DockStyle.Fill;
                Form_Home.sharedPanel.Controls.Add(userControl);
                userControl.BringToFront();
                // Update label text if needed
            }
            /*
             * else if (UC_MC_Issue.sharedPanele != null) 
            {
                
                UC_MC_Issue.sharedPanele.Controls.Clear();
                userControl.Dock = DockStyle.Fill;
                UC_MC_Issue.sharedPanele.Controls.Add(userControl);
                CheckUserAccess2(loggedInUser);
                userControl.BringToFront();
                
            } */
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
                    string query = "SELECT a.Username, a.Name1,a.AA, a.MA, a.Position, b.TitlePosition, b.AccessLevel\r\n\r\nFROM tbl_Users a\r\n\r\nLEFT JOIN tbl_UsersLevel b ON a.Position = b.TitlePosition WHERE a.Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())  // Use SqlDataReader
                        {
                            if (reader.Read()) 
                            {
                                string AA = reader["AA"].ToString();
                                string MA = reader["MA"].ToString();
                                int accessLevel = Convert.ToInt32(reader["AccessLevel"]);


                                if (AA == "1")
                                {
                                    Form_Home.sharedButtonbtnApp.Visible = true;
                                    Form_Home.sharedbuttonInspect.Visible = true;
                                    if (accessLevel > 0 && loggedInDepart == "HR & Admin")
                                    {
                                        Form_Home.sharedButton4.Visible = true;
                                        Form_Home.sharedButton5.Visible = true;
                                    }
                                    else
                                    {
                                        Form_Home.sharedButton4.Visible = false;
                                        Form_Home.sharedButton5.Visible = false;
                                    }
                                    Form_Home.sharedbtnVisitor.Visible = true;
                                    Form_Home.sharedbtnWithdrawEntry.Visible = true;
                                    Form_Home.sharedbtnNewVisitor.Visible = true;
                                    Form_Home.sharedbtnUpdate.Visible = true;
                                    Form_Home.sharedButtonbtnApp.Visible = true;
                                    Form_Home.sharedButtonBC.Visible = true;
                                    Form_Home.sharedButtonbtnWDcar.Visible = true;
                                    Form_Home.sharedbtn_Accident.Visible = true;
                                }
                                else if (MA == "2")
                                {
                                    Form_Home.sharedButtonbtnApp.Visible = true;
                                    Form_Home.sharedButton4.Visible = true;
                                    Form_Home.sharedButton5.Visible = true;
                                    Form_Home.sharedbtnVisitor.Visible = false;
                                    Form_Home.sharedbtnWithdrawEntry.Visible = false;
                                    Form_Home.sharedbtnNewVisitor.Visible = false;
                                    Form_Home.sharedbtnUpdate.Visible = false;
                                    Form_Home.sharedbtnMCReport.Visible = false; // Car Booking
                                }
                                else
                                {
                                    Form_Home.sharedButtonbtnApp.Visible = false;
                                    if (accessLevel > 0 && loggedInDepart == "HR & Admin")
                                    {
                                        Form_Home.sharedButton4.Visible = true;
                                        Form_Home.sharedButton5.Visible = true;
                                    }
                                    else
                                    {
                                        Form_Home.sharedButton4.Visible = false;
                                        Form_Home.sharedButton5.Visible = false;
                                    }
                                    Form_Home.sharedbtnVisitor.Visible = false;
                                    Form_Home.sharedbtnWithdrawEntry.Visible = false;
                                    Form_Home.sharedbtnNewVisitor.Visible = false;
                                    Form_Home.sharedbtnUpdate.Visible = false;
                                }
                            }
                            else
                            {
                                Form_Home.sharedButtonbtnApp.Visible = false;
                                Form_Home.sharedButton4.Visible = false;
                                Form_Home.sharedButton5.Visible = false;
                                Form_Home.sharedbtnVisitor.Visible = false;
                                Form_Home.sharedbtnWithdrawEntry.Visible = false;
                                Form_Home.sharedbtnNewVisitor.Visible = false;
                                Form_Home.sharedbtnUpdate.Visible = false;
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
        private void InspectAccess(string username)
        { }
        private void btnCarBooking_Click(object sender, EventArgs e)
        {
      
            CheckUserAccess(loggedInUser); 

            Form_Home.sharedLabel.Text = "Admin > Car Reservation";
            Form_Home.sharedButton.Visible = false;
            Form_Home.sharedButtonew.Visible = false;
            Form_Home.sharedButtonBC.Visible = true;
            Form_Home.sharedButtonbtnWDcar.Visible = true;
            Form_Home.sharedButton2.Visible = false;
            Form_Home.sharedButton3.Visible = false;
            Form_Home.sharedbtn_verify.Visible = false;
            Form_Home.sharedbtn_Accident.Visible = true;
            Form_Home.sharedButton4.Visible = false;
            Form_Home.sharedButton5.Visible = false;
            Form_Home.sharedButton6.Visible = false;
            Form_Home.sharedbtnVisitor.Visible = false;
            Form_Home.sharedbtnWithdrawEntry.Visible = false;
            Form_Home.sharedbtnNewVisitor.Visible = false;
            Form_Home.sharedbtnUpdate.Visible = false;

            UC_C_Car_Details_Booking ug = new UC_C_Car_Details_Booking(loggedInUser);
            addControls(ug);
        }


        private void btnMeeting_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "Admin > Reserve Meeting Room && Schedule";
            Form_Home.sharedButton.Visible = false;
            Form_Home.sharedButtonew.Visible = true;
            Form_Home.sharedButtonBC.Visible = false;
            Form_Home.sharedButton2.Visible = true;
            Form_Home.sharedButton3.Visible = true;
            Form_Home.sharedButton4.Visible = false;
            Form_Home.sharedButton5.Visible = false;
            Form_Home.sharedButton6.Visible = false;
            UC_R_DetailsRoom ug = new UC_R_DetailsRoom(loggedInUser);
            addControls(ug);
        }
    
        private void btnWB_Click(object sender, EventArgs e)
        {
            CheckUserAccess(loggedInUser);

            Form_Home.sharedLabel.Text = "Welcome Board";
            //Form_Home.sharedLabel.Text = "Administrator ➢ New Room";
            Form_Home.sharedButton.Visible = false;
            Form_Home.sharedButtonew.Visible = false;
            Form_Home.sharedButtonBC.Visible = false;
            Form_Home.sharedButton4.Visible = false;
            Form_Home.sharedButton5.Visible = false;
            Form_Home.sharedButtonbtnApp.Visible = false;
            Form_Home.sharedbuttonInspect.Visible = false;
            Form_Home.sharedbtn_Accident.Visible = false;
            Form_Home.sharedButtonbtnWDcar.Visible = false;
            UC_W_WelcomeBoard ug = new UC_W_WelcomeBoard(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }


        private void CheckUserAccess2(string username)
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
                                    //UC_MC_Issue.sharedPanele.Visible = false;
                                    //UC_MC_Issue.sharedbuttonInspect.Visible = true;
                                }
                                else if (MA == "2")
                                {
                                      // Button1 will be visible
                                    //UC_MC_Issue.sharedBtnTest.Visible = false;          // 3
                                    //UC_MC_Issue.sharedButtonbtnApp.Visible = true;
                                }
                                else
                                {
                                    //UC_MC_Issue.sharedButtonbtnApp.Visible = false;
                                }
                            }
                            else
                            {
                                //UC_MC_Issue.sharedButtonbtnApp.Visible = false;
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

        private void btnMeal_Click(object sender, EventArgs e)
        {
            CheckUserAccess(loggedInUser);

            Form_Home.sharedLabel.Text = "Admin > Meal Request";
            Form_Home.sharedButton.Visible = false;
            Form_Home.sharedButtonew.Visible = false;
            Form_Home.sharedButtonBC.Visible = false;
            Form_Home.sharedButton2.Visible = false;
            Form_Home.sharedButton3.Visible = false;
            //Form_Home.sharedButton6.Visible = true;
            Form_Home.sharedbtnVisitor.Visible = false;
            Form_Home.sharedbtnWithdrawEntry.Visible = false;
            Form_Home.sharedbtnNewVisitor.Visible = false;
            Form_Home.sharedbtnUpdate.Visible = false;
            Form_Home.sharedButtonbtnApp.Visible= false; 
            Form_Home.sharedbuttonInspect.Visible = false; 
            Form_Home.sharedbtn_Accident.Visible = false;
            Form_Home.sharedButtonbtnWDcar.Visible = false;
            UC_Meal_Food ug = new UC_Meal_Food(EventDetails, EventTime, loggedInUser, loggedInDepart);
            addControls(ug);
        }
    }
}
