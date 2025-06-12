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
            {
                string newUsername = txtUser.Text.Trim();
                string newPassword = txtNewPassword.Text.Trim();
                string Department = comboBox1.Text.Trim();
                string IndexNo = txtIndex.Text.Trim();

                if (string.IsNullOrEmpty(newUsername) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(Department) || string.IsNullOrEmpty(IndexNo))
                {
                    MessageBox.Show("Please fill in all fields.", "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                  
                    string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();

                        // Check if the room is already booked
                        string checkQuery = @"SELECT COUNT(*) FROM tbl_Users WHERE Department = @depart AND Username = @name";

                        SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                        checkCmd.Parameters.AddWithValue("@depart", comboBox1.Text);
                        checkCmd.Parameters.AddWithValue("@name", newUsername);

                        int existingBookings = (int)checkCmd.ExecuteScalar();

                        if (existingBookings > 0)
                        {
                            MessageBox.Show("This username already exist", "Please use another username.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtUser.Clear();
                            txtNewPassword.Clear();
                            return;
                        }

                        
                        
                        // Insert new booking
                        string insertQuery = "INSERT INTO tbl_Users (Username, Department, Password, IndexNo) VALUES (@Username, @Department, @Password, @IndexNo)";

                        

                        SqlCommand insertCmd = new SqlCommand(insertQuery, con);

                        insertCmd.Parameters.AddWithValue("@Username", newUsername);
                        insertCmd.Parameters.AddWithValue("@Department", Department);
                        insertCmd.Parameters.AddWithValue("@Password", newPassword); // ❗ Store hashed passwords in real apps!
                        insertCmd.Parameters.AddWithValue("@IndexNo", IndexNo);



                        insertCmd.ExecuteNonQuery();

                        MessageBox.Show("Registration successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //txtUser.Clear();
                        //txtNewPassword.Clear();
                        this.Close(); // Close registration form
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                /*
                try
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();

                        string query = "INSERT INTO tbl_Users (Username, Department, Password) VALUES (@Username, @Department, @Password)";
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@Username", newUsername);
                            cmd.Parameters.AddWithValue("@Department", Department);
                            cmd.Parameters.AddWithValue("@Password", newPassword); // ❗ Store hashed passwords in real apps!

                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Registration successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            //txtUser.Clear();
                            //txtNewPassword.Clear();
                            this.Close(); // Close registration form
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                } */
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
