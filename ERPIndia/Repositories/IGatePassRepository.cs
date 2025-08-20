using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Wordprocessing;
using ERPK12Models.ViewModel.GatePass;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ERPIndia.Repositories.GatePass
{
    // Repository Interface
    public interface IGatePassRepository
    {
        Task<IEnumerable<GatePassRecord>> GetAllGatePassesAsync(Guid sessionId, int tenantCode, GatePassFilterViewModel filters, int page, int pageSize);
        Task<int> GetTotalGatePassesCountAsync(Guid sessionId, int tenantCode, GatePassFilterViewModel filters);
        Task<GatePassRecord> GetGatePassByIdAsync(Guid id, Guid sessionId, int tenantCode);
        Task<Guid> CreateGatePassAsync(GatePassRecord gatePass, Guid sessionId, int tenantCode, Guid userId, Guid tenantId);
        Task<bool> UpdateGatePassAsync(GatePassRecord gatePass, Guid sessionId, int tenantCode, Guid userId);
        Task<bool> DeleteGatePassAsync(Guid id, Guid sessionId, int tenantCode, Guid userId);
        Task<IEnumerable<GatePassRecord>> GetGatePassHistoryByStudentAsync(string studentName, Guid sessionId, int tenantCode);
        Task<IEnumerable<GatePassRecord>> GetGatePassHistoryByStudentIdAsync(Guid studentId, Guid sessionId, int tenantCode);

        Task<bool> IncrementPrintCountAsync(Guid id, Guid sessionId, int tenantCode, Guid userId);
        string GeneratePassNumber(Guid sessionId, int tenantCode);
        Task<IEnumerable<StudentSearchResult>> SearchStudentsAsync(string searchTerm, Guid sessionId, int tenantCode);
    }

    // Repository Implementation
    public class GatePassRepository : IGatePassRepository
    {
        private readonly string _connectionString;

        public GatePassRepository()
        {
            _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        }

        public async Task<IEnumerable<GatePassRecord>> GetAllGatePassesAsync(
      Guid sessionId,
      int tenantCode,
      GatePassFilterViewModel filters,
      int page,
      int pageSize)
        {
            var gatePasses = new List<GatePassRecord>();
            var offset = (page - 1) * pageSize;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var sql = @"
WITH LatestPerStudent AS
(
    SELECT
        gp.*,
        ROW_NUMBER() OVER (
            PARTITION BY gp.StudentId
            ORDER BY gp.Date DESC,
                     gp.TimeIn DESC
        ) AS rn
    FROM vwGatePasses gp
    WHERE gp.SessionId = @SessionId
      AND gp.TenantCode = @TenantCode
      AND gp.IsDeleted = 0"; 
        

        if (filters != null)
                {
                    if (filters.StartDate.HasValue)
                        sql += " AND gp.Date >= @StartDate";
                    if (filters.EndDate.HasValue)
                        sql += " AND gp.Date <= @EndDate";
                    if (!string.IsNullOrEmpty(filters.StudentName))
                        sql += " AND gp.StudentName LIKE @StudentName";
                    if (!string.IsNullOrEmpty(filters.SelectedClass))
                        sql += " AND gp.ClassId = @ClassId";
                    if (!string.IsNullOrEmpty(filters.PassNo))
                        sql += " AND gp.PassNo LIKE @PassNo";
                }

                sql += @"
)
SELECT *
FROM LatestPerStudent
WHERE rn = 1  -- Only the latest record per student
ORDER BY Date DESC, TimeIn DESC
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    if (filters != null)
                    {
                        if (filters.StartDate.HasValue)
                            cmd.Parameters.AddWithValue("@StartDate", filters.StartDate.Value.Date);
                        if (filters.EndDate.HasValue)
                            cmd.Parameters.AddWithValue("@EndDate", filters.EndDate.Value.Date.AddDays(1).AddTicks(-1));
                        if (!string.IsNullOrEmpty(filters.StudentName))
                            cmd.Parameters.AddWithValue("@StudentName", $"%{filters.StudentName}%");
                        if (!string.IsNullOrEmpty(filters.SelectedClass))
                            cmd.Parameters.AddWithValue("@ClassId", Guid.Parse(filters.SelectedClass));
                        if (!string.IsNullOrEmpty(filters.PassNo))
                            cmd.Parameters.AddWithValue("@PassNo", $"%{filters.PassNo}%");
                    }

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            gatePasses.Add(MapGatePassFromReader(reader));
                        }
                    }
                }
            }

            return gatePasses;
        }

        public async Task<int> GetTotalGatePassesCountAsync(
            Guid sessionId,
            int tenantCode,
            GatePassFilterViewModel filters)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Count distinct StudentId to match the one-row-per-student output
                var sql = @"
SELECT COUNT(DISTINCT StudentId)
FROM GatePasses
WHERE SessionId = @SessionId
  AND TenantCode = @TenantCode
  AND IsDeleted = 0";

                // Same filters as above
                if (filters != null)
                {
                    if (filters.StartDate.HasValue)
                        sql += " AND Date >= @StartDate";
                    if (filters.EndDate.HasValue)
                        sql += " AND Date <= @EndDate";
                    if (!string.IsNullOrEmpty(filters.StudentName))
                        sql += " AND StudentName LIKE @StudentName";
                    if (!string.IsNullOrEmpty(filters.SelectedClass))
                        sql += " AND ClassId = @ClassId";
                    if (!string.IsNullOrEmpty(filters.PassNo))
                        sql += " AND PassNo LIKE @PassNo";
                }

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);

                    if (filters != null)
                    {
                        if (filters.StartDate.HasValue)
                            cmd.Parameters.AddWithValue("@StartDate", filters.StartDate.Value.Date);
                        if (filters.EndDate.HasValue)
                            cmd.Parameters.AddWithValue("@EndDate", filters.EndDate.Value.Date.AddDays(1).AddTicks(-1));
                        if (!string.IsNullOrEmpty(filters.StudentName))
                            cmd.Parameters.AddWithValue("@StudentName", $"%{filters.StudentName}%");
                        if (!string.IsNullOrEmpty(filters.SelectedClass))
                            cmd.Parameters.AddWithValue("@ClassId", Guid.Parse(filters.SelectedClass));
                        if (!string.IsNullOrEmpty(filters.PassNo))
                            cmd.Parameters.AddWithValue("@PassNo", $"%{filters.PassNo}%");
                    }

                    return (int)await cmd.ExecuteScalarAsync();
                }
            }
        }
        public async Task<GatePassRecord> GetGatePassByIdAsync(Guid id, Guid sessionId, int tenantCode)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT * FROM GatePasses
                    WHERE Id = @Id 
                        AND SessionId = @SessionId 
                        AND TenantCode = @TenantCode 
                        AND IsDeleted = 0";

                using (var cmd = new System.Data.SqlClient.SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapGatePassFromReader(reader);
                        }
                    }
                }
            }

            return null;
        }

        public async Task<Guid> CreateGatePassAsync(GatePassRecord gatePass, Guid sessionId, int tenantCode, Guid userId, Guid tenantId)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Generate Pass Number within transaction to ensure atomicity
                        if (string.IsNullOrEmpty(gatePass.PassNo) || gatePass.PassNo == "AUTO-GENERATE")
                        {
                            gatePass.PassNo = GeneratePassNumberInTransaction(sessionId, tenantCode, transaction);
                        }

                        var query = @"
                    INSERT INTO GatePasses (
                        Id, PassNo, Date, StudentName, Father, Mother, 
                        Class, ClassId, Address, TimeIn, TimeOut,
                        ParentGuardianName, GuardianMobile, RelationshipToStudent,
                        ReasonForLeave, PrintTime, PrintCount,ReasonFor,
                        SessionId, TenantCode, TenantId, StudentId, AdmNo,
                        CreatedBy, CreatedDate, IsActive, IsDeleted
                    ) VALUES (
                        @Id, @PassNo, @Date, @StudentName, @Father, @Mother,
                        @Class, @ClassId, @Address, @TimeIn, @TimeOut,
                        @ParentGuardianName, @GuardianMobile, @RelationshipToStudent,
                        @ReasonForLeave, @PrintTime, 0,@ReasonFor,
                        @SessionId, @TenantCode, @TenantId, @StudentId, @AdmNo,
                        @CreatedBy, GETDATE(), 1, 0
                    )";

                        using (var cmd = new System.Data.SqlClient.SqlCommand(query, conn, transaction))
                        {
                            gatePass.Id = Guid.NewGuid();

                            cmd.Parameters.AddWithValue("@Id", gatePass.Id);
                            cmd.Parameters.AddWithValue("@PassNo", gatePass.PassNo ?? "");
                            cmd.Parameters.AddWithValue("@Date", gatePass.Date);
                            cmd.Parameters.AddWithValue("@StudentName", gatePass.StudentName);
                            cmd.Parameters.AddWithValue("@Father", (object)gatePass.Father ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Mother", (object)gatePass.Mother ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Class", (object)gatePass.Class ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ClassId", (object)gatePass.ClassId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@Address", (object)gatePass.Address ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@TimeIn", (object)gatePass.TimeIn ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@TimeOut", (object)gatePass.TimeOut ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ParentGuardianName", (object)gatePass.ParentGuardianName ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@GuardianMobile", (object)gatePass.GuardianMobile ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@RelationshipToStudent", (object)gatePass.RelationshipToStudent ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ReasonFor", (object)gatePass.ReasonFor ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@ReasonForLeave", (object)gatePass.ReasonForLeave ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@PrintTime", (object)gatePass.PrintTime ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@SessionId", sessionId);
                            cmd.Parameters.AddWithValue("@TenantCode", tenantCode);
                            cmd.Parameters.AddWithValue("@TenantId", tenantId);
                            cmd.Parameters.AddWithValue("@StudentId", (object)gatePass.StudentId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@AdmNo", (object)gatePass.AdmNo ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@CreatedBy", userId);

                            await cmd.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                        return gatePass.Id;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
        public async Task<bool> UpdateGatePassAsync(GatePassRecord gatePass, Guid sessionId, int tenantCode, Guid userId)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    UPDATE GatePasses SET
                        Date = @Date,
                        StudentName = @StudentName,
                        Father = @Father,
                        Mother = @Mother,
                        Class = @Class,
                        ClassId = @ClassId,
                        Address = @Address,
                        TimeIn = @TimeIn,
                        TimeOut = @TimeOut,
                        ParentGuardianName = @ParentGuardianName,
                        GuardianMobile = @GuardianMobile,
                        RelationshipToStudent = @RelationshipToStudent,
                        ReasonForLeave = @ReasonForLeave,
                        ModifiedBy = @ModifiedBy,
                        ModifiedDate = GETDATE()
                    WHERE Id = @Id 
                        AND SessionId = @SessionId 
                        AND TenantCode = @TenantCode 
                        AND IsDeleted = 0";

                using (var cmd = new System.Data.SqlClient.SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", gatePass.Id);
                    cmd.Parameters.AddWithValue("@Date", gatePass.Date);
                    cmd.Parameters.AddWithValue("@StudentName", gatePass.StudentName);
                    cmd.Parameters.AddWithValue("@Father", (object)gatePass.Father ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Mother", (object)gatePass.Mother ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Class", (object)gatePass.Class ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ClassId", (object)gatePass.ClassId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object)gatePass.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TimeIn", (object)gatePass.TimeIn ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TimeOut", (object)gatePass.TimeOut ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ParentGuardianName", (object)gatePass.ParentGuardianName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GuardianMobile", (object)gatePass.GuardianMobile ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@RelationshipToStudent", (object)gatePass.RelationshipToStudent ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ReasonForLeave", (object)gatePass.ReasonForLeave ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModifiedBy", userId);
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);

                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> DeleteGatePassAsync(Guid id, Guid sessionId, int tenantCode, Guid userId)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    UPDATE GatePasses SET
                        IsDeleted = 1,
                        ModifiedBy = @ModifiedBy,
                        ModifiedDate = GETDATE()
                    WHERE Id = @Id 
                        AND SessionId = @SessionId 
                        AND TenantCode = @TenantCode";

                using (var cmd = new System.Data.SqlClient.SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ModifiedBy", userId);
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);

                    var rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<IEnumerable<GatePassRecord>> GetGatePassHistoryByStudentAsync(string studentName, Guid sessionId, int tenantCode)
        {
            var gatePasses = new List<GatePassRecord>();

            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT * FROM vwGatePasses
                    WHERE StudentName = @StudentName
                        AND SessionId = @SessionId 
                        AND TenantCode = @TenantCode 
                        AND IsDeleted = 0
                    ORDER BY Date DESC, TimeIn DESC";

                using (var cmd = new System.Data.SqlClient.SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StudentName", studentName);
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            gatePasses.Add(MapGatePassFromReader(reader));
                        }
                    }
                }
            }

            return gatePasses;
        }
        public async Task<IEnumerable<GatePassRecord>> GetGatePassHistoryByStudentIdAsync(Guid studentId, Guid sessionId, int tenantCode)
        {
            var gatePasses = new List<GatePassRecord>();

            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
SELECT
   *
FROM vwGatePasses g
WHERE
      g.StudentId  = @studentId
  AND g.SessionId  = @SessionId
  AND g.TenantCode = @TenantCode
  AND g.IsDeleted   = 0
ORDER BY
    g.Date   DESC,
    g.TimeIn DESC;";


                using (var cmd = new System.Data.SqlClient.SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@studentId", studentId);
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            gatePasses.Add(MapGatePassFromReader(reader));
                        }
                    }
                }
            }

            return gatePasses;
        }
        public async Task<bool> IncrementPrintCountAsync(Guid id, Guid sessionId, int tenantCode, Guid userId)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Update print count
                var updateQuery = @"
                    UPDATE GatePasses SET
                        PrintCount = PrintCount + 1,
                        ModifiedBy = @ModifiedBy,
                        ModifiedDate = GETDATE()
                    WHERE Id = @Id 
                        AND SessionId = @SessionId 
                        AND TenantCode = @TenantCode";

                using (var cmd = new System.Data.SqlClient.SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@ModifiedBy", userId);
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);

                    await cmd.ExecuteNonQueryAsync();
                }

                // Log print action
                var logQuery = @"
                    INSERT INTO GatePassPrintLog (Id, GatePassId, PrintedBy, PrintedDate, SessionId, TenantCode)
                    VALUES (NEWID(), @GatePassId, @PrintedBy, GETDATE(), @SessionId, @TenantCode)";

                using (var cmd = new System.Data.SqlClient.SqlCommand(logQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@GatePassId", id);
                    cmd.Parameters.AddWithValue("@PrintedBy", userId);
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);

                    await cmd.ExecuteNonQueryAsync();
                }

                return true;
            }
        }

        public string GeneratePassNumber(Guid sessionId, int tenantCode)
        {
            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                conn.Open();

                using (var cmd = new System.Data.SqlClient.SqlCommand("sp_GenerateGatePassNumber", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);
                    cmd.Parameters.AddWithValue("@Year", DateTime.Now.Year);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader["PassNumber"].ToString();
                        }
                    }
                }
            }

            // Fallback if stored procedure fails
            return $"GP-{DateTime.Now.Year}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        private string GeneratePassNumberInTransaction(Guid sessionId, int tenantCode, System.Data.SqlClient.SqlTransaction transaction)
        {
            var year = DateTime.Now.Year;
            var prefix = "GP";

            // Get the last number with lock within transaction
            var query = @"
                SELECT LastPassNumber 
                FROM GatePassSequence WITH (UPDLOCK, ROWLOCK)
                WHERE SessionId = @SessionId 
                    AND TenantCode = @TenantCode 
                    AND Year = @Year";

            int lastNumber = 0;
            using (var cmd = new System.Data.SqlClient.SqlCommand(query, transaction.Connection, transaction))
            {
                cmd.Parameters.AddWithValue("@SessionId", sessionId);
                cmd.Parameters.AddWithValue("@TenantCode", tenantCode);
                cmd.Parameters.AddWithValue("@Year", year);

                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    lastNumber = Convert.ToInt32(result);
                }
            }

            // Calculate new number
            int newNumber = lastNumber + 1;

            if (lastNumber == 0)
            {
                // Insert new sequence record
                var insertQuery = @"
                    INSERT INTO GatePassSequence (Id, SessionId, TenantCode, Year, LastPassNumber, Prefix, CreatedDate)
                    VALUES (NEWID(), @SessionId, @TenantCode, @Year, @NewNumber, @Prefix, GETDATE())";

                using (var cmd = new System.Data.SqlClient.SqlCommand(insertQuery, transaction.Connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@NewNumber", newNumber);
                    cmd.Parameters.AddWithValue("@Prefix", prefix);
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                // Update existing sequence
                var updateQuery = @"
                    UPDATE GatePassSequence
                    SET LastPassNumber = @NewNumber,
                        ModifiedDate = GETDATE()
                    WHERE SessionId = @SessionId 
                        AND TenantCode = @TenantCode 
                        AND Year = @Year";

                using (var cmd = new System.Data.SqlClient.SqlCommand(updateQuery, transaction.Connection, transaction))
                {
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@NewNumber", newNumber);
                    cmd.ExecuteNonQuery();
                }
            }

            // Return formatted pass number (e.g., GP-2025-0001)
            return $"{prefix}-{year}{newNumber.ToString("0000")}";
        }

        public async Task<IEnumerable<StudentSearchResult>> SearchStudentsAsync(string searchTerm, Guid sessionId, int tenantCode)
        {
            var students = new List<StudentSearchResult>();
            if (string.IsNullOrWhiteSpace(searchTerm))
                return students;

            using (var conn = new System.Data.SqlClient.SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = @"
        SELECT TOP 20 
            StudentId as Id, 
            ISNULL(FirstName, '') as Student,
            FatherName as Father, 
            MotherName as Mother, 
            ClassName as Class, 
            SectionName as Section,
            ClassId,
            Address, 
            Mobile as Mobile1, 
            FatherMobile as Mobile2,
            StudentNo,
            RollNo,
            AdmsnNo,
            SrNo
        FROM StudentInfoBasic
        WHERE (
            FirstName LIKE @SearchTerm 
            OR FatherName LIKE @SearchTerm 
            OR MotherName LIKE @SearchTerm
            OR Mobile LIKE @SearchTerm
            OR FatherMobile LIKE @SearchTerm
            OR MotherMobile LIKE @SearchTerm
            OR StudentNo LIKE @SearchTerm
            OR RollNo LIKE @SearchTerm
            OR SrNo LIKE @SearchTerm
        )
        AND SessionID = @SessionId 
        AND TenantCode = @TenantCode
        AND IsActive = 1
        AND IsDeleted = 0
        ORDER BY FirstName";

                using (var cmd = new System.Data.SqlClient.SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                    cmd.Parameters.AddWithValue("@SessionId", sessionId);
                    cmd.Parameters.AddWithValue("@TenantCode", tenantCode);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            students.Add(new StudentSearchResult
                            {
                                Id = Guid.Parse(reader["Id"].ToString()),
                                StudentId = Guid.Parse(reader["Id"].ToString()),
                                ClassId = Guid.Parse(reader["ClassId"].ToString()),
                                Student = reader["Student"]?.ToString()?.Trim(),
                                Father = reader["Father"]?.ToString(),
                                Mother = reader["Mother"]?.ToString(),
                                Section = reader["Section"]?.ToString(),
                                Class = reader["Class"]?.ToString(),
                                Address = reader["Address"]?.ToString(),
                                Mobile1 = reader["Mobile1"]?.ToString(),
                                Mobile2 = reader["Mobile2"]?.ToString(),
                                AdmNo = reader["AdmsnNo"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["AdmsnNo"])
                            });
                        }
                    }
                }
            }
            return students;
        }
        private GatePassRecord MapGatePassFromReader(System.Data.SqlClient.SqlDataReader reader)
        {
            return new GatePassRecord
            {
                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                StudentId = reader.GetGuid(reader.GetOrdinal("StudentId")),
                PassNo = reader["PassNo"]?.ToString(),
                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                StudentName = reader["StudentName"]?.ToString(),
                Father = reader["Father"]?.ToString(),
                Mother = reader["Mother"]?.ToString(),
                Class = reader["Class"]?.ToString(),
                ClassId = reader.IsDBNull(reader.GetOrdinal("ClassId")) ? (Guid?)null : reader.GetGuid(reader.GetOrdinal("ClassId")),
                Address = reader["Address"]?.ToString(),
                TimeIn = reader.IsDBNull(reader.GetOrdinal("TimeIn")) ? (TimeSpan?)null : (TimeSpan)reader["TimeIn"],
                TimeOut = reader.IsDBNull(reader.GetOrdinal("TimeOut")) ? (TimeSpan?)null : (TimeSpan)reader["TimeOut"],
                ParentGuardianName = reader["ParentGuardianName"]?.ToString(),
                GuardianMobile = reader["GuardianMobile"]?.ToString(),
                RelationshipToStudent = reader["RelationshipToStudent"]?.ToString(),
                ReasonForLeave = reader["ReasonForLeave"]?.ToString(),
                ReasonFor = reader["ReasonFor"]?.ToString(),
                PrintTime = reader.IsDBNull(reader.GetOrdinal("PrintTime")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("PrintTime")),
                PrintCount = reader.GetInt32(reader.GetOrdinal("PrintCount")),
                AdmNo = reader["AdmNo"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["AdmNo"]),
                D16 = reader["D16"]?.ToString(),
                D17 = reader["D17"]?.ToString(),
                D18 = reader["D18"]?.ToString()
            };
        }
    }
}