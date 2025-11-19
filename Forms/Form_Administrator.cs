using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRAdmin.Forms
{
    public partial class Form_Administrator : Form
    {
        private const string AdminUser = "administrator";
        private const string AdminPass = "x3v12p@ssw0rd"; // change this!
        public Form_Administrator()
        {
            InitializeComponent();
            // Ensure password box masks input
            txtPass.UseSystemPasswordChar = true;

            // Allow Enter key to trigger login
            this.AcceptButton = btnLogin;

            // Wire the click event (if not wired in Designer)
            btnLogin.Click += btnLogin_Click;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text.Trim(); // Trim password too

            if (IsValidAdmin(user, pass))
            {
                //MessageBox.Show("Login successful. Welcome, admin!", "Success",
                //    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Open register form
                Form_Register registerForm = new Form_Register();
                registerForm.Show();

                // Hide the login form instead of disposing it
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Login Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Clear only the password
                txtPass.Clear();
                txtPass.Focus();
            }
        }

        private bool IsValidAdmin(string username, string password)
        {
            // Simple hard-coded check
            return username.Equals(AdminUser, StringComparison.OrdinalIgnoreCase)
                && password == AdminPass;
        }
    }
}
