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
    public class CertificateoldController : Controller
    {
        // GET: Certificate
        public ActionResult Index()
        {
            return View();
        }

        // Action method to generate the sports certificate
        public ActionResult GenerateSportsCertificate(StudentCCModel student = null)
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
                Document document = new Document(pdf, PageSize.A4.Rotate()); // Landscape orientation
                document.SetMargins(20, 20, 20, 20);

                // Create the certificate
                CreateSportsCertificate(pdf, document, student);

                // Close the document
                document.Close();

                // Return the PDF
                return File(ms.ToArray(), "application/pdf");
            }
        }

        // Method to create the sports certificate
        private void CreateSportsCertificate(PdfDocument pdf, Document document, StudentCCModel student)
        {
            // Create the main container with border
            Table mainTable = new Table(1).UseAllAvailableWidth();
            mainTable.SetBorder(new SolidBorder(ColorConstants.BLACK, 2));

            // Create the cell for the entire content
            Cell mainCell = new Cell();
            mainCell.SetBorder(Border.NO_BORDER);
            mainCell.SetPadding(10);

            try
            {
                // 1. Add sports images on the left side
                // Football image (top-left)
                string footballImagePath = Server.MapPath("~/Images/football.png");
                if (System.IO.File.Exists(footballImagePath))
                {
                    ImageData footballImageData = ImageDataFactory.Create(footballImagePath);
                    iText.Layout.Element.Image footballImage = new iText.Layout.Element.Image(footballImageData);
                    footballImage.SetFixedPosition(40, 500, 100);
                    document.Add(footballImage);
                }

                // Tennis image (top-left middle)
                string tennisImagePath = Server.MapPath("~/Images/tennis.png");
                if (System.IO.File.Exists(tennisImagePath))
                {
                    ImageData tennisImageData = ImageDataFactory.Create(tennisImagePath);
                    iText.Layout.Element.Image tennisImage = new iText.Layout.Element.Image(tennisImageData);
                    tennisImage.SetFixedPosition(100, 530, 100);
                    document.Add(tennisImage);
                }

                // Volleyball image (left middle)
                string volleyballImagePath = Server.MapPath("~/Images/volleyball.png");
                if (System.IO.File.Exists(volleyballImagePath))
                {
                    ImageData volleyballImageData = ImageDataFactory.Create(volleyballImagePath);
                    iText.Layout.Element.Image volleyballImage = new iText.Layout.Element.Image(volleyballImageData);
                    volleyballImage.SetFixedPosition(70, 350, 100);
                    document.Add(volleyballImage);
                }

                // Basketball image (bottom-left)
                string basketballImagePath = Server.MapPath("~/Images/basketball.png");
                if (System.IO.File.Exists(basketballImagePath))
                {
                    ImageData basketballImageData = ImageDataFactory.Create(basketballImagePath);
                    iText.Layout.Element.Image basketballImage = new iText.Layout.Element.Image(basketballImageData);
                    basketballImage.SetFixedPosition(40, 200, 100);
                    document.Add(basketballImage);
                }

                // 2. Add sports images on the right side (mirror of left side)
                // Football image (bottom-right)
                if (System.IO.File.Exists(footballImagePath))
                {
                    ImageData footballImageData = ImageDataFactory.Create(footballImagePath);
                    iText.Layout.Element.Image footballImage = new iText.Layout.Element.Image(footballImageData);
                    footballImage.SetFixedPosition(680, 150, 100);
                    document.Add(footballImage);
                }

                // Tennis image (bottom-right middle)
                if (System.IO.File.Exists(tennisImagePath))
                {
                    ImageData tennisImageData = ImageDataFactory.Create(tennisImagePath);
                    iText.Layout.Element.Image tennisImage = new iText.Layout.Element.Image(tennisImageData);
                    tennisImage.SetFixedPosition(700, 250, 100);
                    document.Add(tennisImage);
                }

                // Volleyball image (right middle)
                if (System.IO.File.Exists(volleyballImagePath))
                {
                    ImageData volleyballImageData = ImageDataFactory.Create(volleyballImagePath);
                    iText.Layout.Element.Image volleyballImage = new iText.Layout.Element.Image(volleyballImageData);
                    volleyballImage.SetFixedPosition(680, 350, 100);
                    document.Add(volleyballImage);
                }

                // Basketball image (top-right)
                if (System.IO.File.Exists(basketballImagePath))
                {
                    ImageData basketballImageData = ImageDataFactory.Create(basketballImagePath);
                    iText.Layout.Element.Image basketballImage = new iText.Layout.Element.Image(basketballImageData);
                    basketballImage.SetFixedPosition(680, 500, 100);
                    document.Add(basketballImage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading sports images: " + ex.Message);
                // Continue without images if they fail to load
            }

            // Main content table
            Table contentTable = new Table(1).UseAllAvailableWidth();
            contentTable.SetBorder(Border.NO_BORDER);

            // School name
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            Paragraph schoolName = new Paragraph("LAKSHMI CONVENT SCHOOL")
                .SetFont(boldFont)
                .SetFontSize(36)
                .SetFontColor(ColorConstants.RED)
                .SetTextAlignment(TextAlignment.CENTER);
            Cell schoolNameCell = new Cell().Add(schoolName).SetBorder(Border.NO_BORDER);
            contentTable.AddCell(schoolNameCell);

            // School address
            Paragraph schoolAddress = new Paragraph("MAHAVIR COLONY, HISAR, HARIYANA,")
                .SetFontSize(14)
                .SetFontColor(ColorConstants.BLUE)
                .SetTextAlignment(TextAlignment.CENTER);
            Cell schoolAddressCell = new Cell().Add(schoolAddress).SetBorder(Border.NO_BORDER).SetPaddingTop(0);
            contentTable.AddCell(schoolAddressCell);

            // School contact info
            Paragraph contactInfo = new Paragraph("Mobile:- +91-9917004062, +91-9917004062")
                .SetFontSize(12)
                .SetFontColor(new DeviceRgb(128, 0, 128)) // Purple
                .SetTextAlignment(TextAlignment.CENTER);
            Cell contactInfoCell = new Cell().Add(contactInfo).SetBorder(Border.NO_BORDER).SetPaddingTop(0);
            contentTable.AddCell(contactInfoCell);

            // Email info
            Paragraph emailInfo = new Paragraph("Email ID:- ")
                .SetFontSize(12)
                .SetFontColor(ColorConstants.BLACK)
                .SetTextAlignment(TextAlignment.CENTER);

            // Add colored email text
            Text emailText1 = new Text("sumit16parul@gmail.com").SetFontColor(new DeviceRgb(0, 128, 0)); // Green
            Text emailText2 = new Text(" sumit16parul@gmail.com").SetFontColor(ColorConstants.BLACK);
            emailInfo.Add(emailText1).Add(emailText2);

            Cell emailInfoCell = new Cell().Add(emailInfo).SetBorder(Border.NO_BORDER).SetPaddingTop(0);
            contentTable.AddCell(emailInfoCell);

            // Session info
            Paragraph sessionInfo = new Paragraph("SESSION: (2024-25)")
                .SetFontSize(14)
                .SetFontColor(new DeviceRgb(128, 0, 128)) // Purple
                .SetTextAlignment(TextAlignment.CENTER);
            Cell sessionInfoCell = new Cell().Add(sessionInfo).SetBorder(Border.NO_BORDER).SetPaddingTop(0);
            contentTable.AddCell(sessionInfoCell);

            // Sports Certificate title
            try
            {
                // Try to load the fancy "Sports Certificate" text as an image
                string certificateTitlePath = Server.MapPath("~/Images/sports_certificate_title.png");
                if (System.IO.File.Exists(certificateTitlePath))
                {
                    ImageData certificateTitleData = ImageDataFactory.Create(certificateTitlePath);
                    iText.Layout.Element.Image certificateTitleImage = new iText.Layout.Element.Image(certificateTitleData);
                    certificateTitleImage.SetWidth(300);
                    Cell certificateTitleCell = new Cell().Add(certificateTitleImage).SetBorder(Border.NO_BORDER);
                    certificateTitleCell.SetTextAlignment(TextAlignment.CENTER);
                    certificateTitleCell.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                    contentTable.AddCell(certificateTitleCell);
                }
                else
                {
                    // Fallback to text if image is not available
                    Paragraph certificateTitle = new Paragraph("Sports Certificate")
                        .SetFontSize(36)
                        .SetFontColor(new DeviceRgb(205, 133, 63)) // Bronze/orange
                        .SetTextAlignment(TextAlignment.CENTER);
                    Cell certificateTitleCell = new Cell().Add(certificateTitle).SetBorder(Border.NO_BORDER);
                    contentTable.AddCell(certificateTitleCell);
                }
            }
            catch (Exception)
            {
                // Fallback to text if image loading fails
                Paragraph certificateTitle = new Paragraph("Sports Certificate")
                    .SetFontSize(36)
                    .SetFontColor(new DeviceRgb(205, 133, 63)) // Bronze/orange
                    .SetTextAlignment(TextAlignment.CENTER);
                Cell certificateTitleCell = new Cell().Add(certificateTitle).SetBorder(Border.NO_BORDER);
                contentTable.AddCell(certificateTitleCell);
            }

            // "IS PRESENTED TO" text
            Paragraph presentedTo = new Paragraph("IS PRESENTED TO")
                .SetFontSize(14)
                .SetFontColor(ColorConstants.BLACK)
                .SetTextAlignment(TextAlignment.CENTER);
            Cell presentedToCell = new Cell().Add(presentedTo).SetBorder(Border.NO_BORDER).SetPaddingTop(20);
            contentTable.AddCell(presentedToCell);

            // Student name with underline
            Paragraph studentName = new Paragraph(student.Name)
                .SetFontSize(16)
                .SetFont(boldFont)
                .SetFontColor(ColorConstants.BLACK)
                .SetTextAlignment(TextAlignment.CENTER);

            // Add an underline using a table with bottom border
            Table nameUnderlineTable = new Table(1).UseAllAvailableWidth();
            nameUnderlineTable.SetWidth(UnitValue.CreatePercentValue(70));
            Cell nameCell = new Cell().Add(studentName).SetBorder(Border.NO_BORDER);
            nameCell.SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 1));
            nameCell.SetPaddingBottom(2);
            nameUnderlineTable.AddCell(nameCell);

            Cell nameUnderlineCell = new Cell().Add(nameUnderlineTable).SetBorder(Border.NO_BORDER).SetHorizontalAlignment(HorizontalAlignment.CENTER);
            contentTable.AddCell(nameUnderlineCell);

            // "for outstanding accomplishments..." text
            Paragraph accomplishments = new Paragraph("for outstanding accomplishments and sportmanship in")
                .SetFontSize(14)
                .SetFontColor(ColorConstants.BLACK)
                .SetTextAlignment(TextAlignment.CENTER);
            Cell accomplishmentsCell = new Cell().Add(accomplishments).SetBorder(Border.NO_BORDER).SetPaddingTop(20);
            contentTable.AddCell(accomplishmentsCell);

            // Class and Address with underline in a table
            Table classAddressTable = new Table(2).UseAllAvailableWidth();
            classAddressTable.SetWidth(UnitValue.CreatePercentValue(90));

            // Class part
            Paragraph classText = new Paragraph(student.ClassName)
                .SetFontSize(14)
                .SetFontColor(ColorConstants.BLACK)
                .SetTextAlignment(TextAlignment.CENTER);
            Cell classCell = new Cell().Add(classText).SetBorder(Border.NO_BORDER);
            classCell.SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 1));
            classCell.SetPaddingBottom(2);
            classCell.SetWidth(UnitValue.CreatePercentValue(20));
            classAddressTable.AddCell(classCell);

            // Address part
            Paragraph addressText = new Paragraph(student.Address)
                .SetFontSize(14)
                .SetFontColor(ColorConstants.BLACK)
                .SetTextAlignment(TextAlignment.CENTER);
            Cell addressCell = new Cell().Add(addressText).SetBorder(Border.NO_BORDER);
            addressCell.SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 1));
            addressCell.SetPaddingBottom(2);
            addressCell.SetWidth(UnitValue.CreatePercentValue(80));
            classAddressTable.AddCell(addressCell);

            Cell classAddressContainerCell = new Cell().Add(classAddressTable).SetBorder(Border.NO_BORDER);
            classAddressContainerCell.SetPaddingTop(20);
            contentTable.AddCell(classAddressContainerCell);

            // Date and Signature section
            Table dateSignatureTable = new Table(2).UseAllAvailableWidth();

            // Date part
            Cell dateLabelCell = new Cell().Add(
                new Paragraph("DATE:").SetFontColor(new DeviceRgb(205, 133, 63)).SetFontSize(14)
            ).SetBorder(Border.NO_BORDER).SetPaddingTop(40);

            Paragraph dateText = new Paragraph(DateTime.Now.ToString("dd-MMM-yyyy"))
                .SetFontSize(14)
                .SetFontColor(ColorConstants.BLACK)
                .SetTextAlignment(TextAlignment.LEFT);

            Table dateUnderlineTable = new Table(1).UseAllAvailableWidth();
            Cell dateValueCell = new Cell().Add(dateText).SetBorder(Border.NO_BORDER);
            dateValueCell.SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 1));
            dateUnderlineTable.AddCell(dateValueCell);

            Cell dateContainerCell = new Cell().Add(dateLabelCell).Add(dateUnderlineTable).SetBorder(Border.NO_BORDER);
            dateSignatureTable.AddCell(dateContainerCell);

            // Signature part
            Cell signatureLabelCell = new Cell().Add(
                new Paragraph("SIGNATURE:").SetFontColor(new DeviceRgb(205, 133, 63)).SetFontSize(14)
            ).SetBorder(Border.NO_BORDER).SetPaddingTop(40);

            Table signatureUnderlineTable = new Table(1).UseAllAvailableWidth();
            Cell signatureValueCell = new Cell().Add(new Paragraph(" ")).SetBorder(Border.NO_BORDER);
            signatureValueCell.SetBorderBottom(new SolidBorder(ColorConstants.BLACK, 1));
            signatureUnderlineTable.AddCell(signatureValueCell);

            Cell signatureContainerCell = new Cell().Add(signatureLabelCell).Add(signatureUnderlineTable).SetBorder(Border.NO_BORDER);
            dateSignatureTable.AddCell(signatureContainerCell);

            Cell dateSignatureContainerCell = new Cell().Add(dateSignatureTable).SetBorder(Border.NO_BORDER);
            contentTable.AddCell(dateSignatureContainerCell);

            // Try to add the school logo
            try
            {
                string logoPath = Server.MapPath("~/Images/school_logo.png");
                if (System.IO.File.Exists(logoPath))
                {
                    ImageData logoData = ImageDataFactory.Create(logoPath);
                    iText.Layout.Element.Image logoImage = new iText.Layout.Element.Image(logoData);
                    logoImage.SetFixedPosition(220, 240, 90);
                    document.Add(logoImage);
                }
            }
            catch (Exception)
            {
                // Continue without logo if it fails to load
            }

            // Try to add the medal/award icon
            try
            {
                string medalPath = Server.MapPath("~/Images/medal.png");
                if (System.IO.File.Exists(medalPath))
                {
                    ImageData medalData = ImageDataFactory.Create(medalPath);
                    iText.Layout.Element.Image medalImage = new iText.Layout.Element.Image(medalData);
                    medalImage.SetFixedPosition(620, 280, 80);
                    document.Add(medalImage);
                }
            }
            catch (Exception)
            {
                // Continue without medal if it fails to load
            }

            // Try to add the student photo placeholder
            try
            {
                // If student has a photo, use it, otherwise use placeholder
                if (!string.IsNullOrEmpty(student.PhotoPath) && System.IO.File.Exists(student.PhotoPath))
                {
                    ImageData photoData = ImageDataFactory.Create(student.PhotoPath);
                    iText.Layout.Element.Image photoImage = new iText.Layout.Element.Image(photoData);
                    photoImage.SetFixedPosition(680, 350, 120);
                    document.Add(photoImage);
                }
                else
                {
                    // Use placeholder image
                    string placeholderPath = Server.MapPath("~/Images/photo_placeholder.png");
                    if (System.IO.File.Exists(placeholderPath))
                    {
                        ImageData placeholderData = ImageDataFactory.Create(placeholderPath);
                        iText.Layout.Element.Image placeholderImage = new iText.Layout.Element.Image(placeholderData);
                        placeholderImage.SetFixedPosition(620, 360, 120);
                        document.Add(placeholderImage);
                    }
                }
            }
            catch (Exception)
            {
                // Continue without photo if it fails to load
            }

            // Add the content table to the main cell
            mainCell.Add(contentTable);

            // Add the main cell to the main table
            mainTable.AddCell(mainCell);

            // Add the main table to the document
            document.Add(mainTable);
        }

        // Sample student data for testing
        private StudentCCModel GetSampleStudentData()
        {
            return new StudentCCModel
            {
                Name = "Abeera Hayat Jainul Irshad S/O, D /O Mr Mohd Irshad",
                ClassName = "Nursery",
                Address = "Vill prithvipur urf Chiriya khera Teh Bilaspur Rampur",
                DateOfIssue = DateTime.Now,
                PhotoPath = null // Set actual path for testing
            };
        }
    }

    // Model class for student data
    public class StudentCCModel
    {
        public string Name { get; set; }
        public string ClassName { get; set; }
        public string Address { get; set; }
        public DateTime DateOfIssue { get; set; }
        public string PhotoPath { get; set; }
        public StudentCCModel()
        {
            Name = "Abeera Hayat Jainul Irshad S/O, D /O Mr Mohd Irshad";
            ClassName = "Nursery";
            Address = "Vill prithvipur urf Chiriya khera Teh Bilaspur Rampur";
            DateOfIssue = DateTime.Now;
            PhotoPath = null;
        }
    }
}