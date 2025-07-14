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
using WinFormsApp = System.Windows.Forms.Application;

namespace HRAdmin.UserControl
{
    public partial class UC_Meal_ViewReport : System.Windows.Forms.UserControl
    {
        private string eventDetails;
        private DateTime eventDate;
        private DateTime DeliveryTime;
        private string loggedInUser;
        private string loggedInDepart;
        private string EventDetails;
        private string EventTime;
        private string selectedOccasion;
        private bool isFiltering; // Prevent multiple simultaneous filter operations

        public UC_Meal_ViewReport(string username, string department)
        {
            InitializeComponent();
            loggedInUser = username;
            loggedInDepart = department;

            comboBox1.Items.AddRange(new string[] { "Internal", "External" });
            comboBox1.SelectedIndex = -1;
            selectedOccasion = null;
            isFiltering = false;

            // Ensure ValueChanged event is wired
            dtpToDate.ValueChanged += dtpToDate_ValueChanged;
            dtpFromDate.ValueChanged += dtpFromDate_ValueChanged; // Add FromDate event to trigger filtering
        }

        private bool CheckUserAccess(string username)
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
                                return AA == "1" || MA == "2"; // Return true if admin, false otherwise
                            }
                            return false; // User not found, treat as non-admin
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CheckUserAccess: Error - {ex.Message}");
                MessageBox.Show($"Error checking user access: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false; // On error, treat as non-admin
            }
        }

        private void PopulateOrderIds(DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (isFiltering)
            {
                Console.WriteLine("PopulateOrderIds: Already filtering, skipping");
                return;
            }

            isFiltering = true;
            try
            {
                if (string.IsNullOrEmpty(selectedOccasion))
                {
                    Console.WriteLine("PopulateOrderIds: No occasion selected");
                    MessageBox.Show("Please select an occasion (Internal or External) to load Order IDs.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbOrderIds.Items.Clear();
                    return;
                }

                bool isAdmin = CheckUserAccess(loggedInUser);
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                string tableName = selectedOccasion == "Internal" ? "tbl_InternalFoodOrder" : "tbl_ExternalFoodOrder";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Base query
                    string query = $"SELECT OrderID FROM {tableName} WHERE 1=1";

                    // Add RequesterID filter for non-admins
                    if (!isAdmin)
                    {
                        query += " AND RequesterID = @RequesterID";
                    }

                    // Add date filters if provided
                    if (fromDate.HasValue && toDate.HasValue)
                    {
                        query += " AND RequestDate BETWEEN @FromDate AND @ToDate";
                        Console.WriteLine($"PopulateOrderIds: Filtering from {fromDate.Value:yyyy-MM-dd} to {toDate.Value:yyyy-MM-dd} for {(isAdmin ? "all users" : $"user {loggedInUser}")}");
                    }
                    else if (fromDate.HasValue)
                    {
                        query += " AND RequestDate >= @FromDate";
                        Console.WriteLine($"PopulateOrderIds: Filtering from {fromDate.Value:yyyy-MM-dd} for {(isAdmin ? "all users" : $"user {loggedInUser}")}");
                    }
                    else if (toDate.HasValue)
                    {
                        query += " AND RequestDate <= @ToDate";
                        Console.WriteLine($"PopulateOrderIds: Filtering to {toDate.Value:yyyy-MM-dd} for {(isAdmin ? "all users" : $"user {loggedInUser}")}");
                    }
                    else
                    {
                        Console.WriteLine($"PopulateOrderIds: No date filters applied for {(isAdmin ? "all users" : $"user {loggedInUser}")}");
                    }

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add RequesterID parameter for non-admins
                        if (!isAdmin)
                        {
                            cmd.Parameters.AddWithValue("@RequesterID", loggedInUser);
                        }

                        // Add date parameters if applicable
                        if (fromDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@FromDate", fromDate.Value.Date);
                        }
                        if (toDate.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@ToDate", toDate.Value.Date.AddDays(1).AddSeconds(-1)); // Include entire ToDate
                        }

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            cmbOrderIds.Items.Clear();
                            int count = 0;
                            while (reader.Read())
                            {
                                cmbOrderIds.Items.Add(reader["OrderID"].ToString());
                                count++;
                            }
                            Console.WriteLine($"PopulateOrderIds: Found {count} Order IDs for {(isAdmin ? "all users" : $"user {loggedInUser}")}");
                            if (cmbOrderIds.Items.Count > 0)
                            {
                                cmbOrderIds.SelectedIndex = 0;
                                Console.WriteLine("PopulateOrderIds: Set first Order ID as selected");
                            }
                            else
                            {
                                Console.WriteLine("PopulateOrderIds: No Order IDs found");
                                MessageBox.Show(isAdmin ? "No orders found for the selected occasion and date range." : "No orders found for your account in the selected occasion and date range.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PopulateOrderIds: Error - {ex.Message}");
                MessageBox.Show($"Error loading Order IDs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                isFiltering = false;
                cmbOrderIds.Refresh(); // Force UI refresh
                Console.WriteLine("PopulateOrderIds: Filtering completed");
            }
        }

        private byte[] GenerateInternalPDF(string orderId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                Dictionary<string, object> orderDetails = new Dictionary<string, object>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                     SELECT OrderID, RequesterID, Department, OccasionType, RequestDate, DeliveryDate, EventDetails,
                     Menu, Fruit, Snack, Drink1, HOTorCOLD1, Drink2, HOTorCOLD2, No_pax, DeliveryTime, Remark, DeliveryPlace,
                     CheckStatus, CheckedBy, CheckedDate, ApproveStatus, ApprovedBy, ApprovedDate, CheckedDepartment, ApprovedDepartment
                     FROM tbl_InternalFoodOrder
                     WHERE OrderID = @OrderID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orderDetails["OrderID"] = reader["OrderID"].ToString();
                                orderDetails["RequesterID"] = reader["RequesterID"].ToString();
                                orderDetails["Department"] = reader["Department"].ToString();
                                orderDetails["OccasionType"] = reader["OccasionType"].ToString();
                                orderDetails["RequestDate"] = reader["RequestDate"];
                                orderDetails["DeliveryDate"] = reader["DeliveryDate"];
                                orderDetails["EventDetails"] = reader["EventDetails"].ToString();
                                orderDetails["Menu"] = reader["Menu"] != DBNull.Value ? reader["Menu"].ToString() : "-";
                                orderDetails["Fruit"] = reader["Fruit"] != DBNull.Value ? reader["Fruit"].ToString() : "-";
                                orderDetails["Snack"] = reader["Snack"] != DBNull.Value ? reader["Snack"].ToString() : "-";
                                orderDetails["Drink1"] = reader["Drink1"] != DBNull.Value ? reader["Drink1"].ToString() : "-";
                                orderDetails["HOTorCOLD1"] = reader["HOTorCOLD1"] != DBNull.Value ? reader["HOTorCOLD1"].ToString() : "-";
                                orderDetails["Drink2"] = reader["Drink2"] != DBNull.Value ? reader["Drink2"].ToString() : "-";
                                orderDetails["HOTorCOLD2"] = reader["HOTorCOLD2"] != DBNull.Value ? reader["HOTorCOLD2"].ToString() : "-";
                                orderDetails["No_pax"] = reader["No_pax"] != DBNull.Value ? reader["No_pax"].ToString() : "-";
                                orderDetails["DeliveryTime"] = reader["DeliveryTime"] != DBNull.Value ? reader["DeliveryTime"].ToString() : "-";
                                orderDetails["DeliveryPlace"] = reader["DeliveryPlace"] != DBNull.Value ? reader["DeliveryPlace"].ToString() : "-";
                                orderDetails["Remark"] = reader["Remark"] != DBNull.Value ? reader["Remark"].ToString() : "-";
                                orderDetails["CheckStatus"] = reader["CheckStatus"] != DBNull.Value ? reader["CheckStatus"].ToString() : "";
                                orderDetails["CheckedBy"] = reader["CheckedBy"] != DBNull.Value ? reader["CheckedBy"].ToString() : "";
                                orderDetails["CheckedDate"] = reader["CheckedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CheckedDate"]).ToString("dd.MM.yyyy HH:mm") : "";
                                orderDetails["ApproveStatus"] = reader["ApproveStatus"] != DBNull.Value ? reader["ApproveStatus"].ToString() : "";
                                orderDetails["ApprovedBy"] = reader["ApprovedBy"] != DBNull.Value ? reader["ApprovedBy"].ToString() : "";
                                orderDetails["ApprovedDate"] = reader["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ApprovedDate"]).ToString("dd.MM.yyyy HH:mm") : "";
                                orderDetails["CheckedDepartment"] = reader["CheckedDepartment"] != DBNull.Value ? reader["CheckedDepartment"].ToString() : "-";
                                orderDetails["ApprovedDepartment"] = reader["ApprovedDepartment"] != DBNull.Value ? reader["ApprovedDepartment"].ToString() : "-";
                            }
                            else
                            {
                                MessageBox.Show("Order not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return null;
                            }
                        }
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4.Rotate(), 36f, 36f, 36f, 36f);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new PdfPageEventHelper();
                    document.Open();

                    iTextSharp.text.Font titleFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font addressFont = FontFactory.GetFont("Helvetica", 8f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font titleFont1 = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font headerFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font bodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font italicBodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.ITALIC, BaseColor.BLACK);

                    string imagePath = Path.Combine(WinFormsApp.StartupPath, "Img", "hosiden.jpg");
                    //string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo hosiden.jpg");
                    if (File.Exists(imagePath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(imagePath);
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

                    Paragraph titlePara = new Paragraph();
                    titlePara.Add(new Chunk("HOSIDEN ELECTRONICS (M) SDN BHD (198901000700)\n", titleFont));
                    titlePara.Add(new Chunk("Lot 1, Jalan P/1A, Bangi Industrial Estate, 43650 Bandar Baru Bangi, Selangor, Malaysia\n", addressFont));
                    titlePara.Add(new Chunk("\n", addressFont));
                    titlePara.Add(new Chunk("CANTEEN MEAL REQUEST FORM", titleFont));
                    titlePara.Alignment = Element.ALIGN_CENTER;
                    titlePara.SpacingBefore = 0f;
                    titlePara.SpacingAfter = 5f;
                    document.Add(titlePara);

                    PdfPTable mainLayoutTable = new PdfPTable(2);
                    mainLayoutTable.WidthPercentage = 100;
                    mainLayoutTable.SetWidths(new float[] { 1.5f, 0.8f });
                    mainLayoutTable.SpacingBefore = 10f;

                    PdfPCell leftCell = new PdfPCell();
                    leftCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    leftCell.Padding = 0f;

                    PdfPTable detailsTable = new PdfPTable(4);
                    detailsTable.WidthPercentage = 100;
                    detailsTable.SetWidths(new float[] { 0.22f, 0.5f, 0.24f, 0.7f });
                    detailsTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    detailsTable.DefaultCell.Padding = 2f;
                    detailsTable.SpacingBefore = 5f;

                    detailsTable.AddCell(new Phrase("OrderID      :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["OrderID"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Request date:", bodyFont));
                    detailsTable.AddCell(new Phrase(Convert.ToDateTime(orderDetails["RequestDate"]).ToString("dd.MM.yyyy"), bodyFont));

                    detailsTable.AddCell(new Phrase("Requester  :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["RequesterID"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Delivery date:", bodyFont));
                    detailsTable.AddCell(new Phrase(Convert.ToDateTime(orderDetails["DeliveryDate"]).ToString("dd.MM.yyyy"), bodyFont));

                    detailsTable.AddCell(new Phrase("Department:", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["Department"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Event details:", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["EventDetails"].ToString(), bodyFont));

                    leftCell.AddElement(detailsTable);

                    PdfPCell rightCell = new PdfPCell();
                    rightCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    rightCell.Padding = 0f;

                    Paragraph checkedPara = new Paragraph();
                    string checkedBy = orderDetails["CheckedBy"].ToString();
                    string checkedDate = orderDetails["CheckedDate"].ToString();
                    string checkedDepartment = orderDetails["CheckedDepartment"].ToString();
                    if (string.IsNullOrEmpty(checkedBy))
                    {
                        checkedPara.Add(new Chunk("Checked by : Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(checkedDate))
                        {
                            checkedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                        }
                        string checkedText = string.IsNullOrEmpty(checkedDepartment) || checkedDepartment == "-"
                            ? $"Checked by : {checkedBy}   {checkedDate}"
                            : $"Checked by : {checkedBy}   {checkedDate}   \n                      {checkedDepartment}";
                        checkedPara.Add(new Chunk(checkedText, bodyFont));
                    }
                    checkedPara.SpacingBefore = 0f;
                    rightCell.AddElement(checkedPara);

                    Paragraph checkedAdminPara = new Paragraph();
                    checkedAdminPara.Add(new Chunk("", bodyFont));
                    checkedAdminPara.SpacingBefore = 0f;
                    checkedAdminPara.SpacingAfter = 0f;
                    rightCell.AddElement(checkedAdminPara);

                    Paragraph approvedPara = new Paragraph();
                    string approvedBy = orderDetails["ApprovedBy"].ToString();
                    string approvedDate = orderDetails["ApprovedDate"].ToString();
                    string approvedDepartment = orderDetails["ApprovedDepartment"].ToString();
                    if (string.IsNullOrEmpty(approvedBy))
                    {
                        approvedPara.Add(new Chunk("Approved by: Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(approvedDate))
                        {
                            approvedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                        }
                        string approvedText = string.IsNullOrEmpty(approvedDepartment) || approvedDepartment == "-"
                            ? $"Approved by: {approvedBy}   {approvedDate}"
                            : $"Approved by: {approvedBy}   {approvedDate}   \n                      {approvedDepartment}";
                        approvedPara.Add(new Chunk(approvedText, bodyFont));
                    }
                    approvedPara.SpacingBefore = 0f;
                    rightCell.AddElement(approvedPara);

                    Paragraph approvedHrPara = new Paragraph();
                    approvedHrPara.Add(new Chunk("", bodyFont));
                    approvedHrPara.SpacingBefore = 0f;
                    approvedHrPara.SpacingAfter = 0f;
                    rightCell.AddElement(approvedHrPara);

                    Paragraph issuedPara = new Paragraph();
                    issuedPara.Add(new Chunk("Received by: Canteen", bodyFont));
                    issuedPara.SpacingBefore = 0f;
                    issuedPara.SpacingAfter = 0f;
                    rightCell.AddElement(issuedPara);

                    mainLayoutTable.AddCell(leftCell);
                    mainLayoutTable.AddCell(rightCell);
                    document.Add(mainLayoutTable);

                    Paragraph detailsHeading = new Paragraph("Details of the order:", bodyFont);
                    detailsHeading.SpacingBefore = 10f;
                    detailsHeading.SpacingAfter = 5f;
                    document.Add(detailsHeading);

                    PdfPTable detailsTable2 = new PdfPTable(2);
                    detailsTable2.WidthPercentage = 100;
                    detailsTable2.SetWidths(new float[] { 0.5f, 3f });

                    string mealType = "-";
                    if (!string.IsNullOrEmpty(orderDetails["Menu"].ToString()))
                    {
                        string menu = orderDetails["Menu"].ToString().ToLower();
                        if (menu.Contains("breakfast") || menu.Contains("nasi lemak") || menu.Contains("telur"))
                            mealType = "Breakfast";
                        else if (menu.Contains("lunch") || menu.Contains("ayam") || menu.Contains("ikan"))
                            mealType = "Lunch";
                        else if (menu.Contains("tea"))
                            mealType = "Tea";
                        else if (menu.Contains("dinner") || menu.Contains("goreng"))
                            mealType = "Dinner";
                    }

                    AddStyledTableRow(detailsTable2, "Meal Type:", mealType, bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Dish:", orderDetails["Menu"].ToString(), bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Fruit:", orderDetails["Fruit"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Other:", orderDetails["Snack"].ToString(), bodyFont, italicBodyFont, 0);

                    string drink1Value = orderDetails["Drink1"].ToString();
                    string hotCold1Value = orderDetails["HOTorCOLD1"].ToString();
                    string combinedDrink1Value = drink1Value;
                    if (hotCold1Value != "-" && drink1Value != "-")
                    {
                        combinedDrink1Value = $"{drink1Value} ({hotCold1Value})";
                    }
                    AddStyledTableRow(detailsTable2, "Drink1 (Hot/Cold):", combinedDrink1Value, bodyFont, italicBodyFont, 1);

                    string drink2Value = orderDetails["Drink2"].ToString();
                    string hotCold2Value = orderDetails["HOTorCOLD2"].ToString();
                    string combinedDrink2Value = drink2Value;
                    if (hotCold2Value != "-" && drink2Value != "-")
                    {
                        combinedDrink2Value = $"{drink2Value} ({hotCold2Value})";
                    }
                    AddStyledTableRow(detailsTable2, "Drink2 (Hot/Cold):", combinedDrink2Value, bodyFont, italicBodyFont, 0);

                    AddStyledTableRow(detailsTable2, "No. of Pax:", orderDetails["No_pax"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Delivery Time:", orderDetails["DeliveryTime"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Delivery Place:", orderDetails["DeliveryPlace"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Remarks:", orderDetails["Remark"].ToString(), bodyFont, italicBodyFont, 0, true);

                    document.Add(detailsTable2);

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

        private byte[] GenerateExternalPDF(string orderId)
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString;
                Dictionary<string, object> orderDetails = new Dictionary<string, object>();
                Dictionary<string, List<string>> menuItems = new Dictionary<string, List<string>>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
        SELECT OrderID, RequesterID, Department, OccasionType, RequestDate, DeliveryDate, EventDetails,
               B_Nofpax_P, B_DeliveryTime, L_Nofpax_B, L_Nofpax_P, L_DeliveryTime, T_Nofpax_P, T_DeliveryTime, Remark, DeliveryPlace,
               CheckStatus, CheckedBy, CheckedDate, ApproveStatus, ApprovedBy, ApprovedDate, CheckedDepartment, ApprovedDepartment, BreakfastPackage, LunchPackage, TeaPackage
        FROM tbl_ExternalFoodOrder
        WHERE OrderID = @OrderID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orderDetails["OrderID"] = reader["OrderID"].ToString();
                                orderDetails["RequesterID"] = reader["RequesterID"].ToString();
                                orderDetails["Department"] = reader["Department"].ToString();
                                orderDetails["OccasionType"] = reader["OccasionType"].ToString();
                                orderDetails["RequestDate"] = reader["RequestDate"];
                                orderDetails["DeliveryDate"] = reader["DeliveryDate"];
                                orderDetails["EventDetails"] = reader["EventDetails"].ToString();
                                orderDetails["B_Nofpax_P"] = reader["B_Nofpax_P"] != DBNull.Value ? reader["B_Nofpax_P"].ToString() : "-";
                                orderDetails["B_DeliveryTime"] = reader["B_DeliveryTime"] != DBNull.Value ? reader["B_DeliveryTime"].ToString() : "-";
                                orderDetails["L_Nofpax_B"] = reader["L_Nofpax_B"] != DBNull.Value ? reader["L_Nofpax_B"].ToString() : "-";
                                orderDetails["L_Nofpax_P"] = reader["L_Nofpax_P"] != DBNull.Value ? reader["L_Nofpax_P"].ToString() : "-";
                                orderDetails["L_DeliveryTime"] = reader["L_DeliveryTime"] != DBNull.Value ? reader["L_DeliveryTime"].ToString() : "-";
                                orderDetails["T_Nofpax_P"] = reader["T_Nofpax_P"] != DBNull.Value ? reader["T_Nofpax_P"].ToString() : "-";
                                orderDetails["T_DeliveryTime"] = reader["T_DeliveryTime"] != DBNull.Value ? reader["T_DeliveryTime"].ToString() : "-";
                                orderDetails["DeliveryPlace"] = reader["DeliveryPlace"] != DBNull.Value ? reader["DeliveryPlace"].ToString() : "-";
                                orderDetails["Remark"] = reader["Remark"] != DBNull.Value ? reader["Remark"].ToString() : "-";
                                orderDetails["CheckStatus"] = reader["CheckStatus"] != DBNull.Value ? reader["CheckStatus"].ToString() : "";
                                orderDetails["CheckedBy"] = reader["CheckedBy"] != DBNull.Value ? reader["CheckedBy"].ToString() : "";
                                orderDetails["CheckedDate"] = reader["CheckedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CheckedDate"]).ToString("dd.MM.yyyy HH:mm") : "";
                                orderDetails["ApproveStatus"] = reader["ApproveStatus"] != DBNull.Value ? reader["ApproveStatus"].ToString() : "";
                                orderDetails["ApprovedBy"] = reader["ApprovedBy"] != DBNull.Value ? reader["ApprovedBy"].ToString() : "";
                                orderDetails["ApprovedDate"] = reader["ApprovedDate"] != DBNull.Value ? Convert.ToDateTime(reader["ApprovedDate"]).ToString("dd.MM.yyyy HH:mm") : "";
                                orderDetails["CheckedDepartment"] = reader["CheckedDepartment"] != DBNull.Value ? reader["CheckedDepartment"].ToString() : "-";
                                orderDetails["ApprovedDepartment"] = reader["ApprovedDepartment"] != DBNull.Value ? reader["ApprovedDepartment"].ToString() : "-";
                                orderDetails["BreakfastPackage"] = reader["BreakfastPackage"] != DBNull.Value ? reader["BreakfastPackage"].ToString() : "-";
                                orderDetails["LunchPackage"] = reader["LunchPackage"] != DBNull.Value ? reader["LunchPackage"].ToString() : "-";
                                orderDetails["TeaPackage"] = reader["TeaPackage"] != DBNull.Value ? reader["TeaPackage"].ToString() : "-";
                            }
                            else
                            {
                                MessageBox.Show("Order not found in the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return null;
                            }
                        }
                    }

                    string breakfastPackage = orderDetails["BreakfastPackage"].ToString();
                    string lunchPackage = orderDetails["LunchPackage"].ToString();
                    string teaPackage = orderDetails["TeaPackage"].ToString();

                    // Only fetch menu items for packages that are not "-"
                    string[] meals = { "BREAKFAST", "LUNCH", "TEA" };
                    string[] styles = { "PACKING", "BUFFET" };
                    foreach (var meal in meals)
                    {
                        foreach (var style in styles)
                        {
                            if (meal == "BREAKFAST" && style == "BUFFET") continue;
                            if (meal == "TEA" && style == "BUFFET") continue;
                            string package = meal == "BREAKFAST" ? breakfastPackage : meal == "LUNCH" ? lunchPackage : teaPackage;
                            // Skip fetching if the package is "-"
                            if (package == "-") continue;

                            string menuQuery = @"
                SELECT Menu FROM tbl_Menu
                WHERE Package = @Package AND Meal = @Meal AND Style = @Style";
                            using (SqlCommand cmd = new SqlCommand(menuQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@Package", package);
                                cmd.Parameters.AddWithValue("@Meal", meal);
                                cmd.Parameters.AddWithValue("@Style", style);
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    List<string> items = new List<string>();
                                    while (reader.Read())
                                    {
                                        items.Add(reader["Menu"].ToString());
                                    }
                                    menuItems[$"{meal}_{style}"] = items;
                                }
                            }
                        }
                    }
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4.Rotate(), 36f, 36f, 36f, 36f);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new PdfPageEventHelper();
                    document.Open();

                    iTextSharp.text.Font titleFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font addressFont = FontFactory.GetFont("Helvetica", 8f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font titleFont1 = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font headerFont = FontFactory.GetFont("Helvetica", 12f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                    iTextSharp.text.Font bodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                    iTextSharp.text.Font italicBodyFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.ITALIC, BaseColor.BLACK);
                    iTextSharp.text.Font sectionTitleFont = FontFactory.GetFont("Helvetica", 10f, iTextSharp.text.Font.BOLD, new BaseColor(0, 51, 102));
                    iTextSharp.text.Font mealsHeadingFont = FontFactory.GetFont("Helvetica", 13f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

                    BaseColor lightGray = new BaseColor(240, 240, 240);
                    BaseColor darkGray = new BaseColor(150, 150, 150);

                    string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logo hosiden.jpg");
                    if (File.Exists(logoPath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
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

                    Paragraph titlePara = new Paragraph();
                    titlePara.Add(new Chunk("HOSIDEN ELECTRONICS (M) SDN BHD (198901000700)\n", titleFont));
                    titlePara.Add(new Chunk("Lot 1, Jalan P/1A, Bangi Industrial Estate, 43650 Bandar Baru Bangi, Selangor, Malaysia\n", addressFont));
                    titlePara.Add(new Chunk("\n", addressFont));
                    titlePara.Add(new Chunk("CANTEEN MEAL REQUEST FORM", titleFont));
                    titlePara.Alignment = Element.ALIGN_CENTER;
                    titlePara.SpacingBefore = 0f;
                    titlePara.SpacingAfter = 5f;
                    document.Add(titlePara);

                    PdfPTable mainLayoutTable = new PdfPTable(2);
                    mainLayoutTable.WidthPercentage = 100;
                    mainLayoutTable.SetWidths(new float[] { 1.5f, 0.8f });
                    mainLayoutTable.SpacingBefore = 10f;

                    PdfPCell leftCell = new PdfPCell();
                    leftCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    leftCell.Padding = 0f;

                    PdfPTable detailsTable = new PdfPTable(4);
                    detailsTable.WidthPercentage = 100;
                    detailsTable.SetWidths(new float[] { 0.22f, 0.5f, 0.24f, 0.7f });
                    detailsTable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    detailsTable.DefaultCell.Padding = 2f;
                    detailsTable.SpacingBefore = 5f;

                    detailsTable.AddCell(new Phrase("OrderID      :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["OrderID"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Request date:", bodyFont));
                    detailsTable.AddCell(new Phrase(Convert.ToDateTime(orderDetails["RequestDate"]).ToString("dd.MM.yyyy"), bodyFont));

                    detailsTable.AddCell(new Phrase("Requester  :", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["RequesterID"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Delivery date:", bodyFont));
                    detailsTable.AddCell(new Phrase(Convert.ToDateTime(orderDetails["DeliveryDate"]).ToString("dd.MM.yyyy"), bodyFont));

                    detailsTable.AddCell(new Phrase("Department:", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["Department"].ToString(), bodyFont));
                    detailsTable.AddCell(new Phrase("Event details:", bodyFont));
                    detailsTable.AddCell(new Phrase(orderDetails["EventDetails"].ToString(), bodyFont));

                    leftCell.AddElement(detailsTable);

                    PdfPCell rightCell = new PdfPCell();
                    rightCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    rightCell.Padding = 0f;

                    Paragraph checkedPara = new Paragraph();
                    string checkedBy = orderDetails["CheckedBy"].ToString();
                    string checkedDate = orderDetails["CheckedDate"].ToString();
                    string checkedDepartment = orderDetails["CheckedDepartment"].ToString();
                    if (string.IsNullOrEmpty(checkedBy))
                    {
                        checkedPara.Add(new Chunk("Checked by : Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(checkedDate))
                        {
                            checkedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                        }
                        string checkedText = string.IsNullOrEmpty(checkedDepartment) || checkedDepartment == "-"
                            ? $"Checked by : {checkedBy}   {checkedDate}"
                            : $"Checked by : {checkedBy}   {checkedDate}   \n                      {checkedDepartment}";
                        checkedPara.Add(new Chunk(checkedText, bodyFont));
                    }
                    checkedPara.SpacingBefore = 0f;
                    rightCell.AddElement(checkedPara);

                    Paragraph checkedAdminPara = new Paragraph();
                    checkedAdminPara.Add(new Chunk("", bodyFont));
                    checkedAdminPara.SpacingBefore = 0f;
                    checkedAdminPara.SpacingAfter = 0f;
                    rightCell.AddElement(checkedAdminPara);

                    Paragraph approvedPara = new Paragraph();
                    string approvedBy = orderDetails["ApprovedBy"].ToString();
                    string approvedDate = orderDetails["ApprovedDate"].ToString();
                    string approvedDepartment = orderDetails["ApprovedDepartment"].ToString();
                    if (string.IsNullOrEmpty(approvedBy))
                    {
                        approvedPara.Add(new Chunk("Approved by: Pending", bodyFont));
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(approvedDate))
                        {
                            approvedDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                        }
                        string approvedText = string.IsNullOrEmpty(approvedDepartment) || approvedDepartment == "-"
                            ? $"Approved by: {approvedBy}   {approvedDate}"
                            : $"Approved by: {approvedBy}   {approvedDate}   \n                      {approvedDepartment}";
                        approvedPara.Add(new Chunk(approvedText, bodyFont));
                    }
                    approvedPara.SpacingBefore = 0f;
                    rightCell.AddElement(approvedPara);

                    Paragraph approvedHrPara = new Paragraph();
                    approvedHrPara.Add(new Chunk("", bodyFont));
                    approvedHrPara.SpacingBefore = 0f;
                    approvedHrPara.SpacingAfter = 0f;
                    rightCell.AddElement(approvedHrPara);

                    Paragraph issuedPara = new Paragraph();
                    issuedPara.Add(new Chunk("Received by: Canteen", bodyFont));
                    issuedPara.SpacingBefore = 0f;
                    issuedPara.SpacingAfter = 0f;
                    rightCell.AddElement(issuedPara);

                    mainLayoutTable.AddCell(leftCell);
                    mainLayoutTable.AddCell(rightCell);
                    document.Add(mainLayoutTable);

                    Paragraph detailsHeading = new Paragraph("Details of the Order:", bodyFont);
                    detailsHeading.SpacingBefore = 10f;
                    detailsHeading.SpacingAfter = 5f;
                    document.Add(detailsHeading);

                    string breakfastPackage = orderDetails["BreakfastPackage"].ToString();
                    string lunchPackage = orderDetails["LunchPackage"].ToString();
                    string teaPackage = orderDetails["TeaPackage"].ToString();

                    PdfPTable detailsTable2 = new PdfPTable(2);
                    detailsTable2.WidthPercentage = 100;
                    detailsTable2.SetWidths(new float[] { 0.8f, 3f });

                    AddStyledTableRow(detailsTable2, "Breakfast Package:", breakfastPackage, bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Breakfast Packing Pax:", orderDetails["B_Nofpax_P"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Breakfast Delivery Time:", orderDetails["B_DeliveryTime"].ToString(), bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Lunch Package:", lunchPackage, bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Lunch Buffet Pax:", orderDetails["L_Nofpax_B"].ToString(), bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Lunch Packing Pax:", orderDetails["L_Nofpax_P"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Lunch Delivery Time:", orderDetails["L_DeliveryTime"].ToString(), bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Tea Package:", teaPackage, bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Tea Packing Pax:", orderDetails["T_Nofpax_P"].ToString(), bodyFont, italicBodyFont, 0);
                    AddStyledTableRow(detailsTable2, "Tea Delivery Time:", orderDetails["T_DeliveryTime"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Delivery Place:", orderDetails["DeliveryPlace"].ToString(), bodyFont, italicBodyFont, 1);
                    AddStyledTableRow(detailsTable2, "Remarks:", orderDetails["Remark"].ToString(), bodyFont, italicBodyFont, 0, true);

                    document.Add(detailsTable2);

                    // Only proceed with food items if at least one package is selected
                    if (breakfastPackage != "-" || lunchPackage != "-" || teaPackage != "-")
                    {
                        document.NewPage();

                        PdfPTable foodItemsTable = new PdfPTable(2);
                        foodItemsTable.WidthPercentage = 100;
                        foodItemsTable.SetWidths(new float[] { 1f, 1f });
                        foodItemsTable.SpacingBefore = 15f;
                        foodItemsTable.DefaultCell.BorderColor = darkGray;
                        foodItemsTable.KeepTogether = true;

                        PdfPCell headingCell = new PdfPCell(new Phrase($"Meals for Packages - Breakfast: {breakfastPackage}, Lunch: {lunchPackage}, Tea: {teaPackage}", mealsHeadingFont));
                        headingCell.Colspan = 2;
                        headingCell.BackgroundColor = BaseColor.WHITE;
                        headingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                        headingCell.BorderColor = darkGray;
                        headingCell.BorderWidth = 1.5f;
                        headingCell.Padding = 5f;
                        headingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        foodItemsTable.AddCell(headingCell);

                        // Conditionally add Breakfast Packing section
                        if (breakfastPackage != "-")
                        {
                            PdfPCell breakfastHeadingCell = new PdfPCell(new Phrase($"BREAKFAST PACKING: {orderDetails["B_Nofpax_P"]}", sectionTitleFont));
                            breakfastHeadingCell.Colspan = 2;
                            breakfastHeadingCell.BackgroundColor = BaseColor.WHITE;
                            breakfastHeadingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            breakfastHeadingCell.BorderColor = darkGray;
                            breakfastHeadingCell.BorderWidth = 1.5f;
                            breakfastHeadingCell.Padding = 5f;
                            breakfastHeadingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            foodItemsTable.AddCell(breakfastHeadingCell);

                            PdfPCell breakfastCell = new PdfPCell();
                            breakfastCell.Colspan = 2;
                            breakfastCell.BackgroundColor = BaseColor.WHITE;
                            breakfastCell.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            breakfastCell.BorderColor = darkGray;
                            breakfastCell.BorderWidth = 1.5f;
                            breakfastCell.Padding = 5f;
                            AddFoodItemsToCell(breakfastCell, menuItems["BREAKFAST_PACKING"], italicBodyFont);
                            foodItemsTable.AddCell(breakfastCell);
                        }

                        // Conditionally add Lunch section
                        if (lunchPackage != "-")
                        {
                            PdfPCell lunchHeadingCell = new PdfPCell(new Phrase("LUNCH", sectionTitleFont));
                            lunchHeadingCell.Colspan = 2;
                            lunchHeadingCell.BackgroundColor = BaseColor.WHITE;
                            lunchHeadingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            lunchHeadingCell.BorderColor = darkGray;
                            lunchHeadingCell.BorderWidth = 1.5f;
                            lunchHeadingCell.Padding = 5f;
                            lunchHeadingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            foodItemsTable.AddCell(lunchHeadingCell);

                            PdfPCell lunchBuffetCell = new PdfPCell();
                            lunchBuffetCell.BackgroundColor = BaseColor.WHITE;
                            lunchBuffetCell.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            lunchBuffetCell.BorderColor = darkGray;
                            lunchBuffetCell.BorderWidth = 1.5f;
                            lunchBuffetCell.Padding = 5f;
                            Paragraph lunchBuffetTitle = new Paragraph($"BUFFET: {orderDetails["L_Nofpax_B"]}", sectionTitleFont);
                            lunchBuffetTitle.SpacingAfter = 2f;
                            lunchBuffetCell.AddElement(lunchBuffetTitle);
                            AddFoodItemsToCell(lunchBuffetCell, menuItems["LUNCH_BUFFET"], italicBodyFont);
                            foodItemsTable.AddCell(lunchBuffetCell);

                            PdfPCell lunchPackingCell = new PdfPCell();
                            lunchPackingCell.BackgroundColor = BaseColor.WHITE;
                            lunchPackingCell.Border = iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            lunchPackingCell.BorderColor = darkGray;
                            lunchPackingCell.BorderWidth = 1.5f;
                            lunchPackingCell.Padding = 5f;
                            Paragraph lunchPackingTitle = new Paragraph($"PACKING: {orderDetails["L_Nofpax_P"]}", sectionTitleFont);
                            lunchPackingTitle.SpacingAfter = 2f;
                            lunchPackingCell.AddElement(lunchPackingTitle);
                            AddFoodItemsToCell(lunchPackingCell, menuItems["LUNCH_PACKING"], italicBodyFont);
                            foodItemsTable.AddCell(lunchPackingCell);
                        }

                        // Conditionally add Tea Packing section
                        if (teaPackage != "-")
                        {
                            PdfPCell teaHeadingCell = new PdfPCell(new Phrase($"TEA PACKING: {orderDetails["T_Nofpax_P"]}", sectionTitleFont));
                            teaHeadingCell.Colspan = 2;
                            teaHeadingCell.BackgroundColor = BaseColor.WHITE;
                            teaHeadingCell.Border = iTextSharp.text.Rectangle.TOP_BORDER | iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            teaHeadingCell.BorderColor = darkGray;
                            teaHeadingCell.BorderWidth = 1.5f;
                            teaHeadingCell.Padding = 5f;
                            teaHeadingCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            foodItemsTable.AddCell(teaHeadingCell);

                            PdfPCell teaCell = new PdfPCell();
                            teaCell.Colspan = 2;
                            teaCell.BackgroundColor = BaseColor.WHITE;
                            teaCell.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER | iTextSharp.text.Rectangle.BOTTOM_BORDER;
                            teaCell.BorderColor = darkGray;
                            teaCell.BorderWidth = 1.5f;
                            teaCell.Padding = 5f;
                            AddFoodItemsToCell(teaCell, menuItems["TEA_PACKING"], italicBodyFont);
                            foodItemsTable.AddCell(teaCell);
                        }

                        document.Add(foodItemsTable);
                    }

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

        private void AddFoodItemsToCell(PdfPCell cell, List<string> items, iTextSharp.text.Font font)
        {
            foreach (var item in items)
            {
                Paragraph p = new Paragraph($"- {item}", font);
                p.SpacingBefore = 1f;
                p.SpacingAfter = 1f;
                cell.AddElement(p);
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

        private void ViewExistingPDF()
        {
            if (string.IsNullOrEmpty(selectedOccasion))
            {
                MessageBox.Show("Please select an occasion (Internal or External) to view PDFs.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbOrderIds.Items.Count == 0)
            {
                MessageBox.Show("No report available. Please select an occasion and apply a date filter to load Order IDs.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbOrderIds.SelectedItem == null)
            {
                MessageBox.Show("Please select an Order ID to view the PDF.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string orderId = cmbOrderIds.SelectedItem.ToString();
            byte[] pdfBytes = selectedOccasion == "Internal" ? GenerateInternalPDF(orderId) : GenerateExternalPDF(orderId);
            ViewPdf(pdfBytes);
        }

        private void dtpFromDate_ValueChanged(object sender, EventArgs e)
        {
            Console.WriteLine("dtpFromDate_ValueChanged: Event triggered");
            ApplyDateFilter();
        }

        private void dtpToDate_ValueChanged(object sender, EventArgs e)
        {
            Console.WriteLine("dtpToDate_ValueChanged: Event triggered");
            ApplyDateFilter();
        }

        private void ApplyDateFilter()
        {
            if (isFiltering)
            {
                Console.WriteLine("ApplyDateFilter: Already filtering, skipping");
                return;
            }

            if (string.IsNullOrEmpty(selectedOccasion))
            {
                Console.WriteLine("ApplyDateFilter: No occasion selected");
                return;
            }

            DateTime fromDate = dtpFromDate.Value.Date;
            DateTime toDate = dtpToDate.Value.Date;

            Console.WriteLine($"ApplyDateFilter: From {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");

            if (fromDate > toDate)
            {
                Console.WriteLine("ApplyDateFilter: Invalid date range");
                cmbOrderIds.Items.Clear(); // Clear ComboBox when date range is invalid
                MessageBox.Show("From Date cannot be later than To Date.", "Invalid Date Range", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PopulateOrderIds(fromDate, toDate);
        }

        private void btnViewPDF_Click(object sender, EventArgs e)
        {
            ViewExistingPDF();
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
            bool isAdmin = CheckUserAccess(loggedInUser);
            Form_Home.sharedLabel.Text = "Admin > Meal Request";
            Form_Home.sharedButton6.Visible = true;
            if (isAdmin)
            {
                Form_Home.sharedButton4.Visible = true;
                Form_Home.sharedButton5.Visible = true;
            }
            UC_Meal_Food ug = new UC_Meal_Food(EventDetails, EventTime, DeliveryTime, loggedInUser, loggedInDepart);
            addControls(ug);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedOccasion = comboBox1.SelectedItem?.ToString();
            Console.WriteLine($"comboBox1_SelectedIndexChanged: Selected occasion - {selectedOccasion}");
            cmbOrderIds.Items.Clear();
            if (!string.IsNullOrEmpty(selectedOccasion))
            {
                // Populate with date filters if both dates are set
                if (dtpFromDate.Value != dtpFromDate.MinDate && dtpToDate.Value != dtpToDate.MinDate)
                {
                    ApplyDateFilter();
                }
                else
                {
                    PopulateOrderIds(); // No date filters initially
                }
            }
            else
            {
                Console.WriteLine("comboBox1_SelectedIndexChanged: Cleared Order IDs");
            }
            cmbOrderIds.Refresh();
        }

        private void UC_ViewReport_Load(object sender, EventArgs e)
        {
            Console.WriteLine("UC_ViewReport_Load: Form loaded");
            dtpFromDate.Value = DateTime.Today; // Set default FromDate to today
            dtpToDate.Value = DateTime.Today; // Set default ToDate to today
            // Hide Menu Update buttons in this view
            if (Form_Home.sharedButton4 != null && Form_Home.sharedButton5 != null)
            {
                Form_Home.sharedButton4.Visible = false; // Hide "Menu Update (Internal)"
                Form_Home.sharedButton5.Visible = false; // Hide "Menu Update (External)"
            }
            // Add a label to indicate viewing scope
            bool isAdmin = CheckUserAccess(loggedInUser);
            var label = new Label
            {
                Text = isAdmin ? "Viewing all orders" : "Viewing your orders only",
                Location = new Point(10, 10),
                AutoSize = true
            };
            this.Controls.Add(label);
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

        private void cmbOrderIds_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}