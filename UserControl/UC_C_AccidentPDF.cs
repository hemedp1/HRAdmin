using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
//using CrystalDecisions.Shared;
using HRAdmin.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfiumViewer;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Font = iTextSharp.text.Font;
using Rectangle = iTextSharp.text.Rectangle;
using WinFormsApp = System.Windows.Forms.Application;


namespace HRAdmin.UserControl
{

    public partial class UC_C_AccidentPDF : System.Windows.Forms.UserControl
    {
        private bool isFirstLoad = true;
        private string loggedInUser;
        private string loggedInIndex;
        private string loggedInDepart;
        private string loggedInfullName;
        public UC_C_AccidentPDF(string username, string Index, string Depart, string fullName)
        {
            loggedInUser = username;
            loggedInIndex = Index;
            loggedInDepart = Depart;
            loggedInfullName = fullName;
            InitializeComponent();
            Load += UC_C_AccidentPDF_Load;
            loadfilter();
            dTDayPDF.ValueChanged += dTDayPDF_ValueChanged;
        }
        private void UC_C_AccidentPDF_Load(object sender, EventArgs e)
        {
            loadfilter();
            isFirstLoad = false;
            //dTDayPDF.ValueChanged += dTDayPDF_ValueChanged;
        }
        private void cmbDriverNamePDF_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadfilter();
        }
        private void cmbDepartPDF_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadDriver();
        }
        private void dTDayPDF_ValueChanged(object sender, EventArgs e)
        {
            loadDepart();
        }
        private void loadDepart()
        {
            string reportDate = dTDayPDF.Value.ToString("yyyy/MM/dd");
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    string query = "SELECT DISTINCT Dept FROM tbl_AccidentCar where DateReport = @DateReport";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@DateReport", SqlDbType.NVarChar).Value = reportDate;


                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        cmbDepartPDF.DataSource = null;  // Clear previous binding
                        cmbDepartPDF.DataSource = dt;
                        cmbDepartPDF.DisplayMember = "Dept";
                        cmbDepartPDF.ValueMember = "Dept";
                        //cmbDriverNamePDF.SelectedIndex = -1;  // Reset selection
                    }
                }
            }
            catch
            {

            }
        }
        private void  loadDriver()
        {
            string reportDate = dTDayPDF.Value.ToString("yyyy/MM/dd");
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    string query = "SELECT DISTINCT DriverInternal FROM tbl_AccidentCar where Dept = @Dept AND DateReport = @DateReport";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.Add("@Dept", SqlDbType.VarChar).Value = cmbDepartPDF.Text;
                        cmd.Parameters.Add("@DateReport", SqlDbType.NVarChar).Value = reportDate;


                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();

                        da.Fill(dt);

                        cmbDriverNamePDF.DataSource = null;  // Clear previous binding
                        cmbDriverNamePDF.DataSource = dt;
                        cmbDriverNamePDF.DisplayMember = "DriverInternal";
                        cmbDriverNamePDF.ValueMember = "DriverInternal";
                        //cmbDriverNamePDF.SelectedIndex = -1;  // Reset selection
                    }
                }
            }
            catch 
            {

            }
        }
        private void loadfilter()
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
            {
                try
                {
                    con.Open();

                    string query = @"
            SELECT 
                DateReport,
                DriverInternal,
                IndexNo,
                Dept,
                Car,
                CheckStatus,
                ISNULL(CheckedBy, 'Pending') AS CheckedBy,
                ISNULL(CONVERT(varchar, DateCheck, 23), 'Pending') AS DateCheck,
                ApproveStatus,
                ISNULL(ApproveBy, 'Pending') AS ApproveBy,
                ISNULL(CONVERT(varchar, DateApprove, 23), 'Pending') AS DateApprove
            FROM tbl_AccidentCar";

                    // Filtering logic
                    List<string> filters = new List<string>();
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    if (!isFirstLoad && dTDayPDF != null)
                    {
                        filters.Add("DateReport = @DateReport");
                        parameters.Add(new SqlParameter("@DateReport", dTDayPDF.Value.ToString("yyyy/MM/dd")));
                    }


                    if (cmbDepartPDF.SelectedValue != null)
                    {
                        filters.Add("Dept = @Dept");
                        parameters.Add(new SqlParameter("@Dept", cmbDepartPDF.SelectedValue.ToString()));
                    }

                    if (cmbDriverNamePDF.SelectedValue != null)
                    {
                        filters.Add("DriverInternal = @DriverInternal");
                        parameters.Add(new SqlParameter("@DriverInternal", cmbDriverNamePDF.SelectedValue.ToString()));
                    }

                    if (filters.Count > 0)
                    {
                        query += " WHERE " + string.Join(" AND ", filters);
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.Add(param);
                        }

                        DataTable dt = new DataTable();
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        dataGridView1.Columns.Clear();
                        dataGridView1.AutoGenerateColumns = false;
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                        dataGridView1.ScrollBars = ScrollBars.Both;

                        dataGridView1.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                        {
                            Font = new System.Drawing.Font("Arial", 11, FontStyle.Bold),
                        };

                        string[] columnNames = {
                    "DateReport", "DriverInternal", "IndexNo", "Dept", "Car",
                    "CheckStatus", "CheckedBy", "DateCheck", "ApproveStatus",
                    "ApproveBy", "DateApprove"
                };

                        foreach (var col in columnNames)
                        {
                            string headerText;
                            switch (col)
                            {
                                case "DateReport":
                                    headerText = "Date Report";
                                    break;
                                case "DriverInternal":
                                    headerText = "Driver";
                                    break;
                                case "IndexNo":
                                    headerText = "Index No";
                                    break;
                                case "Dept":
                                    headerText = "Department";
                                    break;
                                case "Car":
                                    headerText = "Car";
                                    break;
                                case "CheckStatus":
                                    headerText = "Admin Status Check";
                                    break;
                                case "CheckedBy":
                                    headerText = "Checked By";
                                    break;
                                case "DateCheck":
                                    headerText = "Checked Date";
                                    break;
                                case "ApproveStatus":
                                    headerText = "Admin HOD Status Approval";
                                    break;
                                case "ApproveBy":
                                    headerText = "Approve By";
                                    break;
                                case "DateApprove":
                                    headerText = "Approve Date";
                                    break;
                                default:
                                    headerText = col.Replace("_", " ");
                                    break;
                            }

                            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn()
                            {
                                HeaderText = headerText,
                                DataPropertyName = col,
                                Width = 170,
                                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                                SortMode = DataGridViewColumnSortMode.Automatic,
                                DefaultCellStyle = new DataGridViewCellStyle
                                {
                                    ForeColor = Color.MidnightBlue,
                                    Font = new System.Drawing.Font("Arial", 11),
                                }
                            });
                        }

                        dataGridView1.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading accident reports: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void ConfigureDataGridView(DataGridView dataGridView1, DataTable data, Dictionary<string, (string HeaderText, int Width)> columnConfig)
        {
            dataGridView1.Invoke((MethodInvoker)delegate
            {
                dataGridView1.SuspendLayout();
                dataGridView1.Columns.Clear();
                dataGridView1.DataSource = null;

                // General grid settings
                dataGridView1.AutoGenerateColumns = false;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                dataGridView1.ScrollBars = ScrollBars.Both;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToDeleteRows = false;
                dataGridView1.ReadOnly = true;

                // Style configuration
                var headerStyle = new DataGridViewCellStyle
                {
                    Font = new System.Drawing.Font("Arial", 11, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };

                var cellStyle = new DataGridViewCellStyle
                {
                    Font = new System.Drawing.Font("Arial", 11),
                    ForeColor = Color.MidnightBlue,
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                };
                dataGridView1.RowHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new System.Drawing.Font("Arial", 11, FontStyle.Bold),
                    ForeColor = Color.Black,
                    BackColor = Color.LightGray,
                    SelectionBackColor = Color.DarkOrange,
                    SelectionForeColor = Color.White
                };
                dataGridView1.ColumnHeadersDefaultCellStyle = headerStyle;
                dataGridView1.DefaultCellStyle = cellStyle;

                // Add columns
                foreach (DataColumn column in data.Columns)
                {
                    if (columnConfig.TryGetValue(column.ColumnName, out var config))
                    {
                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                        {
                            Name = column.ColumnName,
                            HeaderText = config.HeaderText,
                            DataPropertyName = column.ColumnName,
                            Width = config.Width,
                            DefaultCellStyle = cellStyle
                        });
                    }
                    else
                    {
                        // Default column configuration
                        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                        {
                            Name = column.ColumnName,
                            HeaderText = column.ColumnName.Replace("_", " "),
                            DataPropertyName = column.ColumnName,
                            Width = 130,
                            DefaultCellStyle = cellStyle
                        });
                    }
                }

                // Set data source and resume layout
                dataGridView1.DataSource = data;
                dataGridView1.ResumeLayout();
            });
        }
        private void btnExportPDF_Click(object sender, EventArgs e)
        {
            string reportDate = dTDayPDF.Value.ToString("yyyy/MM/dd");
            string department = cmbDepartPDF.Text.Trim();
            string driveName = cmbDriverNamePDF.Text.Trim();

            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM tbl_AccidentCar 
                         WHERE DateReport = @DateReport AND DriverInternal = @DriverInternal AND Dept = @Dept";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@DateReport", reportDate);
                    cmd.Parameters.AddWithValue("@DriverInternal", driveName);
                    cmd.Parameters.AddWithValue("@Dept", department);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No Record to Export!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     pull from database
                    
                    //Driver detail

                    string c1 = "";
                    if (DateTime.TryParse(dt.Rows[0]["DateReport"]?.ToString(), out DateTime DateReport))
                    {
                        c1 = DateReport.ToString("dd.MM.yyyy");
                    }
                    string c2 = string.IsNullOrWhiteSpace(dt.Rows[0]["Car"]?.ToString()) || dt.Rows[0]["Car"].ToString() == "0" ? "-" : dt.Rows[0]["Car"].ToString();
                    string c3 = string.IsNullOrWhiteSpace(dt.Rows[0]["DriverInternal"]?.ToString()) || dt.Rows[0]["DriverInternal"].ToString() == "0" ? "-" : dt.Rows[0]["DriverInternal"].ToString();
                    string c4 = string.IsNullOrWhiteSpace(dt.Rows[0]["Dept"]?.ToString()) || dt.Rows[0]["Dept"].ToString() == "0" ? "-" : dt.Rows[0]["Dept"].ToString();
                    string c5 = string.IsNullOrWhiteSpace(dt.Rows[0]["IndexNo"]?.ToString()) || dt.Rows[0]["IndexNo"].ToString() == "0" ? "-" : dt.Rows[0]["IndexNo"].ToString();

                    //External driver detail

                    string c6 = dt.Rows[0]["DriverExternal"]?.ToString();
                    c6 = string.IsNullOrWhiteSpace(c6) || c6 == "0" ? "-" : c6;

                    int noOfVehicle = dt.Rows[0]["NoofVehicle"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["NoofVehicle"]) : 0;
                    string c7 = noOfVehicle == 0 ? "-" : noOfVehicle.ToString();

                    string c8 = string.IsNullOrWhiteSpace(dt.Rows[0]["PlatNo"]?.ToString()) || dt.Rows[0]["PlatNo"].ToString() == "0" ? "-" : dt.Rows[0]["PlatNo"].ToString();
                    string c9 = string.IsNullOrWhiteSpace(dt.Rows[0]["VehicleType"]?.ToString()) || dt.Rows[0]["VehicleType"].ToString() == "0" ? "-" : dt.Rows[0]["VehicleType"].ToString();
                    string c10 = string.IsNullOrWhiteSpace(dt.Rows[0]["InsuranceClass"]?.ToString()) || dt.Rows[0]["InsuranceClass"].ToString() == "0" ? "-" : dt.Rows[0]["InsuranceClass"].ToString();
                    string c11 = string.IsNullOrWhiteSpace(dt.Rows[0]["InsuranceComp"]?.ToString()) || dt.Rows[0]["InsuranceComp"].ToString() == "0" ? "-" : dt.Rows[0]["InsuranceComp"].ToString();
                    string c12 = string.IsNullOrWhiteSpace(dt.Rows[0]["PolicyNo"]?.ToString()) || dt.Rows[0]["PolicyNo"].ToString() == "0" ? "-" : dt.Rows[0]["PolicyNo"].ToString();
                    string c13 = string.IsNullOrWhiteSpace(dt.Rows[0]["Address"]?.ToString()) || dt.Rows[0]["Address"].ToString() == "0" ? "-" : dt.Rows[0]["Address"].ToString();
                    int Tel = dt.Rows[0]["Tel"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["Tel"]) : 0;
                    string c14 = Tel == 0 ? "-" : Tel.ToString();
                    long IC = dt.Rows[0]["IC"] != DBNull.Value ? Convert.ToInt64(dt.Rows[0]["IC"]) : 0;
                    string c15 = IC == 0 ? "-" : IC.ToString();

                    //Accident report
                    string c16 = "";
                    if (DateTime.TryParse(dt.Rows[0]["DateofAccident"]?.ToString(), out DateTime DateofAccident))
                    {
                        c16 = DateofAccident.ToString("dd.MM.yyyy");
                    }
                    string c17 = dt.Rows[0]["Place"]?.ToString() ?? "-";
                    string c18 = "";
                    if (DateTime.TryParse(dt.Rows[0]["Time"]?.ToString(), out DateTime Time))
                    {
                        c18 = Time.ToString("hh:mm");
                    }
                    string c19 = dt.Rows[0]["Explanation"]?.ToString() ?? "-";
                    byte[] imageData = dt.Rows[0]["Attachment"] as byte[];

                    //PM
                    string c20 = dt.Rows[0]["PM"]?.ToString() ?? "-";

                    //Police Report
                    string c21 = dt.Rows[0]["PoliceStation"]?.ToString() ?? "-";
                    string c22 = dt.Rows[0]["ReportNo"]?.ToString() ?? "-";

                    //GA Remarks
                    string c23 = string.IsNullOrWhiteSpace(dt.Rows[0]["Remarks"]?.ToString()) || dt.Rows[0]["Remarks"].ToString() == "0" ? "-" : dt.Rows[0]["Remarks"].ToString();
                    string c24 = dt.Rows[0]["CheckedBy"]?.ToString() ?? "";
                    string c25= "";
                    if (DateTime.TryParse(dt.Rows[0]["DateCheck"]?.ToString(), out DateTime DateCheck))
                    {
                        c25 = DateCheck.ToString("dd.MM.yyyy");
                    }
                    string c26 = string.IsNullOrWhiteSpace(dt.Rows[0]["ApproveBy"]?.ToString()) || dt.Rows[0]["ApproveBy"].ToString() == "0" ? "Pending" : dt.Rows[0]["ApproveBy"].ToString();
                    string c27 = "";
                    if (dt.Rows[0]["DateApprove"] != null && DateTime.TryParse(dt.Rows[0]["DateApprove"].ToString(), out DateTime DateApprove))
                    {
                        c27 = DateApprove.ToString("dd.MM.yyyy");
                    }
                    string c28 = string.IsNullOrWhiteSpace(dt.Rows[0]["CheckedByDepartment"]?.ToString()) || dt.Rows[0]["CheckedByDepartment"].ToString() == "0" ? "Pending" : dt.Rows[0]["CheckedByDepartment"].ToString();
                    string c29 = string.IsNullOrWhiteSpace(dt.Rows[0]["ApproveByDepartment"]?.ToString()) || dt.Rows[0]["ApproveByDepartment"].ToString() == "0" ? "Pending" : dt.Rows[0]["ApproveByDepartment"].ToString();
                    
                    string cCheck = (string.IsNullOrEmpty(c24) || string.IsNullOrEmpty(c28) || string.IsNullOrEmpty(c25)) ? "Pending" : $"{c24} - {c28} - {c25}";
                    string cApp = (string.IsNullOrEmpty(c26) || string.IsNullOrEmpty(c29) || string.IsNullOrEmpty(c27))? "Pending": $"{c26} - {c29} - {c27}";
                    



                    MemoryStream memoryStream = new MemoryStream();
                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 10f);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                    writer.CloseStream = false;
                    writer.PageEvent = new WatermarkPageEvent();
                    pdfDoc.Open();


                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Logo
                    string imagePath = Path.Combine(WinFormsApp.StartupPath, "Img", "hosiden.jpg");

                    if (File.Exists(imagePath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imagePath);
                        logo.ScaleToFit(100f, 100f);
                        logo.Alignment = Element.ALIGN_CENTER;

                        Paragraph p = new Paragraph();
                        p.SpacingBefore = 0f;
                        p.Alignment = Element.ALIGN_CENTER;
                        p.Add(logo);

                        pdfDoc.Add(p);
                    }


                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Title
                    var titleFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.NORMAL);
                    Paragraph title = new Paragraph("HOSIDEN ELECTRONICS (M) SDN BHD (198901000700)", titleFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 1f
                    };
                    pdfDoc.Add(title);
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     address
                    var addresseFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL);
                    Paragraph address = new Paragraph("Lot 1, Jalan P/1A, Bangi Industrial Estate, 43650 Bandar Baru Bangi, Selangor, Malaysia", addresseFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 10f
                    };
                    pdfDoc.Add(address);
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Subtitle
                    var subtitleFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 12);
                    Paragraph subtitle = new Paragraph("COMPANY VEHICLE ACCIDENT REPORT", subtitleFont)
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 15f
                    };
                    pdfDoc.Add(subtitle);


                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section GA contents
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section GA contents
                    PdfPTable verifyTable = new PdfPTable(6);
                    Font font = FontFactory.GetFont("Arial", 9f); // Change size here
                    verifyTable.WidthPercentage = 100;
                    verifyTable.SpacingBefore = 5f;
                    verifyTable.SpacingAfter = 7f;
                    verifyTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    verifyTable.SetWidths(new float[] { 0.2f, 0.1f, 4.2f, 0.9f, 0.1f, 2.8f });

                    columnSetup1(verifyTable, "", "", "Checked by", cCheck, font);
                    columnSetup1(verifyTable, "", "", "Approved by", cApp, font);  // c26 + " - " + c29 + " - " + c27                                   700
                    string imagePath1 = Path.Combine(WinFormsApp.StartupPath, "Img", "logo.png");
                    if (File.Exists(imagePath1) && (!string.IsNullOrEmpty(cApp) && cApp != "Pending")) // Only add watermark if approved or pending
                    {
                        iTextSharp.text.Image watermark = iTextSharp.text.Image.GetInstance(imagePath1);
                        float xPosition = pdfDoc.PageSize.Width * 0.70f; // Approximately 75% of page width
                        float yPosition = pdfDoc.PageSize.Height - 186f;  // Approximate Y position
                        float width = 80f;
                        float height = 80f;
                        watermark.SetAbsolutePosition(xPosition, yPosition);
                        watermark.ScaleToFit(width, height);

                        PdfContentByte under = writer.DirectContentUnder;
                        PdfGState gState = new PdfGState();
                        gState.FillOpacity = 0.05f;
                        under.SetGState(gState);
                        under.AddImage(watermark);
                    }

                    pdfDoc.Add(verifyTable);

                    pdfDoc.Add(new Paragraph("\n"));





                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Driver's details
                    var DRIVER_DETAILSFONT = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10);
                    Chunk DRIVERDETAILS = new Chunk("Driver's details", DRIVER_DETAILSFONT);
                    PdfPTable table = new PdfPTable(1);
                    table.WidthPercentage = 100;

                    PdfPCell cell = new PdfPCell(new Phrase(DRIVERDETAILS));
                    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                    
                    cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cell.Padding = 4f;
                    cell.PaddingBottom = 6f;
                    table.AddCell(cell);
                    pdfDoc.Add(table);
                    
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Driver's details contents

                    PdfPTable infoTable = new PdfPTable(6);
                    infoTable.WidthPercentage = 100;
                    infoTable.SpacingBefore = 5f;
                    infoTable.SpacingAfter = 7f;
                    //Font font = FontFactory.GetFont("Arial", 9f); // Change size here
                    infoTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    infoTable.SetWidths(new float[] { 0.7f, 0.1f, 2.2f, 0.9f, 0.1f, 2.2f });

                    columnSetup1(infoTable, "Date Report", c1, "Vehicle Plate No. (Vehicle Model)", c2, font);
                    columnSetup1(infoTable, "Driver Name", c3, "Department", c4, font);
                    columnSetup1(infoTable, "Employee No.", c5, "", "", font);
                    pdfDoc.Add(infoTable);
                    //pdfDoc.Add(new Paragraph("\n"));
                    //pdfDoc.Add(new Paragraph("\n"));                //+++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Spacing
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Vehicle involved

                    var involeDRIVER_DETAILSFONT = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10);
                    
                    Chunk involveDRIVERDETAILS = new Chunk("Involvement of Vehicle (if applicable)", involeDRIVER_DETAILSFONT);
                    PdfPTable btable = new PdfPTable(1);
                    btable.WidthPercentage = 100;
                    
                    PdfPCell cellq = new PdfPCell(new Phrase(involveDRIVERDETAILS));
                    cellq.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cellq.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cellq.VerticalAlignment = Element.ALIGN_BASELINE;
                    cellq.Padding = 4f;
                    cellq.PaddingBottom = 6f;

                    btable.AddCell(cellq);
                    pdfDoc.Add(btable);


                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Vehicle involved contents
                    PdfPTable vehicleTable = new PdfPTable(6);
                    vehicleTable.WidthPercentage = 100;
                    vehicleTable.SpacingBefore = 5f;
                    vehicleTable.SpacingAfter = 7f;
                    vehicleTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    vehicleTable.SetWidths(new float[] { 1.2f, 0.1f, 2.2f, 1.1f, 0.1f, 2.2f });

                    columnSetup1(vehicleTable, "Driver Name", c6, "Tel No.", c14, font);
                    columnSetup1(vehicleTable, "Driver I/C No.", c15, "Insurance Class", c10, font);
                    columnSetup1(vehicleTable, "No. of Vehicle Involved", c7, "Insurance Company", c11, font);
                    columnSetup1(vehicleTable, "Vehicle Plate No.", c8, "Policy No.", c12, font);
                    columnSetup1(vehicleTable, "Vehicle Type", c9, "Address", c13, font);
                    pdfDoc.Add(vehicleTable);

                    //pdfDoc.Add(new Paragraph("\n"));                //+++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Spacing
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Accident Report

                    var Accident_ReportFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10);
                    Chunk Accident_Report = new Chunk("Accident Report", Accident_ReportFont);
                    PdfPTable Accident_Reporttable = new PdfPTable(1);
                    Accident_Reporttable.WidthPercentage = 100;

                    PdfPCell cellAccident_Report = new PdfPCell(new Phrase(Accident_Report));
                    cellAccident_Report.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cellAccident_Report.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cellAccident_Report.VerticalAlignment = Element.ALIGN_BOTTOM;
                    cellAccident_Report.Padding = 4f;
                    cellAccident_Report.PaddingBottom = 6f;

                    Accident_Reporttable.AddCell(cellAccident_Report);
                    pdfDoc.Add(Accident_Reporttable);

                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Accident Report contents
                    PdfPTable AccidentReportTable = new PdfPTable(6);
                    AccidentReportTable.WidthPercentage = 100;
                    AccidentReportTable.SpacingBefore = 5f;
                    AccidentReportTable.SpacingAfter = 7f;
                    AccidentReportTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    AccidentReportTable.SetWidths(new float[] {1.3f, 0.1f, 6.0f, 0f, 0f, 0f });

                    columnSetup1(AccidentReportTable, "Date | Time of Accident", c16 + " | " + c18, "", "", font);
                    columnSetup1(AccidentReportTable, "Location", c17, "", "", font);
                    columnSetup1(AccidentReportTable, "Details of the Accident", c19, "", "", font);
                    pdfDoc.Add(AccidentReportTable);

                    //pdfDoc.Add(new Paragraph("\n"));                //+++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Spacing
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section PM

                    var PMFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10);
                    Chunk PM = new Chunk("Preventive Measures to Avoid Recurrence", PMFont);
                    PdfPTable PMtable = new PdfPTable(1);
                    PMtable.WidthPercentage = 100;

                    PdfPCell cellPM = new PdfPCell(new Phrase(PM));
                    cellPM.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cellPM.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cellPM.VerticalAlignment = Element.ALIGN_BOTTOM;
                    cellPM.Padding = 4f;
                    cellPM.PaddingBottom = 6f;

                    PMtable.AddCell(cellPM);
                    pdfDoc.Add(PMtable);

                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section PM contents
                    PdfPTable pmTable = new PdfPTable(6);
                    pmTable.WidthPercentage = 100;
                    pmTable.SpacingBefore = 5f;
                    pmTable.SpacingAfter = 7f;
                    pmTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    pmTable.SetWidths(new float[] { 0.6f, 0.1f, 6.0f, 0f, 0f, 0f });

                    columnSetup1(pmTable, "Comment", c20, "", "",font);
                    pdfDoc.Add(pmTable);

                    //pdfDoc.Add(new Paragraph("\n"));                //+++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Spacing
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Polce rep

                    var PoliceFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10);
                    Chunk Police = new Chunk("Police Report", PoliceFont);
                    PdfPTable Policetable = new PdfPTable(1);
                    Policetable.WidthPercentage = 100;

                    PdfPCell cellPolice = new PdfPCell(new Phrase(Police));
                    cellPolice.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cellPolice.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cellPolice.VerticalAlignment = Element.ALIGN_BOTTOM;
                    cellPolice.Padding = 4f;
                    cellPolice.PaddingBottom = 6f;

                    Policetable.AddCell(cellPolice);
                    pdfDoc.Add(Policetable);

                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Police rep contents
                    PdfPTable policeTable = new PdfPTable(6);
                    policeTable.WidthPercentage = 100;
                    policeTable.SpacingBefore = 5f;
                    policeTable.SpacingAfter = 7f;
                    policeTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    policeTable.SetWidths(new float[] { 0.7f, 0.1f, 2.2f, 0.5f, 0.1f, 2.2f });

                    columnSetup1(policeTable, "Police Station", c21, "Report No", c22, font);
                    pdfDoc.Add(policeTable);

                    //pdfDoc.Add(new Paragraph("\n"));                //+++++++++++++++++++++++++++++++++++++++++++++++++++++     Section Spacing
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section GA

                    var GAFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 10);
                    Chunk GA = new Chunk("GA Department Remarks", GAFont);
                    PdfPTable GAtable = new PdfPTable(1);
                    GAtable.WidthPercentage = 100;

                    PdfPCell cellGA = new PdfPCell(new Phrase(GA));
                    cellGA.BackgroundColor = BaseColor.LIGHT_GRAY;
                    cellGA.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cellGA.VerticalAlignment = Element.ALIGN_BOTTOM;
                    cellGA.Padding = 4f;
                    cellGA.PaddingBottom = 6f;

                    GAtable.AddCell(cellGA);
                    pdfDoc.Add(GAtable);

                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++     Section GA contents
                    PdfPTable GATable = new PdfPTable(6);
                    GATable.WidthPercentage = 100;
                    GATable.SpacingBefore = 5f;
                    GATable.SpacingAfter = 7f;
                    GATable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    GATable.SetWidths(new float[] { 0.6f, 0.1f, 6.0f, 0f, 0f, 0f });

                    columnSetup1(GATable, "Comment", c23, "","",font); // c24 + " - " + c28 + " - " + c25
                    //columnSetup1(GATable, "", "", "Checked by", cCheck, font);
                    //columnSetup1(GATable, "", "", "Approved by", cApp, font);  // c26 + " - " + c29 + " - " + c27
                    pdfDoc.Add(GATable);

                    

                    PdfPTable footerTbl = new PdfPTable(1);
                    footerTbl.TotalWidth = pdfDoc.PageSize.Width - pdfDoc.LeftMargin - pdfDoc.RightMargin;
                    footerTbl.DefaultCell.Border = Rectangle.NO_BORDER;

                    

                    PdfContentByte content = writer.DirectContent;
                    Rectangle rectangle = new Rectangle(pdfDoc.PageSize);

                    // Adjust rectangle to account for margins
                    rectangle.Left += pdfDoc.LeftMargin;
                    rectangle.Right -= pdfDoc.RightMargin;
                    rectangle.Top -= pdfDoc.TopMargin;
                    rectangle.Bottom += pdfDoc.BottomMargin;

                    // Draw the border
                    content.SetColorStroke(BaseColor.BLACK);
                    content.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
                    content.Stroke();

                    // Add "Generated on [date]" text just above the bottom line
                    BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    content.BeginText();
                    content.SetFontAndSize(baseFont, 9); // Font size 10

                    // Calculate Y-position (5 units above the bottom line)
                    float textY = rectangle.Bottom + 5;

                    // Left-aligned "Generated on [date]"
                    string generatedText = " This is a computer generated, no signature is required | Generated on " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    content.ShowTextAligned(PdfContentByte.ALIGN_LEFT, generatedText, rectangle.Left, textY, 0);

                    content.EndText();


                    pdfDoc.Close();

                    memoryStream.Position = 0;

                    PdfViewer viewerControl = new PdfViewer();
                    viewerControl.LoadPdfFromStream(memoryStream);

                    Form previewForm = new Form
                    {
                        Text = "Preview PDF",
                        WindowState = FormWindowState.Maximized
                    };
                    viewerControl.Dock = DockStyle.Fill;
                    previewForm.Controls.Add(viewerControl);
                    previewForm.ShowDialog();
                }
            }
        }

        public class WatermarkPageEvent : PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                string imagePath = Path.Combine(WinFormsApp.StartupPath, "Img", "logo.png");
                if (File.Exists(imagePath))
                {
                    iTextSharp.text.Image watermark = iTextSharp.text.Image.GetInstance(imagePath);
                    float pageWidth = document.PageSize.Width;
                    float pageHeight = document.PageSize.Height;
                    float scaleFactor = 0.7f; // Reduce size to 70% of the page dimensions
                    watermark.ScaleToFit(pageWidth * scaleFactor, pageHeight * scaleFactor); // Scale to a smaller size

                    watermark.RotationDegrees = 0; // Rotate for watermark effect

                    // Center the watermark
                    float x = (pageWidth - watermark.ScaledWidth) / 2;
                    float y = (pageHeight - watermark.ScaledHeight) / 2;
                    watermark.SetAbsolutePosition(x, y);

                    PdfContentByte under = writer.DirectContentUnder;
                    PdfGState gState = new PdfGState();
                    gState.FillOpacity = 0.05f; // Set opacity to 5% (0.0f to 1.0f)
                    under.SetGState(gState);
                    under.AddImage(watermark);
                }
            }
        }
        void columnSetup1(PdfPTable table, string label1, string value1, string label2, string value2, Font font)
        {
            AddLabelValuePair1(table, label1, value1, font);
            AddLabelValuePair1(table, label2, value2, font);
        }
        void AddLabelValuePair1(PdfPTable table, string label, string value, Font font)
        {
            if (!string.IsNullOrEmpty(label))
            {
                // Label
                PdfPCell labelCell = new PdfPCell(new Phrase(label, font));
                labelCell.HorizontalAlignment = Element.ALIGN_LEFT;
                labelCell.Border = Rectangle.NO_BORDER;
                labelCell.PaddingRight = 5f;
                table.AddCell(labelCell);

                // Colon
                PdfPCell colonCell = new PdfPCell(new Phrase(":", font));
                colonCell.HorizontalAlignment = Element.ALIGN_CENTER;
                colonCell.Border = Rectangle.NO_BORDER;
                table.AddCell(colonCell);
            }
            else
            {
                // Empty label and colon cells
                table.AddCell(new PdfPCell(new Phrase("", font)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase("", font)) { Border = Rectangle.NO_BORDER });
            }

            // Value
            PdfPCell valueCell = new PdfPCell(new Phrase(value ?? "", font));
            valueCell.HorizontalAlignment = Element.ALIGN_LEFT;
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.PaddingLeft = 5f;
            table.AddCell(valueCell);
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
    
            Form_Home.sharedLabel.Text = "Admin > Car Reservation > Accident";

            Form_Home.sharedbtn_AccidentPDF.Visible = true;
            UC_C_Accident ug = new UC_C_Accident(loggedInUser, loggedInIndex, loggedInDepart, loggedInfullName);
            addControls(ug);
        }
        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            // Reset department and driver filters
            cmbDepartPDF.SelectedIndex = -1; // Clear department selection
            cmbDriverNamePDF.DataSource = null; // Clear driver selection
            cmbDriverNamePDF.SelectedIndex = -1; // Ensure no driver is selected

            // Temporarily set isFirstLoad to true to bypass date filter in loadfilter
            bool originalIsFirstLoad = isFirstLoad;
            isFirstLoad = true;

            // Reload all data without filters
            loadfilter();

            // Restore original isFirstLoad value
            isFirstLoad = originalIsFirstLoad;
        }
    }
}
