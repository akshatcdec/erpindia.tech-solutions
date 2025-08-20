using Dapper;
using ERPIndia.Class.Helper;
using ERPIndia.StudentManagement.Repository;
using ERPK12Models.DTO;
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
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ERPIndia.Controllers.Examination
{
    public interface IExaminationRepository
    {
        Task<List<ExamDetails>> GetExamDetailsAsync(Guid examId,Guid? classId,Guid? SectionId, Guid sessionId, int tenantCode);
        Task<List<ExamDetails>> GetExamFullDetailsAsync(Guid examId, Guid? classId, Guid? SectionId, Guid sessionId, int tenantCode);
        Task<List<StudentExamInfo>> GetExamStudentsAsync(Guid examId,Guid? classId, Guid? SectionId, Guid sessionId, int tenantCode);
    }

    public class ExaminationRepository : IExaminationRepository
    {
        public async Task<List<ExamDetails>> GetExamFullDetailsAsync(Guid examId, Guid? classId, Guid? sectionId, Guid sessionId, int tenantCode)
        {
            // Convert empty GUID to null
            if (sectionId.HasValue && sectionId.Value == Guid.Empty)
                sectionId = null;
            if (classId.HasValue && classId.Value == Guid.Empty)
                classId = null;

            var query = @"
        SELECT 
            es.SubjectID,
            em.ExamName,
            em.AdmitCard, 
            es.ExamDate, 
            es.ExamTime,
            asm.SubjectName,
            t.admitnote1,
            t.admitnote2,
            t.admitnote3,
            t.admitnote4,
            t.admitnote5,
            -- Student Information
            s.StudentId,
            s.FirstName AS StudentName,
            s.MotherName,
            s.FatherName,
            s.ClassName,
            s.SectionName AS Section,
            s.RollNo AS RollNumber,
            CAST(s.AdmsnNo AS NVARCHAR(50)) AS AdmissionNumber,
            s.Photo AS PhotoPath
        FROM dbo.ExamMaster em
        LEFT JOIN dbo.ExamSchedule es ON es.ExamID = em.ExamID 
            AND es.TenantCode = @TenantCode
            AND es.SessionID = @SessionID 
            AND es.ClassID = @ClassID 
            AND (@SectionID IS NULL OR es.SectionID = @SectionID)
            AND es.IsDeleted = 0
        LEFT JOIN dbo.AcademicSubjectMaster asm ON asm.SubjectID = es.SubjectID
        LEFT JOIN dbo.Tenants t ON t.TenantID = em.TenantID
        -- Join with Student table
        CROSS JOIN dbo.StudentInfoBasic s
        WHERE em.ExamID = @ExamID
            AND em.TenantCode = @TenantCode
            AND em.SessionID = @SessionID
            AND em.IsDeleted = 0
            -- Student filters
            AND s.TenantCode = @TenantCode
            AND s.SessionID = @SessionID
            AND s.IsActive = 1
            AND s.IsDeleted = 0
            AND (@ClassID IS NULL OR s.ClassId = @ClassID)
            AND (@SectionID IS NULL OR s.SectionId = @SectionID)
        ORDER BY s.RollNo, es.ExamDate";

            var parameters = new
            {
                ExamID = examId,
                ClassID = classId,
                SectionID = sectionId,
                SessionID = sessionId,
                TenantCode = tenantCode
            };

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                var result = await connection.QueryAsync<ExamDetails>(query, parameters);
                return result.ToList();
            }
        }
        public async Task<List<ExamDetails>> GetExamDetailsAsync(Guid examId, Guid? classId, Guid? sectionId, Guid sessionId, int tenantCode)
        {
            // Convert empty GUID to null
            if (sectionId.HasValue && sectionId.Value == Guid.Empty)
                sectionId = null;

            var query = @"
        SELECT 
            es.SubjectID,
            em.ExamName,
            em.AdmitCard, 
            es.ExamDate, 
            es.ExamTime,
            asm.SubjectName,
            t.admitnote1,
            t.admitnote2,
            t.admitnote3,
            t.admitnote4,
            t.admitnote5
        FROM dbo.ExamMaster em
        LEFT JOIN dbo.ExamSchedule es ON es.ExamID = em.ExamID 
            AND es.TenantCode = @TenantCode
            AND es.SessionID = @SessionID 
            AND es.ClassID = @ClassID 
            AND (@SectionID IS NULL OR es.SectionID = @SectionID)
            AND es.IsDeleted = 0
        LEFT JOIN dbo.AcademicSubjectMaster asm ON asm.SubjectID = es.SubjectID
        LEFT JOIN dbo.Tenants t ON t.TenantID = em.TenantID
        WHERE em.ExamID = @ExamID
            AND em.TenantCode = @TenantCode
            AND em.SessionID = @SessionID
            AND em.IsDeleted = 0 
            order by es.ExamDate ";

            var parameters = new
            {
                ExamID = examId,
                ClassID = classId,
                SectionID = sectionId,
                SessionID = sessionId,
                TenantCode = tenantCode
            };

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                var result = await connection.QueryAsync<ExamDetails>(query, parameters);
                return result.ToList();
            }
        }
        public async Task<List<StudentExamInfo>> GetExamStudentsAsync(Guid examId, Guid? classId, Guid? sectionId, Guid sessionId, int tenantCode)
        {
            // Convert empty GUIDs to null
            if (classId.HasValue && classId.Value == Guid.Empty)
                classId = null;

            if (sectionId.HasValue && sectionId.Value == Guid.Empty)
                sectionId = null;

            var sql = @"
SELECT
    s.StudentId,
    s.FirstName           AS StudentName,
    s.MotherName,
    s.FatherName,
    s.ClassName,
    s.SectionName         AS Section,
    s.RollNo              AS RollNumber,
    CAST(s.AdmsnNo AS NVARCHAR(50)) AS AdmissionNumber,
    s.Photo               AS PhotoPath,
    t.admitnote1,
    t.admitnote2,
    t.admitnote3,
    t.admitnote4,
    t.admitnote5
FROM dbo.StudentInfoBasic AS s
LEFT JOIN dbo.Tenants AS t
       ON s.TenantCode = t.TenantCode
WHERE s.TenantCode = @TenantCode
  AND s.SessionID  = @SessionId
  AND s.IsActive   = 1
  AND s.IsDeleted  = 0
  AND (@ClassId   IS NULL OR s.ClassId   = @ClassId)
  AND (@SectionId IS NULL OR s.SectionId = @SectionId)
ORDER BY s.RollNo;";
            

            var parameters = new
            {
                ExamId = examId,
                ClassId = classId,
                SectionId = sectionId,
                SessionId = sessionId,
                TenantCode = tenantCode
            };

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                var students = await connection.QueryAsync<StudentExamInfo>(sql, parameters);
                return students.ToList();
            }
        }
    }

    public class AdmCardController : BaseController
    {
        private readonly IExaminationRepository _examinationRepository;
        private readonly StudentRepository _studentRepository;

        // Font declarations
        private PdfFont FONT_NORMAL;
        private PdfFont FONT_BOLD;

        // Colors
        private static readonly Color RED_COLOR = new DeviceRgb(255, 0, 0);
        private static readonly Color GREEN_COLOR = new DeviceRgb(0, 128, 0);
        private static readonly Color PURPLE_COLOR = new DeviceRgb(128, 0, 128);
        private static readonly Color YELLOW_COLOR = new DeviceRgb(255, 255, 0);
        private static readonly Color BLACK_COLOR = ColorConstants.BLACK;
        private static readonly Color GRAY_COLOR = new DeviceRgb(240, 240, 240);
        public AdmCardController()
        {
            _examinationRepository = new ExaminationRepository();
            _studentRepository = new StudentRepository();
        }

        public async Task<ActionResult> Generate(Guid? examId, Guid? classId = null, Guid? sectionId = null,bool inline = true)
        {
            var sessionId = CurrentSessionID;
            var tenantCode = TenantCode;
            var currentUserId = CurrentTenantUserID;

            try
            {
                var examDetails = await _examinationRepository.GetExamDetailsAsync(examId ?? Guid.Empty, classId, sectionId, sessionId, tenantCode);
                var students =await _examinationRepository.GetExamStudentsAsync(examId ?? Guid.Empty, classId, sectionId, sessionId, tenantCode);

                if (!students.Any())
                    return Content("Error: No students found for this exam");

                using (var stream = new MemoryStream())
                {
                    using (var writer = new PdfWriter(stream))
                    {
                        writer.SetCloseStream(false);
                        using (var pdf = new PdfDocument(writer))
                        {
                            pdf.SetDefaultPageSize(PageSize.A4);
                            using (var document = new Document(pdf))
                            {
                                document.SetMargins(3, 3, 3, 5);
                                FONT_NORMAL = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                                FONT_BOLD = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                                string code = CommonLogic.GetSessionValue(StringConstants.TenantCode);
                                string img = CommonLogic.GetSessionValue(StringConstants.LogoImg);
                                string logoPath = Server.MapPath(AppLogic.GetLogoImage(code, img));

                                float pageHeight = pdf.GetDefaultPageSize().GetHeight();
                                float topMargin = document.GetTopMargin();
                                float bottomMargin = document.GetBottomMargin();
                                float halfHeight = (pageHeight - topMargin - bottomMargin) / 2 - 5f;

                                for (int i = 0; i < students.Count; i += 2)
                                {
                                    if (i > 0)
                                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                                    AddImprovedAdmitCard(document, students[i], examDetails, logoPath, halfHeight);

                                    if (i + 1 < students.Count)
                                    {
                                        AddCuttingLine(document);
                                        AddImprovedAdmitCard(document, students[i + 1], examDetails, logoPath, halfHeight);
                                    }
                                }
                            }
                        }
                    }

                    stream.Position = 0;
                    string fileName = $"AdmitCards_{examDetails}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                    Response.AddHeader("Content-Disposition", (inline ? "inline" : "attachment") + $"; filename={fileName}");
                    return File(stream.ToArray(), "application/pdf");
                }
            }
            catch (Exception ex)
            {
                return Content("Error generating admit cards: " + ex.Message);
            }
        }

        private void AddImprovedAdmitCard(Document document,
                                  StudentExamInfo student,
                                  List<ExamDetails> examDetails,
                                  string logoPath,
                                  float cardHeight)
        {
            // Full-width container, fixed half-page height
            Table mainContainer = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .UseAllAvailableWidth()
                //.SetHeight(cardHeight)
                .SetBorder(new DoubleBorder(RED_COLOR, 2))
                .SetPadding(2);  // Reduced from 3

            Cell mainCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0);

            AddHeaderSection(mainCell, logoPath, examDetails[0], student);
            AddMainContentSection(mainCell, student, examDetails);
            AddInstructionsSection(mainCell, examDetails[0]);

            mainContainer.AddCell(mainCell);
            document.Add(mainContainer);
        }


        private void AddHeaderSection(Cell container, string logoPath, ExamDetails examDetails, StudentExamInfo student)
        {
            // Header table with logos
            Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 70, 15 }))
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            // Left logo cell - WITH PADDING
            Cell leftLogoCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                .SetPaddingLeft(3)  // Added left padding
                .SetPaddingRight(2); // Added right padding for spacing

            try
            {
                if (!string.IsNullOrEmpty(logoPath) && System.IO.File.Exists(logoPath))
                {
                    ImageData imageData = ImageDataFactory.Create(logoPath);
                    Image logoImage = new Image(imageData);
                    logoImage.SetAutoScale(true);
                    logoImage.SetMaxWidth(35);  // Original size
                    logoImage.SetMaxHeight(25); // Original size
                    leftLogoCell.Add(logoImage);
                }
            }
            catch { }

            // Center text
            Cell centerCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER);

            string schoolName = CommonLogic.GetSessionValue(StringConstants.PrintTitle) ?? "SCHOOL NAME";
            centerCell.Add(new Paragraph(schoolName.ToUpper())
                .SetFont(FONT_BOLD)
                .SetFontSize(12)
                .SetFontColor(RED_COLOR)
                .SetMarginBottom(0.5f));

            centerCell.Add(new Paragraph("English Medium")
                .SetFont(FONT_NORMAL)
                .SetFontSize(10)
                .SetMarginBottom(0.5f));

            string address = CommonLogic.GetSessionValue(StringConstants.Line1) ?? "School Address";
            centerCell.Add(new Paragraph(address)
                .SetFont(FONT_NORMAL)
                .SetFontSize(9)
                .SetFontColor(PURPLE_COLOR)
                .SetMarginBottom(0.5f));

            string email = CommonLogic.GetSessionValue(StringConstants.Line2) ?? "";
            if (!string.IsNullOrEmpty(email))
            {
                centerCell.Add(new Paragraph(email)
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(9)
                    .SetFontColor(PURPLE_COLOR)
                    .SetMarginBottom(0.5f));
            }

            string session = CommonLogic.GetSessionValue(StringConstants.ActiveSessionPrint) ?? "2023-24";
            centerCell.Add(new Paragraph($"SESSION :- ({session})")
                .SetFont(FONT_BOLD)
                .SetFontSize(9)
                .SetPadding(2)
                .SetMarginBottom(0.5f));

            // Right photo cell - RESIZED TO MATCH LOGO SIZE
            Cell rightPhotoCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                .SetPadding(0)
                .SetPaddingRight(3)  // Added right padding
                .SetPaddingLeft(2);  // Added left padding for balance

            // Create photo container div
            Div photoContainer = new Div()
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT)
                .SetMarginRight(0);

            // Create photo border box - MATCHED TO ACTUAL LOGO SIZE
            Table photoBorderBox = new Table(1)
                .SetWidth(75)   // Increased to better match logo
                .SetHeight(75)  // Square container
                .SetBorder(new SolidBorder(0.75f))  // Medium border
                .SetHorizontalAlignment(HorizontalAlignment.RIGHT);

            Cell photoCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetHeight(75)  // Match container
                .SetWidth(75)   // Match container
                .SetPadding(0)  // Remove padding to allow full width
                .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE);

            // Add photo or fallback
            bool photoAdded = false;

            if (!string.IsNullOrEmpty(student.PhotoPath))
            {
                try
                {
                    string fullPhotoPath = Server.MapPath(student.PhotoPath);

                    // Check if student photo exists
                    if (System.IO.File.Exists(fullPhotoPath))
                    {
                        ImageData photoData = ImageDataFactory.Create(fullPhotoPath);
                        Image studentPhoto = new Image(photoData);
                        studentPhoto.SetAutoScale(true)
                            .SetWidth(75)      // Set exact width to fill container
                            .SetHeight(75)     // Set exact height to fill container
                            .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                        photoCell.Add(studentPhoto);
                        photoAdded = true;
                    }
                }
                catch { }
            }

            // If no photo was added, try default image or show text
            if (!photoAdded)
            {
                try
                {
                    string defaultPhotoPath = Server.MapPath(@"/template/assets/img/noimgstu.png");
                    if (System.IO.File.Exists(defaultPhotoPath))
                    {
                        ImageData photoData = ImageDataFactory.Create(defaultPhotoPath);
                        Image defaultPhoto = new Image(photoData);
                        defaultPhoto.SetAutoScale(true)
                            .SetWidth(75)      // Set exact width to fill container
                            .SetHeight(75)     // Set exact height to fill container
                            .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                        photoCell.Add(defaultPhoto);
                    }
                    else
                    {
                        // Fallback to text if no image available
                        photoCell.Add(new Paragraph("PHOTO")
                            .SetFont(FONT_BOLD)
                            .SetFontSize(8)  // Increased font for larger box
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetVerticalAlignment(VerticalAlignment.MIDDLE));
                    }
                }
                catch
                {
                    // Final fallback to text
                    photoCell.Add(new Paragraph("PHOTO")
                        .SetFont(FONT_BOLD)
                        .SetFontSize(6)  // Smaller font for smaller box
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE));
                }
            }

            photoBorderBox.AddCell(photoCell);
            photoContainer.Add(photoBorderBox);
            rightPhotoCell.Add(photoContainer);

            // Add all cells to header table
            headerTable.AddCell(leftLogoCell);
            headerTable.AddCell(centerCell);
            headerTable.AddCell(rightPhotoCell);
            container.Add(headerTable);

            // Add first line - before "Admit Card"
            container.Add(new LineSeparator(new SolidLine(1f)).SetStrokeColor(GRAY_COLOR));
            //container.Add(new Div().SetHeight(1).SetBackgroundColor(ColorConstants.LIGHT_GRAY));

            // Admit Card label
            container.Add(new Paragraph(":: ADMIT CARD ::")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetBackgroundColor(GRAY_COLOR)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(0.5f)
                .SetMarginTop(0.5f));

            // Add second line - after "Admit Card"
            container.Add(new LineSeparator(new SolidLine(1f)).SetStrokeColor(GRAY_COLOR));
            //container.Add(new Div().SetHeight(1).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
        }
        private void AddMainContentSection(Cell container, StudentExamInfo student, List<ExamDetails> examDetails)
        {
            // Change the table structure to 3 columns instead of 4 (removed photo column)
            Table contentTable = new Table(UnitValue.CreatePercentArray(new float[] { 15, 30, 55 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(3)  // Reduced from 5
                .SetKeepTogether(true);

            // Column 1: Labels
            Cell labelsCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPaddingLeft(2).SetPaddingRight(1).SetPaddingTop(0.5f).SetPaddingBottom(0.5f)  // Reduced padding
                .SetVerticalAlignment(VerticalAlignment.TOP);

            AddStudentLabel(labelsCell, "STUDENT'S NAME:");
            AddStudentLabel(labelsCell, "FATHER'S NAME:");
            AddStudentLabel(labelsCell, "MOTHER'S NAME:");
            AddStudentLabel(labelsCell, "CLASS:");
            AddStudentLabel(labelsCell, "SECTION:");
            AddStudentLabel(labelsCell, "ROLL NO.:");
            AddStudentLabel(labelsCell, "ADMISSION NO:");
            contentTable.AddCell(labelsCell);

            // Column 2: Values
            Cell valuesCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPaddingLeft(1).SetPaddingRight(1).SetPaddingTop(0.5f).SetPaddingBottom(0.5f)  // Reduced padding
                .SetVerticalAlignment(VerticalAlignment.TOP);

            AddStudentValue(valuesCell, student.StudentName);
            AddStudentValue(valuesCell, student.FatherName);
            AddStudentValue(valuesCell, student.MotherName);
            AddStudentValue(valuesCell, student.ClassName);
            AddStudentValue(valuesCell, student.Section);
            AddStudentValue(valuesCell, student.RollNumber);
            AddStudentValue(valuesCell, student.AdmissionNumber);
            contentTable.AddCell(valuesCell);

            // Column 3: Exam Schedule (now extended to cover photo area)
            Table scheduleTable = new Table(UnitValue.CreatePercentArray(new float[] { 5, 15, 15, 30, 30 }))
                                      .UseAllAvailableWidth()
                                      .SetBorder(Border.NO_BORDER);

            Cell examCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPaddingLeft(2).SetPaddingRight(2).SetPaddingTop(1).SetPaddingBottom(1); // Reduced padding

            // Check if there are any valid exam details
            bool hasValidExamDetails = examDetails != null && examDetails.Any() &&
                                       examDetails.Any(e => !string.IsNullOrWhiteSpace(e.SubjectName));

            if (hasValidExamDetails)
            {
                // When there are exam details, use header cells
                var titleMergedCell = new Cell(1, 5)
                    .Add(new Paragraph(examDetails.Count > 0 && examDetails[0].ExamName != null
                        ? examDetails[0].AdmitCard
                        : "Annual Examination")
                        .SetFont(FONT_BOLD)
                        .SetFontSize(10)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(new SolidBorder(BLACK_COLOR, 0.5f));
                scheduleTable.AddHeaderCell(titleMergedCell);

                // 5 headers: S.R. | DATE | DAY | TIME | SUBJECT
                scheduleTable.AddHeaderCell(CreateScheduleHeaderCell("S.R."));
                scheduleTable.AddHeaderCell(CreateScheduleHeaderCell("DATE"));
                scheduleTable.AddHeaderCell(CreateScheduleHeaderCell("DAY"));
                scheduleTable.AddHeaderCell(CreateScheduleHeaderCell("TIME"));
                scheduleTable.AddHeaderCell(CreateScheduleHeaderCell("SUBJECT"));
            }
            else
            {
                // When there are no exam details, use regular cells to avoid header/body separator
                var titleMergedCell = new Cell(1, 5)
                    .Add(new Paragraph(examDetails.Count > 0 && examDetails[0].ExamName != null
                        ? examDetails[0].AdmitCard
                        : "Annual Examination")
                        .SetFont(FONT_BOLD)
                        .SetFontSize(10)
                        .SetTextAlignment(TextAlignment.CENTER))
                    .SetBorder(new SolidBorder(BLACK_COLOR, 0.5f));
                scheduleTable.AddCell(titleMergedCell); // Note: AddCell instead of AddHeaderCell

                // Add 5 empty cells for the column headers row
                for (int i = 0; i < 5; i++)
                {
                    scheduleTable.AddCell(CreateEmptyScheduleCell());
                }
            }

            int sr = 1;
            var subjects = examDetails
                           .Where(e => !string.IsNullOrWhiteSpace(e.SubjectName))
                           .OrderBy(e => e.ExamDate)
                           .ToList();

            // Add actual subject rows
            foreach (var subject in subjects)
            {
                scheduleTable.AddCell(CreateScheduleCell(sr.ToString()));
                scheduleTable.AddCell(CreateScheduleCell(subject.ExamDate.HasValue
                    ? subject.ExamDate.Value.ToString("dd/MM/yyyy")
                    : ""));
                scheduleTable.AddCell(CreateScheduleCell(subject.ExamDate.HasValue
                    ? subject.ExamDate.Value.DayOfWeek.ToString()
                    : ""));
                scheduleTable.AddCell(CreateScheduleCell(subject.ExamTime ?? ""));
                scheduleTable.AddCell(CreateScheduleCell(subject.SubjectName ?? ""));
                sr++;
            }

            // Fill up to 12 rows with empty cells (with white/invisible borders)
            const int maxRows = 12;
            for (int i = sr; i <= maxRows; i++)
            {
                // Add empty cells with white borders
                scheduleTable.AddCell(CreateEmptyScheduleCell());  // S.R.
                scheduleTable.AddCell(CreateEmptyScheduleCell());  // DATE
                scheduleTable.AddCell(CreateEmptyScheduleCell());  // DAY
                scheduleTable.AddCell(CreateEmptyScheduleCell());  // TIME
                scheduleTable.AddCell(CreateEmptyScheduleCell());  // SUBJECT
            }

            examCell.Add(scheduleTable);
            contentTable.AddCell(examCell);

            container.Add(contentTable);
        }
        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }
        private Cell CreateEmptyScheduleCell()
        {
            return new Cell()
                .SetBorder(new SolidBorder(ColorConstants.WHITE, 0.5f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(1)  // Reduced padding
                .Add(new Paragraph("\n")  // Use newline to maintain height
                    .SetFont(FONT_NORMAL)  // or FONT_NORMAL if that's what you use
                    .SetFontSize(7)  // Reduced font size
                    .SetMargin(0));
        }
        private Cell CreateEmptyHeaderCell()
        {
            return new Cell()
                 .SetBorder(new SolidBorder(ColorConstants.WHITE, 0.5f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(1)  // Reduced padding
                .Add(new Paragraph("\n")  // Use newline to maintain height
                    .SetFont(FONT_BOLD)
                    .SetFontSize(10) // Reduced font size
                    .SetMargin(0));
        }
        // Helper method to add student labels
        private void AddStudentLabel(Cell container, string label)
        {
            Paragraph labelPara = new Paragraph(label)
                .SetFont(FONT_BOLD)
                .SetFontSize(9)  // Reduced from 8
                .SetMarginBottom(3)  // Reduced from 4
                .SetMarginTop(0.5f);  // Reduced from 1
            container.Add(labelPara);
        }

        // Helper method to add student values
        private void AddStudentValue(Cell container, string value)
        {
            Paragraph valuePara = new Paragraph(value ?? "\n")
                .SetFont(FONT_NORMAL)
                .SetFontSize(9)  // Reduced from 8
                .SetMarginBottom(3)  // Reduced from 4
                .SetMarginTop(0.5f);  // Reduced from 1
            container.Add(valuePara);
        }

        // Helper method to create schedule header cells
        private Cell CreateScheduleHeaderCell(string text)
        {
            return new Cell()
                .SetBorder(new SolidBorder(BLACK_COLOR, 0.5f))
                .SetBackgroundColor(new DeviceRgb(240, 240, 240))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(2)  // Reduced from 3
                .Add(new Paragraph(text)
                    .SetFont(FONT_BOLD)
                    .SetFontSize(8)  // Reduced from 8
                    .SetMargin(0));
        }

        // Helper method to create schedule data cells
        private Cell CreateScheduleCell(string text)
        {
            return new Cell()
                .SetBorder(new SolidBorder(BLACK_COLOR, 0.5f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(1)  // Added padding reduction
                .Add(new Paragraph(text)
                    .SetFont(FONT_NORMAL)
                    .SetFontSize(7)  // Reduced from 7
                    .SetMargin(0));
        }

        private void AddInstructionsSection(Cell container, ExamDetails examDetails)
        {
            Table signatureTable = new Table(UnitValue.CreatePercentArray(new float[] { 33, 33, 33 }))
             .UseAllAvailableWidth();

            // Create a 2-column table, each 50% of the width
            // Left cell: Student's Signature with dotted line
            Cell studentSignCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPaddingRight(10);

            studentSignCell.Add(new Paragraph("...................................")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1));

            studentSignCell.Add(new Paragraph("Principal's Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER));

            Cell teachreSignCell = new Cell()
               .SetBorder(Border.NO_BORDER)
               .SetPaddingRight(10);

            teachreSignCell.Add(new Paragraph("...................................")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1));

            teachreSignCell.Add(new Paragraph("Class Teacher's Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER));

            // Right cell: Principal's Signature with dotted line
            Cell principalSignCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPaddingLeft(10);

            principalSignCell.Add(new Paragraph("...................................")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(1));

            principalSignCell.Add(new Paragraph("Student's Signature")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER));

            // Add them to the table
            signatureTable.AddCell(studentSignCell);
            signatureTable.AddCell(teachreSignCell);
            signatureTable.AddCell(principalSignCell);

            // Finally, add the signature table to the container
            container.Add(signatureTable);
            // Add third line - before "Instructions"
            container.Add(new LineSeparator(new SolidLine(1f)));
           // container.Add(new Div().SetHeight(1).SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            // Instructions wrapper to ensure full width usage
            Table instructionsWrapper = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .UseAllAvailableWidth()
                .SetBorder(Border.NO_BORDER);

            Cell instructionsCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0);

            // Instructions header
            Paragraph instructionsHeader = new Paragraph("INSTRUCTIONS TO THE CANDIDATES:-")
                .SetFont(FONT_BOLD)
                .SetFontSize(8)
                .SetFontColor(RED_COLOR)
                .SetMarginLeft(2);
            instructionsCell.Add(instructionsHeader);

            // Instructions list
            var notes = new[]
                                {
                        examDetails.admitnote1,
        examDetails.admitnote2,
        examDetails.admitnote3,
        examDetails.admitnote4,
        examDetails.admitnote5


    };
            for (int i = 0; i < notes.Length; i++)
            {
                string note = notes[i]?.Trim();
                if (!string.IsNullOrEmpty(note))
                {
                    // show numbered row
                    instructionsCell.Add(new Paragraph($"{i + 1}. {note}")
                                    .SetFont(FONT_NORMAL)
                                    .SetFontSize(8)
                                    .SetMarginLeft(2)
                                    .SetMarginBottom(0.5f)
                                    .SetTextAlignment(TextAlignment.LEFT));
                }
                else
                {
                    // still add one blank line, but without number
                    instructionsCell.Add(new Paragraph("\u00A0")
                                   .SetFont(FONT_NORMAL)
                                   .SetFontSize(8)
                                   .SetMarginLeft(2)
                                   .SetMarginBottom(0.5f)
                                   .SetTextAlignment(TextAlignment.LEFT));
                }
            }

            instructionsWrapper.AddCell(instructionsCell);
            container.Add(instructionsWrapper);
        }

        private void AddCuttingLine(Document document)
        {
            Table cuttingTable = new Table(UnitValue.CreatePercentArray(new float[] { 100 }))
                .UseAllAvailableWidth()
                .SetMarginTop(2)     // Reduced from 3
                .SetMarginBottom(2);  // Reduced from 3

            Cell cuttingCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(0);

            Paragraph cuttingLine = new Paragraph()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFont(FONT_NORMAL)
                .SetFontSize(10)
                .SetMargin(0);

            cuttingLine.Add(new Text("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -"));
            cuttingCell.Add(cuttingLine);
            cuttingTable.AddCell(cuttingCell);
            document.Add(cuttingTable);
        }
    }

    // Model classes
    public class StudentExamInfo
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string MotherName { get; set; }
        public string FatherName { get; set; }
        public string ClassName { get; set; }
        public string Section { get; set; }
        public string RollNumber { get; set; }
        public string AdmissionNumber { get; set; }
        public string PhotoPath { get; set; } = @"/template/assets/img/noimgstu.png";
        public string admitnote1 { get; set; }
        public string admitnote2 { get; set; }
        public string admitnote3 { get; set; }
        public string admitnote4 { get; set; }
        public string admitnote5 { get; set; }
    }

    public class ExamDetails
    {
        public Guid ExamId { get; set; }
        public string ExamName { get; set; }
        public string AdmitCard { get; set; }
        public string Session { get; set; }
        public Guid SubjectID { get; set; }
        public DateTime? ExamDate { get; set; }
        public string ExamTime { get; set; }
        public string SubjectName { get; set; }
        public string ExamDay { get; set; }
        public string admitnote1 { get; set; }
        public string admitnote2 { get; set; }
        public string admitnote3 { get; set; }
        public string admitnote4 { get; set; }
        public string admitnote5 { get; set; }
        public string admitnote6 { get; set; }
        public string admitnote7 { get; set; }
        public string admitnote8 { get; set; }
        public string admitnote9 { get; set; }
        public string admitnote10 { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string MotherName { get; set; }
        public string FatherName { get; set; }
        public string ClassName { get; set; }
        public string Section { get; set; }
        public string RollNumber { get; set; }
        public string AdmissionNumber { get; set; }
        public string PhotoPath { get; set; } = @"/template/assets/img/noimgstu.png";
        public ExamDetails()
        {
            admitnote1 = "The candidate must keep this admission card at the time of Examination";
            admitnote2 = "No entry allowed 30 minutes after exam start.";
            admitnote3 = "Cannot leave before exam ends";
            admitnote4 = "No sharing of stationary items";
            admitnote5 = "Follow Covid-19 precautions, Mask is compulsory";
        }
        public List<ExamSubject> ExamSubjects { get; set; }
    }

    public class ExamSubject
    {
        public string SubjectName { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}