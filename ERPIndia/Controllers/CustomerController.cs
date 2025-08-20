using Dapper;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class Customer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
    public class CustomerController : Controller
    {
        [HttpPost]
        public JsonResult GetCustomers()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

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

                    // Base query
                    var sql = new StringBuilder();
                    sql.Append("WITH CustomerData AS ( ");
                    sql.Append("SELECT *, ROW_NUMBER() OVER (");

                    // Handle sorting
                    if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                    {
                        sql.Append($"ORDER BY {sortColumn} {sortColumnDirection}");
                    }
                    else
                    {
                        sql.Append("ORDER BY Id"); // Default sorting
                    }

                    sql.Append(") AS RowNum ");
                    sql.Append("FROM Customers ");

                    // Handle search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        sql.Append("WHERE FirstName LIKE @Search ");
                        sql.Append("OR LastName LIKE @Search ");
                        sql.Append("OR Contact LIKE @Search ");
                        sql.Append("OR Email LIKE @Search ");
                    }

                    sql.Append(") ");
                    sql.Append("SELECT * FROM CustomerData ");
                    sql.Append("WHERE RowNum BETWEEN @Start + 1 AND @Start + @Length");

                    // Count total records
                    var countSql = "SELECT COUNT(*) FROM Customers";
                    var countFilteredSql = new StringBuilder("SELECT COUNT(*) FROM Customers");

                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        countFilteredSql.Append(" WHERE FirstName LIKE @Search ");
                        countFilteredSql.Append("OR LastName LIKE @Search ");
                        countFilteredSql.Append("OR Contact LIKE @Search ");
                        countFilteredSql.Append("OR Email LIKE @Search");
                    }

                    var parameters = new DynamicParameters();
                    parameters.Add("@Start", start);
                    parameters.Add("@Length", length);
                    parameters.Add("@Search", $"%{searchValue}%");

                    connection.Open();

                    // Execute queries
                    var data = connection.Query<Customer>(sql.ToString(), parameters).ToList();
                    var recordsTotal = connection.ExecuteScalar<int>(countSql);
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
        [HttpPost]
        public JsonResult InsertCustomer(Customer customer)
        {
            try
            {
                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"INSERT INTO Customers (FirstName, LastName, Contact, Email, DateOfBirth) 
                               VALUES (@FirstName, @LastName, @Contact, @Email, @DateOfBirth)";

                    var parameters = new
                    {
                        customer.FirstName,
                        customer.LastName,
                        customer.Contact,
                        customer.Email,
                        customer.DateOfBirth
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Customer inserted successfully!" });
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
        public JsonResult UpdateCustomer(Customer customer)
        {
            try
            {
                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = @"UPDATE Customers 
                          SET FirstName = @FirstName, 
                              LastName = @LastName, 
                              Contact = @Contact, 
                              Email = @Email, 
                              DateOfBirth = @DateOfBirth 
                          WHERE Id = @Id";

                    var parameters = new
                    {
                        customer.Id,
                        customer.FirstName,
                        customer.LastName,
                        customer.Contact,
                        customer.Email,
                        customer.DateOfBirth
                    };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Customer updated successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Update failed. Customer not found." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetCustomerById(int id)
        {
            try
            {
                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = "SELECT * FROM Customers WHERE Id = @Id";
                    var parameters = new { Id = id };

                    connection.Open();
                    Customer customer = connection.QueryFirstOrDefault<Customer>(sql, parameters);

                    if (customer != null)
                    {
                        return Json(customer, JsonRequestBehavior.AllowGet);
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
        public JsonResult DeleteCustomer(int id)
        {
            try
            {
                // Connection string from Web.config
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    string sql = "DELETE FROM Customers WHERE Id = @Id";
                    var parameters = new { Id = id };

                    connection.Open();
                    int rowsAffected = connection.Execute(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true, message = "Customer deleted successfully!" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Delete failed. Customer not found." });
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
