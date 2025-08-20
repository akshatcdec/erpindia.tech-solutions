using ERPIndia.Class.Helper;
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
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class StudentAdmissionFormController : BaseController
    {
        // GET: StudentAdmissionForm
        private static PdfFont FONT_NORMAL;
        private static PdfFont FONT_BOLD;
        private static readonly Color BORDER_COLOR = ColorConstants.BLACK;
       
        // GET: Letterhead/Generate
        public ActionResult Generate(bool inline = true)
        {
            try
            {
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
                    document.SetMargins(12, 12, 25, 12);

                    // Initialize fonts
                    FONT_NORMAL = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    FONT_BOLD = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                    // Get values from session or use defaults
                    string code = CommonLogic.GetSessionValue(StringConstants.TenantCode);
                    string img = CommonLogic.GetSessionValue(StringConstants.LogoImg);
                    string logoPath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetLogoImage(code, img));
                    string sessionprint = CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint);

                    // Add header to PDF
                    AddSchoolHeader(document, logoPath);
                    AddStudentAdmissionForm(document);

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
                    string fileName = $"StudentAdmissionForm_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

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

        private static void AddSchoolHeader(Document document, string logoPath = null)
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
                .SetPadding(5)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            try
            {
                if (!string.IsNullOrEmpty(logoPath) && System.IO.File.Exists(logoPath))
                {
                    ImageData imageData = ImageDataFactory.Create(logoPath);
                    Image logoImage = new Image(imageData);

                    logoImage.SetAutoScale(true);
                    logoImage.SetMaxWidth(70);
                    logoImage.SetMaxHeight(70);
                    logoImage.SetHorizontalAlignment(HorizontalAlignment.LEFT);
                    logoImage.SetMarginLeft(5);

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
                .SetPadding(5);

            // School name
            string schoolNameText = CommonLogic.GetSessionValue(StringConstants.PrintTitle) ?? "ABCD PUBLIC SCHOOL";
            Paragraph schoolName = new Paragraph(schoolNameText)
                .SetFont(FONT_BOLD)
                .SetFontSize(13)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(3);

            // School address
            string addressText = CommonLogic.GetSessionValue(StringConstants.Line1) ?? "123 Education Street, City Name, State - 123456";
            Paragraph schoolAddress = new Paragraph(addressText)
                .SetFont(FONT_NORMAL)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(2);

            // Contact information
            string contactText = CommonLogic.GetSessionValue(StringConstants.Line2) ?? "Email: info@school.edu | Phone: 123-456-7890";
            Paragraph schoolContact = new Paragraph(contactText)
                .SetFont(FONT_NORMAL)
                .SetFontSize(9)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(2);

            // Add all text elements to the text cell in order
            textCell.Add(schoolName);
            textCell.Add(schoolAddress);
            textCell.Add(schoolContact);

            // Session information from Line3
            string sessionText = CommonLogic.GetSessionValue(StringConstants.Line3);
            if (!string.IsNullOrEmpty(sessionText))
            {
                Paragraph schoolSession = new Paragraph(sessionText)
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(2);
                textCell.Add(schoolSession);
            }

            // Add Line4 if exists
            string line4Text = CommonLogic.GetSessionValue(StringConstants.Line4);
            if (!string.IsNullOrEmpty(line4Text))
            {
                Paragraph line4 = new Paragraph(line4Text)
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(2);
                textCell.Add(line4);
            }

            // Session print
            string sessionprint = CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint);
            if (!string.IsNullOrEmpty(sessionprint))
            {
                Paragraph session = new Paragraph("Session: ( " + sessionprint + " )")
                    .SetFont(FONT_BOLD)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(2)
                    .SetMarginBottom(0);
                textCell.Add(session);
            }

            // Right cell for balance (empty)
            Cell rightCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5);
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

            // Add some spacing after the header
            document.Add(new Paragraph("").SetMarginBottom(1));

            // Add the separator line
            document.Add(new LineSeparator(new SolidLine(1.0f)).SetMarginBottom(1));
        }

        // Improved AddStudentAdmissionForm method - COMPACT VERSION for one page
        private static void AddStudentAdmissionForm(Document document)
        {
            // Create a table for the title row with admission info
            Table titleTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.34f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(3)
                .SetMarginBottom(6);

            // Admission No. (left side)
            Cell admissionNoCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph admissionNoPara = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(9)
                .SetMargin(0);

            admissionNoPara.Add(new Text("ADMISSION NO.: ").SetFont(FONT_BOLD));
            admissionNoPara.Add(new Text("__________________"));

            admissionNoCell.Add(admissionNoPara);

            // Title (center)
            Cell titleCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph formTitle = new Paragraph("ADMISSION FORM")
                .SetFont(FONT_BOLD)
                .SetFontSize(11)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMargin(0);

            titleCell.Add(formTitle);

            // Admission Date (right side)
            Cell admissionDateCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph admissionDatePara = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(9)
                .SetMargin(0);

            admissionDatePara.Add(new Text("ADMISSION DATE: ").SetFont(FONT_BOLD));
            admissionDatePara.Add(new Text("__________________"));

            admissionDateCell.Add(admissionDatePara);

            titleTable.AddCell(admissionNoCell);
            titleTable.AddCell(titleCell);
            titleTable.AddCell(admissionDateCell);
            document.Add(titleTable);

            // STUDENT INFORMATION SECTION
            AddSectionHeader(document, "STUDENT INFORMATION");

            // Use a simpler 2-column approach that works reliably
            Table studentMainTable = new Table(UnitValue.CreatePercentArray(new float[] { 75, 25 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            // Left side - create a sub-table for all fields
            Cell fieldsContainer = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetMargin(0);

            // Row 1: Student Name (full width)
            Table row1 = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            row1.AddCell(CreateStyledFieldCell("STUDENT NAME", ""));
            fieldsContainer.Add(row1);

            // Row 2: Class and Section
            Table row2 = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            row2.AddCell(CreateStyledFieldCell("CLASS", ""));
            row2.AddCell(CreateStyledFieldCell("SECTION", ""));
            fieldsContainer.Add(row2);

            // Row 3: Aadhar, Mobile, Roll
            Table row3 = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.34f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            row3.AddCell(CreateStyledFieldCell("AADHAR NO.", ""));
            row3.AddCell(CreateStyledFieldCell("MOBILE NO.", ""));
            row3.AddCell(CreateStyledFieldCell("ROLL NO.", ""));
            fieldsContainer.Add(row3);

            // Row 4: DOB, Gender, Caste
            Table row4 = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.34f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            row4.AddCell(CreateStyledFieldCell("DATE OF BIRTH", ""));
            row4.AddCell(CreateStyledFieldCell("GENDER", ""));
            row4.AddCell(CreateStyledFieldCell("CASTE", ""));
            fieldsContainer.Add(row4);

            // Row 5: Category, Religion, Nationality
            Table row5 = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.34f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);
            row5.AddCell(CreateStyledFieldCell("CATEGORY", ""));
            row5.AddCell(CreateStyledFieldCell("RELIGION", ""));
            row5.AddCell(CreateStyledFieldCell("NATIONALITY", ""));
            fieldsContainer.Add(row5);

            // Right side - photo cell
            Cell photoCell = new Cell()
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetHeight(110) // Set height to match all 5 rows
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetBackgroundColor(new DeviceRgb(250, 250, 250));

            photoCell.Add(new Paragraph("PASTE\nPHOTO\nHERE")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMargin(0));

            // Add both sides to main table
            studentMainTable.AddCell(fieldsContainer);
            studentMainTable.AddCell(photoCell);
            document.Add(studentMainTable);

            // FAMILY INFORMATION SECTION
            AddSectionHeader(document, "FAMILY INFORMATION");

            // Father info row
            Table fatherTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            fatherTable.AddCell(CreateStyledFieldCell("FATHER NAME", ""));
            fatherTable.AddCell(CreateStyledFieldCell("FATHER OCCUPATION", ""));
            document.Add(fatherTable);

            // Mother info row
            Table motherTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            motherTable.AddCell(CreateStyledFieldCell("MOTHER NAME", ""));
            motherTable.AddCell(CreateStyledFieldCell("MOTHER OCCUPATION", ""));
            document.Add(motherTable);

            // ADDRESS INFORMATION SECTION
            AddSectionHeader(document, "ADDRESS INFORMATION");

            // Permanent address row
            Table permAddressTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            permAddressTable.AddCell(CreateStyledFieldCell("FULL ADDRESS", "", 22));
            document.Add(permAddressTable);

            // Current address row
            Table currAddressTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            currAddressTable.AddCell(CreateStyledFieldCell("CITY/VILLAGE NAME", "", 22));
            document.Add(currAddressTable);

            // District, State, PIN row
            Table districtTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.34f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            districtTable.AddCell(CreateStyledFieldCell("DISTRICT", ""));
            districtTable.AddCell(CreateStyledFieldCell("STATE", ""));
            districtTable.AddCell(CreateStyledFieldCell("PIN CODE", ""));
            document.Add(districtTable);

            // SCHOOL & FEE INFORMATION SECTION
            AddSectionHeader(document, "SCHOOL & FEE INFORMATION");

            // Last school and passed class row
            Table schoolTable = new Table(UnitValue.CreatePercentArray(new float[] { 60, 40 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            schoolTable.AddCell(CreateStyledFieldCell("LAST SCHOOL", ""));
            schoolTable.AddCell(CreateStyledFieldCell("PASSED CLASS", ""));
            document.Add(schoolTable);

            // Fee and transport row
            Table feeTransportTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            feeTransportTable.AddCell(CreateStyledFieldCell("FEE DETAILS", ""));
            feeTransportTable.AddCell(CreateStyledFieldCell("TRANSPORT", ""));
            document.Add(feeTransportTable);

            // Payment mode and IFSC row
            Table paymentTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            paymentTable.AddCell(CreateStyledFieldCell("MODE OF PAYMENT", ""));
            paymentTable.AddCell(CreateStyledFieldCell("PEN NO.", ""));
            document.Add(paymentTable);

            // Declaration section
            AddDeclarationSection(document);
        }

        // Helper method to create compact section headers
        private static void AddSectionHeader(Document document, string sectionTitle)
        {
            document.Add(new Paragraph("").SetMarginTop(4));

            Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(1);

            Cell headerCell = new Cell()
                .SetBackgroundColor(new DeviceRgb(220, 220, 220))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.8f))
                .SetPadding(4)
                .SetTextAlignment(TextAlignment.CENTER);

            headerCell.Add(new Paragraph(sectionTitle)
                .SetFont(FONT_BOLD)
                .SetFontSize(9)
                .SetMargin(0));

            headerTable.AddCell(headerCell);
            document.Add(headerTable);
        }

        // Improved field cell creation with proper length underlines
        private static Cell CreateStyledFieldCell(string label, string value, float minHeight = 18)
        {
            Cell cell = new Cell()
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(3)
                .SetMinHeight(minHeight)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            // Create the field content with controlled spacing
            Paragraph fieldPara = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetMargin(0)
                .SetPadding(0);

            // Add label in bold
            fieldPara.Add(new Text(label + ": ").SetFont(FONT_BOLD).SetFontSize(8));

            if (string.IsNullOrEmpty(value))
            {
                string underline = new string('_', 0);
                fieldPara.Add(new Text(underline).SetFont(FONT_NORMAL).SetFontSize(8));
            }
            else
            {
                fieldPara.Add(new Text(value).SetFont(FONT_NORMAL).SetFontSize(8));
            }

            cell.Add(fieldPara);
            return cell;
        }

        // Compact declaration section for one page layout
        // Compact declaration section for one page layout with Principal signature
        // Compact declaration section for one page layout with Principal signature and checkboxes
        // Compact declaration section for one page layout with Principal signature and checkboxes
        private static void AddDeclarationSection(Document document)
        {
            document.Add(new Paragraph("").SetMarginTop(5));

            // Declaration header
            AddSectionHeader(document, "DECLARATION");

            // Declaration box
            Table declarationTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetMarginBottom(3);

            Cell declarationCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(6);

            // Create checkbox helper method
            Cell CreateCheckboxCell()
            {
                Cell checkboxCell = new Cell()
                    .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                    .SetWidth(12)
                    .SetHeight(12)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.TOP)
                    .SetPadding(1)
                    .SetMargin(0);

                string checkedImagePath = System.Web.HttpContext.Current.Server.MapPath("/Content/login/images/emptycheckbox.png");
                if (!string.IsNullOrEmpty(checkedImagePath) && System.IO.File.Exists(checkedImagePath))
                {
                    // Create image from file
                    ImageData imageData = ImageDataFactory.Create(checkedImagePath);
                    Image checkboxImage = new Image(imageData);
                    // Set image properties to fit within the bordered cell
                    checkboxImage.SetAutoScale(true);
                    checkboxImage.SetMaxWidth(10);  // Reduced to fit within border + padding
                    checkboxImage.SetMaxHeight(10); // Reduced to fit within border + padding
                    checkboxImage.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    checkboxCell.Add(checkboxImage);
                }
                else
                {
                    // Fallback to Unicode checkmark if image not found
                    Paragraph checkSymbol = new Paragraph("\u2714")
                        .SetMargin(0)
                        .SetPadding(0)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetFont(FONT_BOLD)
                        .SetFontSize(7)
                        .SetFontColor(new DeviceRgb(0, 128, 0));
                    checkboxCell.Add(checkSymbol);
                }

                return checkboxCell;
            }

            // Create table for checkbox and declaration text
            Table checkboxTable = new Table(UnitValue.CreatePercentArray(new float[] { 3f, 97f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(Border.NO_BORDER)
                .SetMarginBottom(8);

            // Add checkbox cell
            Cell checkboxContainer = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .SetPaddingTop(2); // Slight adjustment to align with text

            checkboxContainer.Add(CreateCheckboxCell());
            checkboxTable.AddCell(checkboxContainer);

            // Add declaration text cell
            Cell textCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetPaddingLeft(5)
                .SetVerticalAlignment(VerticalAlignment.TOP);

            // Declaration text - shorter
            string declarationText = "I hereby declare that all the information provided above is true and correct to the best of my knowledge. " +
                                   "I agree to abide by all rules and regulations of the school.";

            textCell.Add(new Paragraph(declarationText)
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetMargin(0));

            checkboxTable.AddCell(textCell);
            declarationCell.Add(checkboxTable);

            // Optional: Add another checkbox item if needed
            Table checkboxTable2 = new Table(UnitValue.CreatePercentArray(new float[] { 3f, 97f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(Border.NO_BORDER)
                .SetMarginBottom(8);

            // Add second checkbox cell
            Cell checkboxContainer2 = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .SetPaddingTop(2);

            checkboxContainer2.Add(CreateCheckboxCell());
            checkboxTable2.AddCell(checkboxContainer2);

            // Add second declaration text cell
            Cell textCell2 = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetPaddingLeft(5)
                .SetVerticalAlignment(VerticalAlignment.TOP);

            string declarationText2 = "I understand that any false information may result in cancellation of admission.";

            textCell2.Add(new Paragraph(declarationText2)
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.JUSTIFIED)
                .SetMargin(0));

            checkboxTable2.AddCell(textCell2);
            declarationCell.Add(checkboxTable2);

            // Compact signature section - now with 4 columns to include Principal
            Table signatureTable = new Table(UnitValue.CreatePercentArray(new float[] { 20f, 25f, 25f, 30f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(Border.NO_BORDER);

            // Date
            Cell dateCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPaddingTop(5);
            dateCell.Add(new Paragraph("Date: ___________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8));

            // Student signature
            Cell studentSigCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(5);
            studentSigCell.Add(new Paragraph("________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8));
            studentSigCell.Add(new Paragraph("Student Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetMarginTop(1));

            // Parent signature
            Cell parentSigCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(5);
            parentSigCell.Add(new Paragraph("________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8));
            parentSigCell.Add(new Paragraph("Parent Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetMarginTop(1));

            // Principal signature with stamp area
            Cell principalSigCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(5);

            // Create a small table for principal signature and stamp
            Table principalTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(Border.NO_BORDER);

            // Principal signature line
            Cell principalSignature = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingBottom(2);
            principalSignature.Add(new Paragraph("________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8));
            principalSignature.Add(new Paragraph("Principal Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetMarginTop(1));

            // Stamp area
            Cell stampCell = new Cell()
                .SetBorder(new DottedBorder(BORDER_COLOR, 0.5f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetHeight(25)
                .SetMarginTop(3);
            stampCell.Add(new Paragraph("School Stamp")
                .SetFont(FONT_NORMAL)
                .SetFontSize(6)
                .SetFontColor(ColorConstants.GRAY)
                .SetMarginTop(8));

            principalTable.AddCell(principalSignature);
            principalTable.AddCell(stampCell);
            principalSigCell.Add(principalTable);

            signatureTable.AddCell(dateCell);
            signatureTable.AddCell(studentSigCell);
            signatureTable.AddCell(parentSigCell);
            signatureTable.AddCell(principalSigCell);

            declarationCell.Add(signatureTable);
            declarationTable.AddCell(declarationCell);
            document.Add(declarationTable);
        }
    }

    // Simple Footer event handler for compact design
    public class FooterEventHandler : IEventHandler
    {
        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
            PdfDocument pdf = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();

            Rectangle pageSize = page.GetPageSize();
            PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);

            // Define consistent margins
            float margin = 12f;

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

            // Simple footer - just page number and date
            float footerY = pageSize.GetBottom() + margin + 5f;
            float leftX = pageSize.GetLeft() + margin + 3f;
            float rightX = pageSize.GetRight() - margin - 3f;

            // Format date and time
            string formattedDateTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");

            try
            {
                PdfFont footerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Left-aligned: Printed on date/time
                canvas.BeginText()
                    .SetFontAndSize(footerFont, 7)
                    .MoveText(leftX, footerY)
                    .ShowText("Printed on: " + formattedDateTime)
                    .EndText();

                // Right-aligned: Page numbers
                string pageText = "Page " + pdf.GetPageNumber(page) + " of " + pdf.GetNumberOfPages();
                float pageTextWidth = footerFont.GetWidth(pageText, 7);

                canvas.BeginText()
                    .SetFontAndSize(footerFont, 7)
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