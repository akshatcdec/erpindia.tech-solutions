using Dapper;
using ERPIndia.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
namespace ERPIndia.Controllers
{
    public class FeeMapPlanModel
    {
        public Guid MapFeePlanID { get; set; }

        [Required]
        public Guid FeeHeadsID { get; set; }

        [Required]
        public Guid ClassID { get; set; }

        [Required]
        public Guid SectionID { get; set; }

        [Required]
        public Guid FeeCategoryID { get; set; }

        public int SessionYear { get; set; }

        public Guid SessionID { get; set; }

        public Guid TenantID { get; set; }

        public int TenantCode { get; set; }

        [Required]
        public string Frequency { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public decimal April { get; set; }

        [Required]
        public decimal May { get; set; }

        [Required]
        public decimal June { get; set; }

        [Required]
        public decimal July { get; set; }

        [Required]
        public decimal August { get; set; }

        [Required]
        public decimal September { get; set; }

        [Required]
        public decimal October { get; set; }

        [Required]
        public decimal November { get; set; }

        [Required]
        public decimal December { get; set; }

        [Required]
        public decimal January { get; set; }

        [Required]
        public decimal February { get; set; }

        [Required]
        public decimal March { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public Guid CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public Guid? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Additional display properties
        public string FeeHeadName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string CategoryName { get; set; }
        public int TotalCount { get; set; }
    }

    // DataTable parameters helper class
    public class DataTableParameters
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string SearchValue { get; set; }
        public int SortColumn { get; set; }
        public string SortDirection { get; set; }
    }

    public class FeeMapPlansController : BaseController
    {
        private readonly string _connectionString;
        private readonly IFeeManagementRepository _feeRepository;
        public FeeMapPlansController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            _feeRepository = new FeeManagementRepository(
               _connectionString,
               CurrentTenantID,
               CurrentSessionID,
               Utils.ParseInt(CurrentTenantCode),
               CurrentSessionYear,
               CurrentTenantUserID
           );

        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetAllFeeMapPlans(DataTableParameters dtParams)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Base query with joins
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
            ROW_NUMBER() OVER (ORDER BY fmp.CreatedDate DESC) AS RowNum,
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

                    // Create command with parameters
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters to command
                        command.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                        command.Parameters.AddWithValue("@SessionID", CurrentSessionID);
                        command.Parameters.AddWithValue("@StartRow", dtParams.Start + 1);
                        command.Parameters.AddWithValue("@EndRow", dtParams.Start + dtParams.Length);

                        // Execute query and read results
                        var results = new List<dynamic>();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new
                                {
                                    MapFeePlanID = Convert.ToInt32(reader["MapFeePlanID"]),
                                    FeeHeadsID = Convert.ToInt32(reader["FeeHeadsID"]),
                                    ClassID = Convert.ToInt32(reader["ClassID"]),
                                    SectionID = Convert.ToInt32(reader["SectionID"]),
                                    FeeCategoryID = Convert.ToInt32(reader["FeeCategoryID"]),
                                    Frequency = reader["Frequency"].ToString(),
                                    Amount = Convert.ToDecimal(reader["Amount"]),
                                    April = Convert.ToDecimal(reader["April"]),
                                    May = Convert.ToDecimal(reader["May"]),
                                    June = Convert.ToDecimal(reader["June"]),
                                    July = Convert.ToDecimal(reader["July"]),
                                    August = Convert.ToDecimal(reader["August"]),
                                    September = Convert.ToDecimal(reader["September"]),
                                    October = Convert.ToDecimal(reader["October"]),
                                    November = Convert.ToDecimal(reader["November"]),
                                    December = Convert.ToDecimal(reader["December"]),
                                    January = Convert.ToDecimal(reader["January"]),
                                    February = Convert.ToDecimal(reader["February"]),
                                    March = Convert.ToDecimal(reader["March"]),
                                    FeeHeadName = reader["FeeHeadName"].ToString(),
                                    ClassName = reader["ClassName"].ToString(),
                                    SectionName = reader["SectionName"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString(),
                                    TotalCount = Convert.ToInt32(reader["TotalCount"])
                                });
                            }
                        }

                        // Get total count
                        int totalCount = results.Any() ? results.First().TotalCount : 0;

                        return Json(new
                        {
                            draw = dtParams.Draw,
                            recordsTotal = totalCount,
                            recordsFiltered = totalCount,
                            data = results
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = true,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetFeeMapPlanById(Guid id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

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

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@MapFeePlanID", id);
                        command.Parameters.AddWithValue("@TenantID", CurrentTenantID);

                        var result = new object();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                result = new
                                {
                                    MapFeePlanID = Convert.ToInt32(reader["MapFeePlanID"]),
                                    FeeHeadsID = Convert.ToInt32(reader["FeeHeadsID"]),
                                    ClassID = Convert.ToInt32(reader["ClassID"]),
                                    SectionID = Convert.ToInt32(reader["SectionID"]),
                                    FeeCategoryID = Convert.ToInt32(reader["FeeCategoryID"]),
                                    Frequency = reader["Frequency"].ToString(),
                                    Amount = Convert.ToDecimal(reader["Amount"]),
                                    April = Convert.ToDecimal(reader["April"]),
                                    May = Convert.ToDecimal(reader["May"]),
                                    June = Convert.ToDecimal(reader["June"]),
                                    July = Convert.ToDecimal(reader["July"]),
                                    August = Convert.ToDecimal(reader["August"]),
                                    September = Convert.ToDecimal(reader["September"]),
                                    October = Convert.ToDecimal(reader["October"]),
                                    November = Convert.ToDecimal(reader["November"]),
                                    December = Convert.ToDecimal(reader["December"]),
                                    January = Convert.ToDecimal(reader["January"]),
                                    February = Convert.ToDecimal(reader["February"]),
                                    March = Convert.ToDecimal(reader["March"]),
                                    FeeHeadName = reader["FeeHeadName"].ToString(),
                                    ClassName = reader["ClassName"].ToString(),
                                    SectionName = reader["SectionName"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString()
                                };
                            }
                        }

                        return Json(new
                        {
                            success = true,
                            data = result
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult CreateFeeMapPlan(FeeMapPlanModel model)
        {
            try
            {
                // Validate input
                if (model == null)
                {
                    return Json(new { success = false, message = "Invalid model" });
                }

                // Prepare model for insertion
                model.MapFeePlanID = Guid.NewGuid();
                model.TenantID = CurrentTenantID;
                model.TenantCode = Utils.ParseInt(CurrentTenantCode);
                model.SessionID = CurrentSessionID;
                model.SessionYear = CurrentSessionYear;
                model.CreatedBy = CurrentTenantUserID;
                model.CreatedDate = DateTime.Now;
                model.IsActive = true;
                model.IsDeleted = false;

                // Set default value for Frequency if not provided
                if (string.IsNullOrEmpty(model.Frequency))
                {
                    model.Frequency = "NA";
                }

                // Ensure decimal fields have valid values
                if (model.Amount <= 0)
                {
                    model.Amount = 0;
                }

                // Set all monthly fields to 0 if not provided
                if (model.April <= 0) model.April = 0;
                if (model.May <= 0) model.May = 0;
                if (model.June <= 0) model.June = 0;
                if (model.July <= 0) model.July = 0;
                if (model.August <= 0) model.August = 0;
                if (model.September <= 0) model.September = 0;
                if (model.October <= 0) model.October = 0;
                if (model.November <= 0) model.November = 0;
                if (model.December <= 0) model.December = 0;
                if (model.January <= 0) model.January = 0;
                if (model.February <= 0) model.February = 0;
                if (model.March <= 0) model.March = 0;

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
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

                            var duplicateCount = connection.ExecuteScalar<int>(duplicateCheck, model, transaction);
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

                                int rowsAffected2 = connection.Execute(updateQuery, model, transaction);
                                transaction.Commit();
                                return Json(new { success = true, message = "Fee Map Plan updated successfully" });
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

                            int rowsAffected = connection.Execute(insertQuery, model, transaction);
                            // int successCount = await _feeRepository.ApplyFeeStructuresToAllStudentsAsync(model.ClassID, model.SectionID, model.FeeCategoryID);

                            transaction.Commit();

                            return Json(new { success = true, message = "Fee Map Plan created successfully" });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetFeeSetupList(string feeHeadId = "", string classId = "", string sectionId = "", string categoryId = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Base query with all required columns
                    var query = @"
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
        AND fmp.IsDeleted = 0";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add base parameters
                        command.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                        command.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                        // Add filters if provided
                        if (!string.IsNullOrEmpty(feeHeadId))
                        {
                            query += " AND fmp.FeeHeadsID = @FeeHeadID";
                            command.Parameters.AddWithValue("@FeeHeadID", Guid.Parse(feeHeadId));
                        }

                        if (!string.IsNullOrEmpty(classId))
                        {
                            query += " AND fmp.ClassID = @ClassID";
                            command.Parameters.AddWithValue("@ClassID", Guid.Parse(classId));
                        }

                        if (!string.IsNullOrEmpty(sectionId))
                        {
                            query += " AND fmp.SectionID = @SectionID";
                            command.Parameters.AddWithValue("@SectionID", Guid.Parse(sectionId));
                        }

                        if (!string.IsNullOrEmpty(categoryId))
                        {
                            query += " AND fmp.FeeCategoryID = @CategoryID";
                            command.Parameters.AddWithValue("@CategoryID", Guid.Parse(categoryId));
                        }

                        // Order by most recently created
                        query += " ORDER BY fmp.CreatedDate DESC";

                        // Update the command with the final query string
                        command.CommandText = query;

                        var results = new List<dynamic>();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new
                                {
                                    Id = Guid.Parse(reader["Id"].ToString()),
                                    FeeHeadsID = Guid.Parse(reader["FeeHeadsID"].ToString()),
                                    ClassID = Guid.Parse(reader["ClassID"].ToString()),
                                    SectionID = Guid.Parse(reader["SectionID"].ToString()),
                                    FeeCategoryID = Guid.Parse(reader["FeeCategoryID"].ToString()),
                                    Frequency = reader["Frequency"].ToString(),
                                    Amount = Convert.ToDecimal(reader["Amount"]),
                                    April = Convert.ToDecimal(reader["April"]),
                                    May = Convert.ToDecimal(reader["May"]),
                                    June = Convert.ToDecimal(reader["June"]),
                                    July = Convert.ToDecimal(reader["July"]),
                                    August = Convert.ToDecimal(reader["August"]),
                                    September = Convert.ToDecimal(reader["September"]),
                                    October = Convert.ToDecimal(reader["October"]),
                                    November = Convert.ToDecimal(reader["November"]),
                                    December = Convert.ToDecimal(reader["December"]),
                                    January = Convert.ToDecimal(reader["January"]),
                                    February = Convert.ToDecimal(reader["February"]),
                                    March = Convert.ToDecimal(reader["March"]),
                                    FeeHeadName = reader["FeeHeadName"].ToString(),
                                    ClassName = reader["ClassName"].ToString(),
                                    SectionName = reader["SectionName"].ToString(),
                                    CategoryName = reader["CategoryName"].ToString()
                                });
                            }
                        }

                        return Json(results, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public async Task<JsonResult> UpdateFeeMapPlan(FeeMapPlanModel model)
        {
            try
            {
                // Validate input
                if (model == null || model.MapFeePlanID == Guid.Empty)
                {
                    return Json(new { success = false, message = "Invalid model" });
                }

                // Prepare model for update
                model.TenantID = CurrentTenantID;
                model.ModifiedBy = CurrentTenantUserID;
                model.ModifiedDate = DateTime.Now;

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
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

                            var duplicateCount = connection.ExecuteScalar<int>(duplicateCheck, model, transaction);
                            if (duplicateCount > 0)
                            {
                                return Json(new { success = false, message = "A similar Fee Map Plan already exists" });
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

                            int rowsAffected = connection.Execute(updateQuery, model, transaction);

                            int successCount = await _feeRepository.ApplyFeeStructuresToAllStudentsAsync(model.ClassID, model.SectionID, model.FeeCategoryID);

                            transaction.Commit();

                            return Json(new { success = rowsAffected > 0, message = "Fee Map Plan updated successfully" });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteFeeSetup(Guid id)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Soft delete Fee Map Plan
                            var deleteQuery = @" DELETE FROM FeeMapPlans WHERE MapFeePlanID = @MapFeePlanID ";

                            var parameters = new
                            {
                                MapFeePlanID = id,
                            };

                            int rowsAffected = connection.Execute(deleteQuery, parameters, transaction);

                            transaction.Commit();

                            return Json(new
                            {
                                success = rowsAffected > 0,
                                message = rowsAffected > 0
                                    ? "Fee setup deleted successfully"
                                    : "Fee setup not found or already deleted"
                            });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}