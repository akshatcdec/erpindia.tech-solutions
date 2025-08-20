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

namespace ERPIndia.Controllerss
{
    public class CharacterCertificateController : Controller
    {
        // GET: CharacterCertificate
        public ActionResult Index()
        {
            return View();
        }

        // Action method to generate the character certificate
        public ActionResult GenerateCharacterCertificate(CharacterCertificateModel model = null)
        {
            // If no model is provided, use sample data
            if (model == null)
            {
                model = GetSampleData();
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
                CreateCharacterCertificate(pdf, document, model);

                // Close the document
                document.Close();

                // Return the PDF
                return File(ms.ToArray(), "application/pdf", "CharacterCertificate.pdf");
            }
        }

        // Method to create the character certificate
        private void CreateCharacterCertificate(PdfDocument pdf, Document document, CharacterCertificateModel model)
        {
            // Create main border
            Table mainTable = new Table(1).UseAllAvailableWidth();
            mainTable.SetBorder(new SolidBorder(ColorConstants.BLACK, 2));

            Cell mainCell = new Cell();
            mainCell.SetBorder(Border.NO_BORDER);
            mainCell.SetPadding(0);

            // Header table (3 columns: School logo, Session info, Photo)
            Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 25, 50, 25 }));
            headerTable.SetWidth(UnitValue.CreatePercentValue(100));

            // School logo on the left
            Cell logoCell = new Cell();
            logoCell.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
            logoCell.SetPadding(10);

            try
            {
                // Try to load decorative frame for logo
                string frameImagePath = Server.MapPath("~/Images/decorative_frame.png");
                if (System.IO.File.Exists(frameImagePath))
                {
                    ImageData frameData = ImageDataFactory.Create(frameImagePath);
                    iText.Layout.Element.Image frameImage = new iText.Layout.Element.Image(frameData);
                    frameImage.ScaleToFit(100, 100);
                    logoCell.Add(frameImage);
                }
                else
                {
                    // Create a decorative frame using text if image not available
                    Paragraph framePara = new Paragraph("🖋️ School Logo 🖋️")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(10);
                    logoCell.Add(framePara);
                }
            }
            catch (Exception)
            {
                // Fallback if image loading fails
                Paragraph framePara = new Paragraph("🖋️ School Logo 🖋️")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10);
                logoCell.Add(framePara);
            }

            // Session info in the middle
            Cell sessionCell = new Cell();
            sessionCell.SetBorder(Border.NO_BORDER);
            sessionCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);

            // Mobile info
            Paragraph mobileInfo = new Paragraph("Mob.:-")
                .SetFontColor(new DeviceRgb(128, 0, 128)) // Purple color
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12);

            // Add phone numbers with spacing
            Text phoneText = new Text(" +91-, +91-");
            mobileInfo.Add(phoneText);
            sessionCell.Add(mobileInfo);

            // Email info
            Paragraph emailInfo = new Paragraph("Email Id:-")
                .SetFontColor(ColorConstants.BLUE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(12);
            sessionCell.Add(emailInfo);

            // Academic Session with yellow background
            Table sessionTable = new Table(1).UseAllAvailableWidth();
            Cell sessionBgCell = new Cell();
            sessionBgCell.SetBackgroundColor(ColorConstants.YELLOW);
            sessionBgCell.SetBorder(Border.NO_BORDER);

            Paragraph sessionText = new Paragraph("ACADEMIC SESSION:")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(14);
            sessionBgCell.Add(sessionText);
            sessionTable.AddCell(sessionBgCell);
            sessionCell.Add(sessionTable);

            // Character Certificate title with green background
            Table titleTable = new Table(1).UseAllAvailableWidth();
            Cell titleCell = new Cell();
            titleCell.SetBackgroundColor(new DeviceRgb(76, 128, 0)); // Green background
            titleCell.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
            titleCell.SetBorderRadius(new BorderRadius(15)); // Rounded corners

            Paragraph titleText = new Paragraph("Charecter Certificate")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20)
                .SetFontColor(ColorConstants.WHITE)
                .SetPadding(5);
            titleCell.Add(titleText);
            titleTable.AddCell(titleCell);
            sessionCell.Add(titleTable);

            // Photo cell on the right
            Cell photoCell = new Cell();
            photoCell.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
            photoCell.SetPadding(5);
            photoCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);
            photoCell.SetHorizontalAlignment(HorizontalAlignment.CENTER);

            try
            {
                // Try to load student photo or use placeholder
                if (!string.IsNullOrEmpty(model.PhotoPath) && System.IO.File.Exists(model.PhotoPath))
                {
                    ImageData photoData = ImageDataFactory.Create(model.PhotoPath);
                    iText.Layout.Element.Image photoImage = new iText.Layout.Element.Image(photoData);
                    photoImage.ScaleToFit(90, 110);
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
                        placeholderImage.ScaleToFit(90, 110);
                        placeholderImage.SetBorder(new SolidBorder(ColorConstants.GRAY, 1));
                        photoCell.Add(placeholderImage);
                    }
                    else
                    {
                        // Create a text placeholder
                        Div photoPlaceholder = new Div();
                        photoPlaceholder.SetWidth(90);
                        photoPlaceholder.SetHeight(110);
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
                photoPlaceholder.SetWidth(90);
                photoPlaceholder.SetHeight(110);
                photoPlaceholder.SetBorder(new SolidBorder(ColorConstants.GRAY, 1));
                photoPlaceholder.SetBackgroundColor(ColorConstants.LIGHT_GRAY);

                Paragraph placeholderText = new Paragraph("PHOTO\nNOT\nAVAILABLE")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(10);
                photoPlaceholder.Add(placeholderText);

                photoCell.Add(photoPlaceholder);
            }

            // Certificate number row
            Table certNumberTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }));
            certNumberTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Serial number
            Cell serialCell = new Cell();
            serialCell.SetBorder(Border.NO_BORDER);
            serialCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph serialText = new Paragraph("SI.No.00 /22-23")
                .SetFontSize(12);
            serialCell.Add(serialText);
            certNumberTable.AddCell(serialCell);

            // Admission number
            Cell admissionCell = new Cell();
            admissionCell.SetBorder(Border.NO_BORDER);
            admissionCell.SetVerticalAlignment(VerticalAlignment.MIDDLE);
            admissionCell.SetTextAlignment(TextAlignment.RIGHT);

            Paragraph admissionText = new Paragraph("Admission No.")
                .SetFontSize(12);
            admissionCell.Add(admissionText);
            certNumberTable.AddCell(admissionCell);

            // Add cells to header table
            headerTable.AddCell(logoCell);
            headerTable.AddCell(sessionCell);
            headerTable.AddCell(photoCell);

            // Content table
            Table contentTable = new Table(1).UseAllAvailableWidth();
            contentTable.SetBorder(Border.NO_BORDER);

            // Add certificate number row
            Cell certNumberCell = new Cell();
            certNumberCell.SetBorder(Border.NO_BORDER);
            certNumberCell.Add(certNumberTable);
            contentTable.AddCell(certNumberCell);

            // Add line separator
            Cell separatorCell = new Cell();
            separatorCell.SetBorder(new SolidBorder(ColorConstants.BLACK, 1));
            separatorCell.SetHeight(1);
            contentTable.AddCell(separatorCell);

            // Certificate content
            Cell textCell = new Cell();
            textCell.SetBorder(Border.NO_BORDER);
            textCell.SetPadding(10);

            Paragraph p1 = new Paragraph();
            p1.Add(new Text("Certified that Mr/Ms. "));
            p1.Add(new Text("........................................").SetUnderline());
            p1.Add(new Text("son/daughter/wife"));
            p1.SetFontSize(12);
            textCell.Add(p1);

            Paragraph p2 = new Paragraph();
            p2.Add(new Text("of Shri "));
            p2.Add(new Text("................................................................").SetUnderline());
            p2.Add(new Text(" is well known to me"));
            p2.SetFontSize(12);
            textCell.Add(p2);

            Paragraph p3 = new Paragraph();
            p3.Add(new Text("since last "));
            p3.Add(new Text("...........").SetUnderline());
            p3.Add(new Text(" years and"));
            p3.Add(new Text(".............").SetUnderline());
            p3.Add(new Text(" months. To the best of my knowledge"));
            p3.SetFontSize(12);
            textCell.Add(p3);

            Paragraph p4 = new Paragraph();
            p4.Add(new Text("and belief he/she bears a good moral character and has nothing which"));
            p4.SetFontSize(12);
            textCell.Add(p4);

            Paragraph p5 = new Paragraph();
            p5.Add(new Text("debars his/her suitability for Government Job. Mr/Ms"));
            p5.Add(new Text("..................................."));
            p5.SetFontSize(12);
            textCell.Add(p5);

            Paragraph p6 = new Paragraph();
            p6.Add(new Text("is not related to me."));
            p6.SetFontSize(12);
            textCell.Add(p6);

            // Add some spacing
            textCell.Add(new Paragraph("\n"));

            Paragraph p7 = new Paragraph();
            p7.Add(new Text("I wish him/her all the successes in his/her life."));
            p7.SetFontSize(12);
            textCell.Add(p7);

            // Add more spacing
            textCell.Add(new Paragraph("\n\n"));

            // Footer table with place, date, signature, designation
            Table footerTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }));
            footerTable.SetWidth(UnitValue.CreatePercentValue(100));

            // Place and date on the left
            Cell placeCell = new Cell();
            placeCell.SetBorder(Border.NO_BORDER);

            Paragraph placeText = new Paragraph();
            placeText.Add(new Text("Place:"));
            placeText.Add(new Text("..........................."));
            placeText.SetFontSize(12);
            placeCell.Add(placeText);

            Paragraph dateText = new Paragraph();
            dateText.Add(new Text("Dated:"));
            dateText.Add(new Text("..........................."));
            dateText.SetFontSize(12);
            dateText.SetMarginTop(50);
            placeCell.Add(dateText);

            footerTable.AddCell(placeCell);

            // Signature and designation on the right
            Cell signatureCell = new Cell();
            signatureCell.SetBorder(Border.NO_BORDER);

            Paragraph signatureText = new Paragraph();
            signatureText.Add(new Text("Signature"));
            signatureText.Add(new Text(".............................."));
            signatureText.SetFontSize(12);
            signatureCell.Add(signatureText);

            Paragraph designationText = new Paragraph();
            designationText.Add(new Text("Designation"));
            designationText.Add(new Text("..........................."));
            designationText.SetFontSize(12);
            designationText.SetMarginTop(50);
            signatureCell.Add(designationText);

            footerTable.AddCell(signatureCell);

            textCell.Add(footerTable);
            contentTable.AddCell(textCell);

            // Add header and content to main cell
            mainCell.Add(headerTable);
            mainCell.Add(contentTable);

            // Add main cell to main table
            mainTable.AddCell(mainCell);

            // Add main table to document
            document.Add(mainTable);
        }

        // Sample data for testing
        private CharacterCertificateModel GetSampleData()
        {
            return new CharacterCertificateModel
            {
                SerialNumber = "00",
                Session = "22-23",
                StudentName = "John Doe",
                FatherName = "Mr. John Doe Sr.",
                YearsKnown = "2",
                MonthsKnown = "6",
                Place = "New Delhi",
                Date = DateTime.Now.ToString("dd/MM/yyyy"),
                Designation = "Principal",
                PhotoPath = null
            };
        }
    }

    // Model class for character certificate data
    public class CharacterCertificateModel
    {
        public string SerialNumber { get; set; }
        public string Session { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string YearsKnown { get; set; }
        public string MonthsKnown { get; set; }
        public string Place { get; set; }
        public string Date { get; set; }
        public string Designation { get; set; }
        public string PhotoPath { get; set; }
    }
}