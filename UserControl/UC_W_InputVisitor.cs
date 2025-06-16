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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Configuration;
using System.Data.SqlClient;

namespace HRAdmin.UserControl
{
    public partial class UC_W_InputVisitor : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private List<string> allVisitorNames; // To store the full list of visitor names
        private ComboBox[] visitorComboBoxes; // Array of visitor comboboxes for easier management
        private bool isUpdatingComboBoxes; // Flag to prevent recursive event calls

        public UC_W_InputVisitor(string username, string department)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;

            // Initialize allVisitorNames to prevent null reference
            allVisitorNames = new List<string>();

            // Initialize the array of visitor comboboxes
            visitorComboBoxes = new ComboBox[]
            {
                cmbVisitor1, cmbVisitor2, cmbVisitor3, cmbVisitor4, cmbVisitor5,
                cmbVisitor6, cmbVisitor7, cmbVisitor8, cmbVisitor9, cmbVisitor10
            };

            // Attach event handlers for each combobox
            foreach (var comboBox in visitorComboBoxes)
            {
                comboBox.SelectedIndexChanged += VisitorComboBox_SelectedIndexChanged;
            }

            isUpdatingComboBoxes = false; // Initialize flag

            // Initially disable NOP and visitor comboboxes until a company is selected
            cmb_NOP.Enabled = false;
            foreach (var comboBox in visitorComboBoxes)
            {
                comboBox.Enabled = false;
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

        private void UC_InputVisitor_Load(object sender, EventArgs e)
        {
            // Populate ComboBox with numbers 1 to 10 if not already done in designer
            if (cmb_NOP.Items.Count == 0)
            {
                for (int i = 1; i <= 10; i++)
                {
                    cmb_NOP.Items.Add(i.ToString());
                }
            }

            // Populate company combobox
            PopulateCompanyComboBox();

            // Initially, no company is selected, so clear purpose textbox (but keep it enabled)
            txtPurpose.Text = "";

            // Attach event handler for company selection
            cmbCompany.SelectedIndexChanged += cmbCompany_SelectedIndexChanged;

            // Set default selection to 1 and show only cmbVisitor1
            cmb_NOP.SelectedIndex = 0;
            UpdateVisibleComboBoxes(1);

            // Populate visitor comboboxes (initially empty until a company is selected)
            PopulateVisitorComboBoxes();
        }

        private void PopulateCompanyComboBox()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT DISTINCT Company FROM tbl_RegisterVisitor ORDER BY Company";
                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataReader reader = cmd.ExecuteReader();

                    cmbCompany.Items.Clear();
                    while (reader.Read())
                    {
                        cmbCompany.Items.Add(reader["Company"].ToString());
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading companies: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbCompany_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When company selection changes, update visitor comboboxes
            PopulateVisitorComboBoxes();
        }

        private void PopulateVisitorComboBoxes()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            allVisitorNames = new List<string>(); // Reinitialize to clear previous data

            // If no company is selected, clear the visitor list, set NOP to 1, and disable controls
            if (cmbCompany.SelectedItem == null)
            {
                foreach (var comboBox in visitorComboBoxes)
                {
                    comboBox.Items.Clear();
                    comboBox.SelectedIndex = -1;
                    comboBox.Enabled = false;
                }
                cmb_NOP.SelectedIndex = 0; // Default to 1
                cmb_NOP.Enabled = false;
                UpdateVisibleComboBoxes(1);
                UpdateAllVisitorComboBoxes(true); // Force reset
                return;
            }

            // Clear all visitor ComboBox selections and items before repopulating
            foreach (var comboBox in visitorComboBoxes)
            {
                comboBox.SelectedIndex = -1; // Reset selection
                comboBox.Items.Clear(); // Clear items
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string query = "SELECT DISTINCT Visitor FROM tbl_RegisterVisitor WHERE Company = @Company AND Visitor IS NOT NULL ORDER BY Visitor";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Company", cmbCompany.SelectedItem.ToString());
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        allVisitorNames.Add(reader["Visitor"].ToString());
                    }
                    reader.Close();
                }

                // Update cmb_NOP based on the number of visitors
                int visitorCount = allVisitorNames.Count;
                if (visitorCount > 0)
                {
                    // Ensure cmb_NOP has enough options
                    if (visitorCount > cmb_NOP.Items.Count)
                    {
                        cmb_NOP.Items.Clear();
                        for (int i = 1; i <= visitorCount; i++)
                        {
                            cmb_NOP.Items.Add(i.ToString());
                        }
                    }

                    // Set cmb_NOP to the number of visitors (1-based index for display)
                    if (visitorCount <= cmb_NOP.Items.Count)
                    {
                        cmb_NOP.SelectedItem = visitorCount.ToString();
                        UpdateVisibleComboBoxes(visitorCount);
                    }
                    else
                    {
                        cmb_NOP.SelectedIndex = cmb_NOP.Items.Count - 1; // Max available
                        UpdateVisibleComboBoxes(cmb_NOP.Items.Count);
                    }

                    // Enable controls since a company is selected and visitors are available
                    cmb_NOP.Enabled = true;
                    foreach (var comboBox in visitorComboBoxes)
                    {
                        comboBox.Enabled = true;
                    }
                }
                else
                {
                    // No visitors found, default to 1 and disable visitor comboboxes
                    cmb_NOP.SelectedIndex = 0;
                    UpdateVisibleComboBoxes(1);
                    foreach (var comboBox in visitorComboBoxes)
                    {
                        comboBox.Enabled = false;
                    }
                    cmb_NOP.Enabled = true; // Allow changing NOP, but no visitors to select
                }

                // Force a complete reset of all visitor ComboBoxes with the new visitor list
                UpdateAllVisitorComboBoxes(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading visitors: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                allVisitorNames = new List<string>(); // Ensure allVisitorNames is not null
                cmb_NOP.SelectedIndex = 0; // Default to 1
                UpdateVisibleComboBoxes(1);
                foreach (var comboBox in visitorComboBoxes)
                {
                    comboBox.Enabled = false;
                }
                cmb_NOP.Enabled = false;
                UpdateAllVisitorComboBoxes(true); // Force reset on error
            }
        }

        private void VisitorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isUpdatingComboBoxes) return; // Prevent recursive calls

            // Update all comboboxes to filter out selected visitors
            UpdateAllVisitorComboBoxes();
        }

        private void UpdateAllVisitorComboBoxes(bool forceReset = false)
        {
            if (isUpdatingComboBoxes && !forceReset) return; // Prevent recursive calls unless forced

            isUpdatingComboBoxes = true; // Set flag to block events

            try
            {
                // Clear selections for all ComboBoxes if forcing a reset
                if (forceReset)
                {
                    foreach (var comboBox in visitorComboBoxes)
                    {
                        comboBox.Items.Clear(); // Clear items to ensure no old data persists
                        comboBox.SelectedIndex = -1; // Ensure selection is cleared
                    }
                }

                // Get the currently selected visitors (only for non-forced updates)
                List<string> selectedVisitors = new List<string>();
                if (!forceReset) // Only collect selections if not forcing a reset
                {
                    foreach (var comboBox in visitorComboBoxes)
                    {
                        if (comboBox.SelectedItem != null && comboBox.Visible)
                        {
                            selectedVisitors.Add(comboBox.SelectedItem.ToString());
                        }
                    }
                }

                // Update each combobox
                foreach (var comboBox in visitorComboBoxes)
                {
                    if (!comboBox.Visible) continue;

                    // If forcing a reset, ensure the ComboBox is cleared again
                    if (forceReset)
                    {
                        comboBox.Items.Clear();
                        comboBox.SelectedIndex = -1;
                    }

                    string currentSelection = forceReset ? null : comboBox.SelectedItem?.ToString(); // Ignore current selection if forceReset
                    comboBox.BeginUpdate(); // Suspend layout to reduce flicker
                    if (!forceReset || comboBox.Items.Count == 0) // Only clear items if not already cleared
                    {
                        comboBox.Items.Clear();
                    }

                    // Add all visitors except those selected in other comboboxes
                    if (allVisitorNames != null) // Null check
                    {
                        foreach (string name in allVisitorNames)
                        {
                            // Skip if this name is selected in another combobox, but allow it if it's the current combobox's selection
                            if (!selectedVisitors.Contains(name) || name == currentSelection)
                            {
                                comboBox.Items.Add(name);
                            }
                        }
                    }

                    // Restore the current selection only if not forcing a reset and it exists
                    if (!forceReset && currentSelection != null && comboBox.Items.Contains(currentSelection))
                    {
                        comboBox.SelectedItem = currentSelection;
                    }
                    else
                    {
                        comboBox.SelectedIndex = -1; // Clear selection if forcing reset or previous selection is invalid
                    }

                    comboBox.EndUpdate(); // Resume layout
                }
            }
            finally
            {
                isUpdatingComboBoxes = false; // Reset flag
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (cmbCompany.SelectedItem == null)
            {
                MessageBox.Show("Please select a company.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPurpose.Text))
            {
                MessageBox.Show("Please enter the purpose of the visit.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if at least one visitor is selected
            bool hasVisitor = false;
            foreach (var comboBox in visitorComboBoxes)
            {
                if (comboBox.Visible && comboBox.SelectedItem != null)
                {
                    hasVisitor = true;
                    break;
                }
            }
            if (!hasVisitor)
            {
                MessageBox.Show("Please select at least one visitor.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Insert one row per selected visitor
                    string insertQuery = "INSERT INTO tbl_WelcomeBoard (Company, Purpose, StartDate, EndDate, RecordDate, Visitor) VALUES (@Company, @Purpose, @StartDate, @EndDate, @RecordDate, @Visitor)";

                    foreach (var comboBox in visitorComboBoxes)
                    {
                        if (comboBox.Visible && comboBox.SelectedItem != null)
                        {
                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                            {
                                insertCmd.Parameters.AddWithValue("@Company", cmbCompany.SelectedItem.ToString());
                                insertCmd.Parameters.AddWithValue("@Purpose", txtPurpose.Text);
                                insertCmd.Parameters.AddWithValue("@StartDate", dtpStartDate.Value);
                                insertCmd.Parameters.AddWithValue("@EndDate", dtpEndDate.Value);
                                insertCmd.Parameters.AddWithValue("@RecordDate", DateTime.Now);
                                insertCmd.Parameters.AddWithValue("@Visitor", comboBox.SelectedItem.ToString());

                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    MessageBox.Show("Visitor details submitted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Form_Home.sharedLabel.Text = "Display Visitor";
                    Form_Home.sharedButton4.Visible = false;
                    Form_Home.sharedButton5.Visible = false;
                    Form_Home.sharedButton6.Visible = false;
                    Form_Home.sharedbtnVisitor.Visible = false;
                    Form_Home.sharedbtnWithdrawEntry.Visible = false;
                    Form_Home.sharedbtnNewVisitor.Visible = false;
                    Form_Home.sharedbtnUpdate.Visible = false;
                    UC_W_InputVisitor mainPage = new UC_W_InputVisitor(loggedInUser, loggedInDepart);
                    addControls(mainPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting visitor details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmb_NOP_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(cmb_NOP.SelectedItem?.ToString(), out int numberOfVisitors))
            {
                // Skip validation if no company is selected
                if (cmbCompany.SelectedItem == null)
                {
                    UpdateVisibleComboBoxes(numberOfVisitors);
                    UpdateAllVisitorComboBoxes();
                    return;
                }

                // Only update if the number of visitors is valid and within the range of available visitors
                if (allVisitorNames != null && numberOfVisitors <= allVisitorNames.Count)
                {
                    UpdateVisibleComboBoxes(numberOfVisitors);
                    UpdateAllVisitorComboBoxes(); // Update items when visibility changes
                }
                else if (allVisitorNames != null && numberOfVisitors > allVisitorNames.Count)
                {
                    // If selected number exceeds available visitors, set to max available
                    MessageBox.Show($"Only {allVisitorNames.Count} visitors available for this company.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmb_NOP.SelectedItem = allVisitorNames.Count.ToString();
                    UpdateVisibleComboBoxes(allVisitorNames.Count);
                    UpdateAllVisitorComboBoxes();
                }
            }
        }

        private void UpdateVisibleComboBoxes(int numberOfVisitors)
        {
            for (int i = 0; i < visitorComboBoxes.Length; i++)
            {
                visitorComboBoxes[i].Visible = i < numberOfVisitors;
                visitorComboBoxes[i].SelectedIndex = -1; // Clear selection for all ComboBoxes, visible or not
            }
        }

        private void txtPurpose_TextChanged(object sender, EventArgs e)
        {

        }

      
    }
}