using Dapper;
using ERPIndia.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class FeeMapConcessionModel
    {
        public Guid MapConcessionID { get; set; }

        [Required]
        public Guid FeeHeadsID { get; set; }

        [Required]
        public Guid ClassID { get; set; }

        [Required]
        public Guid SectionID { get; set; }

        [Required]
        public Guid FeeDiscountID { get; set; }

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
        public string DiscountName { get; set; }
        public int TotalCount { get; set; }
    }

    public class FeeMapConcessionController : BaseController
    {
        private readonly string _connectionString;
        private readonly IFeeManagementRepository _feeRepository;

        public FeeMapConcessionController()
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
        public JsonResult GetFeeConcessionList(string feeHeadId = "", string classId = "",
                                  string sectionId = "", string discountId = "")
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Base query with all required columns
                    var query = @"
                    SELECT 
                        fmc.MapConcessionID AS Id, 
                        fmc.FeeHeadsID,
                        fmc.ClassID,
                        fmc.SectionID,
                        fmc.FeeDiscountID,
                        fmc.Frequency,
                        fmc.Amount,
                        fmc.April,
                        fmc.May,
                        fmc.June,
                        fmc.July,
                        fmc.August,
                        fmc.September,
                        fmc.October,
                        fmc.November,
                        fmc.December,
                        fmc.January,
                        fmc.February,
                        fmc.March,
                        fh.HeadsName AS FeeHeadName,
                        cls.ClassName,
                        sec.SectionName,
                        disc.DiscountName
                    FROM FeeMapConcession fmc
                    INNER JOIN FeeHeadsMaster fh ON fmc.FeeHeadsID = fh.FeeHeadsID
                    INNER JOIN AcademicClassMaster cls ON fmc.ClassID = cls.ClassID
                    INNER JOIN AcademicSectionMaster sec ON fmc.SectionID = sec.SectionID
                    INNER JOIN FeeDiscountMaster disc ON fmc.FeeDiscountID = disc.FeeDiscountID
                    WHERE 
                        fmc.TenantID = @TenantID 
                        AND fmc.SessionID = @SessionID 
                        AND fmc.IsDeleted = 0";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add base parameters
                        command.Parameters.AddWithValue("@TenantID", CurrentTenantID);
                        command.Parameters.AddWithValue("@SessionID", CurrentSessionID);

                        // Add filters if provided
                        if (!string.IsNullOrEmpty(feeHeadId))
                        {
                            query += " AND fmc.FeeHeadsID = @FeeHeadID";
                            command.Parameters.AddWithValue("@FeeHeadID", Guid.Parse(feeHeadId));
                        }

                        if (!string.IsNullOrEmpty(classId))
                        {
                            query += " AND fmc.ClassID = @ClassID";
                            command.Parameters.AddWithValue("@ClassID", Guid.Parse(classId));
                        }

                        if (!string.IsNullOrEmpty(sectionId))
                        {
                            query += " AND fmc.SectionID = @SectionID";
                            command.Parameters.AddWithValue("@SectionID", Guid.Parse(sectionId));
                        }

                        if (!string.IsNullOrEmpty(discountId))
                        {
                            query += " AND fmc.FeeDiscountID = @DiscountID";
                            command.Parameters.AddWithValue("@DiscountID", Guid.Parse(discountId));
                        }

                        // Order by most recently created
                        query += " ORDER BY fmc.CreatedDate DESC";

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
                                    FeeDiscountID = Guid.Parse(reader["FeeDiscountID"].ToString()),
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
                                    DiscountName = reader["DiscountName"].ToString()
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
        public JsonResult CreateFeeConcession(FeeMapConcessionModel model)
        {
            try
            {
                // Validate input
                if (model == null)
                {
                    return Json(new { success = false, message = "Invalid model" });
                }

                // Prepare model for insertion
                model.MapConcessionID = Guid.NewGuid();
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
                            SELECT COUNT(*) FROM FeeMapConcession 
                            WHERE 
                                TenantID = @TenantID 
                                AND SessionID = @SessionID 
                                AND FeeHeadsID = @FeeHeadsID 
                                AND ClassID = @ClassID 
                                AND SectionID = @SectionID 
                                AND FeeDiscountID = @FeeDiscountID 
                                AND IsDeleted = 0";

                            var duplicateCount = connection.ExecuteScalar<int>(duplicateCheck, model, transaction);
                            if (duplicateCount > 0)
                            {
                                // Update existing record instead of creating a new one
                                var updateQuery = @"
                                UPDATE FeeMapConcession 
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
                                    AND FeeDiscountID = @FeeDiscountID 
                                    AND IsDeleted = 0";

                                int rowsAffected = connection.Execute(updateQuery, model, transaction);
                                transaction.Commit();
                                return Json(new { success = true, message = "Fee Concession updated successfully" });
                            }

                            // Insert new Fee Concession
                            var insertQuery = @"
                            INSERT INTO FeeMapConcession 
                            (MapConcessionID, FeeHeadsID, ClassID, SectionID, FeeDiscountID, 
                             SessionYear, SessionID, TenantID, TenantCode, 
                             Frequency, Amount,
                             April, May, June, July, August, September,
                             October, November, December, January, February, March,
                             IsActive, IsDeleted, CreatedBy, CreatedDate)
                            VALUES 
                            (@MapConcessionID, @FeeHeadsID, @ClassID, @SectionID, @FeeDiscountID, 
                             @SessionYear, @SessionID, @TenantID, @TenantCode, 
                             @Frequency, @Amount,
                             @April, @May, @June, @July, @August, @September,
                             @October, @November, @December, @January, @February, @March,
                             @IsActive, @IsDeleted, @CreatedBy, @CreatedDate)";

                            int rowsInserted = connection.Execute(insertQuery, model, transaction);

                            transaction.Commit();

                            return Json(new { success = true, message = "Fee Concession created successfully" });
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
        public JsonResult DeleteFeeConcession(Guid id)
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
                            // Soft delete Fee Concession
                            var deleteQuery = @" DELETE FROM  FeeMapConcession WHERE MapConcessionID = @MapConcessionID";
                            var parameters = new
                            {
                                MapConcessionID = id,
                            };

                            int rowsAffected = connection.Execute(deleteQuery, parameters, transaction);

                            transaction.Commit();

                            return Json(new
                            {
                                success = rowsAffected > 0,
                                message = rowsAffected > 0
                                    ? "Fee concession deleted successfully"
                                    : "Fee concession not found or already deleted"
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

        [HttpGet]
        public JsonResult GetExistingConcessions()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Query to get existing fee concessions
                    string query = @"
                        SELECT FeeHeadsID, ClassID, SectionID, FeeDiscountID
                        FROM FeeMapConcession
                        WHERE TenantID = @TenantID AND SessionID = @SessionID AND IsDeleted = 0";

                    var parameters = new { TenantID = CurrentTenantID, SessionID = CurrentSessionID };
                    var result = connection.Query(query, parameters).ToList();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}