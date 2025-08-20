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
    public class BirthCertificateController : Controller
    {
        // GET: BirthCertificate
        public ActionResult Index()
        {
            return View();
        }

        // Action method to generate the birth certificate
        public ActionResult GenerateBirthCertificate(StudentDOBModel student = null)
        {
            // If no student data is provided, use sample data
            if (student == null)
            {
                student = GetSampleStudentData();
            }

            // Set up the memory stream for PDF
            using (MemoryStream ms = new MemoryStream())
            {
                // Initialize PDF writer and document
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf, PageSize.A4);
                document.SetMargins(20, 20, 20, 20);

                // Create the certificate
                CreateBirthCertificate(pdf, document, student);

                // Close the document
                document.Close();

                // Return the PDF
                return File(ms.ToArray(), "application/pdf");
            }
        }

        // Method to create the birth certificate
        private void CreateBirthCertificate(PdfDocument pdf, Document document, StudentDOBModel student)
        {
            // Create the outer border
            Table outerTable = new Table(1).UseAllAvailableWidth();
            outerTable.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));

            // Create the inner content with a margin
            Cell outerCell = new Cell();
            outerCell.SetPadding(10);
            outerCell.SetBorder(Border.NO_BORDER);

            // Inner border table
            Table innerTable = new Table(1).UseAllAvailableWidth();
            innerTable.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));

            Cell innerCell = new Cell();
            innerCell.SetPadding(10);
            innerCell.SetBorder(Border.NO_BORDER);

            // Header Table
            Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 25, 75 }));
            headerTable.SetWidth(UnitValue.CreatePercentValue(100));

            // School Logo
            Cell logoCell = new Cell();
            logoCell.SetBorder(Border.NO_BORDER);
            logoCell.SetPadding(5);

            try
            {
                // Try to load the school logo
                string logoPath = Server.MapPath("~/Images/school_logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    ImageData logoData = ImageDataFactory.Create(logoPath);
                    iText.Layout.Element.Image logoImage = new iText.Layout.Element.Image(logoData);
                    logoImage.ScaleToFit(100, 100);
                    logoCell.Add(logoImage);
                }
                else
                {
                    // If logo file doesn't exist, add a placeholder
                    logoCell.Add(new Paragraph("LOGO"));
                }
            }
            catch (Exception)
            {
                // If logo loading fails, add a placeholder text
                logoCell.Add(new Paragraph("LOGO"));
            }

            headerTable.AddCell(logoCell);

            // School Details
            Cell schoolDetailsCell = new Cell();
            schoolDetailsCell.SetBorder(Border.NO_BORDER);
            schoolDetailsCell.SetPadding(5);

            // School name
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            Paragraph schoolName = new Paragraph("LAKSHMI CONVENT SCHOOL")
                .SetFont(boldFont)
                .SetFontSize(18)
                .SetTextAlignment(TextAlignment.CENTER);
            schoolDetailsCell.Add(schoolName);

            // School address
            Paragraph schoolAddress = new Paragraph("MAHAVIR COLONY, HISAR, HARIYANA,")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER);
            schoolDetailsCell.Add(schoolAddress);

            // Contact info
            Paragraph contactInfo = new Paragraph("9917004062, 9917004062")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER);
            schoolDetailsCell.Add(contactInfo);

            // Website
            Paragraph website = new Paragraph("Website :- sumit16parul@gmail.com")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER);
            schoolDetailsCell.Add(website);

            // Email
            Paragraph email = new Paragraph("Email :- sumit16parul@gmail.com")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER);
            schoolDetailsCell.Add(email);

            // Session
            Paragraph session = new Paragraph("SESSION :- (2024-25)")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER);
            schoolDetailsCell.Add(session);

            headerTable.AddCell(schoolDetailsCell);

            // Add header table to inner cell
            innerCell.Add(headerTable);

            // Add a separator line
            Table separatorTable = new Table(1).UseAllAvailableWidth();
            Cell separatorCell = new Cell();
            separatorCell.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
            separatorCell.SetHeight(1);
            separatorTable.AddCell(separatorCell);
            innerCell.Add(separatorTable);

            // Certificate Title and Photo Section
            Table titlePhotoTable = new Table(UnitValue.CreatePercentArray(new float[] { 70, 30 }));
            titlePhotoTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Certificate Title
            Cell titleCell = new Cell();
            titleCell.SetBorder(Border.NO_BORDER);
            titleCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);

            // Certificate title in a gray box
            Table titleBoxTable = new Table(1).UseAllAvailableWidth();
            Cell titleBoxCell = new Cell();
            titleBoxCell.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            titleBoxCell.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
            titleBoxCell.SetBorderRadius(new BorderRadius(5));

            Paragraph titleText = new Paragraph("DATE OF BIRTH CERTIFICATE")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER);
            titleBoxCell.Add(titleText);

            titleBoxTable.AddCell(titleBoxCell);
            titleCell.Add(titleBoxTable);

            titlePhotoTable.AddCell(titleCell);

            // Photo placeholder
            Cell photoCell = new Cell();
            photoCell.SetBorder(Border.NO_BORDER);
            photoCell.SetPadding(5);
            photoCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);

            try
            {
                // Try to load student photo or use placeholder
                if (!string.IsNullOrEmpty(student.PhotoPath) && System.IO.File.Exists(student.PhotoPath))
                {
                    ImageData photoData = ImageDataFactory.Create(student.PhotoPath);
                    iText.Layout.Element.Image photoImage = new iText.Layout.Element.Image(photoData);
                    photoImage.ScaleToFit(100, 120);
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
                        placeholderImage.ScaleToFit(100, 120);
                        placeholderImage.SetBorder(new SolidBorder(ColorConstants.GRAY, 1));
                        photoCell.Add(placeholderImage);
                    }
                    else
                    {
                        // Create a text placeholder
                        Div photoPlaceholder = new Div();
                        photoPlaceholder.SetWidth(100);
                        photoPlaceholder.SetHeight(120);
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
                photoPlaceholder.SetWidth(100);
                photoPlaceholder.SetHeight(120);
                photoPlaceholder.SetBorder(new SolidBorder(ColorConstants.GRAY, 1));
                photoPlaceholder.SetBackgroundColor(ColorConstants.LIGHT_GRAY);

                Paragraph placeholderText = new Paragraph("PHOTO\nNOT\nAVAILABLE")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10);
                photoPlaceholder.Add(placeholderText);

                photoCell.Add(photoPlaceholder);
            }

            titlePhotoTable.AddCell(photoCell);

            // Add title and photo table to inner cell
            innerCell.Add(titlePhotoTable);

            // Add some spacing
            innerCell.Add(new Paragraph("\n"));

            // Certificate content
            Paragraph line1 = new Paragraph();
            line1.Add(new Text("This is to certify that Mr./Miss "));
            line1.Add(new Text(student.Name).SetFont(boldFont));
            line1.Add(new Text(" Class "));
            line1.Add(new Text(student.ClassName).SetFont(boldFont));
            line1.Add(new Text(" Son/Daughter of Mr "));
            line1.Add(new Text(student.FatherName).SetFont(boldFont));
            innerCell.Add(line1);

            Paragraph line2 = new Paragraph();
            line2.Add(new Text(" is a student of my school. And Reading in the year of "));
            line2.Add(new Text(student.EnrollmentDate.ToString("dd/MM/yyyy")).SetFont(boldFont));
            innerCell.Add(line2);

            Paragraph line3 = new Paragraph();
            line3.Add(new Text("His/Her date of birth is "));
            line3.Add(new Text(student.DateOfBirth.ToString("dd/MM/yyyy")).SetFont(boldFont));
            line3.Add(new Text(" as stands at school register."));
            innerCell.Add(line3);

            // Add some spacing
            innerCell.Add(new Paragraph("\n\n"));

            // Footer with Office Seal and Signature
            Table footerTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }));
            footerTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Office Seal
            Cell sealCell = new Cell();
            sealCell.SetBorder(Border.NO_BORDER);

            Paragraph sealText = new Paragraph("Office Seal");
            sealText.SetFont(boldFont);
            sealCell.Add(sealText);

            Paragraph dateText = new Paragraph("Date ...................................");
            sealCell.Add(dateText);

            footerTable.AddCell(sealCell);

            // Signature
            Cell signatureCell = new Cell();
            signatureCell.SetBorder(Border.NO_BORDER);
            signatureCell.SetTextAlignment(TextAlignment.RIGHT);

            // Dashed line for signature
            Table signatureLineTable = new Table(1).UseAllAvailableWidth();
            Cell signatureLineCell = new Cell();
            signatureLineCell.SetBorder(new DashedBorder(ColorConstants.BLACK, 1));
            signatureLineCell.SetHeight(1);
            signatureLineTable.AddCell(signatureLineCell);

            signatureCell.Add(signatureLineTable);
            signatureCell.Add(new Paragraph("Signature of the principal").SetFont(boldFont));

            footerTable.AddCell(signatureCell);

            // Add footer to inner cell
            innerCell.Add(footerTable);

            // Add inner cell to inner table
            innerTable.AddCell(innerCell);

            // Add inner table to outer cell
            outerCell.Add(innerTable);

            // Add outer cell to outer table
            outerTable.AddCell(outerCell);

            // Add outer table to document
            document.Add(outerTable);
        }

        // Sample student data for testing
        private StudentDOBModel GetSampleStudentData()
        {
            return new StudentDOBModel
            {
                Name = "Abeera Hayat",
                ClassName = "Nursery",
                FatherName = "Mohd Irshad",
                DateOfBirth = new DateTime(2017, 11, 11),
                EnrollmentDate = new DateTime(2024, 4, 4),
                PhotoPath = null // Set actual path for testing
            };
        }
    }

    // Model class for student data
    public class StudentDOBModel
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string FatherName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string PhotoPath { get; set; }
        public StudentDOBModel()
        {
            Name = "Abeera Hayat";
            ClassName = "Nursery";
            FatherName = "Mohd Irshad";
            DateOfBirth = new DateTime(2017, 11, 11);
            EnrollmentDate = new DateTime(2024, 4, 4);
            PhotoPath = null;// Set actual path for testing
        }
    }
}