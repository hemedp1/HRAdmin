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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRAdmin
{
    public partial class Form_Login : Form
    {
        public Form_Login()
        {
            InitializeComponent();
        }

        // Modified login method with better security and efficiency
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = @"SELECT u.Name, u.Name1, u.Department, u.IndexNo, 
                            ud.BankName, ud.AccountNo, f.AccessLevel, 
                            d.Department0, d.Department1  
                            FROM tbl_Users u 
                            FULL OUTER JOIN tbl_UserDetail ud ON u.IndexNo = ud.IndexNo 
                            FULL OUTER JOIN tbl_UsersLevel f ON u.Position = f.TitlePosition 
                            FULL OUTER JOIN tbl_Department d ON u.Department = d.Department1 
                            WHERE u.Username = @Username AND u.Password = @Password";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);

                        List<string> userRoles = new List<string>();
                        bool isFirstRow = true;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                MessageBox.Show("Invalid username or password.", "Login Failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txtUser.Clear();
                                txtPass.Clear();
                                return;
                            }

                            while (reader.Read())
                            {
                                // Collect all roles from Department0
                                string currentRole = reader["Department0"].ToString();
                                if (!string.IsNullOrEmpty(currentRole))
                                {
                                    if (!userRoles.Contains(currentRole))
                                        userRoles.Add(currentRole);
                                }

                                // Set user session data only once (from first valid row)
                                if (isFirstRow)
                                {
                                    UserSession.LoggedInUser = username;
                                    UserSession.loggedInDepart = reader["Department"].ToString();
                                    UserSession.loggedInIndex = reader["IndexNo"].ToString();
                                    UserSession.loggedInName = reader["Name1"].ToString();
                                    UserSession.loggedInfullName = reader["Name"].ToString();
                                    UserSession.logginInUserAccessLevel = reader["AccessLevel"].ToString();

                                    UserSession.logginDepart0Lvl = reader["Department0"].ToString();
                                    UserSession.logginDepart1stLvl = reader["Department1"].ToString();
                                    UserSession.LoggedInBank = reader["BankName"].ToString();
                                    UserSession.LoggedInAccNo = reader["AccountNo"].ToString();

                                    isFirstRow = false;
                                }
                            }
                        }

                        // Store all collected roles
                        UserSession.UserRoles = userRoles;

                        // Debug output
                        Debug.WriteLine($"User Roles: {string.Join(", ", userRoles)}");

                        this.Hide();
                        Form_Home mainForm = new Form_Home(
                            UserSession.LoggedInUser,
                            UserSession.loggedInDepart,
                            UserSession.loggedInIndex,
                            UserSession.loggedInName,
                            UserSession.loggedInfullName,
                            UserSession.LoggedInBank,
                            UserSession.LoggedInAccNo,
                            UserSession.logginInUserAccessLevel
                        );
                        mainForm.Show();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LLregiester_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form_Register registerForm = new Form_Register();
            registerForm.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {

        }
    }
}
