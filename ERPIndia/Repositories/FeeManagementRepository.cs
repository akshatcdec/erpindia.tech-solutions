using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using ERPIndia.Controllers;
using ERPIndia.Models;

namespace ERPIndia.Repositories
{
    public interface IFeeManagementRepository
    {
        Task<IEnumerable<FeeMapPlanModel>> GetAllFeeMapPlansAsync(int start, int length, string searchValue, string sortColumn, string sortDirection);
        Task<int> GetTotalFeeMapPlansCountAsync(string searchValue);
        Task<FeeMapPlanModel> GetFeeMapPlanByIdAsync(Guid id);
        Task<bool> CreateFeeMapPlanAsync(FeeMapPlanModel model);
        Task<bool> UpdateFeeMapPlanAsync(FeeMapPlanModel model);
        Task<bool> DeleteFeeMapPlanAsync(Guid id);
        Task<IEnumerable<FeeMapPlanModel>> GetFeeSetupListAsync(string feeHeadId = "", string classId = "", string sectionId = "", string categoryId = "");
        Task<bool> ApplyFeeStructureToStudentAsync(Guid studentId, Guid classId, Guid sectionId, Guid feeCategoryId);
        Task<int> ApplyFeeStructuresToAllStudentsAsync(Guid? classId = null, Guid? sectionId = null, Guid? feeCategoryId = null);
        Task<IEnumerable<StudentFeeStructureModel>> GetStudentFeeStructuresAsync(Guid studentId);
    }

    public class FeeManagementRepository : IFeeManagementRepository
    {
        private readonly string _connectionString;
        private readonly Guid _currentTenantID;
        private readonly Guid _currentSessionID;
        private readonly int _currentTenantCode;
        private readonly int _currentSessionYear;
        private readonly Guid _currentTenantUserID;

        public FeeManagementRepository(
            string connectionString,
            Guid currentTenantID,
            Guid currentSessionID,
            int currentTenantCode,
            int currentSessionYear,
            Guid currentTenantUserID)
        {
            _connectionString = connectionString;
            _currentTenantID = currentTenantID;
            _currentSessionID = currentSessionID;
            _currentTenantCode = currentTenantCode;
            _currentSessionYear = currentSessionYear;
            _currentTenantUserID = currentTenantUserID;
        }

        public async Task<IEnumerable<FeeMapPlanModel>> GetAllFeeMapPlansAsync(int start, int length, string searchValue, string sortColumn, string sortDirection)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var orderBy = string.IsNullOrEmpty(sortColumn)
                    ? "fmp.CreatedDate DESC"
                    : $"{sortColumn} {sortDirection}";

                var query = @"
                    WITH FeeMapPlanCTE AS (
                        SELECT 
                            fmp.MapFeePlanID, 
                            fmp.FeeHeadsID,
                            fmp.ClassID,
                            fmp.SectionID,
                            fmp.FeeCategoryID,
                            fmp.Frequency,
                            fmp.Amount,
                            fmp.April,
                            fmp.May,
                            fmp.June,
                            fmp.July,
                            fmp.August,
                            fmp.September,
                            fmp.October,
                            fmp.November,
                            fmp.December,
                            fmp.January,
                            fmp.February,
                            fmp.March,
                            fh.HeadsName AS FeeHeadName,
                            cls.ClassName,
                            sec.SectionName,
                            cat.CategoryName,
                            ROW_NUMBER() OVER (ORDER BY " + orderBy + @") AS RowNum,
                            COUNT(*) OVER () AS TotalCount
                        FROM FeeMapPlans fmp
                        INNER JOIN FeeHeadsMaster fh ON fmp.FeeHeadsID = fh.FeeHeadsID
                        INNER JOIN AcademicClassMaster cls ON fmp.ClassID = cls.ClassID
                        INNER JOIN AcademicSectionMaster sec ON fmp.SectionID = sec.SectionID
                        INNER JOIN FeeCategoryMaster cat ON fmp.FeeCategoryID = cat.FeeCategoryID
                        WHERE 
                            fmp.TenantID = @TenantID 
                            AND fmp.SessionID = @SessionID 
                            AND fmp.IsDeleted = 0
                            AND (
                                @SearchValue = '' OR
                                fh.HeadsName LIKE '%' + @SearchValue + '%' OR
                                cls.ClassName LIKE '%' + @SearchValue + '%' OR
                                sec.SectionName LIKE '%' + @SearchValue + '%' OR
                                cat.CategoryName LIKE '%' + @SearchValue + '%'
                            )
                    )
                    SELECT 
                        MapFeePlanID, 
                        FeeHeadsID,
                        ClassID,
                        SectionID,
                        FeeCategoryID,
                        Frequency,
                        Amount,
                        April,
                        May,
                        June,
                        July,
                        August,
                        September,
                        October,
                        November,
                        December,
                        January,
                        February,
                        March,
                        FeeHeadName,
                        ClassName,
                        SectionName,
                        CategoryName,
                        TotalCount
                    FROM FeeMapPlanCTE
                    WHERE RowNum BETWEEN @StartRow AND @EndRow";

                var parameters = new DynamicParameters();
                parameters.Add("@TenantID", _currentTenantID);
                parameters.Add("@SessionID", _currentSessionID);
                parameters.Add("@SearchValue", searchValue ?? string.Empty);
                parameters.Add("@StartRow", start + 1);
                parameters.Add("@EndRow", start + length);

                return await connection.QueryAsync<FeeMapPlanModel>(query, parameters);
            }
        }

        public async Task<int> GetTotalFeeMapPlansCountAsync(string searchValue)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT COUNT(*)
                    FROM FeeMapPlans fmp
                    INNER JOIN FeeHeadsMaster fh ON fmp.FeeHeadsID = fh.FeeHeadsID
                    INNER JOIN AcademicClassMaster cls ON fmp.ClassID = cls.ClassID
                    INNER JOIN AcademicSectionMaster sec ON fmp.SectionID = sec.SectionID
                    INNER JOIN FeeCategoryMaster cat ON fmp.FeeCategoryID = cat.FeeCategoryID
                    WHERE 
                        fmp.TenantID = @TenantID 
                        AND fmp.SessionID = @SessionID 
                        AND fmp.IsDeleted = 0
                        AND (
                            @SearchValue = '' OR
                            fh.HeadsName LIKE '%' + @SearchValue + '%' OR
                            cls.ClassName LIKE '%' + @SearchValue + '%' OR
                            sec.SectionName LIKE '%' + @SearchValue + '%' OR
                            cat.CategoryName LIKE '%' + @SearchValue + '%'
                        )";

                var parameters = new DynamicParameters();
                parameters.Add("@TenantID", _currentTenantID);
                parameters.Add("@SessionID", _currentSessionID);
                parameters.Add("@SearchValue", searchValue ?? string.Empty);

                return await connection.ExecuteScalarAsync<int>(query, parameters);
            }
        }

        public async Task<FeeMapPlanModel> GetFeeMapPlanByIdAsync(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        fmp.*, 
                        fh.HeadsName AS FeeHeadName,
                        cls.ClassName,
                        sec.SectionName,
                        cat.CategoryName
                    FROM FeeMapPlans fmp
                    INNER JOIN FeeHeadsMaster fh ON fmp.FeeHeadsID = fh.FeeHeadsID
                    INNER JOIN AcademicClassMaster cls ON fmp.ClassID = cls.ClassID
                    INNER JOIN AcademicSectionMaster sec ON fmp.SectionID = sec.SectionID
                    INNER JOIN FeeCategoryMaster cat ON fmp.FeeCategoryID = cat.FeeCategoryID
                    WHERE 
                        fmp.MapFeePlanID = @MapFeePlanID 
                        AND fmp.TenantID = @TenantID 
                        AND fmp.IsDeleted = 0";

                var parameters = new DynamicParameters();
                parameters.Add("@MapFeePlanID", id);
                parameters.Add("@TenantID", _currentTenantID);

                return await connection.QueryFirstOrDefaultAsync<FeeMapPlanModel>(query, parameters);
            }
        }

        public async Task<bool> CreateFeeMapPlanAsync(FeeMapPlanModel model)
        {
            // Validate input
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "Model cannot be null");
            }

            // Prepare model for insertion
            model.MapFeePlanID = Guid.NewGuid();
            model.TenantID = _currentTenantID;
            model.TenantCode = _currentTenantCode;
            model.SessionID = _currentSessionID;
            model.SessionYear = _currentSessionYear;
            model.CreatedBy = _currentTenantUserID;
            model.CreatedDate = DateTime.Now;
            model.IsActive = true;
            model.IsDeleted = false;

            // Set default value for Frequency if not provided
            if (string.IsNullOrEmpty(model.Frequency))
            {
                model.Frequency = "NA";
            }

            // Ensure all monthly fields have valid values
            model.Amount = model.Amount <= 0 ? 0 : model.Amount;
            model.April = model.April <= 0 ? 0 : model.April;
            model.May = model.May <= 0 ? 0 : model.May;
            model.June = model.June <= 0 ? 0 : model.June;
            model.July = model.July <= 0 ? 0 : model.July;
            model.August = model.August <= 0 ? 0 : model.August;
            model.September = model.September <= 0 ? 0 : model.September;
            model.October = model.October <= 0 ? 0 : model.October;
            model.November = model.November <= 0 ? 0 : model.November;
            model.December = model.December <= 0 ? 0 : model.December;
            model.January = model.January <= 0 ? 0 : model.January;
            model.February = model.February <= 0 ? 0 : model.February;
            model.March = model.March <= 0 ? 0 : model.March;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Check for existing duplicate
                        var duplicateCheck = @"
                            SELECT COUNT(*) FROM FeeMapPlans 
                            WHERE 
                                TenantID = @TenantID 
                                AND SessionID = @SessionID 
                                AND FeeHeadsID = @FeeHeadsID 
                                AND ClassID = @ClassID 
                                AND SectionID = @SectionID 
                                AND FeeCategoryID = @FeeCategoryID 
                                AND IsDeleted = 0";

                        var duplicateCount = await connection.ExecuteScalarAsync<int>(duplicateCheck, model, transaction);
                        if (duplicateCount > 0)
                        {
                            // Update existing record instead of creating a new one
                            var updateQuery = @"
                                UPDATE FeeMapPlans 
                                SET 
                                    Frequency = @Frequency,
                                    Amount = @Amount,
                                    April = @April,
                                    May = @May,
                                    June = @June,
                                    July = @July,
                                    August = @August,
                                    September = @September,
                                    October = @October,
                                    November = @November,
                                    December = @December,
                                    January = @January,
                                    February = @February,
                                    March = @March,
                                    IsActive = @IsActive, 
                                    ModifiedBy = @CreatedBy, 
                                    ModifiedDate = @CreatedDate
                                WHERE 
                                    TenantID = @TenantID 
                                    AND SessionID = @SessionID 
                                    AND FeeHeadsID = @FeeHeadsID 
                                    AND ClassID = @ClassID 
                                    AND SectionID = @SectionID 
                                    AND FeeCategoryID = @FeeCategoryID 
                                    AND IsDeleted = 0";

                            await connection.ExecuteAsync(updateQuery, model, transaction);
                            transaction.Commit();
                            return true;
                        }

                        // Insert new Fee Map Plan
                        var insertQuery = @"
                            INSERT INTO FeeMapPlans 
                            (MapFeePlanID, FeeHeadsID, ClassID, SectionID, FeeCategoryID, 
                             SessionYear, SessionID, TenantID, TenantCode, 
                             Frequency, Amount,
                             April, May, June, July, August, September,
                             October, November, December, January, February, March,
                             IsActive, IsDeleted, CreatedBy, CreatedDate)
                            VALUES 
                            (@MapFeePlanID, @FeeHeadsID, @ClassID, @SectionID, @FeeCategoryID, 
                             @SessionYear, @SessionID, @TenantID, @TenantCode, 
                             @Frequency, @Amount,
                             @April, @May, @June, @July, @August, @September,
                             @October, @November, @December, @January, @February, @March,
                             @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                        await connection.ExecuteAsync(insertQuery, model, transaction);
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

        public async Task<bool> UpdateFeeMapPlanAsync(FeeMapPlanModel model)
        {
            // Validate input
            if (model == null || model.MapFeePlanID == Guid.Empty)
            {
                throw new ArgumentException("Invalid model or MapFeePlanID");
            }

            // Prepare model for update
            model.TenantID = _currentTenantID;
            model.ModifiedBy = _currentTenantUserID;
            model.ModifiedDate = DateTime.Now;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Check for existing duplicate excluding current record
                        var duplicateCheck = @"
                            SELECT COUNT(*) FROM FeeMapPlans 
                            WHERE 
                                TenantID = @TenantID 
                                AND SessionID = @SessionID 
                                AND FeeHeadsID = @FeeHeadsID 
                                AND ClassID = @ClassID 
                                AND SectionID = @SectionID 
                                AND FeeCategoryID = @FeeCategoryID 
                                AND MapFeePlanID != @MapFeePlanID
                                AND IsDeleted = 0";

                        var duplicateCount = await connection.ExecuteScalarAsync<int>(duplicateCheck, model, transaction);
                        if (duplicateCount > 0)
                        {
                            throw new DuplicateNameException("A similar Fee Map Plan already exists");
                        }

                        // Update Fee Map Plan
                        var updateQuery = @"
                            UPDATE FeeMapPlans 
                            SET 
                                FeeHeadsID = @FeeHeadsID, 
                                ClassID = @ClassID, 
                                SectionID = @SectionID, 
                                FeeCategoryID = @FeeCategoryID,
                                Frequency = @Frequency,
                                Amount = @Amount,
                                April = @April,
                                May = @May,
                                June = @June,
                                July = @July,
                                August = @August,
                                September = @September,
                                October = @October,
                                November = @November,
                                December = @December,
                                January = @January,
                                February = @February,
                                March = @March,
                                IsActive = @IsActive, 
                                ModifiedBy = @ModifiedBy, 
                                ModifiedDate = @ModifiedDate
                            WHERE 
                                MapFeePlanID = @MapFeePlanID 
                                AND TenantID = @TenantID 
                                AND IsDeleted = 0";

                        int rowsAffected = await connection.ExecuteAsync(updateQuery, model, transaction);
                        transaction.Commit();
                        return rowsAffected > 0;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> DeleteFeeMapPlanAsync(Guid id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Soft delete Fee Map Plan
                        var deleteQuery = @"
                            UPDATE FeeMapPlans 
                            SET 
                                IsDeleted = 1, 
                                ModifiedBy = @ModifiedBy,
                                ModifiedDate = @ModifiedDate
                            WHERE 
                                MapFeePlanID = @MapFeePlanID 
                                AND TenantID = @TenantID 
                                AND IsDeleted = 0";

                        var parameters = new DynamicParameters();
                        parameters.Add("@MapFeePlanID", id);
                        parameters.Add("@TenantID", _currentTenantID);
                        parameters.Add("@ModifiedBy", _currentTenantUserID);
                        parameters.Add("@ModifiedDate", DateTime.Now);

                        int rowsAffected = await connection.ExecuteAsync(deleteQuery, parameters, transaction);
                        transaction.Commit();
                        return rowsAffected > 0;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<IEnumerable<FeeMapPlanModel>> GetFeeSetupListAsync(string feeHeadId = "", string classId = "", string sectionId = "", string categoryId = "")
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Build query with all required columns
                var queryBuilder = new System.Text.StringBuilder(@"
                    SELECT 
                        fmp.MapFeePlanID AS Id, 
                        fmp.FeeHeadsID,
                        fmp.ClassID,
                        fmp.SectionID,
                        fmp.FeeCategoryID,
                        fmp.Frequency,
                        fmp.Amount,
                        fmp.April,
                        fmp.May,
                        fmp.June,
                        fmp.July,
                        fmp.August,
                        fmp.September,
                        fmp.October,
                        fmp.November,
                        fmp.December,
                        fmp.January,
                        fmp.February,
                        fmp.March,
                        fh.HeadsName AS FeeHeadName,
                        cls.ClassName,
                        sec.SectionName,
                        cat.CategoryName
                    FROM FeeMapPlans fmp
                    INNER JOIN FeeHeadsMaster fh ON fmp.FeeHeadsID = fh.FeeHeadsID
                    INNER JOIN AcademicClassMaster cls ON fmp.ClassID = cls.ClassID
                    INNER JOIN AcademicSectionMaster sec ON fmp.SectionID = sec.SectionID
                    INNER JOIN FeeCategoryMaster cat ON fmp.FeeCategoryID = cat.FeeCategoryID
                    WHERE 
                        fmp.TenantID = @TenantID 
                        AND fmp.SessionID = @SessionID 
                        AND fmp.IsDeleted = 0");

                var parameters = new DynamicParameters();
                parameters.Add("@TenantID", _currentTenantID);
                parameters.Add("@SessionID", _currentSessionID);

                // Add filters if provided
                if (!string.IsNullOrEmpty(feeHeadId))
                {
                    queryBuilder.Append(" AND fmp.FeeHeadsID = @FeeHeadID");
                    parameters.Add("@FeeHeadID", Guid.Parse(feeHeadId));
                }

                if (!string.IsNullOrEmpty(classId))
                {
                    queryBuilder.Append(" AND fmp.ClassID = @ClassID");
                    parameters.Add("@ClassID", Guid.Parse(classId));
                }

                if (!string.IsNullOrEmpty(sectionId))
                {
                    queryBuilder.Append(" AND fmp.SectionID = @SectionID");
                    parameters.Add("@SectionID", Guid.Parse(sectionId));
                }

                if (!string.IsNullOrEmpty(categoryId))
                {
                    queryBuilder.Append(" AND fmp.FeeCategoryID = @CategoryID");
                    parameters.Add("@CategoryID", Guid.Parse(categoryId));
                }

                // Order by most recently created
                queryBuilder.Append(" ORDER BY fmp.CreatedDate DESC");

                return await connection.QueryAsync<FeeMapPlanModel>(queryBuilder.ToString(), parameters);
            }
        }

        public async Task<bool> ApplyFeeStructureToStudentAsync(Guid studentId, Guid classId, Guid sectionId, Guid feeCategoryId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Set up parameters for stored procedure
                    var parameters = new DynamicParameters();
                    parameters.Add("@StudentID", studentId);
                    parameters.Add("@ClassID", classId);
                    parameters.Add("@SectionID", sectionId);
                    parameters.Add("@FeeCategoryID", feeCategoryId);
                    parameters.Add("@TenantID", _currentTenantID);
                    parameters.Add("@SessionID", _currentSessionID);
                    parameters.Add("@CreatedBy", _currentTenantUserID);
                    parameters.Add("@Success", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    // Execute the stored procedure
                    await connection.ExecuteAsync(
                        "sp_ApplyFeeStructureToStudent",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                    // Get the output parameter value
                    bool success = parameters.Get<bool>("@Success");
                    return success;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> ApplyFeeStructuresToAllStudentsAsync(Guid? classId = null, Guid? sectionId = null, Guid? feeCategoryId = null)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Set up parameters for stored procedure
                var parameters = new DynamicParameters();
                parameters.Add("@TenantID", _currentTenantID);
                parameters.Add("@SessionID", _currentSessionID);
                parameters.Add("@ClassID", classId.HasValue ? classId : null);
                parameters.Add("@SectionID", sectionId.HasValue ? sectionId : null);
                parameters.Add("@FeeCategoryID", feeCategoryId.HasValue ? feeCategoryId : null);
                parameters.Add("@CreatedBy", _currentTenantUserID);
                parameters.Add("@SuccessCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

                // Execute the stored procedure
                await connection.ExecuteAsync(
                    "sp_ApplyFeeStructuresToAllStudents",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                // Get the output parameter value
                int successCount = parameters.Get<int>("@SuccessCount");
                return successCount;
            }
        }

        public async Task<IEnumerable<StudentFeeStructureModel>> GetStudentFeeStructuresAsync(Guid studentId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        sfs.*,
                        fh.HeadsName AS FeeHeadName,
                        (sfs.April + sfs.May + sfs.June + sfs.July + sfs.August + sfs.September +
                         sfs.October + sfs.November + sfs.December + sfs.January + sfs.February + sfs.March) AS TotalAmount
                    FROM StudentFeeStructures sfs
                    INNER JOIN FeeHeadsMaster fh ON sfs.FeeHeadID = fh.FeeHeadsID
                    WHERE 
                        sfs.StudentId = @StudentId
                        AND sfs.TenantID = @TenantID
                        AND sfs.IsDeleted = 0
                    ORDER BY fh.HeadsName";

                var parameters = new DynamicParameters();
                parameters.Add("@StudentId", studentId);
                parameters.Add("@TenantID", _currentTenantID);

                return await connection.QueryAsync<StudentFeeStructureModel>(query, parameters);
            }
        }

        // Helper method to create student fee structure from fee map plan
        private async Task CreateStudentFeeStructureFromPlanAsync(
            Guid studentId,
            FeeMapPlanModel plan,
            int schoolCode,
            SqlConnection connection,
            IDbTransaction transaction)
        {
            var insertQuery = @"
                INSERT INTO StudentFeeStructures
                (StudentId, FeeHeadID, April, May, June, July, August, September,
                October, November, December, January, February, March,
                TenantID, SchoolCode, CreatedBy, CreatedDate, IsActive, IsDeleted)
                VALUES
                (@StudentId, @FeeHeadID, @April, @May, @June, @July, @August, @September,
                @October, @November, @December, @January, @February, @March,
                @TenantID, @SchoolCode, @CreatedBy, @CreatedDate, 1, 0)";

            var parameters = new DynamicParameters();
            parameters.Add("@StudentId", studentId);
            parameters.Add("@FeeHeadID", plan.FeeHeadsID);
            parameters.Add("@April", plan.April);
            parameters.Add("@May", plan.May);
            parameters.Add("@June", plan.June);
            parameters.Add("@July", plan.July);
            parameters.Add("@August", plan.August);
            parameters.Add("@September", plan.September);
            parameters.Add("@October", plan.October);
            parameters.Add("@November", plan.November);
            parameters.Add("@December", plan.December);
            parameters.Add("@January", plan.January);
            parameters.Add("@February", plan.February);
            parameters.Add("@March", plan.March);
            parameters.Add("@TenantID", _currentTenantID);
            parameters.Add("@SchoolCode", schoolCode);
            parameters.Add("@CreatedBy", Convert.ToInt32(_currentTenantUserID.ToString().Substring(0, 8), 16) % 1000000);
            parameters.Add("@CreatedDate", DateTime.Now);

            await connection.ExecuteAsync(insertQuery, parameters, transaction);
        }
    }

    // Student Fee Structure Model
    public class StudentFeeStructureModel
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid FeeHeadID { get; set; }
        public decimal April { get; set; }
        public decimal May { get; set; }
        public decimal June { get; set; }
        public decimal July { get; set; }
        public decimal August { get; set; }
        public decimal September { get; set; }
        public decimal October { get; set; }
        public decimal November { get; set; }
        public decimal December { get; set; }
        public decimal January { get; set; }
        public decimal February { get; set; }
        public decimal March { get; set; }
        public Guid TenantID { get; set; }
        public int SchoolCode { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Additional properties
        public string FeeHeadName { get; set; }
        public decimal TotalAmount { get; set; }
    }
}