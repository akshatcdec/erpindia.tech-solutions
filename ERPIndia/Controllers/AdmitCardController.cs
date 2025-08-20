    using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace ERPIndia.Controllers
{
    public class AdmitCardController : Controller
    {
        // GET: AdmitCard
        public ActionResult Index()
        {
            return View();
        }

        // Action method to generate the admit cards
        public ActionResult GenerateAdmitCards(List<StudentAdmitModel> students = null)
        {
            // If no student data is provided, use sample data
            if (students == null || students.Count == 0)
            {
                students = GetSampleStudentData();
            }

            // Set up the memory stream for PDF
            using (MemoryStream ms = new MemoryStream())
            {
                // Initialize PDF writer and document
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf, PageSize.A4);
                document.SetMargins(10, 10, 10, 10);

                // Create admit cards (2 per page)
                for (int i = 0; i < students.Count; i += 2)
                {
                    // First card
                    CreateAdmitCard(pdf, document, students[i], 0);

                    // Second card (if available)
                    if (i + 1 < students.Count)
                    {
                        CreateAdmitCard(pdf, document, students[i + 1], 1);
                    }

                    // Add a new page if needed (except for the last page)
                    if (i + 2 < students.Count)
                    {
                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    }
                }

                // Close the document
                document.Close();

                // Return the PDF
                return File(ms.ToArray(), "application/pdf");
            }
        }

        // Method to create a single admit card
        private void CreateAdmitCard(PdfDocument pdf, Document document, StudentAdmitModel student, int position)
        {
            // Calculate vertical position (0 = top, 1 = bottom)
            float yPos = position == 0 ? 580 : 250;
            float cardHeight = 320;

            // Create outer border
            Table outerTable = new Table(1).UseAllAvailableWidth();
            outerTable.SetFixedPosition(10, yPos, 575, cardHeight);
            outerTable.SetBorder(new SolidBorder(ColorConstants.BLACK, 2));

            // Create inner border with margin
            Cell outerCell = new Cell();
            outerCell.SetPadding(10);
            outerCell.SetBorder(Border.NO_BORDER);

            // Inner table
            Table innerTable = new Table(1).UseAllAvailableWidth();
            innerTable.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));

            Cell innerCell = new Cell();
            innerCell.SetPadding(5);
            innerCell.SetBorder(Border.NO_BORDER);

            // Header Table (3 columns: logo, school info, logo)
            Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 20, 60, 20 }));
            headerTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Left Logo
            Cell leftLogoCell = new Cell();
            leftLogoCell.SetBorder(Border.NO_BORDER);
            leftLogoCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);

            // Right Logo (same as left)
            Cell rightLogoCell = new Cell();
            rightLogoCell.SetBorder(Border.NO_BORDER);
            rightLogoCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);

            try
            {
                // Try to load the school logo
                string logoPath = Server.MapPath("~/Images/school_logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    ImageData logoData = ImageDataFactory.Create(logoPath);

                    // Left logo
                    iText.Layout.Element.Image leftLogo = new iText.Layout.Element.Image(logoData);
                    leftLogo.ScaleToFit(80, 80);
                    leftLogoCell.Add(leftLogo);

                    // Right logo
                    iText.Layout.Element.Image rightLogo = new iText.Layout.Element.Image(logoData);
                    rightLogo.ScaleToFit(80, 80);
                    rightLogoCell.Add(rightLogo);
                }
                else
                {
                    // If no logo file, add placeholder
                    leftLogoCell.Add(new Paragraph("LOGO").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                    rightLogoCell.Add(new Paragraph("LOGO").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                }
            }
            catch (Exception)
            {
                // If logo loading fails, add placeholder
                leftLogoCell.Add(new Paragraph("LOGO").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                rightLogoCell.Add(new Paragraph("LOGO").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
            }

            headerTable.AddCell(leftLogoCell);

            // School Details Cell
            Cell schoolDetailsCell = new Cell();
            schoolDetailsCell.SetBorder(Border.NO_BORDER);
            schoolDetailsCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);

            // School name
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            Paragraph schoolName = new Paragraph("LAKSHMI CONVENT SCHOOL")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetFontColor(ColorConstants.BLUE)
                .SetTextAlignment(TextAlignment.CENTER);
            schoolDetailsCell.Add(schoolName);

            // English Medium with decorative lines
            Table englishMediumTable = new Table(1).UseAllAvailableWidth();
            Cell englishMediumCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetMargin(0);

            // Top pink line
            Cell topLineCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetHeight(2)
                .SetBackgroundColor(new DeviceRgb(255, 0, 255)) // Pink
                .SetMarginBottom(1);
            englishMediumTable.AddCell(topLineCell);

            // English Medium text
            Cell englishTextCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .Add(new Paragraph("English Medium")
                    .SetFontSize(12)
                    .SetFontColor(new DeviceRgb(0, 128, 0)) // Green
                    .SetTextAlignment(TextAlignment.CENTER));
            englishMediumTable.AddCell(englishTextCell);

            // Bottom pink line
            Cell bottomLineCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetHeight(2)
                .SetBackgroundColor(new DeviceRgb(255, 0, 255)) // Pink
                .SetMarginTop(1);
            englishMediumTable.AddCell(bottomLineCell);

            schoolDetailsCell.Add(englishMediumTable);

            // Address
            Paragraph address = new Paragraph("MAHAVIR COLONY, HISAR , HARIYANA ,")
                .SetFontSize(10)
                .SetFontColor(new DeviceRgb(128, 0, 128)) // Purple
                .SetTextAlignment(TextAlignment.CENTER);
            schoolDetailsCell.Add(address);

            // Email
            Paragraph email = new Paragraph("E-mail Id:- sumit16parul@gmail.com")
                .SetFontSize(10)
                .SetFontColor(ColorConstants.BLUE)
                .SetTextAlignment(TextAlignment.CENTER);
            schoolDetailsCell.Add(email);

            // Session
            Paragraph session = new Paragraph("SESSION : (2024-25)")
                .SetFontSize(10)
                .SetFontColor(ColorConstants.RED)
                .SetTextAlignment(TextAlignment.CENTER);
            schoolDetailsCell.Add(session);

            // Admit Card Title
            Table admitCardTable = new Table(1).UseAllAvailableWidth();
            admitCardTable.SetWidth(UnitValue.CreatePercentValue(50));
            Cell admitCardCell = new Cell()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1))
                .Add(new Paragraph(":: ADMIT CARD ::")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12)
                    .SetFont(boldFont))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            admitCardTable.AddCell(admitCardCell);

            Table centerTable = new Table(1).UseAllAvailableWidth();
            Cell centerCell = new Cell().SetBorder(Border.NO_BORDER).Add(admitCardTable);
            centerCell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
            schoolDetailsCell.Add(centerCell);

            headerTable.AddCell(schoolDetailsCell);
            headerTable.AddCell(rightLogoCell);

            // Add header to inner cell
            innerCell.Add(headerTable);

            // Student Details and Photo Table
            Table studentTable = new Table(UnitValue.CreatePercentArray(new float[] { 80, 20 }));
            studentTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Student Details Cell
            Cell studentDetailsCell = new Cell();
            studentDetailsCell.SetBorder(Border.NO_BORDER);

            // Create a table for student details (field, colon, value)
            Table detailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 25, 5, 70 }));
            detailsTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Add student details rows
            AddDetailRow(detailsTable, "STUDENT'S NAME", student.Name, boldFont, ColorConstants.BLUE);
            AddDetailRow(detailsTable, "MOTHER'S NAME", student.MotherName, boldFont, ColorConstants.BLUE);
            AddDetailRow(detailsTable, "FATHER'S NAME", student.FatherName, boldFont, ColorConstants.BLUE);
            AddDetailRow(detailsTable, "CLASS", student.ClassName, boldFont, ColorConstants.BLUE);
            AddDetailRow(detailsTable, "SECTION", student.Section, boldFont, ColorConstants.BLUE);
            AddDetailRow(detailsTable, "ROLL NO.", student.RollNo, boldFont, ColorConstants.BLUE);
            AddDetailRow(detailsTable, "ADMISSION NO", student.AdmissionNo, boldFont, ColorConstants.BLUE);

            studentDetailsCell.Add(detailsTable);
            studentTable.AddCell(studentDetailsCell);

            // Photo Cell
            Cell photoCell = new Cell();
            photoCell.SetBorder(Border.NO_BORDER);
            photoCell.SetVerticalAlignment(VerticalAlignment.TOP);
            photoCell.SetHorizontalAlignment(HorizontalAlignment.CENTER);

            try
            {
                // Try to load student photo or use placeholder
                if (!string.IsNullOrEmpty(student.PhotoPath) && System.IO.File.Exists(student.PhotoPath))
                {
                    ImageData photoData = ImageDataFactory.Create(student.PhotoPath);
                    iText.Layout.Element.Image photoImage = new iText.Layout.Element.Image(photoData);
                    photoImage.ScaleToFit(80, 100);
                    photoImage.SetBorder(new SolidBorder(ColorConstants.GRAY, 1));
                    photoCell.Add(photoImage);
                }
                else
                {
                    // Use placeholder image or create one
                    string placeholderPath = Server.MapPath("~/Images/photo_placeholder.png");
                    if (System.IO.File.Exists(placeholderPath))
                    {
                        ImageData placeholderData = ImageDataFactory.Create(placeholderPath);
                        iText.Layout.Element.Image placeholderImage = new iText.Layout.Element.Image(placeholderData);
                        placeholderImage.ScaleToFit(80, 100);
                        placeholderImage.SetBorder(new SolidBorder(ColorConstants.GRAY, 1));
                        photoCell.Add(placeholderImage);
                    }
                    else
                    {
                        // Create a text placeholder
                        Div photoPlaceholder = new Div();
                        photoPlaceholder.SetWidth(80);
                        photoPlaceholder.SetHeight(100);
                        photoPlaceholder.SetBorder(new SolidBorder(ColorConstants.GRAY, 1));
                        photoPlaceholder.SetBackgroundColor(ColorConstants.LIGHT_GRAY);

                        Paragraph placeholderText = new Paragraph("PHOTO\nNOT\nAVAILABLE")
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(10);
                        photoPlaceholder.Add(placeholderText);

                        photoCell.Add(photoPlaceholder);
                    }
                }
            }
            catch (Exception)
            {
                // Create a basic text placeholder if all else fails
                Div photoPlaceholder = new Div();
                photoPlaceholder.SetWidth(80);
                photoPlaceholder.SetHeight(100);
                photoPlaceholder.SetBorder(new SolidBorder(ColorConstants.GRAY, 1));
                photoPlaceholder.SetBackgroundColor(ColorConstants.LIGHT_GRAY);

                Paragraph placeholderText = new Paragraph("PHOTO\nNOT\nAVAILABLE")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10);
                photoPlaceholder.Add(placeholderText);

                photoCell.Add(photoPlaceholder);
            }

            studentTable.AddCell(photoCell);

            // Add student details table to inner cell
            innerCell.Add(studentTable);

            // Signature table
            Table signatureTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f }));
            signatureTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Principal signature
            Cell principalCell = new Cell();
            principalCell.SetBorder(Border.NO_BORDER);
            principalCell.SetTextAlignment(TextAlignment.CENTER);
            principalCell.Add(new Paragraph(".......................\nPRINCIPAL"));
            signatureTable.AddCell(principalCell);

            // Class teacher signature
            Cell teacherCell = new Cell();
            teacherCell.SetBorder(Border.NO_BORDER);
            teacherCell.SetTextAlignment(TextAlignment.CENTER);
            teacherCell.Add(new Paragraph("..............................\nCLASS TEACHER"));
            signatureTable.AddCell(teacherCell);

            // Student signature
            Cell studentSignCell = new Cell();
            studentSignCell.SetBorder(Border.NO_BORDER);
            studentSignCell.SetTextAlignment(TextAlignment.CENTER);
            studentSignCell.Add(new Paragraph(".....................\nSTUDENT"));
            signatureTable.AddCell(studentSignCell);

            // Add signature table to inner cell
            innerCell.Add(signatureTable);

            // Instructions header
            Paragraph instructionsHeader = new Paragraph("INSTRUCTIONS TO THE CANDIDATES:-")
                .SetFont(boldFont)
                .SetFontSize(10)
                .SetFontColor(ColorConstants.RED);
            innerCell.Add(instructionsHeader);

            // Instructions list
            String[] instructions = {
                "01.The candidate must keep this admit card at the time of Examination and present on demand to the authorised person.",
                "02. Candidate must report at the examination venue 10 minutes before scheduled commencement of the examination.",
                "03. No candidate will be allowed to enter the examination room 30 minutes after the scheduled start of the examination.",
                "04. Candidate will not be permitted to leave the examination room before the end of the examination.",
                "05. Sharing of any stationary items (pen,pencils,eraser etc.) are STRICTLY PROHIBITED.",
                "06. Do not put any specific mark on either Question Paper or Answer Sheet. This act will be treated as unfair means and that case answer script will not be evaluated.",
                "07. To get a new copy of Admit Card, INR 45.00 will be charged."
            };

            for (int i = 0; i < instructions.Length; i++)
            {
                Paragraph instruction = new Paragraph(instructions[i])
                    .SetFontSize(8)
                    .SetMarginTop(0)
                    .SetMarginBottom(0)
                    .SetMultipliedLeading(1.0f);
                innerCell.Add(instruction);
            }

            // Add inner cell to inner table
            innerTable.AddCell(innerCell);

            // Add inner table to outer cell
            outerCell.Add(innerTable);

            // Add outer cell to outer table
            outerTable.AddCell(outerCell);

            // Add the table to the document
            document.Add(outerTable);
        }

        // Helper method to add detail row to the table
        private void AddDetailRow(Table table, string label, string value, PdfFont boldFont, Color color)
        {
            // Label cell
            Cell labelCell = new Cell();
            labelCell.SetBorder(Border.NO_BORDER);
            labelCell.Add(new Paragraph(label)
                .SetFont(boldFont)
                .SetFontColor(color)
                .SetFontSize(10));
            table.AddCell(labelCell);

            // Colon cell
            Cell colonCell = new Cell();
            colonCell.SetBorder(Border.NO_BORDER);
            colonCell.Add(new Paragraph(":")
                .SetFontSize(10));
            table.AddCell(colonCell);

            // Value cell
            Cell valueCell = new Cell();
            valueCell.SetBorder(Border.NO_BORDER);
            valueCell.Add(new Paragraph(value)
                .SetFontSize(10));
            table.AddCell(valueCell);
        }

        // Sample student data for testing
        private List<StudentAdmitModel> GetSampleStudentData()
        {
            return new List<StudentAdmitModel>
            {
                new StudentAdmitModel
                {
                    Name = "Abeera Hayat",
                    MotherName = "Mrs. Chandani Bi",
                    FatherName = "Mr Mohd Irshad",
                    ClassName = "Nursery",
                    Section = "A",
                    RollNo = "1",
                    AdmissionNo = "1",
                    PhotoPath = null
                },
                new StudentAdmitModel
                {
                    Name = "mohit",
                    MotherName = "MOHNI DEVI",
                    FatherName = "MOHIT KUMAR",
                    ClassName = "Nursery",
                    Section = "A",
                    RollNo = "43532",
                    AdmissionNo = "1",
                    PhotoPath = null
                }
            };
        }
    }

    // Model class for student data
    public class StudentAdmitModel
    {
        public string Name { get; set; }
        public string MotherName { get; set; }
        public string FatherName { get; set; }
        public string ClassName { get; set; }
        public string Section { get; set; }
        public string RollNo { get; set; }
        public string AdmissionNo { get; set; }
        public string PhotoPath { get; set; }
    }
}