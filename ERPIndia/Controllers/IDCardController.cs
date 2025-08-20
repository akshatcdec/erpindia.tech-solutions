using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class IDCardController : Controller
    {
        // GET: IDCard
        public ActionResult Index()
        {
            return View();
        }

        // Action method to generate ID cards
        public ActionResult GenerateIDCards(List<StudentIDCardModel> students)
        {
            // If you don't have students data, you can generate sample data for testing
            if (students == null || !students.Any())
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
                document.SetMargins(20, 20, 20, 20);

                // Number of cards per page (2x5 grid for exactly 10 cards)
                int cardsPerRow = 2;
                int rowsPerPage = 5;
                int cardWidth = 250;
                int cardHeight = 140;

                // Spacing between cards
                float horizontalSpacing = 20;
                float verticalSpacing = 5;

                // Counter to track position
                int counter = 0;

                // Process each student to create ID card
                foreach (var student in students)
                {
                    // Calculate position on page
                    int row = (counter / cardsPerRow) % rowsPerPage;
                    int col = counter % cardsPerRow;

                    // Check if we need a new page
                    if (counter > 0 && counter % (cardsPerRow * rowsPerPage) == 0)
                    {
                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                    }

                    // Calculate coordinates
                    float x = col * (cardWidth + horizontalSpacing) + document.GetLeftMargin();
                    float y = document.GetPageEffectiveArea(PageSize.A4).GetHeight() - (row * (cardHeight + verticalSpacing) + cardHeight);

                    // Create and add the ID card
                    CreateIDCard(pdf, document, student, x, y, cardWidth, cardHeight);

                    counter++;
                }

                // Close the document
                document.Close();

                // Return the PDF
                return File(ms.ToArray(), "application/pdf");
            }
        }

        // Method to create a single ID card
        private void CreateIDCard(PdfDocument pdf, Document document, StudentIDCardModel student, float x, float y, float width, float height)
        {
            // Create the main outer container
            Table cardTable = new Table(UnitValue.CreatePercentArray(1)).UseAllAvailableWidth();
            cardTable.SetFixedPosition(x, y, width);
            cardTable.SetHeight(height);
            cardTable.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
            // Set a gradient-like background color
            cardTable.SetBackgroundColor(new DeviceRgb(220, 240, 255)); // Light blue background

            // Create header for school name
            Table headerTable = new Table(UnitValue.CreatePercentArray(1)).UseAllAvailableWidth();

            // School name
            Cell schoolNameCell = new Cell()
                .Add(new Paragraph("LAKSHMI CONVENT SCHOOL")
                    .SetFontColor(new DeviceRgb(255, 20, 147)) // Pink color
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(12)
                    .SetBold());
            schoolNameCell.SetBorder(Border.NO_BORDER);
            headerTable.AddCell(schoolNameCell);

            // School address
            Cell addressCell = new Cell()
                .Add(new Paragraph("MAHAVIR COLONY, HISAR, HARIYANA")
                    .SetFontColor(ColorConstants.BLUE)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(8));
            addressCell.SetBorder(Border.NO_BORDER);
            headerTable.AddCell(addressCell);

            // School contact
            Cell contactCell = new Cell()
                .Add(new Paragraph("Mob: 9917004062, 9917004062")
                    .SetFontColor(ColorConstants.BLUE)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(8));
            contactCell.SetBorder(Border.NO_BORDER);
            headerTable.AddCell(contactCell);

            // ID card title with yellow background
            Cell titleCell = new Cell()
                .Add(new Paragraph("IDENTITY CARD")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetBold());
            titleCell.SetBackgroundColor(ColorConstants.YELLOW);
            titleCell.SetBorder(Border.NO_BORDER);
            headerTable.AddCell(titleCell);

            // Main content table with two columns (Photo + Details)
            Table contentTable = new Table(UnitValue.CreatePercentArray(new float[] { 25, 75 })).UseAllAvailableWidth();

            // Photo placeholder
            Cell photoCell = new Cell();
            try
            {
                // If student has a photo, use it
                if (!string.IsNullOrEmpty(student.PhotoPath) && System.IO.File.Exists(student.PhotoPath))
                {
                    ImageData imageData = ImageDataFactory.Create(student.PhotoPath);
                    iText.Layout.Element.Image img = new iText.Layout.Element.Image(imageData);
                    img.SetWidth(UnitValue.CreatePercentValue(100));
                    img.SetHeight(UnitValue.CreatePercentValue(100));
                    photoCell.Add(img);
                }
                else
                {
                    // Add placeholder image or text
                    Div photoPlaceholder = new Div()
                        .SetWidth(60)
                        .SetHeight(70)
                        .SetBorder(new SolidBorder(ColorConstants.GRAY, 1))
                        .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                        .Add(new Paragraph("PHOTO\nNOT\nAVAILABLE")
                            .SetFontSize(8)
                            .SetTextAlignment(TextAlignment.CENTER));
                    photoCell.Add(photoPlaceholder);
                }
            }
            catch (Exception)
            {
                // Fallback to placeholder if image loading fails
                Div photoPlaceholder = new Div()
                    .SetWidth(60)
                    .SetHeight(70)
                    .SetBorder(new SolidBorder(ColorConstants.GRAY, 1))
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .Add(new Paragraph("PHOTO\nNOT\nAVAILABLE")
                        .SetFontSize(8)
                        .SetTextAlignment(TextAlignment.CENTER));
                photoCell.Add(photoPlaceholder);
            }
            photoCell.SetBorder(new SolidBorder(ColorConstants.GRAY, 1));
            photoCell.SetPadding(3);

            // Details column
            Cell detailsCell = new Cell();
            detailsCell.SetBorder(Border.NO_BORDER);
            detailsCell.SetPadding(3);

            // Student details
            AddDetailLine(detailsCell, "Name: ", student.Name, true);
            AddDetailLine(detailsCell, "Father: ", student.FatherName);
            AddDetailLine(detailsCell, "DOB: ", student.DateOfBirth.ToString("dd-MMM-yyyy"));
            AddDetailLine(detailsCell, "Mobile: ", student.Mobile);

            if (!string.IsNullOrEmpty(student.Address))
            {
                AddDetailLine(detailsCell, "Address:", student.Address);
            }

            // Add photo and details to content table
            contentTable.AddCell(photoCell);
            contentTable.AddCell(detailsCell);

            // Footer table for SR, Class and validity
            Table footerTable = new Table(UnitValue.CreatePercentArray(new float[] { 35, 65 })).UseAllAvailableWidth();

            // SR and Class
            Cell srClassCell = new Cell();
            srClassCell.SetBorder(Border.NO_BORDER);
            srClassCell.Add(new Paragraph("SR: " + student.SerialNumber)
                .SetFontSize(9))
                .Add(new Paragraph("Class: " + student.ClassName)
                .SetFontSize(9));
            footerTable.AddCell(srClassCell);

            // Validity and principal signature
            Cell validityCell = new Cell().SetBorder(Border.NO_BORDER);

            // Validity band with purple background
            Table validityTable = new Table(UnitValue.CreatePercentArray(1)).UseAllAvailableWidth();
            Cell validBand = new Cell()
                .Add(new Paragraph("Valid up to: 31 March 2026")
                    .SetFontColor(ColorConstants.WHITE)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER));
            validBand.SetBackgroundColor(new DeviceRgb(75, 0, 130)); // Purple
            validBand.SetBorder(Border.NO_BORDER);
            validityTable.AddCell(validBand);

            validityCell.Add(validityTable);
            validityCell.Add(new Paragraph("Signature of Principal")
                .SetFontSize(7)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(2));

            footerTable.AddCell(validityCell);

            try
            {
                // Draw the academic year text vertically on the side
                PdfPage page = pdf.GetPage(pdf.GetNumberOfPages());
                iText.Kernel.Pdf.Canvas.PdfCanvas canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);

                // Create font
                PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // Draw vertical text using direct matrix transformation
                canvas.SaveState()
                      .BeginText()
                      .SetFontAndSize(font, 16)
                      .SetFillColor(ColorConstants.RED)
                      .SetTextMatrix(0, 1, -1, 0, x + width - 15, y + height / 2 - 30)
                      .ShowText("2025:26")
                      .EndText()
                      .RestoreState();
            }
            catch (Exception ex)
            {
                // If vertical text fails, try a simpler approach with horizontal text
                Console.WriteLine("Error drawing vertical text: " + ex.Message);

                // Add a simple text as fallback
                Paragraph yearText = new Paragraph("2025:26")
                    .SetFontColor(ColorConstants.RED)
                    .SetFontSize(12)
                    .SetBold();

                Div yearDiv = new Div().Add(yearText)
                    .SetFixedPosition(x + width - 80, y + height - 25, 70)
                    .SetBorder(Border.NO_BORDER);

                document.Add(yearDiv);
            }

            // Add all components to the card
            Cell mainCell = new Cell().SetBorder(Border.NO_BORDER);
            mainCell.Add(headerTable);
            mainCell.Add(contentTable);
            mainCell.Add(footerTable);
            cardTable.AddCell(mainCell);

            // Add the card to the document
            document.Add(cardTable);
        }

        // Helper method to add a detail line
        private void AddDetailLine(Cell cell, string label, string value, bool bold = false)
        {
            Paragraph p = new Paragraph();
            Text labelText = new Text(label).SetFontColor(ColorConstants.DARK_GRAY).SetFontSize(9);
            Text valueText = new Text(value ?? "").SetFontColor(ColorConstants.BLACK).SetFontSize(9);

            if (bold)
            {
                labelText.SetBold();
                valueText.SetBold();
            }

            p.Add(labelText).Add(valueText);
            cell.Add(p);
        }

        // Generate sample data for testing
        private List<StudentIDCardModel> GetSampleStudentData()
        {
            return new List<StudentIDCardModel>
            {
                new StudentIDCardModel
                {
                    SerialNumber = "01 Nsy",
                    Name = "Abeera Hayat",
                    FatherName = "Mr Mohd Irshad",
                    DateOfBirth = new DateTime(2017, 11, 11),
                    Mobile = "8279708394",
                    Address = "Vill prithvipur urf Chiriya khera Teh Bilaspur Rampur",
                    ClassName = "Nursery- A",
                    PhotoPath = null // Set actual path for testing
                },
                new StudentIDCardModel
                {
                    SerialNumber = "2",
                    Name = "mohit",
                    FatherName = "MOHIT KUMAR",
                    DateOfBirth = new DateTime(2000, 5, 2),
                    Mobile = "9411578652",
                    Address = "MANIMAU KANNAUJ",
                    ClassName = "Nursery- A",
                    PhotoPath = null
                },
                new StudentIDCardModel
                {
                    SerialNumber = "3",
                    Name = "VINEET KASHYAP",
                    FatherName = "Mr. PUNNA",
                    DateOfBirth = new DateTime(2024, 8, 15),
                    Mobile = "",
                    Address = "",
                    ClassName = "Nursery- A",
                    PhotoPath = null
                },
                new     StudentIDCardModel
                {
                    SerialNumber = "10",
                    Name = "XYZ",
                    FatherName = "",
                    DateOfBirth = new DateTime(2021, 1, 1),
                    Mobile = "",
                    Address = "",
                     ClassName = "NURSERY- A",
                    PhotoPath = null
                },
                new StudentIDCardModel
                {
                    SerialNumber = "15",
                    Name = "Ruhi",
                    FatherName = "",
                    DateOfBirth = new DateTime(2024, 11, 13),
                    Mobile = "",
                    Address = "",
                    ClassName = "NURSERY- A",
                    PhotoPath = null
                },
                new StudentIDCardModel
                {
                    SerialNumber = "19",
                    Name = "keshav kumar",
                    FatherName = "",
                    DateOfBirth = new DateTime(2024, 12, 2),
                    Mobile = "",
                    Address = "",
                    ClassName = "NURSERY- A",
                    PhotoPath = null
                },
                new StudentIDCardModel
                {
                    SerialNumber = "20",
                    Name = "lalita",
                    FatherName = "",
                    DateOfBirth = new DateTime(2020, 11, 28),
                    Mobile = "",
                    Address = "",
                    ClassName = "NURSERY- A",
                    PhotoPath = null
                },
                new StudentIDCardModel
                {
                    SerialNumber = "22",
                    Name = "Sankalp",
                    FatherName = "",
                    DateOfBirth = new DateTime(2020, 1, 1),
                    Mobile = "1234567890",
                    Address = "",
                    ClassName = "NURSERY- A",
                    PhotoPath = null
                }
            };
        }
    }

    // Model class for student data
    public class StudentIDCardModel
    {
        public string SerialNumber { get; set; }
        public string Name { get; set; }
        public string FatherName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string ClassName { get; set; }
        public string PhotoPath { get; set; }
    }
}