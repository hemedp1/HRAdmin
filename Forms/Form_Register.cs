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

namespace HRAdmin.Forms
{
    public partial class Form_Register : Form
    {
        public Form_Register()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string newUsername = txtUser.Text.Trim();
            string newPassword = txtNewPassword.Text.Trim();
            string Department = comboBox1.Text.Trim();
            string IndexNo = txtIndex.Text.Trim();

            if (string.IsNullOrEmpty(newUsername) || string.IsNullOrEmpty(newPassword) ||
                string.IsNullOrEmpty(Department) || string.IsNullOrEmpty(IndexNo))
            {
                MessageBox.Show("Please fill in all fields.", "Registration Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Check if the username or index number already exists
                    string checkQuery = @"SELECT COUNT(*) FROM tbl_Users 
                                 WHERE (Username = @name) OR (IndexNo = @IndexNo)";

                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@name", newUsername);
                    checkCmd.Parameters.AddWithValue("@IndexNo", IndexNo);

                    int existingRecords = (int)checkCmd.ExecuteScalar();

                    if (existingRecords > 0)
                    {
                        // Additional check to see which one exists
                        string specificCheck = @"SELECT 
                                       CASE WHEN Username = @name THEN 'Username' 
                                            WHEN IndexNo = @IndexNo THEN 'IndexNo' 
                                       END 
                                       FROM tbl_Users 
                                       WHERE Username = @name OR IndexNo = @IndexNo";

                        SqlCommand specificCmd = new SqlCommand(specificCheck, con);
                        specificCmd.Parameters.AddWithValue("@name", newUsername);
                        specificCmd.Parameters.AddWithValue("@IndexNo", IndexNo);

                        string conflictField = (string)specificCmd.ExecuteScalar();

                        MessageBox.Show($"This {conflictField} is already registered.",
                                      "Registration Error",
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        if (conflictField == "Username")
                        {
                            txtUser.Clear();
                            txtUser.Focus();
                        }
                        else
                        {
                            txtIndex.Clear();
                            txtIndex.Focus();
                        }

                        txtNewPassword.Clear();
                        return;
                    }

                    // Insert new user
                    string insertQuery = @"INSERT INTO tbl_Users 
                                 (Username, Department, Password, IndexNo) 
                                 VALUES (@Username, @Department, @Password, @IndexNo)";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, con);
                    insertCmd.Parameters.AddWithValue("@Username", newUsername);
                    insertCmd.Parameters.AddWithValue("@Department", Department);
                    insertCmd.Parameters.AddWithValue("@Password", newPassword); // ❗ Store hashed passwords in real apps!
                    insertCmd.Parameters.AddWithValue("@IndexNo", IndexNo);

                    insertCmd.ExecuteNonQuery();

                    MessageBox.Show("Registration successful!", "Success",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close(); // Close registration form
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtUser_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
