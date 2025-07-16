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
    public partial class UC_Meal_InternalMenu : System.Windows.Forms.UserControl
    {
        private string eventDetails;
        private DateTime eventDate;
        private DateTime DeliveryTime;
        private string loggedInUser;
        private string loggedInDepart;
        private string EventDetails;
        private string EventTime;
        private string mealType; // Default meal type

        public UC_Meal_InternalMenu(string username, string department)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;

            // Initialize ComboBox with meal options
            InitializeComboBox();

            // Subscribe to the RowsAdded event
            dgv_FD.RowsAdded += dgv_FD_RowsAdded;
        }

        private void InitializeComboBox()
        {
            // Assuming a ComboBox named cmbMeal is added to the designer
            cmbMeal.Items.AddRange(new string[] { "BREAKFAST", "LUNCH", "TEA", "DINNER" });
            cmbMeal.SelectedIndex = -1; // No default selection
            cmbMeal.SelectedIndexChanged += cbMealType_SelectedIndexChanged;

            cmbType.Items.AddRange(new string[] { "FOOD", "OTHER", "WATER" });
            cmbType.SelectedIndex = -1; // No default selection
            cmbType.SelectedIndexChanged += cmbType_SelectedIndexChanged;
        }

        private void LoadMenuItems(string mealType, string type)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT id, Meal, Menu, Type FROM tbl_InternalMenu WHERE 1=1";
                    if (!string.IsNullOrEmpty(mealType))
                    {
                        query += " AND Meal = @mealType";
                    }
                    if (!string.IsNullOrEmpty(type))
                    {
                        query += " AND Type = @type";
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(query, con);
                    if (!string.IsNullOrEmpty(mealType))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@mealType", mealType);
                    }
                    if (!string.IsNullOrEmpty(type))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@type", type);
                    }

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // Bind the filtered data to the DataGridView
                    dgv_FD.DataSource = dt;

                    // Configure DataGridView properties
                    dgv_FD.ReadOnly = false;
                    if (dgv_FD.Columns.Contains("id"))
                    {
                        dgv_FD.Columns["id"].ReadOnly = true; // Set column id to read-only
                    }
                    dgv_FD.AllowUserToAddRows = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading menu items: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Style the DataGridView
            StyleDataGridView(dgv_FD);
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
                dgv.Columns["id"].ReadOnly = true; // Set column id to read-only
                dgv.Columns["id"].Width = 35; // Set a smaller width for the id column (e.g., 50 pixels)
            }
            if (dgv.Columns.Contains("Meal"))
            {
                dgv.Columns["Meal"].Visible = false; // Hide Meal column
            }
            if (dgv.Columns.Contains("Type"))
            {
                dgv.Columns["Type"].Visible = false; // Hide Type column
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

        private void UC_InternalMenu_Load(object sender, EventArgs e)
        {
        }

        private void cbMealType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get selected values from both ComboBoxes
            string mealType = cmbMeal.SelectedItem?.ToString();
            string type = cmbType.SelectedItem?.ToString();

            // Load menu items with current selections
            LoadMenuItems(mealType, type);

            // If no selections are made, clear the DataGridView
            if (string.IsNullOrEmpty(mealType) && string.IsNullOrEmpty(type))
            {
                dgv_FD.DataSource = null;
            }
        }

        private void cmbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Get selected values from both ComboBoxes
            string mealType = cmbMeal.SelectedItem?.ToString();
            string type = cmbType.SelectedItem?.ToString();

            // Load menu items with current selections
            LoadMenuItems(mealType, type);

            // If no selections are made, clear the DataGridView
            if (string.IsNullOrEmpty(mealType) && string.IsNullOrEmpty(type))
            {
                dgv_FD.DataSource = null;
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
            Form_Home.sharedLabel.Text = "Admin > Meal Request";
            Form_Home.sharedButton4.Visible = true;
            Form_Home.sharedButton5.Visible = true;
            //Form_Home.sharedButton6.Visible = true;
            UC_Meal_Food ug = new UC_Meal_Food(EventDetails, EventTime, DeliveryTime, loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void gbExternal_Enter(object sender, EventArgs e)
        {
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void btSExternal_Click(object sender, EventArgs e)
        {
            // Check if a meal is selected
            if (cmbMeal.Enabled && cmbMeal.Visible && cmbMeal.SelectedItem == null)
            {
                MessageBox.Show("Meal is required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if a type is selected
            if (cmbType.Enabled && cmbType.Visible && cmbType.SelectedItem == null)
            {
                MessageBox.Show("Type is required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if the DataGridView has data
            if (dgv_FD.DataSource == null || ((DataTable)dgv_FD.DataSource).Rows.Count == 0)
            {
                MessageBox.Show("No menu items to save. Please add items to the menu.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT id, Meal, Menu, Type FROM tbl_InternalMenu", con);
                    SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                    DataTable dt = (DataTable)dgv_FD.DataSource;

                    adapter.Update(dt);

                    MessageBox.Show("Menu updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gbExternal_Enter_1(object sender, EventArgs e)
        {
        }

        private void dgv_FD_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            string mealType = cmbMeal.SelectedItem?.ToString();
            string type = cmbType.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(mealType) && !string.IsNullOrEmpty(type))
            {
                foreach (DataGridViewRow row in dgv_FD.Rows)
                {
                    if (row.IsNewRow) continue; // Skip the new row placeholder
                    if (row.Cells["Meal"].Value == null || string.IsNullOrEmpty(row.Cells["Meal"].Value.ToString()))
                    {
                        row.Cells["Meal"].Value = mealType;
                    }
                    if (row.Cells["Type"].Value == null || string.IsNullOrEmpty(row.Cells["Type"].Value.ToString()))
                    {
                        row.Cells["Type"].Value = type;
                    }
                }
            }
        }
    }
}