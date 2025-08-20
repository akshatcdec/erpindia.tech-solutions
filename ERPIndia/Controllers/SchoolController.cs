using Dapper;
using ERPIndia.Class.Helper;
using ERPIndia.Tenant;
using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
   
    public class SchoolController : BaseController
    {
        private readonly string _connectionString;
        private readonly TenantUserRepository _repository;

        public SchoolController()
        {
            // Initialize connection string
            _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            // Create repository with connection string
            _repository = new TenantUserRepository(_connectionString);
        }

        // GET: School/Create
        public ActionResult Create()
        {
            // Return the create view with a new model
            return View(new TenantUserModel());
        }

        // POST: School/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(TenantUserModel model)
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                // If not valid, return the view with the model to show validation errors
                return View(model);
            }

            try
            {
                // Attempt to create tenant user
                var tenantUserId = await _repository.CreateTenantUserAsync(model);

                // If successful, redirect to details or index
                TempData["SuccessMessage"] = "Tenant user created successfully.";
                return RedirectToAction("Details", new { id = tenantUserId });
            }
            catch (Exception ex)
            {
                // Log the exception (you should implement proper logging)
                // System.Diagnostics.Debug.WriteLine(ex);

                // Add a generic error message
                ModelState.AddModelError(string.Empty, "An error occurred while creating the tenant user.");

                // Return to the view with the model
                return View(model);
            }
        }

        // GET: School/Details/{id}
        public ActionResult Details(Guid id)
        {
            // TODO: Implement method to fetch and display tenant user details
            // This is a placeholder implementation
            return View();
        }
    }
}
namespace ERPIndia.Tenant
{
    public class TenantUserModel
    {
        // Tenant Information
        [Display(Name = "Tenant ID")]
        public Guid TenantID { get; set; } = Guid.NewGuid();

        [Required]
        [Display(Name = "Tenant Name")]
        [StringLength(500)]
        public string TenantName { get; set; }

        [Required]
        [Display(Name = "Tenant Code")]
        public int TenantCode { get; set; }

        [Display(Name = "Address Line 1")]
        [StringLength(250)]
        public string TenantAddress1 { get; set; }

        [Display(Name = "City")]
        [StringLength(50)]
        public string TenantCity { get; set; }

        [Display(Name = "State")]
        [StringLength(50)]
        public string TenantState { get; set; }

        [Display(Name = "Zip Code")]
        [StringLength(20)]
        public string TenantZipCode { get; set; }

        [Display(Name = "Tenant Email")]
        [StringLength(255)]
        [EmailAddress]
        public string TenantEmail { get; set; }

        [Display(Name = "Tenant Phone")]
        [StringLength(20)]
        [Phone]
        public string TenantPhone { get; set; }

        // System User Information
        [Required]
        [Display(Name = "Username")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(50)]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "User Email")]
        [StringLength(100)]
        [EmailAddress]
        public string UserEmail { get; set; }

        [Required]
        [Display(Name = "User Phone")]
        [StringLength(20)]
        [Phone]
        public string UserPhone { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [StringLength(50, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // Tenant User Relationship
        [Required]
        [Display(Name = "Role")]
        public long RoleId { get; set; }

        // Additional Properties
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        // Computed Property
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Trim();
    }
    public class TenantUserRepository
    {
        private readonly string _connectionString;

        public TenantUserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Guid> CreateTenantUserAsync(TenantUserModel model)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Insert Tenant
                        string tenantSql = @"
                        INSERT INTO Tenants 
                        (TenantName, TenantCode, Address1, City, State, ZipCode, 
                         Email, Phone, IsActive, IsDeleted, CreatedDate)
                        VALUES 
                        (@TenantName, @TenantCode, @TenantAddress1, @TenantCity, 
                         @TenantState, @TenantZipCode, @TenantEmail, @TenantPhone, 
                         @IsActive, 0, GETDATE());
                        SELECT CAST(SCOPE_IDENTITY() AS UNIQUEIDENTIFIER);";

                        var tenantId = await connection.ExecuteScalarAsync<Guid>(
                            tenantSql,
                            new
                            {
                                model.TenantName,
                                model.TenantCode,
                                model.TenantAddress1,
                                model.TenantCity,
                                model.TenantState,
                                model.TenantZipCode,
                                model.TenantEmail,
                                model.TenantPhone,
                                model.IsActive
                            },
                            transaction);

                        // Insert System User
                        string userSql = @"
                        INSERT INTO SystemUsers 
                        (Username, FirstName, MiddleName, LastName, 
                         Email, Phone, Password, AccessLevel, IsActive, CreatedDate)
                        VALUES 
                        (@Username, @FirstName, @MiddleName, @LastName, 
                         @UserEmail, @UserPhone, @Password, 1, @IsActive, GETDATE());
                        SELECT SCOPE_IDENTITY();";

                        var userId = await connection.ExecuteScalarAsync<long>(
                            userSql,
                            new
                            {
                                model.Username,
                                model.FirstName,
                                model.MiddleName,
                                model.LastName,
                                model.UserEmail,
                                model.UserPhone,
                                Password = HashPassword(model.Password), // Implement password hashing
                                model.IsActive
                            },
                            transaction);

                        // Insert Tenant User
                        string tenantUserSql = @"
                        INSERT INTO TenantUsers 
                        (UserId, TenantId, RoleId)
                        VALUES 
                        (@UserId, @TenantId, @RoleId);
                        SELECT TenantUserId;";

                        var tenantUserId = await connection.ExecuteScalarAsync<Guid>(
                            tenantUserSql,
                            new
                            {
                                UserId = userId,
                                TenantId = tenantId,
                                model.RoleId
                            },
                            transaction);

                        // Commit transaction
                        transaction.Commit();

                        return tenantUserId;
                    }
                    catch
                    {
                        // Rollback transaction if anything fails
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Basic password hashing method (replace with a more secure implementation)
        private string HashPassword(string password)
        {
            // TODO: Implement proper password hashing (e.g., using BCrypt or PBKDF2)
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}