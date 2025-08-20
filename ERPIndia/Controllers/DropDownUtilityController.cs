using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class DropdownItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public int Count { get; set; }
        public int SortOrder { get; set; }
    }
    public class DropdownStringItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public int SortOrder { get; set; }
    }
    /// <summary>
    /// Response model for dropdown data
    /// </summary>
    public class DropdownResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<DropdownItem> Data { get; set; }
        public int TotalCount { get; set; }
    }
    public class DropdownStringResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<DropdownStringItem> Data { get; set; }
        public int TotalCount { get; set; }
    }
    public class DropDownUtilityController : BaseController
    {
        // GET: DropDownUtility
        [HttpGet]
        public JsonResult GetFeeCategories(string sessionId, bool activeOnly = true)
        {
            try
            {
                Guid parsedSessionId;
                if (!Guid.TryParse(sessionId, out parsedSessionId))
                {
                    return Json(new DropdownResponse
                    {
                        Success = false,
                        Message = "Invalid session ID",
                        Data = new List<DropdownItem>()
                    }, JsonRequestBehavior.AllowGet);
                }

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"
                        SELECT 
                            fc.FeeCategoryID AS Id, 
                            fc.CategoryName AS Name,
                            fc.SortOrder,
                            (SELECT COUNT(*) FROM FeeStructure fs 
                             WHERE fs.FeeCategoryID = fc.FeeCategoryID 
                             AND fs.IsDeleted = 0) AS Count
                        FROM FeeCategoryMaster fc
                        WHERE fc.TenantID = @TenantID
                        AND fc.SessionID = @SessionID
                        AND fc.IsDeleted = 0
                        " + (activeOnly ? "AND fc.IsActive = 1" : "") + @"
                        ORDER BY fc.SortOrder, fc.CategoryName";

                    var parameters = new
                    {
                        TenantID = CurrentTenantID,
                        SessionID = parsedSessionId
                    };

                    connection.Open();
                    var categories = connection.Query<DropdownItem>(sql, parameters).ToList();

                    return Json(new DropdownResponse
                    {
                        Success = true,
                        Data = categories,
                        TotalCount = categories.Sum(c => c.Count)
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new DropdownResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Data = new List<DropdownItem>()
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}