using ERPIndia.Class.Helper;
using ERPIndia.StudentManagement.Repository;
using ERPK12Models.DTO;
using ERPK12Models.ViewModel.Enquiry;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ERPIndia.Controllers.EnquiryForm
{
    public class EnquiryFormController : BaseController
    {
        private readonly IEnquiryRepository _enquiryRepository;

        // Static font declarations
        private static PdfFont FONT_NORMAL;
        private static PdfFont FONT_BOLD;
        private static readonly Color BORDER_COLOR = ColorConstants.BLACK;

        public EnquiryFormController()
        {
            _enquiryRepository = new EnquiryRepository();
        }

        // GET: EnquiryForm/Generate - Now accepts enquiry ID to pre-fill form
        public async Task<ActionResult> Generate(Guid? enquiryId = null, bool inline = true)
        {
            var sessionId = CurrentSessionID;
            var tenantCode = TenantCode;
            var currentUserId = CurrentTenantUserID;
            try
            {
                StudentEnquiry enquiry = null;
                if (enquiryId.HasValue)
                {
                    enquiry = await _enquiryRepository.GetEnquiryByIdAsync(enquiryId.Value, sessionId, tenantCode);
                }

                // Create PDF
                MemoryStream stream = new MemoryStream();
                PdfWriter writer = null;
                PdfDocument pdf = null;
                Document document = null;

                try
                {
                    writer = new PdfWriter(stream);
                    writer.SetCloseStream(false);

                    pdf = new PdfDocument(writer);
                    pdf.SetDefaultPageSize(PageSize.A4);
                    pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new FooterEventHandler());

                    document = new Document(pdf);
                    document.SetMargins(10, 10, 20, 10);

                    // Initialize fonts
                    FONT_NORMAL = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    FONT_BOLD = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                    // Get values from session
                    string code = CommonLogic.GetSessionValue(StringConstants.TenantCode);
                    string img = CommonLogic.GetSessionValue(StringConstants.LogoImg);
                    string logoPath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetLogoImage(code, img));

                    // Add dynamic header
                    AddDynamicSchoolHeader(document, logoPath);

                    // Add enquiry form with data if available
                    AddEnquiryForm(document, enquiry);

                    // Close document
                    document.Close();
                    document = null;
                    pdf.Close();
                    pdf = null;
                    writer.Close();
                    writer = null;

                    // Reset stream position
                    stream.Position = 0;

                    // Set file name
                    string fileName = enquiry != null
                        ? $"StudentEnquiryForm_{enquiry.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf"
                        : $"StudentEnquiryForm_Blank_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                    // Set content disposition
                    if (inline)
                    {
                        Response.AddHeader("Content-Disposition", "inline; filename=" + fileName);
                    }
                    else
                    {
                        Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
                    }

                    return File(stream, "application/pdf");
                }
                catch (Exception ex)
                {
                    // Clean up resources
                    if (document != null) document.Close();
                    if (pdf != null) pdf.Close();
                    if (writer != null) writer.Close();
                    stream.Dispose();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Content("Error generating PDF: " + ex.Message);
            }
        }

        // UNIFIED PROFESSIONAL HEADER - Used for both Admission Form and Receipt
        private static void AddDynamicSchoolHeader(Document document, string logoPath = null)
        {
            // Create a single-column table for the entire header
            Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(Border.NO_BORDER)
                .SetMargin(0)
                .SetPadding(0);

            // Create a cell that will contain both logo and text
            Cell headerCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0);

            // Create an inner table for layout with three columns
            Table layoutTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 70, 15 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(Border.NO_BORDER)
                .SetMargin(0)
                .SetPadding(0);

            // Left cell for logo
            Cell logoCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(3)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            try
            {
                if (!string.IsNullOrEmpty(logoPath) && System.IO.File.Exists(logoPath))
                {
                    ImageData imageData = ImageDataFactory.Create(logoPath);
                    Image logoImage = new Image(imageData);

                    logoImage.SetAutoScale(true);
                    logoImage.SetMaxWidth(60);
                    logoImage.SetMaxHeight(60);
                    logoImage.SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    logoImage.SetMarginLeft(3);

                    logoCell.Add(logoImage);
                }
                else
                {
                    logoCell.Add(new Paragraph(""));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading logo: {ex.Message}");
                logoCell.Add(new Paragraph(""));
            }

            // Center cell for all text content
            Cell textCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPadding(3);

            // School name
            string schoolNameText = CommonLogic.GetSessionValue(StringConstants.PrintTitle) ?? "SCHOOL NAME";
            Paragraph schoolName = new Paragraph(schoolNameText)
                .SetFont(FONT_BOLD)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(2);

            // School address
            string addressText = CommonLogic.GetSessionValue(StringConstants.Line1) ?? "School Address";
            Paragraph schoolAddress = new Paragraph(addressText)
                .SetFont(FONT_NORMAL)
                .SetFontSize(9)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1);

            // Contact information
            string contactText = CommonLogic.GetSessionValue(StringConstants.Line2) ?? "Contact Information";
            Paragraph schoolContact = new Paragraph(contactText)
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1);

            // Add all text elements to the text cell
            textCell.Add(schoolName);
            textCell.Add(schoolAddress);
            textCell.Add(schoolContact);

            // Mobile numbers or Line3
            string line3Text = CommonLogic.GetSessionValue(StringConstants.Line3);
            if (!string.IsNullOrEmpty(line3Text))
            {
                Paragraph line3Para = new Paragraph(line3Text)
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(1);
                textCell.Add(line3Para);
            }

            // Tagline or Line4
            string line4Text = CommonLogic.GetSessionValue(StringConstants.Line4);
            if (!string.IsNullOrEmpty(line4Text))
            {
                Paragraph line4Para = new Paragraph(line4Text)
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(1);
                textCell.Add(line4Para);
            }

            // Session print
            string sessionprint = CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint);
            if (!string.IsNullOrEmpty(sessionprint))
            {
                Paragraph session = new Paragraph("Session: ( " + sessionprint + " )")
                    .SetFont(FONT_BOLD)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(1)
                    .SetMarginBottom(0);
                textCell.Add(session);
            }

            // Right cell for balance (empty)
            Cell rightCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(3);
            rightCell.Add(new Paragraph(""));

            // Add all cells to the layout table
            layoutTable.AddCell(logoCell);
            layoutTable.AddCell(textCell);
            layoutTable.AddCell(rightCell);

            // Add the layout table to the header cell
            headerCell.Add(layoutTable);

            // Add the header cell to the main table
            headerTable.AddCell(headerCell);

            // Add the complete header to the document
            document.Add(headerTable);

            // Add minimal spacing after the header
            document.Add(new Paragraph("").SetMarginBottom(0.5f));

            // Add the separator line
            document.Add(new LineSeparator(new SolidLine(1.0f)).SetMarginBottom(0.5f));
        }

        private static void AddEnquiryForm(Document document, StudentEnquiry enquiry = null)
        {
            // Create a table for the enquiry number and date
            Table enquiryInfoTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.34f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(2)
                .SetMarginBottom(3);

            // Enquiry No. (left side)
            Cell enquiryNoCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph enquiryNoPara = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetMargin(0);

            enquiryNoPara.Add(new Text("Enquiry NO.: ").SetFont(FONT_BOLD));
            enquiryNoPara.Add(new Text(enquiry?.EnqNo.ToString() ?? "__________________"));

            enquiryNoCell.Add(enquiryNoPara);

            // Title (center)
            Cell titleCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph formTitle = new Paragraph("ENQUIRY FORM")
                .SetFont(FONT_BOLD)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMargin(0);

            titleCell.Add(formTitle);

            // Enquiry Date (right side)
            Cell enquiryDateCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph enquiryDatePara = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetMargin(0);

            enquiryDatePara.Add(new Text("Enquiry DATE: ").SetFont(FONT_BOLD));
            enquiryDatePara.Add(new Text(enquiry?.EnquiryDate.ToString("dd-MM-yyyy") ?? "__________________"));

            enquiryDateCell.Add(enquiryDatePara);

            enquiryInfoTable.AddCell(enquiryNoCell);
            enquiryInfoTable.AddCell(titleCell);
            enquiryInfoTable.AddCell(enquiryDateCell);
            document.Add(enquiryInfoTable);

            // STUDENT INFORMATION SECTION
            AddSectionHeader(document, "STUDENT INFORMATION");

            // Student Name (full width)
            Table studentNameTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            studentNameTable.AddCell(CreateStyledFieldCell("STUDENT NAME", enquiry?.Student ?? ""));
            document.Add(studentNameTable);

            // Class, Gender, and No of Child row
            Table classGenderTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.34f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            classGenderTable.AddCell(CreateStyledFieldCell("APPLYING FOR CLASS", enquiry?.ApplyingForClass ?? ""));
            classGenderTable.AddCell(CreateStyledFieldCell("Gender", enquiry?.Gender ?? ""));
            classGenderTable.AddCell(CreateStyledFieldCell("No Of Child", enquiry?.NoOfChild?.ToString() ?? ""));
            document.Add(classGenderTable);

            // FAMILY INFORMATION SECTION
            AddSectionHeader(document, "FAMILY INFORMATION");

            // Father info row
            Table fatherTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            fatherTable.AddCell(CreateStyledFieldCell("FATHER NAME", enquiry?.Father ?? ""));
            fatherTable.AddCell(CreateStyledFieldCell("Mobile Number 01", enquiry?.Mobile1 ?? ""));
            document.Add(fatherTable);

            // Mother info row
            Table motherTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            motherTable.AddCell(CreateStyledFieldCell("MOTHER NAME", enquiry?.Mother ?? ""));
            motherTable.AddCell(CreateStyledFieldCell("Mobile Number 02", enquiry?.Mobile2 ?? ""));
            document.Add(motherTable);

            // ADDRESS INFORMATION SECTION
            AddSectionHeader(document, "ADDRESS INFORMATION");

            // Check if it's a blank form (no enquiry data)
            if (enquiry == null)
            {
                // Add expanded address fields for blank form
                AddBlankFormAddressSection(document);
            }
            else
            {
                // Full address row for filled form
                Table addressTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(0);
                addressTable.AddCell(CreateStyledFieldCell("FULL ADDRESS", enquiry.Address ?? "", 20));
                document.Add(addressTable);
            }

            // SCHOOL & FEE INFORMATION SECTION
            AddSectionHeader(document, "SCHOOL & FEE INFORMATION");

            // For blank form, add additional fields
            if (enquiry == null)
            {
                // Last school and Passed Class row
                Table schoolClassTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(0);
                schoolClassTable.AddCell(CreateStyledFieldCell("LAST SCHOOL", ""));
                schoolClassTable.AddCell(CreateStyledFieldCell("PASSED CLASS", ""));
                document.Add(schoolClassTable);

                // Relation and Source row
                Table relationSourceTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(0);
                relationSourceTable.AddCell(CreateStyledFieldCell("RELATION", ""));
                relationSourceTable.AddCell(CreateStyledFieldCell("SOURCE", ""));
                document.Add(relationSourceTable);

                // Note and Form Fees row
                Table noteFeesTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(0);
                noteFeesTable.AddCell(CreateStyledFieldCell("NOTE", ""));
                noteFeesTable.AddCell(CreateStyledFieldCell("FORM FEES", ""));
                document.Add(noteFeesTable);

                // Add Contact Preference Section for blank form
                AddContactPreferenceSection(document);
            }
            else
            {
                // Original layout for filled form
                // Last school and Deal by row
                Table schoolDealTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(0);
                schoolDealTable.AddCell(CreateStyledFieldCell("LAST SCHOOL", enquiry.PreviousSchool ?? ""));
                schoolDealTable.AddCell(CreateStyledFieldCell("DEAL BY", enquiry.DealBy ?? ""));
                document.Add(schoolDealTable);

                // Relation and Source row
                Table relationSourceTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(0);
                relationSourceTable.AddCell(CreateStyledFieldCell("RELATION", enquiry.Relation ?? ""));
                relationSourceTable.AddCell(CreateStyledFieldCell("SOURCE", enquiry.Source ?? ""));
                document.Add(relationSourceTable);

                // Note and Form Fees row
                Table noteFeesTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetMarginBottom(0);
                noteFeesTable.AddCell(CreateStyledFieldCell("NOTE", enquiry.Note ?? ""));
                noteFeesTable.AddCell(CreateStyledFieldCell("FORM FEES", enquiry.FormAmt?.ToString("0") ?? ""));
                document.Add(noteFeesTable);

                // PREVIOUS FOLLOW-UP DETAILS SECTION (only for filled forms)
                AddSectionHeader(document, "PREVIOUS FOLLOW-UP DETAILS");
                AddFollowUpTable(document, enquiry);
            }

            // Declaration section
            AddDeclarationSection(document);
        }
        private static void AddFollowUpTable(Document document, StudentEnquiry enquiry = null)
        {
            // Create the follow-up details table
            Table followUpTable = new Table(UnitValue.CreatePercentArray(new float[] { 8f, 15f, 15f, 15f, 38f, 15f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(3);

            // Add header row
            followUpTable.AddHeaderCell(CreateHeaderCell("SR."));
            followUpTable.AddHeaderCell(CreateHeaderCell("FOLLOW-UP DATE"));
            followUpTable.AddHeaderCell(CreateHeaderCell("FOLLOW-UP TIME"));
            followUpTable.AddHeaderCell(CreateHeaderCell("CALL STATUS"));
            followUpTable.AddHeaderCell(CreateHeaderCell("RESPONSE"));
            followUpTable.AddHeaderCell(CreateHeaderCell("NXT F/U DATE"));

            // Add follow-up data if available
            if (enquiry?.FollowUps != null && enquiry.FollowUps.Count > 0)
            {
                int rowNumber = 1;
                foreach (var followUp in enquiry.FollowUps)
                {
                    // SR. - Centered
                    followUpTable.AddCell(CreateCenteredTableCell(rowNumber.ToString("00")));

                    // FOLLOW-UP DATE - Centered
                    followUpTable.AddCell(CreateCenteredTableCell(followUp.FollowDate.ToString("dd-MM-yyyy") ?? ""));

                    // FOLLOW-UP TIME - Centered with AM/PM
                    string timeString = "";
                    if (followUp.FollowTime != null)
                    {
                        // Convert TimeSpan to DateTime for AM/PM formatting
                        DateTime timeOnly = DateTime.Today.Add(followUp.FollowTime);
                        timeString = timeOnly.ToString("hh:mm tt");
                    }
                    followUpTable.AddCell(CreateCenteredTableCell(timeString));

                    // CALL STATUS - Left aligned (as requested only specific columns to be centered)
                    followUpTable.AddCell(CreateCenteredTableCell(followUp.CallStatus ?? ""));

                    // RESPONSE - Left aligned (as requested only specific columns to be centered)
                    followUpTable.AddCell(CreateTableCell(followUp.Response ?? ""));

                    // NXT F/U DATE - Centered
                    followUpTable.AddCell(CreateCenteredTableCell(followUp.NextFollowDate?.ToString("dd-MM-yyyy") ?? ""));

                    rowNumber++;
                }

                // Add empty rows to fill up to 10
                for (int i = rowNumber; i <= 10; i++)
                {
                    followUpTable.AddCell(CreateCenteredTableCell(i.ToString("00")));
                    followUpTable.AddCell(CreateCenteredTableCell(""));
                    followUpTable.AddCell(CreateCenteredTableCell(""));
                    followUpTable.AddCell(CreateTableCell(""));
                    followUpTable.AddCell(CreateTableCell(""));
                    followUpTable.AddCell(CreateCenteredTableCell(""));
                }
            }
            else
            {
                // Add 10 empty rows
                for (int i = 1; i <= 10; i++)
                {
                    followUpTable.AddCell(CreateCenteredTableCell(i.ToString("00")));
                    followUpTable.AddCell(CreateCenteredTableCell(""));
                    followUpTable.AddCell(CreateCenteredTableCell(""));
                    followUpTable.AddCell(CreateTableCell(""));
                    followUpTable.AddCell(CreateTableCell(""));
                    followUpTable.AddCell(CreateCenteredTableCell(""));
                }
            }

            document.Add(followUpTable);
        }

        // GET: EnquiryForm/GenerateReceipt - Now works with actual database data
        public async Task<ActionResult> GenerateReceipt(Guid enquiryId, bool inline = true)
        {
            try
            {
                var sessionId = CurrentSessionID;
                var tenantCode = TenantCode;
                var currentUserId = CurrentTenantUserID;
                // Get enquiry from database
                var enquiry = await _enquiryRepository.GetEnquiryByIdAsync(enquiryId, sessionId, tenantCode);
                if (enquiry == null)
                {
                    return Content("Error: Enquiry not found");
                }

                // Get the latest receipt info
                var receiptInfo = await _enquiryRepository.GetReceiptInfoAsync(enquiryId, sessionId, tenantCode);

                // Create PDF
                MemoryStream stream = new MemoryStream();
                PdfWriter writer = null;
                PdfDocument pdf = null;
                Document document = null;

                try
                {
                    writer = new PdfWriter(stream);
                    writer.SetCloseStream(false);

                    pdf = new PdfDocument(writer);
                    pdf.SetDefaultPageSize(PageSize.A4);

                    document = new Document(pdf);
                    document.SetMargins(10, 10, 15, 10);

                    // Initialize fonts
                    FONT_NORMAL = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    FONT_BOLD = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                    // Get values from session
                    string code = CommonLogic.GetSessionValue(StringConstants.TenantCode);
                    string img = CommonLogic.GetSessionValue(StringConstants.LogoImg);
                    string logoPath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetLogoImage(code, img));
                    string sessionprint = CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint);

                    // FIRST COPY - ORIGINAL
                    AddReceiptCopy(document, logoPath, "SCHOOL COPY", enquiry, receiptInfo);

                    // Add empty rows for spacing
                 /*   Table emptyRowsTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginTop(5)
                        .SetMarginBottom(3);

                    for (int i = 0; i < 3; i++)
                    {
                        Cell emptyRowCell = new Cell()
                            .SetBorder(Border.NO_BORDER)
                            .SetMinHeight(15)
                            .SetPadding(0);

                        emptyRowCell.Add(new Paragraph("").SetMargin(0));
                        emptyRowsTable.AddCell(emptyRowCell);
                    }

                    document.Add(emptyRowsTable);
                 */
                    // CUTTING LINE
                    Table cuttingTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetMarginTop(8)
                        .SetMarginBottom(8);

                    Cell cuttingCell = new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetPadding(5);

                    Paragraph cuttingLine = new Paragraph()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFont(FONT_BOLD)
                        .SetFontSize(9)
                        .SetMargin(0);

                    cuttingLine.Add(new Text("[ CUT HERE ] "));
                    cuttingLine.Add(new Text("- - - - - - - - - - - - - - - - - - - - - - - -- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -").SetFont(FONT_NORMAL));

                    cuttingCell.Add(cuttingLine);
                    cuttingTable.AddCell(cuttingCell);
                    document.Add(cuttingTable);

                    // SECOND COPY - DUPLICATE
                    AddReceiptCopy(document, logoPath, "STUDENT COPY", enquiry, receiptInfo);

                    // Close document
                    document.Close();
                    document = null;
                    pdf.Close();
                    pdf = null;
                    writer.Close();
                    writer = null;

                    // Reset stream position
                    stream.Position = 0;

                    // Set file name
                    string fileName = $"AdmissionReceipt_{receiptInfo?.ReceiptNo ?? enquiry.Id.ToString()}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                    // Set content disposition
                    if (inline)
                    {
                        Response.AddHeader("Content-Disposition", "inline; filename=" + fileName);
                    }
                    else
                    {
                        Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
                    }

                    return File(stream, "application/pdf");
                }
                catch (Exception ex)
                {
                    // Clean up resources
                    if (document != null) document.Close();
                    if (pdf != null) pdf.Close();
                    if (writer != null) writer.Close();
                    stream.Dispose();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return Content("Error generating receipt PDF: " + ex.Message);
            }
        }
        // Add this method to create receipt rules section
        private static void AddReceiptRules(Document document)
        {
            // Add some spacing before rules
            document.Add(new Paragraph("").SetMarginTop(5));

            // Create rules table
            Table rulesTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(3);

            // Rules header
            Cell rulesHeaderCell = new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.8f))
                .SetPadding(3)
                .SetTextAlignment(TextAlignment.LEFT);

            rulesHeaderCell.Add(new Paragraph("RECEIPT RULES")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetMargin(0));

            rulesTable.AddCell(rulesHeaderCell);

            // Rules content cell
            Cell rulesContentCell = new Cell()
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(4);

            // Rule 1
            Paragraph rule1 = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(7)
                .SetMarginBottom(2)
                .SetTextAlignment(TextAlignment.JUSTIFIED);
            rule1.Add(new Text("1. ").SetFont(FONT_BOLD));
            rule1.Add(new Text("Once the fee is paid, it will not be refunded under any circumstances."));

            // Rule 2
            Paragraph rule2 = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(7)
                .SetMarginBottom(2)
                .SetTextAlignment(TextAlignment.JUSTIFIED);
            rule2.Add(new Text("2. ").SetFont(FONT_BOLD));
            rule2.Add(new Text("Fee must be paid only at the authorized counter and receipt should be collected immediately."));

            // Rule 3
            Paragraph rule3 = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(7)
                .SetMarginBottom(0)
                .SetTextAlignment(TextAlignment.JUSTIFIED);
            rule3.Add(new Text("3. ").SetFont(FONT_BOLD));
            rule3.Add(new Text("Keep the receipt safe as it will be valid proof in case of any dispute in the future."));

            // Add all rules to content cell
            rulesContentCell.Add(rule1);
            rulesContentCell.Add(rule2);
            rulesContentCell.Add(rule3);

            rulesTable.AddCell(rulesContentCell);
            document.Add(rulesTable);
        }
        private static void AddBlankFormAddressSection(Document document)
        {
            // Full address row
            Table fullAddressTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            fullAddressTable.AddCell(CreateStyledFieldCell("FULL ADDRESS", "", 20));
            document.Add(fullAddressTable);

            // City/Village name row
            Table cityTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            cityTable.AddCell(CreateStyledFieldCell("CITY/VILLAGE NAME", ""));
            document.Add(cityTable);

            // District, State, and Pin Code row
            Table districtStateTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.34f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            districtStateTable.AddCell(CreateStyledFieldCell("DISTRICT", ""));
            districtStateTable.AddCell(CreateStyledFieldCell("STATE", ""));
            districtStateTable.AddCell(CreateStyledFieldCell("PIN CODE", ""));
            document.Add(districtStateTable);
        }
        private static void AddContactPreferenceSection(Document document)
        {
            // Create a table with two columns for the header
            Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(2)
                .SetMarginBottom(0);

            // Preferred Contact Method header
            Cell preferredMethodHeader = new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.8f))
                .SetPadding(3)
                .SetTextAlignment(TextAlignment.CENTER);
            preferredMethodHeader.Add(new Paragraph("PREFERRED CONTACT METHOD")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetMargin(0));

            // Best Time to Contact header
            Cell bestTimeHeader = new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.8f))
                .SetPadding(3)
                .SetTextAlignment(TextAlignment.CENTER);
            bestTimeHeader.Add(new Paragraph("BEST TIME TO CONTACT")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetMargin(0));

            headerTable.AddCell(preferredMethodHeader);
            headerTable.AddCell(bestTimeHeader);
            document.Add(headerTable);

            // Contact details table
            Table contactTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            // Email row
            contactTable.AddCell(CreateStyledFieldCell("EMAIL", ""));
            contactTable.AddCell(CreateStyledFieldCell("MORNING (9-12)", ""));

            // Phone row
            contactTable.AddCell(CreateStyledFieldCell("PHONE", ""));
            contactTable.AddCell(CreateStyledFieldCell("AFTERNOON (12-5)", ""));

            // Text/WhatsApp row
            contactTable.AddCell(CreateStyledFieldCell("TEXT MESSAGE / WHATSAPP", ""));
            contactTable.AddCell(CreateStyledFieldCell("EVENING (5-9)", ""));

            document.Add(contactTable);
        }
        // FIXED: Now uses the same professional header as admission form
        private static void AddReceiptCopy(Document document, string logoPath, string copyType,
            StudentEnquiry enquiry, ReceiptInfo receiptInfo)
        {
            // Copy type label
            Paragraph copyLabel = new Paragraph(copyType)
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginBottom(3);
            document.Add(copyLabel);

            // ✅ NOW USING THE SAME PROFESSIONAL HEADER AS ADMISSION FORM
            AddDynamicSchoolHeader(document, logoPath);

            // Receipt title
            Table titleTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(3)
                .SetMarginBottom(5);

            Cell titleCell = new Cell()
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetBorder(new SolidBorder(BORDER_COLOR, 1f))
                .SetPadding(4)
                .SetTextAlignment(TextAlignment.CENTER);

            titleCell.Add(new Paragraph("ADMISSION FORM FEE RECEIPT")
                .SetFont(FONT_BOLD)
                .SetFontSize(9)
                .SetMargin(0));

            titleTable.AddCell(titleCell);
            document.Add(titleTable);

            // Receipt details table
            Table receiptTable = new Table(UnitValue.CreatePercentArray(new float[] { 25, 40, 20, 15 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(5);

            // Add all receipt fields with data from database
            receiptTable.AddCell(CreateCompactReceiptLabelCell("STUDENT'S NAME"));
            receiptTable.AddCell(CreateCompactReceiptValueCell(enquiry.Student));
            receiptTable.AddCell(CreateCompactReceiptLabelCell("RECEIPT NO."));
            receiptTable.AddCell(CreateCompactReceiptValueCell(receiptInfo?.ReceiptNo ?? "N/A"));

            receiptTable.AddCell(CreateCompactReceiptLabelCell("FATHER'S NAME"));
            receiptTable.AddCell(CreateCompactReceiptValueCell(enquiry.Father));
            receiptTable.AddCell(CreateCompactReceiptLabelCell("RECEIVE DATE"));
            receiptTable.AddCell(CreateCompactReceiptValueCell(enquiry.RcptDate?.ToString("dd-MM-yyyy") ?? DateTime.Now.ToString("dd-MM-yyyy")));

            receiptTable.AddCell(CreateCompactReceiptLabelCell("MOTHER'S NAME"));
            receiptTable.AddCell(CreateCompactReceiptValueCell(enquiry.Mother ?? ""));
            receiptTable.AddCell(CreateCompactReceiptLabelCell("ENQUIRY NO"));
            receiptTable.AddCell(CreateCompactReceiptValueCell(enquiry.EnqNo.ToString()));

            receiptTable.AddCell(CreateCompactReceiptLabelCell("CLASS APPLIED FOR"));
            receiptTable.AddCell(CreateCompactReceiptValueCell(enquiry.ApplyingForClass));
            receiptTable.AddCell(CreateCompactReceiptLabelCell("ENQUIRY DATE"));
            receiptTable.AddCell(CreateCompactReceiptValueCell(enquiry.EnquiryDate.ToString("dd-MM-yyyy") ?? ""));

            receiptTable.AddCell(CreateCompactReceiptLabelCell("CONTACT NO."));
            receiptTable.AddCell(CreateCompactReceiptValueCell(enquiry.Mobile1));
            receiptTable.AddCell(CreateCompactReceiptLabelCell("AMOUNT RECEIVED:"));
            receiptTable.AddCell(CreateCompactReceiptValueCell((enquiry.FormAmt ?? 0).ToString("0")));

            // Address row
            receiptTable.AddCell(CreateCompactReceiptLabelCell("ADDRESS"));
            Cell addressCell = new Cell(1, 3)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(2)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph(enquiry.Address ?? "")
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(7)
                    .SetMargin(0));
            receiptTable.AddCell(addressCell);

            document.Add(receiptTable);

            // Amount and signature section
            Table amountSignatureTable = new Table(UnitValue.CreatePercentArray(new float[] { 60, 40 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(8)
                .SetMarginBottom(5);

            // Amount section
            Cell leftAmountCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetVerticalAlignment(VerticalAlignment.TOP);

            decimal amountReceived = enquiry.FormAmt ?? 0;

            Paragraph amountBoxPara = new Paragraph()
                .SetFont(FONT_BOLD)
                .SetFontSize(14)
                .SetMargin(0)
                .SetMarginBottom(3)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBorder(new SolidBorder(BORDER_COLOR, 1.5f))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))  // Gray background added
                .SetPadding(8)
                .SetWidth(UnitValue.CreatePointValue(100))
                .SetHorizontalAlignment(HorizontalAlignment.LEFT);

            amountBoxPara.Add(new Text("Rs. ").SetFont(FONT_BOLD));
            amountBoxPara.Add(new Text($"{amountReceived} /-").SetFont(FONT_BOLD));

            string amountInWords = ConvertAmountToWords(amountReceived);
            Paragraph wordsParag = new Paragraph(amountInWords)
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetMargin(0)
                .SetMarginTop(2)
                .SetTextAlignment(TextAlignment.LEFT);

            leftAmountCell.Add(amountBoxPara);
            leftAmountCell.Add(wordsParag);

            // Signature section
            Cell rightSignatureCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.BOTTOM)
                .SetPaddingTop(15);

            Paragraph signatureLine = new Paragraph("---------------------------------------")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetMarginBottom(2)
                .SetMarginTop(20)
                .SetTextAlignment(TextAlignment.CENTER);

            Paragraph signatureText = new Paragraph("Sign with Stamp")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetMargin(0)
                .SetTextAlignment(TextAlignment.CENTER);

            rightSignatureCell.Add(signatureLine);
            rightSignatureCell.Add(signatureText);

            amountSignatureTable.AddCell(leftAmountCell);
            amountSignatureTable.AddCell(rightSignatureCell);
            document.Add(amountSignatureTable);

            // ADD THIS LINE - Call the receipt rules method
            AddReceiptRules(document);
        }

        // Helper methods remain the same
        private static Cell CreateHeaderCell(string text)
        {
            return new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(2)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph(text)
                    .SetFont(FONT_BOLD)
                    .SetFontSize(7)
                    .SetMargin(0));
        }

        private static Cell CreateTableCell(string text)
        {
            return new Cell()
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(2)
                .SetMinHeight(15)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph(text)
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(7)
                    .SetMargin(0));
        }

        private static void AddSectionHeader(Document document, string sectionTitle)
        {
            document.Add(new Paragraph("").SetMarginTop(2));

            Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0.5f);

            Cell headerCell = new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.8f))
                .SetPadding(3)
                .SetTextAlignment(TextAlignment.CENTER);

            headerCell.Add(new Paragraph(sectionTitle)
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetMargin(0));

            headerTable.AddCell(headerCell);
            document.Add(headerTable);
        }

        private static Cell CreateStyledFieldCell(string label, string value, float minHeight = 15)
        {
            Cell cell = new Cell()
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(2)
                .SetMinHeight(minHeight)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph fieldPara = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(7)
                .SetMargin(0)
                .SetPadding(0);

            fieldPara.Add(new Text(label + ": ").SetFont(FONT_BOLD).SetFontSize(7));

            if (string.IsNullOrEmpty(value))
            {
                // Add underline for empty fields
                //string underline = new string('_', 30);
                //fieldPara.Add(new Text(underline).SetFont(FONT_NORMAL).SetFontSize(7));
            }
            else
            {
                fieldPara.Add(new Text(value).SetFont(FONT_NORMAL).SetFontSize(7));
            }

            cell.Add(fieldPara);
            return cell;
        }

        private static Cell CreateCompactReceiptLabelCell(string text)
        {
            return new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(2)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph(text + " :")
                    .SetFont(FONT_BOLD)
                    .SetFontSize(7)
                    .SetMargin(0));
        }
        private static Cell CreateCenteredTableCell(string text)
        {
            return new Cell()
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(2)
                .SetMinHeight(15)
                .SetTextAlignment(TextAlignment.CENTER)  // Center alignment added
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph(text)
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(7)
                    .SetMargin(0));
        }
        private static Cell CreateCompactReceiptValueCell(string text)
        {
            return new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(2)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .Add(new Paragraph(text)
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(7)
                    .SetMargin(0));
        }

        private static void AddDeclarationSection(Document document)
        {
            document.Add(new Paragraph("").SetMarginTop(3));

            AddSectionHeader(document, "DECLARATION");

            Table declarationTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetMarginBottom(2);

            Cell declarationCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(4);

            string declarationText1 = "I hereby declare that the information provided in this form is true and correct to the best of my knowledge. If any information is found to be false or incorrect, the institution has the right to reject my application.";

            declarationCell.Add(new Paragraph(declarationText1)
                .SetFont(FONT_NORMAL)
                .SetFontSize(7)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetMarginBottom(3));

            declarationCell.Add(new Paragraph("Date: _________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(7)
                .SetMarginBottom(8));

            Table signatureTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.34f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(Border.NO_BORDER);

            // Student signature
            Cell studentSigCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(3);
            studentSigCell.Add(new Paragraph("____________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(7));
            studentSigCell.Add(new Paragraph("Student Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(6)
                .SetMarginTop(0.5f));

            // Parent signature
            Cell parentSigCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(3);
            parentSigCell.Add(new Paragraph("____________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(7));
            parentSigCell.Add(new Paragraph("Parent Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(6)
                .SetMarginTop(0.5f));

            // Principal signature
            Cell principalSigCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(3);
            principalSigCell.Add(new Paragraph("____________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(7));
            principalSigCell.Add(new Paragraph("Principal Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(6)
                .SetMarginTop(0.5f));

            signatureTable.AddCell(studentSigCell);
            signatureTable.AddCell(parentSigCell);
            signatureTable.AddCell(principalSigCell);

            declarationCell.Add(signatureTable);
            declarationTable.AddCell(declarationCell);
            document.Add(declarationTable);
        }

        private static string ConvertAmountToWords(decimal amount)
        {
            if (amount == 0) return "ZERO RUPEES ONLY";

            string[] ones = { "", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN",
                             "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN" };
            string[] tens = { "", "", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY" };

            long num = (long)amount;
            if (num == 0) return "ZERO RUPEES ONLY";

            string words = "";

            // Handle crores
            if (num >= 10000000)
            {
                words += ConvertToWords(num / 10000000, ones, tens) + " CRORE ";
                num %= 10000000;
            }

            // Handle lakhs
            if (num >= 100000)
            {
                words += ConvertToWords(num / 100000, ones, tens) + " LAKH ";
                num %= 100000;
            }

            // Handle thousands
            if (num >= 1000)
            {
                words += ConvertToWords(num / 1000, ones, tens) + " THOUSAND ";
                num %= 1000;
            }

            // Handle hundreds
            if (num >= 100)
            {
                words += ones[num / 100] + " HUNDRED ";
                num %= 100;
            }

            // Handle tens and ones
            if (num > 0)
            {
                if (num < 20)
                    words += ones[num];
                else
                {
                    words += tens[num / 10];
                    if (num % 10 > 0)
                        words += " " + ones[num % 10];
                }
            }

            return words.Trim() + " RUPEES ONLY";
        }

        private static string ConvertToWords(long num, string[] ones, string[] tens)
        {
            if (num == 0) return "";
            else if (num < 20) return ones[num];
            else if (num < 100) return tens[num / 10] + (num % 10 > 0 ? " " + ones[num % 10] : "");
            else return ones[num / 100] + " HUNDRED" + (num % 100 > 0 ? " " + ConvertToWords(num % 100, ones, tens) : "");
        }
    }

    // Footer event handler remains the same
    public class FooterEventHandler : IEventHandler
    {
        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
            PdfDocument pdf = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();

            Rectangle pageSize = page.GetPageSize();
            PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);

            float margin = 10f;

            // Add page border
            canvas.SaveState()
                .SetStrokeColor(ColorConstants.BLACK)
                .SetLineWidth(1f)
                .Rectangle(
                    pageSize.GetLeft() + margin,
                    pageSize.GetBottom() + margin,
                    pageSize.GetWidth() - (2 * margin),
                    pageSize.GetHeight() - (2 * margin)
                )
                .Stroke()
                .RestoreState();

            // Footer
            float footerY = pageSize.GetBottom() + margin + 3f;
            float leftX = pageSize.GetLeft() + margin + 3f;
            float rightX = pageSize.GetRight() - margin - 3f;

            string formattedDateTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");

            try
            {
                PdfFont footerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Left-aligned: Printed on date/time
                canvas.BeginText()
                    .SetFontAndSize(footerFont, 6)
                    .MoveText(leftX, footerY)
                    .ShowText("Printed on: " + formattedDateTime)
                    .EndText();

                // Right-aligned: Page numbers
                string pageText = "Page " + pdf.GetPageNumber(page) + " of " + pdf.GetNumberOfPages();
                float pageTextWidth = footerFont.GetWidth(pageText, 6);

                canvas.BeginText()
                    .SetFontAndSize(footerFont, 6)
                    .MoveText(rightX - pageTextWidth, footerY)
                    .ShowText(pageText)
                    .EndText();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating footer: {ex.Message}");
            }

            canvas.Release();
        }
    }
}