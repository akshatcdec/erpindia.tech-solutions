using ERPIndia;
using ERPIndia.Class.Helper;
using ERPIndia.Controllers;
using ERPIndia.StudentManagement.Repository;
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
using StudentManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace StudentProfileMVC.Controllers
{
    public class StudentProfileController : BaseController
    {
        // Define constants for styling - smaller sizes for compact layout
        private static readonly Color SECTION_HEADER_BG = new DeviceRgb(80, 80, 80);
        private static readonly Color BORDER_COLOR = ColorConstants.BLACK;
        private static readonly Color LABEL_COLOR = ColorConstants.BLACK;
        private static readonly Color VALUE_COLOR = ColorConstants.BLACK;

        // Define fonts
        private static PdfFont FONT_NORMAL;
        private static PdfFont FONT_BOLD;

        private readonly StudentRepository _repository;

        public StudentProfileController()
        {
            // Get connection string from web.config
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            _repository = new StudentRepository();
        }

            // Helper method to get school code - implement based on your system
       
        // GET: StudentProfile/GenerateAdmissionForm/7
        public async Task<ActionResult> GenerateAdmissionForm(int id, bool inline = true)
        {
            try
            {
                // Get school code from configuration or session
                int schoolCode =Utils.ParseInt(CommonLogic.GetSessionValue(StringConstants.TenantCode)); // You need to implement this based on your system

                // Get student data from database
                var studentData = await _repository.GetStudentAdmissionDataAsync(id, schoolCode);

                if (studentData?.BasicInfo == null)
                {
                    return HttpNotFound("Student not found");
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
                    document.SetMargins(20, 20, 30, 20);

                    // Initialize fonts
                    FONT_NORMAL = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                    FONT_BOLD = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    string code = CommonLogic.GetSessionValue(StringConstants.TenantCode);
                    string img = CommonLogic.GetSessionValue(StringConstants.LogoImg);
                    string logoPath = Server.MapPath(ERPIndia.Class.Helper.AppLogic.GetLogoImage(code, img));
                    string sessionprint = CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint);
                    string StudentPhotoPath = Server.MapPath(studentData.BasicInfo.Photo);
                    string FatherPhotoPath = Server.MapPath(studentData.FamilyInfo.FPhoto);
                    string MotherPhotoPath = Server.MapPath(studentData.FamilyInfo.MPhoto);
                    string guardianPhotoPath = Server.MapPath(studentData.FamilyInfo.GPhoto);
                    string checkedImagePath = Server.MapPath("/Content/login/images/checked.png");
                    string uncheckedImagePath = Server.MapPath("/Content/login/images/unchecked.png");

                    studentData.BasicInfo.AcademicYear = sessionprint;
                    // Add content to PDF
                    AddSchoolHeader(document,logoPath);
                    AddCompactHeader(document);
                    
                    AddCompactPersonalInformation(document, studentData.BasicInfo, StudentPhotoPath);
                    AddCompactSubjectInformation(document, studentData.Subjects, checkedImagePath, uncheckedImagePath);
                    AddCompactParentsInformation(document, studentData.FamilyInfo, FatherPhotoPath, MotherPhotoPath, guardianPhotoPath);
                    AddCompactAddressInformation(document, studentData.FamilyInfo, studentData.BasicInfo.VillageName);
                    AddCompactSiblingInformation(document, studentData.FamilyInfo, studentData.Siblings);
                    AddCompactTransportInformation(document, studentData.BasicInfo, studentData.FamilyInfo);
                    AddCompactHostelInformation(document, studentData.BasicInfo,studentData.FamilyInfo.HostelNo);
                    AddCompactMedicalHistory(document, studentData.OtherInfo);
                    AddCompactPreviousSchoolDetails(document, studentData.OtherInfo);
                    AddCompactEducationDetails(document, studentData.EducationDetails);
                    AddCompactBankingDetails(document, studentData.OtherInfo);
                    AddCompactMiscellaneousDetails(document, studentData.BasicInfo, studentData.OtherInfo);
                    AddSignatureSection(document);

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
                    string fileName = $"AdmissionForm_{id}.pdf";

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
                System.Diagnostics.Debug.WriteLine($"Error generating PDF: {ex.Message}");
                return Content("Error generating PDF: " + ex.Message);
            }
        }
        // School header remains the same
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
            // REMOVED FIXED HEIGHT - let it expand based on content
            // .SetHeight(100); 

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
                    // Empty cell if no logo
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

            // DEBUG: Let's check if the value is actually retrieved
            string debugSessionPrint = CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint);
            System.Diagnostics.Debug.WriteLine($"ActiveSessionPrint value: '{debugSessionPrint}'");

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

            // Session print - FIXED: Changed margin and added debug
            string sessionprint = CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint);

            // Alternative 1: Try without null check to see if it appears
            // Paragraph session = new Paragraph("Session: ( " + (sessionprint ?? "2024-25") + " )")
            //     .SetFont(FONT_BOLD)
            //     .SetFontSize(9)
            //     .SetTextAlignment(TextAlignment.CENTER)
            //     .SetMarginTop(2)
            //     .SetMarginBottom(0);
            // textCell.Add(session);

            // Alternative 2: Current approach with debugging
            if (!string.IsNullOrEmpty(sessionprint))
            {
                Paragraph session = new Paragraph("Session: ( " + sessionprint + " )")
                    .SetFont(FONT_BOLD)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(2)  // Changed from MarginBottom to MarginTop
                    .SetMarginBottom(0);
                textCell.Add(session);

                // Debug log
                System.Diagnostics.Debug.WriteLine($"Session paragraph added with text: 'Session: ( {sessionprint} )'");
            }
            else
            {
                // Debug log when sessionprint is empty
                System.Diagnostics.Debug.WriteLine("Session print is null or empty - not adding session paragraph");

                // Optional: Add a placeholder to confirm positioning
                // Paragraph sessionPlaceholder = new Paragraph("Session: ( Not Available )")
                //     .SetFont(FONT_BOLD)
                //     .SetFontSize(9)
                //     .SetTextAlignment(TextAlignment.CENTER)
                //     .SetMarginTop(2)
                //     .SetMarginBottom(0)
                //     .SetFontColor(ColorConstants.RED);
                // textCell.Add(sessionPlaceholder);
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
        private static void AddCompactHeader(Document document)
        {

            Paragraph header = new Paragraph("ADMISSION FORM")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(10)
                .SetFont(FONT_BOLD)
                .SetMarginBottom(3);

            document.Add(header);
           
        }

        // Helper methods
        private static void AddCompactSectionHeader(Document document, string title)
        {
            Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            Cell headerCell = new Cell()
                .SetBackgroundColor(SECTION_HEADER_BG)
                .SetPadding(5);

            Paragraph headerText = new Paragraph(title)
                .SetFontSize(9)
                .SetFontColor(ColorConstants.WHITE)
                .SetFont(FONT_BOLD);

            headerCell.Add(headerText);
            headerTable.AddCell(headerCell);

            document.Add(headerTable);
        }

        private static Cell CreateCompactFieldCell(string label, string value, int colspan = 1)
        {
            Cell cell = new Cell(1, colspan)
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.1f))
                .SetPadding(5);

            Paragraph content = new Paragraph()
                .SetMultipliedLeading(1.0f);

            Text labelText = new Text(label + ": ")
                .SetFontSize(8)
                .SetFont(FONT_BOLD)
                .SetFontColor(ColorConstants.BLACK);

            Text valueText = new Text(value ?? "")
                .SetFontSize(8)
                .SetFont(FONT_NORMAL)
                .SetFontColor(ColorConstants.BLACK);

            content.Add(labelText).Add(valueText);
            cell.Add(content);

            return cell;
        }

        private static Cell CreatePhotoPlaceholder(string label)
        {
            Cell cell = new Cell()
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetHeight(80)
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph placeholder = new Paragraph(label)
                .SetFontSize(8)
                .SetFont(FONT_NORMAL)
                .SetTextAlignment(TextAlignment.CENTER);

            cell.Add(placeholder);
            return cell;
        }

        // Personal information with data
        private static void AddCompactPersonalInformation(Document document, StudentInfoBasicDto student, string studentPhotoPath = null)
        {
            AddCompactSectionHeader(document, "Personal Information");

            Table mainTable = new Table(UnitValue.CreatePercentArray(new float[] { 20, 80 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            // Create photo cell with actual student photo
            Cell photoCell = CreateStudentPhotoCell(studentPhotoPath);
            mainTable.AddCell(photoCell);

            Cell detailsCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0);

            Table detailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f }))
                .SetWidth(UnitValue.CreatePercentValue(100));

            // First row
            detailsTable.AddCell(CreateCompactFieldCell("Admission No", student.AdmsnNo.ToString()));
            detailsTable.AddCell(CreateCompactFieldCell("Roll No", student.RollNo));
            detailsTable.AddCell(CreateCompactFieldCell("SR No.", student.SrNo));

            // Second row
            detailsTable.AddCell(CreateCompactFieldCell("Class", student.ClassName ?? student.Class));
            detailsTable.AddCell(CreateCompactFieldCell("Section", student.SectionName ?? student.Section));
            detailsTable.AddCell(CreateCompactFieldCell("Name", student.FirstName));

            // Third row
            detailsTable.AddCell(CreateCompactFieldCell("Gender", student.Gender));
            detailsTable.AddCell(CreateCompactFieldCell("Date of Birth",
            student.DOB.HasValue ? student.DOB.Value.ToString("dd/MM/yyyy") : ""));
            detailsTable.AddCell(CreateCompactFieldCell("Blood Group", student.BloodGroup));

            // Fourth row
            detailsTable.AddCell(CreateCompactFieldCell("Religion", student.Religion));
            detailsTable.AddCell(CreateCompactFieldCell("Category", student.CategoryName ?? student.Category));
            detailsTable.AddCell(CreateCompactFieldCell("Caste", student.Caste));

            // Fifth row
            detailsTable.AddCell(CreateCompactFieldCell("Admission Date",
            student.AdmsnDate.HasValue ? student.AdmsnDate.Value.ToString("dd/MM/yyyy") : ""));
            detailsTable.AddCell(CreateCompactFieldCell("Student Aadhar", student.AadharNo));
            detailsTable.AddCell(CreateCompactFieldCell("Other Name", student.LastName));

            detailsCell.Add(detailsTable);
            mainTable.AddCell(detailsCell);
            document.Add(mainTable);

            // Additional details
            Table additionalTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            additionalTable.AddCell(CreateCompactFieldCell("Mother Tongue", student.MotherTongue));
            additionalTable.AddCell(CreateCompactFieldCell("Mobile", student.Mobile));
            additionalTable.AddCell(CreateCompactFieldCell("Email", student.Email));
            additionalTable.AddCell(CreateCompactFieldCell("Height", student.Height));
            additionalTable.AddCell(CreateCompactFieldCell("Weight", student.Weight));
            additionalTable.AddCell(CreateCompactFieldCell("Languages Known", student.LanguagesKnown));

            document.Add(additionalTable);
            document.Add(new Paragraph("").SetMarginBottom(3));
        }
        // Subject information with data
        private static Cell CreateStudentPhotoCell(string photoBasePath = null)
        {
            Cell cell = new Cell()
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetHeight(80)
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            try
            {
                string photoPath = null;

               if (!string.IsNullOrEmpty(photoBasePath))
                {
                    photoPath = photoBasePath;
                }

                // Check if photo exists and load it
                if (!string.IsNullOrEmpty(photoPath) && System.IO.File.Exists(photoPath))
                {
                    ImageData imageData = ImageDataFactory.Create(photoPath);
                    Image photoImage = new Image(imageData);

                    // Set image properties to fit within the cell
                    photoImage.SetAutoScale(true);
                    photoImage.SetMaxWidth(70);  // Max width in points
                    photoImage.SetMaxHeight(70); // Max height in points

                    cell.Add(photoImage);
                }
                else
                {
                    // Fallback to placeholder text
                    Paragraph placeholder = new Paragraph("Student Photo")
                        .SetFontSize(8)
                        .SetFont(FONT_NORMAL)
                        .SetTextAlignment(TextAlignment.CENTER);
                    cell.Add(placeholder);
                }
            }
            catch (Exception ex)
            {
                // If image loading fails, use text placeholder
                System.Diagnostics.Debug.WriteLine($"Error loading student photo: {ex.Message}");
                Paragraph placeholder = new Paragraph("Student Photo")
                    .SetFontSize(8)
                    .SetFont(FONT_NORMAL)
                    .SetTextAlignment(TextAlignment.CENTER);
                cell.Add(placeholder);
            }

            return cell;
        }
        private static void AddCompactSubjectInformation(Document document, List<StudentInfoSubjectDto> subjects, string checkedImagePath = null, string uncheckedImagePath = null)
        {
            AddCompactSectionHeader(document, "Subject Information");

            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetMargin(0) // Remove table margin
                .SetPadding(0); // Remove table padding

            // Add ALL subjects (both selected and unselected)
            foreach (var subject in subjects)
            {
                table.AddCell(CreateSubjectCell(subject.Name, subject.IsSelected, checkedImagePath, uncheckedImagePath));
            }

            // Add empty cells if needed to complete the row
            int totalSubjects = subjects.Count;
            int remainingCells = 3 - (totalSubjects % 3);
            if (remainingCells < 3)
            {
                for (int i = 0; i < remainingCells; i++)
                {
                    Cell emptyCell = new Cell()
                        .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                        .SetHeight(24) // Match the compact height
                        .SetPadding(0);
                    table.AddCell(emptyCell);
                }
            }

            document.Add(table);
            document.Add(new Paragraph("").SetMarginBottom(2)); // Reduced from 3 to 2
        }
        private static Cell CreateSubjectCell(string subjectName, bool isSelected, string checkedImagePath, string uncheckedImagePath = null)
        {
            Cell cell = new Cell()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetPadding(4)
                .SetHeight(28)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Table layoutTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(Border.NO_BORDER);

            // Create checkbox cell with border
            Cell checkboxCell = new Cell()
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 1f)) // Add border around checkbox
                .SetWidth(14)
                .SetHeight(14)
                .SetPadding(1) // Small padding for better image placement
                .SetMargin(1)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetBackgroundColor(ColorConstants.WHITE); // White background

            if (isSelected)
            {
                try
                {
                    // Use the passed checked image path
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
                }
                catch (Exception ex)
                {
                    // Error handling - fallback to bordered green square
                    System.Diagnostics.Debug.WriteLine($"Error loading checkbox image: {ex.Message}");
                    checkboxCell.SetBackgroundColor(new DeviceRgb(0, 200, 0)); // Green background, border already set
                    checkboxCell.Add(new Paragraph("").SetMargin(0).SetPadding(0));
                }
            }
            else
            {
                try
                {
                    // For unselected, use unchecked image if provided
                    if (!string.IsNullOrEmpty(uncheckedImagePath) && System.IO.File.Exists(uncheckedImagePath))
                    {
                        // Use unchecked image if available
                        ImageData imageData = ImageDataFactory.Create(uncheckedImagePath);
                        Image uncheckboxImage = new Image(imageData);

                        uncheckboxImage.SetAutoScale(true);
                        uncheckboxImage.SetMaxWidth(10);  // Reduced to fit within border
                        uncheckboxImage.SetMaxHeight(10); // Reduced to fit within border
                        uncheckboxImage.SetHorizontalAlignment(HorizontalAlignment.CENTER);

                        checkboxCell.Add(uncheckboxImage);
                    }
                    else
                    {
                        // Fallback: Empty bordered checkbox (border already set above)
                        checkboxCell.Add(new Paragraph("").SetMargin(0).SetPadding(0));
                    }
                }
                catch (Exception ex)
                {
                    // Fallback for unselected - empty bordered checkbox (border already set above)
                    System.Diagnostics.Debug.WriteLine($"Error loading unchecked image: {ex.Message}");
                    checkboxCell.Add(new Paragraph("").SetMargin(0).SetPadding(0));
                }
            }

            // Subject name cell
            Cell subjectCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPaddingLeft(6)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            Paragraph subjectPara = new Paragraph(subjectName ?? "")
                .SetFont(FONT_NORMAL)
                .SetFontSize(9)
                .SetFontColor(ColorConstants.BLACK)
                .SetMargin(0);

            subjectCell.Add(subjectPara);

            layoutTable.AddCell(checkboxCell);
            layoutTable.AddCell(subjectCell);

            cell.Add(layoutTable);
            return cell;
        }
        private static void AddCompactParentsInformation(Document document, StudentInfoFamilyDto family, string fatherPhotoPath = null, string motherPhotoPath = null, string guardianPhotoPath = null)
        {
            // Father's Info
            Table fatherTable = new Table(UnitValue.CreatePercentArray(new float[] { 20, 80 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetKeepTogether(true);

            Cell fatherHeader = new Cell(1, 2)
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.1f))
                .SetBackgroundColor(SECTION_HEADER_BG)
                .SetPadding(5);

            Paragraph fatherTitle = new Paragraph("Father's Info")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetFontColor(ColorConstants.WHITE);

            fatherHeader.Add(fatherTitle);
            fatherTable.AddHeaderCell(fatherHeader);

            // Use the new photo cell method for father
            Cell fatherPhotoCell = CreatePersonPhotoCell(fatherPhotoPath, "Father Photo", 100);
            fatherTable.AddCell(fatherPhotoCell);

            Cell fatherDetailsCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0);

            Table fatherDetailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f }))
                .SetWidth(UnitValue.CreatePercentValue(100));

            fatherDetailsTable.AddCell(CreateCompactFieldCell("Name", family?.FName));
            fatherDetailsTable.AddCell(CreateCompactFieldCell("Email", family?.FEmail));
            fatherDetailsTable.AddCell(CreateCompactFieldCell("Phone", family?.FPhone));
            fatherDetailsTable.AddCell(CreateCompactFieldCell("Occupation", family?.FOccupation));
            fatherDetailsTable.AddCell(CreateCompactFieldCell("Education", family?.FEducation));
            fatherDetailsTable.AddCell(CreateCompactFieldCell("Aadhar", family?.FAadhar));

            Cell fatherNoteCell = CreateCompactFieldCell("Father Note", family?.FNote, 3);
            fatherDetailsTable.AddCell(fatherNoteCell);

            fatherDetailsCell.Add(fatherDetailsTable);
            fatherTable.AddCell(fatherDetailsCell);

            document.Add(fatherTable);
            document.Add(new Paragraph("").SetMarginBottom(2));

            // Mother's Info
            Table motherTable = new Table(UnitValue.CreatePercentArray(new float[] { 20, 80 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetKeepTogether(true);

            Cell motherHeader = new Cell(1, 2)
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.1f))
                .SetBackgroundColor(SECTION_HEADER_BG)
                .SetPadding(5);

            Paragraph motherTitle = new Paragraph("Mother's Info")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetFontColor(ColorConstants.WHITE);

            motherHeader.Add(motherTitle);
            motherTable.AddHeaderCell(motherHeader);

            // Use the new photo cell method for mother
            Cell motherPhotoCell = CreatePersonPhotoCell(motherPhotoPath, "Mother Photo", 100);
            motherTable.AddCell(motherPhotoCell);

            Cell motherDetailsCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0);

            Table motherDetailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f }))
                .SetWidth(UnitValue.CreatePercentValue(100));

            motherDetailsTable.AddCell(CreateCompactFieldCell("Name", family?.MName));
            motherDetailsTable.AddCell(CreateCompactFieldCell("Email", family?.MEmail));
            motherDetailsTable.AddCell(CreateCompactFieldCell("Phone", family?.MPhone));
            motherDetailsTable.AddCell(CreateCompactFieldCell("Occupation", family?.MOccupation));
            motherDetailsTable.AddCell(CreateCompactFieldCell("Education", family?.MEducation));
            motherDetailsTable.AddCell(CreateCompactFieldCell("Aadhar", family?.MAadhar));

            Cell motherNoteCell = CreateCompactFieldCell("Mother Note", family?.MNote, 3);
            motherDetailsTable.AddCell(motherNoteCell);

            motherDetailsCell.Add(motherDetailsTable);
            motherTable.AddCell(motherDetailsCell);

            document.Add(motherTable);
            document.Add(new Paragraph("").SetMarginBottom(2));

            // Guardian's Info
            Table guardianTable = new Table(UnitValue.CreatePercentArray(new float[] { 20, 80 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .SetKeepTogether(true);

            Cell guardianHeader = new Cell(1, 2)
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.1f))
                .SetBackgroundColor(SECTION_HEADER_BG)
                .SetPadding(5);

            Paragraph guardianTitle = new Paragraph("Guardian's Info")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetFontColor(ColorConstants.WHITE);

            guardianHeader.Add(guardianTitle);
            guardianTable.AddHeaderCell(guardianHeader);

            // Use the new photo cell method for guardian
            Cell guardianPhotoCell = CreatePersonPhotoCell(guardianPhotoPath, "Guardian Photo", 100);
            guardianTable.AddCell(guardianPhotoCell);

            Cell guardianDetailsCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0);

            Table guardianDetailsTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f }))
                .SetWidth(UnitValue.CreatePercentValue(100));

            guardianDetailsTable.AddCell(CreateCompactFieldCell("Name", family?.GName));
            guardianDetailsTable.AddCell(CreateCompactFieldCell("Relation", family?.GRelation));
            guardianDetailsTable.AddCell(CreateCompactFieldCell("Phone", family?.GPhone));
            guardianDetailsTable.AddCell(CreateCompactFieldCell("Email", family?.GEmail));
            guardianDetailsTable.AddCell(CreateCompactFieldCell("Occupation", family?.GOccupation));
            guardianDetailsTable.AddCell(CreateCompactFieldCell("Education", family?.GEducation));
            Cell guardianNoteCell = CreateCompactFieldCell("Guardian Note", family?.GRemark, 3);
            guardianDetailsTable.AddCell(guardianNoteCell);

            guardianDetailsCell.Add(guardianDetailsTable);
            guardianTable.AddCell(guardianDetailsCell);

            document.Add(guardianTable);
            document.Add(new Paragraph("").SetMarginBottom(3));
        }
        // Alternative approach if the above doesn't center properly
        private static Cell CreatePersonPhotoCell(string photoPath = null, string placeholderText = "Photo", int height = 100)
        {
            Cell cell = new Cell()
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetHeight(height)
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            try
            {
                // Check if photo exists and load it
                if (!string.IsNullOrEmpty(photoPath) && System.IO.File.Exists(photoPath))
                {
                    ImageData imageData = ImageDataFactory.Create(photoPath);
                    Image photoImage = new Image(imageData);

                    // Set image properties to fit within the cell
                    photoImage.SetAutoScale(true);
                    photoImage.SetMaxWidth(height - 20);  // Leave some padding space
                    photoImage.SetMaxHeight(height - 20); // Leave some padding space

                    // Wrap image in a centered paragraph
                    Paragraph imageParagraph = new Paragraph()
                        .Add(photoImage)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMargin(0)
                        .SetPadding(0);

                    cell.Add(imageParagraph);
                }
                else
                {
                    // Fallback to placeholder text
                    Paragraph placeholder = new Paragraph(placeholderText)
                        .SetFontSize(8)
                        .SetFont(FONT_NORMAL)
                        .SetTextAlignment(TextAlignment.CENTER);
                    cell.Add(placeholder);
                }
            }
            catch (Exception ex)
            {
                // If image loading fails, use text placeholder
                System.Diagnostics.Debug.WriteLine($"Error loading photo ({placeholderText}): {ex.Message}");
                Paragraph placeholder = new Paragraph(placeholderText)
                    .SetFontSize(8)
                    .SetFont(FONT_NORMAL)
                    .SetTextAlignment(TextAlignment.CENTER);
                cell.Add(placeholder);
            }

            return cell;
        }
        private static void AddCompactAddressInformation(Document document, StudentInfoFamilyDto family,string Name)
        {
            AddCompactSectionHeader(document, "Address");

            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            table.AddCell(CreateCompactFieldCell("Address", family?.StCurrentAddress));
            table.AddCell(CreateCompactFieldCell("Village/Town/Dist", Name));

            document.Add(table);
            document.Add(new Paragraph("").SetMarginBottom(3));
        }

        // Sibling information with data
        private static void AddCompactSiblingInformation(Document document, StudentInfoFamilyDto family, List<StudentInfoSiblingDto> siblings)
        {
            AddCompactSectionHeader(document, "Siblings");

            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            string siblingStatus = siblings != null && siblings.Count > 0 == true ? "Yes" : "No";
            table.AddCell(CreateCompactFieldCell("Is Sibling in Same School", siblingStatus));

            // Add sibling details if any
            if (siblings != null && siblings.Count > 0)
            {
                StringBuilder siblingInfo = new StringBuilder();
                foreach (var sibling in siblings)
                {
                    if (siblingInfo.Length > 0) siblingInfo.Append(", ");
                    siblingInfo.Append($"{sibling.Name} (Class: {sibling.Class}, Roll: {sibling.RollNo})");
                }
                table.AddCell(CreateCompactFieldCell("Sibling Details", siblingInfo.ToString()));
            }
            else
            {
                table.AddCell(CreateCompactFieldCell("Sibling Details", "None"));
            }

            document.Add(table);
            document.Add(new Paragraph("").SetMarginBottom(3));
        }

        // Transport information with data
        private static void AddCompactTransportInformation(Document document, StudentInfoBasicDto student, StudentInfoFamilyDto family)
        {
            // Check if there's any transport information to display
            bool hasTransportData = !string.IsNullOrEmpty(student.RouteName) ||
                                   (student.RouteId.HasValue && student.RouteId.Value != Guid.Empty) ||
                                   !string.IsNullOrEmpty(student.PickupName) ||
                                   !string.IsNullOrEmpty(student.PickupPoint) ||
                                   !string.IsNullOrEmpty(student.VechileName) ||
                                   !string.IsNullOrEmpty(family?.VehicleNumber);

            // Only show the section if there's actual transport data
            if (!hasTransportData)
            {
                return; // Don't add the section at all if no transport data
            }

            AddCompactSectionHeader(document, "Transport Information");

            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            table.AddCell(CreateCompactFieldCell("Route", student.RouteName ?? student.RouteId?.ToString()));
            table.AddCell(CreateCompactFieldCell("Pickup Point", student.PickupName ?? student.PickupPoint));
            table.AddCell(CreateCompactFieldCell("Vehicle Number", student.VechileName ?? family?.VehicleNumber));

            document.Add(table);
            document.Add(new Paragraph("").SetMarginBottom(3));
        }

        // Hostel information with data - only show if there's hostel data
        private static void AddCompactHostelInformation(Document document, StudentInfoBasicDto student,string HostelNo)
        {
            // Check if there's any hostel information to display
            bool hasHostelData = !string.IsNullOrEmpty(student.HostelName) ||
                                (student.HostelId.HasValue && student.HostelId.Value != Guid.Empty);

            // Only show the section if there's actual hostel data
            if (!hasHostelData)
            {
                return; // Don't add the section at all if no hostel data
            }

            AddCompactSectionHeader(document, "Hostel Information");

            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            table.AddCell(CreateCompactFieldCell("Hostel", student.HostelName ?? student.HostelId?.ToString()));
            table.AddCell(CreateCompactFieldCell("Room Number", HostelNo)); // Add room number field if available in future

            document.Add(table);
            document.Add(new Paragraph("").SetMarginBottom(3));
        }
        // Medical history with data
        private static void AddCompactMedicalHistory(Document document, StudentInfoOtherDto other)
        {
            AddCompactSectionHeader(document, "Medical History");

            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 50.00f, 50.00f}))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            table.AddCell(CreateCompactFieldCell("Allergies", other?.Allergies));
            table.AddCell(CreateCompactFieldCell("Medications", other?.Medications));

            document.Add(table);
            document.Add(new Paragraph("").SetMarginBottom(3));
        }

        // Previous school details with data
        private static void AddCompactPreviousSchoolDetails(Document document, StudentInfoOtherDto other)
        {
            AddCompactSectionHeader(document, "Previous School Details");

            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            table.AddCell(CreateCompactFieldCell("School Name", other?.PreviousSchoolDtl));
            table.AddCell(CreateCompactFieldCell("Address", other?.PreviousSchoolAddress));
            table.AddCell(CreateCompactFieldCell("UDISE Code", other?.UdiseCode));
            table.AddCell(CreateCompactFieldCell("School Note", other?.SchoolNote));

            document.Add(table);
            document.Add(new Paragraph("").SetMarginBottom(3));
        }

        // Education details with data
        // Education details with data - only show if records exist
        private static void AddCompactEducationDetails(Document document, List<StudentInfoEduDetailDto> educationDetails)
        {
            // Only proceed if there are education details to show
            if (educationDetails == null || educationDetails.Count == 0)
            {
                return; // Don't add the section at all if no data
            }

            AddCompactSectionHeader(document, "Education Details");

            // Updated to 7 columns to include Roll No
            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 16, 14, 10, 14, 16, 16, 14 }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f));

            Color headerColor = new DeviceRgb(80, 80, 80);

            // Header cells - added "Roll No"
            string[] headers = { "Examination", "Board", "Year", "Roll No", "Max. Mark", "Obtain Mark", "Percentage" };
            foreach (var header in headers)
            {
                Cell headerCell = new Cell()
                    .SetBackgroundColor(headerColor)
                    .SetPadding(5)
                    .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f));

                headerCell.Add(new Paragraph(header)
                    .SetFont(FONT_BOLD)
                    .SetFontSize(8)
                    .SetFontColor(ColorConstants.WHITE));

                table.AddHeaderCell(headerCell);
            }

            // Add education details rows - added Roll No data
            foreach (var edu in educationDetails)
            {
                table.AddCell(CreateEducationCell(edu.Class));
                table.AddCell(CreateEducationCell(edu.Board));
                table.AddCell(CreateEducationCell(edu.PassingYear));
                table.AddCell(CreateEducationCell(edu.RollNo)); // Added Roll No
                table.AddCell(CreateEducationCell(edu.MaximumMarks?.ToString("0.##")));
                table.AddCell(CreateEducationCell(edu.ObtainedMarks?.ToString("0.##")));
                table.AddCell(CreateEducationCell(edu.Percentage?.ToString("0.##") + "%"));
            }

            document.Add(table);
            document.Add(new Paragraph("").SetMarginBottom(3));
        }
        private static Cell CreateEducationCell(string value)
        {
            return new Cell()
                .SetPadding(5)
                .SetHeight(20)
                .SetBorder(new SolidBorder(ColorConstants.BLACK, 0.5f))
                .Add(new Paragraph(value ?? "")
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(8));
        }
        // Banking details with data
        private static void AddCompactBankingDetails(Document document, StudentInfoOtherDto other)
        {
            AddCompactSectionHeader(document, "Banking & Other Details");

            // Create table with 2 columns for better layout
            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 50f, 50f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            // Row 1: Bank Name and Branch
            table.AddCell(CreateCompactFieldCell("Bank Name", other?.BankName));
            table.AddCell(CreateCompactFieldCell("Branch", other?.BankBranch));

            // Row 2: Account Number and IFSC Code
            table.AddCell(CreateCompactFieldCell("Account Number", other?.BankAcNo));
            table.AddCell(CreateCompactFieldCell("IFSC Code", other?.IfscCode));

            // Row 3: SSSM ID and Family ID
            table.AddCell(CreateCompactFieldCell("SSSM ID", other?.NADID));
            table.AddCell(CreateCompactFieldCell("Family Id", other?.IDentityLocal));

            // Row 4: Identity Other spanning 2 columns
            Cell identityOtherCell = new Cell(1, 2) // 1 row, 2 columns span
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(3);

            Paragraph identityLabel = new Paragraph()
                .Add(new Text("Identity Other: ").SetFont(FONT_BOLD).SetFontSize(8))
                .Add(new Text(other?.IdentityOther ?? "").SetFont(FONT_NORMAL).SetFontSize(8))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetMargin(0);

            identityOtherCell.Add(identityLabel);
            table.AddCell(identityOtherCell);

            // Row 5: Additional Information spanning 2 columns
            Cell additionalInfoCell = new Cell(1, 2) // 1 row, 2 columns span
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(3)
                .SetBackgroundColor(new DeviceRgb(248, 249, 250)); // Very light gray background

            Paragraph additionalInfoParagraph = new Paragraph()
                .Add(new Text("Additional Information: ").SetFont(FONT_BOLD).SetFontSize(8))
                .Add(new Text(other?.OtherInformation ?? "").SetFont(FONT_NORMAL).SetFontSize(8))
                .SetTextAlignment(TextAlignment.LEFT)
                .SetMargin(0);

            additionalInfoCell.Add(additionalInfoParagraph);
            table.AddCell(additionalInfoCell);

            document.Add(table);
            document.Add(new Paragraph("").SetMarginBottom(3));
        }
        // Miscellaneous details with data
        private static void AddCompactMiscellaneousDetails(Document document, StudentInfoBasicDto student, StudentInfoOtherDto other)
        {
            AddCompactSectionHeader(document, "Miscellaneous Details");

            Table table = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            table.AddCell(CreateCompactFieldCell("PEN Number", student.PENNo));
            table.AddCell(CreateCompactFieldCell("Old Balance", student.OldBalance?.ToString()));
            table.AddCell(CreateCompactFieldCell("Student Login ID", student.SrNo));
            table.AddCell(CreateCompactFieldCell("Student Login Password", student.Password));
            document.Add(table);
            document.Add(new Paragraph("").SetMarginBottom(3));
        }

        // Signature section remains the same
        private static void AddSignatureSection(Document document)
        {
            Table signatureTable = new Table(UnitValue.CreatePercentArray(new float[] { 33.33f, 33.33f, 33.33f }))
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetMarginTop(20);

            // Student Cell - Left Aligned
            Cell studentCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.LEFT);
            Paragraph studentLine = new Paragraph("_______________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.LEFT);
            Paragraph studentLabel = new Paragraph("Student Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.LEFT);
            studentCell.Add(studentLine);
            studentCell.Add(studentLabel);

            // Parent Cell - Center Aligned
            Cell parentCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.CENTER);
            Paragraph parentLine = new Paragraph("_______________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER);
            Paragraph parentLabel = new Paragraph("Parent Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER);
            parentCell.Add(parentLine);
            parentCell.Add(parentLabel);

            // Principal Cell - Right Aligned
            Cell principalCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.RIGHT);
            Paragraph principalLine = new Paragraph("___________________________")
                .SetFont(FONT_NORMAL)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.RIGHT);
            Paragraph principalLabel = new Paragraph("Principal Signature with Stamp")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.RIGHT);
            principalCell.Add(principalLine);
            principalCell.Add(principalLabel);

            signatureTable.AddCell(studentCell);
            signatureTable.AddCell(parentCell);
            signatureTable.AddCell(principalCell);

            document.Add(signatureTable);
        }
    }

    // Footer event handler remains the same
    // Footer event handler with corrected date and time formats
    public class FooterEventHandler : IEventHandler
    {
        public void HandleEvent(Event @event)
        {
            PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
            PdfDocument pdf = docEvent.GetDocument();
            PdfPage page = docEvent.GetPage();

            Rectangle pageSize = page.GetPageSize();
            PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);

            canvas.SaveState()
                .SetStrokeColor(ColorConstants.LIGHT_GRAY)
                .MoveTo(pageSize.GetLeft() + 20, pageSize.GetBottom() + 20)
                .LineTo(pageSize.GetRight() - 20, pageSize.GetBottom() + 20)
                .Stroke()
                .RestoreState();

            // Format: DD-MM-YYYY and 12-hour time with AM/PM
            string formattedDateTime = DateTime.Now.ToString("dd-MM-yyyy hh:mm tt");

            canvas.BeginText()
                .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 8)
                .MoveText(pageSize.GetLeft() + 20, pageSize.GetBottom() + 10)
                .ShowText("Printed on: " + formattedDateTime)
                .EndText();

            canvas.BeginText()
                .SetFontAndSize(PdfFontFactory.CreateFont(StandardFonts.HELVETICA), 8)
                .MoveText(pageSize.GetRight() - 80, pageSize.GetBottom() + 10)
                .ShowText("Page " + pdf.GetPageNumber(page) + " of " + pdf.GetNumberOfPages())
                .EndText();

            canvas.Release();
        }
    }


}