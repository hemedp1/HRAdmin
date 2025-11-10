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
using System.Net.Mail;
using System.Net;
using HRAdmin.Components;
using System.Threading;

namespace HRAdmin.Forms
{
    public partial class Form_ForgetPass : Form
    {
        public Form_ForgetPass()
        {
            InitializeComponent();
        }
        private void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                // Connection string (replace with your actual connection string)
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT Mail, Password, Port, SmtpClient FROM tbl_Administrator WHERE ID = 1";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string fromEmail = reader["Mail"].ToString();
                                string password = reader["Password"].ToString();
                                int port = Convert.ToInt32(reader["Port"]);
                                string smtpClient = reader["SmtpClient"].ToString();

                                MailMessage mail = new MailMessage();
                                mail.From = new MailAddress(fromEmail);
                                mail.To.Add(toEmail);
                                mail.Subject = subject;
                                mail.Body = body;
                                mail.IsBodyHtml = true;

                                SmtpClient smtp = new SmtpClient(smtpClient, port);
                                smtp.Credentials = new NetworkCredential(fromEmail, password);
                                smtp.EnableSsl = false;

                                smtp.Send(mail);

                                //MessageBox.Show("Notification for your booking will be sent to your approver.",
                                //    "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (SmtpException smtpEx)
            {
                MessageBox.Show($"SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}\n\nFull Details:\n{smtpEx.ToString()}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"General Error: {ex.Message}\n\nFull Details:\n{ex.ToString()}");
            }
        }
        private void btnChange_Click(object sender, EventArgs e)
        {
            string indexNo = txtIndex.Text.Trim();
            string newPassword = txtNewPassword.Text.Trim();
            string reenterPassword = txtReenterPassword.Text.Trim();

            // Step 1: Basic validation
            if (string.IsNullOrEmpty(indexNo) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(reenterPassword))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (newPassword != reenterPassword)
            {
                MessageBox.Show("Passwords do not match.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Step 2: Check if IndexNo exists and update password
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // Check if user exists
                    string checkQuery = "SELECT COUNT(*) FROM tbl_Users WHERE IndexNo = @IndexNo";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@IndexNo", indexNo);

                        int count = (int)checkCmd.ExecuteScalar();
                        if (count == 0)
                        {
                            MessageBox.Show("Index number not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Update password
                    string updateQuery = "UPDATE tbl_Users SET Password = @Password WHERE IndexNo = @IndexNo";
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@Password", newPassword);
                        cmd.Parameters.AddWithValue("@IndexNo", indexNo);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            MessageBox.Show("Password changed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            //++++++++++++++++++++++++++++++++++++++         Email Fx        ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                            string query1 = @"SELECT A.Email, B.Name1 
                                              FROM tbl_UserDetail A 
                                              LEFT JOIN tbl_Users B ON B.IndexNo = A.IndexNo 
                                              WHERE A.IndexNo = @IndexNumber";

                            List<string> approverEmails = new List<string>();
                            string namefull = ""; // <--- move here
                            using (SqlCommand emailCmd = new SqlCommand(query1, con))
                            {
                                emailCmd.Parameters.AddWithValue("@IndexNumber", indexNo);
                                //con.Open(); // 

                                using (SqlDataReader reader = emailCmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string email = reader["Email"]?.ToString();
                                        namefull = reader["Name1"]?.ToString();
                                        if (!string.IsNullOrEmpty(email))
                                        {
                                            approverEmails.Add(email);
                                        }
                                    }
                                }
                            }
                            if (approverEmails.Count > 0)
                            {

                                string subject = "HEM Admin Accessibility Notification: Password Successfully Changed";

                                string body = $@"                   
                                                <p>Dear Mr./Ms. <strong>{namefull}</strong>,</p>

                                                <p>
                                                    This is to notify you that your account password has been successfully changed.<br/>
                                                    <strong>New Password:</strong> {newPassword}
                                                </p>

                                                <p><u>Security Reminder:</u></p>
                                                <ul>
                                                    <li>Please keep your password confidential.</li>
                                                    <li>Do not share it with anyone.</li>
                                                    <li>If this change was not done by you, contact HR & Admin immediately.</li>
                                                </ul>

                                                <p>Thank you,<br/>HEM Admin Accessibility</p>
                                                 ";


                                foreach (var email in approverEmails)
                                {
                                    SendEmail(email, subject, body);
                                }

                                MessageBox.Show(
                                    "Your password has been successfully changed. A confirmation email has been sent to your email address.",
                                    "Password Updated",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information
                                );

                            }




                            txtIndex.Clear();
                            txtNewPassword.Clear();
                            txtReenterPassword.Clear();
                            this.Close(); // Close registration form
                        }
                        else
                        {
                            MessageBox.Show("Failed to change password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
