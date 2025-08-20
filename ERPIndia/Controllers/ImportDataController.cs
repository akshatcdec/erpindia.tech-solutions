using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using ERPIndia.StudentManagement.Repository;
using ExcelDataReader;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;


namespace ERPIndia.Controllers
{

    public class ImportDataController : BaseController
    {
        private readonly StudentRepository _repository;
        public ImportDataController()
        {
            // GetCurrentClientId( = GetCurrentClientId();
            _repository = new StudentRepository();
        }
        // GET: ImportData
        bool ValidateSchoolIds(List<StudentData> studentList, List<FamilyData> familyList, List<OtherData> otherList, string expectedSchoolId)
        {
            return studentList.All(s => s.SchoolCode == expectedSchoolId) &&
                   familyList.All(f => f.SchoolCode == expectedSchoolId) &&
                   otherList.All(o => o.SchoolCode == expectedSchoolId);
        }

        public void SaveDistinctStudentValuesToConfigTable(List<StudentData> finalStudentList, int clientId)
        {
            if (finalStudentList == null || !finalStudentList.Any())
                return;

            // Define configuration categories to save
            var configData = new Dictionary<string, List<string>>
    {
        { "Classes", finalStudentList.Where(s => !string.IsNullOrEmpty(s.Class)).Select(s => s.Class).Distinct().ToList() },
        { "Sections", finalStudentList.Where(s => !string.IsNullOrEmpty(s.Section)).Select(s => s.Section).Distinct().ToList() },
        { "Houses", finalStudentList.Where(s => !string.IsNullOrEmpty(s.House)).Select(s => s.House).Distinct().ToList() },
        { "FeeCategories", finalStudentList.Where(s => !string.IsNullOrEmpty(s.FeeCategory)).Select(s => s.FeeCategory).Distinct().ToList() }
    };

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();


                try
                {
                    foreach (var category in configData.Keys)
                    {
                        var values = configData[category];
                        if (values == null || !values.Any())
                            continue;

                        // Set the module name - adjust as needed for your system
                        string module = "Student";

                        // Get existing values for this category and client
                        string selectSql = @"
                        SELECT KeyValue 
                        FROM dbo.Configurations 
                        WHERE ClientID = @ClientID 
                        AND KeyName = @KeyName
                        AND Module = @Module";

                        List<string> existingValues = new List<string>();
                        using (var command = new SqlCommand(selectSql, connection))
                        {
                            command.Parameters.AddWithValue("@ClientID", clientId);
                            command.Parameters.AddWithValue("@KeyName", category);
                            command.Parameters.AddWithValue("@Module", module);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    existingValues.Add(reader.GetString(0));
                                }
                            }
                        }

                        // Filter out values that already exist
                        var newValues = values
                            .Where(v => !existingValues.Contains(v, StringComparer.OrdinalIgnoreCase))
                            .ToList();

                        if (newValues.Any())
                        {
                            // Insert new values
                            string insertSql = @"
                            INSERT INTO dbo.Configurations 
                            (ClientID, KeyName, KeyValue, Module, SortOrder, CreatedDate, ModifiedDate) 
                            VALUES 
                            (@ClientID, @KeyName, @KeyValue, @Module, @SortOrder, GETDATE(), GETDATE())";

                            using (var command = new SqlCommand(insertSql, connection))
                            {
                                // Create parameters once
                                var clientIdParam = command.Parameters.Add("@ClientID", SqlDbType.Int);
                                var keyNameParam = command.Parameters.Add("@KeyName", SqlDbType.NVarChar, 100);
                                var keyValueParam = command.Parameters.Add("@KeyValue", SqlDbType.NVarChar, 500);
                                var moduleParam = command.Parameters.Add("@Module", SqlDbType.NVarChar, 50);
                                var sortOrderParam = command.Parameters.Add("@SortOrder", SqlDbType.Int);

                                // Set common parameter values
                                clientIdParam.Value = clientId;
                                keyNameParam.Value = category;
                                moduleParam.Value = module;

                                // Calculate starting sort order (max existing + 1)
                                int startingSortOrder = 0;
                                if (existingValues.Any())
                                {
                                    string maxSortSql = @"
                                    SELECT ISNULL(MAX(SortOrder), 0) 
                                    FROM dbo.Configurations 
                                    WHERE ClientID = @ClientID 
                                    AND KeyName = @KeyName
                                    AND Module = @Module";

                                    using (var maxCommand = new SqlCommand(maxSortSql, connection))
                                    {
                                        maxCommand.Parameters.AddWithValue("@ClientID", clientId);
                                        maxCommand.Parameters.AddWithValue("@KeyName", category);
                                        maxCommand.Parameters.AddWithValue("@Module", module);

                                        object result = maxCommand.ExecuteScalar();
                                        if (result != null && result != DBNull.Value)
                                        {
                                            startingSortOrder = Convert.ToInt32(result);
                                        }
                                    }
                                }

                                // For each new value
                                int sortOrder = startingSortOrder + 1;
                                foreach (var value in newValues)
                                {
                                    keyValueParam.Value = value;
                                    sortOrderParam.Value = sortOrder++;

                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                    }

                    //transaction.Commit();
                }
                catch (Exception ex)
                {
                    //transaction.Rollback();
                    // Log the exception
                    System.Diagnostics.Debug.WriteLine($"Error saving configuration values: {ex.Message}");
                    throw; // Re-throw to handle at calling method
                }

            }
        }

        /**
         * Gets configuration values from the Configurations table
         * @param clientId Client/School ID
         * @param keyName Configuration key name
         * @param module Module name
         * @return List of configuration values
         */
        public List<string> GetConfigurationValues(int clientId, string keyName, string module = "Student")
        {
            List<string> values = new List<string>();

            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            using (var connection = new SqlConnection(connectionString))
            {
                string sql = @"
            SELECT KeyValue 
            FROM dbo.Configurations 
            WHERE ClientID = @ClientID 
            AND KeyName = @KeyName 
            AND Module = @Module
            ORDER BY SortOrder";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@ClientID", clientId);
                    command.Parameters.AddWithValue("@KeyName", keyName);
                    command.Parameters.AddWithValue("@Module", module);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            values.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return values;
        }


        // Usage example
        public void ProcessStudentConfigData(List<StudentData> finalStudentList, int clientId)
        {
            try
            {
                // Save all distinct values to the Configurations table
                SaveDistinctStudentValuesToConfigTable(finalStudentList, clientId);

                // Example of retrieving values
                var classes = GetConfigurationValues(clientId, "Classes");
                var sections = GetConfigurationValues(clientId, "Sections");

                Console.WriteLine($"Successfully processed {classes.Count} classes and {sections.Count} sections.");
            }
            catch (Exception ex)
            {
                // Handle exception
                Console.WriteLine($"Error processing student configuration data: {ex.Message}");
            }
        }
        public ActionResult Index()
        {
            ImportDataModel user = null;//UserBLL.GetById(SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId)));

            string path = CommonLogic.GetConfigValue(StringConstants.AppConfig_ProfilePicFolderPath);
            string defPath = CommonLogic.GetConfigValue(StringConstants.AppConfig_DefaultProfilePic);
            string picFolder = AppLogic.GetProfilePicFolder();

            if (user == null)
            {
                user = new ImportDataModel();
            }

            if (!string.IsNullOrEmpty(user.ProfilePic))
            {
                string physicalPath = Path.Combine(Server.MapPath(path), picFolder, Convert.ToString("1"), user.ProfilePic);
                if (System.IO.File.Exists(physicalPath))
                {
                    user.ProfilePic = Path.Combine(path, picFolder, Convert.ToString("1"), user.ProfilePic);
                }
                else
                {
                    user.ProfilePic = defPath;
                }
            }
            else
            {
                user.ProfilePic = defPath;
            }


            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", user) : this.View(user);
        }
        private DataTable ConvertToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            foreach (T item in items)
            {
                DataRow row = dataTable.NewRow();
                foreach (PropertyInfo property in properties)
                {
                    row[property.Name] = property.GetValue(item) ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ImportDataModel model, HttpPostedFileBase ProfileImg)
        {
            try
            {
                if (ProfileImg == null)
                {
                    ModelState.AddModelError("Validation", "File  is required To Import.");
                    return View(model);
                }
                if (ModelState.IsValid)
                {

                    string path = Path.Combine(Server.MapPath(CommonLogic.GetConfigValue(StringConstants.AppConfig_ExcelFolderPath)));
                    if (ProfileImg != null && ProfileImg.ContentLength > 0)
                    {
                        try
                        {
                            int MaxContentLength = CommonLogic.GetConfigIntValue(StringConstants.AppConfig_ProfilePicMaxSize);
                            string[] AllowedFileExtensions = CommonLogic.GetConfigValue(StringConstants.AppConfig_ReportExcelAllowedFileType).Split(',');

                            string fileName = Path.GetFileNameWithoutExtension(ProfileImg.FileName);
                            string fileExtension = Path.GetExtension(ProfileImg.FileName);

                            if (!AllowedFileExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError("Validation", string.Format(StringConstants.ValidFileTypeMsg, string.Join(", ", AllowedFileExtensions)));
                                return this.View(model);
                            }
                            else if (ProfileImg.ContentLength > MaxContentLength)
                            {
                                ModelState.AddModelError("Validation", string.Format(StringConstants.ValidFileSizeMsg, MaxContentLength / 1024.0));
                                return this.View(model);
                            }
                            else
                            {
                                fileName = string.Concat(DateTime.Now.ToString("yyyyMMddHHmmss"), fileExtension);

                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }

                                Stream stream = ProfileImg.InputStream;
                                List<StudentData> _StudentList = new List<StudentData>();
                                List<OtherData> _OtherList = new List<OtherData>();
                                List<FamilyData> _Familyist = new List<FamilyData>();
                                StringBuilder stringBuilder = new StringBuilder();
                                IExcelDataReader reader = null;

                                List<StudentData> _FinalStudentList = new List<StudentData>();
                                List<FamilyData> _FinalFamilyist = new List<FamilyData>();
                                List<OtherData> _FinalOtherList = new List<OtherData>();
                                reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                                try
                                {
                                    stringBuilder.Append("<table  class=\"bs-table table-striped table-bordered text-nowrap\" width=\"100%\">");
                                    stringBuilder.Append("<tr><td colspan=\"2\" align=\"center\">Import Excel Summary</td></tr>");
                                    var dtss = reader.AsDataSet().Tables.Count;
                                    for (int i = 0; i < dtss; i++)
                                    {
                                        stringBuilder.Append("<tr>");
                                        stringBuilder.Append("<td><b>");
                                        var sheetName = reader.AsDataSet().Tables[i].TableName;
                                        stringBuilder.Append(sheetName);
                                        stringBuilder.Append("</b></td>");
                                        var dtTable = reader.AsDataSet().Tables[i];
                                        if (dtTable != null)
                                        {
                                            var jsonobject = Utils.GenerateJsonFromDataTable(dtTable);
                                            //we can chnage this function to get the directly from list  from the datatable
                                            switch (sheetName.ToLower())
                                            {
                                                case "studentbasic":
                                                    try
                                                    {

                                                        stringBuilder.Append("<td>");
                                                        List<StudentData> oMyclass = Utils.ConvertDataTableToListWithHeaderRow<StudentData>(dtTable);
                                                        stringBuilder.Append("<table width=\"100%\">");
                                                        for (int x = 0; x < oMyclass.Count; x++)
                                                        {
                                                            var studentData = oMyclass[x];
                                                            studentData.SetDefaultValue();
                                                            StudentValidator validator = new StudentValidator(studentData);
                                                            FluentValidation.Results.ValidationResult result = validator.Validate(studentData);
                                                            
                                                            int exist = 0;
                                                            if (result.IsValid == false)
                                                            {
                                                                studentData.IsValidData = "N";
                                                            }
                                                            if (result.IsValid)
                                                            {
                                                                exist = StudentUtilBLL.IsStudentBasicDetailsExists(studentData);
                                                                studentData.IsValidData = "Y";
                                                            }
                                                            if (exist == 0)
                                                            {
                                                                if (studentData.IsValidData == "Y")
                                                                {
                                                                    stringBuilder.Append("<tr><td style=\"padding: 1px;color:green\">" + string.Format("Row {0} : Valid Row ", x + 1) + "</td></tr>"); ;
                                                                    //stringBuilder.Append("<tr><td>&nbsp;</td></tr>");
                                                                }
                                                                else
                                                                {
                                                                    stringBuilder.Append("<tr><td style=\"padding: 1px\">" + string.Format("Row {0} :  {1}", x + 1, result.ToString()) + "</td></tr>");
                                                                    //stringBuilder.Append("<tr><td>&nbsp;</td></tr>");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                studentData.IsValidData = "N";
                                                                stringBuilder.Append($"<tr><td style=\"padding: 1px;color:gray\">Admission No ({studentData.AdmsnNo}) :: With School Code ({studentData.SchoolCode})  Already Exists</td></tr>");
                                                                // stringBuilder.Append("<tr><td>&nbsp;</td></tr>");
                                                            }
                                                            if (studentData.IsValidData == "Y")
                                                            {
                                                                _FinalStudentList.Add(studentData);
                                                            }
                                                            else
                                                            {
                                                                _StudentList.Add(studentData);
                                                            }
                                                        }
                                                        stringBuilder.Append("</table>");
                                                        stringBuilder.Append("</td>");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ModelState.AddModelError("Validation", "Invalid Data in studentbasic" + ex.Message);
                                                        return this.View(model);

                                                    }
                                                    break;
                                                case "studentfamily":

                                                    try
                                                    {

                                                        stringBuilder.Append("<td>");

                                                        List<FamilyData> oMyclass = Utils.ConvertDataTableToListWithHeaderRow<FamilyData>(dtTable);
                                                        stringBuilder.Append("<table width=\"100%\">");

                                                        for (int x = 0; x < oMyclass.Count; x++)
                                                        {
                                                            var _famailyData = oMyclass[x];
                                                            _famailyData.SetDefaultValue();
                                                            FamilyDataValidator validator = new FamilyDataValidator(_famailyData);
                                                            FluentValidation.Results.ValidationResult result = validator.Validate(_famailyData);
                                                            int exist = 0;
                                                            if (result.IsValid == false)
                                                            {
                                                                _famailyData.IsValidData = "N";
                                                            }
                                                            if (result.IsValid)
                                                            {
                                                                exist = StudentUtilBLL.IsStudentFamilyDetailsExists(_famailyData);
                                                                _famailyData.IsValidData = "Y";
                                                            }
                                                            if (exist == 0)
                                                            {
                                                                if (_famailyData.IsValidData == "Y")
                                                                {
                                                                    stringBuilder.Append("<tr><td style=\"padding: 1px;color:green\">" + string.Format("Row {0} : Valid Row ", x + 1) + "</td></tr>"); ;
                                                                    //stringBuilder.Append("<tr><td>&nbsp;</td></tr>");
                                                                }
                                                                else
                                                                {
                                                                    stringBuilder.Append("<tr><td style=\"padding: 1px\">" + string.Format("Row {0} :  {1}", x + 1, result.ToString()) + "</td></tr>");
                                                                    //stringBuilder.Append("<tr><td>&nbsp;</td></tr>");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                _famailyData.IsValidData = "N";
                                                                stringBuilder.Append($"<tr><td style=\"padding: 1px;color:gray\"> Admission No ({_famailyData.AdmsnNo}) :: With School Code ({_famailyData.SchoolCode})  Already Exists</td></tr>");
                                                                //stringBuilder.Append("<tr><td>&nbsp;</td></tr>");
                                                            }
                                                            if (_famailyData.IsValidData == "Y")
                                                            {
                                                                _FinalFamilyist.Add(_famailyData);
                                                            }
                                                            else
                                                            {
                                                                _Familyist.Add(_famailyData);
                                                            }
                                                        }
                                                        stringBuilder.Append("</table>");
                                                        //stringBuilder.Append(""+family+"Record Imported");
                                                        stringBuilder.Append("</td>");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ModelState.AddModelError("Validation", "Invalid Data in studentfamily" + ex.Message);
                                                        return this.View(model);
                                                    }
                                                    break;
                                                case "studentother":
                                                    try
                                                    {

                                                        stringBuilder.Append("<td>");
                                                        List<OtherData> oMyclass = Utils.ConvertDataTableToListWithHeaderRow<OtherData>(dtTable);
                                                        //List<OtherData> oMyclass = Newtonsoft.Json.JsonConvert.DeserializeObject<List<OtherData>>(jsonobject);
                                                        stringBuilder.Append("<table width=\"100%\">");

                                                        for (int x = 0; x < oMyclass.Count; x++)
                                                        {
                                                            var _otherData = oMyclass[x];
                                                            OtherDataValidator validator = new OtherDataValidator(_otherData);
                                                            FluentValidation.Results.ValidationResult result = validator.Validate(_otherData);
                                                            int exist = 0;
                                                            if (result.IsValid == false)
                                                            {
                                                                _otherData.IsValidData = "N";
                                                            }
                                                            if (result.IsValid)
                                                            {
                                                                exist = StudentUtilBLL.IsStudentOtherDetailsExists(_otherData);
                                                                _otherData.IsValidData = "Y";
                                                            }
                                                            if (exist == 0)
                                                            {
                                                                if (_otherData.IsValidData == "Y")
                                                                {
                                                                    stringBuilder.Append("<tr><td style=\"padding: 1px;;color:green\">" + string.Format("Row {0} : Valid Row ", x + 1) + "</td></tr>"); ;
                                                                    //stringBuilder.Append("<tr><td>&nbsp;</td></tr>");
                                                                }
                                                                else
                                                                {
                                                                    stringBuilder.Append("<tr><td style=\"padding: 1px\">" + string.Format("Row {0} :  {1}", x + 1, result.ToString()) + "</td></tr>");
                                                                    //stringBuilder.Append("<tr><td>&nbsp;</td></tr>");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                _otherData.IsValidData = "N";
                                                                stringBuilder.Append($"<tr><td style=\"padding: 1px;color:gray\"> Admission No ({_otherData.AdmsnNo}) :: With School Code ({_otherData.SchoolCode})  Already Exists</td></tr>");
                                                                // stringBuilder.Append("<tr><td>&nbsp;</td></tr>");
                                                            }
                                                            if (_otherData.IsValidData == "Y")
                                                            {
                                                                _FinalOtherList.Add(_otherData);
                                                            }
                                                            else
                                                            {
                                                                _OtherList.Add(_otherData);
                                                            }
                                                        }
                                                        stringBuilder.Append("</table>");
                                                        //stringBuilder.Append("" + other + "Record Imported");
                                                        stringBuilder.Append("</td>");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        ModelState.AddModelError("Validation", "Invalid Data in studentothers" + ex.Message);
                                                        return this.View(model);
                                                    }
                                                    break;

                                            }
                                        }
                                        stringBuilder.Append("</tr>");
                                    }
                                    stringBuilder.Append("</table>");
                                    bool containStudentBasic = _StudentList.Any(p => p.IsValidData == "N");
                                    bool containStudentFamily = _Familyist.Any(p => p.IsValidData == "N");
                                    bool containStudentOther = _OtherList.Any(p => p.IsValidData == "N");
                                    int RoleId = Utils.ParseInt(CommonLogic.GetSessionValue(StringConstants.RoleId));
                                    int SchoolId = Utils.ParseInt(CommonLogic.GetSessionValue(StringConstants.SchoolCode));
                                    string SessionId = CommonLogic.GetSessionValue(StringConstants.ActiveSessionID);
                                    string UserId = CommonLogic.GetSessionValue(StringConstants.TenantId);
                                    string currentStep = "Initializing";
                                    bool isValid = ValidateSchoolIds(_FinalStudentList, _FinalFamilyist, _FinalOtherList, SchoolId.ToString());
                                    if (!isValid)
                                    {
                                        ModelState.AddModelError("Validation", ($"Error: Lists contain differing school IDs. Expected SchoolId: {SchoolId}"));
                                        return this.View(model);
                                    }
                                    if (containStudentBasic == false && containStudentFamily == false && containStudentOther == false)
                                    {

                                        try
                                        {
                                            // Create a string to track where we are in the process

                                            for (int x = 0; x < _FinalStudentList.Count; x++)
                                            {
                                                currentStep = $"Saving Student Basic Information - Record {x + 1} of {_FinalStudentList.Count}";
                                                StudentUtilBLL.SaveStudentBasic(_FinalStudentList[x], RoleId, SchoolId);
                                            }

                                            // Save student family information
                                            currentStep = "Saving Student Family Information";
                                            for (int x = 0; x < _FinalFamilyist.Count; x++)
                                            {
                                                currentStep = $"Saving Student Family Information - Record {x + 1} of {_FinalFamilyist.Count}";
                                                StudentUtilBLL.SaveStudentFamily(_FinalFamilyist[x], RoleId, SchoolId);
                                            }

                                            // Save student other information
                                            currentStep = "Saving Student Other Information";
                                            for (int x = 0; x < _FinalOtherList.Count; x++)
                                            {
                                                currentStep = $"Saving Student Other Information - Record {x + 1} of {_FinalOtherList.Count}";
                                                StudentUtilBLL.SaveStudentOther(_FinalOtherList[x], RoleId, SchoolId);
                                            }
                                            await _repository.MapAndUpdateStudentReferences1(Guid.Parse(SessionId), Guid.Parse(UserId));
                                            //scope.Complete();
                                            ModelState.AddModelError("Validation", "All Excel Data Imported Successfully");

                                        }
                                        catch (Exception ex)
                                        {

                                            // Log detailed error information including which step failed
                                            string errorMessage = $"Error in step: {currentStep}. Error details: {ex.Message}";

                                            // You can also log the stack trace for more detailed debugging
                                            string stackTrace = ex.StackTrace;

                                            // Add the error to ModelState
                                            ModelState.AddModelError("Validation", errorMessage);

                                            // Optionally, write to a log file or system log
                                            System.Diagnostics.Debug.WriteLine(errorMessage);
                                            System.Diagnostics.Debug.WriteLine(stackTrace);
                                        }



                                        // Save student basic information

                                        /*         string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                                                 using (SqlConnection connection = new SqlConnection(connectionString))
                                                 {
                                                     connection.Open();

                                                     // Create transaction
                                                     using (SqlTransaction transaction = connection.BeginTransaction())
                                                     {
                                                         try
                                                         {
                                                             // Import Students
                                                             using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                                                             {
                                                                 bulkCopy.DestinationTableName = "StudentInfoBasic";
                                                                 bulkCopy.BatchSize = 1000;

                                                                 // Map columns

                                                                 // Convert list to DataTable
                                                                 DataTable studentsTable = ConvertToDataTable(_FinalStudentList);

                                                                 // Perform bulk insert
                                                                 bulkCopy.WriteToServer(studentsTable);
                                                             }

                                                             // Import Family data
                                                             using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                                                             {
                                                                 bulkCopy.DestinationTableName = "StudentInfoFamily";
                                                                 bulkCopy.BatchSize = 1000;

                                                                 // Convert list to DataTable
                                                                 DataTable familiesTable = ConvertToDataTable(_FinalFamilyist);

                                                                 // Perform bulk insert
                                                                 bulkCopy.WriteToServer(familiesTable);
                                                             }

                                                             // Import Other data
                                                             using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                                                             {
                                                                 bulkCopy.DestinationTableName = "StudentInfoOther";
                                                                 bulkCopy.BatchSize = 1000;

                                                                 // Map columns
                                                                 // Convert list to DataTable
                                                                 DataTable othersTable = ConvertToDataTable(_FinalOtherList);

                                                                 // Perform bulk insert
                                                                 bulkCopy.WriteToServer(othersTable);
                                                             }

                                                             // Commit transaction if all bulk copies succeed
                                                             transaction.Commit();

                                                             // Log success
                                                             // Logger.LogInfo("Bulk import completed successfully");
                                                         }
                                                         catch (Exception ex)
                                                         {
                                                             // Rollback transaction on error
                                                             transaction.Rollback();

                                                             // Log error
                                                             // Logger.LogError("Bulk import failed: " + ex.Message);

                                                             // Rethrow or handle as needed
                                                             throw new Exception("Failed to import student data", ex);
                                                         }
                                                     }

                                                 */
                                        //SaveDistinctStudentValuesToConfigTable(_FinalStudentList, 1);

                                        //}

                                        //}
                                        //catch (Exception ex)
                                        //{
                                        //    // This catches errors that might occur outside the transaction
                                        //    string errorMessage = $"Error initializing transaction: {ex.Message}";
                                        //    ModelState.AddModelError("Validation", errorMessage);
                                        //    System.Diagnostics.Debug.WriteLine(errorMessage);
                                        //    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                                        //}
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("Validation", "Record Exist or Inavlid Row Error");
                                    }
                                    ModelState.AddModelError("ProfilePic", stringBuilder.ToString());
                                    return View(model);
                                }
                                catch (Exception ex)
                                {
                                    ModelState.AddModelError("Validation", "Unable to Upload file!" + ex.Message);

                                }

                            }
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        model.ProfilePic = "";
                    }
                }
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMsg = CommonLogic.GetExceptionMessage(ex);
            }
            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", model) : this.View(model);
        }
    }
}