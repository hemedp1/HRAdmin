using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;
using HRAdmin.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Diagnostics;
using iTextRectangle = iTextSharp.text.Rectangle;

namespace HRAdmin.UserControl
{
    public partial class UC_Meal_Internal : System.Windows.Forms.UserControl
    {
        private string loggedInUser;
        private string loggedInDepart;
        private string eventDetails;
        private DateTime eventText;
        private DateTime deliveryTime;
        private string cmbOccasion = "Internal";
        private string pdfFilePath;

        public UC_Meal_Internal(string eventDescription, DateTime? eventText, DateTime? deliveryTime, string username, string department)
        {
            InitializeComponent();

            loggedInUser = username;
            this.eventDetails = eventDescription;
            this.eventText = eventText ?? DateTime.Now;
            this.deliveryTime = deliveryTime ?? this.eventText;
            loggedInDepart = department;

            lblEvent1.Text = eventDetails;
            lblRequestDate1.Text = this.eventText.ToString("dd.MM.yyyy");
            lblDeliveryDate1.Text = this.deliveryTime.ToString("dd.MM.yyyy");

            // Disable Hot/Cold by default until a drink is selected
            cmb_HC1.Enabled = false;
            cmb_HC2.Enabled = false;

            cmb_Drink1.SelectedIndexChanged += new EventHandler(cmb_Drink_SelectedIndexChanged);
            cmb_Drink2.SelectedIndexChanged += new EventHandler(cmb_Drink_SelectedIndexChanged);
        }

        private void btNext_Click(object sender, EventArgs e)
        {
            // Validate meal selection
            if (cmb_Meal.Enabled && cmb_Meal.Visible && cmb_Meal.SelectedItem == null)
            {
                MessageBox.Show("Please select a meal.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate number of pax
            if (string.IsNullOrWhiteSpace(txt_Npax.Text))
            {
                MessageBox.Show("No. of Pax required", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate delivery time
            if (cmb_DeliveryT.SelectedItem == null)
            {
                MessageBox.Show("Delivery Time required.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate remark
            if (string.IsNullOrWhiteSpace(txt_Remark.Text))
            {
                MessageBox.Show("Remark required", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate that at least one of cmb_Menu or cmb_Drink1 is selected when txt_Npax and cmb_DeliveryT are selected
            if (!string.IsNullOrWhiteSpace(txt_Npax.Text) && cmb_DeliveryT.SelectedItem != null)
            {
                if (cmb_Menu.SelectedItem == null && cmb_Drink1.SelectedItem == null)
                {
                    MessageBox.Show("Please select at least a Dish or Drink.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Validate Drink1 hot/cold selection only if cmb_HC1 is enabled
            if (cmb_Drink1.SelectedItem != null && cmb_HC1.Enabled)
            {
                string selectedDrink1 = cmb_Drink1.SelectedItem.ToString();
                if (selectedDrink1 != "Mineral water 500ML" && (cmb_HC1.SelectedItem == null || string.IsNullOrEmpty(cmb_HC1.SelectedItem.ToString())))
                {
                    MessageBox.Show("Please select Hot or Cold for Drink 1.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Validate Drink2 hot/cold selection only if cmb_HC2 is enabled
            if (cmb_Drink2.SelectedItem != null && cmb_HC2.Enabled)
            {
                string selectedDrink2 = cmb_Drink2.SelectedItem.ToString();
                if (selectedDrink2 != "Mineral water 500ML" && (cmb_HC2.SelectedItem == null || string.IsNullOrEmpty(cmb_HC2.SelectedItem.ToString())))
                {
                    MessageBox.Show("Please select Hot or Cold for Drink 2.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // Add the new validation here
            if ((cmb_Menu.SelectedItem?.ToString() == "-" || cmb_Menu.SelectedItem == null) &&
                (cmb_Drink1.SelectedItem?.ToString() == "-" || cmb_Drink1.SelectedItem == null) &&
                (cmb_Drink2.SelectedItem?.ToString() == "-" || cmb_Drink2.SelectedItem == null) &&
                (cmb_Snack.SelectedItem?.ToString() == "-" || cmb_Snack.SelectedItem == null))
            {
                MessageBox.Show("Please select at least one item (Dish, Drink1, Drink2, or Snack).",
                    "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Proceed with database insertion and PDF generation
            string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string insertQuery = @"
INSERT INTO tbl_InternalFoodOrder (OrderID, RequesterID, Department, OccasionType, RequestDate, DeliveryDate, EventDetails, Menu, Snack, Drink1, HOTorCOLD1, Drink2, HOTorCOLD2, No_pax, DeliveryTime, Remark, OrderType) 
VALUES (@OrderID, @RequesterID, @Department, @OccasionType, @RequestDate, @DeliveryDate, @EventDetails, @Menu, @Snack, @Drink1, @HOTorCOLD1, @Drink2, @HOTorCOLD2, @No_pax, @DeliveryTime, @Remark, @OrderType)";

                SqlCommand insertCmd = new SqlCommand(insertQuery, con);

                string mealCode;
                string selectedMeal = cmb_Meal.SelectedItem?.ToString();
                switch (selectedMeal)
                {
                    case "Breakfast":
                        mealCode = "B";
                        break;
                    case "Lunch":
                        mealCode = "L";
                        break;
                    case "Tea":
                        mealCode = "T";
                        break;
                    case "Dinner":
                        mealCode = "D";
                        break;
                    default:
                        mealCode = "N";
                        break;
                }

                string combinedValue = $"{DateTime.Now:ddMmyyyy_HHmmss}_{mealCode}";

                insertCmd.Parameters.AddWithValue("@OrderID", combinedValue);
                insertCmd.Parameters.AddWithValue("@RequesterID", loggedInUser);
                insertCmd.Parameters.AddWithValue("@Department", loggedInDepart);
                insertCmd.Parameters.AddWithValue("@OccasionType", cmbOccasion);
                insertCmd.Parameters.AddWithValue("@RequestDate", eventText);
                insertCmd.Parameters.AddWithValue("@DeliveryDate", deliveryTime);
                insertCmd.Parameters.AddWithValue("@EventDetails", eventDetails);
                insertCmd.Parameters.AddWithValue("@Menu", cmb_Menu.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Snack", cmb_Snack.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Drink1", cmb_Drink1.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@HOTorCOLD1", cmb_HC1.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Drink2", cmb_Drink2.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@HOTorCOLD2", cmb_HC2.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@No_pax", string.IsNullOrEmpty(txt_Npax.Text) ? (object)DBNull.Value : txt_Npax.Text);
                insertCmd.Parameters.AddWithValue("@DeliveryTime", cmb_DeliveryT.SelectedItem?.ToString() ?? (object)DBNull.Value);
                insertCmd.Parameters.AddWithValue("@Remark", string.IsNullOrEmpty(txt_Remark.Text) ? (object)DBNull.Value : txt_Remark.Text);
                insertCmd.Parameters.AddWithValue("@OrderType", cmb_Meal.SelectedItem?.ToString() ?? (object)DBNull.Value);

                insertCmd.ExecuteNonQuery();

                // Generate and store PDF
                byte[] pdfBytes = GeneratePDF(combinedValue, selectedMeal);
                if (pdfBytes != null)
                {
                    StorePdfInDatabase(combinedValue, pdfBytes);

                    // Show success message with "View in PDF" option
                    DialogResult result = MessageBox.Show("Submitted Successfully! Would you like to view the order in PDF?",
                        "Success", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (result == DialogResult.Yes)
                    {
                        ViewPdf(pdfBytes);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to generate PDF. Order submitted but PDF not created.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Return to main page
                CheckUserAccess(loggedInUser);

                Form_Home.sharedLabel.Text = "Admin > Meal Request > Internal";
                Form_Home.sharedButton4.Visible = false;
                Form_Home.sharedButton5.Visible = false;
                Form_Home.sharedButton6.Visible = false;
                UC_Meal_Internal ug = new UC_Meal_Internal(eventDetails, eventText, deliveryTime, loggedInUser, loggedInDepart);
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

                        using (SqlDataReader reader = cmd.ExecuteReader())  // Use SqlDataReader
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
        private byte[] GeneratePDF(string orderId, string selectedMeal)
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
                    iTextSharp.text.Font addressFont = FontFactory.GetFont("Helvetica", 8f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK); // Smaller font for address
                    iTextSharp.text.Font titleFont1 = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font headerFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font bodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font italicBodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.ITALIC, BaseColor.BLACK);

                    // Add company logo or name
                    string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo hosiden.jpg");
                    if (File.Exists(logoPath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                        logo.ScaleToFit(100f, 100f);
                        logo.Alignment = Element.ALIGN_CENTER;
                        logo.SpacingAfter = 0f;
                        document.Add(logo);
                    }
                    else
                    {
                        Paragraph companyPara = new Paragraph("Hosiden Electronics (M) Sdn Bhd", headerFont);
                        companyPara.Alignment = Element.ALIGN_CENTER;
                        companyPara.SpacingAfter = 0f;
                        document.Add(companyPara);
                    }

                    // Add the title
                    Paragraph titlePara = new Paragraph();
                    titlePara.Add(new Chunk("HOSIDEN ELECTRONICS (M) SDN BHD (198901000700)\n", titleFont)); // Unbolded title
                    titlePara.Add(new Chunk("Lot 1, Jalan P/1A, Bangi Industrial Estate, 43650 Bandar Baru Bangi, Selangor, Malaysia\n", addressFont)); // Smaller address font
                    titlePara.Add(new Chunk("\n", addressFont)); // Additional newline for spacing
                    titlePara.Add(new Chunk("CANTEEN MEAL REQUEST FORM", titleFont)); // Unbolded title
                    titlePara.Alignment = Element.ALIGN_CENTER;
                    titlePara.SpacingBefore = 0f;
                    titlePara.SpacingAfter = 5f;
                    document.Add(titlePara);

                    // Retrieve CheckStatus and ApproveStatus from the database
                    string checkStatus = "";
                    string approveStatus = "";
                    string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "SELECT CheckStatus, ApproveStatus FROM tbl_InternalFoodOrder WHERE OrderID = @OrderID";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    checkStatus = reader["CheckStatus"] != DBNull.Value ? reader["CheckStatus"].ToString() : "";
                                    approveStatus = reader["ApproveStatus"] != DBNull.Value ? reader["ApproveStatus"].ToString() : "";
                                }
                            }
                        }
                    }

                    // Create a two-column layout
                    PdfPTable mainLayoutTable = new PdfPTable(2);
                    mainLayoutTable.WidthPercentage = 100;
                    mainLayoutTable.SetWidths(new float[] { 1.5f, 0.8f });
                    mainLayoutTable.SpacingBefore = 5f;

                    // Left column: Request details
                    PdfPCell leftCell = new PdfPCell();
                    leftCell.Border = iTextRectangle.NO_BORDER;
                    leftCell.Padding = 0f;

                    // Create a nested table for the two-column layout
                    PdfPTable detailsTable = new PdfPTable(4); // 4 columns to handle label-value pairs
                    detailsTable.WidthPercentage = 100;
                    detailsTable.SetWidths(new float[] { 0.22f, 0.5f, 0.24f, 0.7f }); // Adjust widths for spacing
                    detailsTable.DefaultCell.Border = iTextRectangle.NO_BORDER;
                    detailsTable.DefaultCell.Padding = 2f;
                    detailsTable.SpacingBefore = 5f;

                    // Row 1: OrderID and Request date
                    detailsTable.AddCell(new Phrase("OrderID      :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderId, bodyFont));
                    detailsTable.AddCell(new Phrase("Request date:", bodyFont));
                    detailsTable.AddCell(new Phrase(eventText.ToString("dd.MM.yyyy"), bodyFont));

                    // Row 2: Requester and Delivery date
                    detailsTable.AddCell(new Phrase("Requester  :", bodyFont));
                    detailsTable.AddCell(new Phrase(loggedInUser, bodyFont));
                    detailsTable.AddCell(new Phrase("Delivery date:", bodyFont));
                    detailsTable.AddCell(new Phrase(deliveryTime.ToString("dd.MM.yyyy"), bodyFont));

                    // Row 3: Department and Event details
                    detailsTable.AddCell(new Phrase("Department:", bodyFont));
                    detailsTable.AddCell(new Phrase(loggedInDepart, bodyFont));
                    detailsTable.AddCell(new Phrase("Event details:", bodyFont));
                    detailsTable.AddCell(new Phrase(eventDetails, bodyFont));

                    // Add the nested table to the left cell
                    leftCell.AddElement(detailsTable);

                    // Right column: Approval statuses as paragraphs with specified layout
                    PdfPCell rightCell = new PdfPCell();
                    rightCell.Border = iTextRectangle.NO_BORDER;
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

                    mainLayoutTable.AddCell(leftCell);
                    mainLayoutTable.AddCell(rightCell);
                    document.Add(mainLayoutTable);

                    // Add "Details of the meals:" heading
                    Paragraph detailsHeading = new Paragraph("Details of the order:", bodyFont);
                    detailsHeading.SpacingBefore = 10f;
                    detailsHeading.SpacingAfter = 5f;
                    document.Add(detailsHeading);

                    // Details Section
                    PdfPTable detailsTable2 = new PdfPTable(2);
                    detailsTable2.WidthPercentage = 100;
                    detailsTable2.SetWidths(new float[] { 0.5f, 3f });

                    AddStyledTableRow(detailsTable2, "Meal type:", selectedMeal ?? "-", bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Delivery time:", cmb_DeliveryT.SelectedItem?.ToString() ?? "-", bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Dish:", cmb_Menu.SelectedItem?.ToString() ?? "-", bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Other:", cmb_Snack.SelectedItem?.ToString() ?? "-", bodyFont, italicBodyFont, 0);

                    string drink1Value = cmb_Drink1.SelectedItem?.ToString() ?? "-";
                    string hotCold1Value = cmb_HC1.SelectedItem?.ToString() ?? "-";
                    string combinedDrink1Value = drink1Value;
                    if (hotCold1Value != "-" && drink1Value != "-")
                    {
                        combinedDrink1Value = $"{drink1Value} ({hotCold1Value})";
                    }
                    AddStyledTableRow(detailsTable2, "Drink1 (Hot/Cold):", combinedDrink1Value, bodyFont, italicBodyFont, 1);

                    string drink2Value = cmb_Drink2.SelectedItem?.ToString() ?? "-";
                    string hotCold2Value = cmb_HC2.SelectedItem?.ToString() ?? "-";
                    string combinedDrink2Value = drink2Value;
                    if (hotCold2Value != "-" && drink2Value != "-")
                    {
                        combinedDrink2Value = $"{drink2Value} ({hotCold2Value})";
                    }
                    AddStyledTableRow(detailsTable2, "Drink2 (Hot/Cold):", combinedDrink2Value, bodyFont, italicBodyFont, 0);

                    AddStyledTableRow(detailsTable2, "No. of Pax:", txt_Npax.Text ?? "-", bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Remarks:", txt_Remark.Text ?? "-", bodyFont, italicBodyFont, 0, true);

                    document.Add(detailsTable2);

                    // Footer
                    Paragraph footer = new Paragraph("This is a computer generated document, no signature is required | Generated on: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"), bodyFont);
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
                    string query = "INSERT INTO tbl_InternalPdfStorage (OrderID, PdfData, CreatedDate) VALUES (@OrderID, @PdfData, @CreatedDate)";
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
                cell.Border = iTextRectangle.NO_BORDER;
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                footerTbl.AddCell(cell);
                footerTbl.WriteSelectedRows(0, -1, 36, 20, writer.DirectContent);
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e) { }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) { }

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
            UC_Meal_Food ug = new UC_Meal_Food(eventDetails, eventText.ToString(), deliveryTime, loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void cmb_Meal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_Meal.SelectedIndex != -1)
            {
                string selectedMeal = cmb_Meal.SelectedItem.ToString().ToLower();

                // Enable controls
                cmb_Menu.Enabled = true;
                cmb_Drink1.Enabled = true;
                cmb_HC1.Enabled = false; // Disabled until a drink is selected
                cmb_Drink2.Enabled = true;
                cmb_HC2.Enabled = false; // Disabled until a drink is selected
                cmb_Snack.Enabled = true;
                txt_Npax.Enabled = true;
                cmb_DeliveryT.Enabled = true;
                txt_Remark.Enabled = true;

                // Clear ComboBoxes
                cmb_Menu.Items.Clear();
                cmb_Drink1.Items.Clear();
                cmb_Drink1.SelectedIndex = -1; // Ensure no selection
                cmb_Drink2.Items.Clear();
                cmb_Drink2.SelectedIndex = -1; // Ensure no selection
                cmb_HC1.Items.Clear();
                cmb_HC2.Items.Clear();
                cmb_Snack.Items.Clear();
                cmb_DeliveryT.Items.Clear();

                // Database connection string
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        switch (selectedMeal)
                        {
                            case "breakfast":

                                // Populate Menu
                                PopulateComboBoxFromQuery(cmb_Menu, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'BREAKFAST' AND Type = 'FOOD'");

                                // Populate Drink1 and Drink2
                                PopulateComboBoxFromQuery(cmb_Drink1, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'BREAKFAST' AND Type = 'WATER'");
                                PopulateComboBoxFromQuery(cmb_Drink2, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'BREAKFAST' AND Type = 'WATER'");

                                // Populate Snack
                                PopulateComboBoxFromQuery(cmb_Snack, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'BREAKFAST' AND Type = 'OTHER'");

                                // Populate HC1 and HC2
                                cmb_HC1.Items.AddRange(new string[] { "Hot", "Cold", "" });
                                cmb_HC2.Items.AddRange(new string[] { "Hot", "Cold", "" });

                                // Populate Delivery Times
                                cmb_DeliveryT.Items.AddRange(new string[]
                                {
                            "08:00", "08:30", "09:00", "09:30", "10:00", "10:30", "11:00", "11:30"
                                });
                                break;

                            case "lunch":

                                // Populate Menu
                                PopulateComboBoxFromQuery(cmb_Menu, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'LUNCH' AND Type = 'FOOD'");

                                // Populate Drink1 and Drink2
                                PopulateComboBoxFromQuery(cmb_Drink1, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'LUNCH' AND Type = 'WATER'");
                                PopulateComboBoxFromQuery(cmb_Drink2, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'LUNCH' AND Type = 'WATER'");

                                // Populate Snack
                                PopulateComboBoxFromQuery(cmb_Snack, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'LUNCH' AND Type = 'OTHER'");

                                // Populate HC1 and HC2
                                cmb_HC1.Items.AddRange(new string[] { "Hot", "Cold", "" });
                                cmb_HC2.Items.AddRange(new string[] { "Hot", "Cold", "" });

                                // Populate Delivery Times
                                cmb_DeliveryT.Items.AddRange(new string[]
                                {
                            "12:00", "12:30", "13:00", "13:30", "14:00", "14:30"
                                });
                                break;

                            case "tea":

                                // Populate Menu
                                PopulateComboBoxFromQuery(cmb_Menu, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'TEA' AND Type = 'FOOD'");

                                // Populate Drink1 and Drink2
                                PopulateComboBoxFromQuery(cmb_Drink1, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'TEA' AND Type = 'WATER'");
                                PopulateComboBoxFromQuery(cmb_Drink2, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'TEA' AND Type = 'WATER'");

                                // Populate Snack
                                PopulateComboBoxFromQuery(cmb_Snack, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'TEA' AND Type = 'OTHER'");

                                // Populate HC1 and HC2
                                cmb_HC1.Items.AddRange(new string[] { "Hot", "Cold", "" });
                                cmb_HC2.Items.AddRange(new string[] { "Hot", "Cold", "" });

                                // Populate Delivery Times
                                cmb_DeliveryT.Items.AddRange(new string[]
                                {
                            "15:00", "15:30", "16:00", "16:30", "17:00", "17:30", "18:00", "18:30"
                                });
                                break;

                            case "dinner":
                                cmb_Snack.Enabled = false;
                                cmb_Snack.SelectedIndex = -1;

                                // Populate Menu
                                PopulateComboBoxFromQuery(cmb_Menu, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'DINNER' AND Type = 'FOOD'");

                                // Populate Drink1 and Drink2
                                PopulateComboBoxFromQuery(cmb_Drink1, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'DINNER' AND Type = 'WATER'");
                                PopulateComboBoxFromQuery(cmb_Drink2, connection,
                                    "SELECT Menu FROM tbl_InternalMenu WHERE Meal = 'DINNER' AND Type = 'WATER'");

                                // Populate HC1 and HC2
                                cmb_HC1.Items.AddRange(new string[] { "Hot", "Cold", "" });
                                cmb_HC2.Items.AddRange(new string[] { "Hot", "Cold", "" });

                                // Populate Delivery Times
                                cmb_DeliveryT.Items.AddRange(new string[]
                                {
                            "19:00", "19:30", "20:00"
                                });
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading menu: " + ex.Message);
                }
            }
            else
            {
                // Disable controls when no meal is selected
                cmb_Menu.Enabled = false;
                cmb_Drink1.Enabled = false;
                cmb_HC1.Enabled = false;
                cmb_Drink2.Enabled = false;
                cmb_HC2.Enabled = false;
                cmb_Snack.Enabled = false;
                txt_Npax.Enabled = false;
                cmb_DeliveryT.Enabled = false;
                txt_Remark.Enabled = false;
            }
        }
        // Helper method to populate a ComboBox from a SQL query
        private void PopulateComboBoxFromQuery(ComboBox comboBox, SqlConnection connection, string query)
        {
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        comboBox.Items.Add(reader["Menu"].ToString());
                    }
                }
            }
        }
        private void lblEvent1_Click(object sender, EventArgs e) { }
        private void gbExternal_Enter(object sender, EventArgs e) { }

        private void txt_Npax_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txt_Npax.Text))
            {
                if (!int.TryParse(txt_Npax.Text, out int number) || number < 1 || number > 100)
                {
                    MessageBox.Show("Please enter a number between 1 and 100.");
                    txt_Npax.Text = "";
                }
            }
        }

        private void cmb_HC_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void cmb_Drink_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Handle Drink1 selection
            if (cmb_Drink1.SelectedItem == null || string.IsNullOrEmpty(cmb_Drink1.SelectedItem.ToString()) || cmb_Drink1.SelectedItem.ToString() == "-")
            {
                cmb_HC1.Enabled = false;
                cmb_HC1.SelectedIndex = -1;
                cmb_HC1.Items.Clear();
            }
            else
            {
                string selectedDrink = cmb_Drink1.SelectedItem.ToString();
                if (selectedDrink == "Mineral water 500ML")
                {
                    cmb_HC1.Enabled = false;
                    cmb_HC1.SelectedIndex = -1;
                    cmb_HC1.Items.Clear();
                }
                else if (selectedDrink == "Cordial sarsi" || selectedDrink == "Cordial grape" || selectedDrink == "Cordial sunquick orange")
                {
                    cmb_HC1.Enabled = true;
                    cmb_HC1.Items.Clear();
                    cmb_HC1.Items.Add("Cold");
                    cmb_HC1.SelectedIndex = 0;
                }
                else
                {
                    cmb_HC1.Enabled = true;
                    if (cmb_HC1.Items.Count != 2 || !cmb_HC1.Items.Contains("Hot") || !cmb_HC1.Items.Contains("Cold"))
                    {
                        cmb_HC1.Items.Clear();
                        cmb_HC1.Items.AddRange(new string[] { "Hot", "Cold" });
                    }
                }
            }

            // Handle Drink2 selection
            if (cmb_Drink2.SelectedItem == null || string.IsNullOrEmpty(cmb_Drink2.SelectedItem.ToString()) || cmb_Drink2.SelectedItem.ToString() == "-")
            {
                cmb_HC2.Enabled = false;
                cmb_HC2.SelectedIndex = -1;
                cmb_HC2.Items.Clear();
            }
            else
            {
                string selectedDrink = cmb_Drink2.SelectedItem.ToString();
                if (selectedDrink == "Mineral water 500ML")
                {
                    cmb_HC2.Enabled = false;
                    cmb_HC2.SelectedIndex = -1;
                    cmb_HC2.Items.Clear();
                }
                else if (selectedDrink == "Cordial sarsi" || selectedDrink == "Cordial grape" || selectedDrink == "Cordial sunquick orange")
                {
                    cmb_HC2.Enabled = true;
                    cmb_HC2.Items.Clear();
                    cmb_HC2.Items.Add("Cold");
                    cmb_HC2.SelectedIndex = 0;
                }
                else
                {
                    cmb_HC2.Enabled = true;
                    if (cmb_HC2.Items.Count != 2 || !cmb_HC2.Items.Contains("Hot") || !cmb_HC2.Items.Contains("Cold"))
                    {
                        cmb_HC2.Items.Clear();
                        cmb_HC2.Items.AddRange(new string[] { "Hot", "Cold" });
                    }
                }
            }
        }

        private void lblRequestDate1_Click(object sender, EventArgs e)
        {

        }
    }
}