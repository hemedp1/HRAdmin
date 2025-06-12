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

namespace HRAdmin.UserControl
{
    
    public partial class UC_MC_Issue : System.Windows.Forms.UserControl
    {
        public static Button shareBTNSUBMIT;
        private string loggedInUser;
        public static Panel sharedPanele;
        public static Button sharedBtnTest;

        public bool ShowButton1 { get; set; }  //1
        public UC_MC_Issue(string username)
        {
            InitializeComponent();
            LoadData();
            loggedInUser = username;
            shareBTNSUBMIT = btnSubmit;
            
            sharedPanele = panel1;
            
        }
       
        private void LoadData()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM tbl_dumm", con);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGridView1.DataSource = dt;

                dataGridView1.ReadOnly = false;
                if (dataGridView1.Columns.Contains("id"))
                {
                    dataGridView1.Columns["id"].ReadOnly = true;            //++++++++         set column id to read-only

                }
                dataGridView1.AllowUserToAddRows = true;                   //enable add row
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM tbl_dumm", con);
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter); //sqlcommandbuilder all in one, ada fx insert, udate, delete, CRUD Things

                DataTable dt = (DataTable)dataGridView1.DataSource;

                adapter.Update(dt); 
                MessageBox.Show("Data updated successfully!");

                LoadData(); // call balik loadData fr refresh fx
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {

        }
    }
}
