using HRAdmin.Forms;
using System;
using HRAdmin.Components;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data.SqlClient;
using System.Configuration;

namespace HRAdmin.UserControl
{
    public partial class UC_C_InspectionLog : System.Windows.Forms.UserControl
    {
        public UC_C_InspectionLog()
        {
            InitializeComponent();
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
            Form_Home.sharedLabel.Text = "Admin > Car Reservation > Inspection";
             UC_C_Inspection ug = new UC_C_Inspection(UserSession.LoggedInUser);
            addControls(ug);
        }
        private void UC_C_InspectionLog_Load(object sender, EventArgs e)
        {
            LoadInspectionHistory();
        }
        private void LoadInspectionHistory()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                con.Open();

                string query = @"
            SELECT 
                ID,
                Driver,
                Brakes,
                Signal_light,
                Head_light,
                Body,
                Front_Bumper,
                Rear_Bumper,
                View_Mirror,
                Tyres,
                Others,
                Milleage,
                Actual_TimeIN,
                DateInspect,
                CarType,
                Person
            FROM tbl_CarInspection
            WHERE DateInspect BETWEEN @StartDate AND @EndDate
              AND (@Driver = '' OR Driver = @Driver)
              AND (@CarType = '' OR CarType = @CarType)
            ORDER BY DateInspect DESC, Actual_TimeIN DESC;";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // Dates
                    cmd.Parameters.AddWithValue("@StartDate", StartDate.Value.Date);
                    cmd.Parameters.AddWithValue("@EndDate", EndDate.Value.Date);

                    // Driver filter
                    string driver = cmb_Driver.SelectedIndex >= 0 ? cmb_Driver.SelectedItem.ToString() : "";
                    cmd.Parameters.AddWithValue("@Driver", driver);

                    // Car filter
                    string car = cmbCar.SelectedIndex >= 0 ? cmbCar.SelectedItem.ToString() : "";
                    cmd.Parameters.AddWithValue("@CarType", car);

                    // Load data
                    DataTable dt = new DataTable();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;
                }

                // ===== Column Headers =====
                dataGridView1.Columns["ID"].HeaderText = "Inspection ID";
                dataGridView1.Columns["Driver"].HeaderText = "Driver Name";
                dataGridView1.Columns["Brakes"].HeaderText = "Brakes Condition";
                dataGridView1.Columns["Signal_light"].HeaderText = "Signal Lights";
                dataGridView1.Columns["Head_light"].HeaderText = "Headlights";
                dataGridView1.Columns["Body"].HeaderText = "Car Body";
                dataGridView1.Columns["Front_Bumper"].HeaderText = "Front Bumper";
                dataGridView1.Columns["Rear_Bumper"].HeaderText = "Rear Bumper";
                dataGridView1.Columns["View_Mirror"].HeaderText = "Side Mirrors";
                dataGridView1.Columns["Tyres"].HeaderText = "Tyres Condition";
                dataGridView1.Columns["Others"].HeaderText = "Other Issues";
                dataGridView1.Columns["Milleage"].HeaderText = "Mileage";
                dataGridView1.Columns["Actual_TimeIN"].HeaderText = "Time In";
                dataGridView1.Columns["DateInspect"].HeaderText = "Inspection Date";
                dataGridView1.Columns["CarType"].HeaderText = "Car Type";
                dataGridView1.Columns["Person"].HeaderText = "Inspector";

                // ===== Styling =====
                dataGridView1.DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Regular),
                    //ForeColor = System.Drawing.Color.MidnightBlue
                };

                dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold),
                    //ForeColor = System.Drawing.Color.MidnightBlue
                };


                // ===== Column Sizes =====
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    col.Width = col.Index == 0 ? 80 : 120; // First column smaller
                }

                // Optional: Adjust row height for better readability
                dataGridView1.RowTemplate.Height = 28;
            }
        }
        private void StartDate_ValueChanged(object sender, EventArgs e)
        {
            LoadInspectionHistory();
        }
        private void EndDate_ValueChanged(object sender, EventArgs e)
        {
            LoadInspectionHistory();
        }
        private void cmb_Driver_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadInspectionHistory();
        }
        private void cmbCar_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadInspectionHistory();
        }
        private void btnSubmit_Click(object sender, EventArgs e)
        {

        }
    }
}
