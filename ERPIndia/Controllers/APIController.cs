using Dapper;
using ERPIndia.DTOs.Ledger;
using ERPIndia.Fee;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class StudentDto
    {
        public Guid StudentId { get; set; }
        public int SerialNumber { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public FeeDetailsDto FeeDetails { get; set; }
        public string srNo { get; set; }
        public string name { get; set; }
        public string @class { get; set; }
        public string fatherName { get; set; }
        public string motherName { get; set; }
        public string mobNo { get; set; }
        public string dob { get; set; }
        public string admNo { get; set; }
        public string type { get; set; }
        public string feeType { get; set; }
    }
    public class APIController : Controller
    {

        private string ProcessPhotoPath(string photoPath)
        {
            if (string.IsNullOrEmpty(photoPath))
                return null;

            // If it's already a full URL, return it as is
            if (photoPath.StartsWith("http://") || photoPath.StartsWith("https://"))
                return photoPath;

            // If it's just a filename, prepend the virtual path to your photos folder
            string virtualPath = "~/Content/StudentPhotos/";

            // Check if the file exists
            string physicalPath = Server.MapPath(virtualPath + photoPath);
            if (System.IO.File.Exists(physicalPath))
                return VirtualPathUtility.ToAbsolute(virtualPath + photoPath);

            // If file doesn't exist, return null to use default photo
            return null;
        }
        [HttpGet]
        public ActionResult SearchStudents(string query, int maxResults = 5)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            try
            {
                if (string.IsNullOrEmpty(query) || query.Length < 2)
                {
                    return Json(new List<object>(), JsonRequestBehavior.AllowGet);
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // SQL query using Dapper to get top N matches from StudentBasic table
                    var sql = @"
                SELECT TOP (@MaxResults)
                    s.AdmsnNo,
                    s.SchoolCode,
                    s.SrNo,
                    s.RollNo,
                    s.FirstName,
                    s.LastName,
                    CONCAT(s.FirstName, ' ', s.LastName) AS Name,
                    s.Class,
                    s.Section,
                    CONCAT(s.Class, '-', s.Section) AS ClassSection,
                    s.Gender,
                    s.Mobile AS MobNo,
                    s.DiscountCategory,
                    s.Photo AS PhotoUrl,
                    s.Status,
                    s.FeeCategory AS FeeType
                FROM 
                    dbo.StudentBasic s
                WHERE 
                    s.FirstName LIKE @SearchTerm OR
                    s.LastName LIKE @SearchTerm OR
                    s.SrNo LIKE @SearchTerm OR
                    s.StudentNo LIKE @SearchTerm OR
                    s.RollNo LIKE @SearchTerm OR
                    s.Mobile LIKE @SearchTerm OR
                    s.Class LIKE @SearchTerm OR
                    CONCAT(s.FirstName, ' ', s.LastName) LIKE @SearchTerm OR
                    CONCAT(s.Class, '-', s.Section) LIKE @SearchTerm
                ORDER BY 
                    CASE 
                        WHEN s.FirstName LIKE @ExactSearchTerm THEN 1
                        WHEN s.LastName LIKE @ExactSearchTerm THEN 2
                        WHEN s.SrNo LIKE @ExactSearchTerm THEN 3
                        WHEN s.StudentNo LIKE @ExactSearchTerm THEN 4
                        WHEN s.RollNo LIKE @ExactSearchTerm THEN 5
                        ELSE 6
                    END";

                    var parameters = new
                    {
                        SearchTerm = "%" + query + "%",
                        ExactSearchTerm = query + "%",
                        MaxResults = maxResults
                    };

                    var students = connection.Query<dynamic>(sql, parameters)
                        .Select(s => new
                        {
                            admsnNo = s.AdmsnNo.ToString(),
                            schoolCode = s.SchoolCode.ToString(),
                            srNo = s.SrNo,
                            rollNo = s.RollNo,
                            name = s.Name,
                            @class = s.Class,
                            section = s.Section,
                            fatherName = "Not Available", // Since you don't have father info in this table
                            mobNo = s.MobNo,
                            gender = s.Gender,
                            discountCategory = s.DiscountCategory,
                            status = s.Status,
                            feeType = s.FeeType,
                            Photo = ProcessPhotoPath(s.Photo)
                        })
                .ToList();

                    // Important: Return directly as Json instead of serializing manually
                    return Json(students, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // Log exception
                return Json(new { error = "An error occurred while searching for students." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Students()
        {
            FeeCollectRepository obj = new FeeCollectRepository();
            return Json(obj.GetAllStudents().ToList(), JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult feestudent()
        {
            FeeCollectRepository obj = new FeeCollectRepository();
            var data = obj.GetStudentFeeData(1);

            // Return your data but forced through the lower-case serializer
            return new LowerCaseJsonResult
            {
                Data = data,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };

        }

    }
    public class LowerCaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();

        }
    }
    public class LowerCaseJsonResult : JsonResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            // Set the response content type
            context.HttpContext.Response.ContentType = string.IsNullOrEmpty(ContentType)
                ? "application/json"
                : ContentType;

            // If there is data, serialize it using our custom resolver
            if (Data != null)
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new LowerCaseContractResolver()
                };

                string json = JsonConvert.SerializeObject(Data, settings);
                context.HttpContext.Response.Write(json);
            }
        }
    }
}