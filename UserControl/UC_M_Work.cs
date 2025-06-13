using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HRAdmin.Forms;
using System.Configuration;
using System.Data.SqlClient;

namespace HRAdmin.UserControl
{
    public partial class UC_M_Work : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        public UC_M_Work(string username, string department)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;
            LoadPendingBookings();
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
        
        private void btnBack_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "Account > Miscellaneous Claim";

            UC_M_MiscellaneousClaim ug = new UC_M_MiscellaneousClaim(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT id, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice FROM tbl_MiscellaneousClaim";
                using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                {

                    SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                    DataTable dt = (DataTable)dgvW.DataSource;

                    adapter.Update(dt);

                    MessageBox.Show("Menu updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

        }
        private void LoadPendingBookings()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                string query = @"
                                SELECT id, Vendor, Item, InvoiceAmount, InvoiceNo, Invoice FROM tbl_MiscellaneousClaim";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dgvW.DataSource = dt;
            }
        }

    }
}





