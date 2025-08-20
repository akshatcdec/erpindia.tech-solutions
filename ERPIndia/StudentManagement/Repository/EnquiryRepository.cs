using Dapper;
using ERPK12Models.DTO;
using ERPK12Models.ViewModel.Enquiry;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ERPIndia.StudentManagement.Repository
{
    public class EnquiryRepository : IEnquiryRepository
    {
        private readonly string _connectionString;

        public EnquiryRepository()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        }

        private IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }

        // NEW METHOD: Generate EnqNo as simple sequential number (1, 2, 3...)
        public async Task<int> GenerateEnquiryNumberAsync(Guid sessionId, int tenantCode)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT ISNULL(MAX(EnqNo), 0) + 1
                    FROM Enquiries
                    WHERE SessionID = @SessionId 
                    AND TenantCode = @TenantCode
                    AND IsDeleted = 0";

                var nextEnqNo = await connection.QuerySingleOrDefaultAsync<int>(sql,
                    new { SessionId = sessionId, TenantCode = tenantCode });

                // Start from 1 for each tenant: 1, 2, 3, 4, 5...
                return nextEnqNo;
            }
        }
        // Add this implementation to your EnquiryRepository class
public async Task<ConversionResult> ConvertEnquiryToAdmissionAsync(
    Guid enquiryId,
    int schoolCode,
    DateTime admissionDate,
    Guid sessionId,
    int tenantCode,
    Guid createdBy)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@EnquiryId", enquiryId);
                    parameters.Add("@SchoolCode", schoolCode);
                    parameters.Add("@AdmissionDate", admissionDate);
                    parameters.Add("@CreatedBy", createdBy);
                    parameters.Add("@SessionID", sessionId);
                    parameters.Add("@Result", dbType: DbType.String, direction: ParameterDirection.Output, size: 500);

                    await connection.ExecuteAsync(
                        "dbo.sp_ConvertEnquiryToAdmission",
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    var result = parameters.Get<string>("@Result");

                    if (result.StartsWith("SUCCESS"))
                    {
                        // Extract admission number from success message
                        var admissionNoMatch = System.Text.RegularExpressions.Regex.Match(result, @"Admission No: (\d+)");
                        int? admissionNo = null;
                        if (admissionNoMatch.Success && int.TryParse(admissionNoMatch.Groups[1].Value, out int admNo))
                        {
                            admissionNo = admNo;
                        }

                        return new ConversionResult
                        {
                            Success = true,
                            Message = "Enquiry successfully converted to admission!",
                            AdmissionNo = admissionNo
                        };
                    }
                    else
                    {
                        return new ConversionResult
                        {
                            Success = false,
                            Message = result.Replace("ERROR: ", "")
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ConversionResult
                {
                    Success = false,
                    Message = "Database error: " + ex.Message
                };
            }
        }
        public async Task<string> GenerateReceiptNumberAsync(Guid sessionId, int tenantCode)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT 
                        'RCP' + CONVERT(VARCHAR(4), YEAR(GETDATE())) + 
                        RIGHT('00000' + CAST(ISNULL(MAX(CAST(RIGHT(CAST(Id AS VARCHAR(36)), 5) AS INT)), 0) + 1 AS VARCHAR), 5)
                    FROM Enquiries
                    WHERE YEAR(CreatedDate) = YEAR(GETDATE()) 
                    AND SessionID = @SessionId 
                    AND TenantCode = @TenantCode
                    AND IsDeleted = 0";

                var receiptNo = await connection.QuerySingleOrDefaultAsync<string>(sql,
                    new { SessionId = sessionId, TenantCode = tenantCode });
                return receiptNo ?? "RCP" + DateTime.Now.Year + "00001";
            }
        }

        public async Task<ReceiptInfo> GetReceiptInfoAsync(Guid enquiryId, Guid sessionId, int tenantCode)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
            SELECT 
                CAST((EnqNo + 1000) AS VARCHAR) AS ReceiptNo,
                RcptDate AS ReceiptDate,
                FormAmt AS Amount,
                'Cash' AS PaymentMode
            FROM Enquiries 
            WHERE Id = @EnquiryId 
            AND SessionID = @SessionId 
            AND TenantCode = @TenantCode
            AND IsDeleted = 0";

                var receiptInfo = await connection.QuerySingleOrDefaultAsync<ReceiptInfo>(sql,
                    new { EnquiryId = enquiryId, SessionId = sessionId, TenantCode = tenantCode });

                if (receiptInfo != null && !receiptInfo.ReceiptDate.HasValue)
                {
                    receiptInfo.ReceiptDate = DateTime.Now;
                }

                return receiptInfo;
            }
        }
        public async Task<IEnumerable<StudentEnquiry>> GetAllEnquiriesAsync(Guid sessionId, int tenantCode,
            EnquiryFilterViewModel filters = null, int page = 1, int pageSize = 10)
        {
            using (var connection = CreateConnection())
            {
                var sql = BuildEnquiryQuery(sessionId, tenantCode, filters, page, pageSize);
                var parameters = BuildParameters(sessionId, tenantCode, filters, page, pageSize);

                var enquiries = await connection.QueryAsync<StudentEnquiry>(sql, parameters);

                // Load follow-ups for each enquiry
                foreach (var enquiry in enquiries)
                {
                    enquiry.FollowUps = (await GetFollowUpsByEnquiryIdAsync(enquiry.Id, sessionId, tenantCode)).ToList();
                }

                return enquiries;
            }
        }

        public async Task<StudentEnquiry> GetEnquiryByIdAsync(Guid id, Guid sessionId, int tenantCode)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT * FROM Enquiries 
                    WHERE Id = @Id AND SessionID = @SessionId AND TenantCode = @TenantCode AND IsDeleted = 0;
                    
                    SELECT * FROM EnquiriesFollowUps 
                    WHERE EnquiryId = @Id AND SessionID = @SessionId AND TenantCode = @TenantCode AND IsDeleted = 0
                    ORDER BY SrNo;
                ";

                using (var multi = await connection.QueryMultipleAsync(sql,
                    new { Id = id, SessionId = sessionId, TenantCode = tenantCode }))
                {
                    var enquiry = await multi.ReadSingleOrDefaultAsync<StudentEnquiry>();
                    if (enquiry != null)
                    {
                        enquiry.FollowUps = (await multi.ReadAsync<EnquiryFollowUp>()).ToList();
                    }
                    return enquiry;
                }
            }
        }

        // UPDATED: Now generates EnqNo automatically based on TenantCode
        public async Task<Guid> CreateEnquiryAsync(StudentEnquiry enquiry, Guid sessionId, int tenantCode, Guid createdBy)
        {
            using (var connection = CreateConnection())
            {
                // Generate the next EnqNo for this tenant
                var nextEnqNo = await GenerateEnquiryNumberAsync(sessionId, tenantCode);

                var sql = @"
                    INSERT INTO Enquiries 
                    (Id, EnqNo, Student, Father, Mother, ApplyingForClass, Mobile1, Mobile2, Gender, Address, 
                     PreviousSchool, Relation, NoOfChild, EnquiryDate, DealBy, Source, SendSMS, 
                     FormAmt, RcptDate, Note, PaymentStatus, InterestLevel, NextFollowup, 
                     CreatedDate, SessionID, TenantCode, CreatedBy, IsActive, IsDeleted,ClassId,TenantId)
                    VALUES 
                    (@Id, @EnqNo, @Student, @Father, @Mother,(SELECT ClassName FROM AcademicClassMaster WHERE ClassID = @ClassId), @Mobile1, @Mobile2, @Gender, @Address,
                     @PreviousSchool, @Relation, @NoOfChild, @EnquiryDate, @DealBy, @Source, @SendSMS,
                     @FormAmt, @RcptDate, @Note, @PaymentStatus, @InterestLevel, @NextFollowup,
                     @CreatedDate, @SessionID, @TenantCode, @CreatedBy, @IsActive, @IsDeleted,@ClassId,@TenantId);
                    SELECT @Id;
                ";

                enquiry.Id = Guid.NewGuid();
                enquiry.EnqNo = nextEnqNo; // Set the generated EnqNo
                enquiry.CreatedDate = DateTime.Now;
                enquiry.SessionID = sessionId;
                enquiry.TenantCode = tenantCode;
                enquiry.CreatedBy = createdBy;
                enquiry.IsActive = true;
                enquiry.IsDeleted = false;

                var id = await connection.QuerySingleAsync<Guid>(sql, enquiry);
                return id;
            }
        }

        public async Task<bool> UpdateEnquiryAsync(StudentEnquiry enquiry, Guid sessionId, int tenantCode, Guid modifiedBy)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                            UPDATE Enquiries SET
                            Student = @Student, Father = @Father, Mother = @Mother, 
                            ApplyingForClass = (SELECT ClassName FROM AcademicClassMaster WHERE ClassId = @ClassId), 
                            Mobile1 = @Mobile1, Mobile2 = @Mobile2,
                            Gender = @Gender, Address = @Address, PreviousSchool = @PreviousSchool,
                            Relation = @Relation, NoOfChild = @NoOfChild, EnquiryDate = @EnquiryDate,
                            DealBy = @DealBy, Source = @Source, SendSMS = @SendSMS, FormAmt = @FormAmt,
                            RcptDate = @RcptDate, Note = @Note, PaymentStatus = @PaymentStatus,
                            InterestLevel = @InterestLevel, NextFollowup = @NextFollowup, 
                            ClassId = @ClassId, ModifiedDate = @ModifiedDate, ModifiedBy = @ModifiedBy
                            WHERE Id = @Id 
                            AND SessionID = @SessionID 
                            AND TenantCode = @TenantCode 
                            AND IsDeleted = 0
                            ";

                enquiry.ModifiedDate = DateTime.Now;
                enquiry.ModifiedBy = modifiedBy;
                enquiry.SessionID = sessionId;
                enquiry.TenantCode = tenantCode;

                var rowsAffected = await connection.ExecuteAsync(sql, enquiry);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteEnquiryAsync(Guid id, Guid sessionId, int tenantCode, Guid deletedBy)
        {
            using (var connection = CreateConnection())
            {
                // Soft delete follow-ups first
                await connection.ExecuteAsync(@"
                    UPDATE EnquiriesFollowUps 
                    SET IsDeleted = 1, ModifiedDate = @ModifiedDate, ModifiedBy = @ModifiedBy 
                    WHERE Id = @Id",
                    new { Id = id,ModifiedDate = DateTime.Now, ModifiedBy = deletedBy });

                // Soft delete enquiry
                var sql = @"
                     UPDATE EnquiriesFollowUps 
                    SET IsDeleted = 1, ModifiedDate = @ModifiedDate, ModifiedBy = @ModifiedBy 
                    WHERE Id = @Id";

                var rowsAffected = await connection.ExecuteAsync(sql,
                    new { Id = id,ModifiedDate = DateTime.Now, ModifiedBy = deletedBy });
                return rowsAffected > 0;
            }
        }
        public async Task<bool> DeleteEnquiryFollowupAsync(Guid id, Guid sessionId, int tenantCode, Guid deletedBy)
        {
            using (var connection = CreateConnection())
            {
                // Soft delete follow-ups 
                var sql = @"
                    UPDATE EnquiriesFollowUps 
                    SET IsDeleted = 1, ModifiedDate = @ModifiedDate, ModifiedBy = @ModifiedBy 
                    WHERE Id = @Id";

                var rowsAffected = await connection.ExecuteAsync(sql,
                    new { Id = id, ModifiedDate = DateTime.Now, ModifiedBy = deletedBy });
                return rowsAffected > 0;
            }
        }
        public async Task<int> GetTotalEnquiriesCountAsync(Guid sessionId, int tenantCode, EnquiryFilterViewModel filters = null)
        {
            using (var connection = CreateConnection())
            {
                var sql = "SELECT COUNT(DISTINCT e.Id) FROM Enquiries e";

                // Add LEFT JOIN only if we need to filter by follow-up dates or call status
                if (filters?.FollowupDate.HasValue == true || filters?.NextFollowup.HasValue == true || !string.IsNullOrEmpty(filters?.CallStatus))
                {
                    sql += " LEFT JOIN EnquiriesFollowUps ef ON e.Id = ef.EnquiryId AND ef.SessionID = e.SessionID AND ef.TenantCode = e.TenantCode AND ef.IsDeleted = 0";
                }

                sql += BuildWhereClause(sessionId, tenantCode, filters);
                var parameters = BuildFilterParameters(sessionId, tenantCode, filters);

                return await connection.QuerySingleAsync<int>(sql, parameters);
            }
        }

        public async Task<IEnumerable<EnquiryFollowUp>> GetFollowUpsByEnquiryIdAsync(Guid enquiryId, Guid sessionId, int tenantCode)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    SELECT * FROM EnquiriesFollowUps 
                    WHERE EnquiryId = @EnquiryId 
                    AND SessionID = @SessionId 
                    AND TenantCode = @TenantCode 
                    AND IsDeleted = 0
                    ORDER BY FollowDate DESC, FollowTime DESC";

                return await connection.QueryAsync<EnquiryFollowUp>(sql,
                    new { EnquiryId = enquiryId, SessionId = sessionId, TenantCode = tenantCode });
            }
        }

        public async Task<Guid> CreateFollowUpAsync(EnquiryFollowUp followUp, Guid sessionId, int tenantCode, Guid createdBy)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    INSERT INTO EnquiriesFollowUps 
                    (Id, EnquiryId, FollowDate, FollowTime, CallStatus, InterestLevel, Response, NextFollowDate, 
                     CreatedDate, SessionID, TenantCode, CreatedBy, IsActive, IsDeleted)
                    VALUES 
                    (@Id, @EnquiryId, @FollowDate, @FollowTime, @CallStatus, @InterestLevel, @Response, @NextFollowDate,
                     @CreatedDate, @SessionID, @TenantCode, @CreatedBy, @IsActive, @IsDeleted);
                    SELECT @Id;
                ";

                followUp.Id = Guid.NewGuid();
                followUp.CreatedDate = DateTime.Now;
                followUp.SessionID = sessionId;
                followUp.TenantCode = tenantCode;
                followUp.CreatedBy = createdBy;
                followUp.IsActive = true;
                followUp.IsDeleted = false;

                var id = await connection.QuerySingleAsync<Guid>(sql, followUp);

                // Update the main enquiry record with latest follow-up info
                await UpdateEnquiryFollowUpInfo(followUp.EnquiryId, followUp.InterestLevel, followUp.NextFollowDate, sessionId, tenantCode);

                return id;
            }
        }

        public async Task<bool> UpdateFollowUpAsync(EnquiryFollowUp followUp, Guid sessionId, int tenantCode, Guid modifiedBy)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    UPDATE EnquiriesFollowUps SET
                        FollowDate = @FollowDate, FollowTime = @FollowTime, CallStatus = @CallStatus,
                        InterestLevel = @InterestLevel, Response = @Response, NextFollowDate = @NextFollowDate,
                        ModifiedDate = @ModifiedDate, ModifiedBy = @ModifiedBy
                    WHERE Id = @Id 
                    AND SessionID = @SessionID 
                    AND TenantCode = @TenantCode 
                    AND IsDeleted = 0
                ";

                followUp.ModifiedDate = DateTime.Now;
                followUp.ModifiedBy = modifiedBy;
                followUp.SessionID = sessionId;
                followUp.TenantCode = tenantCode;

                var rowsAffected = await connection.ExecuteAsync(sql, followUp);
                return rowsAffected > 0;
            }
        }

        public async Task<bool> DeleteFollowUpAsync(Guid id, Guid sessionId, int tenantCode, Guid deletedBy)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    UPDATE EnquiriesFollowUps 
                    SET IsDeleted = 1, ModifiedDate = @ModifiedDate, ModifiedBy = @ModifiedBy 
                    WHERE Id = @Id";

                var rowsAffected = await connection.ExecuteAsync(sql,
                    new { Id = id, ModifiedDate = DateTime.Now, ModifiedBy = deletedBy });
                return rowsAffected > 0;
            }
        }

        private async Task UpdateEnquiryFollowUpInfo(Guid enquiryId, string interestLevel, DateTime? nextFollowDate, Guid sessionId, int tenantCode)
        {
            using (var connection = CreateConnection())
            {
                var sql = @"
                    UPDATE Enquiries 
                    SET InterestLevel = @InterestLevel, NextFollowup = @NextFollowup 
                    WHERE Id = @Id 
                    AND SessionID = @SessionId 
                    AND TenantCode = @TenantCode 
                    AND IsDeleted = 0
                ";

                await connection.ExecuteAsync(sql, new
                {
                    Id = enquiryId,
                    InterestLevel = interestLevel,
                    NextFollowup = nextFollowDate,
                    SessionId = sessionId,
                    TenantCode = tenantCode
                });
            }
        }


        private string BuildEnquiryQuery(Guid sessionId, int tenantCode, EnquiryFilterViewModel filters, int page, int pageSize)
        {
            var sql = "SELECT DISTINCT e.* FROM Enquiries e";

            // Add LEFT JOIN only if we need to filter by follow-up dates or call status
            if (filters?.FollowupDate.HasValue == true || filters?.NextFollowup.HasValue == true || !string.IsNullOrEmpty(filters?.CallStatus))
            {
                sql += " LEFT JOIN EnquiriesFollowUps ef ON e.Id = ef.EnquiryId AND ef.SessionID = e.SessionID AND ef.TenantCode = e.TenantCode AND ef.IsDeleted = 0";
            }

            sql += BuildWhereClause(sessionId, tenantCode, filters);
            sql += " ORDER BY e.EnqNo";
            sql += $" OFFSET {(page - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY";
            return sql;
        }

        private string BuildWhereClause(Guid sessionId, int tenantCode, EnquiryFilterViewModel filters)
        {
            var conditions = new List<string>
    {
        "e.SessionID = @SessionId",
        "e.TenantCode = @TenantCode",
        "e.IsDeleted = 0",
        "e.InterestLevel <> 'Admitted'"
    };

            if (filters != null)
            {
                // Date range filter - check both FromDate/ToDate and StartDate/EndDate
                if (filters.FromDate.HasValue && filters.ToDate.HasValue)
                {
                    conditions.Add("e.EnquiryDate BETWEEN @FromDate AND @ToDate");
                }
                else if (filters.StartDate.HasValue && filters.EndDate.HasValue)
                {
                    conditions.Add("e.EnquiryDate BETWEEN @StartDate AND @EndDate");
                }

                // Class filter - handle both Guid and string values
                if (!string.IsNullOrEmpty(filters.SelectedClass) && filters.SelectedClass != Guid.Empty.ToString())
                {
                    conditions.Add("e.ClassId = @SelectedClass");
                }

                // Interest Level filter
                if (!string.IsNullOrEmpty(filters.InterestLevel))
                {
                    conditions.Add("e.InterestLevel = @InterestLevel");
                }

                // Call Status filter - from follow-ups table
                if (!string.IsNullOrEmpty(filters.CallStatus))
                {
                    conditions.Add("ef.CallStatus = @CallStatus");
                }

                // Follow-up Date filter - checks actual follow-up dates
                if (filters.FollowupDate.HasValue)
                {
                    conditions.Add(@"(
                CAST(ef.FollowDate AS DATE) = @FollowupDate
            )");
                }

                // Next Follow-up Date filter - checks next follow-up dates
                if (filters.NextFollowup.HasValue)
                {
                    conditions.Add(@"(
                CAST(e.NextFollowup AS DATE) = @NextFollowup OR 
                CAST(ef.NextFollowDate AS DATE) = @NextFollowup
            )");
                }
            }

            return " WHERE " + string.Join(" AND ", conditions);
        }
        private object BuildFilterParameters(Guid sessionId, int tenantCode, EnquiryFilterViewModel filters)
        {
            var parameters = new Dictionary<string, object>
            {
                ["SessionId"] = sessionId,
                ["TenantCode"] = tenantCode
            };

            if (filters != null)
            {
                // Date range parameters
                if (filters.FromDate.HasValue) parameters["FromDate"] = filters.FromDate;
                if (filters.ToDate.HasValue) parameters["ToDate"] = filters.ToDate;
                if (filters.StartDate.HasValue) parameters["StartDate"] = filters.StartDate;
                if (filters.EndDate.HasValue) parameters["EndDate"] = filters.EndDate;

                // Class parameter - handle Guid values
                if (!string.IsNullOrEmpty(filters.SelectedClass) && filters.SelectedClass != Guid.Empty.ToString())
                {
                    parameters["SelectedClass"] = filters.SelectedClass;
                }

                // Other filters
                if (!string.IsNullOrEmpty(filters.InterestLevel)) parameters["InterestLevel"] = filters.InterestLevel;
                if (!string.IsNullOrEmpty(filters.CallStatus)) parameters["CallStatus"] = filters.CallStatus;
                if (filters.FollowupDate.HasValue) parameters["FollowupDate"] = filters.FollowupDate;
                if (filters.NextFollowup.HasValue) parameters["NextFollowup"] = filters.NextFollowup;
            }

            return parameters;
        }

        private object BuildParameters(Guid sessionId, int tenantCode, EnquiryFilterViewModel filters, int page, int pageSize)
        {
            return BuildFilterParameters(sessionId, tenantCode, filters);
        }
    }
}