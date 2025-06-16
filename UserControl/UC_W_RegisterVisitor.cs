using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using HRAdmin.Forms;
using System.Configuration;
using System.Data.SqlClient;

namespace HRAdmin.UserControl
{
    public partial class UC_W_RegisterVisitor : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        public UC_W_RegisterVisitor(string username, string department)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;

            // Populate cmb_NOP with unique numbers 1 to 10
            for (int i = 1; i <= 10; i++)
            {
                cmb_NOP.Items.Add(i.ToString());
            }

            // Set default selection to 1
            cmb_NOP.SelectedIndex = 0; // Selects "1" by default

            // Attach the event handler
            cmb_NOP.SelectedIndexChanged += cmb_NOP_SelectedIndexChanged;

            // Ensure txtVisitor1 is always visible by default
            txtVisitor1.Visible = true;

            // Hide other visitor textboxes by default (will be managed by cmb_NOP selection)
            TextBox[] visitorTextBoxes =
            {
                txtVisitor2, txtVisitor3, txtVisitor4, txtVisitor5,
                txtVisitor6, txtVisitor7, txtVisitor8, txtVisitor9, txtVisitor10
            };
            foreach (var textBox in visitorTextBoxes)
            {
                textBox.Visible = false;
            }

            // Configure cmbCompany to allow typing
            cmbCompany.DropDownStyle = ComboBoxStyle.DropDown;
            // Load companies from database
            LoadCompanies();
        }

        private void LoadCompanies()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT DISTINCT Company FROM tbl_RegisterVisitor ORDER BY Company";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        cmbCompany.Items.Clear();
                        while (reader.Read())
                        {
                            cmbCompany.Items.Add(reader["Company"].ToString());
                        }
                    }
                }
            }
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

        private void button2_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "Welcome Board";
            Form_Home.sharedButton4.Visible = false;
            Form_Home.sharedButton5.Visible = false;
            Form_Home.sharedButton6.Visible = false;
            Form_Home.sharedbtnVisitor.Visible = true;
            Form_Home.sharedbtnWithdrawEntry.Visible = true;
            Form_Home.sharedbtnNewVisitor.Visible = true;
            Form_Home.sharedbtnUpdate.Visible = true;
            UC_W_WelcomeBoard ug = new UC_W_WelcomeBoard(loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cmbCompany.Text))
            {
                MessageBox.Show("Company's name required.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtVisitor1.Text))
            {
                MessageBox.Show("Visitor's name required.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Array of visitor textboxes
                TextBox[] visitorTextBoxes = { txtVisitor1, txtVisitor2, txtVisitor3, txtVisitor4, txtVisitor5,
                                             txtVisitor6, txtVisitor7, txtVisitor8, txtVisitor9, txtVisitor10 };

                // Check for existing entries
                bool duplicateFound = false;
                List<string> duplicateVisitors = new List<string>();

                foreach (var textBox in visitorTextBoxes)
                {
                    if (textBox.Visible && !string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        string checkQuery = "SELECT COUNT(*) FROM tbl_RegisterVisitor WHERE Company = @Company AND Visitor = @Visitor";
                        using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                        {
                            checkCmd.Parameters.AddWithValue("@Company", cmbCompany.Text);
                            checkCmd.Parameters.AddWithValue("@Visitor", textBox.Text);
                            int count = (int)checkCmd.ExecuteScalar();
                            if (count > 0)
                            {
                                duplicateFound = true;
                                duplicateVisitors.Add(textBox.Text);
                            }
                        }
                    }
                }

                // If duplicates found, show warning and ask to proceed
                if (duplicateFound)
                {
                    string message = $"The following visitor(s) already exist for {cmbCompany.Text}:\n" +
                                   string.Join(", ", duplicateVisitors) +
                                   "\n\nWould you like to proceed with registration?";
                    DialogResult result = MessageBox.Show(message, "Duplicate Entry Warning",
                                                       MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (result == DialogResult.No)
                    {
                        return; // User chose not to proceed
                    }
                }

                // Insert new records
                foreach (var textBox in visitorTextBoxes)
                {
                    if (textBox.Visible && !string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        string insertQuery = "INSERT INTO tbl_RegisterVisitor (Company, Visitor) VALUES (@Company, @Visitor)";
                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                        {
                            insertCmd.Parameters.AddWithValue("@Company", cmbCompany.Text);
                            insertCmd.Parameters.AddWithValue("@Visitor", textBox.Text);
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Visitor details submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Stay on the same page
                Form_Home.sharedLabel.Text = "Register Visitor";
                Form_Home.sharedButton.Visible = false;
                Form_Home.sharedButtonew.Visible = false;
                Form_Home.sharedButtonBC.Visible = false;
                Form_Home.sharedButton4.Visible = false;
                Form_Home.sharedButton5.Visible = false;
                Form_Home.sharedbtnVisitor.Visible = false;
                Form_Home.sharedbtnWithdrawEntry.Visible = false;
                Form_Home.sharedbtnNewVisitor.Visible = false;

                UC_W_RegisterVisitor mainPage = new UC_W_RegisterVisitor(loggedInUser, loggedInDepart);
                addControls(mainPage);
            }
        }

        private void cmb_NOP_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_NOP.SelectedItem == null) return;

            if (int.TryParse(cmb_NOP.SelectedItem.ToString(), out int numberOfVisitors))
            {
                TextBox[] visitorTextBoxes =
                {
                    txtVisitor1, txtVisitor2, txtVisitor3, txtVisitor4, txtVisitor5,
                    txtVisitor6, txtVisitor7, txtVisitor8, txtVisitor9, txtVisitor10
                };

                for (int i = 0; i < visitorTextBoxes.Length; i++)
                {
                    visitorTextBoxes[i].Visible = i < numberOfVisitors;
                }
            }
        }
    }
}  //UC_W_RegisterVisitor