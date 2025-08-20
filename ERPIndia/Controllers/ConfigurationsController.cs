using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class Configuration
    {
        public int Id { get; set; }
        public int ClientID { get; set; }
        public string KeyName { get; set; }
        public string KeyValue { get; set; }
        public string Module { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
    public class ConfigurationController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult IndexByKey()
        {
            return View();
        }
        public ActionResult ManageConfiguration(string key = null, string module = null)
        {
            ViewBag.KeyName = key;
            ViewBag.Module = module;

            return View();
        }
        // This method could get the current client ID from session, claims, or other authentication mechanism
        private int GetCurrentClientId()
        {
            // Replace with your actual implementation
            // For example, from claims or session
            // return Convert.ToInt32(User.Identity.GetClientId());

            // For testing, return a default value or from session if available
            return Session["UserId"] != null ? Convert.ToInt32(Session["UserId"]) : 1;
        }



        [HttpPost]

        public JsonResult GetConfigurationsByKey(string key, string module)
        {
            try
            {
                key = key.Replace(" ", "").ToUpper();
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                int clientId = GetCurrentClientId();

                // Validate required parameters
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(module))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Key and module parameters are required",
                        draw = "0",
                        recordsFiltered = 0,
                        recordsTotal = 0,
                        data = new List<Configuration>()
                    }, JsonRequestBehavior.AllowGet);
                }

                using (var connection = new SqlConnection(connectionString))
                {
                    var request = HttpContext.Request;
                    var draw = request.Form["draw"];
                    var start = Convert.ToInt32(request.Form["start"] ?? "0");
                    var length = Convert.ToInt32(request.Form["length"] ?? "0");
                    var sortColumnIndex = request.Form["order[0][column]"];
                    var sortColumn = request.Form[$"columns[{sortColumnIndex}][name]"];
                    var sortColumnDirection = request.Form["order[0][dir]"];
                    var searchValue = request.Form["search[value]"];

                    // Base query with ClientID, key, and module filters
                    var sql = new StringBuilder();
                    sql.Append("WITH ConfigData AS ( ");
                    sql.Append("SELECT *, ROW_NUMBER() OVER (");

                    // Handle sorting
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    {
                        sql.Append($"ORDER BY {sortColumn} {sortColumnDirection}");
                    }
                    else
                    {
                        // Default sorting by SortOrder
                        sql.Append("ORDER BY SortOrder");
                    }

                    sql.Append(") AS RowNum ");

                    // Apply exact match filters for KeyName and Module
                    sql.Append("FROM SchoolConfigurations WHERE ClientID = @ClientID ");
                    sql.Append("AND KeyName = @Key ");
                    sql.Append("AND Module = @Module ");

                    // Handle search (only searching in KeyValue)
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND KeyValue LIKE @Search ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM ConfigData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records that match key and module
                    var countSql = "SELECT COUNT(*) FROM SchoolConfigurations WHERE ClientID = @ClientID AND KeyName = @Key AND Module = @Module";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder("SELECT COUNT(*) FROM SchoolConfigurations WHERE ClientID = @ClientID AND KeyName = @Key AND Module = @Module");

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND KeyValue LIKE @Search");
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@ClientID", clientId);
                    parameters.Add("@Key", key);
                    parameters.Add("@Module", module);
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    connection.Open();

                    // Debug logging (can be removed in production)
                    System.Diagnostics.Debug.WriteLine($"Filtering by Key: {key}, Module: {module}, ClientID: {clientId}");
                    System.Diagnostics.Debug.WriteLine($"SQL Query: {sql}");

                    // Execute queries
                    var data = connection.Query<Configuration>(sql.ToString(), parameters).ToList();
                    var recordsTotal = connection.ExecuteScalar<int>(countSql, parameters);
                    var recordsFiltered = connection.ExecuteScalar<int>(countFilteredSql.ToString(), parameters);

                    var jsonData = new
                    {
                        draw = draw,
                        recordsFiltered = recordsFiltered,
                        recordsTotal = recordsTotal,
                        data = data
                    };

                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (implement logging as needed)
                System.Diagnostics.Debug.WriteLine($"Error in GetConfigurations: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    draw = "0",
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = new List<Configuration>()
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult GetConfigurations()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                int clientId = GetCurrentClientId();

                using (var connection = new SqlConnection(connectionString))
                {
                    var request = HttpContext.Request;

                    var draw = request.Form["draw"];
                    var start = Convert.ToInt32(request.Form["start"] ?? "0");
                    var length = Convert.ToInt32(request.Form["length"] ?? "0");
                    var sortColumnIndex = request.Form["order[0][column]"];
                    var sortColumn = request.Form[$"columns[{sortColumnIndex}][name]"];
                    var sortColumnDirection = request.Form["order[0][dir]"];
                    var searchValue = request.Form["search[value]"];

                    // Base query with ClientID filter
                    var sql = new StringBuilder();
                    sql.Append("WITH ConfigData AS ( ");
                    sql.Append("SELECT *, ROW_NUMBER() OVER (");

                    // Handle sorting
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    {
                        sql.Append($"ORDER BY {sortColumn} {sortColumnDirection}");
                    }
                    else
                    {
                        // Default sorting by Module and SortOrder
                        sql.Append("ORDER BY Module, SortOrder");
                    }

                    sql.Append(") AS RowNum ");
                    sql.Append("FROM SchoolConfigurations WHERE ClientID = @ClientID ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("AND (KeyName LIKE @Search ");
                        sql.Append("OR KeyValue LIKE @Search ");
                        sql.Append("OR Module LIKE @Search) ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM ConfigData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records for the client
                    var countSql = "SELECT COUNT(*) FROM SchoolConfigurations WHERE ClientID = @ClientID";

                    // Count filtered records
                    var countFilteredSql = new StringBuilder("SELECT COUNT(*) FROM SchoolConfigurations WHERE ClientID = @ClientID");

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" AND (KeyName LIKE @Search ");
                        countFilteredSql.Append("OR KeyValue LIKE @Search ");
                        countFilteredSql.Append("OR Module LIKE @Search)");
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@ClientID", clientId);
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    connection.Open();

                    // Execute queries
                    var data = connection.Query<Configuration>(sql.ToString(), parameters).ToList();
                    var recordsTotal = connection.ExecuteScalar<int>(countSql, new { ClientID = clientId });
                    var recordsFiltered = connection.ExecuteScalar<int>(countFilteredSql.ToString(), parameters);

                    var jsonData = new
                    {
                        draw = draw,
                        recordsFiltered = recordsFiltered,
                        recordsTotal = recordsTotal,
                        data = data
                    };

                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (implement logging as needed)
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private bool IsValueDuplicate(SqlConnection connection, Configuration config)
        {
            string sql = @"SELECT COUNT(*) FROM SchoolConfigurations 
                  WHERE ClientID = @ClientID 
                  AND KeyName = @KeyName 
                  AND Module = @Module 
                  AND KeyValue = @KeyValue
                  AND Id != @Id";

            var parameters = new
            {
                config.ClientID,
                config.KeyName,
                config.Module,
                config.KeyValue,
                Id = config.Id > 0 ? config.Id : 0 // For updates, exclude the current record
            };

            int count = connection.ExecuteScalar<int>(sql, parameters);
            return count > 0;
        }

        // Then modify these methods to include the validation:

        [HttpPost]
        public JsonResult InsertConfiguration(Configuration config)
        {
            try
            {
                // Set the ClientID from the current context
                config.ClientID = GetCurrentClientId();

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate values
                    if (IsValueDuplicate(connection, config))
                    {
                        return Json(new { success = false, message = "This " + config.KeyValue + " already exists for this " + config.KeyName + " and module. Please enter a unique value." });
                    }

                    string sql = @"INSERT INTO SchoolConfigurations (ClientID, KeyName, KeyValue, Module, SortOrder, CreatedDate, ModifiedDate) 
                       VALUES (@ClientID, @KeyName, @KeyValue, @Module, @SortOrder, GETDATE(), GETDATE())";
                    string keyName = config.KeyName.ToUpper();
                    keyName = keyName.Replace(" ", "");
                    string Module = config.Module.ToUpper();
                    var parameters = new
                    {
                        config.ClientID,
                        keyName,
                        config.KeyValue,
                        Module,
                        config.SortOrder
                    };

                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "inserted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Insertion failed." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateConfiguration(Configuration config)
        {
            try
            {
                // Set the ClientID from the current context
                config.ClientID = GetCurrentClientId();

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check for duplicate values (excluding the current record)
                    if (IsValueDuplicate(connection, config))
                    {
                        return Json(new { success = false, message = "This value already exists for this key and module. Please enter a unique value." });
                    }

                    // Include ClientID in WHERE clause to ensure users can only update their own configurations
                    string sql = @"UPDATE  SchoolConfigurations
                  SET KeyValue = @KeyValue, 
                      SortOrder = @SortOrder,
                      ModifiedDate = GETDATE()
                  WHERE Id = @Id AND ClientID = @ClientID";

                    var parameters = new
                    {
                        config.Id,
                        config.ClientID,
                        config.KeyValue,
                        config.SortOrder
                    };

                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = " updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Update failed. Configuration not found or not owned by your client." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetConfigurationById(int id)
        {
            try
            {
                int clientId = GetCurrentClientId();

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Include ClientID in WHERE clause for security
                    string sql = "SELECT * FROM SchoolConfigurations WHERE Id = @Id AND ClientID = @ClientID";
                    var parameters = new { Id = id, ClientID = clientId };

                    connection.Open();
                    Configuration config = connection.QueryFirstOrDefault<Configuration>(sql, parameters);

                    if (config != null)
                    {
                        return Json(config, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(null, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult DeleteConfiguration(int id)
        {
            try
            {
                int clientId = GetCurrentClientId();

                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Include ClientID in WHERE clause for security
                    string sql = "DELETE FROM SchoolConfigurations WHERE Id = @Id AND ClientID = @ClientID";
                    var parameters = new { Id = id, ClientID = clientId };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = " deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Delete failed. Configuration not found or not owned by your client." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to get all configuration values by key name and module for the current client
        [HttpGet]
        public JsonResult GetConfigurationValues(string keyName, string module)
        {
            try
            {
                int clientId = GetCurrentClientId();

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = "SELECT * FROM SchoolConfigurations WHERE KeyName = @KeyName AND Module = @Module AND ClientID = @ClientID ORDER BY SortOrder";
                    var parameters = new
                    {
                        KeyName = keyName,
                        Module = module,
                        ClientID = clientId
                    };

                    connection.Open();
                    var values = connection.Query<Configuration>(sql, parameters).ToList();

                    return Json(new { success = true, data = values }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper method to get a single configuration value by key name and module
        [HttpGet]
        public JsonResult GetSingleConfigurationValue(string keyName, string module)
        {
            try
            {
                int clientId = GetCurrentClientId();

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // This gets only the first value (lowest sort order) if there are multiple with the same key
                    string sql = "SELECT TOP 1 KeyValue FROM SchoolConfigurations WHERE KeyName = @KeyName AND Module = @Module AND ClientID = @ClientID ORDER BY SortOrder";
                    var parameters = new
                    {
                        KeyName = keyName,
                        Module = module,
                        ClientID = clientId
                    };

                    connection.Open();
                    string value = connection.QueryFirstOrDefault<string>(sql, parameters);

                    return Json(new { success = true, value = value }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper method to get all configurations for a specific module for the current client
        [HttpGet]
        public JsonResult GetConfigurationsByModule(string module)
        {
            try
            {
                int clientId = GetCurrentClientId();

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = "SELECT * FROM SchoolConfigurations WHERE Module = @Module AND ClientID = @ClientID ORDER BY KeyName, SortOrder";
                    var parameters = new { Module = module, ClientID = clientId };

                    connection.Open();
                    var configs = connection.Query<Configuration>(sql, parameters).ToList();

                    return Json(new { success = true, data = configs }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult GetNextSortOrder(string keyName, string module)
        {
            try
            {
                int clientId = GetCurrentClientId();

                // Normalize the key name and module to match the storage format
                keyName = keyName.Replace(" ", "").ToUpper();
                module = module.ToUpper();

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    // Get the maximum existing sort order for this client, module, and key name
                    string sql = @"SELECT ISNULL(MAX(SortOrder), 0) + 1 
                          FROM SchoolConfigurations 
                          WHERE ClientID = @ClientID 
                          AND KeyName = @KeyName 
                          AND Module = @Module";

                    var parameters = new
                    {
                        ClientID = clientId,
                        KeyName = keyName,
                        Module = module
                    };

                    connection.Open();
                    int nextSortOrder = connection.ExecuteScalar<int>(sql, parameters);

                    return Json(new { success = true, nextSortOrder = nextSortOrder }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        // Helper method to get all distinct key names for a specific module
        [HttpGet]
        public JsonResult GetDistinctKeysByModule(string module)
        {
            try
            {
                int clientId = GetCurrentClientId();

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = "SELECT DISTINCT KeyName FROM SchoolConfigurations WHERE Module = @Module AND ClientID = @ClientID ORDER BY KeyName";
                    var parameters = new { Module = module, ClientID = clientId };

                    connection.Open();
                    var keys = connection.Query<string>(sql, parameters).ToList();

                    return Json(new { success = true, data = keys }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Helper method to get all distinct modules
        [HttpGet]
        public JsonResult GetDistinctModules()
        {
            try
            {
                int clientId = GetCurrentClientId();

                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = "SELECT DISTINCT Module FROM SchoolConfigurations WHERE ClientID = @ClientID ORDER BY Module";
                    var parameters = new { ClientID = clientId };

                    connection.Open();
                    var modules = connection.Query<string>(sql, parameters).ToList();

                    return Json(new { success = true, data = modules }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
