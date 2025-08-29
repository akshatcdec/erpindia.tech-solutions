// TeacherRepository.cs
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERPIndia.TeacherManagement.Models;

namespace ERPIndia.TeacherManagement.Repository
{
    public class TeacherRepository
    {
        private readonly string _connectionString;

        public TeacherRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        }

        public async Task<List<TeacherBasic>> GetAllTeachersAsync(int tenantCode, Guid sessionId,
            Guid? classId, Guid? sectionId, Guid? designationId, string viewType)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT 
                        t.TeacherId,
                        t.TeacherCode,
                        t.FirstName,
                        t.LastName,
                        t.FatherName,
                        t.DesignationName,
                        t.DepartmentName,
                        t.PrimaryContactNumber,
                        t.Email,
                        t.DateOfJoining,
                        t.Status,
                        t.IsActive,
                        t.Photo,
                        t.ClassName,
                        t.SectionName,
                        t.SubjectName,
                        t.Gender,
                        t.BloodGroup,
                        t.CurrentAddress,
                        t.Qualification,
                        t.WorkExperience
                    FROM HR_MST_Teacher t
                    WHERE t.TenantCode = @TenantCode 
                    AND t.SessionId = @SessionId
                    AND ISNULL(t.IsDeleted, 0) = 0
                    AND (@ClassId IS NULL OR t.ClassId = @ClassId)
                    AND (@SectionId IS NULL OR t.SectionId = @SectionId)
                    AND (@DesignationId IS NULL OR t.DesignationId = @DesignationId)
                    ORDER BY t.FirstName, t.LastName";

                var teachers = await connection.QueryAsync<TeacherBasic>(query, new
                {
                    TenantCode = tenantCode,
                    SessionId = sessionId,
                    ClassId = classId,
                    SectionId = sectionId,
                    DesignationId = designationId
                });

                return teachers.ToList();
            }
        }

        public async Task<TeacherViewModel> GetTeacherByIdAsync(Guid teacherId, Guid tenantId, Guid sessionId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var model = new TeacherViewModel();

                // Get basic info
                var basicQuery = "SELECT * FROM HR_MST_Teacher WHERE TeacherId = @TeacherId AND TenantId = @TenantId";
                model.Basic = await connection.QueryFirstOrDefaultAsync<TeacherBasic>(basicQuery,
                    new { TeacherId = teacherId, TenantId = tenantId });

                if (model.Basic == null)
                    return null;

                // Get payroll info
                var payrollQuery = @"SELECT * FROM HR_TBL_TeacherPayroll 
                                    WHERE TeacherId = @TeacherId 
                                    AND IsDeleted = 0 
                                    ORDER BY EffectiveDate DESC";
                model.Payroll = await connection.QueryFirstOrDefaultAsync<TeacherPayroll>(payrollQuery,
                    new { TeacherId = teacherId }) ?? new TeacherPayroll();

                // Get leaves info
                var leavesQuery = @"SELECT * FROM HR_TBL_TeacherLeaves 
                                   WHERE TeacherId = @TeacherId 
                                   AND SessionId = @SessionId 
                                   AND IsDeleted = 0";
                model.Leaves = await connection.QueryFirstOrDefaultAsync<TeacherLeaves>(leavesQuery,
                    new { TeacherId = teacherId, SessionId = sessionId }) ?? new TeacherLeaves();

                // Get bank details
                var bankQuery = @"SELECT * FROM HR_TBL_TeacherBankDetails 
                                 WHERE TeacherId = @TeacherId 
                                 AND IsDeleted = 0";
                model.BankDetails = await connection.QueryFirstOrDefaultAsync<TeacherBankDetails>(bankQuery,
                    new { TeacherId = teacherId }) ?? new TeacherBankDetails();

                // Get social media
                var socialQuery = @"SELECT * FROM HR_TBL_TeacherSocialMedia 
                                   WHERE TeacherId = @TeacherId 
                                   AND IsDeleted = 0";
                model.SocialMedia = await connection.QueryFirstOrDefaultAsync<TeacherSocialMedia>(socialQuery,
                    new { TeacherId = teacherId }) ?? new TeacherSocialMedia();

                // Get documents
                var documentsQuery = @"
                    SELECT 
                        DocumentId,
                        DocumentType,
                        DocumentTitle,
                        DocumentPath,
                        FileSize,
                        MimeType,
                        UploadDate
                    FROM HR_TBL_TeacherDocuments 
                    WHERE TeacherId = @TeacherId 
                    AND IsDeleted = 0";
                model.Documents = await connection.QueryAsync<TeacherDocument>(documentsQuery,
                    new { TeacherId = teacherId });

                return model;
            }
        }

        public async Task<string> GetNextTeacherCodeAsync(int tenantCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT ISNULL(MAX(CAST(TeacherCode AS INT)), 0) + 1 
                    FROM HR_MST_Teacher 
                    WHERE TenantCode = @TenantCode 
                    AND ISNUMERIC(TeacherCode) = 1";

                var nextId = await connection.QueryFirstOrDefaultAsync<int>(query,
                    new { TenantCode = tenantCode });

                return "EMP" + nextId.ToString().PadLeft(6, '0');
            }
        }

        public async Task<string> SaveTeacherAsync(TeacherViewModel model)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Generate new TeacherId if not provided
                        if (model.Basic.TeacherId == Guid.Empty)
                            model.Basic.TeacherId = Guid.NewGuid();

                        // Generate TeacherCode if not provided
                        if (string.IsNullOrEmpty(model.Basic.TeacherCode))
                            model.Basic.TeacherCode = await GetNextTeacherCodeAsync(model.Basic.TenantCode);

                        // Parse date fields
                        DateTime? dateOfJoining = ParseDate(model.Basic.DateOfJoining);
                        DateTime? dateOfBirth = ParseDate(model.Basic.DateOfBirth);

                        // Handle nullable GUIDs
                        Guid? classId = ParseNullableGuid(model.Basic.ClassId);
                        Guid? sectionId = ParseNullableGuid(model.Basic.SectionId);
                        Guid? subjectId = ParseNullableGuid(model.Basic.SubjectId);
                        Guid? routeId = ParseNullableGuid(model.Basic.RouteId);
                        Guid? vehicleId = ParseNullableGuid(model.Basic.VehicleId);
                        Guid? pickupId = ParseNullableGuid(model.Basic.PickupId);
                        Guid? hostelId = ParseNullableGuid(model.Basic.HostelId);
                        Guid? designationId = ParseNullableGuid(model.Basic.DesignationId);
                        Guid? departmentId = ParseNullableGuid(model.Basic.DepartmentId);
                        Guid? employeeTypeId = ParseNullableGuid(model.Basic.EmployeeTypeId);
                        Guid? branchId = ParseNullableGuid(model.Basic.BranchId);
                        Guid? managerId = ParseNullableGuid(model.Basic.ManagerId);

                        // Insert basic info
                        var basicQuery = @"
                            INSERT INTO HR_MST_Teacher (
                                TeacherId, TeacherCode, FirstName, LastName, ClassId, SectionId, SubjectId,
                                Gender, PrimaryContactNumber, Email, BloodGroup, DateOfJoining,
                                FatherName, MotherName, DateOfBirth, MaritalStatus, LanguagesKnown,
                                Qualification, WorkExperience, PreviousSchool, PreviousSchoolAddress,
                                PreviousSchoolPhone, CurrentAddress, PermanentAddress, PANNumber,
                                AadharNumber, Status, Notes, Photo, LoginId, Password,
                                RouteId, VehicleId, PickupId, HostelId, RoomNo, Religion,
                                ExperienceDetails, TimeIn, TimeOut, DesignationId, DepartmentId,
                                EmployeeTypeId, BranchId, ManagerId, UANNo, NPSNo, PFNO,
                                DesignationName, DepartmentName, EmployeeTypeName, BranchName,
                                ManagerName, ClassName, SectionName, SubjectName, RouteName,
                                VehicleName, PickupName, HostelName, SchoolCode, TenantId,
                                TenantCode, SessionId, IsActive, IsDeleted, CreatedBy, CreatedDate,
                                OtherSubject
                            ) VALUES (
                                @TeacherId, @TeacherCode, @FirstName, @LastName, @ClassId, @SectionId, @SubjectId,
                                @Gender, @PrimaryContactNumber, @Email, @BloodGroup, @DateOfJoining,
                                @FatherName, @MotherName, @DateOfBirth, @MaritalStatus, @LanguagesKnown,
                                @Qualification, @WorkExperience, @PreviousSchool, @PreviousSchoolAddress,
                                @PreviousSchoolPhone, @CurrentAddress, @PermanentAddress, @PANNumber,
                                @AadharNumber, @Status, @Notes, @Photo, @LoginId, @Password,
                                @RouteId, @VehicleId, @PickupId, @HostelId, @RoomNo, @Religion,
                                @ExperienceDetails, @TimeIn, @TimeOut, @DesignationId, @DepartmentId,
                                @EmployeeTypeId, @BranchId, @ManagerId, @UANNo, @NPSNo, @PFNO,
                                @DesignationName, @DepartmentName, @EmployeeTypeName, @BranchName,
                                @ManagerName, @ClassName, @SectionName, @SubjectName, @RouteName,
                                @VehicleName, @PickupName, @HostelName, @SchoolCode, @TenantId,
                                @TenantCode, @SessionId, @IsActive, 0, @CreatedBy, GETDATE(),
                                @OtherSubject
                            )";

                        await connection.ExecuteAsync(basicQuery, new
                        {
                            model.Basic.TeacherId,
                            model.Basic.TeacherCode,
                            model.Basic.FirstName,
                            model.Basic.LastName,
                            ClassId = classId,
                            SectionId = sectionId,
                            SubjectId = subjectId,
                            model.Basic.Gender,
                            model.Basic.PrimaryContactNumber,
                            model.Basic.Email,
                            model.Basic.BloodGroup,
                            DateOfJoining = dateOfJoining,
                            model.Basic.FatherName,
                            model.Basic.MotherName,
                            DateOfBirth = dateOfBirth,
                            model.Basic.MaritalStatus,
                            model.Basic.LanguagesKnown,
                            model.Basic.Qualification,
                            model.Basic.WorkExperience,
                            model.Basic.PreviousSchool,
                            model.Basic.PreviousSchoolAddress,
                            model.Basic.PreviousSchoolPhone,
                            model.Basic.CurrentAddress,
                            model.Basic.PermanentAddress,
                            model.Basic.PANNumber,
                            model.Basic.AadharNumber,
                            Status = string.IsNullOrEmpty(model.Basic.Status) ? "Active" : model.Basic.Status,
                            model.Basic.Notes,
                            model.Basic.Photo,
                            model.Basic.LoginId,
                            model.Basic.Password,
                            RouteId = routeId,
                            VehicleId = vehicleId,
                            PickupId = pickupId,
                            HostelId = hostelId,
                            model.Basic.RoomNo,
                            model.Basic.Religion,
                            model.Basic.ExperienceDetails,
                            model.Basic.TimeIn,
                            model.Basic.TimeOut,
                            DesignationId = designationId,
                            DepartmentId = departmentId,
                            EmployeeTypeId = employeeTypeId,
                            BranchId = branchId,
                            ManagerId = managerId,
                            model.Basic.UANNo,
                            model.Basic.NPSNo,
                            model.Basic.PFNO,
                            model.Basic.DesignationName,
                            model.Basic.DepartmentName,
                            model.Basic.EmployeeTypeName,
                            model.Basic.BranchName,
                            model.Basic.ManagerName,
                            model.Basic.ClassName,
                            model.Basic.SectionName,
                            model.Basic.SubjectName,
                            model.Basic.RouteName,
                            model.Basic.VehicleName,
                            model.Basic.PickupName,
                            model.Basic.HostelName,
                            model.Basic.SchoolCode,
                            model.Basic.TenantId,
                            model.Basic.TenantCode,
                            model.Basic.SessionId,
                            model.Basic.IsActive,
                            model.Basic.CreatedBy,
                            model.Basic.OtherSubject
                        }, transaction);

                        // Insert payroll info
                        if (model.Payroll != null && model.Payroll.BasicSalary > 0)
                        {
                            await InsertPayroll(connection, transaction, model);
                        }

                        // Insert leaves info
                        if (model.Leaves != null)
                        {
                            await InsertLeaves(connection, transaction, model);
                        }

                        // Insert bank details
                        if (model.BankDetails != null && !string.IsNullOrWhiteSpace(model.BankDetails.AccountNumber))
                        {
                            await InsertBankDetails(connection, transaction, model);
                        }

                        // Insert social media
                        if (model.SocialMedia != null && HasSocialMediaData(model.SocialMedia))
                        {
                            await InsertSocialMedia(connection, transaction, model);
                        }

                        // Insert documents
                        if (model.Documents != null && model.Documents.Any())
                        {
                            await InsertDocuments(connection, transaction, model);
                        }

                        transaction.Commit();
                        return model.Basic.TeacherId.ToString();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error saving teacher: {ex.Message}", ex);
                    }
                }
            }
        }

        public async Task UpdateTeacherAsync(TeacherViewModel model)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Parse date fields
                        DateTime? dateOfJoining = ParseDate(model.Basic.DateOfJoining);
                        DateTime? dateOfBirth = ParseDate(model.Basic.DateOfBirth);

                        // Handle nullable GUIDs
                        Guid? classId = ParseNullableGuid(model.Basic.ClassId);
                        Guid? sectionId = ParseNullableGuid(model.Basic.SectionId);
                        Guid? subjectId = ParseNullableGuid(model.Basic.SubjectId);
                        Guid? routeId = ParseNullableGuid(model.Basic.RouteId);
                        Guid? vehicleId = ParseNullableGuid(model.Basic.VehicleId);
                        Guid? pickupId = ParseNullableGuid(model.Basic.PickupId);
                        Guid? hostelId = ParseNullableGuid(model.Basic.HostelId);
                        Guid? designationId = ParseNullableGuid(model.Basic.DesignationId);
                        Guid? departmentId = ParseNullableGuid(model.Basic.DepartmentId);
                        Guid? employeeTypeId = ParseNullableGuid(model.Basic.EmployeeTypeId);
                        Guid? branchId = ParseNullableGuid(model.Basic.BranchId);
                        Guid? managerId = ParseNullableGuid(model.Basic.ManagerId);

                        // Update basic info
                        var basicQuery = @"
                            UPDATE HR_MST_Teacher SET
                                FirstName = @FirstName, LastName = @LastName,
                                ClassId = @ClassId, SectionId = @SectionId, SubjectId = @SubjectId,
                                Gender = @Gender, PrimaryContactNumber = @PrimaryContactNumber,
                                Email = @Email, BloodGroup = @BloodGroup, DateOfJoining = @DateOfJoining,
                                FatherName = @FatherName, MotherName = @MotherName,
                                DateOfBirth = @DateOfBirth, MaritalStatus = @MaritalStatus,
                                LanguagesKnown = @LanguagesKnown, Qualification = @Qualification,
                                WorkExperience = @WorkExperience, PreviousSchool = @PreviousSchool,
                                PreviousSchoolAddress = @PreviousSchoolAddress,
                                PreviousSchoolPhone = @PreviousSchoolPhone,
                                CurrentAddress = @CurrentAddress, PermanentAddress = @PermanentAddress,
                                PANNumber = @PANNumber, AadharNumber = @AadharNumber,
                                Status = @Status, Notes = @Notes, Photo = ISNULL(@Photo, Photo),
                                RouteId = @RouteId, VehicleId = @VehicleId,
                                PickupId = @PickupId, HostelId = @HostelId, RoomNo = @RoomNo,
                                Religion = @Religion, ExperienceDetails = @ExperienceDetails,
                                TimeIn = @TimeIn, TimeOut = @TimeOut,
                                DesignationId = @DesignationId, DepartmentId = @DepartmentId,
                                EmployeeTypeId = @EmployeeTypeId, BranchId = @BranchId,
                                ManagerId = @ManagerId, UANNo = @UANNo, NPSNo = @NPSNo, PFNO = @PFNO,
                                DesignationName = @DesignationName, DepartmentName = @DepartmentName,
                                EmployeeTypeName = @EmployeeTypeName, BranchName = @BranchName,
                                ManagerName = @ManagerName, ClassName = @ClassName,
                                SectionName = @SectionName, SubjectName = @SubjectName,
                                RouteName = @RouteName, VehicleName = @VehicleName,
                                PickupName = @PickupName, HostelName = @HostelName,
                                OtherSubject = @OtherSubject,
                                ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
                            WHERE TeacherId = @TeacherId AND TenantId = @TenantId";

                        await connection.ExecuteAsync(basicQuery, new
                        {
                            model.Basic.TeacherId,
                            model.Basic.TenantId,
                            model.Basic.FirstName,
                            model.Basic.LastName,
                            ClassId = classId,
                            SectionId = sectionId,
                            SubjectId = subjectId,
                            model.Basic.Gender,
                            model.Basic.PrimaryContactNumber,
                            model.Basic.Email,
                            model.Basic.BloodGroup,
                            DateOfJoining = dateOfJoining,
                            model.Basic.FatherName,
                            model.Basic.MotherName,
                            DateOfBirth = dateOfBirth,
                            model.Basic.MaritalStatus,
                            model.Basic.LanguagesKnown,
                            model.Basic.Qualification,
                            model.Basic.WorkExperience,
                            model.Basic.PreviousSchool,
                            model.Basic.PreviousSchoolAddress,
                            model.Basic.PreviousSchoolPhone,
                            model.Basic.CurrentAddress,
                            model.Basic.PermanentAddress,
                            model.Basic.PANNumber,
                            model.Basic.AadharNumber,
                            model.Basic.Status,
                            model.Basic.Notes,
                            model.Basic.Photo,
                            RouteId = routeId,
                            VehicleId = vehicleId,
                            PickupId = pickupId,
                            HostelId = hostelId,
                            model.Basic.RoomNo,
                            model.Basic.Religion,
                            model.Basic.ExperienceDetails,
                            model.Basic.TimeIn,
                            model.Basic.TimeOut,
                            DesignationId = designationId,
                            DepartmentId = departmentId,
                            EmployeeTypeId = employeeTypeId,
                            BranchId = branchId,
                            ManagerId = managerId,
                            model.Basic.UANNo,
                            model.Basic.NPSNo,
                            model.Basic.PFNO,
                            model.Basic.DesignationName,
                            model.Basic.DepartmentName,
                            model.Basic.EmployeeTypeName,
                            model.Basic.BranchName,
                            model.Basic.ManagerName,
                            model.Basic.ClassName,
                            model.Basic.SectionName,
                            model.Basic.SubjectName,
                            model.Basic.RouteName,
                            model.Basic.VehicleName,
                            model.Basic.PickupName,
                            model.Basic.HostelName,
                            model.Basic.OtherSubject,
                            model.Basic.ModifiedBy
                        }, transaction);

                        // Update or insert related records
                        await UpdateOrInsertPayroll(connection, transaction, model);
                        await UpdateOrInsertLeaves(connection, transaction, model);
                        await UpdateOrInsertBankDetails(connection, transaction, model);
                        await UpdateOrInsertSocialMedia(connection, transaction, model);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error updating teacher: {ex.Message}", ex);
                    }
                }
            }
        }

        public async Task DeleteTeacherAsync(Guid teacherId, int tenantCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Soft delete main teacher record
                        var query = @"
                            UPDATE HR_MST_Teacher 
                            SET IsDeleted = 1, ModifiedDate = GETDATE() 
                            WHERE TeacherId = @TeacherId AND TenantCode = @TenantCode";

                        await connection.ExecuteAsync(query,
                            new { TeacherId = teacherId, TenantCode = tenantCode }, transaction);

                        // Soft delete related records
                        await connection.ExecuteAsync(
                            "UPDATE HR_TBL_TeacherPayroll SET IsDeleted = 1 WHERE TeacherId = @TeacherId",
                            new { TeacherId = teacherId }, transaction);

                        await connection.ExecuteAsync(
                            "UPDATE HR_TBL_TeacherLeaves SET IsDeleted = 1 WHERE TeacherId = @TeacherId",
                            new { TeacherId = teacherId }, transaction);

                        await connection.ExecuteAsync(
                            "UPDATE HR_TBL_TeacherBankDetails SET IsDeleted = 1 WHERE TeacherId = @TeacherId",
                            new { TeacherId = teacherId }, transaction);

                        await connection.ExecuteAsync(
                            "UPDATE HR_TBL_TeacherSocialMedia SET IsDeleted = 1 WHERE TeacherId = @TeacherId",
                            new { TeacherId = teacherId }, transaction);

                        await connection.ExecuteAsync(
                            "UPDATE HR_TBL_TeacherDocuments SET IsDeleted = 1 WHERE TeacherId = @TeacherId",
                            new { TeacherId = teacherId }, transaction);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception($"Error deleting teacher: {ex.Message}", ex);
                    }
                }
            }
        }

        // Helper methods
        private DateTime? ParseDate(object dateValue)
        {
            if (dateValue == null)
                return null;

            string dateString = dateValue.ToString();
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            // Try different date formats
            string[] formats = { "dd/MM/yyyy", "yyyy-MM-dd", "MM/dd/yyyy", "dd-MM-yyyy" };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }

            // Try default parse as last resort
            if (DateTime.TryParse(dateString, out DateTime defaultResult))
                return defaultResult;

            return null;
        }

        private Guid? ParseNullableGuid(object guidValue)
        {
            if (guidValue == null)
                return null;

            string guidString = guidValue.ToString();
            if (string.IsNullOrWhiteSpace(guidString))
                return null;

            if (Guid.TryParse(guidString, out Guid result) && result != Guid.Empty)
                return result;

            return null;
        }

        private bool HasSocialMediaData(TeacherSocialMedia socialMedia)
        {
            return !string.IsNullOrWhiteSpace(socialMedia.Facebook) ||
                   !string.IsNullOrWhiteSpace(socialMedia.Instagram) ||
                   !string.IsNullOrWhiteSpace(socialMedia.LinkedIn) ||
                   !string.IsNullOrWhiteSpace(socialMedia.YouTube) ||
                   !string.IsNullOrWhiteSpace(socialMedia.Twitter);
        }

        private async Task InsertPayroll(SqlConnection connection, SqlTransaction transaction, TeacherViewModel model)
        {
            model.Payroll.PayrollId = Guid.NewGuid();
            model.Payroll.TeacherId = model.Basic.TeacherId;

            DateTime? dateOfLeaving = ParseDate(model.Payroll.DateOfLeaving);
            DateTime? effectiveDate = ParseDate(model.Payroll.EffectiveDate) ?? DateTime.Today;
            DateTime? endDate = ParseDate(model.Payroll.EndDate);

            var payrollQuery = @"
                INSERT INTO HR_TBL_TeacherPayroll (
                    PayrollId, TeacherId, EPFNo, BasicSalary, ContractType,
                    WorkShift, WorkLocation, DateOfLeaving, LateFinePerHour,
                    PayrollNote, EffectiveDate, EndDate, SchoolCode, TenantId,
                    TenantCode, IsActive, IsDeleted, CreatedBy, CreatedDate
                ) VALUES (
                    @PayrollId, @TeacherId, @EPFNo, @BasicSalary, @ContractType,
                    @WorkShift, @WorkLocation, @DateOfLeaving, @LateFinePerHour,
                    @PayrollNote, @EffectiveDate, @EndDate, @SchoolCode, @TenantId,
                    @TenantCode, 1, 0, @CreatedBy, GETDATE()
                )";

            await connection.ExecuteAsync(payrollQuery, new
            {
                model.Payroll.PayrollId,
                model.Payroll.TeacherId,
                model.Payroll.EPFNo,
                model.Payroll.BasicSalary,
                model.Payroll.ContractType,
                model.Payroll.WorkShift,
                model.Payroll.WorkLocation,
                DateOfLeaving = dateOfLeaving,
                model.Payroll.LateFinePerHour,
                model.Payroll.PayrollNote,
                EffectiveDate = effectiveDate,
                EndDate = endDate,
                model.Basic.SchoolCode,
                model.Basic.TenantId,
                model.Basic.TenantCode,
                model.Basic.CreatedBy
            }, transaction);
        }

        private async Task InsertLeaves(SqlConnection connection, SqlTransaction transaction, TeacherViewModel model)
        {
            model.Leaves.LeaveId = Guid.NewGuid();
            model.Leaves.TeacherId = model.Basic.TeacherId;

            var leavesQuery = @"
                INSERT INTO HR_TBL_TeacherLeaves (
                    LeaveId, TeacherId, SessionId, MedicalLeaves, CasualLeaves,
                    MaternityLeaves, SickLeaves, EarnedLeaves, SchoolCode,
                    TenantId, TenantCode, IsActive, IsDeleted, CreatedBy, CreatedDate
                ) VALUES (
                    @LeaveId, @TeacherId, @SessionId, @MedicalLeaves, @CasualLeaves,
                    @MaternityLeaves, @SickLeaves, @EarnedLeaves, @SchoolCode,
                    @TenantId, @TenantCode, 1, 0, @CreatedBy, GETDATE()
                )";

            await connection.ExecuteAsync(leavesQuery, new
            {
                model.Leaves.LeaveId,
                model.Leaves.TeacherId,
                model.Basic.SessionId,
                model.Leaves.MedicalLeaves,
                model.Leaves.CasualLeaves,
                model.Leaves.MaternityLeaves,
                model.Leaves.SickLeaves,
                model.Leaves.EarnedLeaves,
                model.Basic.SchoolCode,
                model.Basic.TenantId,
                model.Basic.TenantCode,
                model.Basic.CreatedBy
            }, transaction);
        }

        private async Task InsertBankDetails(SqlConnection connection, SqlTransaction transaction, TeacherViewModel model)
        {
            model.BankDetails.BankId = Guid.NewGuid();
            model.BankDetails.TeacherId = model.Basic.TeacherId;

            var bankQuery = @"
                INSERT INTO HR_TBL_TeacherBankDetails (
                    BankId, TeacherId, AccountName, AccountNumber,
                    BankName, IFSCCode, BranchName, UPIID, SchoolCode,
                    TenantId, TenantCode, IsActive, IsDeleted, CreatedBy, CreatedDate
                ) VALUES (
                    @BankId, @TeacherId, @AccountName, @AccountNumber,
                    @BankName, @IFSCCode, @BranchName, @UPIID, @SchoolCode,
                    @TenantId, @TenantCode, 1, 0, @CreatedBy, GETDATE()
                )";

            await connection.ExecuteAsync(bankQuery, new
            {
                model.BankDetails.BankId,
                model.BankDetails.TeacherId,
                model.BankDetails.AccountName,
                model.BankDetails.AccountNumber,
                model.BankDetails.BankName,
                model.BankDetails.IFSCCode,
                model.BankDetails.BranchName,
                model.BankDetails.UPIID,
                model.Basic.SchoolCode,
                model.Basic.TenantId,
                model.Basic.TenantCode,
                model.Basic.CreatedBy
            }, transaction);
        }

        private async Task InsertSocialMedia(SqlConnection connection, SqlTransaction transaction, TeacherViewModel model)
        {
            model.SocialMedia.SocialMediaId = Guid.NewGuid();
            model.SocialMedia.TeacherId = model.Basic.TeacherId;

            var socialQuery = @"
                INSERT INTO HR_TBL_TeacherSocialMedia (
                    SocialMediaId, TeacherId, Facebook, Instagram,
                    LinkedIn, YouTube, Twitter, SchoolCode,
                    TenantId, TenantCode, IsActive, IsDeleted, CreatedBy, CreatedDate
                ) VALUES (
                    @SocialMediaId, @TeacherId, @Facebook, @Instagram,
                    @LinkedIn, @YouTube, @Twitter, @SchoolCode,
                    @TenantId, @TenantCode, 1, 0, @CreatedBy, GETDATE()
                )";

            await connection.ExecuteAsync(socialQuery, new
            {
                model.SocialMedia.SocialMediaId,
                model.SocialMedia.TeacherId,
                model.SocialMedia.Facebook,
                model.SocialMedia.Instagram,
                model.SocialMedia.LinkedIn,
                model.SocialMedia.YouTube,
                model.SocialMedia.Twitter,
                model.Basic.SchoolCode,
                model.Basic.TenantId,
                model.Basic.TenantCode,
                model.Basic.CreatedBy
            }, transaction);
        }

        private async Task InsertDocuments(SqlConnection connection, SqlTransaction transaction, TeacherViewModel model)
        {
            foreach (var doc in model.Documents)
            {
                var query = @"
                    INSERT INTO HR_TBL_TeacherDocuments (
                        DocumentId, TeacherId, DocumentType, DocumentTitle,
                        DocumentPath, FileSize, MimeType, UploadDate, SchoolCode,
                        TenantId, TenantCode, IsActive, IsDeleted, CreatedBy, CreatedDate
                    ) VALUES (
                        @DocumentId, @TeacherId, @DocumentType, @DocumentTitle,
                        @DocumentPath, @FileSize, @MimeType, GETDATE(), @SchoolCode,
                        @TenantId, @TenantCode, 1, 0, @CreatedBy, GETDATE()
                    )";

                await connection.ExecuteAsync(query, new
                {
                    DocumentId = Guid.NewGuid(),
                    TeacherId = model.Basic.TeacherId,
                    doc.DocumentType,
                    doc.DocumentTitle,
                    doc.DocumentPath,
                    doc.FileSize,
                    doc.MimeType,
                    model.Basic.SchoolCode,
                    model.Basic.TenantId,
                    model.Basic.TenantCode,
                    model.Basic.CreatedBy
                }, transaction);
            }
        }

        private async Task UpdateOrInsertPayroll(SqlConnection connection, SqlTransaction transaction,
            TeacherViewModel model)
        {
            if (model.Payroll == null || model.Payroll.BasicSalary <= 0)
                return;

            var exists = await connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT COUNT(1) FROM HR_TBL_TeacherPayroll WHERE TeacherId = @TeacherId AND IsDeleted = 0",
                new { model.Basic.TeacherId }, transaction);

            DateTime? dateOfLeaving = ParseDate(model.Payroll.DateOfLeaving);
            DateTime? effectiveDate = ParseDate(model.Payroll.EffectiveDate) ?? DateTime.Today;
            DateTime? endDate = ParseDate(model.Payroll.EndDate);

            if (exists)
            {
                var updateQuery = @"
                    UPDATE HR_TBL_TeacherPayroll SET
                        EPFNo = @EPFNo, BasicSalary = @BasicSalary,
                        ContractType = @ContractType, WorkShift = @WorkShift,
                        WorkLocation = @WorkLocation, DateOfLeaving = @DateOfLeaving,
                        LateFinePerHour = @LateFinePerHour, PayrollNote = @PayrollNote,
                        EffectiveDate = @EffectiveDate, EndDate = @EndDate,
                        ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
                    WHERE TeacherId = @TeacherId AND IsDeleted = 0";

                await connection.ExecuteAsync(updateQuery, new
                {
                    model.Payroll.EPFNo,
                    model.Payroll.BasicSalary,
                    model.Payroll.ContractType,
                    model.Payroll.WorkShift,
                    model.Payroll.WorkLocation,
                    DateOfLeaving = dateOfLeaving,
                    model.Payroll.LateFinePerHour,
                    model.Payroll.PayrollNote,
                    EffectiveDate = effectiveDate,
                    EndDate = endDate,
                    model.Basic.TeacherId,
                    model.Basic.ModifiedBy
                }, transaction);
            }
            else
            {
                await InsertPayroll(connection, transaction, model);
            }
        }

        private async Task UpdateOrInsertLeaves(SqlConnection connection, SqlTransaction transaction,
            TeacherViewModel model)
        {
            if (model.Leaves == null)
                return;

            var exists = await connection.QueryFirstOrDefaultAsync<bool>(
                @"SELECT COUNT(1) FROM HR_TBL_TeacherLeaves 
                  WHERE TeacherId = @TeacherId AND SessionId = @SessionId AND IsDeleted = 0",
                new { model.Basic.TeacherId, model.Basic.SessionId }, transaction);

            if (exists)
            {
                var updateQuery = @"
                    UPDATE HR_TBL_TeacherLeaves SET
                        MedicalLeaves = @MedicalLeaves, CasualLeaves = @CasualLeaves,
                        MaternityLeaves = @MaternityLeaves, SickLeaves = @SickLeaves,
                        EarnedLeaves = @EarnedLeaves,
                        ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
                    WHERE TeacherId = @TeacherId AND SessionId = @SessionId AND IsDeleted = 0";

                await connection.ExecuteAsync(updateQuery, new
                {
                    model.Leaves.MedicalLeaves,
                    model.Leaves.CasualLeaves,
                    model.Leaves.MaternityLeaves,
                    model.Leaves.SickLeaves,
                    model.Leaves.EarnedLeaves,
                    model.Basic.TeacherId,
                    model.Basic.SessionId,
                    model.Basic.ModifiedBy
                }, transaction);
            }
            else
            {
                await InsertLeaves(connection, transaction, model);
            }
        }

        private async Task UpdateOrInsertBankDetails(SqlConnection connection, SqlTransaction transaction,
            TeacherViewModel model)
        {
            if (model.BankDetails == null || string.IsNullOrWhiteSpace(model.BankDetails.AccountNumber))
                return;

            var exists = await connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT COUNT(1) FROM HR_TBL_TeacherBankDetails WHERE TeacherId = @TeacherId AND IsDeleted = 0",
                new { model.Basic.TeacherId }, transaction);

            if (exists)
            {
                var updateQuery = @"
                    UPDATE HR_TBL_TeacherBankDetails SET
                        AccountName = @AccountName, AccountNumber = @AccountNumber,
                        BankName = @BankName, IFSCCode = @IFSCCode,
                        BranchName = @BranchName, UPIID = @UPIID,
                        ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
                    WHERE TeacherId = @TeacherId AND IsDeleted = 0";

                await connection.ExecuteAsync(updateQuery, new
                {
                    model.BankDetails.AccountName,
                    model.BankDetails.AccountNumber,
                    model.BankDetails.BankName,
                    model.BankDetails.IFSCCode,
                    model.BankDetails.BranchName,
                    model.BankDetails.UPIID,
                    model.Basic.TeacherId,
                    model.Basic.ModifiedBy
                }, transaction);
            }
            else
            {
                await InsertBankDetails(connection, transaction, model);
            }
        }

        private async Task UpdateOrInsertSocialMedia(SqlConnection connection, SqlTransaction transaction,
            TeacherViewModel model)
        {
            if (model.SocialMedia == null || !HasSocialMediaData(model.SocialMedia))
                return;

            var exists = await connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT COUNT(1) FROM HR_TBL_TeacherSocialMedia WHERE TeacherId = @TeacherId AND IsDeleted = 0",
                new { model.Basic.TeacherId }, transaction);

            if (exists)
            {
                var updateQuery = @"
                    UPDATE HR_TBL_TeacherSocialMedia SET
                        Facebook = @Facebook, Instagram = @Instagram,
                        LinkedIn = @LinkedIn, YouTube = @YouTube, Twitter = @Twitter,
                        ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
                    WHERE TeacherId = @TeacherId AND IsDeleted = 0";

                await connection.ExecuteAsync(updateQuery, new
                {
                    model.SocialMedia.Facebook,
                    model.SocialMedia.Instagram,
                    model.SocialMedia.LinkedIn,
                    model.SocialMedia.YouTube,
                    model.SocialMedia.Twitter,
                    model.Basic.TeacherId,
                    model.Basic.ModifiedBy
                }, transaction);
            }
            else
            {
                await InsertSocialMedia(connection, transaction, model);
            }
        }

        // Additional utility methods for fetching lookup data
        public async Task<IEnumerable<dynamic>> GetDesignationsAsync(int tenantCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"SELECT DesignationID, DesignationName 
                             FROM HR_MST_Designation 
                             WHERE TenantCode = @TenantCode AND IsActive = 1";
                return await connection.QueryAsync(query, new { TenantCode = tenantCode });
            }
        }

        public async Task<IEnumerable<dynamic>> GetDepartmentsAsync(int tenantCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"SELECT DepartmentID, DepartmentName 
                             FROM HR_MST_Department 
                             WHERE TenantCode = @TenantCode AND IsActive = 1";
                return await connection.QueryAsync(query, new { TenantCode = tenantCode });
            }
        }

        public async Task<IEnumerable<dynamic>> GetEmployeeTypesAsync(int tenantCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"SELECT EmployeeTypeID, EmployeeTypeName 
                             FROM HR_MST_EmployeeType 
                             WHERE TenantCode = @TenantCode AND IsActive = 1";
                return await connection.QueryAsync(query, new { TenantCode = tenantCode });
            }
        }

        public async Task<IEnumerable<dynamic>> GetBranchesAsync(int tenantCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"SELECT BranchID, BranchName 
                             FROM HR_MST_Branch 
                             WHERE TenantCode = @TenantCode AND IsActive = 1";
                return await connection.QueryAsync(query, new { TenantCode = tenantCode });
            }
        }

        public async Task<IEnumerable<dynamic>> GetManagersAsync(int tenantCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"SELECT TeacherId as ManagerId, 
                             CONCAT(FirstName, ' ', LastName) as ManagerName 
                             FROM HR_MST_Teacher 
                             WHERE TenantCode = @TenantCode 
                             AND IsActive = 1 
                             AND IsDeleted = 0
                             AND DesignationName IN ('Manager', 'Principal', 'Director', 'Head')";
                return await connection.QueryAsync(query, new { TenantCode = tenantCode });
            }
        }
    }
}