using ClosedXML.Excel;
using Dapper;
using ERPIndia.FeeDefaultersModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{

    public class FeeDefaultersController : BaseController
    {
        private readonly IFeeDefaultersRepository _feeDefaultersRepository;
        private readonly DropdownController _dropdownController;
        private List<SelectListItem> ConvertToSelectListDefault(JsonResult result)
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
                    // Create a new list with "ALL" as the first item
                    var newList = new List<DropdownItem>
            {
                new DropdownItem
                {
                    Id = Guid.Empty,
                    Name = "ALL"
                }
            };

                    // Add the existing items to the new list
                    newList.AddRange(dropdownResponse.Data);

                    // Convert to SelectListItems
                    return newList.Select(item => new SelectListItem
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
        private List<SelectListItem> ConvertToSelectList(JsonResult result)
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

        public FeeDefaultersController()
        {
            // Initialize repository with connection string
            _feeDefaultersRepository = new FeeDefaultersRepository(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
            _dropdownController = new DropdownController();
        }

        [HttpPost]
        public FileResult ExportFeeDefaulters(FeeDefaultersSearchRequest searchRequest)
        {
            try
            {
                // Ensure tenant and session IDs are set
                searchRequest.TenantId = CurrentTenantID;
                searchRequest.SessionId = CurrentSessionID;

                // Perform search with all results (remove pagination)
                searchRequest.Start = 0;
                searchRequest.Length = int.MaxValue;

                // Get full results
                var result = _feeDefaultersRepository.GetFeeDefaultersList(searchRequest);

                // Convert to list to enable indexing
                var defaultersList = result.Data.ToList();

                // Create Excel file
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Fee Defaulters");

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Class Name";
                    worksheet.Cell(1, 2).Value = "Section";
                    worksheet.Cell(1, 3).Value = "Roll No.";
                    worksheet.Cell(1, 4).Value = "Student Name";
                    worksheet.Cell(1, 5).Value = "Father Name";
                    worksheet.Cell(1, 6).Value = "Mobile";
                    worksheet.Cell(1, 7).Value = "Total Fees";
                    worksheet.Cell(1, 8).Value = "Paid Amount";
                    worksheet.Cell(1, 9).Value = "Due Amount";
                    worksheet.Cell(1, 10).Value = "Last Payment Date";

                    // Style headers
                    var headerRange = worksheet.Range(1, 1, 1, 11);
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                    // Populate data
                    for (int row = 2; row <= defaultersList.Count + 1; row++)
                    {
                        var defaulter = defaultersList[row - 2];

                        worksheet.Cell(row, 1).Value = defaulter.ClassName;
                        worksheet.Cell(row, 2).Value = defaulter.SectionName;
                        worksheet.Cell(row, 3).Value = defaulter.RollNo;
                        worksheet.Cell(row, 4).Value = defaulter.StudentName;
                        worksheet.Cell(row, 5).Value = defaulter.FatherName;
                        worksheet.Cell(row, 6).Value = defaulter.Mobile;
                        worksheet.Cell(row, 7).Value = defaulter.TotalFees;
                        worksheet.Cell(row, 8).Value = defaulter.PaidAmount;
                        worksheet.Cell(row, 9).Value = defaulter.DueAmount;
                        worksheet.Cell(row, 10).Value = defaulter.LastPaymentDate?.ToString("dd/MM/yyyy");
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Convert to byte array
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"Fee_Defaulters_Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Export Error: {ex.Message}");

                // Return an error file or throw
                return File(new byte[0], "application/octet-stream", "error.txt");
            }
        }
        [HttpPost]
        public ActionResult Search(FeeDefaultersSearchRequest searchRequest)
        {
            try
            {
                // Ensure tenant and session IDs are set
                searchRequest.TenantId = CurrentTenantID;
                searchRequest.SessionId = CurrentSessionID;

                // Perform search
                var result = _feeDefaultersRepository.GetFeeDefaultersList(searchRequest);

                // Return partial view with results
                return PartialView("Partial/_FeeDefaultersTable", result.Data);
            }
            catch (Exception ex)
            {
                // Log the exception
                //LogError(ex, "Search Action");
                return PartialView("Partial/_FeeDefaultersTable", new List<FeeDefaultersDto>());
            }
        }
        // GET: FeeDefaulters
        public ActionResult Index()
        {
            var classesResult = new DropdownController().GetClasses();
            ViewData["Classes"] = ConvertToSelectListDefault(classesResult);

            // Sections Dropdown
            var sectionsResult = new DropdownController().GetSections();
            ViewData["Sections"] = ConvertToSelectListDefault(sectionsResult);

            // Initialize search model with default values
            var searchModel = new FeeDefaultersSearchRequest
            {
                AsOfDate = DateTime.Now,
                MinimumDueAmount = 0,
                TenantId = CurrentTenantID,
                SessionId = CurrentSessionID
            };

            return View(searchModel);
        }

        [HttpPost]
        public JsonResult GetFeeDefaultersList(FeeDefaultersSearchRequest searchRequest)
        {
            try
            {
                // Validate input
                if (searchRequest == null)
                {
                    searchRequest = new FeeDefaultersSearchRequest();
                }

                // Perform search and get results
                var result = _feeDefaultersRepository.GetFeeDefaultersList(searchRequest);

                return Json(new
                {
                    draw = searchRequest.Draw,
                    recordsTotal = result.TotalCount,
                    recordsFiltered = result.FilteredCount,
                    data = result.Data
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                return Json(new
                {
                    error = "An error occurred while fetching fee defaulters.",
                    details = ex.Message
                });
            }
        }

        // Export methods can be added here if needed

    }

    // DTOs and Request Models

}
namespace ERPIndia.FeeDefaultersModels
{
    public class FeeDefaultersSearchRequest
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public Guid TenantId { get; set; }
        public Guid SessionId { get; set; }
        public Guid? ClassId { get; set; }
        public string SectionId { get; set; }
        public decimal MinimumDueAmount { get; set; }
        public DateTime? AsOfDate { get; set; }
    }

    public class FeeDefaultersResult
    {
        public int TotalCount { get; set; }
        public int FilteredCount { get; set; }
        public IEnumerable<FeeDefaultersDto> Data { get; set; }
    }

    public class FeeDefaultersDto
    {
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string RollNo { get; set; }
        public string SrNo { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string Mobile { get; set; }
        public string CurrentAddress { get; set; }
        public decimal DiscountAmount { get; set; }  // Add this property
        public decimal NetFees { get; set; }
        public decimal TotalFees { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal TransportFee { get; set; }
        
        public decimal DueAmount { get; set; }
        public DateTime? LastPaymentDate { get; set; }
    }

    public interface IFeeDefaultersRepository
    {
        FeeDefaultersResult GetFeeDefaultersList(FeeDefaultersSearchRequest searchRequest);
    }

    public class FeeDefaultersRepository : IFeeDefaultersRepository
    {
        private readonly string _connectionString;

        public FeeDefaultersRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public FeeDefaultersResult GetFeeDefaultersList(FeeDefaultersSearchRequest searchRequest)
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                dbConnection.Open();

                // Prepare dynamic parameters
                var dynamicParams = new DynamicParameters();
                dynamicParams.Add("@TenantID", searchRequest.TenantId);
                dynamicParams.Add("@SessionID", searchRequest.SessionId);
                // ClassID - Handle null, empty, or default GUID
                if (searchRequest.ClassId == null || searchRequest.ClassId == Guid.Empty)
                {
                    // Use null for Guid parameters
                    dynamicParams.Add("@ClassID", null);
                }
                else
                {
                    dynamicParams.Add("@ClassID", searchRequest.ClassId);
                }

                // SectionID - Handle null or empty
                if (string.IsNullOrWhiteSpace(searchRequest.SectionId))
                {
                    dynamicParams.Add("@SectionID", null);
                }
                else
                {
                    dynamicParams.Add("@SectionID", searchRequest.SectionId);
                }
                dynamicParams.Add("@MinimumDueAmount", searchRequest.MinimumDueAmount);
                DateTime asOfDate;
                if (string.IsNullOrWhiteSpace(searchRequest.AsOfDate.ToString()))
                {
                    // Default to current date if no date is provided
                    asOfDate = DateTime.Now;
                }
                else
                {
                    // Try parsing with multiple formats
                    if (!DateTime.TryParseExact(searchRequest.AsOfDate.ToString(),
                        new[]
                        {
                    "dd/MM/yyyy",    // Most common format
                    "d/M/yyyy",      // Single digit day/month
                    "dd-MM-yyyy",    // Alternative separator
                    "MM/dd/yyyy",    // US format
                    "yyyy-MM-dd"     // ISO format
                        },
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out asOfDate))
                    {
                        // If parsing fails, use current date
                        asOfDate = DateTime.Now;

                        // Optionally log the parsing failure
                        System.Diagnostics.Debug.WriteLine($"Failed to parse date: {searchRequest.AsOfDate}");
                    }
                }
                dynamicParams.Add("@AsOfDate", asOfDate);


                // Get total count (unfiltered)
                int totalCount = dbConnection.ExecuteScalar<int>(
                    @"SELECT COUNT(*) 
                      FROM StudentInfoBasic s
                      WHERE s.TenantID = @TenantID 
                      AND s.SessionID = @SessionID 
                      AND s.IsDeleted = 0 
                      AND s.IsActive = 1",
                    dynamicParams
                );

                // Execute stored procedure with pagination
                var result = dbConnection.Query<FeeDefaultersDto>(
                    "sp_GetFeeDefaultersList",
                    dynamicParams,
                    commandType: CommandType.StoredProcedure
                ).ToList();


                return new FeeDefaultersResult
                {
                    TotalCount = totalCount,
                    FilteredCount = result.Count,
                    Data = result
                };
            }
        }
    }
}