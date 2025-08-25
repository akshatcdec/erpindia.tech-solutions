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

        public async Task<List<TeacherBasic>> GetAllTeachersAsync(int schoolCode, string sessionId,
            Guid? classId, Guid? sectionId, string viewType)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT * FROM TeacherInfoBasic 
                    WHERE SchoolCode = @SchoolCode 
                    AND SessionId = @SessionId
                    AND IsDeleted = 0
                    AND (@ClassId IS NULL OR ClassId = @ClassId)
                    AND (@SectionId IS NULL OR SectionId = @SectionId)
                    AND (@ViewType = 'All' OR Status = @ViewType)
                    ORDER BY FirstName, LastName";

                var teachers = await connection.QueryAsync<TeacherBasic>(query, new
                {
                    SchoolCode = schoolCode,
                    SessionId = sessionId,
                    ClassId = classId,
                    SectionId = sectionId,
                    ViewType = viewType
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
                var basicQuery = "SELECT * FROM TeacherInfoBasic WHERE TeacherId = @TeacherId AND TenantId = @TenantId";
                model.Basic = await connection.QueryFirstOrDefaultAsync<TeacherBasic>(basicQuery,
                    new { TeacherId = teacherId, TenantId = tenantId });

                if (model.Basic == null)
                    return null;

                // Get payroll info
                var payrollQuery = "SELECT * FROM TeacherPayroll WHERE TeacherId = @TeacherId";
                model.Payroll = await connection.QueryFirstOrDefaultAsync<TeacherPayroll>(payrollQuery,
                    new { TeacherId = teacherId }) ?? new TeacherPayroll();

                // Get leaves info
                var leavesQuery = "SELECT * FROM TeacherLeaves WHERE TeacherId = @TeacherId AND SessionId = @SessionId";
                model.Leaves = await connection.QueryFirstOrDefaultAsync<TeacherLeaves>(leavesQuery,
                    new { TeacherId = teacherId, SessionId = sessionId }) ?? new TeacherLeaves();

                // Get bank details
                var bankQuery = "SELECT * FROM TeacherBankDetails WHERE TeacherId = @TeacherId";
                model.BankDetails = await connection.QueryFirstOrDefaultAsync<TeacherBankDetails>(bankQuery,
                    new { TeacherId = teacherId }) ?? new TeacherBankDetails();

                // Get social media
                var socialQuery = "SELECT * FROM TeacherSocialMedia WHERE TeacherId = @TeacherId";
                model.SocialMedia = await connection.QueryFirstOrDefaultAsync<TeacherSocialMedia>(socialQuery,
                    new { TeacherId = teacherId }) ?? new TeacherSocialMedia();

                // Get documents
                var documentsQuery = @"
                    SELECT 
                        MAX(CASE WHEN DocumentType = 'Resume' THEN DocumentPath END) as ResumePath,
                        MAX(CASE WHEN DocumentType = 'JoiningLetter' THEN DocumentPath END) as JoiningLetterPath
                    FROM TeacherDocuments 
                    WHERE TeacherId = @TeacherId";
                model.Documents = await connection.QueryFirstOrDefaultAsync<TeacherDocuments>(documentsQuery,
                    new { TeacherId = teacherId }) ?? new TeacherDocuments();

                return model;
            }
        }

        public async Task<string> GetNextTeacherIdAsync(string schoolCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT ISNULL(MAX(CAST(TeacherCode AS INT)), 0) + 1 
                    FROM TeacherInfoBasic 
                    WHERE SchoolCode = @SchoolCode 
                    AND ISNUMERIC(TeacherCode) = 1";

                var nextId = await connection.QueryFirstOrDefaultAsync<int>(query,
                    new { SchoolCode = schoolCode });

                return nextId.ToString().PadLeft(6, '0');
            }
        }

        public async Task SaveTeacherAsync(TeacherViewModel model)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Generate new TeacherId
                        model.Basic.TeacherId = Guid.NewGuid();

                        // Insert basic info
                        var basicQuery = @"
                            INSERT INTO TeacherInfoBasic (
                                TeacherId, TeacherCode, FirstName, LastName, ClassId, SectionId, SubjectId,
                                Gender, PrimaryContactNumber, Email, BloodGroup, DateOfJoining,
                                FatherName, MotherName, DateOfBirth, MaritalStatus, LanguagesKnown,
                                Qualification, WorkExperience, PreviousSchool, PreviousSchoolAddress,
                                PreviousSchoolPhone, CurrentAddress, PermanentAddress, PANNumber,
                                AadharNumber, Status, Notes, Photo, LoginId, Password,
                                RouteId, VehicleId, PickupId, HostelId, RoomNo,
                                SchoolCode, TenantId, SessionId, CreatedBy, CreatedDate, IsActive
                            ) VALUES (
                                @TeacherId, @TeacherCode, @FirstName, @LastName, @ClassId, @SectionId, @SubjectId,
                                @Gender, @PrimaryContactNumber, @Email, @BloodGroup, @DateOfJoining,
                                @FatherName, @MotherName, @DateOfBirth, @MaritalStatus, @LanguagesKnown,
                                @Qualification, @WorkExperience, @PreviousSchool, @PreviousSchoolAddress,
                                @PreviousSchoolPhone, @CurrentAddress, @PermanentAddress, @PANNumber,
                                @AadharNumber, @Status, @Notes, @Photo, @LoginId, @Password,
                                @RouteId, @VehicleId, @PickupId, @HostelId, @RoomNo,
                                @SchoolCode, @TenantId, @SessionId, @CreatedBy, GETDATE(), @IsActive
                            )";

                        await connection.ExecuteAsync(basicQuery, model.Basic, transaction);

                        // Insert payroll info
                        if (model.Payroll != null)
                        {
                            model.Payroll.PayrollId = Guid.NewGuid();
                            model.Payroll.TeacherId = model.Basic.TeacherId;

                            var payrollQuery = @"
                                INSERT INTO TeacherPayroll (
                                    PayrollId, TeacherId, EPFNo, BasicSalary, ContractType,
                                    WorkShift, WorkLocation, DateOfLeaving, SchoolCode
                                ) VALUES (
                                    @PayrollId, @TeacherId, @EPFNo, @BasicSalary, @ContractType,
                                    @WorkShift, @WorkLocation, @DateOfLeaving, @SchoolCode
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
                                model.Payroll.DateOfLeaving,
                                model.Basic.SchoolCode
                            }, transaction);
                        }

                        // Insert leaves info
                        if (model.Leaves != null)
                        {
                            model.Leaves.LeaveId = Guid.NewGuid();
                            model.Leaves.TeacherId = model.Basic.TeacherId;

                            var leavesQuery = @"
                                INSERT INTO TeacherLeaves (
                                    LeaveId, TeacherId, MedicalLeaves, CasualLeaves,
                                    MaternityLeaves, SickLeaves, SchoolCode, SessionId
                                ) VALUES (
                                    @LeaveId, @TeacherId, @MedicalLeaves, @CasualLeaves,
                                    @MaternityLeaves, @SickLeaves, @SchoolCode, @SessionId
                                )";

                            await connection.ExecuteAsync(leavesQuery, new
                            {
                                model.Leaves.LeaveId,
                                model.Leaves.TeacherId,
                                model.Leaves.MedicalLeaves,
                                model.Leaves.CasualLeaves,
                                model.Leaves.MaternityLeaves,
                                model.Leaves.SickLeaves,
                                model.Basic.SchoolCode,
                                model.Basic.SessionId
                            }, transaction);
                        }

                        // Insert bank details
                        if (model.BankDetails != null && !string.IsNullOrEmpty(model.BankDetails.AccountNumber))
                        {
                            model.BankDetails.BankId = Guid.NewGuid();
                            model.BankDetails.TeacherId = model.Basic.TeacherId;

                            var bankQuery = @"
                                INSERT INTO TeacherBankDetails (
                                    BankId, TeacherId, AccountName, AccountNumber,
                                    BankName, IFSCCode, BranchName, SchoolCode
                                ) VALUES (
                                    @BankId, @TeacherId, @AccountName, @AccountNumber,
                                    @BankName, @IFSCCode, @BranchName, @SchoolCode
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
                                model.Basic.SchoolCode
                            }, transaction);
                        }

                        // Insert social media
                        if (model.SocialMedia != null)
                        {
                            model.SocialMedia.SocialMediaId = Guid.NewGuid();
                            model.SocialMedia.TeacherId = model.Basic.TeacherId;

                            var socialQuery = @"
                                INSERT INTO TeacherSocialMedia (
                                    SocialMediaId, TeacherId, Facebook, Instagram,
                                    LinkedIn, YouTube, Twitter, SchoolCode
                                ) VALUES (
                                    @SocialMediaId, @TeacherId, @Facebook, @Instagram,
                                    @LinkedIn, @YouTube, @Twitter, @SchoolCode
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
                                model.Basic.SchoolCode
                            }, transaction);
                        }

                        // Insert documents
                        if (model.Documents != null)
                        {
                            if (!string.IsNullOrEmpty(model.Documents.ResumePath))
                            {
                                await InsertDocument(connection, transaction, model.Basic.TeacherId,
                                    "Resume", model.Documents.ResumePath, model.Basic.SchoolCode);
                            }

                            if (!string.IsNullOrEmpty(model.Documents.JoiningLetterPath))
                            {
                                await InsertDocument(connection, transaction, model.Basic.TeacherId,
                                    "JoiningLetter", model.Documents.JoiningLetterPath, model.Basic.SchoolCode);
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
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
                        // Update basic info
                        var basicQuery = @"
                            UPDATE TeacherInfoBasic SET
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
                                ModifiedDate = GETDATE()
                            WHERE TeacherId = @TeacherId";

                        await connection.ExecuteAsync(basicQuery, model.Basic, transaction);

                        // Update or insert payroll
                        await UpdateOrInsertPayroll(connection, transaction, model);

                        // Update or insert leaves
                        await UpdateOrInsertLeaves(connection, transaction, model);

                        // Update or insert bank details
                        await UpdateOrInsertBankDetails(connection, transaction, model);

                        // Update or insert social media
                        await UpdateOrInsertSocialMedia(connection, transaction, model);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task DeleteTeacherAsync(Guid teacherId, int schoolCode)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    UPDATE TeacherInfoBasic 
                    SET IsDeleted = 1, ModifiedDate = GETDATE() 
                    WHERE TeacherId = @TeacherId AND SchoolCode = @SchoolCode";

                await connection.ExecuteAsync(query, new { TeacherId = teacherId, SchoolCode = schoolCode });
            }
        }

        // Helper methods
        private async Task InsertDocument(SqlConnection connection, SqlTransaction transaction,
            Guid teacherId, string documentType, string documentPath, int schoolCode)
        {
            var query = @"
                INSERT INTO TeacherDocuments (
                    DocumentId, TeacherId, DocumentType, DocumentPath, 
                    UploadDate, SchoolCode
                ) VALUES (
                    @DocumentId, @TeacherId, @DocumentType, @DocumentPath, 
                    GETDATE(), @SchoolCode
                )";

            await connection.ExecuteAsync(query, new
            {
                DocumentId = Guid.NewGuid(),
                TeacherId = teacherId,
                DocumentType = documentType,
                DocumentPath = documentPath,
                SchoolCode = schoolCode
            }, transaction);
        }

        private async Task UpdateOrInsertPayroll(SqlConnection connection, SqlTransaction transaction,
            TeacherViewModel model)
        {
            var exists = await connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT COUNT(1) FROM TeacherPayroll WHERE TeacherId = @TeacherId",
                new { model.Basic.TeacherId }, transaction);

            if (exists)
            {
                var updateQuery = @"
                    UPDATE TeacherPayroll SET
                        EPFNo = @EPFNo, BasicSalary = @BasicSalary, 
                        ContractType = @ContractType, WorkShift = @WorkShift,
                        WorkLocation = @WorkLocation, DateOfLeaving = @DateOfLeaving
                    WHERE TeacherId = @TeacherId";

                await connection.ExecuteAsync(updateQuery, model.Payroll, transaction);
            }
            else if (model.Payroll != null)
            {
                model.Payroll.PayrollId = Guid.NewGuid();
                model.Payroll.TeacherId = model.Basic.TeacherId;

                var insertQuery = @"
                    INSERT INTO TeacherPayroll (
                        PayrollId, TeacherId, EPFNo, BasicSalary, ContractType,
                        WorkShift, WorkLocation, DateOfLeaving, SchoolCode
                    ) VALUES (
                        @PayrollId, @TeacherId, @EPFNo, @BasicSalary, @ContractType,
                        @WorkShift, @WorkLocation, @DateOfLeaving, @SchoolCode
                    )";

                await connection.ExecuteAsync(insertQuery, new
                {
                    model.Payroll.PayrollId,
                    model.Payroll.TeacherId,
                    model.Payroll.EPFNo,
                    model.Payroll.BasicSalary,
                    model.Payroll.ContractType,
                    model.Payroll.WorkShift,
                    model.Payroll.WorkLocation,
                    model.Payroll.DateOfLeaving,
                    model.Basic.SchoolCode
                }, transaction);
            }
        }

        private async Task UpdateOrInsertLeaves(SqlConnection connection, SqlTransaction transaction,
            TeacherViewModel model)
        {
            var exists = await connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT COUNT(1) FROM TeacherLeaves WHERE TeacherId = @TeacherId AND SessionId = @SessionId",
                new { model.Basic.TeacherId, model.Basic.SessionId }, transaction);

            if (exists)
            {
                var updateQuery = @"
                    UPDATE TeacherLeaves SET
                        MedicalLeaves = @MedicalLeaves, CasualLeaves = @CasualLeaves,
                        MaternityLeaves = @MaternityLeaves, SickLeaves = @SickLeaves
                    WHERE TeacherId = @TeacherId AND SessionId = @SessionId";

                await connection.ExecuteAsync(updateQuery, new
                {
                    model.Leaves.MedicalLeaves,
                    model.Leaves.CasualLeaves,
                    model.Leaves.MaternityLeaves,
                    model.Leaves.SickLeaves,
                    model.Basic.TeacherId,
                    model.Basic.SessionId
                }, transaction);
            }
            else if (model.Leaves != null)
            {
                model.Leaves.LeaveId = Guid.NewGuid();
                model.Leaves.TeacherId = model.Basic.TeacherId;

                var insertQuery = @"
                    INSERT INTO TeacherLeaves (
                        LeaveId, TeacherId, MedicalLeaves, CasualLeaves,
                        MaternityLeaves, SickLeaves, SchoolCode, SessionId
                    ) VALUES (
                        @LeaveId, @TeacherId, @MedicalLeaves, @CasualLeaves,
                        @MaternityLeaves, @SickLeaves, @SchoolCode, @SessionId
                    )";

                await connection.ExecuteAsync(insertQuery, new
                {
                    model.Leaves.LeaveId,
                    model.Leaves.TeacherId,
                    model.Leaves.MedicalLeaves,
                    model.Leaves.CasualLeaves,
                    model.Leaves.MaternityLeaves,
                    model.Leaves.SickLeaves,
                    model.Basic.SchoolCode,
                    model.Basic.SessionId
                }, transaction);
            }
        }

        private async Task UpdateOrInsertBankDetails(SqlConnection connection, SqlTransaction transaction,
            TeacherViewModel model)
        {
            if (model.BankDetails == null || string.IsNullOrEmpty(model.BankDetails.AccountNumber))
                return;

            var exists = await connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT COUNT(1) FROM TeacherBankDetails WHERE TeacherId = @TeacherId",
                new { model.Basic.TeacherId }, transaction);

            if (exists)
            {
                var updateQuery = @"
                    UPDATE TeacherBankDetails SET
                        AccountName = @AccountName, AccountNumber = @AccountNumber,
                        BankName = @BankName, IFSCCode = @IFSCCode, BranchName = @BranchName
                    WHERE TeacherId = @TeacherId";

                await connection.ExecuteAsync(updateQuery, model.BankDetails, transaction);
            }
            else
            {
                model.BankDetails.BankId = Guid.NewGuid();
                model.BankDetails.TeacherId = model.Basic.TeacherId;

                var insertQuery = @"
                    INSERT INTO TeacherBankDetails (
                        BankId, TeacherId, AccountName, AccountNumber,
                        BankName, IFSCCode, BranchName, SchoolCode
                    ) VALUES (
                        @BankId, @TeacherId, @AccountName, @AccountNumber,
                        @BankName, @IFSCCode, @BranchName, @SchoolCode
                    )";

                await connection.ExecuteAsync(insertQuery, new
                {
                    model.BankDetails.BankId,
                    model.BankDetails.TeacherId,
                    model.BankDetails.AccountName,
                    model.BankDetails.AccountNumber,
                    model.BankDetails.BankName,
                    model.BankDetails.IFSCCode,
                    model.BankDetails.BranchName,
                    model.Basic.SchoolCode
                }, transaction);
            }
        }

        private async Task UpdateOrInsertSocialMedia(SqlConnection connection, SqlTransaction transaction,
            TeacherViewModel model)
        {
            if (model.SocialMedia == null)
                return;

            var exists = await connection.QueryFirstOrDefaultAsync<bool>(
                "SELECT COUNT(1) FROM TeacherSocialMedia WHERE TeacherId = @TeacherId",
                new { model.Basic.TeacherId }, transaction);

            if (exists)
            {
                var updateQuery = @"
                    UPDATE TeacherSocialMedia SET
                        Facebook = @Facebook, Instagram = @Instagram,
                        LinkedIn = @LinkedIn, YouTube = @YouTube, Twitter = @Twitter
                    WHERE TeacherId = @TeacherId";

                await connection.ExecuteAsync(updateQuery, model.SocialMedia, transaction);
            }
            else
            {
                model.SocialMedia.SocialMediaId = Guid.NewGuid();
                model.SocialMedia.TeacherId = model.Basic.TeacherId;

                var insertQuery = @"
                    INSERT INTO TeacherSocialMedia (
                        SocialMediaId, TeacherId, Facebook, Instagram,
                        LinkedIn, YouTube, Twitter, SchoolCode
                    ) VALUES (
                        @SocialMediaId, @TeacherId, @Facebook, @Instagram,
                        @LinkedIn, @YouTube, @Twitter, @SchoolCode
                    )";

                await connection.ExecuteAsync(insertQuery, new
                {
                    model.SocialMedia.SocialMediaId,
                    model.SocialMedia.TeacherId,
                    model.SocialMedia.Facebook,
                    model.SocialMedia.Instagram,
                    model.SocialMedia.LinkedIn,
                    model.SocialMedia.YouTube,
                    model.SocialMedia.Twitter,
                    model.Basic.SchoolCode
                }, transaction);
            }
        }
    }
}