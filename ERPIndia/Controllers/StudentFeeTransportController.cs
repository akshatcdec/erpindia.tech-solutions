using Dapper;
using ERPIndia.Repositories;
using ERPIndia.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class TransportFeeMonthModel
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid SessionID { get; set; }
        public int SessionYear { get; set; }
        public Guid TenantID { get; set; }
        public int TenantCode { get; set; }
        public int SchoolCode { get; set; }
        public bool AprilFee { get; set; }
        public bool MayFee { get; set; }
        public bool JuneFee { get; set; }
        public bool JulyFee { get; set; }
        public bool AugustFee { get; set; }
        public bool SeptemberFee { get; set; }
        public bool OctoberFee { get; set; }
        public bool NovemberFee { get; set; }
        public bool DecemberFee { get; set; }
        public bool JanuaryFee { get; set; }
        public bool FebruaryFee { get; set; }
        public bool MarchFee { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class StudentFeeTransportController : BaseController
    {
        private readonly string _connectionString;
        private readonly IFeeManagementRepository _feeRepository;

        public StudentFeeTransportController()
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
            // Check if we need to initialize configuration
            EnsureTransportFeeConfiguration();
            return View();
        }

        // Initialize configuration if it doesn't exist
        private void EnsureTransportFeeConfiguration()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Check if configuration exists
                    var query = @"
                        SELECT COUNT(*) 
                        FROM TenantTransportFeeConfig 
                        WHERE SessionID = @SessionID 
                          AND SessionYear = @SessionYear 
                          AND TenantID = @TenantID 
                          AND TenantCode = @TenantCode 
                          AND IsDeleted = 0";

                    var parameters = new
                    {
                        SessionID = CurrentSessionID,
                        SessionYear = CurrentSessionYear,
                        TenantID = CurrentTenantID,
                        TenantCode = CurrentSchoolCode
                    };

                    var count = connection.ExecuteScalar<int>(query, parameters);

                    // If no configuration exists, create a default one
                    if (count == 0)
                    {
                        var insertQuery = @"
                            INSERT INTO TenantTransportFeeConfig 
                            (Id, SessionID, SessionYear, TenantID, TenantCode,
                             AprilFee, MayFee, JuneFee, JulyFee, AugustFee, SeptemberFee,
                             OctoberFee, NovemberFee, DecemberFee, JanuaryFee, FebruaryFee, MarchFee,
                             CreatedBy, CreatedDate, IsActive, IsDeleted,DefaultFeeAmount)
                            VALUES 
                            (@Id, @SessionID, @SessionYear, @TenantID, @TenantCode,
                             1, 1, 1, 1, 1, 1,
                             1, 1, 1, 1, 1, 1,
                             @CreatedBy, @CreatedDate, 1, 0,0)";

                        var insertParams = new
                        {
                            Id = Guid.NewGuid(),
                            SessionID = CurrentSessionID,
                            SessionYear = CurrentSessionYear,
                            TenantID = CurrentTenantID,
                            TenantCode = Utils.ParseInt(CurrentTenantCode),
                            SchoolCode = CurrentSchoolCode,
                            CreatedBy = CurrentTenantUserID,
                            CreatedDate = DateTime.Now
                        };

                        connection.Execute(insertQuery, insertParams);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Error initializing transport fee configuration: {ex.Message}");
            }
        }

        [HttpGet]
        public JsonResult GetTransportFeeConfig()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                        SELECT * 
                        FROM TenantTransportFeeConfig 
                        WHERE SessionID = @SessionID 
                          AND SessionYear = @SessionYear 
                          AND TenantID = @TenantID 
                          AND TenantCode = @TenantCode 
                          AND IsActive = 1 
                          AND IsDeleted = 0";

                    var parameters = new
                    {
                        SessionID = CurrentSessionID,
                        SessionYear = CurrentSessionYear,
                        TenantID = CurrentTenantID,
                        TenantCode = CurrentSchoolCode
                    };

                    var config = connection.QueryFirstOrDefault<TransportFeeMonthModel>(query, parameters);

                    if (config == null)
                    {
                        // If no configuration found, ensure it exists and try again
                        EnsureTransportFeeConfiguration();
                        config = connection.QueryFirstOrDefault<TransportFeeMonthModel>(query, parameters);
                    }

                    return Json(new { success = true, data = config }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult SaveTransportFeeConfig(TransportFeeMonthModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Invalid model" });
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Check if configuration exists
                            var checkQuery = @"
                                SELECT Id FROM TenantTransportFeeConfig 
                                WHERE SessionID = @SessionID 
                                  AND SessionYear = @SessionYear 
                                  AND TenantID = @TenantID 
                                  AND TenantCode = @SchoolCode 
                                  AND IsDeleted = 0";

                            var parameters = new
                            {
                                SessionID = CurrentSessionID,
                                SessionYear = CurrentSessionYear,
                                TenantID = CurrentTenantID,
                                SchoolCode = CurrentSchoolCode
                            };

                            var existingId = connection.QueryFirstOrDefault<Guid?>(checkQuery, parameters, transaction);

                            if (existingId.HasValue)
                            {
                                // Update existing configuration
                                var updateQuery = @"
                                    UPDATE TenantTransportFeeConfig 
                                    SET AprilFee = @AprilFee,
                                        MayFee = @MayFee,
                                        JuneFee = @JuneFee,
                                        JulyFee = @JulyFee,
                                        AugustFee = @AugustFee,
                                        SeptemberFee = @SeptemberFee,
                                        OctoberFee = @OctoberFee,
                                        NovemberFee = @NovemberFee,
                                        DecemberFee = @DecemberFee,
                                        JanuaryFee = @JanuaryFee,
                                        FebruaryFee = @FebruaryFee,
                                        MarchFee = @MarchFee,
                                        ModifiedBy = @ModifiedBy,
                                        ModifiedDate = @ModifiedDate
                                    WHERE Id = @Id";

                                var updateParams = new
                                {
                                    Id = existingId.Value,
                                    AprilFee = model.AprilFee,
                                    MayFee = model.MayFee,
                                    JuneFee = model.JuneFee,
                                    JulyFee = model.JulyFee,
                                    AugustFee = model.AugustFee,
                                    SeptemberFee = model.SeptemberFee,
                                    OctoberFee = model.OctoberFee,
                                    NovemberFee = model.NovemberFee,
                                    DecemberFee = model.DecemberFee,
                                    JanuaryFee = model.JanuaryFee,
                                    FebruaryFee = model.FebruaryFee,
                                    MarchFee = model.MarchFee,
                                    ModifiedBy = CurrentTenantUserID,
                                    ModifiedDate = DateTime.Now
                                };

                                connection.Execute(updateQuery, updateParams, transaction);
                            }
                            else
                            {
                                // Insert new configuration
                                var insertQuery = @"
                                    INSERT INTO TenantTransportFeeConfig 
                                    (Id, SessionID, SessionYear, TenantID, TenantCode, SchoolCode,
                                     AprilFee, MayFee, JuneFee, JulyFee, AugustFee, SeptemberFee,
                                     OctoberFee, NovemberFee, DecemberFee, JanuaryFee, FebruaryFee, MarchFee,
                                     CreatedBy, CreatedDate, IsActive, IsDeleted)
                                    VALUES 
                                    (@Id, @SessionID, @SessionYear, @TenantID, @TenantCode, @SchoolCode,
                                     @AprilFee, @MayFee, @JuneFee, @JulyFee, @AugustFee, @SeptemberFee,
                                     @OctoberFee, @NovemberFee, @DecemberFee, @JanuaryFee, @FebruaryFee, @MarchFee,
                                     @CreatedBy, @CreatedDate, 1, 0)";

                                var insertParams = new
                                {
                                    Id = Guid.NewGuid(),
                                    SessionID = CurrentSessionID,
                                    SessionYear = CurrentSessionYear,
                                    TenantID = CurrentTenantID,
                                    TenantCode = Utils.ParseInt(CurrentTenantCode),
                                    SchoolCode = CurrentSchoolCode,
                                    AprilFee = model.AprilFee,
                                    MayFee = model.MayFee,
                                    JuneFee = model.JuneFee,
                                    JulyFee = model.JulyFee,
                                    AugustFee = model.AugustFee,
                                    SeptemberFee = model.SeptemberFee,
                                    OctoberFee = model.OctoberFee,
                                    NovemberFee = model.NovemberFee,
                                    DecemberFee = model.DecemberFee,
                                    JanuaryFee = model.JanuaryFee,
                                    FebruaryFee = model.FebruaryFee,
                                    MarchFee = model.MarchFee,
                                    CreatedBy = CurrentTenantUserID,
                                    CreatedDate = DateTime.Now
                                };

                                connection.Execute(insertQuery, insertParams, transaction);
                            }

                            transaction.Commit();
                            return Json(new { success = true, message = "Transport fee configuration saved successfully" });
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
        public JsonResult ApplyFeesToStudents()
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
                            // Get the current transport fee configuration
                            var configQuery = @"
                                SELECT * 
                                FROM TenantTransportFeeConfig 
                                WHERE SessionID = @SessionID 
                                  AND SessionYear = @SessionYear 
                                  AND TenantID = @TenantID 
                                  AND TenantCode = @SchoolCode 
                                  AND IsActive = 1 
                                  AND IsDeleted = 0";

                            var parameters = new
                            {
                                SessionID = CurrentSessionID,
                                SessionYear = CurrentSessionYear,
                                TenantID = CurrentTenantID,
                                SchoolCode = CurrentSchoolCode
                            };

                            var config = connection.QueryFirstOrDefault<TransportFeeMonthModel>(configQuery, parameters, transaction);

                            if (config == null)
                            {
                                return Json(new { success = false, message = "Transport fee configuration not found" });
                            }

                            // Get all students with transport allocation
                            var studentsQuery = @"
                                SELECT DISTINCT s.StudentId 
                                FROM Students s
                                INNER JOIN TransportAllocation ta ON s.StudentId = ta.StudentId
                                WHERE s.SessionID = @SessionID 
                                  AND s.SessionYear = @SessionYear 
                                  AND s.TenantID = @TenantID 
                                  AND s.SchoolCode = @SchoolCode 
                                  AND s.IsActive = 1 
                                  AND s.IsDeleted = 0
                                  AND ta.IsActive = 1
                                  AND ta.IsDeleted = 0";

                            var students = connection.Query<Guid>(studentsQuery, parameters, transaction).ToList();

                            int updatedCount = 0;
                            int createdCount = 0;

                            foreach (var studentId in students)
                            {
                                // Check if student transport fee record exists
                                var checkQuery = @"
                                    SELECT Id FROM StudentFeeTransports
                                    WHERE StudentId = @StudentId
                                      AND SessionID = @SessionID 
                                      AND SessionYear = @SessionYear 
                                      AND TenantID = @TenantID 
                                      AND TenantCode = @SchoolCode 
                                      AND IsDeleted = 0";

                                var checkParams = new
                                {
                                    StudentId = studentId,
                                    SessionID = CurrentSessionID,
                                    SessionYear = CurrentSessionYear,
                                    TenantID = CurrentTenantID,
                                    SchoolCode = CurrentSchoolCode
                                };

                                var existingId = connection.QueryFirstOrDefault<Guid?>(checkQuery, checkParams, transaction);

                                if (existingId.HasValue)
                                {
                                    // Update existing transport fee record
                                    var updateQuery = @"
                                        UPDATE StudentFeeTransports 
                                        SET April = CASE WHEN @AprilFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            May = CASE WHEN @MayFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            June = CASE WHEN @JuneFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            July = CASE WHEN @JulyFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            August = CASE WHEN @AugustFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            September = CASE WHEN @SeptemberFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            October = CASE WHEN @OctoberFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            November = CASE WHEN @NovemberFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            December = CASE WHEN @DecemberFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            January = CASE WHEN @JanuaryFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            February = CASE WHEN @FebruaryFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            March = CASE WHEN @MarchFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                            ModifiedBy = @ModifiedBy,
                                            ModifiedDate = @ModifiedDate
                                        WHERE Id = @Id";

                                    var updateParams = new
                                    {
                                        Id = existingId.Value,
                                        StudentId = studentId,
                                        AprilFee = config.AprilFee,
                                        MayFee = config.MayFee,
                                        JuneFee = config.JuneFee,
                                        JulyFee = config.JulyFee,
                                        AugustFee = config.AugustFee,
                                        SeptemberFee = config.SeptemberFee,
                                        OctoberFee = config.OctoberFee,
                                        NovemberFee = config.NovemberFee,
                                        DecemberFee = config.DecemberFee,
                                        JanuaryFee = config.JanuaryFee,
                                        FebruaryFee = config.FebruaryFee,
                                        MarchFee = config.MarchFee,
                                        ModifiedBy = CurrentTenantUserID,
                                        ModifiedDate = DateTime.Now
                                    };

                                    connection.Execute(updateQuery, updateParams, transaction);
                                    updatedCount++;
                                }
                                else
                                {
                                    // Insert new transport fee record
                                    var insertQuery = @"
                                        INSERT INTO StudentFeeTransports
                                        (Id, StudentId, SessionID, SessionYear, TenantID, TenantCode, SchoolCode,
                                         April, May, June, July, August, September,
                                         October, November, December, January, February, March,
                                         CreatedBy, CreatedDate, IsActive, IsDeleted)
                                        VALUES
                                        (@Id, @StudentId, @SessionID, @SessionYear, @TenantID, @TenantCode, @SchoolCode,
                                         CASE WHEN @AprilFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         CASE WHEN @MayFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         CASE WHEN @JuneFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         CASE WHEN @JulyFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         CASE WHEN @AugustFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         CASE WHEN @SeptemberFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         CASE WHEN @OctoberFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         CASE WHEN @NovemberFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         CASE WHEN @DecemberFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         CASE WHEN @JanuaryFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         CASE WHEN @FebruaryFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         CASE WHEN @MarchFee = 1 THEN (SELECT Amount FROM TransportFeeSetup WHERE StudentId = @StudentId) ELSE 0 END,
                                         @CreatedBy, @CreatedDate, 1, 0)";

                                    var insertParams = new
                                    {
                                        Id = Guid.NewGuid(),
                                        StudentId = studentId,
                                        SessionID = CurrentSessionID,
                                        SessionYear = CurrentSessionYear,
                                        TenantID = CurrentTenantID,
                                        TenantCode = Utils.ParseInt(CurrentTenantCode),
                                        SchoolCode = CurrentSchoolCode,
                                        AprilFee = config.AprilFee,
                                        MayFee = config.MayFee,
                                        JuneFee = config.JuneFee,
                                        JulyFee = config.JulyFee,
                                        AugustFee = config.AugustFee,
                                        SeptemberFee = config.SeptemberFee,
                                        OctoberFee = config.OctoberFee,
                                        NovemberFee = config.NovemberFee,
                                        DecemberFee = config.DecemberFee,
                                        JanuaryFee = config.JanuaryFee,
                                        FebruaryFee = config.FebruaryFee,
                                        MarchFee = config.MarchFee,
                                        CreatedBy = CurrentTenantUserID,
                                        CreatedDate = DateTime.Now
                                    };

                                    connection.Execute(insertQuery, insertParams, transaction);
                                    createdCount++;
                                }
                            }

                            transaction.Commit();

                            return Json(new
                            {
                                success = true,
                                message = $"Transport fees applied successfully. Updated: {updatedCount}, Created: {createdCount} student records."
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.Message);
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