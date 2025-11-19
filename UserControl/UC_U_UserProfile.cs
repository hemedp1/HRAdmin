using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
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
using System.Globalization;

namespace HRAdmin.UserControl
{
    public partial class UC_U_UserProfile : System.Windows.Forms.UserControl
    {
        public UC_U_UserProfile()
        {
            InitializeComponent();
            LoadUserDetails();
            this.Load += UC_U_UserProfile_Load;
        }
        private void TxtName1_Leave(object sender, EventArgs e)
        {
            TxtName1.Text = FormatName(TxtName1.Text);
        }
        private void TxtName2_Leave(object sender, EventArgs e)
        {
            TxtName2.Text = FormatName(TxtName2.Text);
        }

        private string FormatName(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Convert whole string to lowercase first
            input = input.ToLower();

            // Convert to proper case (first letter of each word uppercase)
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(input);
        }
        private void UC_U_UserProfile_Load(object sender, EventArgs e)
        {
            LoadDepartmentAndPosition();
        }
        private void LoadUserDetails()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                SELECT 
                    A.Username,
                    A.Name,
                    A.Name1,
                    A.Department,
                    A.Position,
                    A.Password,
                    A.IndexNo,
                    B.BankName,
                    B.AccountNo,
                    B.Email
                FROM tbl_Users A
                LEFT JOIN tbl_UserDetail B ON A.IndexNo = B.IndexNo
                WHERE A.Username = @Username";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@Username", SqlDbType.VarChar).Value = UserSession.LoggedInUser;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                LblUsername.Text = reader["Username"]?.ToString();
                                LblPassword.Text = reader["Password"]?.ToString();
                                lblDepartment.Text = reader["Department"]?.ToString();
                                lblIndexNo.Text = reader["IndexNo"]?.ToString();
                                lblPosition.Text = reader["Position"]?.ToString();
                                TxtEmail.Text = reader["Email"]?.ToString();
                                TxtName1.Text = reader["Name"]?.ToString();
                                TxtName2.Text = reader["Name1"]?.ToString();
                                TxtBankName.Text = reader["BankName"]?.ToString();
                                TxtAccountNo.Text = reader["AccountNo"]?.ToString();
                            }
                            else
                            {
                                MessageBox.Show("No user details found for the current session.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading user details:\n" + ex.Message,
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // First check if username already exists for another user
                    string checkQuery = @"
                SELECT COUNT(*) 
                FROM tbl_Users 
                WHERE Username = @Username AND IndexNo <> @IndexNo";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", LblUsername.Text.Trim());
                        checkCmd.Parameters.AddWithValue("@IndexNo", lblIndexNo.Text.Trim());

                        int exists = (int)checkCmd.ExecuteScalar();
                        if (exists > 0)
                        {
                            MessageBox.Show("The username already exists. Please choose a different one.",
                                            "Duplicate Username",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);
                            return; // Stop update
                        }
                    }

                    // Continue with transaction if username is unique
                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // Update tbl_Users
                            string updateUsers = @"
                        UPDATE tbl_Users
                        SET 
                            Username   = @Username,
                            Name       = @Name,
                            Name1      = @Name1,
                            Department = @Department,
                            Position   = @Position,
                            Password   = @Password
                        WHERE IndexNo = @IndexNo;";

                            using (SqlCommand cmd = new SqlCommand(updateUsers, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Username", LblUsername.Text.Trim());
                                cmd.Parameters.AddWithValue("@Name", TxtName1.Text.Trim());
                                cmd.Parameters.AddWithValue("@Name1", TxtName2.Text.Trim());
                                cmd.Parameters.AddWithValue("@Department", lblDepartment.Text.Trim());
                                cmd.Parameters.AddWithValue("@Position", lblPosition.Text.Trim());
                                cmd.Parameters.AddWithValue("@Password", LblPassword.Text.Trim());
                                cmd.Parameters.AddWithValue("@IndexNo", lblIndexNo.Text.Trim());
                                cmd.ExecuteNonQuery();
                            }

                            // Update tbl_UserDetail
                            string updateDetails = @"
                        UPDATE tbl_UserDetail
                        SET 
                            BankName  = @BankName,
                            AccountNo = @AccountNo,
                            Email     = @Email
                        WHERE IndexNo = @IndexNo;";

                            using (SqlCommand cmd = new SqlCommand(updateDetails, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@BankName", TxtBankName.Text.Trim());
                                cmd.Parameters.AddWithValue("@AccountNo", TxtAccountNo.Text.Trim());
                                cmd.Parameters.AddWithValue("@Email", TxtEmail.Text.Trim());
                                cmd.Parameters.AddWithValue("@IndexNo", lblIndexNo.Text.Trim());
                                cmd.ExecuteNonQuery();
                            }

                            transaction.Commit();

                            MessageBox.Show("User details updated successfully!",
                                            "Success",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while updating user details:\n" + ex.Message,
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
        /*
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Transaction ensures both updates succeed or fail together
                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // Update tbl_Users
                            string updateUsers = @"
                        UPDATE tbl_Users
                        SET 
                            Name       = @Name,
                            Name1      = @Name1,
                            Department = @Department,
                            Position   = @Position,
                            Password   = @Password
                        WHERE IndexNo = @IndexNo;";

                            using (SqlCommand cmd = new SqlCommand(updateUsers, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@Name", TxtName1.Text.Trim());
                                cmd.Parameters.AddWithValue("@Name1", TxtName2.Text.Trim());
                                cmd.Parameters.AddWithValue("@Department", cmbDepartment.Text.Trim());
                                cmd.Parameters.AddWithValue("@Position", cmbPosition.Text.Trim());
                                cmd.Parameters.AddWithValue("@Password", LblPassword.Text.Trim());
                                cmd.Parameters.AddWithValue("@IndexNo", TxtIndexNo.Text.Trim());
                                cmd.ExecuteNonQuery();
                            }

                            // Update tbl_UserDetail
                            string updateDetails = @"
                        UPDATE tbl_UserDetail
                        SET 
                            BankName  = @BankName,
                            AccountNo = @AccountNo,
                            Email     = @Email
                        WHERE IndexNo = @IndexNo;";

                            using (SqlCommand cmd = new SqlCommand(updateDetails, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@BankName", TxtBankName.Text.Trim());
                                cmd.Parameters.AddWithValue("@AccountNo", TxtAccountNo.Text.Trim());
                                cmd.Parameters.AddWithValue("@Email", TxtEmail.Text.Trim());
                                cmd.Parameters.AddWithValue("@IndexNo", TxtIndexNo.Text.Trim());
                                cmd.ExecuteNonQuery();
                            }

                            // Commit transaction if both succeed
                            transaction.Commit();

                            MessageBox.Show("User details updated successfully!",
                                            "Success",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information);
                        }
                        catch
                        {
                            // Rollback if any update fails
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while updating user details:\n" + ex.Message,
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }


        */
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
            Form_Home.sharedLabel.Text = "User";
            UC_U_User ug = new UC_U_User();
            addControls(ug);
        }
        private void LoadDepartmentAndPosition()
        {
            //Load Departments
            lblDepartment.Text = UserSession.loggedInDepart;
            lblIndexNo.Text = UserSession.loggedInIndex;
            lblPosition.Text = UserSession.LoggedInUserTitlePosition;

            /*
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();




                // Load Departments
                string deptQuery = "SELECT DISTINCT Department FROM tbl_Users WHERE Department IS NOT NULL ORDER BY Department";
                using (SqlCommand cmd = new SqlCommand(deptQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cmbDepartment.Items.Add(reader["Department"].ToString());
                    }
                } 
                

                // Load Positions
                string posQuery = "SELECT DISTINCT Position FROM tbl_Users WHERE Position IS NOT NULL ORDER BY Position";
                using (SqlCommand cmd = new SqlCommand(posQuery, con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cmbPosition.Items.Add(reader["Position"].ToString());
                    }
                }
                
        }
            */
            // Enable AutoComplete for Department
            //cmbDepartment.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            //cmbDepartment.AutoCompleteSource = AutoCompleteSource.ListItems;

            // Enable AutoComplete for Position
            //cmbPosition.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            //cmbPosition.AutoCompleteSource = AutoCompleteSource.ListItems;
        }

        private void cmbDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
