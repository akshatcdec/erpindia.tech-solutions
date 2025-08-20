using Dapper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2016.Excel;
using ERPIndia.BulkUpdate;
using ERPIndia.Class.Helper;
using ERPIndia.Controllers.Examination;
using ERPIndia.Models.School;
using ERPIndia.Models.SystemSettings;
using ERPIndia.StudentManagement.Models;
using ERPIndia.Utilities;
using ERPK12Models.StudentInformation;
using Hangfire.Common;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.X509;
using StudentManagement.DTOs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using static ERPIndia.Controllers.BulkPhotoUploadController;

namespace ERPIndia.StudentManagement.Repository
{
    public class StudentRepository
    {
        private readonly string _connectionString;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        public StudentRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        }
        // Add this method to your StudentRepository class
        // Add this method to your StudentRepository class

        public bool UpdateStudentPhotosByStudentId(StudentPhotoUpdate photoUpdate, Guid modifiedBy)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update StudentInfoBasic - only update the student photo
                        if (!string.IsNullOrEmpty(photoUpdate.StudentPhoto))
                        {
                            var updateBasicSql = @"
                        UPDATE StudentInfoBasic 
                        SET Photo = @Photo,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy
                        WHERE StudentId = @StudentId 
                        AND SchoolCode = @SchoolCode";

                            connection.Execute(updateBasicSql, new
                            {
                                Photo = photoUpdate.StudentPhoto,
                                StudentId = photoUpdate.StudentId,
                                SchoolCode = photoUpdate.SchoolCode,
                                ModifiedBy = modifiedBy
                            }, transaction);
                        }

                        // Check if we need to update family photos
                        if (!string.IsNullOrEmpty(photoUpdate.FatherPhoto) ||
                            !string.IsNullOrEmpty(photoUpdate.MotherPhoto) ||
                            !string.IsNullOrEmpty(photoUpdate.GuardianPhoto))
                        {
                            // First ensure family record exists
                            var ensureFamilySql = @"
                        IF NOT EXISTS (SELECT 1 FROM StudentInfoFamily WHERE StudentId = @StudentId AND SchoolCode = @SchoolCode)
                        BEGIN
                            INSERT INTO StudentInfoFamily (StudentId, AdmsnNo, SchoolCode, CreatedBy, CreatedDate, IsActive)
                            SELECT StudentId, AdmsnNo, SchoolCode, @ModifiedBy, GETDATE(), 1
                            FROM StudentInfoBasic
                            WHERE StudentId = @StudentId AND SchoolCode = @SchoolCode
                        END";

                            connection.Execute(ensureFamilySql, new
                            {
                                StudentId = photoUpdate.StudentId,
                                SchoolCode = photoUpdate.SchoolCode,
                                ModifiedBy = modifiedBy
                            }, transaction);

                            // Build dynamic update for family photos
                            var updateFields = new List<string>();
                            var parameters = new DynamicParameters();

                            parameters.Add("@StudentId", photoUpdate.StudentId);
                            parameters.Add("@SchoolCode", photoUpdate.SchoolCode);
                            parameters.Add("@ModifiedBy", modifiedBy);

                            if (!string.IsNullOrEmpty(photoUpdate.FatherPhoto))
                            {
                                updateFields.Add("FPhoto = @FPhoto");
                                parameters.Add("@FPhoto", photoUpdate.FatherPhoto);
                            }

                            if (!string.IsNullOrEmpty(photoUpdate.MotherPhoto))
                            {
                                updateFields.Add("MPhoto = @MPhoto");
                                parameters.Add("@MPhoto", photoUpdate.MotherPhoto);
                            }

                            if (!string.IsNullOrEmpty(photoUpdate.GuardianPhoto))
                            {
                                updateFields.Add("GPhoto = @GPhoto");
                                parameters.Add("@GPhoto", photoUpdate.GuardianPhoto);
                            }

                            if (updateFields.Any())
                            {
                                updateFields.Add("ModifiedDate = GETDATE()");
                                updateFields.Add("ModifiedBy = @ModifiedBy");

                                var updateFamilySql = $@"
                            UPDATE StudentInfoFamily 
                            SET {string.Join(", ", updateFields)}
                            WHERE StudentId = @StudentId 
                            AND SchoolCode = @SchoolCode";

                                connection.Execute(updateFamilySql, parameters, transaction);
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Error updating student photos: {ex.Message}");
                        throw;
                    }
                }
            }
        }
        public bool UpdateStudentPhotos(StudentPhotoUpdate photoUpdate, Guid modifiedBy)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Update StudentInfoBasic - only update the student photo
                        if (!string.IsNullOrEmpty(photoUpdate.StudentPhoto))
                        {
                            var updateBasicSql = @"
                        UPDATE StudentInfoBasic 
                        SET Photo = @Photo,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy
                        WHERE AdmsnNo = @AdmsnNo 
                        AND SchoolCode = @SchoolCode";

                            connection.Execute(updateBasicSql, new
                            {
                                Photo = photoUpdate.StudentPhoto,
                                AdmsnNo = photoUpdate.AdmsnNo,
                                SchoolCode = photoUpdate.SchoolCode,
                                ModifiedBy = modifiedBy
                            }, transaction);
                        }

                        // Check if we need to update family photos
                        if (!string.IsNullOrEmpty(photoUpdate.FatherPhoto) ||
                            !string.IsNullOrEmpty(photoUpdate.MotherPhoto) ||
                            !string.IsNullOrEmpty(photoUpdate.GuardianPhoto))
                        {
                            // First ensure family record exists
                            var ensureFamilySql = @"
                        IF NOT EXISTS (SELECT 1 FROM StudentInfoFamily WHERE AdmsnNo = @AdmsnNo AND SchoolCode = @SchoolCode)
                        BEGIN
                            INSERT INTO StudentInfoFamily (StudentId, AdmsnNo, SchoolCode, CreatedBy, CreatedDate, IsActive)
                            SELECT StudentId, AdmsnNo, SchoolCode, @ModifiedBy, GETDATE(), 1
                            FROM StudentInfoBasic
                            WHERE AdmsnNo = @AdmsnNo AND SchoolCode = @SchoolCode
                        END";

                            connection.Execute(ensureFamilySql, new
                            {
                                AdmsnNo = photoUpdate.AdmsnNo,
                                SchoolCode = photoUpdate.SchoolCode,
                                ModifiedBy = modifiedBy
                            }, transaction);

                            // Build dynamic update for family photos
                            var updateFields = new List<string>();
                            var parameters = new DynamicParameters();

                            parameters.Add("@AdmsnNo", photoUpdate.AdmsnNo);
                            parameters.Add("@SchoolCode", photoUpdate.SchoolCode);
                            parameters.Add("@ModifiedBy", modifiedBy);

                            if (!string.IsNullOrEmpty(photoUpdate.FatherPhoto))
                            {
                                updateFields.Add("FPhoto = @FPhoto");
                                parameters.Add("@FPhoto", photoUpdate.FatherPhoto);
                            }

                            if (!string.IsNullOrEmpty(photoUpdate.MotherPhoto))
                            {
                                updateFields.Add("MPhoto = @MPhoto");
                                parameters.Add("@MPhoto", photoUpdate.MotherPhoto);
                            }

                            if (!string.IsNullOrEmpty(photoUpdate.GuardianPhoto))
                            {
                                updateFields.Add("GPhoto = @GPhoto");
                                parameters.Add("@GPhoto", photoUpdate.GuardianPhoto);
                            }

                            if (updateFields.Any())
                            {
                                updateFields.Add("ModifiedDate = GETDATE()");
                                updateFields.Add("ModifiedBy = @ModifiedBy");

                                var updateFamilySql = $@"
                            UPDATE StudentInfoFamily 
                            SET {string.Join(", ", updateFields)}
                            WHERE AdmsnNo = @AdmsnNo 
                            AND SchoolCode = @SchoolCode";

                                connection.Execute(updateFamilySql, parameters, transaction);
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Error updating student photos: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        // Add this class to the BulkPhotoUploadController namespace if not already present

        public List<StudentInfoBasicDto> GetStudentsByClassAndSection(string className, string section)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"SELECT 
    AdmsnNo,
    AadharNo,
    FatherAadhar,
    MotherAadhar,
    FatherPhoto,
    MotherPhoto,
    GuardianPhoto,
    BloodGroup,
    SchoolCode,
    StudentNo,
    SrNo,
    RollNo,
    DOB,
    AdmsnDate,
    Class,
    ClassName,
    Section,
    SectionName,
    FirstName,
    LastName,
    FatherName,
    MotherName,
    PreviousSchool,
    UDISE,
    Gender,
    Mobile,
    Photo,
    StudentId, 
    Email = '', -- Email not available directly in view
    FatherMobile,
    MotherMobile,
    StCurrentAddress AS Address,
    Category,
    '' AS House,
    Password, 
    FeeCategory, 
    Height, 
    Weight, 
    PENNo,
    VillegeName, 
    VillegeId,
    OldBalance,
    IsActive,
    PickupName AS PickupPoint,
    CASE 
        WHEN EXISTS (
            SELECT 1 
            FROM FeeReceivedTbl f 
            WHERE f.StudentId = vwStudentInfo.StudentId 
                AND f.IsActive = 1 
                AND f.IsDeleted = 0
        ) THEN 'Y'
        ELSE 'N'
    END AS HasFeeRecords
FROM vwStudentInfo
WHERE Class = @Class 
    AND (@Section IS NULL OR Section = @Section)
    AND IsActive = 1 
    AND IsDeleted = 0
ORDER BY RollNo, FirstName";

                var parameters = new
                {
                    Class = className,
                    Section = string.IsNullOrEmpty(section) ? null : section
                };

                return connection.Query<StudentInfoBasicDto>(sql, parameters).ToList();
            }
        }
        public bool BulkUpdateStudents(string columnName, List<UpdateItem> updates, Guid ModifiedBy)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name must be provided", nameof(columnName));

            if (updates == null || updates.Count == 0)
                return false;

            // Define columns that belong to StudentInfoFamily
            var familyColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "FatherName", "FName" },
        { "MotherName", "MName" },
        { "GuardianName", "GName" },
        { "FatherAadhar", "FAadhar" },
        { "MotherAadhar", "MAadhar" },
        { "Address", "StCurrentAddress" },
        { "GuardianMobile", "GPhone" },
        { "MotherMobile", "MPhone" }
    };

            // All other allowed columns for StudentInfoBasic
            var basicColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "AdmsnNo", "AadharNo", "SrNo", "RollNo", "FirstName", "LastName",
        "Mobile", "DOB", "OldBalance", "Gender",
        "BloodGroup", "Category", "Section",
        "Password", "FeeCategory",
        "Height", "Weight", "PENNo", "PreviousSchool",
        "UDISE", "Photo", "IsActive", "VillegeName",
        "AdmsnDate"
    };

            var otherColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "PreviousSchool", "PreviousSchoolDtl" },
        { "UDISE", "UdiseCode" }
    };

            // Check if column is valid
            bool isValidColumn = familyColumns.ContainsKey(columnName)
                        || basicColumns.Contains(columnName)
                        || otherColumns.ContainsKey(columnName);

            if (!isValidColumn)
                throw new ArgumentException($"Invalid column name: {columnName}", nameof(columnName));

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string sql;

                        // SQL to ensure family records exist
                        string ensureFamilyRecordsSql = @"
                    INSERT INTO dbo.StudentInfoFamily (StudentId, AdmsnNo, SchoolCode, CreatedBy, CreatedDate, IsActive)
                    SELECT b.StudentId, b.AdmsnNo, b.SchoolCode, @ModifiedBy, GETDATE(), 1
                    FROM dbo.StudentInfoBasic b
                    WHERE b.AdmsnNo = @AdmsnNo 
                      AND b.SchoolCode = @SchoolCode
                      AND NOT EXISTS (
                          SELECT 1 FROM dbo.StudentInfoFamily f 
                          WHERE f.StudentId = b.StudentId
                      )";

                        // SQL to ensure other records exist
                        string ensureOtherRecordsSql = @"
                    INSERT INTO dbo.StudentInfoOther (
                        StudentId, AdmsnNo, SchoolCode, TenantID, SessionID, 
                        CreatedBy, CreatedDate, IsActive, IsDeleted, TenantCode
                    )
                    SELECT 
                        b.StudentId, b.AdmsnNo, b.SchoolCode, b.TenantID, b.SessionID,
                        @ModifiedBy, GETDATE(), 1, 0, b.TenantCode
                    FROM dbo.StudentInfoBasic b
                    WHERE b.AdmsnNo = @AdmsnNo 
                      AND b.SchoolCode = @SchoolCode
                      AND NOT EXISTS (
                          SELECT 1 FROM dbo.StudentInfoOther o 
                          WHERE o.StudentId = b.StudentId
                      )";

                        bool updateFamilyTable = familyColumns.ContainsKey(columnName);
                        bool updateOtherTable = otherColumns.ContainsKey(columnName);

                        if (updateFamilyTable)
                        {
                            // Get the actual column name in StudentInfoFamily table
                            string actualColumnName = familyColumns[columnName];

                            // Update StudentInfoFamily
                            sql = $@"
                        UPDATE f
                        SET [{actualColumnName}] = @Value,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy
                        FROM dbo.StudentInfoFamily f
                        INNER JOIN dbo.StudentInfoBasic b ON f.StudentId = b.StudentId
                        WHERE b.AdmsnNo = @AdmsnNo
                          AND b.SchoolCode = @SchoolCode;

                        -- Also update the denormalized column in StudentInfoBasic
                        UPDATE dbo.StudentInfoBasic
                        SET [{columnName}] = @Value,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy
                        WHERE AdmsnNo = @AdmsnNo
                          AND SchoolCode = @SchoolCode;";
                        }
                        else if (updateOtherTable)
                        {
                            // Get the actual column name in StudentInfoOther table
                            string actualColumnName = otherColumns[columnName];

                            sql = $@"
                        UPDATE o
                        SET [{actualColumnName}] = @Value,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy
                        FROM dbo.StudentInfoOther o
                        INNER JOIN dbo.StudentInfoBasic b ON o.StudentId = b.StudentId
                        WHERE b.AdmsnNo = @AdmsnNo
                          AND b.SchoolCode = @SchoolCode;";
                        }
                        else
                        {
                            // Update only StudentInfoBasic
                            var quotedColumn = $"[{columnName}]";
                            sql = $@"
                        UPDATE dbo.StudentInfoBasic
                        SET {quotedColumn} = @Value,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy
                        WHERE AdmsnNo = @AdmsnNo
                          AND SchoolCode = @SchoolCode;";
                        }

                        int successCount = 0;
                        var errors = new List<string>();

                        foreach (var update in updates)
                        {
                            if (update == null)
                                continue;

                            try
                            {
                                // Ensure family record exists if updating family table
                                if (updateFamilyTable)
                                {
                                    var ensureParams = new DynamicParameters();
                                    ensureParams.Add("@AdmsnNo", update.AdmsnNo);
                                    ensureParams.Add("@SchoolCode", update.SchoolCode);
                                    ensureParams.Add("@ModifiedBy", ModifiedBy);

                                    connection.Execute(ensureFamilyRecordsSql, ensureParams, transaction);
                                }

                                // Ensure other record exists if updating other table
                                if (updateOtherTable)
                                {
                                    var ensureParams = new DynamicParameters();
                                    ensureParams.Add("@AdmsnNo", update.AdmsnNo);
                                    ensureParams.Add("@SchoolCode", update.SchoolCode);
                                    ensureParams.Add("@ModifiedBy", ModifiedBy);

                                    connection.Execute(ensureOtherRecordsSql, ensureParams, transaction);
                                }

                                // Convert value based on column type
                                object paramValue = ConvertValueForColumn(columnName, update.Value);

                                var parameters = new DynamicParameters();
                                parameters.Add("@Value", paramValue);
                                parameters.Add("@AdmsnNo", update.AdmsnNo);
                                parameters.Add("@SchoolCode", update.SchoolCode);
                                parameters.Add("@ModifiedBy", ModifiedBy);

                                var rowsAffected = connection.Execute(sql, parameters, transaction);

                                if (rowsAffected > 0)
                                {
                                    successCount++;
                                }
                                else
                                {
                                    errors.Add($"No student found with AdmsnNo: {update.AdmsnNo}");
                                }
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Error updating AdmsnNo {update.AdmsnNo}: {ex.Message}");
                            }
                        }

                        // If all updates were successful, commit
                        if (errors.Count == 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                        else if (successCount > 0)
                        {
                            // Partial success - commit the successful ones
                            transaction.Commit();

                            // Log errors
                            foreach (var error in errors)
                            {
                                System.Diagnostics.Debug.WriteLine(error);
                            }

                            return true;
                        }
                        else
                        {
                            // No successes, rollback
                            transaction.Rollback();
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"BulkUpdateStudents error: {ex.Message}");
                        throw;
                    }
                }
            }
        } // Helper method to convert values based on column type
        private object ConvertValueForColumn(string columnName, string value)
        {
            // Handle null or empty values
            if (string.IsNullOrEmpty(value))
            {
                // For certain columns, we might want to set specific defaults
                if (columnName.Equals("IsActive", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                return DBNull.Value;
            }

            // Date conversions
            if (columnName.Equals("DOB", StringComparison.OrdinalIgnoreCase) ||
                columnName.Equals("AdmsnDate", StringComparison.OrdinalIgnoreCase))
            {
                if (DateTime.TryParse(value, out DateTime dateValue))
                {
                    return dateValue;
                }
                return DBNull.Value;
            }

            // Boolean conversion
            if (columnName.Equals("IsActive", StringComparison.OrdinalIgnoreCase))
            {
                // Handle various boolean representations
                value = value.Trim().ToLower();
                return value == "1" || value == "true" || value == "yes" || value == "active";
            }

            // Numeric conversions
            if (columnName.Equals("Height", StringComparison.OrdinalIgnoreCase) ||
                columnName.Equals("Weight", StringComparison.OrdinalIgnoreCase) ||
                columnName.Equals("OldBalance", StringComparison.OrdinalIgnoreCase))
            {
                if (decimal.TryParse(value, out decimal decimalValue))
                {
                    return decimalValue;
                }
                return DBNull.Value;
            }

            // Integer conversions for columns that might be stored as int
            if (columnName.Equals("SrNo", StringComparison.OrdinalIgnoreCase) ||
                columnName.Equals("RollNo", StringComparison.OrdinalIgnoreCase) ||
                columnName.Equals("StudentNo", StringComparison.OrdinalIgnoreCase))
            {
                // These might be stored as NVARCHAR based on your schema, 
                // so we'll keep them as strings unless you confirm they're integers
                return value.Trim();
            }

            // Password handling - you might want to hash it
            if (columnName.Equals("Password", StringComparison.OrdinalIgnoreCase))
            {
                // Consider hashing the password here
                // return HashPassword(value);
                return value; // For now, returning as-is
            }

            // Photo handling - might be multiple paths separated by semicolon
            if (columnName.Equals("Photo", StringComparison.OrdinalIgnoreCase))
            {
                // Clean up the path
                return value.Trim();
            }

            // Default: return as string
            return value.Trim();
        }

        // Helper method to get current user (implement based on your authentication)
        private string GetCurrentUser()
        {
            // Implement based on your authentication system
            // For example:
            // return HttpContext.Current?.User?.Identity?.Name ?? "System";

            return "System"; // Default value
        }

        // Optional: Method to validate values before update
        private bool ValidateValue(string columnName, string value, out string errorMessage)
        {
            errorMessage = null;

            // Aadhar validation
            if ((columnName.Equals("AadharNo", StringComparison.OrdinalIgnoreCase) ||
                 columnName.Equals("FatherAadhar", StringComparison.OrdinalIgnoreCase) ||
                 columnName.Equals("MotherAadhar", StringComparison.OrdinalIgnoreCase)) &&
                !string.IsNullOrEmpty(value))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{12}$"))
                {
                    errorMessage = "Aadhar number must be exactly 12 digits";
                    return false;
                }
            }

            // Mobile validation
            if (columnName.Equals("Mobile", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(value))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^[6-9]\d{9}$"))
                {
                    errorMessage = "Mobile number must be 10 digits starting with 6-9";
                    return false;
                }
            }

            // Gender validation
            if (columnName.Equals("Gender", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(value))
            {
                var validGenders = new[] { "M", "F", "O", "Male", "Female", "Other" };
                if (!validGenders.Contains(value, StringComparer.OrdinalIgnoreCase))
                {
                    errorMessage = "Gender must be M, F, or O";
                    return false;
                }
            }

            // Blood group validation
            if (columnName.Equals("BloodGroup", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(value))
            {
                var validBloodGroups = new[] { "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" };
                if (!validBloodGroups.Contains(value, StringComparer.OrdinalIgnoreCase))
                {
                    errorMessage = "Invalid blood group";
                    return false;
                }
            }

            return true;
        }

        // Optional: Enhanced version with validation
        public bool BulkUpdateStudentsWithValidation(string columnName, List<UpdateItem> updates, out List<string> validationErrors, Guid ModifiedBy)
        {
            validationErrors = new List<string>();

            // First validate all values
            foreach (var update in updates)
            {
                if (ValidateValue(columnName, update.Value, out string error))
                {
                    continue;
                }
                validationErrors.Add($"Student {update.AdmsnNo}: {error}");
            }

            // If there are validation errors, don't proceed
            if (validationErrors.Any())
            {
                return false;
            }

            // Proceed with the update
            return BulkUpdateStudents(columnName, updates, ModifiedBy);
        } // Helper method to get a new connection
        private IDbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
        public async Task<List<StudentExamInfo>> GetStudentsByIdsAsync(List<Guid> studentIds, Guid sessionId, int tenantCode)
        {
            // Simulating asynchronous operation (replace this with real DB/service call)
            //await Task.Delay(10); // Simulate async work

            var students = new List<StudentExamInfo>();

            // Example logic - replace with actual data fetching
            foreach (var id in studentIds)
            {
                students.Add(new StudentExamInfo
                {
                    StudentId = id
                    // Add other properties as needed
                });
            }

            return students;
        }


        /// <summary>
        /// Retrieves complete student admission form data using a single database connection
        /// with multiple result sets for optimal performance
        /// </summary>
        /// <param name="admsnNo">Admission Number</param>
        /// <param name="schoolCode">School Code</param>
        /// <returns>Complete student admission form data</returns>
        public async Task<StudentAdmissionFormDto> GetStudentAdmissionDataAsync(int admsnNo, int schoolCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = @"
                    -- Query 1: Basic Student Information
                   SELECT sib.*, 
                   acm.ClassName,
                   asm.SectionName,
                   tv.VehicleNo AS VechileName,
                   tr.Name AS RouteName,
                   tp.PickupName, 
                   av.VillageName,
                   ah.HostelName
                   FROM StudentInfoBasic sib
                   LEFT JOIN AcademicClassMaster acm ON acm.ClassID = sib.ClassId
                   LEFT JOIN AcademicSectionMaster asm ON asm.SectionID = sib.SectionId
                   LEFT JOIN TransportVehicles AS tv ON tv.TransportVehiclesId=sib.VechileId
                   LEFT JOIN TransportRoute AS tr ON tr.TransportRouteId=sib.RouteId
                   LEFT JOIN TransportPickups AS tp ON tp.TransportPickupsId=sib.PickupId
                   LEFT JOIN AcademicVillageMaster AS av ON av.VillageID=sib.VillegeId
                   LEFT JOIN AcademicHostelMaster AS ah ON ah.HostelID=sib.HostelId
                   WHERE sib.AdmsnNo = @AdmsnNo 
                   AND sib.SchoolCode = @SchoolCode;

                    -- Query 2: Family Information
                    SELECT * FROM StudentInfoFamily
                    WHERE AdmsnNo = @AdmsnNo AND SchoolCode = @SchoolCode;

                    -- Query 3: Other Information
                    SELECT * FROM StudentInfoOther
                    WHERE AdmsnNo = @AdmsnNo AND SchoolCode = @SchoolCode;

                    -- Query 4: Subject Information (One-to-Many)
                    SELECT * FROM StudentInfoSubjects
                    WHERE AdmsnNo = @AdmsnNo AND SchoolCode = @SchoolCode;

                    -- Query 5: Sibling Information (One-to-Many)
                    SELECT * FROM StudentInfoSiblings
                    WHERE AdmsnNo = @AdmsnNo AND SchoolCode = @SchoolCode;

                    -- Query 6: Education Details (One-to-Many)
                    SELECT * FROM StudentInfoEduDetails
                    WHERE AdmsnNo = @AdmsnNo AND SchoolCode = @SchoolCode;";

                var parameters = new { AdmsnNo = admsnNo, SchoolCode = schoolCode };

                using (var multi = await connection.QueryMultipleAsync(sql, parameters))
                {
                    var result = new StudentAdmissionFormDto();

                    // Read each result set
                    result.BasicInfo = (await multi.ReadAsync<StudentInfoBasicDto>()).FirstOrDefault();
                    result.FamilyInfo = (await multi.ReadAsync<StudentInfoFamilyDto>()).FirstOrDefault();
                    result.OtherInfo = (await multi.ReadAsync<StudentInfoOtherDto>()).FirstOrDefault();
                    result.Subjects = (await multi.ReadAsync<StudentInfoSubjectDto>()).ToList();
                    result.Siblings = (await multi.ReadAsync<StudentInfoSiblingDto>()).ToList();
                    result.EducationDetails = (await multi.ReadAsync<StudentInfoEduDetailDto>()).ToList();

                    return result;
                }
            }
        }
        public void MapAndUpdateStudentReferences(Guid sessionId, Guid userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_MapAndUpdateStudentReferences", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.Add("@SessionID", SqlDbType.UniqueIdentifier).Value = sessionId;
                    command.Parameters.Add("@CreatedBy", SqlDbType.UniqueIdentifier).Value = userId;

                    connection.OpenAsync();
                    command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task MapAndUpdateStudentReferences1(Guid sessionId, Guid userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("sp_MapAndUpdateStudentReferences", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.Add("@SessionID", SqlDbType.UniqueIdentifier).Value = sessionId;
                    command.Parameters.Add("@CreatedBy", SqlDbType.UniqueIdentifier).Value = userId;

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
        public async Task<StudentCompleteInfoDto> GetStudentCompleteInfoAsync(Guid studentId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Execute the stored procedure using Dapper
                using (var multi = await connection.QueryMultipleAsync(
                    "dbo.sp_GetCompleteStudentInfo",
                    new { StudentId = studentId },
                    commandType: CommandType.StoredProcedure))
                {
                    // Create the result object
                    var result = new StudentCompleteInfoDto();

                    // Read the basic info (first result set)
                    // Dapper will automatically map database fields to DTO properties by name
                    result.BasicInfo = await multi.ReadFirstOrDefaultAsync<StudentBasicInfoDto>();

                    // If no student found, return null
                    if (result.BasicInfo == null)
                        return null;

                    // Read education details (second result set)
                    result.EducationDetails = (await multi.ReadAsync<StudentEducationDetailDto>()).ToList();

                    // Read siblings information (third result set)
                    result.Siblings = (await multi.ReadAsync<StudentSiblingDto>()).ToList();

                    // Read subjects information (fourth result set)
                    result.Subjects = (await multi.ReadAsync<StudentSubjectDto>()).ToList();

                    return result;
                }
            }
        }
        // Insert a new student with all related data
        public async Task<bool> SaveStudentAsync(StudentViewModel studentModel)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Set up default values if not provided
                        studentModel.Basic.EntryDate = studentModel.Basic.EntryDate ?? DateTime.Now;
                        studentModel.Basic.UserId = studentModel.Basic.UserId ?? "System";
                        var basicSql = "sp_InsertStudentBasicByCreateForm";
                        var parameters = new DynamicParameters();

                        // Required primary keys
                        parameters.Add("@AdmsnNo", studentModel.Basic.AdmsnNo);
                        parameters.Add("@SchoolCode", studentModel.Basic.SchoolCode);
                        DateTime? admdate = Utils.SplitAndConvertDate(studentModel.Basic.AdmsnDate);
                        if (admdate.HasValue)
                        {
                            parameters.Add("@AdmsnDate", admdate);
                        }
                        DateTime? dobdate = Utils.SplitAndConvertDate(studentModel.Basic.DOB);
                        if (dobdate.HasValue)
                        {
                            parameters.Add("@DOB", dobdate);
                        }
                        // Student basic info
                        parameters.Add("@StudentNo", studentModel.Basic.StudentNo);
                        parameters.Add("@SrNo", studentModel.Basic.SrNo);
                        parameters.Add("@RollNo", studentModel.Basic.RollNo);
                        parameters.Add("@Class", studentModel.Basic.Class);
                        parameters.Add("@Section", studentModel.Basic.Section);
                        parameters.Add("@FirstName", studentModel.Basic.FirstName);
                        parameters.Add("@LastName", studentModel.Basic.LastName);
                        parameters.Add("@Gender", studentModel.Basic.Gender);
                        parameters.Add("@Category", studentModel.Basic.Category);
                        parameters.Add("@Religion", studentModel.Basic.Religion);
                        parameters.Add("@Caste", studentModel.Basic.Caste);
                        parameters.Add("@Mobile", studentModel.Basic.Mobile);
                        parameters.Add("@WhatsAppNum", studentModel.Basic.WhatsAppNum);
                        parameters.Add("@Email", studentModel.Basic.Email);


                        // For the photo, use the file path rather than the file object
                        parameters.Add("@Photo", studentModel.Basic.Photo);
                        parameters.Add("@BloodGroup", studentModel.Basic.BloodGroup);
                        parameters.Add("@House", studentModel.Basic.House);
                        parameters.Add("@Height", studentModel.Basic.Height);
                        parameters.Add("@Weight", studentModel.Basic.Weight);
                        parameters.Add("@AsOnDt", studentModel.Basic.AsOnDt);
                        parameters.Add("@SiblingRef", studentModel.Basic.SiblingRef);
                        parameters.Add("@SiblingRef2", studentModel.Basic.SiblingRef2);
                        parameters.Add("@DiscountCategory", studentModel.Basic.DiscountCategory);
                        parameters.Add("@DiscountNote", studentModel.Basic.DiscountNote);
                        parameters.Add("@LoginPwd", studentModel.Basic.LoginPwd);
                        parameters.Add("@OldBalance", studentModel.Basic.OldBalance);
                        parameters.Add("@FeeCategory", studentModel.Basic.FeeCategory);
                        parameters.Add("@Active", studentModel.Basic.IsActive);
                        parameters.Add("@EnquiryData", studentModel.Basic.EnquiryData);
                        parameters.Add("@SendSMS", studentModel.Basic.SendSMS);
                        parameters.Add("@IsLateFee", studentModel.Basic.IsLateFee);
                        parameters.Add("@UserId", studentModel.Basic.UserId);
                        parameters.Add("@EntryDate", studentModel.Basic.EntryDate);
                        parameters.Add("@AcademicYear", studentModel.Basic.AcademicYear);
                        parameters.Add("@MotherTongue", studentModel.Basic.MotherTongue);
                        parameters.Add("@Status", studentModel.Basic.Status);
                        parameters.Add("@LanguagesKnown", studentModel.Basic.LanguagesKnown);
                        parameters.Add("@AadharNo", studentModel.Basic.AadharNo);
                        parameters.Add("@PENNo", studentModel.Basic.PENNo);
                        parameters.Add("@PickupPoint", studentModel.Basic.PickupPoint);
                        parameters.Add("@LoginId", studentModel.Basic.AdmsnNo);
                        parameters.Add("@Password", studentModel.Basic.Password);

                        // IDs
                        parameters.Add("@ClassId", studentModel.Basic.ClassId);
                        parameters.Add("@SectionId", studentModel.Basic.SectionId);
                        parameters.Add("@HouseId", studentModel.Basic.HouseId);
                        parameters.Add("@FeeCategoryId", studentModel.Basic.FeeCategoryId);
                        parameters.Add("@FeeDiscountId", studentModel.Basic.FeeDiscountId);
                        parameters.Add("@TenantID", studentModel.Basic.TenantId);
                        parameters.Add("@TenantCode", studentModel.Basic.TenantCode);

                        // Default system columns
                        parameters.Add("@IsActive", studentModel.Basic.IsActive);
                        parameters.Add("@IsDeleted", false);
                        parameters.Add("@CreatedBy", studentModel.Basic.CreatedBy);
                        parameters.Add("@SessionID", studentModel.Basic.SessionId);

                        parameters.Add("@VechileId", studentModel.Basic.VechileId);
                        parameters.Add("@VillegeId", studentModel.Basic.VillegeId);
                        parameters.Add("@RouteId", studentModel.Basic.RouteId);
                        parameters.Add("@PickupId", studentModel.Basic.PickupId);
                        parameters.Add("@HostelId", studentModel.Basic.HostelId);
                        
                        parameters.Add("@StudentNameHindi", studentModel.Basic.StudentNameHindi);
                        parameters.Add("@FatherNameHindi", studentModel.Basic.FatherNameHindi);
                        parameters.Add("@MotherNameHindi", studentModel.Basic.MotherNameHindi);
                        parameters.Add("@GuardianNameHindi", studentModel.Basic.GuardianNameHindi);

                        parameters.Add("@BatchId", Utils.ParseGuid(studentModel.Basic.BatchId));
                        parameters.Add("@ABCID", studentModel.Basic.ABCID);
                        parameters.Add("@APARID", studentModel.Basic.APARID);
                        parameters.Add("@LivingHere", studentModel.Basic.LivingHere);

                      


                        var studentId = await connection.ExecuteScalarAsync<Guid>(
                            basicSql,
                            parameters,
                            transaction,
                            commandType: CommandType.StoredProcedure);


                        //await connection.ExecuteAsync(basicSql, studentModel.Basic, transaction);

                        // Set foreign keys for the related tables
                        studentModel.Family.AdmsnNo = Utils.ParseInt(studentModel.Basic.AdmsnNo);
                        studentModel.Family.SchoolCode = studentModel.Basic.SchoolCode;
                        studentModel.Family.StudentId = studentId;
                        studentModel.Other.AdmsnNo = Utils.ParseInt(studentModel.Basic.AdmsnNo);
                        studentModel.Other.SchoolCode = studentModel.Basic.TenantCode;
                        studentModel.Other.StudentId = studentId;
                        studentModel.Other.TenantCode = studentModel.Basic.TenantCode;
                        studentModel.Other.TenantId = studentModel.Basic.TenantId;
                        studentModel.Other.SessionId = studentModel.Basic.SessionId;
                        studentModel.Other.CreatedBy = studentModel.Basic.CreatedBy;

                        studentModel.Family.TenantCode = studentModel.Basic.TenantCode;
                        studentModel.Family.TenantId = studentModel.Basic.TenantId;
                        studentModel.Family.SessionId = studentModel.Basic.SessionId;
                        studentModel.Family.CreatedBy = studentModel.Basic.CreatedBy;
                        studentModel.Family.VehicleId = Utils.ParseGuid(studentModel.Family.VehicleNumber); 
                        studentModel.Family.RouteId = Utils.ParseGuid(studentModel.Family.Route); 
                        studentModel.Family.PickUpId = Utils.ParseGuid(studentModel.Basic.PickupPoint); 

                        // Insert StudentFamily record
                        var familySql = @"
                INSERT INTO dbo.StudentInfoFamily (
                    AdmsnNo, SchoolCode, FName, FPhone, FOccupation, FAadhar, FNote, FPhoto, MName, MPhone, 
                    MOccupation, MAadhar, MNote, MPhoto, GName, GRelation, GEmail, GPhoto, GPhone, GOccupation, 
                    GAddress, GRemark, StCurrentAddress, StPermanentAddress, Route, HostelDetail, HostelNo,
                    FEmail, MEmail, IsSiblingInSameSchool, TransportNeeded, VehicleNumber, HostelNeeded,
                    FEducation, MEducation, GEducation,StudentId,TenantCode,TenantId,SessionId,CreatedBy,VehicleId,RouteId,PickUpId
                ) VALUES (
                    @AdmsnNo, @SchoolCode, @FName, @FPhone, @FOccupation, @FAadhar, @FNote, @FPhoto, @MName, @MPhone, 
                    @MOccupation, @MAadhar, @MNote, @MPhoto, @GName, @GRelation, @GEmail, @GPhoto, @GPhone, @GOccupation, 
                    @GAddress, @GRemark, @StCurrentAddress, @StPermanentAddress, @Route, @HostelDetail, @HostelNo,
                    @FEmail, @MEmail, @IsSiblingInSameSchool, @TransportNeeded, @VehicleNumber, @HostelNeeded,
                    @FEducation, @MEducation, @GEducation,@StudentId,@TenantCode,@TenantId,@SessionId,@CreatedBy,@VehicleId,@RouteId,@PickUpId
                )";

                        await connection.ExecuteAsync(familySql, studentModel.Family, transaction);

                        // Set foreign keys
                        studentModel.Other.AdmsnNo = Utils.ParseInt(studentModel.Basic.AdmsnNo);
                        studentModel.Other.SchoolCode = studentModel.Basic.SchoolCode;

                        // Insert StudentOther record
                        var otherSql = @"
                INSERT INTO dbo.StudentInfoOther (
                    AdmsnNo, SchoolCode, BankAcNo, BankName, IfscCode, NADID, IDentityLocal, IdentityOther, 
                    PreviousSchoolDtl, Note, UploadTitle1, UpldPath1, UploadTitle2, UpldPath2, UploadTitle3, 
                    UpldPath3, UploadTitle4, UpldPath4, UploadTitle5, UpldPath5, UploadTitle6, UpldPath6,
                    MedicalCondition, Allergies, Medications, BankBranch, OtherInformation, PreviousSchoolAddress,
                    UdiseCode,SchoolNote,TenantCode,TenantId,SessionId,StudentId,CreatedBy
                ) VALUES (
                    @AdmsnNo, @SchoolCode, @BankAcNo, @BankName, @IfscCode, @NADID, @IDentityLocal, @IdentityOther, 
                    @PreviousSchoolDtl, @Note, @UploadTitle1, @UpldPath1, @UploadTitle2, @UpldPath2, @UploadTitle3, 
                    @UpldPath3, @UploadTitle4, @UpldPath4, @UploadTitle5, @UpldPath5, @UploadTitle6, @UpldPath6,
                    @MedicalCondition, @Allergies, @Medications, @BankBranch, @OtherInformation, @PreviousSchoolAddress,
                    @UdiseCode,@SchoolNote,@TenantCode,@TenantId,@SessionId,@StudentId,@CreatedBy
                )";
                        await connection.ExecuteAsync(otherSql, studentModel.Other, transaction);

                        // Handle Subject selections
                        if (studentModel.Basic.Subjects != null && studentModel.Basic.Subjects.Any())
                        {
                            foreach (var subject in studentModel.Basic.Subjects)
                            {
                                var subjectDetails = subject.KeyValue;
                                string[] subdt= subjectDetails.Split('|');
                                var subjectSql = @"
                        INSERT INTO dbo.StudentInfoSubjects (
                            AdmsnNo, SchoolCode, SubjectId, Name, IsElective, TeacherName, IsSelected,StudentId,TenantCode,TenantId,SessionId,CreatedBy
                        ) VALUES (
                            @AdmsnNo, @SchoolCode, @SubjectId, @Name, @IsElective, @TeacherName, @IsSelected,@StudentId,@TenantCode,@TenantId,@SessionId,@CreatedBy
                        )";

                                await connection.ExecuteAsync(subjectSql, new
                                {
                                    AdmsnNo = studentModel.Basic.AdmsnNo,
                                    SchoolCode = studentModel.Basic.SchoolCode,
                                    SubjectId = subdt[0],
                                    Name = subdt[1],
                                    IsElective = subject.IsElective,
                                    TeacherName = subject.TeacherName,
                                    IsSelected = subject.IsSelected,
                                    SessionId= studentModel.Basic.SessionId,
                                    StudentId= studentModel.Family.StudentId,
                                    TenantCode =studentModel.Basic.TenantCode,
                                    TenantId=studentModel.Basic.TenantId,
                                    CreatedBy = studentModel.Basic.CreatedBy
                            }, transaction);
                            }
                        }

                        // Handle Siblings
                        if (studentModel.Family.Siblings != null && studentModel.Family.Siblings.Any())
                        {
                            foreach (var sibling in studentModel.Family.Siblings)
                            {
                                // Skip empty sibling entries
                                if (sibling == null || string.IsNullOrEmpty(sibling.AdmissionNo.ToString()))
                                    continue;

                                var siblingSql = @"
                        INSERT INTO dbo.StudentInfoSiblings (
                            AdmsnNo, SchoolCode, Name, RollNo, AdmissionNo, Class, FatherName, FatherAadharNo,StudentId,TenantCode,TenantId,SessionId,CreatedBy
                        ) VALUES (
                            @AdmsnNo, @SchoolCode, @Name, @RollNo, @AdmissionNo, @Class, @FatherName, @FatherAadharNo,@StudentId,@TenantCode,@TenantId,@SessionId,@CreatedBy
                        )";

                                await connection.ExecuteAsync(siblingSql, new
                                {
                                    AdmsnNo = studentModel.Basic.AdmsnNo,
                                    SchoolCode = studentModel.Basic.SchoolCode,
                                    SessionId = studentModel.Basic.SessionId,
                                    StudentId = studentModel.Family.StudentId,
                                    TenantCode = studentModel.Basic.TenantCode,
                                    TenantId = studentModel.Basic.TenantId,
                                    CreatedBy = studentModel.Basic.CreatedBy,
                                    Name = sibling.Name,
                                    RollNo = sibling.RollNo,
                                    AdmissionNo = sibling.AdmissionNo,
                                    Class = sibling.Class,
                                    FatherName = sibling.FatherName,
                                    FatherAadharNo = sibling.FatherAadharNo
                                }, transaction);
                            }
                        }

                        // Handle Education Details
                        if (studentModel.Other.EducationDetails != null && studentModel.Other.EducationDetails.Any())
                        {
                            foreach (var eduDetail in studentModel.Other.EducationDetails)
                            {
                                // Skip null or invalid education details
                                if (eduDetail == null || string.IsNullOrEmpty(eduDetail.Class))
                                    continue;

                                var eduSql = @"
                        INSERT INTO dbo.StudentInfoEduDetails (
                            AdmsnNo, SchoolCode, Class, RollNo, MaximumMarks, ObtainedMarks, 
                            Board, Subjects, Others, PassingYear, Percentage,StudentId,TenantCode,TenantId,SessionId,CreatedBy
                        ) VALUES (
                            @AdmsnNo, @SchoolCode, @Class, @RollNo, @MaximumMarks, @ObtainedMarks, 
                            @Board, @Subjects, @Others, @PassingYear, @Percentage,@StudentId,@TenantCode,@TenantId,@SessionId,@CreatedBy
                        )";

                                await connection.ExecuteAsync(eduSql, new
                                {
                                    AdmsnNo = studentModel.Basic.AdmsnNo,
                                    SessionId = studentModel.Basic.SessionId,
                                    StudentId = studentModel.Family.StudentId,
                                    TenantCode = studentModel.Basic.TenantCode,
                                    TenantId = studentModel.Basic.TenantId,
                                    CreatedBy = studentModel.Basic.CreatedBy,
                                    SchoolCode = studentModel.Basic.SchoolCode,
                                    Class = eduDetail.Class,
                                    RollNo = eduDetail.RollNo,
                                    MaximumMarks = eduDetail.MaximumMarks,
                                    ObtainedMarks = eduDetail.ObtainedMarks,
                                    Board = eduDetail.Board,
                                    Subjects = eduDetail.Subjects,
                                    Others = eduDetail.Others,
                                    PassingYear = eduDetail.PassingYear,
                                    Percentage = eduDetail.Percentage
                                }, transaction);
                            }
                        }

                        if (studentModel.Family.TransportNeeded)
                        {
                            var transportParams = new DynamicParameters();
                            transportParams.Add("@SessionID", studentModel.Basic.SessionId);
                            transportParams.Add("@StudentID", studentId);
                            transportParams.Add("@SchoolCode", studentModel.Basic.SchoolCode);
                            transportParams.Add("@TenantID", studentModel.Basic.TenantId);
                            transportParams.Add("@TenantCode", studentModel.Basic.TenantCode);
                            transportParams.Add("@CreatedBy", studentModel.Basic.CreatedBy);

                            var transportResult = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                "dbo.InsertStudentTransportConfig",
                                transportParams,
                                transaction,
                                commandType: CommandType.StoredProcedure);

                            // Optionally handle the result
                            if (transportResult != null)
                            {
                                string resultStatus = transportResult.Result;
                                string message = transportResult.Message ?? string.Empty;

                                // Log or handle the transport configuration result if needed
                                System.Diagnostics.Debug.WriteLine($"Transport config result: {resultStatus}, Message: {message}");
                            }
                        }
                        await connection.ExecuteAsync("sp_UpdateStudentDenormalizedFields", new { StudentId = studentId }, transaction, commandType: CommandType.StoredProcedure);
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                       
                        // Detailed exception logging for debugging
                        System.Diagnostics.Debug.WriteLine($"Error saving student: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                        if (ex.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                            System.Diagnostics.Debug.WriteLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
                        }
                        Logger.Error($"Inner Exception: {ex.InnerException.Message}");
                        Logger.Error($"Inner StackTrace: {ex.InnerException.StackTrace}");
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        } // Update an existing student
        public async Task<bool> UpdateStudentAsync(StudentViewModel studentModel)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Set up default values if not provided
                        var basicSql = "sp_UpdateStudentBasic";
                        var parameters = new DynamicParameters();

                        // Required primary keys
                        parameters.Add("@AdmsnNo", studentModel.Basic.AdmsnNo);
                         DateTime? admdate = Utils.SplitAndConvertDate(studentModel.Basic.AdmsnDate);
                        if (admdate.HasValue)
                        {
                            parameters.Add("@AdmsnDate", admdate);
                        }
                        DateTime? dobdate = Utils.SplitAndConvertDate(studentModel.Basic.DOB);
                        if (dobdate.HasValue)
                        {
                            parameters.Add("@DOB", dobdate);
                        }
                        // Student basic info
                        parameters.Add("@StudentNo", studentModel.Basic.StudentNo);
                        parameters.Add("@SrNo", studentModel.Basic.SrNo);
                        parameters.Add("@RollNo", studentModel.Basic.RollNo);
                        parameters.Add("@Class", studentModel.Basic.Class);
                        parameters.Add("@Section", studentModel.Basic.Section);
                        parameters.Add("@FirstName", studentModel.Basic.FirstName);
                        parameters.Add("@LastName", studentModel.Basic.LastName);
                        parameters.Add("@Gender", studentModel.Basic.Gender);
                        parameters.Add("@Category", studentModel.Basic.Category);
                        parameters.Add("@Religion", studentModel.Basic.Religion);
                        parameters.Add("@Caste", studentModel.Basic.Caste);
                        parameters.Add("@Mobile", studentModel.Basic.Mobile);
                        parameters.Add("@WhatsAppNum", studentModel.Basic.WhatsAppNum);
                        parameters.Add("@Email", studentModel.Basic.Email);


                        // For the photo, use the file path rather than the file object

                        parameters.Add("@BloodGroup", studentModel.Basic.BloodGroup);
                        parameters.Add("@House", studentModel.Basic.House);
                        parameters.Add("@Height", studentModel.Basic.Height);
                        parameters.Add("@Weight", studentModel.Basic.Weight);
                        parameters.Add("@AsOnDt", studentModel.Basic.AsOnDt);
                        parameters.Add("@SiblingRef", studentModel.Basic.SiblingRef);
                        parameters.Add("@SiblingRef2", studentModel.Basic.SiblingRef2);
                        parameters.Add("@DiscountCategory", studentModel.Basic.DiscountCategory);
                        parameters.Add("@DiscountNote", studentModel.Basic.DiscountNote);
                        parameters.Add("@LoginPwd", studentModel.Basic.LoginPwd);
                        parameters.Add("@OldBalance", studentModel.Basic.OldBalance);
                        parameters.Add("@FeeCategory", studentModel.Basic.FeeCategory);
                        parameters.Add("@Active", studentModel.Basic.IsActive);
                        parameters.Add("@EnquiryData", studentModel.Basic.EnquiryData);
                        parameters.Add("@SendSMS", studentModel.Basic.SendSMS);
                        parameters.Add("@UserId", studentModel.Basic.UserId);
                        parameters.Add("@AcademicYear", studentModel.Basic.AcademicYear);
                        parameters.Add("@MotherTongue", studentModel.Basic.MotherTongue);
                        parameters.Add("@Status", studentModel.Basic.Status);
                        parameters.Add("@LanguagesKnown", studentModel.Basic.LanguagesKnown);
                        parameters.Add("@AadharNo", studentModel.Basic.AadharNo);
                        parameters.Add("@PENNo", studentModel.Basic.PENNo);
                        parameters.Add("@PickupPoint", studentModel.Basic.PickupPoint);
                        parameters.Add("@LoginId", studentModel.Basic.LoginId);
                        parameters.Add("@Password", studentModel.Basic.Password);

                        // IDs
                        parameters.Add("@ClassId", studentModel.Basic.ClassId);
                        parameters.Add("@SectionId", studentModel.Basic.SectionId);
                        parameters.Add("@HouseId", studentModel.Basic.HouseId);
                        parameters.Add("@FeeCategoryId", studentModel.Basic.FeeCategoryId);
                        parameters.Add("@FeeDiscountId", studentModel.Basic.FeeDiscountId);
                        parameters.Add("@TenantID", studentModel.Basic.TenantId);
                        parameters.Add("@TenantCode", studentModel.Basic.TenantCode);
                        parameters.Add("@SchoolCode", studentModel.Basic.TenantCode);

                        // Default system columns
                        parameters.Add("@IsActive", studentModel.Basic.IsActive);
                        parameters.Add("@IsDeleted", false);
                        parameters.Add("@SessionID", studentModel.Basic.SessionId);
                        parameters.Add("@StudentId", studentModel.Basic.StudentID);
                        // Remove @EntryDate parameter
                        parameters.Add("@ModifiedBy", studentModel.Basic.CreatedBy); // Add this
                        parameters.Add("@Photo", studentModel.Basic.Photo);

                        parameters.Add("@VechileId", studentModel.Basic.VechileId);
                        parameters.Add("@VillegeId", studentModel.Basic.VillegeId);
                        parameters.Add("@RouteId", studentModel.Basic.RouteId);
                        parameters.Add("@PickupId", studentModel.Basic.PickupId);
                        parameters.Add("@HostelId", studentModel.Basic.HostelId);

                        parameters.Add("@StudentNameHindi", studentModel.Basic.StudentNameHindi);
                        parameters.Add("@FatherNameHindi", studentModel.Basic.FatherNameHindi);
                        parameters.Add("@MotherNameHindi", studentModel.Basic.MotherNameHindi);
                        parameters.Add("@GuardianNameHindi", studentModel.Basic.GuardianNameHindi);
                        
                        parameters.Add("@BatchId", Utils.ParseGuid(studentModel.Basic.BatchId));
                        parameters.Add("@ABCID", studentModel.Basic.ABCID);
                        parameters.Add("@APARID", studentModel.Basic.APARID);
                        parameters.Add("@LivingHere", studentModel.Basic.LivingHere);


                        var studentId = await connection.ExecuteScalarAsync<Guid>(
                            basicSql,
                            parameters,
                            transaction,
                            commandType: CommandType.StoredProcedure);


                        //await connection.ExecuteAsync(basicSql, studentModel.Basic, transaction);

                        // Set foreign keys for the related tables
                        studentModel.Family.AdmsnNo = Utils.ParseInt(studentModel.Basic.AdmsnNo);
                        studentModel.Family.SchoolCode = studentModel.Basic.TenantCode;
                        studentModel.Family.StudentId = studentId;
                        studentModel.Family.TenantCode = studentModel.Basic.TenantCode;
                        studentModel.Family.TenantId = studentModel.Basic.TenantId;
                        studentModel.Family.SessionId = studentModel.Basic.SessionId;
                        studentModel.Family.CreatedBy = studentModel.Basic.CreatedBy;
                        studentModel.Family.VehicleId = Utils.ParseGuid(studentModel.Family.VehicleNumber);
                        studentModel.Family.RouteId = Utils.ParseGuid(studentModel.Family.Route);
                        studentModel.Family.PickUpId = Utils.ParseGuid(studentModel.Basic.PickupPoint);

                        studentModel.Other.AdmsnNo = Utils.ParseInt(studentModel.Basic.AdmsnNo);
                        studentModel.Other.SchoolCode = studentModel.Basic.TenantCode;
                        studentModel.Other.StudentId = studentId;
                        studentModel.Other.TenantCode = studentModel.Basic.TenantCode;
                        studentModel.Other.TenantId = studentModel.Basic.TenantId;
                        studentModel.Other.SessionId = studentModel.Basic.SessionId;
                     

                        // Insert StudentFamily record
                        var familySql = @"
                UPDATE dbo.StudentInfoFamily
    SET
        AdmsnNo = @AdmsnNo,
        SchoolCode = @SchoolCode,
        FName = @FName,
        FPhone = @FPhone,
        FOccupation = @FOccupation,
        FAadhar = @FAadhar,
        FNote = @FNote,
        FPhoto = @FPhoto,
        MName = @MName,
        MPhone = @MPhone,
        MOccupation = @MOccupation,
        MAadhar = @MAadhar,
        MNote = @MNote,
        MPhoto = @MPhoto,
        GName = @GName,
        GRelation = @GRelation,
        GEmail = @GEmail,
        GPhoto = @GPhoto,
        GPhone = @GPhone,
        GOccupation = @GOccupation,
        GAddress = @GAddress,
        GRemark = @GRemark,
        StCurrentAddress = @StCurrentAddress,
        StPermanentAddress = @StPermanentAddress,
        Route = @Route,
        HostelDetail = @HostelDetail,
        HostelNo = @HostelNo,
        FEmail = @FEmail,
        MEmail = @MEmail,
        IsSiblingInSameSchool = @IsSiblingInSameSchool,
        TransportNeeded = @TransportNeeded,
        VehicleNumber = @VehicleNumber,
        HostelNeeded = @HostelNeeded,
        FEducation = @FEducation,
        MEducation = @MEducation,
        GEducation = @GEducation,
        TenantCode = @TenantCode,
        TenantId = @TenantId,
        SessionId = @SessionId,
        ModifiedDate = GETDATE(),
        VehicleId = @VehicleId,
        RouteId = @RouteId,
        PickUpId = @PickUpId
    WHERE
        StudentId = @StudentId";

                        await connection.ExecuteAsync(familySql, studentModel.Family, transaction);

                        // Set foreign keys
                        studentModel.Other.AdmsnNo = Utils.ParseInt(studentModel.Basic.AdmsnNo);
                        studentModel.Other.SchoolCode = studentModel.Basic.TenantCode;

                        // Insert StudentOther record
                        var otherSql = @"
                 UPDATE dbo.StudentInfoOther
    SET
        BankAcNo = @BankAcNo,
        BankName = @BankName,
        IfscCode = @IfscCode,
        NADID = @NADID,
        IDentityLocal = @IDentityLocal,
        IdentityOther = @IdentityOther,
        PreviousSchoolDtl = @PreviousSchoolDtl,
        Note = @Note,
        UploadTitle1 = @UploadTitle1,
        UpldPath1 = @UpldPath1,
        UploadTitle2 = @UploadTitle2,
        UpldPath2 = @UpldPath2,
        UploadTitle3 = @UploadTitle3,
        UpldPath3 = @UpldPath3,
        UploadTitle4 = @UploadTitle4,
        UpldPath4 = @UpldPath4,
        UploadTitle5 = @UploadTitle5,
        UpldPath5 = @UpldPath5,
        UploadTitle6 = @UploadTitle6,
        UpldPath6 = @UpldPath6,
        MedicalCondition = @MedicalCondition,
        Allergies = @Allergies,
        Medications = @Medications,
        BankBranch = @BankBranch,
        OtherInformation = @OtherInformation,
        PreviousSchoolAddress = @PreviousSchoolAddress,
        UdiseCode = @UdiseCode,
        SchoolNote = @SchoolNote,
        ModifiedDate = GETDATE()
        WHERE
        StudentId = @StudentId ";
                        await connection.ExecuteAsync(otherSql, studentModel.Other, transaction);

                        // Handle Subject selections
                        if (studentModel.Basic.Subjects != null)
                        {
                            // Get the IDs of selected subjects (if any)
                            var selectedSubjectIds = studentModel.Basic.Subjects
                                .Where(s => s.IsSelected && s.KeyValue != null)
                                .Select(s => new Guid(s.KeyValue.Split('|')[0]))
                                .ToArray();

                            // If there are no selected subjects, delete all subjects for this student
                            if (!selectedSubjectIds.Any())
                            {
                                var deleteAllSubjectsSql = @"
            DELETE FROM dbo.StudentInfoSubjects
            WHERE StudentId = @StudentId
            AND TenantCode = @TenantCode
            AND SessionId = @SessionId";

                                await connection.ExecuteAsync(deleteAllSubjectsSql, new
                                {
                                    StudentId = studentModel.Family.StudentId,
                                    TenantCode = studentModel.Basic.TenantCode,
                                    SessionId = studentModel.Basic.SessionId
                                }, transaction);
                            }
                            else
                            {
                                // Process selected subjects
                                foreach (var subject in studentModel.Basic.Subjects.Where(s => s.IsSelected))
                                {
                                    if (subject.KeyValue != null)
                                    {
                                        var subjectDetails = subject.KeyValue;
                                        string[] subdt = subjectDetails.Split('|');

                                        var spSql = "EXEC [dbo].[sp_UpsertStudentSubject] @AdmsnNo, @SchoolCode, @SubjectId, @Name, @IsElective, @TeacherName, @IsSelected, @StudentId, @TenantCode, @TenantId, @SessionId, @CreatedBy";

                                        await connection.ExecuteAsync(spSql, new
                                        {
                                            AdmsnNo = studentModel.Basic.AdmsnNo,
                                            SchoolCode = studentModel.Basic.TenantCode,
                                            SubjectId = new Guid(subdt[0]),
                                            Name = subdt[1],
                                            IsElective = subject.IsElective,
                                            TeacherName = subject.TeacherName ?? string.Empty,
                                            IsSelected = subject.IsSelected,
                                            StudentId = studentModel.Family.StudentId,
                                            TenantCode = studentModel.Basic.TenantCode,
                                            TenantId = studentModel.Basic.TenantId,
                                            SessionId = studentModel.Basic.SessionId,
                                            CreatedBy = studentModel.Basic.CreatedBy
                                        }, transaction);
                                    }
                                }

                                // Delete deselected subjects
                                var deselectedSubjectSql = @"
            DELETE FROM dbo.StudentInfoSubjects
            WHERE AdmsnNo = @AdmsnNo 
            AND SchoolCode = @SchoolCode 
            AND StudentId = @StudentId
            AND TenantCode = @TenantCode
            AND SessionId = @SessionId
            AND SubjectId NOT IN @SelectedSubjectIds";

                                await connection.ExecuteAsync(deselectedSubjectSql, new
                                {
                                    AdmsnNo = studentModel.Basic.AdmsnNo,
                                    SchoolCode = studentModel.Basic.TenantCode,
                                    StudentId = studentModel.Family.StudentId,
                                    TenantCode = studentModel.Basic.TenantCode,
                                    SessionId = studentModel.Basic.SessionId,
                                    CreatedBy = studentModel.Basic.CreatedBy,
                                    SelectedSubjectIds = selectedSubjectIds
                                }, transaction);
                            }
                        }

                        // Handle Siblings
                        if (studentModel.Family.Siblings != null && studentModel.Family.Siblings.Any())
                        {
                            foreach (var sibling in studentModel.Family.Siblings)
                            {
                                // Skip empty sibling entries
                                if (sibling == null || string.IsNullOrEmpty(sibling.AdmissionNo))
                                    continue;

                                // Check if this sibling already exists in the database
                                var existingSibling = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                    "SELECT SiblingId FROM dbo.StudentInfoSiblings WHERE AdmissionNo = @AdmissionNo AND StudentId = @StudentId AND TenantId = @TenantId",
                                    new
                                    {
                                        AdmissionNo = sibling.AdmissionNo,
                                        StudentId = studentModel.Family.StudentId,
                                        TenantId = studentModel.Basic.TenantId
                                    },
                                    transaction);

                                if (existingSibling != null)
                                {
                                    // Update existing record
                                    var updateSql = @"
            UPDATE dbo.StudentInfoSiblings SET
                AdmsnNo = @AdmsnNo,
                SchoolCode = @SchoolCode,
                Name = @Name,
                RollNo = @RollNo,
                AdmissionNo = @AdmissionNo,
                Class = @Class,
                FatherName = @FatherName,
                FatherAadharNo = @FatherAadharNo,
                ModifiedBy = @CreatedBy,
                ModifiedDate = GETDATE()
            WHERE SiblingId = @SiblingId AND StudentId = @StudentId AND TenantId = @TenantId";

                                    await connection.ExecuteAsync(updateSql, new
                                    {
                                        SiblingId = sibling.SiblingId,
                                        StudentId = studentModel.Family.StudentId,
                                        TenantId = studentModel.Basic.TenantId,
                                        AdmsnNo = studentModel.Basic.AdmsnNo,
                                        SchoolCode = studentModel.Basic.TenantCode,
                                        Name = sibling.Name,
                                        RollNo = sibling.RollNo,
                                        AdmissionNo = sibling.AdmissionNo,
                                        Class = sibling.Class,
                                        FatherName = sibling.FatherName,
                                        FatherAadharNo = sibling.FatherAadharNo,
                                        CreatedBy = studentModel.Basic.CreatedBy
                                    }, transaction);
                                }
                                else
                                {
                                    // Insert new record
                                    var insertSql = @"
            INSERT INTO dbo.StudentInfoSiblings (
                AdmsnNo, SchoolCode, Name, RollNo, AdmissionNo, Class, FatherName, FatherAadharNo,
                StudentId, TenantCode, TenantId, SessionId, CreatedBy
            ) VALUES (
                @AdmsnNo, @SchoolCode, @Name, @RollNo, @AdmissionNo, @Class, @FatherName, @FatherAadharNo,
                @StudentId, @TenantCode, @TenantId, @SessionId, @CreatedBy
            )";

                                    await connection.ExecuteAsync(insertSql, new
                                    {
                                        AdmsnNo = studentModel.Basic.AdmsnNo,
                                        SchoolCode = studentModel.Basic.TenantCode,
                                        SessionId = studentModel.Basic.SessionId,
                                        StudentId = studentModel.Family.StudentId,
                                        TenantCode = studentModel.Basic.TenantCode,
                                        TenantId = studentModel.Basic.TenantId,
                                        CreatedBy = studentModel.Basic.CreatedBy,
                                        Name = sibling.Name,
                                        RollNo = sibling.RollNo,
                                        AdmissionNo = sibling.AdmissionNo,
                                        Class = sibling.Class,
                                        FatherName = sibling.FatherName,
                                        FatherAadharNo = sibling.FatherAadharNo
                                    }, transaction);
                                }
                            }
                        }

                        // Handle Education Details
                        if (studentModel.Other.EducationDetails != null && studentModel.Other.EducationDetails.Any())
                        {
                            foreach (var eduDetail in studentModel.Other.EducationDetails)
                            {
                                // Skip null or invalid education details
                                if (eduDetail == null || string.IsNullOrEmpty(eduDetail.Class))
                                    continue;

                                // Check if EducationID exists and is greater than zero
                                if (eduDetail.EducationId > 0)
                                {
                                    // Update existing record
                                    var updateSql = @"
UPDATE dbo.StudentInfoEduDetails 
SET Class = @Class, 
    RollNo = @RollNo,
    MaximumMarks = @MaximumMarks, 
    ObtainedMarks = @ObtainedMarks,
    Board = @Board, 
    Subjects = @Subjects, 
    Others = @Others, 
    PassingYear = @PassingYear, 
    Percentage = @Percentage
WHERE EducationID = @EducationID";

                                    await connection.ExecuteAsync(updateSql, new
                                    {
                                        EducationID = eduDetail.EducationId,
                                        AdmsnNo = studentModel.Basic.AdmsnNo,
                                        SchoolCode = studentModel.Basic.TenantCode,
                                        Class = eduDetail.Class,
                                        RollNo = eduDetail.RollNo,
                                        MaximumMarks = eduDetail.MaximumMarks,
                                        ObtainedMarks = eduDetail.ObtainedMarks,
                                        Board = eduDetail.Board,
                                        Subjects = eduDetail.Subjects,
                                        Others = eduDetail.Others,
                                        PassingYear = eduDetail.PassingYear,
                                        Percentage = eduDetail.Percentage
                                    }, transaction);
                                }
                                else
                                {
                                    // Insert new record
                                    var insertSql = @"
INSERT INTO dbo.StudentInfoEduDetails (
    AdmsnNo, SchoolCode, Class, RollNo, MaximumMarks, ObtainedMarks, 
    Board, Subjects, Others, PassingYear, Percentage,StudentId,TenantCode,TenantId,SessionId,CreatedBy
) VALUES (
    @AdmsnNo, @SchoolCode, @Class, @RollNo, @MaximumMarks, @ObtainedMarks, 
    @Board, @Subjects, @Others, @PassingYear, @Percentage,@StudentId,@TenantCode,@TenantId,@SessionId,@CreatedBy
)";
                                    await connection.ExecuteAsync(insertSql, new
                                    {
                                        AdmsnNo = studentModel.Basic.AdmsnNo,
                                        SchoolCode = studentModel.Basic.TenantCode,
                                        SessionId = studentModel.Basic.SessionId,
                                        StudentId = studentModel.Family.StudentId,
                                        TenantCode = studentModel.Basic.TenantCode,
                                        TenantId = studentModel.Basic.TenantId,
                                        CreatedBy = studentModel.Basic.CreatedBy,
                                        Class = eduDetail.Class,
                                        RollNo = eduDetail.RollNo,
                                        MaximumMarks = eduDetail.MaximumMarks,
                                        ObtainedMarks = eduDetail.ObtainedMarks,
                                        Board = eduDetail.Board,
                                        Subjects = eduDetail.Subjects,
                                        Others = eduDetail.Others,
                                        PassingYear = eduDetail.PassingYear,
                                        Percentage = eduDetail.Percentage
                                    }, transaction);
                                }
                            }
                        }

                        if (studentModel.TransportConfig != null && studentModel.Family.TransportNeeded)
                        {
                            var transportParams = new DynamicParameters();
                            transportParams.Add("@SessionID", studentModel.Basic.SessionId);
                            transportParams.Add("@StudentID", studentModel.Basic.StudentID);
                            transportParams.Add("@SchoolCode", studentModel.Basic.TenantCode);
                            transportParams.Add("@TenantID", studentModel.Basic.TenantId);
                            transportParams.Add("@TenantCode", studentModel.Basic.TenantCode);
                            transportParams.Add("@CreatedBy", studentModel.Basic.CreatedBy);

                            // Add the monthly fee parameters
                            transportParams.Add("@AprilFee", studentModel.TransportConfig.AprilFee);
                            transportParams.Add("@MayFee", studentModel.TransportConfig.MayFee);
                            transportParams.Add("@JuneFee", studentModel.TransportConfig.JuneFee);
                            transportParams.Add("@JulyFee", studentModel.TransportConfig.JulyFee);
                            transportParams.Add("@AugustFee", studentModel.TransportConfig.AugustFee);
                            transportParams.Add("@SeptemberFee", studentModel.TransportConfig.SeptemberFee);
                            transportParams.Add("@OctoberFee", studentModel.TransportConfig.OctoberFee);
                            transportParams.Add("@NovemberFee", studentModel.TransportConfig.NovemberFee);
                            transportParams.Add("@DecemberFee", studentModel.TransportConfig.DecemberFee);
                            transportParams.Add("@JanuaryFee", studentModel.TransportConfig.JanuaryFee);
                            transportParams.Add("@FebruaryFee", studentModel.TransportConfig.FebruaryFee);
                            transportParams.Add("@MarchFee", studentModel.TransportConfig.MarchFee);



                            // Call your stored procedure with the parameters
                            var transportResult = await connection.QueryFirstOrDefaultAsync<dynamic>(
                                "dbo.UpdateStudentTransportConfig", // You might need to create this stored procedure
                                transportParams,
                                transaction,
                                commandType: CommandType.StoredProcedure);

                            // Log or handle the result
                            if (transportResult != null)
                            {
                                string resultStatus = transportResult.Result;
                                string message = transportResult.Message ?? string.Empty;
                                System.Diagnostics.Debug.WriteLine($"Transport config result: {resultStatus}, Message: {message}");
                            }
                        }
                        await connection.ExecuteAsync("sp_UpdateStudentDenormalizedFields",new { StudentId = studentId },transaction,commandType: CommandType.StoredProcedure);
                        
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {

                        // Detailed exception logging for debugging
                        System.Diagnostics.Debug.WriteLine($"Error saving student: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                        if (ex.InnerException != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                            System.Diagnostics.Debug.WriteLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
                        }
                        Logger.Error($"Inner Exception: {ex.Message}");
                       
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        } // Update an existing student
        public async Task<StudentBasic> GetStudentDetailsByIdAsync(int admsnNo, int schoolCode)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                const string sql = @"
                    SELECT 
                        sb.AdmsnNo ,
                        sb.StudFullName,
                        sb.RollNo,
                        sb.Class AS Class
                    FROM 
                        dbo.StudentInfoBasic sb
                    WHERE 
                        sb.AdmsnNo = @AdmsnNo AND sb.SchoolCode = @SchoolCode";

                return await connection.QueryFirstOrDefaultAsync<StudentBasic>(sql,
                    new { AdmsnNo = admsnNo, SchoolCode = schoolCode });
            }
        }
        public async Task<List<StudentBasic>> GetFilteredStudentsBySchoolCodeAsync(int schoolCode, string searchTerm,Guid TenantID, Guid SessionID)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                string sql = @"
                SELECT TOP 11 -- Fetch 11 to check if there are more than 10
                    sb.AdmsnNo,
                    sb.FirstName,
                    sb.RollNo,
                    sb.ClassName,
                    sb.FatherName ,
                    sb.FatherAadhar 
                FROM 
                      dbo.vwStudentInfo sb
                WHERE 
                        sb.TenantID=@TenantID      
                    AND sb.SessionID=@SessionID
                    AND sb.SchoolCode = @SchoolCode ";

                // Add search condition if searchTerm is provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += @"
                    AND (
                        sb.FirstName LIKE @SearchPattern
                        OR sb.FatherName LIKE @SearchPattern
                        OR sb.ClassName LIKE @SearchPattern
                        OR CAST(sb.RollNo AS NVARCHAR(50)) LIKE @SearchPattern
                        OR CAST(sb.AdmsnNo AS NVARCHAR(50)) LIKE @SearchPattern
                    ) ";
                }

                sql += " ORDER BY sb.FirstName";

                var parameters = new
                {
                    SchoolCode = schoolCode,
                    TenantID = TenantID,
                    SessionID = SessionID,
                    SearchPattern = !string.IsNullOrWhiteSpace(searchTerm) ? $"%{searchTerm}%" : null
                };

                var students = await connection.QueryAsync<StudentBasic>(sql, parameters);
                return students.Take(10).ToList(); // Limit to 10 results
            }
        }
        public async Task<List<StudentBasic>> GetAllStudentsBySchholCodeAsync(int schoolCode)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                const string sql = @"
                    SELECT 
                        sb.AdmsnNo ,
                        sb.FirstName,
                        sb.RollNo,
                        sb.Class AS Class,
                        sf.FName
                       
                    FROM 
                        dbo.StudentInfoBasic sb
                    LEFT JOIN 
                        dbo.StudentInfoFamily sf ON sb.AdmsnNo = sf.AdmsnNo AND sb.SchoolCode = sf.SchoolCode
                    WHERE 
                        sb.SchoolCode = @SchoolCode
                    ORDER BY 
                        sb.FirstName";

                var students = await connection.QueryAsync<StudentBasic>(sql, new { SchoolCode = schoolCode });
                return students.ToList();
            }
        }
        // Get a student by ID
        public async Task<StudentViewModel> GetStudentByIdAsync(Guid StudentId, Guid CurrentTenantID, Guid CurrentSessionID)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                // Get basic student info
                var student = await connection.QueryFirstOrDefaultAsync<StudentBasic>(
                    "SELECT *,FORMAT(DOB, 'dd/MM/yyyy') as vDOB,FORMAT(AdmsnDate, 'dd/MM/yyyy') as vAdm ,dbo.HasStudentPaidFees(StudentId,TenantID,SessionID) AS IsFeeRecordExist FROM dbo.StudentInfoBasic WHERE StudentId=@StudentId",
                   new { StudentId });
                         
                if (student == null) return null;

                // Get family info
                var family = await connection.QueryFirstOrDefaultAsync<StudentFamily>(
                    "SELECT * FROM dbo.StudentInfoFamily WHERE StudentId=@StudentId",
                    new { StudentId });

                // Get other info
                var other = await connection.QueryFirstOrDefaultAsync<StudentOther>(
                    "SELECT * FROM dbo.StudentInfoOther WHERE StudentId=@StudentId",
                    new { StudentId });

                // Get student's subjects with all available subjects
                var subjects = await connection.QueryAsync<SubjectInfo>(
                   @"SELECT 
                asm.SubjectID,
                asm.SubjectName as Name,
                ISNULL(sis.Id, 0) as Id,
                ISNULL(sis.AdmsnNo, 0) as AdmsnNo,
                ISNULL(sis.SchoolCode, 0) as SchoolCode,
                ISNULL(sis.IsElective, 0) as IsElective,
                sis.TeacherName,
                ISNULL(sis.IsSelected, 0) as IsSelected,
                asm.TenantID,
                asm.IsActive,
                asm.IsDeleted,
                asm.CreatedBy,
                asm.SessionID,
                asm.ModifiedBy,
                asm.ModifiedDate,
                asm.TenantCode,
                @StudentId as StudentId,
                CONVERT(VARCHAR(50), asm.SubjectID) + '|' + asm.SubjectName AS KeyValue
            FROM 
                AcademicSubjectMaster asm
            LEFT JOIN 
                StudentInfoSubjects sis ON sis.SubjectId = asm.SubjectID AND sis.StudentId = @StudentId
            WHERE 
            asm.TenantID = @TenantID 
            AND asm.SessionID = @SessionID 
            AND asm.IsDeleted = 0
            AND asm.IsActive = 1",
                     new { StudentId, TenantID = CurrentTenantID, SessionID = CurrentSessionID });

                // Get siblings
                var siblings = await connection.QueryAsync<SiblingInfo>(
                    "SELECT * FROM dbo.StudentInfoSiblings WHERE StudentId=@StudentId",
                    new { StudentId });

                // Get education details
                var educationDetails = await connection.QueryAsync<EducationDetail>(
                    "SELECT * FROM dbo.StudentInfoEduDetails WHERE StudentId=@StudentId",
                    new { StudentId });

                // Create the model
                var model = new StudentViewModel
                {
                    Basic = student,
                    Family = family ?? new StudentFamily(),
                    Other = other ?? new StudentOther()
                };

                // Set the subjects directly - they already have the correct IsSelected flags from the query
                model.Basic.Subjects = subjects.ToList();

                // Set siblings
                if (siblings.Any())
                {
                    model.Family.Siblings = siblings.ToList();
                }

                // Set education details
                if (educationDetails.Any())
                {
                    model.Other.EducationDetails = educationDetails.ToList();
                }

                return model;
            }
        } // Delete a student by ID
        public async Task<bool> DeleteStudentAsync(Guid admsnNo, int schoolCode)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Delete from StudentOther
                        await connection.ExecuteAsync(
                            "UPDATE StudentInfoOther SET IsDeleted=1 WHERE StudentId = @AdmsnNo AND SchoolCode = @SchoolCode",
                            new { AdmsnNo = admsnNo, SchoolCode = schoolCode }, transaction);

                        // Delete from StudentFamily
                        await connection.ExecuteAsync(
                            "UPDATE dbo.StudentInfoFamily   SET IsDeleted=1 WHERE StudentId = @AdmsnNo AND SchoolCode = @SchoolCode",
                            new { AdmsnNo = admsnNo, SchoolCode = schoolCode }, transaction);

                        // Delete from StudentBasic
                        await connection.ExecuteAsync(
                            "UPDATE  dbo.StudentInfoBasic  SET IsDeleted=1 WHERE StudentId = @AdmsnNo AND SchoolCode = @SchoolCode",
                            new { AdmsnNo = admsnNo, SchoolCode = schoolCode }, transaction);

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Get all students (with basic info only for list views)
        public async Task<IEnumerable<StudentBasic>> GetAllStudentsAsync(int schoolCode, string sessionID, Guid? classId = null, Guid? sectionId = null, string viewType = "Active")
        {
            using (var connection = GetConnection())
            {
                var sql = @"SELECT * FROM vwStudentInfo 
                   WHERE SchoolCode = @SchoolCode 
                   AND SessionID = @SessionID 
                   AND IsDeleted = 0";

                var parameters = new DynamicParameters();
                parameters.Add("@SchoolCode", schoolCode);
                parameters.Add("@SessionID", sessionID);
                if (viewType == "Active")
                {
                    sql += " AND IsActive <> 0";
                }
                else if (viewType == "Discontinued")
                {
                    sql += " AND IsActive = 0";
                }
                // Add optional filters
                if (classId.HasValue)
                {
                    sql += " AND ClassId = @ClassId";
                    parameters.Add("@ClassId", classId.Value);
                }

                if (sectionId.HasValue)
                {
                    sql += " AND SectionId = @SectionId";
                    parameters.Add("@SectionId", sectionId.Value);
                }

                sql += " Order By AdmsnNo";

                return await connection.QueryAsync<StudentBasic>(sql, parameters);
            }
        }
        public async Task<IEnumerable<StudentBasic>> GetAllDeActiveStudentsAsync(int schoolCode, string sessionID)
        {
            using (var connection = GetConnection())
            {
                var sql = "SELECT * FROM vwStudentInfo WHERE SchoolCode = @SchoolCode AND SessionID=@SessionID AND IsActive=0 Order By AdmsnNo";
                return await connection.QueryAsync<StudentBasic>(sql, new { SchoolCode = schoolCode, SessionID = sessionID });
            }
        }

        // For the admission number (existing code)
        public async Task<int> GetNextAdmissionNumberAsync(string schoolCode)
        {
            using (var connection = GetConnection())
            {
                var sql = "SELECT ISNULL(MAX(AdmsnNo), 0) + 1 FROM dbo.StudentInfoBasic WHERE SchoolCode = @SchoolCode";
                return await connection.ExecuteScalarAsync<int>(sql, new { SchoolCode = schoolCode });
            }
        }

        // For the roll number
        public async Task<string> GetNextRollNumberAsync(string schoolCode)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    // This query extracts only records where RollNo is purely numeric
                    // and returns the maximum numeric value + 1
                    var sql = @"
        SELECT ISNULL(MAX(CAST(RollNo AS INT)), 1000) + 1 
        FROM dbo.StudentInfoBasic 
        WHERE SchoolCode = @SchoolCode 
        AND ISNUMERIC(RollNo) = 1";

                    int nextRollNo = Utils.ParseInt(await connection.ExecuteScalarAsync<int>(sql, new { SchoolCode = schoolCode }));
                    return nextRollNo.ToString();
                }
                catch
                {
                    // Fallback to a safe default
                    return "1001";
                }
            }
        }

        // Get the next serial number by finding the maximum numeric value
        public async Task<string> GetNextSerialNumberAsync(string schoolCode)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    // Only consider purely numeric SrNo values
                    var sql = @"
        SELECT ISNULL(MAX(CAST(SrNo AS INT)), 0) + 1 
        FROM dbo.StudentInfoBasic 
        WHERE SchoolCode = @SchoolCode 
        AND ISNUMERIC(SrNo) = 1";

                    int nextSrNo = Utils.ParseInt(await connection.ExecuteScalarAsync<int>(sql, new { SchoolCode = schoolCode }));
                    return nextSrNo.ToString();
                }
                catch
                {
                    // Fallback to a safe default
                    return "1";
                }
            }
        }

        // Add this method to your StudentRepository.cs file

        public bool BulkUpdateStudentsByStudentId(string columnName, List<UpdateItem> updates, Guid ModifiedBy)
        {
            if (string.IsNullOrWhiteSpace(columnName))
                throw new ArgumentException("Column name must be provided", nameof(columnName));

            if (updates == null || updates.Count == 0)
                return false;

            // Define columns that belong to StudentInfoFamily
            var familyColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "FatherName", "FName" },
        { "MotherName", "MName" },
        { "GuardianName", "GName" },
        { "FatherAadhar", "FAadhar" },
        { "MotherAadhar", "MAadhar" },
        { "Address", "StCurrentAddress" },
        { "GuardianMobile", "GPhone" },
        { "MotherMobile", "MPhone" }
    };

            // All other allowed columns for StudentInfoBasic
            var basicColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "AdmsnNo", "AadharNo", "SrNo", "RollNo", "FirstName", "LastName",
        "Mobile", "DOB", "OldBalance", "Gender",
        "BloodGroup", "Category", "Section",
        "Password", "FeeCategory",
        "Height", "Weight", "PENNo", "PreviousSchool",
        "UDISE", "Photo", "IsActive", "VillegeName",
        "AdmsnDate", "VillegeId"  // Added VillegeId to allowed columns (note the spelling)
    };

            // Define columns that belong to StudentInfoOther
            var otherColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "PreviousSchool", "PreviousSchoolDtl" },
        { "UDISE", "UdiseCode" }
    };

            // Check if column is valid
            bool isValidColumn = familyColumns.ContainsKey(columnName)
                        || basicColumns.Contains(columnName)
                        || otherColumns.ContainsKey(columnName);

            if (!isValidColumn)
                throw new ArgumentException($"Invalid column name: {columnName}", nameof(columnName));

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string sql;

                        // SQL to ensure family records exist
                        string ensureFamilyRecordsSql = @"
                INSERT INTO dbo.StudentInfoFamily (StudentId, AdmsnNo, SchoolCode, CreatedBy, CreatedDate, IsActive)
                SELECT b.StudentId, b.AdmsnNo, b.SchoolCode, @ModifiedBy, GETDATE(), 1
                FROM dbo.StudentInfoBasic b
                WHERE b.StudentId = @StudentId 
                  AND NOT EXISTS (
                      SELECT 1 FROM dbo.StudentInfoFamily f 
                      WHERE f.StudentId = b.StudentId
                  )";

                        // SQL to ensure other records exist
                        string ensureOtherRecordsSql = @"
                INSERT INTO dbo.StudentInfoOther (
                    StudentId, AdmsnNo, SchoolCode, TenantID, SessionID, 
                    CreatedBy, CreatedDate, IsActive, IsDeleted, TenantCode
                )
                SELECT 
                    b.StudentId, b.AdmsnNo, b.SchoolCode, b.TenantID, b.SessionID,
                    @ModifiedBy, GETDATE(), 1, 0, b.TenantCode
                FROM dbo.StudentInfoBasic b
                WHERE b.StudentId = @StudentId 
                  AND NOT EXISTS (
                      SELECT 1 FROM dbo.StudentInfoOther o 
                      WHERE o.StudentId = b.StudentId
                  )";

                        bool updateFamilyTable = familyColumns.ContainsKey(columnName);
                        bool updateOtherTable = otherColumns.ContainsKey(columnName);

                        if (updateFamilyTable)
                        {
                            // Get the actual column name in StudentInfoFamily table
                            string actualColumnName = familyColumns[columnName];

                            // Update StudentInfoFamily
                            sql = $@"
                    UPDATE f
                    SET [{actualColumnName}] = @Value,
                        ModifiedDate = GETDATE(),
                        ModifiedBy = @ModifiedBy
                    FROM dbo.StudentInfoFamily f
                    WHERE f.StudentId = @StudentId;

                    -- Also update the denormalized column in StudentInfoBasic
                    UPDATE dbo.StudentInfoBasic
                    SET [{columnName}] = @Value,
                        ModifiedDate = GETDATE(),
                        ModifiedBy = @ModifiedBy
                    WHERE StudentId = @StudentId;";
                        }
                        else if (updateOtherTable)
                        {
                            // Get the actual column name in StudentInfoOther table
                            string actualColumnName = otherColumns[columnName];

                            sql = $@"
                    UPDATE o
                    SET [{actualColumnName}] = @Value,
                        ModifiedDate = GETDATE(),
                        ModifiedBy = @ModifiedBy
                    FROM dbo.StudentInfoOther o
                    WHERE o.StudentId = @StudentId;";
                        }
                        else
                        {
                            // Special handling for VillegeName - update VillegeId in Basic and StPermanentAddress in Family
                            if (string.Equals(columnName, "VillegeName", StringComparison.OrdinalIgnoreCase))
                            {
                                sql = $@"
                        -- Update VillegeId in StudentInfoBasic
                        UPDATE dbo.StudentInfoBasic
                        SET [VillegeId] = @VillegeId,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy
                        WHERE StudentId = @StudentId;

                        -- Update StPermanentAddress in StudentInfoFamily with the village name
                        UPDATE dbo.StudentInfoFamily
                        SET [StPermanentAddress] = @Value,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy
                        WHERE StudentId = @StudentId;";
                            }
                            else
                            {
                                // Update only StudentInfoBasic for other columns
                                var quotedColumn = $"[{columnName}]";
                                sql = $@"
                        UPDATE dbo.StudentInfoBasic
                        SET {quotedColumn} = @Value,
                            ModifiedDate = GETDATE(),
                            ModifiedBy = @ModifiedBy
                        WHERE StudentId = @StudentId;";
                            }
                        }

                        int successCount = 0;
                        var errors = new List<string>();

                        foreach (var update in updates)
                        {
                            if (update == null)
                                continue;

                            try
                            {
                                // Ensure family record exists if updating family table OR if updating VillegeName
                                if (updateFamilyTable || string.Equals(columnName, "VillegeName", StringComparison.OrdinalIgnoreCase))
                                {
                                    var ensureParams = new DynamicParameters();
                                    ensureParams.Add("@StudentId", update.StudentId);
                                    ensureParams.Add("@ModifiedBy", ModifiedBy);

                                    connection.Execute(ensureFamilyRecordsSql, ensureParams, transaction);
                                }

                                // Ensure other record exists if updating other table
                                if (updateOtherTable)
                                {
                                    var ensureParams = new DynamicParameters();
                                    ensureParams.Add("@StudentId", update.StudentId);
                                    ensureParams.Add("@ModifiedBy", ModifiedBy);

                                    connection.Execute(ensureOtherRecordsSql, ensureParams, transaction);
                                }

                                // Convert value based on column type
                                object paramValue = ConvertValueForColumn(columnName, update.Value);

                                var parameters = new DynamicParameters();
                                parameters.Add("@Value", paramValue);
                                parameters.Add("@StudentId", update.StudentId);
                                parameters.Add("@ModifiedBy", ModifiedBy);

                                // Special handling for VillegeName - add VillegeId parameter
                                if (string.Equals(columnName, "VillegeName", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Convert the village name to a GUID for VillegeId
                                    Guid villageId;

                                    // Try to parse if the value is already a GUID string
                                    if (!Guid.TryParse(update.Value, out villageId))
                                    {
                                        // If not a valid GUID, generate a deterministic GUID based on the village name
                                        using (var md5 = System.Security.Cryptography.MD5.Create())
                                        {
                                            byte[] hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(update.Value ?? ""));
                                            villageId = new Guid(hash);
                                        }

                                        // Alternative: Use a lookup to get the actual VillageId from a master table
                                        // villageId = GetVillageIdByName(update.Value, connection, transaction);
                                    }

                                    parameters.Add("@VillegeId", villageId);
                                }

                                var rowsAffected = connection.Execute(sql, parameters, transaction);

                                if (rowsAffected > 0)
                                {
                                    successCount++;
                                }
                                else
                                {
                                    errors.Add($"No student found with StudentId: {update.StudentId}");
                                }
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Error updating StudentId {update.StudentId}: {ex.Message}");
                            }
                        }

                        // If all updates were successful, commit
                        if (errors.Count == 0)
                        {
                            transaction.Commit();
                            return true;
                        }
                        else if (successCount > 0)
                        {
                            // Partial success - commit the successful ones
                            transaction.Commit();

                            // Log errors
                            foreach (var error in errors)
                            {
                                System.Diagnostics.Debug.WriteLine(error);
                            }

                            return true;
                        }
                        else
                        {
                            // No successes, rollback
                            transaction.Rollback();
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"BulkUpdateStudentsByStudentId error: {ex.Message}");
                        throw;
                    }
                }
            }
        }
        // Helper method to get VillageId from a master table (optional)
    }
}