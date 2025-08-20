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
    public class GatePassPrnController : BaseController
    {
        private static PdfFont FONT_NORMAL;
        private static PdfFont FONT_BOLD;
        private static readonly Color BORDER_COLOR = ColorConstants.BLACK;

        // GET: GatePass/Generate
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
                    // Set page size to A4
                    pdf.SetDefaultPageSize(PageSize.A4);

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

                    // Generate Pass Numbers for both passes
                    string passNumber1 = GeneratePassNumber();
                    System.Threading.Thread.Sleep(10); // Small delay to ensure different timestamps
                    string passNumber2 = GeneratePassNumber();

                    // Add first gate pass (top half)
                    AddGatePassWithHeader(document, logoPath, passNumber1, sessionprint);

                    // Add separator line between passes with scissors symbols
                    document.Add(new Paragraph("").SetMarginTop(5));

                    // Create a table for the cutting line
                    Table cutLineTable = new Table(UnitValue.CreatePercentArray(new float[] { 10, 80, 10 }))
                        .SetWidth(UnitValue.CreatePercentValue(100))
                        .SetBorder(Border.NO_BORDER)
                        .SetMarginBottom(5);

                    // Left scissors symbol
                    Cell leftScissors = new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE);
                    leftScissors.Add(new Paragraph("✂")
                        .SetFont(FONT_NORMAL)
                        .SetFontSize(12));

                    // Dashed line
                    Cell dashedLineCell = new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE);
                    dashedLineCell.Add(new LineSeparator(new DashedLine(1.0f))
                        .SetMarginTop(5)
                        .SetMarginBottom(5));

                    // Right text
                    Cell rightText = new Cell()
                        .SetBorder(Border.NO_BORDER)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE);
                    rightText.Add(new Paragraph("CUT HERE")
                        .SetFont(FONT_NORMAL)
                        .SetFontSize(8)
                        .SetFontColor(ColorConstants.GRAY));

                    cutLineTable.AddCell(leftScissors);
                    cutLineTable.AddCell(dashedLineCell);
                    cutLineTable.AddCell(rightText);

                    document.Add(cutLineTable);
                    document.Add(new Paragraph("").SetMarginBottom(5));

                    // Add second gate pass (bottom half)
                    AddGatePassWithHeader(document, logoPath, passNumber2, sessionprint);

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
                    string fileName = $"GatePasses_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

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

        // Generate unique pass number
        private string GeneratePassNumber()
        {
            // Format: GP-YYYY-MMDD-XXXX
            string datePrefix = DateTime.Now.ToString("yyyy-MMdd");
            Random random = new Random();
            int randomNumber = random.Next(1000, 9999);
            // Add milliseconds to ensure uniqueness
            string uniqueSuffix = DateTime.Now.Millisecond.ToString();
            return $"GP-{datePrefix}-{randomNumber}{uniqueSuffix}";
        }

        private static void AddGatePassWithHeader(Document document, string logoPath, string passNumber, string sessionprint)
        {
            // Create a container table with border for the entire gate pass
            Table containerTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 1f))
                .SetPadding(5);

            // Create a cell to hold all content
            Cell containerCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0);

            // Create a sub-document div to add content to the cell
            Div contentDiv = new Div();

            // Add school header to the div
            AddSchoolHeaderToDiv(contentDiv, logoPath);

            // Add gate pass form to the div
            AddGatePassFormToDiv(contentDiv, passNumber);

            // Add footer information
            AddGatePassFooter(contentDiv);

            // Add the div to the cell and cell to table
            containerCell.Add(contentDiv);
            containerTable.AddCell(containerCell);

            // Add the complete gate pass to the document
            document.Add(containerTable);
        }

        private static void AddSchoolHeaderToDiv(Div contentDiv, string logoPath = null)
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

            // Add the complete header to the div
            contentDiv.Add(headerTable);

            // Add some spacing after the header
            contentDiv.Add(new Paragraph("").SetMarginBottom(1));

            // Add the separator line
            contentDiv.Add(new LineSeparator(new SolidLine(1.0f)).SetMarginBottom(1));
        }

        private static void AddGatePassFormToDiv(Div contentDiv, string passNumber)
        {
            // Title with Pass Number
            Table titleTable = new Table(UnitValue.CreatePercentArray(new float[] { 30, 40, 30 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(3)
                .SetMarginBottom(5);

            // Pass No. (left)
            Cell passNoCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph passNoPara = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetMargin(0);

            passNoPara.Add(new Text("PASS NO.: ").SetFont(FONT_BOLD));
            passNoPara.Add(new Text(passNumber).SetFont(FONT_BOLD).SetUnderline());

            passNoCell.Add(passNoPara);

            // Title (center)
            Cell titleCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph formTitle = new Paragraph("GATE PASS")
                .SetFont(FONT_BOLD)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMargin(0);

            titleCell.Add(formTitle);

            // Date (right)
            Cell dateCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph datePara = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetMargin(0);

            datePara.Add(new Text("DATE: ").SetFont(FONT_BOLD));
            datePara.Add(new Text(DateTime.Now.ToString("dd-MM-yyyy")));

            dateCell.Add(datePara);

            titleTable.AddCell(passNoCell);
            titleTable.AddCell(titleCell);
            titleTable.AddCell(dateCell);
            contentDiv.Add(titleTable);

            // Main form table
            Table formTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f }))
            .SetWidth(UnitValue.CreatePercentValue(100))
            .SetBorder(new SolidBorder(BORDER_COLOR, 0.8f))
            .SetMarginBottom(5);

            // First Row - Name, Class, Section
            formTable.AddCell(CreateTitleValueCell("NAME", ""));
            formTable.AddCell(CreateTitleValueCell("CLASS", ""));
            formTable.AddCell(CreateTitleValueCell("SECTION", ""));

            // Second Row - Guardian Name, Mobile No, Time-In
            formTable.AddCell(CreateTitleValueCell("GUARDIAN NAME", ""));
            formTable.AddCell(CreateTitleValueCell("MOBILE NO.", ""));
            formTable.AddCell(CreateTitleValueCell("TIME-IN", ""));

            // Third Row - Time-Out, Reason for Leaving (alternative approach)
            formTable.AddCell(CreateTitleValueCell("TIME-OUT", ""));
            formTable.AddCell(CreateTitleValueCell("REASON FOR LEAVING", ""));
            formTable.AddCell(new Cell().SetBorder(Border.NO_BORDER)); // Empty cell

            contentDiv.Add(formTable);
            // Signature Section
            AddSignatureSectionToDiv(contentDiv);

            // Instructions
            AddInstructionsToDiv(contentDiv);
        }
        private static Cell CreateTitleValueCell(string title, string value)
        {
            Cell cell = new Cell()
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(1);
                

            // Create title paragraph
            Paragraph titlePara = new Paragraph()
                .Add(new Text(title + ":").SetBold().SetFontSize(7))
                .SetMarginBottom(1);

            // Create value paragraph
            Paragraph valuePara = new Paragraph()
                .Add(new Text(value).SetFontSize(9))
                .SetMarginTop(0);

            cell.Add(titlePara);
            cell.Add(valuePara);

            return cell;
        }
        private static void AddSignatureSectionToDiv(Div contentDiv)
        {
            Table signatureTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.34f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(8)
                .SetBorder(Border.NO_BORDER);

            // Guardian Signature
            Cell guardianSigCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(3);
            guardianSigCell.Add(new Paragraph("_____________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8));
            guardianSigCell.Add(new Paragraph("Guardian Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetMarginTop(1));

            // Security Guard
            Cell securityCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(3);
            securityCell.Add(new Paragraph("_____________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8));
            securityCell.Add(new Paragraph("Security Guard")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetMarginTop(1));

            // Authorized Signature
            Cell authorizedCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(3);
            authorizedCell.Add(new Paragraph("_____________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8));
            authorizedCell.Add(new Paragraph("Authorized Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetMarginTop(1));

            signatureTable.AddCell(guardianSigCell);
            signatureTable.AddCell(securityCell);
            signatureTable.AddCell(authorizedCell);

            contentDiv.Add(signatureTable);
        }

        private static void AddInstructionsToDiv(Div contentDiv)
        {
            // Instructions box
            Table instructionTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(5)
                .SetBorder(new SolidBorder(ColorConstants.GRAY, 0.5f));

            Cell instructionCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetBackgroundColor(new DeviceRgb(245, 245, 245))
                .SetPadding(5);

            Paragraph instructionTitle = new Paragraph("IMPORTANT INSTRUCTIONS")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(3);

            instructionCell.Add(instructionTitle);

            // Add instructions
            string[] instructions = new string[]
            {
                "1. All fields marked with (*) are mandatory.",
                "2. This pass is valid only for the date mentioned above.",
                "3. Guardian must show valid ID proof at the gate.",
                "4. Student must be accompanied by the guardian mentioned in this pass."
            };

            foreach (string instruction in instructions)
            {
                Paragraph instructionPara = new Paragraph(instruction)
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(6)
                    .SetMarginBottom(1);
                instructionCell.Add(instructionPara);
            }

            instructionTable.AddCell(instructionCell);
            contentDiv.Add(instructionTable);
        }

        private static void AddGatePassFooter(Div contentDiv)
        {
            // Add footer information at the bottom of the gate pass
            Table footerTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(5)
                .SetBorder(Border.NO_BORDER);

            Cell footerCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER);

            string formattedDateTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");
            Paragraph footerPara = new Paragraph($"Printed on: {formattedDateTime}")
                .SetFont(FONT_NORMAL)
                .SetFontSize(7)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER);

            footerCell.Add(footerPara);
            footerTable.AddCell(footerCell);
            contentDiv.Add(footerTable);
        }

        private static void AddGatePassForm(Document document, string passNumber)
        {
            // Title with Pass Number
            Table titleTable = new Table(UnitValue.CreatePercentArray(new float[] { 30, 40, 30 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(3)
                .SetMarginBottom(5);

            // Pass No. (left)
            Cell passNoCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph passNoPara = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetMargin(0);

            passNoPara.Add(new Text("PASS NO.: ").SetFont(FONT_BOLD));
            passNoPara.Add(new Text(passNumber).SetFont(FONT_BOLD).SetUnderline());

            passNoCell.Add(passNoPara);

            // Title (center)
            Cell titleCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph formTitle = new Paragraph("GATE PASS")
                .SetFont(FONT_BOLD)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMargin(0);

            titleCell.Add(formTitle);

            // Date (right)
            Cell dateCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph datePara = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetMargin(0);

            datePara.Add(new Text("DATE: ").SetFont(FONT_BOLD));
            datePara.Add(new Text(DateTime.Now.ToString("dd-MM-yyyy")));

            dateCell.Add(datePara);

            titleTable.AddCell(passNoCell);
            titleTable.AddCell(titleCell);
            titleTable.AddCell(dateCell);
            document.Add(titleTable);

            // Main form table
            Table formTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.8f))
                .SetMarginBottom(5);

            // Guardian Information
            Table guardianTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            guardianTable.AddCell(CreateFieldCell("GUARDIAN NAME", "", true, 20));
            guardianTable.AddCell(CreateFieldCell("MOBILE NO.", "", true, 20));

            Cell guardianCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(3);
            guardianCell.Add(guardianTable);
            formTable.AddCell(guardianCell);

            // Time Information
            Table timeTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginBottom(0);

            timeTable.AddCell(CreateFieldCell("TIME-IN", "", true, 20));
            timeTable.AddCell(CreateFieldCell("TIME-OUT", "", true, 20));

            Cell timeCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(3);
            timeCell.Add(timeTable);
            formTable.AddCell(timeCell);

            // Reason for leaving
            Cell reasonCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(3);
            reasonCell.Add(CreateFieldCell("REASON FOR LEAVING", "", true, 30));
            formTable.AddCell(reasonCell);

            document.Add(formTable);

            // Signature Section
            AddSignatureSection(document);

            // Instructions
            AddInstructions(document);
        }

        private static Cell CreateFieldCell(string label, string value, bool mandatory = false, float minHeight = 20)
        {
            Cell cell = new Cell()
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(3)
                .SetMinHeight(minHeight)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph fieldPara = new Paragraph()
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetMargin(0)
                .SetPadding(0);

            // Add label
            string labelText = mandatory ? label + " *: " : label + ": ";
            fieldPara.Add(new Text(labelText).SetFont(FONT_BOLD).SetFontSize(8));

            if (string.IsNullOrEmpty(value))
            {
                // Add blank line for writing
                fieldPara.Add(new Text("\n_______________________________").SetFont(FONT_NORMAL).SetFontSize(8));
            }
            else
            {
                fieldPara.Add(new Text(value).SetFont(FONT_NORMAL).SetFontSize(8));
            }

            cell.Add(fieldPara);
            return cell;
        }

        private static void AddSignatureSection(Document document)
        {
            Table signatureTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.34f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(8)
                .SetBorder(Border.NO_BORDER);

            // Guardian Signature
            Cell guardianSigCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(3);
            guardianSigCell.Add(new Paragraph("_____________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8));
            guardianSigCell.Add(new Paragraph("Guardian Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetMarginTop(1));

            // Security Guard
            Cell securityCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(3);
            securityCell.Add(new Paragraph("_____________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8));
            securityCell.Add(new Paragraph("Security Guard")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetMarginTop(1));

            // Authorized Signature
            Cell authorizedCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(3);
            authorizedCell.Add(new Paragraph("_____________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8));
            authorizedCell.Add(new Paragraph("Authorized Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetMarginTop(1));

            signatureTable.AddCell(guardianSigCell);
            signatureTable.AddCell(securityCell);
            signatureTable.AddCell(authorizedCell);

            document.Add(signatureTable);
        }

        private static void AddInstructions(Document document)
        {
            // Instructions box
            Table instructionTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(5)
                .SetBorder(new SolidBorder(ColorConstants.GRAY, 0.5f));

            Cell instructionCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetBackgroundColor(new DeviceRgb(245, 245, 245))
                .SetPadding(5);

            Paragraph instructionTitle = new Paragraph("IMPORTANT INSTRUCTIONS")
                .SetFont(FONT_BOLD)
                .SetFontSize(7)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(3);

            instructionCell.Add(instructionTitle);

            // Add instructions
            string[] instructions = new string[]
            {
                "1. All fields marked with (*) are mandatory.",
                "2. This pass is valid only for the date mentioned above.",
                "3. Guardian must show valid ID proof at the gate.",
                "4. Student must be accompanied by the guardian mentioned in this pass."
            };

            foreach (string instruction in instructions)
            {
                Paragraph instructionPara = new Paragraph(instruction)
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(6)
                    .SetMarginBottom(1);
                instructionCell.Add(instructionPara);
            }

            instructionTable.AddCell(instructionCell);
            document.Add(instructionTable);
        }
    }
}