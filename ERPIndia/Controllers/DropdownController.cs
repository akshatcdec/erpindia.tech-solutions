using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class DropdownController : BaseController
    {
        private readonly string _connectionString;
        public List<SelectListItem> ConvertToSelectList(JsonResult result)
        {
            try
            {
                // Serialize the Data to a string to handle dynamic object
                string jsonString = JsonConvert.SerializeObject(result.Data);

                // Deserialize into a strongly-typed object
                var dropdownResponse = JsonConvert.DeserializeObject<DropdownResponse>(jsonString);

                // Check if deserialization was successful and data exists
                if (dropdownResponse?.Success == true && dropdownResponse.Data != null)
                {
                    return dropdownResponse.Data.Select(item => new SelectListItem
                    {
                        Value = item.Id.ToString(),
                        Text = item.Name
                    }).ToList();
                }

                // Return empty list if no data or unsuccessful
                return new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                return new List<SelectListItem>();
                // Log the error
                // LogError(ex, "ConvertToSelectList");
            }
        }
        public DropdownController()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        }

        [HttpGet]
        public JsonResult GetPickupPointsByRoute(Guid routeId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                SELECT 
                    TransportPickupsId AS Id, 
                    PickupName AS Name
                FROM 
                    TransportPickups
                WHERE 
                    RouteId = @RouteId
                    AND TenantID = @TenantID 
                    AND SessionID = @SessionID 
                    AND IsDeleted = 0 
                    AND IsActive = 1 
                ORDER BY 
                    SortOrder, PickupName";

                    var pickupPoints = connection.Query<DropdownItem>(query, new
                    {
                        RouteId = routeId,
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = pickupPoints }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetUpdateColumn()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @" SELECT KeyValue AS [Key],KeyName AS [Value] FROM dbo.SchoolConfigurations WHERE Module = 'UPDATECOLUMN' ORDER BY SortOrder";
                    var columnlists = connection.Query<DropdownItem>(query).ToList();

                    return Json(new { success = true, data = columnlists }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetSubjects()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                         SELECT SubjectID AS Id, SubjectName AS Name 
                         FROM AcademicSubjectMaster 
                         WHERE TenantID = @TenantID 
                         AND SessionID = @SessionID 
                         AND IsDeleted = 0 
                         AND IsActive = 1 
                         ORDER BY SortOrder, SubjectName";

                    var subjects = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = subjects }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetRoutes()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                                 SELECT TransportRouteId AS Id, Name 
                                 FROM TransportRoute 
                                 WHERE TenantID = @TenantID 
                                 AND SessionID = @SessionID 
                                 AND IsDeleted = 0 
                                 AND IsActive = 1 
                                 ORDER BY SortOrder, Name";

                        var routes = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = routes }, JsonRequestBehavior.AllowGet);
           }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetTown()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                                 SELECT VillageID AS Id,VillageName as  Name 
                                 FROM AcademicVillageMaster 
                                 WHERE TenantID = @TenantID 
                                 AND SessionID = @SessionID 
                                 AND IsDeleted = 0 
                                 AND IsActive = 1 
                                 ORDER BY SortOrder, VillageName";

                    var routes = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = routes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetHostel()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                                 SELECT HostelID AS Id,HostelName as  Name 
                                 FROM AcademicHostelMaster 
                                 WHERE TenantID = @TenantID 
                                 AND SessionID = @SessionID 
                                 AND IsDeleted = 0 
                                 AND IsActive = 1 
                                 ORDER BY SortOrder, HostelName";

                    var routes = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = routes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetGender()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"SELECT  KeyValue As Name , KeyValue As Id FROM dbo.SchoolConfigurations WHERE KeyName = 'GENDER' AND Module = 'ACADEMICS' AND ClientID=0 ORDER BY SortOrder, KeyValue";

                    var result = connection.Query<DropdownItemValue>(query).ToList();

                    return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetBloodGroup()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"SELECT  KeyValue As Name , KeyValue As Id FROM dbo.SchoolConfigurations WHERE KeyName = 'BLOODGROUP' AND ClientID=0 ORDER BY SortOrder, KeyValue";

                    var result = connection.Query<DropdownItemValue>(query).ToList();

                    return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetReligion()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"SELECT  KeyValue As Name , KeyValue As Id FROM dbo.SchoolConfigurations WHERE KeyName = 'RELIGION' AND ClientID=0 ORDER BY SortOrder, KeyValue";

                    var result = connection.Query<DropdownItemValue>(query).ToList();

                    return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetCategory()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"SELECT  KeyValue As Name , KeyValue As Id  FROM dbo.SchoolConfigurations WHERE KeyName = 'CATGORY' AND ClientID=0 ORDER BY SortOrder, KeyValue";

                    var result = connection.Query<DropdownItemValue>(query).ToList();

                    return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetMotherTounge()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"SELECT  KeyValue As Name , KeyValue As Id FROM dbo.SchoolConfigurations WHERE KeyName = 'MOTHERTONGE' AND ClientID=0 ORDER BY SortOrder, KeyValue";

                    var result = connection.Query<DropdownItemValue>(query).ToList();

                    return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetVehicle()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                                 SELECT TransportVehiclesId AS Id,VehicleNo as  Name 
                                 FROM TransportVehicles 
                                 WHERE TenantID = @TenantID 
                                 AND SessionID = @SessionID 
                                 AND IsDeleted = 0 
                                 AND IsActive = 1 
                                 ORDER BY SortOrder, Name";

                    var routes = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = routes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetFeeHeads()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        FeeHeadsID AS Id, 
                        HeadsName AS Name 
                    FROM FeeHeadsMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND IsDeleted = 0 
                        AND IsActive = 1
                    ORDER BY SortOrder,HeadsName";

                    var feeHeads = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = feeHeads }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpGet]
        public JsonResult GetFeeDiscountHeads()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        FeeDiscountID AS Id, 
                        DiscountName AS Name 
                    FROM FeeDiscountMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND IsDeleted = 0 
                        AND IsActive = 1
                    ORDER BY SortOrder,DiscountName";

                    var feeHeads = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = feeHeads }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public List<DropdownItem> GetSubjectsByClassSection(Guid classId, Guid sectionId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
                SELECT s.SubjectID AS Id, s.SubjectName AS Name
                FROM AcademicSubjectMaster s
                INNER JOIN AcademicSubjectMapping m ON s.SubjectID = m.SubjectID
                WHERE
                    m.ClassID = @ClassID
                    AND m.SectionID = @SectionID
                    AND m.SessionID = @SessionID
                    AND m.TenantID = @TenantID
                    AND s.IsActive = 1
                    AND s.IsDeleted = 0
                    AND m.IsActive = 1
                    AND m.IsDeleted = 0
                ORDER BY s.SortOrder, s.SubjectName";

                    var subjects = connection.Query<DropdownItem>(query, new
                    {
                        ClassID = classId,
                        SectionID = sectionId,
                        SessionID = CurrentSessionID,
                        TenantID = CurrentTenantID
                    }).ToList();

                    return subjects;
                }
            }
            catch (Exception ex)
            {
                // Optionally log the error
                return new List<DropdownItem>();
            }
        }

        [HttpGet]
        public JsonResult GetClasses()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        ClassID AS Id, 
                        ClassName AS Name 
                    FROM AcademicClassMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND IsDeleted = 0 
                        AND IsActive = 1
                    ORDER BY SortOrder,ClassName";

                    var classes = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = classes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetExamSchedule()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        ExamID AS Id, 
                        CASE 
                        WHEN AdmitCard IS NULL OR AdmitCard = '' THEN ExamName
                        ELSE AdmitCard
                        END AS Name  
                    FROM ExamMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND AC = 1
                    ORDER BY SerialNumber,ExamName";

                    var classes = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = classes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetExamMarks()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        ExamID AS Id, 
                        ExamName AS Name  
                    FROM ExamMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND Num = 1
                    ORDER BY SerialNumber,ExamName";

                    var classes = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = classes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetSubjectGrades()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        SubjectGradeID AS Id, 
                        SubjectGradeName AS Name  
                    FROM AcademicSubjectGradeMaster
                    WHERE TenantID = @TenantID 
                         AND SessionID = @SessionID 
                         AND IsDeleted = 0 
                         AND IsActive = 1 
                         ORDER BY SortOrder, SubjectGradeName";

                    var classes = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = classes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetExams()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        ExamID AS Id, 
                        ExamName AS Name 
                    FROM ExamMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                    ORDER BY SerialNumber,ExamName";

                    var classes = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = classes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetHouses()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        HouseID AS Id, 
                        HouseName AS Name 
                    FROM AcademicHouseMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND IsDeleted = 0 
                        AND IsActive = 1
                    ORDER BY SortOrder";

                    var classes = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = classes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetClassesByTenant(string TenantID,string SessionID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        ClassID AS Id, 
                        ClassName AS Name 
                    FROM AcademicClassMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND IsDeleted = 0 
                        AND IsActive = 1
                    ORDER BY SortOrder,ClassName";

                    var classes = connection.Query<DropdownItem>(query, new
                    {
                        TenantID,
                        SessionID
                    }).ToList();

                    return Json(new { success = true, data = classes }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetSections()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        SectionID AS Id, 
                        SectionName AS Name 
                    FROM AcademicSectionMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND IsDeleted = 0 
                        AND IsActive = 1
                    ORDER BY SortOrder,SectionName";

                    var sections = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = sections }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetExamTypes()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        SectionID AS Id, 
                        SectionName AS Name 
                    FROM AcademicSectionMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND IsDeleted = 0 
                        AND IsActive = 1
                    ORDER BY SortOrder,SectionName";

                    var sections = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = sections }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetBatches()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var query = @"
            SELECT 
                BatchID AS Id, 
                BatchName AS Name 
            FROM AcademicBatchMaster
            WHERE 
                TenantID = @TenantID 
                AND SessionID = @SessionID 
                AND IsDeleted = 0 
                AND IsActive = 1
            ORDER BY SortOrder, BatchName";

                    var batches = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = batches }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetSectionByTenant(string TenantID, string SessionID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        SectionID AS Id, 
                        SectionName AS Name 
                    FROM AcademicSectionMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND IsDeleted = 0 
                        AND IsActive = 1
                    ORDER BY SortOrder,SectionName";

                    var sections = connection.Query<DropdownItem>(query, new
                    {
                        TenantID,
                        SessionID
                    }).ToList();

                    return Json(new { success = true, data = sections }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetFeeCategories()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        FeeCategoryID AS Id, 
                        CategoryName AS Name 
                    FROM FeeCategoryMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND IsDeleted = 0 
                        AND IsActive = 1
                    ORDER BY SortOrder,CategoryName";

                    var categories = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = CurrentSessionID
                    }).ToList();

                    return Json(new { success = true, data = categories }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetFeeByTenant(string TenantID, string SessionID)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        FeeCategoryID AS Id, 
                        CategoryName AS Name 
                    FROM FeeCategoryMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND SessionID = @SessionID 
                        AND IsDeleted = 0 
                        AND IsActive = 1
                    ORDER BY SortOrder,CategoryName";

                    var categories = connection.Query<DropdownItem>(query, new
                    {
                        TenantID ,
                        SessionID
                    }).ToList();

                    return Json(new { success = true, data = categories }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetAcademicSessions()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    var query = @"
                    SELECT 
                        SessionID AS Id, 
                        SessionName AS Name 
                    FROM AcademicSessionMaster
                    WHERE 
                        TenantID = @TenantID 
                        AND IsDeleted = 0 
                        AND IsActive = 1
                    ORDER BY SortOrder,SessionName";

                    var sessions = connection.Query<DropdownItem>(query, new
                    {
                        TenantID = CurrentTenantID
                    }).ToList();

                    return Json(new { success = true, data = sessions }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public class DropdownItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
        }
        public class DropdownItemValue
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}