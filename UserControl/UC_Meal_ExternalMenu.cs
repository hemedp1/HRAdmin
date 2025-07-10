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
using DrawingFont = System.Drawing.Font;

namespace HRAdmin.UserControl
{
    public partial class UC_Meal_ExternalMenu : System.Windows.Forms.UserControl
    {
        private string eventDetails;
        private DateTime eventDate;
        private DateTime DeliveryTime;
        private string loggedInUser;
        private string loggedInDepart;
        private string EventDetails;
        private string EventTime;

        public UC_Meal_ExternalMenu(string loggedInUser, string department)
        {
            InitializeComponent();
            this.loggedInUser = loggedInUser;
            loggedInDepart = department;
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            // Initialize ComboBoxes with options
            cmbMeal.Items.Clear();
            cmbPackage.Items.Clear();
            cmbStyle.Items.Clear();
            cmbPackage.Items.AddRange(new string[] { "A", "B", "C" });
            cmbMeal.Items.AddRange(new string[] { "BREAKFAST", "LUNCH", "TEA" });
            cmbStyle.Items.AddRange(new string[] { "Buffet", "Packing" });

            // Set default selections to load initial data
            cmbPackage.SelectedIndex = -1; // Default to Package A
            cmbMeal.SelectedIndex = -1;    // Default to BREAKFAST
            cmbStyle.SelectedIndex = -1;   // Default to Buffet

            // Attach event handlers
            cmbPackage.SelectedIndexChanged += cmbPackage_SelectedIndexChanged;
            cmbMeal.SelectedIndexChanged += cmbMeal_SelectedIndexChanged;
            cmbStyle.SelectedIndexChanged += cmbStyle_SelectedIndexChanged;
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

        private void UC_ExternalMenu_Load(object sender, EventArgs e)
        {
            StyleDataGridView(dgv_EM);
            // Load initial data based on default ComboBox selections
            if (cmbPackage.SelectedItem != null && cmbMeal.SelectedItem != null && cmbStyle.SelectedItem != null)
            {
                LoadMenuItems(cmbPackage.SelectedItem.ToString(), cmbMeal.SelectedItem.ToString(), cmbStyle.SelectedItem.ToString());
            }
        }

        private void LoadMenuItems(string package, string mealType, string style)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    // Updated query to include Package, Meal, and Style
                    string query = "SELECT id, Package, Meal, Style, Menu FROM tbl_Menu WHERE Package = @Package AND Meal = @Meal AND Style = @Style";
                    using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@Package", package);
                        da.SelectCommand.Parameters.AddWithValue("@Meal", mealType);
                        da.SelectCommand.Parameters.AddWithValue("@Style", style);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgv_EM.DataSource = dt;
                    }

                    // Configure DataGridView properties
                    dgv_EM.ReadOnly = false;
                    if (dgv_EM.Columns.Contains("id"))
                    {
                        dgv_EM.Columns["id"].ReadOnly = true;
                    }
                    if (dgv_EM.Columns.Contains("Package"))
                    {
                        dgv_EM.Columns["Package"].Visible = false; // Hide Package column in the grid
                    }
                    if (dgv_EM.Columns.Contains("Meal"))
                    {
                        dgv_EM.Columns["Meal"].Visible = false; // Hide Meal column in the grid
                    }
                    if (dgv_EM.Columns.Contains("Style"))
                    {
                        dgv_EM.Columns["Style"].Visible = false; // Hide Style column in the grid
                    }
                    dgv_EM.AllowUserToAddRows = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading menu items: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Style the DataGridView
            StyleDataGridView(dgv_EM);
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.ColumnHeadersVisible = false;
            dgv.Dock = DockStyle.Fill;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false;

            dgv.ReadOnly = false;
            if (dgv.Columns.Contains("id"))
            {
                dgv.Columns["id"].ReadOnly = true;
                dgv.Columns["id"].Width = 35;

            }
            dgv.AllowUserToAddRows = true;

            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.Black,
                    Font = new DrawingFont("Helvetica", 11),
                    BackColor = Color.WhiteSmoke
                };
                column.Resizable = DataGridViewTriState.False;
                column.ReadOnly = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form_Home.sharedLabel.Text = "Admin > Meal Request";
            Form_Home.sharedButton4.Visible = true;
            Form_Home.sharedButton5.Visible = true;
            Form_Home.sharedButton6.Visible = true;
            UC_Meal_Food ug = new UC_Meal_Food(EventDetails, EventTime, DeliveryTime, loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate ComboBox selections
                if (cmbPackage.SelectedItem == null)
                {
                    MessageBox.Show("Package is required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cmbMeal.SelectedItem == null)
                {
                    MessageBox.Show("Meal is required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cmbStyle.SelectedItem == null)
                {
                    MessageBox.Show("Style is required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    // Updated query to include Package, Meal, and Style
                    string query = "SELECT id, Package, Meal, Style, Menu FROM tbl_Menu WHERE Package = @Package AND Meal = @Meal AND Style = @Style";
                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, con))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@Package", cmbPackage.SelectedItem.ToString());
                        adapter.SelectCommand.Parameters.AddWithValue("@Meal", cmbMeal.SelectedItem.ToString());
                        adapter.SelectCommand.Parameters.AddWithValue("@Style", cmbStyle.SelectedItem.ToString());
                        SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                        DataTable dt = (DataTable)dgv_EM.DataSource;

                        // Ensure Meal and Style columns in the DataTable are updated with ComboBox values
                        foreach (DataRow row in dt.Rows)
                        {
                            row["Package"] = cmbPackage.SelectedItem.ToString();
                            row["Meal"] = cmbMeal.SelectedItem.ToString();
                            row["Style"] = cmbStyle.SelectedItem.ToString();
                        }

                        adapter.Update(dt);

                        MessageBox.Show("Menu updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cmbPackage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPackage.SelectedItem != null && cmbMeal.SelectedItem != null && cmbStyle.SelectedItem != null)
            {
                LoadMenuItems(cmbPackage.SelectedItem.ToString(), cmbMeal.SelectedItem.ToString(), cmbStyle.SelectedItem.ToString());
            }
        }

        private void cmbMeal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPackage.SelectedItem != null && cmbMeal.SelectedItem != null && cmbStyle.SelectedItem != null)
            {
                LoadMenuItems(cmbPackage.SelectedItem.ToString(), cmbMeal.SelectedItem.ToString(), cmbStyle.SelectedItem.ToString());
            }
        }

        private void cmbStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPackage.SelectedItem != null && cmbMeal.SelectedItem != null && cmbStyle.SelectedItem != null)
            {
                LoadMenuItems(cmbPackage.SelectedItem.ToString(), cmbMeal.SelectedItem.ToString(), cmbStyle.SelectedItem.ToString());
            }
        }
    }
}