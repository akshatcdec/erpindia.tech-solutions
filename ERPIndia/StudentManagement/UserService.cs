using Dapper;
using ERPIndia.ForgetPassword.Models;
using ERPIndia.ForgetPassword.Services;
using ERPIndia.Models.SystemSettings;
using ERPIndia.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ERPIndia.ForgetPassword.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }
    }

    public class FindEmailBySchoolCodeViewModel
    {
        [Required(ErrorMessage = "School code is required")]
        [Display(Name = "School Code")]
        public string SchoolCode { get; set; }
    }

    public class ForgotPasswordResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Email { get; set; }
        public object Data { get; set; }
    }
}

// Models/User.cs
namespace ERPIndia.ForgetPassword.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string SchoolCode { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

// Services/IUserService.cs


namespace ERPIndia.ForgetPassword.Services
{
    public interface IUserService
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserBySchoolCodeAsync(string schoolCode);
        Task<bool> UpdatePasswordAsync(int userId, string newPassword);
        Task<bool> SendPasswordEmailAsync(string email, string password, string userName,string schoolcode);
    }
}

namespace ERPIndia.ForgetPassword.Services
{
    public class UserService : IUserService
    {
        private readonly string _connectionString;

        public UserService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string query = @"SELECT SystemUserId as UserId, UserName, Email, Password, SchoolCode, TenantName as Name  
                               FROM vwTenantDetails 
                               WHERE Email = @Email";

                return await db.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
            }
        }

        public async Task<User> GetUserBySchoolCodeAsync(string schoolCode)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string query = @"SELECT SystemUserId as UserId, UserName, Email, Password, SchoolCode, TenantName as Name  
                               FROM vwTenantDetails 
                               WHERE SchoolCode = @SchoolCode";

                return await db.QueryFirstOrDefaultAsync<User>(query, new { SchoolCode = schoolCode });
            }
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPassword)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE Users 
                               SET Password = @Password, 
                                   ModifiedDate = GETDATE() 
                               WHERE UserId = @UserId";

                var result = await db.ExecuteAsync(query, new { UserId = userId, Password = newPassword });
                return result > 0;
            }
        }

        public async Task<bool> SendPasswordEmailAsync(string email, string password, string userName,string schoolcode)
        {
            try
            {
                string subject = "Your Password - School Management System";

                string body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 10px;'>
                            <h2 style='color: #667eea;'>Password Recovery</h2>
                            <p>Dear {userName},</p>
                            <p>You requested your password for the School Management System.</p>
                            <div style='background-color: #fff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <p><strong>Your login credentials:</strong></p>
                                <p>User Name: <strong>{userName}</strong></p>
                                <p>Password: <strong>{password}</strong></p>
                                <p>Client Code: <strong>{schoolcode}</strong></p>
                                <p>Email: <strong>{email}</strong></p>
                            </div>
                            <p style='color: #dc3545;'><strong>Important:</strong> For security reasons, we recommend changing your password after logging in.</p>
                            <p>If you didn't request this, please contact your administrator immediately.</p>
                            <br>
                            <p>Best regards,<br>School Management System Team</p>
                        </div>
                    </body>
                    </html>";

                // Call your existing static Google email function
                // Assuming it's something like: GoogleEmailService.SendEmail(to, subject, body)
                // Replace with your actual static function
                return await Task.Run(() => GmailEmailUtility.SendPasswordEmail(email, subject, body));
            }
            catch
            {
                return false;
            }
        }
    }
}