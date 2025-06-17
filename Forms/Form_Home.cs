using HRAdmin.UserControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace HRAdmin.Forms
{
    public partial class Form_Home : Form
    {
        private UC_W_WelcomeBoard currentWelcomeBoard;
        private string loggedInIndex;
        private string loggedInUser;
        private string loggedInDepart;
        private int borderSize = 2;
        public static Panel sharedPanel;
        public static Label sharedLabel;
        public static Button sharedButton;
        public static Button sharedButtonew;
        public static Button sharedButtonBC;
        public static Button sharedButton2;
        public static Button sharedButton3;
        public static Button sharedButtonbtnApp;
        public static Button sharedButtonbtnWDcar;
        public static Button sharedbuttonInspect;
        public static Button sharedbtn_Accident;
        public static Button sharedbtn_AccidentPDF;
        public static Button sharedbtn_verify;
        public static Button sharedButton4; //external
        public static Button sharedButton5; //internal
        public static Button sharedButton6; //view report
        public static Button sharedbtnVisitor;
        public static Button sharedbtnWithdrawEntry;
        public static Button sharedbtnNewVisitor;
        public static Button sharedbtnUpdate;


        public Form_Home(string username, string depart, string index)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = depart;
            loggedInIndex = index;
            lblUsername.Text = $"Hi, {loggedInUser}!"; // Display username
            Menu.Dock = DockStyle.Right;
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;   //withdra
            button3.Visible = false;  //replace
            btnApp.Visible = false;
            btnWDcar.Visible = false;
            btnInspect.Visible = false;
            btn_Accident.Visible = false;
            btn_AccidentPDF.Visible = false; 
            btnCarCondition.Visible = false;
            button4.Visible = false; //external
            button5.Visible = false; //internal
            button6.Visible = false; //view report
            btnVisitor.Visible = false; //visitor
            btnWithdrawEntry.Visible = false; //withdraw entry
            btnNewVisitor.Visible = false; //new visitor
            btnUpdate.Visible = false; //update
            //CollapseMenu();
            this.Padding = new Padding(borderSize);//Border size
            sharedPanel = panel5;  // Assign shared panel
            sharedLabel = label1; // Assign shared label
            sharedButton = btnAddpeople;
            sharedButtonew = btn_New;
            sharedButtonBC = btnBookCar;
            sharedButton2 = button2;
            sharedButton3 = button3;
            sharedButton4 = button4;  //external
            sharedButton5 = button5;  //internal
            sharedButton6 = button6;  //view report
            sharedButtonbtnApp = btnApp;
            sharedButtonbtnWDcar = btnWDcar;
            sharedbuttonInspect = btnInspect;
            sharedbtn_Accident = btn_Accident;
            sharedbtn_AccidentPDF = btn_AccidentPDF;
            sharedbtn_verify = btnCarCondition;
            sharedbtnVisitor = btnVisitor;
            sharedbtnWithdrawEntry = btnWithdrawEntry;
            sharedbtnNewVisitor = btnNewVisitor;
            sharedbtnUpdate = btnUpdate;

        }
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();
        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void Form_Home_Load(object sender, EventArgs e)
        {
            
        }
        private void btn_New_Click(object sender, EventArgs e)
        {
            label1.Text = "Admin > Reserve Meeting Room && Schedule > New Meeting";
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            

            UC_R_detail_booking ug = new UC_R_detail_booking(loggedInUser, loggedInDepart);
            addControls(ug);
        }
        private void CollapseMenu()
        {
            if (this.panel2.Width > 140) //Collapse menu curr 211
            {
                panel2.Width = 55;
                button1.Width = 55;
                //button2.Width = 55;
                //button3.Width = 55;
                btnAdmin.Width = 55;
                //label1.Visible = false;
                Menu.Dock = DockStyle.Fill;
                foreach (Button menuButton in panel2.Controls.OfType<Button>())
                {
                    menuButton.Text = "";
                    menuButton.ImageAlign = ContentAlignment.MiddleCenter;
                    menuButton.Padding = new Padding(0);
                }
            }
            else
            { //Expand menu
                panel2.Width = 150;
                button1.Width = 150;
                //button2.Width = 150;
                //button3.Width = 150;
                btnAdmin.Width = 150;
                //label1.Visible = true;
                Menu.Dock = DockStyle.Right;
                //InitializeComponent();
                foreach (Button menuButton in panel2.Controls.OfType<Button>())
                {
                    menuButton.Text = "   " + menuButton.Tag.ToString();
                    menuButton.ImageAlign = ContentAlignment.MiddleLeft;
                    menuButton.Padding = new Padding(10, 0, 0, 0);
                }
            }

            this.PerformLayout();
        }
        private void Menu_Click(object sender, EventArgs e)
        {
            label1.Text = "Hosiden Electronic (M) Sdn. Bhd.";

            CollapseMenu();
        }
        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
        private void addControls(System.Windows.Forms.UserControl userControl)
        {
            panel5.Controls.Clear();
            userControl.Dock = DockStyle.Fill;
            panel5.Controls.Add(userControl);
            userControl.BringToFront();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "HR && Admin";
            btn_New.Visible = false;
            btnAddpeople.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;   //withdra
            button3.Visible = false;  //replace
            btnWDcar.Visible = false;
            btnApp.Visible = false;   
            btn_AccidentPDF.Visible = false;
            btnInspect.Visible = false;
            btn_Accident.Visible = false;
            button4.Visible = false; //external
            button5.Visible = false; //internal
            button6.Visible = false; //view report
            btnVisitor.Visible = false; //visitor
            btnWithdrawEntry.Visible = false; //withdraw entry
            btnNewVisitor.Visible = false; //new visitor
            btnUpdate.Visible = false; //update
            UC_A_Admin ug = new UC_A_Admin(loggedInUser, loggedInDepart);
            addControls(ug);
        }
        private void panel5_Paint(object sender, PaintEventArgs e)
        {

        }
        private void button2_Click(object sender, EventArgs e)
        {
            //label1.Text = "Car Availability && Booking";
            label1.Text = "Hosiden Electronic(M) Sdn.Bhd.";
            btnBookCar.Visible = false;
            btn_New.Visible = false;
            btnAddpeople.Visible = false;

            //UC_W_WelcomeBoard ug = new UC_W_WelcomeBoard();
            //addControls(ug);
            //UC_CarDetails ug = new UC_CarDetails();
            //addControls(ug);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //label1.Text = "Welcome Board";
            label1.Text = "Hosiden Electronic(M) Sdn.Bhd.";
            btnBookCar.Visible = false;
            btn_New.Visible = false;
            btnAddpeople.Visible = false;

            //UC_W_WelcomeBoard ug = new UC_W_WelcomeBoard();
            //addControls(ug);
        }
        private void btnAdmin_Click(object sender, EventArgs e)
        {
            label1.Text = "Hosiden Electronic(M) Sdn.Bhd.";
            btnBookCar.Visible = false;
            btn_New.Visible = false;
            btnAddpeople.Visible = false;
            //UC_W_WelcomeBoard ug = new UC_W_WelcomeBoard();
            //addControls(ug);
        }
        private void Form_Home_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to exit?", "Exit Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true; // Prevent closing if the user clicks "No"
            }
            else
            {
                this.FormClosing -= Form_Home_FormClosing;
                Application.Exit();
            }
        }
        private void button2_Click_1(object sender, EventArgs e)
        {
            btnAddpeople.Visible = false;
            btnBookCar.Visible = false;
            btn_New.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            label1.Text = "Admin > Reserve Meeting Room && Schedule > Withdraw";
            UC_R_WithDraw ug = new UC_R_WithDraw(loggedInUser, loggedInDepart);
            addControls(ug);
        }
        private void button3_Click_1(object sender, EventArgs e)
        {
            btnAddpeople.Visible = false;
            btnBookCar.Visible = false;
            btn_New.Visible = false;
            button2.Visible = false;
            button3.Visible = false;

            label1.Text = "Admin > Reserve Meeting Room && Schedule > Replace";
            UC_R_ReplaceMeeting ug = new UC_R_ReplaceMeeting(loggedInUser);
            addControls(ug);
        }
        private void btnBookCar_Click(object sender, EventArgs e)
        {
            //
            btnApp.Visible = false;
            btnWDcar.Visible = false;
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            btnInspect.Visible = false;
            btn_Accident.Visible = false;
            btnCarCondition.Visible = true;
            label1.Text = "Admin > Car Reservation > Reservation";
            UC_C_BookingCar ug = new UC_C_BookingCar(loggedInUser, loggedInIndex);
            addControls(ug);
        }
        private void CheckUserAccess(string username)   // for Approve and Check
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
                                    btnApp.Visible = false;
                                    btnWDcar.Visible = false;
                                    btnAddpeople.Visible = false;
                                    btn_New.Visible = false;
                                    btnBookCar.Visible = false;
                                    button2.Visible = false;
                                    button3.Visible = false;
                                    btnInspect.Visible = false;
                                    btn_Accident.Visible=false;
                                    btnCarCondition.Visible = false;
                                    label1.Text = "Admin > Car Reservation > Approval";
                                    UC_C_ApprovCar ug = new UC_C_ApprovCar(loggedInUser);
                                    addControls(ug);
                                }
                                else if (MA == "2") 
                                {
                                    btnInspect.Visible = false;
                                    btnApp.Visible = false;
                                    btnWDcar.Visible = false;
                                    btnAddpeople.Visible = false;
                                    btn_New.Visible = false;
                                    btnBookCar.Visible = false;
                                    button2.Visible = false;
                                    button3.Visible = false;
                                    btn_Accident.Visible = false;
                                    btnCarCondition.Visible = false;
                                    label1.Text = "Admin > Car Reservation > Check";
                                    UC_C_CarCheckFromManager ug = new UC_C_CarCheckFromManager(loggedInUser, loggedInDepart);
                                    addControls(ug);
                                }
                            }
                            else
                            {
                                //Form_Home.sharedButtonbtnApp.Visible = false;
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
        private void btnApp_Click(object sender, EventArgs e)    //validate
        {
            CheckUserAccess(loggedInUser);
        }
        private void CheckUserAccess1(string username)
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
                                    btnApp.Visible = false;
                                    btnWDcar.Visible = false;
                                    btnAddpeople.Visible = false;
                                    btn_New.Visible = false;
                                    btnBookCar.Visible = false;
                                    button2.Visible = false;
                                    button3.Visible = false;
                                    btnInspect.Visible = false;
                                    btn_Accident.Visible = false;
                                    btnCarCondition.Visible = false;
                                    label1.Text = "Admin > Car Reservation > Inspection";
                                    UC_C_Inspection ug = new UC_C_Inspection(loggedInUser);
                                    addControls(ug);
                                }
                                else if (MA == "2")
                                {
                                    btnApp.Visible = false;
                                    btnWDcar.Visible = false;
                                    btnAddpeople.Visible = false;
                                    btn_New.Visible = false;
                                    btnBookCar.Visible = false;
                                    button2.Visible = false;
                                    button3.Visible = false;
                                    btnInspect.Visible = false;
                                    btn_Accident.Visible = false;
                                    btnCarCondition.Visible = false;
                                    label1.Text = "Admin > Car Reservation > Inspection";
                                    UC_C_Inspection ug = new UC_C_Inspection(loggedInUser);
                                    addControls(ug);
                                }
                                else
                                {
                                   
                                }
                            }
                            else
                            {
                                //Form_Home.sharedButtonbtnApp.Visible = false;
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
        private void btnInspect_Click(object sender, EventArgs e)
        {
            CheckUserAccess1(loggedInUser);
        }
        private void btnAddpeople_Click(object sender, EventArgs e)
        {

        }

        private void btnWDcar_Click(object sender, EventArgs e)
        {
            btnApp.Visible = false;
            btnWDcar.Visible = false;
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            btnInspect.Visible = false;
            btn_Accident.Visible = false;
            btn_AccidentPDF.Visible = false;
            btnCarCondition.Visible = false;
            label1.Text = "Admin > Car Reservation > Withdraw";
            UC_C_WithDrawCar ug = new UC_C_WithDrawCar(loggedInUser, loggedInIndex);
            addControls(ug);
            
        }
        
        private void btn_Accident_Click(object sender, EventArgs e)
        {
            btnApp.Visible = false;
            btnWDcar.Visible = false;
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            btnInspect.Visible = false;
            btn_Accident.Visible = false;
            btnCarCondition.Visible = false;
            btn_AccidentPDF.Visible = true;
            label1.Text = "Admin > Car Reservation > Accident";
            UC_C_Accident ug = new UC_C_Accident(loggedInUser, loggedInIndex, loggedInDepart);
            //groupBox6.Visible = false;
            addControls(ug);
        }

        private void btn_AccidentPDF_Click(object sender, EventArgs e)
        {
            btn_AccidentPDF.Visible = false;
            label1.Text = "Admin > Car Reservation > Accident > View Report";
            UC_C_AccidentPDF ug = new UC_C_AccidentPDF(loggedInUser, loggedInIndex, loggedInDepart);
            addControls(ug);

        }

        private void btnCarCondition_Click(object sender, EventArgs e)
        {
            btnApp.Visible = false;
            btnWDcar.Visible = false;
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            btnInspect.Visible = false;
            btn_Accident.Visible = false;
            btn_AccidentPDF.Visible = false;
            btnCarCondition.Visible = false;
            label1.Text = "Admin > Car Reservation > Acknowledgement";
            UC_C_Verify ug = new UC_C_Verify(loggedInUser, loggedInIndex, loggedInDepart);
            //groupBox6.Visible = false;
            addControls(ug);
        }

        private void btnAccount_Click(object sender, EventArgs e)
        {
            label1.Text = "Account";
            btn_New.Visible = false;
            btnAddpeople.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;   //withdra
            button3.Visible = false;  //replace
            btnWDcar.Visible = false;
            btnApp.Visible = false;
            btn_AccidentPDF.Visible = false;
            btnInspect.Visible = false;
            btn_Accident.Visible = false;
            UC_Acc_Account ug = new UC_Acc_Account(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            label1.Text = "Admin > Meal Request > Internal Menu Edit";
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;

            UC_Meal_InternalMenu ug = new UC_Meal_InternalMenu(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            label1.Text = "Admin > Meal Request > External Menu Edit";
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;

            UC_Meal_ExternalMenu ug = new UC_Meal_ExternalMenu(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            label1.Text = "Admin > Meal Request > View report";
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            btnVisitor.Visible = false;


            UC_Meal_ViewReport ug = new UC_Meal_ViewReport(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void btnNewVisitor_Click(object sender, EventArgs e)
        {
            label1.Text = "Admin > Register New Visitor ";
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            btnVisitor.Visible = false;
            btnWithdrawEntry.Visible = false;
            btnNewVisitor.Visible = false;
            btnUpdate.Visible = false;

            UC_W_RegisterVisitor ug = new UC_W_RegisterVisitor(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void btnVisitor_Click(object sender, EventArgs e)
        {
            label1.Text = "Admin > Display Visitor ";
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            btnVisitor.Visible = false;
            btnWithdrawEntry.Visible = false;
            btnNewVisitor.Visible = false;
            btnUpdate.Visible = false;

            UC_W_InputVisitor ug = new UC_W_InputVisitor(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void btnWithdrawEntry_Click(object sender, EventArgs e)
        {
            label1.Text = "Admin > Withdraw ";
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            btnVisitor.Visible = false;
            btnWithdrawEntry.Visible = false;
            btnNewVisitor.Visible = false;
            btnUpdate.Visible = false;

            UC_W_WithdrawVisitor ug = new UC_W_WithdrawVisitor(loggedInUser, loggedInDepart, currentWelcomeBoard);
            addControls(ug);
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            label1.Text = "Admin > Update Company ";
            btnAddpeople.Visible = false;
            btn_New.Visible = false;
            btnBookCar.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            button5.Visible = false;
            button6.Visible = false;
            btnVisitor.Visible = false;
            btnWithdrawEntry.Visible = false;
            btnNewVisitor.Visible = false;
            btnUpdate.Visible = false;

            UC_W_UpdateCompany ug = new UC_W_UpdateCompany(loggedInUser, loggedInDepart);
            addControls(ug);
        }
    }
}
