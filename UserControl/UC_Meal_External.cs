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
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Diagnostics;
using DrawingFont = System.Drawing.Font;
using WinFormsApp = System.Windows.Forms.Application;

namespace HRAdmin.UserControl
{
    public partial class UC_Meal_External : System.Windows.Forms.UserControl
    {
        private string eventDetails;
        private DateTime eventDate;
        private DateTime DeliveryTime;
        private string loggedInUser;
        private string loggedInDepart;
        private string EventDetails;
        private string EventTime;
        private string cmbOccasion = "External";
        private string pdfFilePath;

        public UC_Meal_External(string eventDescription, DateTime? eventDate, DateTime? deliveryTime, string username, string department)
        {
            InitializeComponent();
            loadmenu();
            loggedInUser = username;
            this.eventDetails = eventDescription;
            this.eventDate = eventDate ?? DateTime.Now;
            this.DeliveryTime = deliveryTime ?? this.eventDate;
            loggedInDepart = department;

            lblEvent2.Text = eventDetails;
            lblRequestDate2.Text = this.eventDate.ToString("dd.MM.yyyy");
            lblDeliveryDate2.Text = this.DeliveryTime.ToString("dd.MM.yyyy");

            // Ensure all DataGridViews are visible
            SetDataGridViewsVisibility(true);
        }

        private void SetDataGridViewsVisibility(bool visible)
        {
            // Breakfast
            dgvA_B_P.Visible = visible;
            dgvB_B_P.Visible = visible;
            dgvC_B_P.Visible = visible;

            // Lunch
            dgvA_L_B.Visible = visible;
            dgvB_L_B.Visible = visible;
            dgvC_L_B.Visible = visible;
            dgvA_L_P.Visible = visible;
            dgvB_L_P.Visible = visible;
            dgvC_L_P.Visible = visible;

            // Tea
            dgvA_T_P.Visible = visible;
            dgvB_T_P.Visible = visible;
            dgvC_T_P.Visible = visible;
        }

        private void btSExternal_Click(object sender, EventArgs e)
        {
            // Check if at least one package is selected
            bool isBreakfastPackageSelected = cmbB_Package.SelectedItem != null;
            bool isLunchPackageSelected = cmbL_Package.SelectedItem != null;
            bool isTeaPackageSelected = cmbT_Package.SelectedItem != null;

            if (!isBreakfastPackageSelected && !isLunchPackageSelected && !isTeaPackageSelected)
            {
                MessageBox.Show("Please select at least a package for Breakfast or Lunch or Tea.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate Breakfast section
            bool isBreakfastSelected = cmbB_Package.SelectedItem != null || !string.IsNullOrWhiteSpace(txtB_Pack.Text) || cmbB_DT.SelectedItem != null;
            if (isBreakfastSelected)
            {
                if (cmbB_Package.SelectedItem == null)
                {
                    MessageBox.Show("Package for Breakfast required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtB_Pack.Text))
                {
                    MessageBox.Show("No. of Pax Packing (Breakfast) required.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cmbB_DT.SelectedItem == null)
                {
                    MessageBox.Show("Delivery Time for Breakfast required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtE_Remark.Text))
                {
                    MessageBox.Show("Remark required.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cmb_DeliveryP.SelectedItem == null)
                {
                    MessageBox.Show("Delivery Place required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Validate Lunch section
            bool isLunchSelected = cmbL_Package.SelectedItem != null || !string.IsNullOrWhiteSpace(txtL_Buffet.Text) ||
                                  !string.IsNullOrWhiteSpace(txtL_Pack.Text) || cmbL_DT.SelectedItem != null;
            if (isLunchSelected)
            {
                if (cmbL_Package.SelectedItem == null)
                {
                    MessageBox.Show("Please select a package for Lunch.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtL_Buffet.Text))
                {
                    MessageBox.Show("No. of Pax Buffet (Lunch) required.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtL_Pack.Text))
                {
                    MessageBox.Show("No. of Pax Packing (Lunch) required.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cmbL_DT.SelectedItem == null)
                {
                    MessageBox.Show("Delivery Time for Lunch required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtE_Remark.Text))
                {
                    MessageBox.Show("Remark required.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Validate Tea section
            bool isTeaSelected = cmbT_Package.SelectedItem != null || !string.IsNullOrWhiteSpace(txtT_Pack.Text) || cmbT_DT.SelectedItem != null;
            if (isTeaSelected)
            {
                if (cmbT_Package.SelectedItem == null)
                {
                    MessageBox.Show("Please select a package for Tea.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtT_Pack.Text))
                {
                    MessageBox.Show("No. of Pax Packing (Tea) required.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (cmbT_DT.SelectedItem == null)
                {
                    MessageBox.Show("Delivery Time for Tea required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtE_Remark.Text))
                {
                    MessageBox.Show("Remark required.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string insertQuery = @"
INSERT INTO tbl_ExternalFoodOrder (OrderID, RequesterID, Department, OccasionType, RequestDate, DeliveryDate, EventDetails, B_Nofpax_P, B_DeliveryTime, L_Nofpax_B, L_Nofpax_P, L_DeliveryTime, T_Nofpax_P, T_DeliveryTime, Remark, BreakfastPackage, LunchPackage, TeaPackage, OrderType, DeliveryPlace) 
VALUES (@OrderID, @RequesterID, @Department, @OccasionType, @RequestDate, @DeliveryDate, @EventDetails, @B_Nofpax_P, @B_DeliveryTime, @L_Nofpax_B, @L_Nofpax_P, @L_DeliveryTime, @T_Nofpax_P, @T_DeliveryTime, @Remark, @BreakfastPackage, @LunchPackage, @TeaPackage, @OrderType, @DeliveryPlace)";

                SqlCommand insertCmd = new SqlCommand(insertQuery, con);

                string combinedValue = $"{DateTime.Now:ddMMyyyy_HHmmss}_{cmbB_Package.SelectedItem?.ToString() ?? (cmbL_Package.SelectedItem?.ToString() ?? (cmbT_Package.SelectedItem?.ToString() ?? ""))}";

                insertCmd.Parameters.AddWithValue("@OrderID", combinedValue);
                insertCmd.Parameters.AddWithValue("@RequesterID", loggedInUser);
                insertCmd.Parameters.AddWithValue("@Department", loggedInDepart);
                insertCmd.Parameters.AddWithValue("@OccasionType", cmbOccasion);
                insertCmd.Parameters.AddWithValue("@RequestDate", eventDate);
                insertCmd.Parameters.AddWithValue("@DeliveryDate", DeliveryTime);
                insertCmd.Parameters.AddWithValue("@EventDetails", eventDetails);
                insertCmd.Parameters.AddWithValue("@BreakfastPackage", cmbB_Package.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@B_Nofpax_P", string.IsNullOrEmpty(txtB_Pack.Text) ? (object)DBNull.Value : txtB_Pack.Text);
                insertCmd.Parameters.AddWithValue("@B_DeliveryTime", cmbB_DT.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@LunchPackage", cmbL_Package.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@L_Nofpax_B", string.IsNullOrEmpty(txtL_Buffet.Text) ? (object)DBNull.Value : txtL_Buffet.Text);
                insertCmd.Parameters.AddWithValue("@L_Nofpax_P", string.IsNullOrEmpty(txtL_Pack.Text) ? (object)DBNull.Value : txtL_Pack.Text);
                insertCmd.Parameters.AddWithValue("@L_DeliveryTime", cmbL_DT.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@TeaPackage", cmbT_Package.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@T_Nofpax_P", string.IsNullOrEmpty(txtT_Pack.Text) ? (object)DBNull.Value : txtT_Pack.Text);
                insertCmd.Parameters.AddWithValue("@T_DeliveryTime", cmbT_DT.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Remark", string.IsNullOrEmpty(txtE_Remark.Text) ? (object)DBNull.Value : txtE_Remark.Text);
                insertCmd.Parameters.AddWithValue("@OrderType", $"{cmbB_Package.SelectedItem?.ToString() ?? "-"},{cmbL_Package.SelectedItem?.ToString() ?? "-"},{cmbT_Package.SelectedItem?.ToString() ?? "-"}");
                insertCmd.Parameters.AddWithValue("@DeliveryPlace", cmb_DeliveryP.SelectedItem?.ToString() ?? (object)DBNull.Value);

                insertCmd.ExecuteNonQuery();

                // Generate and store PDF
                byte[] pdfBytes = GeneratePDF(combinedValue);
                StorePdfInDatabase(combinedValue, pdfBytes);

                // Show success message with "View in PDF" option
                DialogResult result = MessageBox.Show("Submitted Successfully! Would you like to view the order in PDF?",
                    "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    ViewPdf(pdfBytes);
                }

                // Return to main page
                CheckUserAccess(loggedInUser);

                Form_Home.sharedLabel.Text = "Admin > Meal Request > External";
                Form_Home.sharedButton4.Visible = false;
                Form_Home.sharedButton5.Visible = false;
                Form_Home.sharedButton6.Visible = false;
                UC_Meal_External ug = new UC_Meal_External(eventDetails, eventDate, eventDate, loggedInUser, loggedInDepart);
                addControls(ug);
            }
        }

        private void CheckUserAccess(string username)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString))
                {
                    con.Open();
                    string query = "SELECT AA, MA FROM tbl_Users WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string AA = reader["AA"].ToString();
                                string MA = reader["MA"].ToString();

                                if (AA == "1")
                                {
                                    Form_Home.sharedButton4.Visible = true;
                                    Form_Home.sharedButton5.Visible = true;
                                }
                                else if (MA == "2")
                                {
                                    Form_Home.sharedButton4.Visible = true;
                                    Form_Home.sharedButton5.Visible = true;
                                }
                                else
                                {
                                    Form_Home.sharedButton4.Visible = false;
                                    Form_Home.sharedButton5.Visible = false;
                                }
                            }
                            else
                            {
                                Form_Home.sharedButton4.Visible = false;
                                Form_Home.sharedButton5.Visible = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public byte[] GeneratePDF(string orderId)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4.Rotate(), 36f, 36f, 36f, 36f);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new PdfPageEventHelper();
                    document.Open();

                    // Define fonts
                    iTextSharp.text.Font titleFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font addressFont = FontFactory.GetFont("Helvetica", 8f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font titleFont1 = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font headerFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font bodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font italicBodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.ITALIC, BaseColor.BLACK);
                    iTextSharp.text.Font sectionTitleFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.BOLD, new BaseColor(0, 51, 102));
                    iTextSharp.text.Font mealsHeadingFont = FontFactory.GetFont("Helvetica", 13f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

                    // Define colors
                    BaseColor lightGray = new BaseColor(240, 240, 240);
                    BaseColor darkGray = new BaseColor(150, 150, 150);

                    // Add company logo or name
                    string imagePath = Path.Combine(WinFormsApp.StartupPath, "Img", "hosiden.jpg");
                    if (File.Exists(imagePath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imagePath);
                        logo.ScaleToFit(100f, 100f);
                        logo.Alignment = Element.ALIGN_CENTER;
                        document.Add(logo);
                    }
                    else
                    {
                        Paragraph companyPara = new Paragraph("Hosiden Electronics (M) Sdn Bhd", headerFont);
                        companyPara.Alignment = Element.ALIGN_CENTER;
                        document.Add(companyPara);
                    }

                    // Add title
                    Paragraph titlePara = new Paragraph();
                    titlePara.Add(new Chunk("HOSIDEN ELECTRONICS (M) SDN BHD (198901000700)\n", titleFont));
                    titlePara.Add(new Chunk("Lot 1, Jalan P/1A, Bangi Industrial Estate, 43650 Bandar Baru Bangi, Selangor, Malaysia\n", addressFont));
                    titlePara.Add(new Chunk("\n", addressFont));
                    titlePara.Add(new Chunk("CANTEEN MEAL REQUEST FORM", titleFont));
                    titlePara.Alignment = Element.ALIGN_CENTER;
                    titlePara.SpacingBefore = 0f;
                    titlePara.SpacingAfter = 5f;
                    document.Add(titlePara);

                    // Fetch order details from tbl_ExternalFoodOrder
                    string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                    string checkStatus = "";
                    string approveStatus = "";
                    string breakfastPackage = "A";
                    string lunchPackage = "B";
                    string teaPackage = "C";
                    string bNofpaxP = "";
                    string bDeliveryTime = "";
                    string lNofpaxB = "";
                    string lNofpaxP = "";
                    string lDeliveryTime = "";
                    string tNofpaxP = "";
                    string tDeliveryTime = "";
                    string DeliveryP = ""; 
                    string remark = "";

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = @"
                    SELECT CheckStatus, ApproveStatus, B_Nofpax_P, B_DeliveryTime, L_Nofpax_B, L_Nofpax_P, 
                           L_DeliveryTime, T_Nofpax_P, T_DeliveryTime, DeliveryPlace, Remark
                    FROM tbl_ExternalFoodOrder 
                    WHERE OrderID = @OrderID";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    checkStatus = reader["CheckStatus"] != DBNull.Value ? reader["CheckStatus"].ToString() : "";
                                    approveStatus = reader["ApproveStatus"] != DBNull.Value ? reader["ApproveStatus"].ToString() : "";
                                    bNofpaxP = reader["B_Nofpax_P"] != DBNull.Value ? reader["B_Nofpax_P"].ToString() : "";
                                    bDeliveryTime = reader["B_DeliveryTime"] != DBNull.Value ? reader["B_DeliveryTime"].ToString() : "";
                                    lNofpaxB = reader["L_Nofpax_B"] != DBNull.Value ? reader["L_Nofpax_B"].ToString() : "";
                                    lNofpaxP = reader["L_Nofpax_P"] != DBNull.Value ? reader["L_Nofpax_P"].ToString() : "";
                                    lDeliveryTime = reader["L_DeliveryTime"] != DBNull.Value ? reader["L_DeliveryTime"].ToString() : "";
                                    tNofpaxP = reader["T_Nofpax_P"] != DBNull.Value ? reader["T_Nofpax_P"].ToString() : "";
                                    tDeliveryTime = reader["T_DeliveryTime"] != DBNull.Value ? reader["T_DeliveryTime"].ToString() : "";
                                    DeliveryP = reader["DeliveryPlace"] != DBNull.Value ? reader["DeliveryPlace"].ToString() : "";
                                    remark = reader["Remark"] != DBNull.Value ? reader["Remark"].ToString() : "";
                                }
                            }
                        }
                        conn.Close();
                    }

                    // Replace empty strings with "-" for display in the PDF
                    bNofpaxP = string.IsNullOrEmpty(bNofpaxP) ? "-" : bNofpaxP;
                    bDeliveryTime = string.IsNullOrEmpty(bDeliveryTime) ? "-" : bDeliveryTime;
                    lNofpaxB = string.IsNullOrEmpty(lNofpaxB) ? "-" : lNofpaxB;
                    lNofpaxP = string.IsNullOrEmpty(lNofpaxP) ? "-" : lNofpaxP;
                    lDeliveryTime = string.IsNullOrEmpty(lDeliveryTime) ? "-" : lDeliveryTime;
                    tNofpaxP = string.IsNullOrEmpty(tNofpaxP) ? "-" : tNofpaxP;
                    tDeliveryTime = string.IsNullOrEmpty(tDeliveryTime) ? "-" : tDeliveryTime;
                    DeliveryP = string.IsNullOrEmpty(DeliveryP) ? "-" : DeliveryP;
                    remark = string.IsNullOrEmpty(remark) ? "-" : remark;

                    // Extract package from ComboBoxes, handling null SelectedItem
                    string packageBreakfast = cmbB_Package?.SelectedItem != null ? cmbB_Package.SelectedItem.ToString() : "-";
                    string packageLunch = cmbL_Package?.SelectedItem != null ? cmbL_Package.SelectedItem.ToString() : "-";
                    string packageTea = cmbT_Package?.SelectedItem != null ? cmbT_Package.SelectedItem.ToString() : "-";
                    breakfastPackage = packageBreakfast;
                    lunchPackage = packageLunch;
                    teaPackage = packageTea;

                    // Create a two-column layout for request details and approval statuses
                    PdfPTable mainLayoutTable = new PdfPTable(2);
                    mainLayoutTable.WidthPercentage = 100;
                    mainLayoutTable.SetWidths(new float[] { 1.5f, 0.8f });
                    mainLayoutTable.SpacingBefore = 10f;

                    // Left column: Request details
                    PdfPCell leftCell = new PdfPCell();
                    leftCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    leftCell.Padding = 0f;

                    // Create a nested table for the two-column layout
                    PdfPTable detailsTable = new PdfPTable(4);
                    detailsTable.WidthPercentage = 100;
                    detailsTable.SetWidths(new float[] { 0.22f, 0.5f, 0.24f, 0.7f });
                    detailsTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    detailsTable.DefaultCell.Padding = 2f;
                    detailsTable.SpacingBefore = 5f;

                    // Row 1: OrderID and Request date
                    detailsTable.AddCell(new Phrase("OrderID      :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderId, bodyFont));
                    detailsTable.AddCell(new Phrase("Request date:", bodyFont));
                    detailsTable.AddCell(new Phrase(eventDate.ToString("dd.MM.yyyy"), bodyFont));

                    // Row 2: Requester and Delivery date
                    detailsTable.AddCell(new Phrase("Requester  :", bodyFont));
                    detailsTable.AddCell(new Phrase(loggedInUser, bodyFont));
                    detailsTable.AddCell(new Phrase("Delivery date:", bodyFont));
                    detailsTable.AddCell(new Phrase(DeliveryTime.ToString("dd.MM.yyyy"), bodyFont));

                    // Row 3: Department and Event details
                    detailsTable.AddCell(new Phrase("Department:", bodyFont));
                    detailsTable.AddCell(new Phrase(loggedInDepart, bodyFont));
                    detailsTable.AddCell(new Phrase("Event details:", bodyFont));
                    detailsTable.AddCell(new Phrase(eventDetails, bodyFont));

                    // Add the nested table to the left cell
                    leftCell.AddElement(detailsTable);

                    // Right column: Approval statuses as paragraphs
                    PdfPCell rightCell = new PdfPCell();
                    rightCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    rightCell.Padding = 0f;

                    // Checked by
                    Paragraph checkedPara = new Paragraph();
                    checkedPara.Add(new Chunk($"Checked by : {checkStatus}", bodyFont));
                    checkedPara.SpacingBefore = 0f;
                    rightCell.AddElement(checkedPara);

                    Paragraph checkedAdminPara = new Paragraph();
                    checkedAdminPara.Add(new Chunk($"", bodyFont));
                    checkedAdminPara.SpacingBefore = 0f;
                    checkedAdminPara.SpacingAfter = 0f;
                    rightCell.AddElement(checkedAdminPara);

                    // Approved by
                    Paragraph approvedPara = new Paragraph();
                    approvedPara.Add(new Chunk($"Approved by: {approveStatus}", bodyFont));
                    approvedPara.SpacingBefore = 0f;
                    rightCell.AddElement(approvedPara);

                    Paragraph approvedHrPara = new Paragraph();
                    approvedHrPara.Add(new Chunk($"", bodyFont));
                    approvedHrPara.SpacingBefore = 0f;
                    approvedHrPara.SpacingAfter = 0f;
                    rightCell.AddElement(approvedHrPara);

                    // Issued by
                    Paragraph issuedPara = new Paragraph();
                    issuedPara.Add(new Chunk($"Received by: Canteen", bodyFont));
                    issuedPara.SpacingBefore = 0f;
                    rightCell.AddElement(issuedPara);

                    // Add the left and right cells to the main layout table
                    mainLayoutTable.AddCell(leftCell);
                    mainLayoutTable.AddCell(rightCell);

                    // Add the main layout table to the document
                    document.Add(mainLayoutTable);

                    // Details Heading
                    Paragraph detailsHeading = new Paragraph("Details of the Order:", bodyFont);
                    detailsHeading.SpacingBefore = 10f;
                    detailsHeading.SpacingAfter = 5f;
                    document.Add(detailsHeading);

                    // Details Table
                    PdfPTable detailsTable2 = new PdfPTable(2);
                    detailsTable2.WidthPercentage = 100;
                    detailsTable2.SetWidths(new float[] { 0.8f, 3f });

                    AddStyledTableRow(detailsTable2, "Breakfast Package:", breakfastPackage, bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Breakfast Packing Pax:", bNofpaxP, bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Breakfast Delivery Time:", bDeliveryTime, bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Lunch Package:", lunchPackage, bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Lunch Buffet Pax:", lNofpaxB, bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Lunch Packing Pax:", lNofpaxP, bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Lunch Delivery Time:", lDeliveryTime, bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Tea Package:", teaPackage, bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Tea Packing Pax:", tNofpaxP, bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Tea Delivery Time:", tDeliveryTime, bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Delivery Place:", DeliveryP, bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Remarks:", remark, bodyFont, italicBodyFont, 0, true);

                    document.Add(detailsTable2);

                    // Force a new page before the food items section
                    document.NewPage();

                    // Single Table for All Food Items
                    PdfPTable foodItemsTable = new PdfPTable(2);
                    foodItemsTable.WidthPercentage = 100;
                    foodItemsTable.SetWidths(new float[] { 1f, 1f });
                    foodItemsTable.SpacingBefore = 15f;
                    foodItemsTable.DefaultCell.BorderColor = darkGray;
                    foodItemsTable.KeepTogether = true;

                    // Row 1: Meals for Packages Heading
                    PdfPCell headingCell = new PdfPCell(new Phrase($"Meal Packages - Breakfast: {breakfastPackage}, Lunch: {lunchPackage}, Tea: {teaPackage}", mealsHeadingFont));
                    headingCell.Colspan = 2;
                    headingCell.BackgroundColor = BaseColor.WHITE;
                    headingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                    headingCell.BorderColor = darkGray;
                    headingCell.BorderWidth = 1.5f;
                    headingCell.Padding = 5f;
                    headingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    foodItemsTable.AddCell(headingCell);

                    // Fetch menu items from tbl_Menu only for selected packages
                    DataTable breakfastPackingItems = new DataTable();
                    DataTable lunchBuffetItems = new DataTable();
                    DataTable lunchPackingItems = new DataTable();
                    DataTable teaPackingItems = new DataTable();

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        if (breakfastPackage != "-")
                        {
                            string queryBreakfast = $"SELECT Menu FROM tbl_Menu WHERE Package = @Package AND Meal = 'BREAKFAST' AND Style = 'PACKING'";
                            using (SqlDataAdapter da = new SqlDataAdapter(queryBreakfast, conn))
                            {
                                da.SelectCommand.Parameters.AddWithValue("@Package", breakfastPackage);
                                da.Fill(breakfastPackingItems);
                            }
                        }

                        if (lunchPackage != "-")
                        {
                            string queryLunchBuffet = $"SELECT Menu FROM tbl_Menu WHERE Package = @Package AND Meal = 'LUNCH' AND Style = 'BUFFET'";
                            string queryLunchPacking = $"SELECT Menu FROM tbl_Menu WHERE Package = @Package AND Meal = 'LUNCH' AND Style = 'PACKING'";
                            using (SqlDataAdapter da = new SqlDataAdapter(queryLunchBuffet, conn))
                            {
                                da.SelectCommand.Parameters.AddWithValue("@Package", lunchPackage);
                                da.Fill(lunchBuffetItems);
                            }
                            using (SqlDataAdapter da = new SqlDataAdapter(queryLunchPacking, conn))
                            {
                                da.SelectCommand.Parameters.AddWithValue("@Package", lunchPackage);
                                da.Fill(lunchPackingItems);
                            }
                        }

                        if (teaPackage != "-")
                        {
                            string queryTea = $"SELECT Menu FROM tbl_Menu WHERE Package = @Package AND Meal = 'TEA' AND Style = 'PACKING'";
                            using (SqlDataAdapter da = new SqlDataAdapter(queryTea, conn))
                            {
                                da.SelectCommand.Parameters.AddWithValue("@Package", teaPackage);
                                da.Fill(teaPackingItems);
                            }
                        }
                        conn.Close();
                    }

                    // Conditionally add Breakfast Packing section
                    if (breakfastPackage != "-")
                    {
                        // Breakfast Packing Heading
                        PdfPCell breakfastHeadingCell = new PdfPCell(new Phrase($"BREAKFAST PACKING: {txtB_Pack.Text}", sectionTitleFont));
                        breakfastHeadingCell.Colspan = 2;
                        breakfastHeadingCell.BackgroundColor = BaseColor.WHITE;
                        breakfastHeadingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                        breakfastHeadingCell.BorderColor = darkGray;
                        breakfastHeadingCell.BorderWidth = 1.5f;
                        breakfastHeadingCell.Padding = 5f;
                        breakfastHeadingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        foodItemsTable.AddCell(breakfastHeadingCell);

                        // Breakfast Packing Items
                        PdfPCell breakfastCell = new PdfPCell();
                        breakfastCell.Colspan = 2;
                        breakfastCell.BackgroundColor = BaseColor.WHITE;
                        breakfastCell.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                        breakfastCell.BorderColor = darkGray;
                        breakfastCell.BorderWidth = 1.5f;
                        breakfastCell.Padding = 5f;
                        foreach (DataRow row in breakfastPackingItems.Rows)
                        {
                            string menuItem = row["Menu"].ToString();
                            Paragraph item = new Paragraph($"- {menuItem}", italicBodyFont);
                            item.SpacingBefore = 1f;
                            item.SpacingAfter = 1f;
                            breakfastCell.AddElement(item);
                        }
                        foodItemsTable.AddCell(breakfastCell);
                    }

                    // Conditionally add Lunch section
                    if (lunchPackage != "-")
                    {
                        // Lunch Heading
                        PdfPCell lunchHeadingCell = new PdfPCell(new Phrase("LUNCH", sectionTitleFont));
                        lunchHeadingCell.Colspan = 2;
                        lunchHeadingCell.BackgroundColor = BaseColor.WHITE;
                        lunchHeadingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                        lunchHeadingCell.BorderColor = darkGray;
                        lunchHeadingCell.BorderWidth = 1.5f;
                        lunchHeadingCell.Padding = 5f;
                        lunchHeadingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        foodItemsTable.AddCell(lunchHeadingCell);

                        // Lunch Buffet and Lunch Packing
                        PdfPCell lunchBuffetCell = new PdfPCell();
                        lunchBuffetCell.BackgroundColor = BaseColor.WHITE;
                        lunchBuffetCell.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                        lunchBuffetCell.BorderColor = darkGray;
                        lunchBuffetCell.BorderWidth = 1.5f;
                        lunchBuffetCell.Padding = 5f;
                        Paragraph lunchBuffetTitle = new Paragraph($"BUFFET: {txtL_Buffet.Text}", sectionTitleFont);
                        lunchBuffetTitle.SpacingAfter = 2f;
                        lunchBuffetCell.AddElement(lunchBuffetTitle);
                        foreach (DataRow row in lunchBuffetItems.Rows)
                        {
                            string menuItem = row["Menu"].ToString();
                            Paragraph item = new Paragraph($"- {menuItem}", italicBodyFont);
                            item.SpacingBefore = 1f;
                            item.SpacingAfter = 1f;
                            lunchBuffetCell.AddElement(item);
                        }
                        foodItemsTable.AddCell(lunchBuffetCell);

                        PdfPCell lunchPackingCell = new PdfPCell();
                        lunchPackingCell.BackgroundColor = BaseColor.WHITE;
                        lunchPackingCell.Border = iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                        lunchPackingCell.BorderColor = darkGray;
                        lunchPackingCell.BorderWidth = 1.5f;
                        lunchPackingCell.Padding = 5f;
                        Paragraph lunchPackingTitle = new Paragraph($"PACKING: {txtL_Pack.Text}", sectionTitleFont);
                        lunchPackingTitle.SpacingAfter = 2f;
                        lunchPackingCell.AddElement(lunchPackingTitle);
                        foreach (DataRow row in lunchPackingItems.Rows)
                        {
                            string menuItem = row["Menu"].ToString();
                            Paragraph item = new Paragraph($"- {menuItem}", italicBodyFont);
                            item.SpacingBefore = 1f;
                            item.SpacingAfter = 1f;
                            lunchPackingCell.AddElement(item);
                        }
                        foodItemsTable.AddCell(lunchPackingCell);
                    }

                    // Conditionally add Tea Packing section
                    if (teaPackage != "-")
                    {
                        // Tea Packing Heading
                        PdfPCell teaHeadingCell = new PdfPCell(new Phrase($"TEA PACKING: {txtT_Pack.Text}", sectionTitleFont));
                        teaHeadingCell.Colspan = 2;
                        teaHeadingCell.BackgroundColor = BaseColor.WHITE;
                        teaHeadingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                        teaHeadingCell.BorderColor = darkGray;
                        teaHeadingCell.BorderWidth = 1.5f;
                        teaHeadingCell.Padding = 5f;
                        teaHeadingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        foodItemsTable.AddCell(teaHeadingCell);

                        // Tea Packing Items
                        PdfPCell teaCell = new PdfPCell();
                        teaCell.Colspan = 2;
                        teaCell.BackgroundColor = BaseColor.WHITE;
                        teaCell.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                        teaCell.BorderColor = darkGray;
                        teaCell.BorderWidth = 1.5f;
                        teaCell.Padding = 5f;
                        foreach (DataRow row in teaPackingItems.Rows)
                        {
                            string menuItem = row["Menu"].ToString();
                            Paragraph item = new Paragraph($"- {menuItem}", italicBodyFont);
                            item.SpacingBefore = 1f;
                            item.SpacingAfter = 1f;
                            teaCell.AddElement(item);
                        }
                        foodItemsTable.AddCell(teaCell);
                    }

                    // Only add the foodItemsTable if at least one section is present
                    if (breakfastPackage != "-" || lunchPackage != "-" || teaPackage != "-")
                    {
                        document.Add(foodItemsTable);
                    }

                    // Footer
                    Paragraph footer = new Paragraph("This is a computer generated document, no signature is required | Generated on: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), bodyFont);
                    footer.Alignment = Element.ALIGN_LEFT;
                    footer.SpacingBefore = 20f;
                    footer.Font.Color = new BaseColor(100, 100, 100);
                    document.Add(footer);

                    document.Close();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void StorePdfInDatabase(string orderId, byte[] pdfBytes)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO tbl_ExternalPdfStorage (OrderID, PdfData, CreatedDate) VALUES (@OrderID, @PdfData, @CreatedDate)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        cmd.Parameters.AddWithValue("@PdfData", pdfBytes);
                        cmd.Parameters.AddWithValue("@CreatedDate", DateTime.Now);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error storing PDF in database: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewPdf(byte[] pdfBytes)
        {
            if (pdfBytes != null)
            {
                string tempFile = Path.GetTempFileName() + ".pdf";
                File.WriteAllBytes(tempFile, pdfBytes);
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
            else
            {
                MessageBox.Show("No PDF data available to view.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddStyledTableRow(PdfPTable table, string label, string value, iTextSharp.text.Font labelFont, iTextSharp.text.Font valueFont, int rowIndex, bool multiLine = false)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont));
            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont)) { MinimumHeight = 20f };

            labelCell.BackgroundColor = new BaseColor(255, 255, 255);
            valueCell.BackgroundColor = new BaseColor(255, 255, 255);

            labelCell.Phrase = new Phrase(label, new iTextSharp.text.Font(labelFont.BaseFont, labelFont.Size, labelFont.Style, BaseColor.BLACK));
            valueCell.Phrase = new Phrase(value, new iTextSharp.text.Font(valueFont.BaseFont, valueFont.Size, valueFont.Style, BaseColor.BLACK));

            labelCell.Padding = 8f;
            valueCell.Padding = 8f;
            labelCell.BorderColor = new BaseColor(150, 150, 150);
            valueCell.BorderColor = new BaseColor(150, 150, 150);
            labelCell.HorizontalAlignment = Element.ALIGN_LEFT;
            valueCell.HorizontalAlignment = Element.ALIGN_LEFT;

            if (multiLine)
            {
                valueCell.NoWrap = false;
            }

            table.AddCell(labelCell);
            table.AddCell(valueCell);
        }

        public class PdfPageEventHelper : iTextSharp.text.pdf.PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                PdfPTable footerTbl = new PdfPTable(1);
                footerTbl.TotalWidth = document.PageSize.Width - 72;
                PdfPCell cell = new PdfPCell(new Phrase($"Page {writer.PageNumber}", FontFactory.GetFont("Helvetica", 8, BaseColor.GRAY)));
                cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                footerTbl.AddCell(cell);
                footerTbl.WriteSelectedRows(0, -1, 36, 20, writer.DirectContent);
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
            CheckUserAccess(loggedInUser);

            Form_Home.sharedLabel.Text = "Admin > Meal Request";
            Form_Home.sharedButton6.Visible = true;
            UC_Meal_Food ug = new UC_Meal_Food(EventDetails, EventTime, DeliveryTime, loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void loadmenu()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = "SELECT Menu FROM tbl_Menu WHERE Package = 'A' AND Meal = 'BREAKFAST' AND Style = 'PACKING'";
                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                {
                    da.Fill(dt);
                }
                dgvA_B_P.DataSource = dt;

                string query2 = "SELECT Menu FROM tbl_Menu WHERE Package = 'A' AND Meal = 'LUNCH' AND Style = 'BUFFET'";
                DataTable dt2 = new DataTable();
                using (SqlDataAdapter da2 = new SqlDataAdapter(query2, con))
                {
                    da2.Fill(dt2);
                }
                dgvA_L_B.DataSource = dt2;

                string query3 = "SELECT Menu FROM tbl_Menu WHERE Package = 'A' AND Meal = 'LUNCH' AND Style = 'PACKING'";
                DataTable dt3 = new DataTable();
                using (SqlDataAdapter da3 = new SqlDataAdapter(query3, con))
                {
                    da3.Fill(dt3);
                }
                dgvA_L_P.DataSource = dt3;

                string query4 = "SELECT Menu FROM tbl_Menu WHERE Package = 'A' AND Meal = 'TEA' AND Style = 'PACKING'";
                DataTable dt4 = new DataTable();
                using (SqlDataAdapter da4 = new SqlDataAdapter(query4, con))
                {
                    da4.Fill(dt4);
                }
                dgvA_T_P.DataSource = dt4;

                string query5 = "SELECT Menu FROM tbl_Menu WHERE Package = 'B' AND Meal = 'BREAKFAST' AND Style = 'PACKING'";
                DataTable dt5 = new DataTable();
                using (SqlDataAdapter da5 = new SqlDataAdapter(query5, con))
                {
                    da5.Fill(dt5);
                }
                dgvB_B_P.DataSource = dt5;

                string query6 = "SELECT Menu FROM tbl_Menu WHERE Package = 'B' AND Meal = 'LUNCH' AND Style = 'BUFFET'";
                DataTable dt6 = new DataTable();
                using (SqlDataAdapter da6 = new SqlDataAdapter(query6, con))
                {
                    da6.Fill(dt6);
                }
                dgvB_L_B.DataSource = dt6;

                string query7 = "SELECT Menu FROM tbl_Menu WHERE Package = 'B' AND Meal = 'LUNCH' AND Style = 'PACKING'";
                DataTable dt7 = new DataTable();
                using (SqlDataAdapter da7 = new SqlDataAdapter(query7, con))
                {
                    da7.Fill(dt7);
                }
                dgvB_L_P.DataSource = dt7;

                string query8 = "SELECT Menu FROM tbl_Menu WHERE Package = 'B' AND Meal = 'TEA' AND Style = 'PACKING'";
                DataTable dt8 = new DataTable();
                using (SqlDataAdapter da8 = new SqlDataAdapter(query8, con))
                {
                    da8.Fill(dt8);
                }
                dgvB_T_P.DataSource = dt8;

                string query9 = "SELECT Menu FROM tbl_Menu WHERE Package = 'C' AND Meal = 'BREAKFAST' AND Style = 'PACKING'";
                DataTable dt9 = new DataTable();
                using (SqlDataAdapter da9 = new SqlDataAdapter(query9, con))
                {
                    da9.Fill(dt9);
                }
                dgvC_B_P.DataSource = dt9;

                string query10 = "SELECT Menu FROM tbl_Menu WHERE Package = 'C' AND Meal = 'LUNCH' AND Style = 'BUFFET'";
                DataTable dt10 = new DataTable();
                using (SqlDataAdapter da10 = new SqlDataAdapter(query10, con))
                {
                    da10.Fill(dt10);
                }
                dgvC_L_B.DataSource = dt10;

                string query11 = "SELECT Menu FROM tbl_Menu WHERE Package = 'C' AND Meal = 'LUNCH' AND Style = 'PACKING'";
                DataTable dt11 = new DataTable();
                using (SqlDataAdapter da11 = new SqlDataAdapter(query11, con))
                {
                    da11.Fill(dt11);
                }
                dgvC_L_P.DataSource = dt11;

                string query12 = "SELECT Menu FROM tbl_Menu WHERE Package = 'C' AND Meal = 'TEA' AND Style = 'PACKING'";
                DataTable dt12 = new DataTable();
                using (SqlDataAdapter da12 = new SqlDataAdapter(query12, con))
                {
                    da12.Fill(dt12);
                }
                dgvC_T_P.DataSource = dt12;

                con.Close();

                // Style all DataGridViews
                StyleDataGridView(dgvA_B_P);
                StyleDataGridView(dgvA_L_B);
                StyleDataGridView(dgvA_L_P);
                StyleDataGridView(dgvA_T_P);
                StyleDataGridView(dgvB_B_P);
                StyleDataGridView(dgvB_L_B);
                StyleDataGridView(dgvB_L_P);
                StyleDataGridView(dgvB_T_P);
                StyleDataGridView(dgvC_B_P);
                StyleDataGridView(dgvC_L_B);
                StyleDataGridView(dgvC_L_P);
                StyleDataGridView(dgvC_T_P);
            }
        }

        private void StyleDataGridView(DataGridView dgv)
        {
            dgv.ColumnHeadersVisible = false;
            dgv.Dock = DockStyle.Fill;
            dgv.AllowUserToResizeRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            foreach (DataGridViewColumn column in dgv.Columns)
            {
                column.DefaultCellStyle = new DataGridViewCellStyle
                {
                    ForeColor = Color.Black,
                    Font = new DrawingFont("Arial", 11, FontStyle.Italic)
                };
                column.Resizable = DataGridViewTriState.False;
                column.ReadOnly = true;
            }
        }

        private void cmbB_Package_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbB_Package.SelectedItem == null) return;

            string selectedPackage = cmbB_Package.SelectedItem.ToString();
            string query = $"SELECT Menu FROM tbl_Menu WHERE Package = '{selectedPackage}' AND Meal = 'BREAKFAST' AND Style = 'PACKING'";
            UpdateDataGridView(query, selectedPackage == "A" ? dgvA_B_P : selectedPackage == "B" ? dgvB_B_P : dgvC_B_P);
        }

        private void cmbL_Package_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbL_Package.SelectedItem == null) return;

            string selectedPackage = cmbL_Package.SelectedItem.ToString();
            string queryBuffet = $"SELECT Menu FROM tbl_Menu WHERE Package = '{selectedPackage}' AND Meal = 'LUNCH' AND Style = 'BUFFET'";
            string queryPacking = $"SELECT Menu FROM tbl_Menu WHERE Package = '{selectedPackage}' AND Meal = 'LUNCH' AND Style = 'PACKING'";
            UpdateDataGridView(queryBuffet, selectedPackage == "A" ? dgvA_L_B : selectedPackage == "B" ? dgvB_L_B : dgvC_L_B);
            UpdateDataGridView(queryPacking, selectedPackage == "A" ? dgvA_L_P : selectedPackage == "B" ? dgvB_L_P : dgvC_L_P);
        }

        private void cmbT_Package_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbT_Package.SelectedItem == null) return;

            string selectedPackage = cmbT_Package.SelectedItem.ToString();
            string query = $"SELECT Menu FROM tbl_Menu WHERE Package = '{selectedPackage}' AND Meal = 'TEA' AND Style = 'PACKING'";
            UpdateDataGridView(query, selectedPackage == "A" ? dgvA_T_P : selectedPackage == "B" ? dgvB_T_P : dgvC_T_P);
        }

        private void UpdateDataGridView(string query, DataGridView dgv)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                DataTable dt = new DataTable();
                using (SqlDataAdapter da = new SqlDataAdapter(query, con))
                {
                    da.Fill(dt);
                }
                dgv.DataSource = dt;
                con.Close();
            }
        }

        private void txtB_Pack_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtB_Pack.Text))
            {
                if (!int.TryParse(txtB_Pack.Text, out int number) || number < 0 || number > 100)
                {
                    MessageBox.Show("Please enter a number between 0 and 100.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtB_Pack.Text = "";
                }
            }
        }
        private void txtL_Buffet_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtL_Buffet.Text))
            {
                if (!int.TryParse(txtL_Buffet.Text, out int number) || number < 0 || number > 100)
                {
                    MessageBox.Show("Please enter a number between 0 and 100.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtL_Buffet.Text = "";
                }
            }
        }

        private void txtL_Pack_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtL_Pack.Text))
            {
                if (!int.TryParse(txtL_Pack.Text, out int number) || number < 0 || number > 100)
                {
                    MessageBox.Show("Please enter a number between 0 and 100.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtL_Pack.Text = "";
                }
            }
        }

        private void txtT_Pack_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtT_Pack.Text))
            {
                if (!int.TryParse(txtT_Pack.Text, out int number) || number < 0 || number > 100)
                {
                    MessageBox.Show("Please enter a number between 0 and 100.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtT_Pack.Text = "";
                }
            }
        }
        private void lblRequestDate2_Click(object sender, EventArgs e) { }
        private void UC_Meal_External_Load(object sender, EventArgs e) { }
        private void txtE_Remark_TextChanged(object sender, EventArgs e) { }
        private void cmbB_DT_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cmbL_DT_SelectedIndexChanged(object sender, EventArgs e) { }
        private void dgvA_B_P_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}