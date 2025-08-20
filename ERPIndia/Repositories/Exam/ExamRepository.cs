using Dapper;
using ERPIndia.Models.Exam;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ERPIndia.Repositories.Exam
{
    public class ExamRepository : IExamRepository
    {
        private readonly string _connectionString;

        public ExamRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<ExamMaster>> GetExamsBySessionAsync(Guid sessionId, Guid tenantId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                    SELECT ExamID, SerialNumber, ExamMonth, ExamName, ExamType, 
                           Remarks, SessionYear, SessionID, TenantID, TenantCode,
                           IsActive, IsDeleted, CreatedBy, CreatedDate, 
                           ModifiedBy, ModifiedDate, Num, MS, AdmitCard, AC
                    FROM ExamMaster
                    WHERE SessionID = @SessionID 
                      AND TenantID = @TenantID 
                      AND IsDeleted = 0
                    ORDER BY SerialNumber";

                var result = await connection.QueryAsync<ExamMaster>(query, new { SessionID = sessionId, TenantID = tenantId });
                return result;
            }
        }

        public async Task<bool> InitializeExamsForSessionAsync(Guid sessionId, Guid tenantId, int tenantCode, Guid userId, int sessionYear)
        {
            var months = new[] { "April", "May", "June", "July", "August", "September", "October", "November", "December", "January", "February", "March" };
            var defaultExams = new[]
            {
                new { Month = "April", Name = "1st test", Type = "Test", MaxMarks = 10 },
                new { Month = "May", Name = "2nd test", Type = "Test", MaxMarks = 10 },
                new { Month = "June", Name = "3rd test", Type = "Test", MaxMarks = 10 },
                new { Month = "July", Name = "half yearly", Type = "Half Yearly", MaxMarks = 70 },
                new { Month = "August", Name = "ANNUAL EXAM", Type = "Annual", MaxMarks = 100 },
                new { Month = "September", Name = "ANNUAL", Type = "Annual", MaxMarks = 0 },
                new { Month = "October", Name = "E3", Type = "Test", MaxMarks = 0 },
                new { Month = "November", Name = "FINAL REPORT CARD", Type = "Report", MaxMarks = 0 },
                new { Month = "December", Name = "Sea", Type = "Other", MaxMarks = 0 },
                new { Month = "January", Name = "PT 3", Type = "Test", MaxMarks = 0 },
                new { Month = "February", Name = "Blank", Type = "Other", MaxMarks = 0 },
                new { Month = "March", Name = "Final", Type = "Final", MaxMarks = 0 , },
                new { Month = "Quarterly", Name = "Quarterly", Type = "Quarterly", MaxMarks = 0 , },
                new { Month = "Half-Yearly", Name = "Half-Yearly", Type = "Half-Yearly", MaxMarks = 0 , },
                new { Month = "Annual", Name = "Annual", Type = "Annual", MaxMarks = 0 , }
            };

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var insertQuery = @"
                            INSERT INTO ExamMaster (ExamID, SerialNumber, ExamMonth, ExamName, ExamType, 
                                                   Remarks, SessionYear, SessionID, TenantID, TenantCode, 
                                                   IsActive, CreatedBy, CreatedDate,AdmitCard,Num,AC,MS)
                            VALUES (NEWID(), @SerialNumber, @ExamMonth, @ExamName, @ExamType, 
                                    @Remarks, @SessionYear, @SessionID, @TenantID, @TenantCode, 
                                    @IsActive, @CreatedBy, GETDATE(),@AdmitCard,@Num,@AC,@MS)";
                        var admitCardMap = new Dictionary<int, string>
                        {
                            [1] = "Quarterly",
                            [2] = "Half-Yearly",
                            [3] = "Annually"
                        };
                        var ExamMap = new Dictionary<int, string>
                        {
                            [13] = "Quarterly",
                            [14] = "Half-Yearly",
                            [15] = "Annually"
                        };
                        var msSerials = new HashSet<int>(Enumerable.Range(13, 3));
                        var examsList = defaultExams
                          .Select((exam, index) => {
                              int sn = index + 1;   // 1-based serial
                              return new
                              {
                                  SerialNumber = sn,
                                  ExamMonth = exam.Month,
                                  ExamName = ExamMap.TryGetValue(sn, out var examlabel) ? examlabel : $"Exam{sn}",
                                  ExamType = $"Exam{sn}",
                                  Remarks = $"Marksheet{sn}",
                                  SessionYear = sessionYear,
                                  AdmitCard = admitCardMap.TryGetValue(sn, out var label)? label: $"AdmitCard{sn}",
                                  SessionID = sessionId,
                                  // Num goes 1,2,…,8
                                  Num = sn <= 8 ? sn : 0,
                                  // AC = 1 only for serials 1…3
                                  AC = (sn >= 1 && sn <= 3) ? 1 : 0,
                                  // MS = 1 for serials 1…8 (and would cover 13…15 if you had them)
                                  MS = msSerials.Contains(sn) ? 1 : 0,
                                  TenantID = tenantId,
                                  TenantCode = tenantCode,
                                  IsActive=1,
                                  CreatedBy = userId
                              };
                          })
                          .ToList();

                        await connection.ExecuteAsync(insertQuery, examsList, transaction);

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
        public async Task<bool> BulkUpdateExamsAsync(IEnumerable<ExamMaster> exams)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var query = @"
                            UPDATE ExamMaster 
                            SET
                                ExamName = @ExamName,
                                IsActive = @IsActive,
                                Num = @Num,
                                MS = @MS,
                                AdmitCard = @AdmitCard,
                                AC = @AC,
                                ModifiedBy = @ModifiedBy,
                                ModifiedDate = @ModifiedDate
                            WHERE ExamID = @ExamID";

                        foreach (var exam in exams)
                        {
                            await connection.ExecuteAsync(query, new
                            {
                                exam.ExamID,
                                exam.ExamMonth,
                                exam.ExamName,
                                exam.ExamType,
                                exam.Remarks,
                                exam.IsActive,
                                exam.Num,
                                exam.MS,
                                exam.AdmitCard,
                                exam.AC,
                                exam.ModifiedBy,
                                exam.ModifiedDate
                            }, transaction);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        // Log exception
                        throw;
                    }
                }
            }
        }
    }
}