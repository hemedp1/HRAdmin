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
                    //string query = "SELECT Name, a.Name1, a.Department, a.IndexNo, b.AccessLevel \r\nFROM tbl_Users a left join tbl_UsersLevel b ON a.Position = b.TitlePosition  WHERE Username = @Username AND Password = @Password";
                    string query = "SELECT u.Name, u.Name1, u.Department, u.IndexNo, ud.BankName, ud.AccountNo, f.AccessLevel  \r\n" +
                                   "FROM tbl_Users u \r\n" +
                                   "FULL OUTER JOIN tbl_UserDetail ud ON u.IndexNo = ud.IndexNo \r\n" +
                                   "FULL OUTER JOIN tbl_UsersLevel f ON u.Position = f.TitlePosition \r\n" +
                                   "WHERE u.Username = @Username AND u.Password = @Password";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        // In production, implement password hashing
                        cmd.Parameters.AddWithValue("@Password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                
                                string depart = reader["Department"].ToString();
                                string Index = reader["IndexNo"].ToString();
                                string fullName = reader["Name"].ToString();
                                string Name = reader["Name1"].ToString();
                                string UL = reader["AccessLevel"].ToString();

                                string bank = reader["BankName"].ToString();
                                string accountNo = reader["AccountNo"].ToString();
                                UserSession.LoggedInUser = username;
                                UserSession.loggedInDepart = depart;
                                UserSession.loggedInIndex = Index;
                                UserSession.loggedInName = Name;
                                UserSession.loggedInfullName = fullName;
                                UserSession.logginInUserAccessLevel = UL;

                                //UserSession.LoggedInBank = bank;
                                //UserSession.LoggedInAccNo = accountNo;
                                //MessageBox.Show($"logginInUserAccessLevel: {UL}");
                                UserSession.LoggedInBank = bank;
                                UserSession.LoggedInAccNo = accountNo;
                                //MessageBox.Show($"DDSDSDDWDWWD: {Index}");
                                this.Hide();
                                //Form_Home mainForm = new Form_Home(username, depart, Index, Name, fullName, UL);
                                Form_Home mainForm = new Form_Home(username, depart, Index, Name, fullName, bank, accountNo, UL);

                                mainForm.Show();
                            }
                            else
                            {
                                MessageBox.Show("Invalid username or password.", "Login Failed",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                txtUser.Clear();
                                txtPass.Clear();
                            }
                        }
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
