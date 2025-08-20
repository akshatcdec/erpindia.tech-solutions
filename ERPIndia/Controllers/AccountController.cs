using Dapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.ForgetPassword.Services;
using ERPIndia.ForgetPassword.ViewModels;
using ERPIndia.Models;
using ERPIndia.Models.SystemSettings;
using ERPIndia.Models.TokenAuth;
using ERPIndia.Utilities;
using Microsoft.Reporting.WebForms;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ERPIndia.Controllers
{
    //https://www.billingsoftware.in/gst-invoice.html
    /// <summary>
    /// Account controller class.
    /// 
    /// </summary>
    /// *
    public class AccountController : BaseController
    {
        private readonly IUserService _userService;
        public AccountController()
        {
            // Initialize with your connection string
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            _userService = new UserService(connectionString);
        }
        #region Action Methods
        public ActionResult Email()
        {
            string subject = "Welcome to our platform!";
            string body = @"
                <h2>Welcome!</h2>
                <p>Thank you for joining our platform.</p>
                <p>Best regards,<br/>Your Team</p>";

            bool success = GmailEmailUtility.SendWelcomeEmail("mr.akshatkumar@gmail.com","erakshat87");

            if (success)
            {
                ViewBag.Message = "Email sent successfully!";
            }
            else
            {
                ViewBag.Message = "Failed to send email.";
            }

            return View();
        }
        /// <summary>
        /// Login view action.
        /// </summary>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>
        /// Returns login action result.
        /// </returns>
        public ActionResult Login(string returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            LoginModel model = new LoginModel();

            model.UserName = CommonLogic.GetCookieValue(StringConstants.RememberUserName);
            model.Password = CommonLogic.GetCookieValue(StringConstants.RememberPassword);

            if (!string.IsNullOrEmpty(model.UserName) && !string.IsNullOrEmpty(model.Password))
            {
                model.UserName = CommonLogic.Decrypt(model.UserName);
                model.Password = CommonLogic.Decrypt(model.Password);
                model.RememberMe = true;
            }

            return this.View(model);
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var response = new ForgotPasswordResponseModel();

            try
            {
                if (!ModelState.IsValid)
                {
                    response.Success = false;
                    response.Message = "Please enter a valid email address.";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                // Check if user exists
                var user = await _userService.GetUserByEmailAsync(model.Email);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "Email address not found in our records. Please check and try again.";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                // Send password email
                bool emailSent = await _userService.SendPasswordEmailAsync(user.Email, user.Password, user.UserName, user.SchoolCode);

                if (emailSent)
                {
                    response.Success = true;
                    response.Message = $"Password has been sent to {user.Email}. Please check your inbox and spam folder.";

                    // Log the password recovery attempt
                    //await LogPasswordRecoveryAttempt(user.UserId, "Email", true);
                }
                else
                {
                    response.Success = false;
                    response.Message = "Failed to send email. Please try again later or contact support.";
                }
            }
            catch (Exception ex)
            {
                // Log the error
                LogError(ex);

                response.Success = false;
                response.Message = "An error occurred. Please try again later.";
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // POST: Account/FindEmailBySchoolCode
        [HttpPost]
        public async Task<JsonResult> FindEmailBySchoolCode(FindEmailBySchoolCodeViewModel model)
        {
            var response = new ForgotPasswordResponseModel();

            try
            {
                if (!ModelState.IsValid)
                {
                    response.Success = false;
                    response.Message = "Please enter your Client code.";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                // Convert to uppercase for consistency
                string schoolCode = model.SchoolCode.ToUpper();

                // Find user by school code
                var user = await _userService.GetUserBySchoolCodeAsync(schoolCode);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "Client code not found in our records. Please check and try again.";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                response.Success = true;
                response.Message = "Email address found successfully!";
                response.Email = user.Email;
                response.Data = new { Name = user.Name };

                // Log the attempt
                //await LogPasswordRecoveryAttempt(user.UserId, "SchoolCode", true);
            }
            catch (Exception ex)
            {
                // Log the error
                LogError(ex);

                response.Success = false;
                response.Message = "An error occurred. Please try again later.";
            }

            return Json(response, JsonRequestBehavior.AllowGet);
        }

        // Helper method to log password recovery attempts
        private async Task LogPasswordRecoveryAttempt(int userId, string method, bool success)
        {
            try
            {
                using (var db = new System.Data.SqlClient.SqlConnection(
                    System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    string query = @"INSERT INTO PasswordRecoveryLogs 
                                   (UserId, RecoveryMethod, Success, AttemptDate, IPAddress) 
                                   VALUES (@UserId, @Method, @Success, GETDATE(), @IPAddress)";

                    await db.ExecuteAsync(query, new
                    {
                        UserId = userId,
                        Method = method,
                        Success = success,
                        IPAddress = Request.UserHostAddress
                    });
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw - this is not critical
                LogError(ex);
            }
        }

        // Helper method to log errors
        private void LogError(Exception ex)
        {
            // Implement your error logging here
            // Example: Logger.Error(ex.Message, ex);
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
        /// <summary>
        /// Login view post action.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>
        /// Returns login action result.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            // Check if user already has valid auth token cookie
            HttpCookie authCookie = Request.Cookies["MV5AuthToken"];
            if (authCookie != null)
            {
                string token = authCookie.Value;
                AuthTokenModel tokenModel = AuthTokenBLL.GetTokenByValue(token);

                if (tokenModel != null && tokenModel.IsActive && DateTime.Now < tokenModel.ExpiryDate)
                {
                    // User has valid token, get user info and restore session
                    SystemUsersModel user = UserBLL.GetSystemUserById(tokenModel.UserId);
                    if (user != null)
                    {
                        // Set auth cookie
                        FormsAuthentication.SetAuthCookie(user.Email, true);

                        // Restore session data
                        RestoreUserSession(tokenModel.UserId);

                        // Redirect to dashboard
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return this.Redirect(returnUrl);
                        }
                        else
                        {
                            return this.RedirectToAction<AdminDashboardController>(dc => dc.Index());
                        }
                    }
                }
            }

            // If we get here, no valid token or process normal login
            if (ModelState.IsValid)
            {
                SystemUsersModel loggedUser = UserBLL.ValidateLoginWithSchoolCode(model.UserName.Trim(), model.Password.Trim(), model.Code.Trim());

                if (loggedUser != null)
                {
                    if (loggedUser.SystemUserId > 0)
                    {
                        // Store essential user information in session
                        CommonLogic.SetSessionValue(StringConstants.UserId, loggedUser.SystemUserId);
                        CommonLogic.SetSessionValue(StringConstants.UserName, loggedUser.Email);
                        CommonLogic.SetSessionValue(StringConstants.FullName, loggedUser.FirstName);
                        CommonLogic.SetSessionValue(StringConstants.RoleId, loggedUser.SystemRoleId);
                        CommonLogic.SetSessionValue(StringConstants.RoleName, loggedUser.SystemRoleName);
                        CommonLogic.SetSessionValue(StringConstants.ProfilePic, loggedUser.ProfilePic);

                        // Get and store system settings
                        SystemSettingModel settingModel = SystemSettingBLL.GetSetting();
                        CommonLogic.SetSessionValue(StringConstants.FinancialYear, settingModel.FYearId);

                        // Store tenant information
                        CommonLogic.SetSessionValue(StringConstants.TenantID, loggedUser.TenantID);
                        CommonLogic.SetSessionValue(StringConstants.TenantName, loggedUser.TenantName);
                        CommonLogic.SetSessionValue(StringConstants.TenantUserId, loggedUser.TenantUserId);
                        CommonLogic.SetSessionValue(StringConstants.ActiveSessionID, loggedUser.ActiveSessionID);
                        CommonLogic.SetSessionValue(StringConstants.ActiveSessionYear, loggedUser.ActiveSessionYear);
                        CommonLogic.SetSessionValue(StringConstants.TenantCode, model.Code);

                        // Store school information if available
                        if (loggedUser.SchoolId > 0)
                        {
                            SchoolModel schoolModel = SchoolBLL.GetById(Convert.ToInt64(loggedUser.SchoolCode));
                            if (schoolModel != null)
                            {
                                CommonLogic.SetSessionValue(StringConstants.SchoolName, schoolModel.SchoolName);
                                CommonLogic.SetSessionValue(StringConstants.ActiveSessionPrint, schoolModel.ActiveSessionPrint);
                                CommonLogic.SetSessionValue(StringConstants.SchoolCode, schoolModel.SchoolCode);
                                CommonLogic.SetSessionValue(StringConstants.PrintTitle, schoolModel.PrintTitle);
                                CommonLogic.SetSessionValue(StringConstants.Line1, schoolModel.Line1);
                                CommonLogic.SetSessionValue(StringConstants.Line2, schoolModel.Line2);
                                CommonLogic.SetSessionValue(StringConstants.Line3, schoolModel.Line3);
                                CommonLogic.SetSessionValue(StringConstants.Line4, schoolModel.Line4);
                                CommonLogic.SetSessionValue(StringConstants.ActiveHeaderImg, schoolModel.ActiveHeaderImg);
                                CommonLogic.SetSessionValue(StringConstants.LogoImg, schoolModel.LogoImg);
                                CommonLogic.SetSessionValue(StringConstants.ReceiptBannerImg, schoolModel.ReceiptBannerImg);
                                CommonLogic.SetSessionValue(StringConstants.AdmitCardBannerImg, schoolModel.AdmitCardBannerImg);
                                CommonLogic.SetSessionValue(StringConstants.ReportCardBannerImg, schoolModel.ReportCardBannerImg);
                                CommonLogic.SetSessionValue(StringConstants.TransferCertBannerImg, schoolModel.TransferCertBannerImg);
                                CommonLogic.SetSessionValue(StringConstants.SalarySlipBannerImg, schoolModel.SalarySlipBannerImg);
                                CommonLogic.SetSessionValue(StringConstants.ICardNameBannerImg, schoolModel.ICardNameBannerImg);
                                CommonLogic.SetSessionValue(StringConstants.ICardAddressBannerImg, schoolModel.ICardAddressBannerImg);
                                CommonLogic.SetSessionValue(StringConstants.PrincipalSignImg, schoolModel.PrincipalSignImg);
                                CommonLogic.SetSessionValue(StringConstants.ReceiptSignImg, schoolModel.ReceiptSignImg);
                                CommonLogic.SetSessionValue(StringConstants.IsSingleFee, schoolModel.IsSingleFee);
                                CommonLogic.SetSessionValue(StringConstants.EnableOnlineFee, schoolModel.EnableOnlineFee);
                                CommonLogic.SetSessionValue(StringConstants.TopBarName, schoolModel.TopBarName);
                                CommonLogic.SetSessionValue(StringConstants.TopBarAddress, schoolModel.TopBarAddress);

                            }
                        }

                        // Log login history
                        SystemLoginHistoryModel loginHistory = new SystemLoginHistoryModel();
                        loginHistory.UserId = loggedUser.SystemUserId;
                        loginHistory.Action = SystemLoginHistoryAction.LogIn;
                        loginHistory.IPAddress = CommonLogic.GetClientIPAddress();
                        LoginHistoryBLL.SaveLoginHistory(loginHistory);

                        // Only set permanent login if "Remember Me" is checked
                        FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);

                        // Only create persistent token if "Remember Me" is checked
                        if (model.RememberMe)
                        {
                            // Generate a secure token for additional validation
                            string authToken = GenerateSecureToken(loggedUser.SystemUserId);

                            // Store the token in a cookie (30 days expiration)
                            HttpCookie newAuthCookie = new HttpCookie("MV5AuthToken");
                            newAuthCookie.Value = authToken;
                            newAuthCookie.Expires = DateTime.Now.AddDays(30);
                            // Make sure cookie is accessible from root path
                            newAuthCookie.Path = "/";
                            Response.Cookies.Add(newAuthCookie);

                            // Save the token in the database
                            AuthTokenBLL.SaveToken(loggedUser.SystemUserId, authToken, DateTime.Now.AddDays(30));

                            // Store login credentials in cookie
                            CommonLogic.SetCookieValue(StringConstants.RememberUserName, CommonLogic.Encrypt(model.UserName), DateTime.Now.AddDays(30));
                            CommonLogic.SetCookieValue(StringConstants.RememberPassword, CommonLogic.Encrypt(model.Password), DateTime.Now.AddDays(30));

                        }
                        else
                        {
                            // Clear any existing persistent cookies
                            CommonLogic.ClearCookie("MV5AuthToken");
                            CommonLogic.ClearCookie(StringConstants.RememberUserName);
                            CommonLogic.ClearCookie(StringConstants.RememberPassword);
                          
                        }

                        // Redirect to appropriate page
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return this.Redirect(returnUrl);
                        }
                        else
                        {
                            return this.RedirectToAction<AdminDashboardController>(dc => dc.Index());
                        }
                    }
                    else if (loggedUser.SystemUserId == -1)
                    {
                        ViewBag.ErrorMsg = StringConstants.UserNotExist;
                    }
                    else if (loggedUser.SystemUserId == -2)
                    {
                        ViewBag.ErrorMsg = StringConstants.UserNotActive;
                    }
                    else
                    {
                        ViewBag.ErrorMsg = StringConstants.UserLoginFailed;
                    }
                }
                else
                {
                    ViewBag.ErrorMsg = StringConstants.UserNotExist;
                }
            }

            return this.View(model);
        }

        // Helper method to generate a secure token
        private string GenerateSecureToken(long userId)
        {
            // Create a unique token combining userId and random values
            string uniqueString = userId.ToString() + Guid.NewGuid().ToString() + DateTime.Now.Ticks.ToString();

            // Generate a secure hash of the token
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(uniqueString));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    builder.Append(hashBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // Helper method to restore user session
        public static void RestoreUserSession(long userId)
        {
            try
            {
                // Get user data
                SystemUsersModel user = UserBLL.GetSystemUserById(userId);

                if (user != null)
                {
                    // Restore session values
                    CommonLogic.SetSessionValue(StringConstants.UserId, user.SystemUserId);
                    CommonLogic.SetSessionValue(StringConstants.UserName, user.Email);
                    CommonLogic.SetSessionValue(StringConstants.FullName, user.FullName);
                    CommonLogic.SetSessionValue(StringConstants.RoleId, user.SystemRoleId);
                    CommonLogic.SetSessionValue(StringConstants.RoleName, user.SystemRoleName);
                    CommonLogic.SetSessionValue(StringConstants.ProfilePic, user.ProfilePic);

                    // Get system settings
                    SystemSettingModel settingModel = SystemSettingBLL.GetSetting();
                    CommonLogic.SetSessionValue(StringConstants.FinancialYear, settingModel.FYearId);

                    // Restore tenant information
                    CommonLogic.SetSessionValue(StringConstants.TenantID, user.TenantID);
                    CommonLogic.SetSessionValue(StringConstants.TenantName, user.TenantName);
                    CommonLogic.SetSessionValue(StringConstants.TenantUserId, user.TenantUserId);
                    CommonLogic.SetSessionValue(StringConstants.ActiveSessionID, user.ActiveSessionID);
                    CommonLogic.SetSessionValue(StringConstants.ActiveSessionYear, user.ActiveSessionYear);
                    CommonLogic.SetSessionValue(StringConstants.TenantCode, user.SchoolCode);
                    // Restore school information if available
                    if (user.SchoolId > 0)
                    {
                        SchoolModel schoolModel = SchoolBLL.GetById(Convert.ToInt64(user.SchoolCode));
                        if (schoolModel != null)
                        {
                            CommonLogic.SetSessionValue(StringConstants.SchoolName, schoolModel.SchoolName);
                            CommonLogic.SetSessionValue(StringConstants.ActiveSessionPrint, schoolModel.ActiveSessionPrint);
                            CommonLogic.SetSessionValue(StringConstants.SchoolCode, schoolModel.SchoolCode);
                            CommonLogic.SetSessionValue(StringConstants.PrintTitle, schoolModel.PrintTitle);
                            CommonLogic.SetSessionValue(StringConstants.Line1, schoolModel.Line1);
                            CommonLogic.SetSessionValue(StringConstants.Line2, schoolModel.Line2);
                            CommonLogic.SetSessionValue(StringConstants.Line3, schoolModel.Line3);
                            CommonLogic.SetSessionValue(StringConstants.Line4, schoolModel.Line4);
                            CommonLogic.SetSessionValue(StringConstants.ActiveHeaderImg, schoolModel.ActiveHeaderImg);
                            CommonLogic.SetSessionValue(StringConstants.LogoImg, schoolModel.LogoImg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Logger.Error(ex.Message);
            }
        }
        private void SaveAuthToken(long userId, string token, DateTime expiryDate)
        {
            // Example implementation:
            // Create a model to hold token data
            AuthTokenModel tokenModel = new AuthTokenModel
            {
                UserId = userId,
                Token = token,
                CreatedDate = DateTime.Now,
                ExpiryDate = expiryDate,
                IsActive = true
            };

            // Call your business logic layer to save the token
            // AuthTokenBLL.SaveToken(tokenModel);

            // Note: You will need to create:
            // 1. A database table to store tokens (UserId, Token, CreatedDate, ExpiryDate, IsActive)
            // 2. A model class (AuthTokenModel)
            // 3. Data access methods (AuthTokenBLL)
        }
        /// <summary>
        /// Logout view action.
        /// </summary>
        /// <returns>
        /// Returns logout action result.
        /// </returns>
        public ActionResult Logout()
        {
            // Get user ID before clearing session
            long userId = 0;
            if (CommonLogic.GetSessionValue(StringConstants.UserId) != null)
            {
                try
                {
                    userId = Convert.ToInt64(CommonLogic.GetSessionValue(StringConstants.UserId));
                }
                catch (Exception ex) { 
                
                }
            }

            // Log the logout action
            if (userId > 0)
            {
                SystemLoginHistoryModel loginHistory = new SystemLoginHistoryModel();
                loginHistory.UserId = userId;
                loginHistory.Action = SystemLoginHistoryAction.LogOut;
                loginHistory.IPAddress = CommonLogic.GetClientIPAddress();
                LoginHistoryBLL.SaveLoginHistory(loginHistory);

                // Invalidate any auth tokens for this user
                AuthTokenBLL.InvalidateTokens(userId);
            }

            // Clear authentication
            FormsAuthentication.SignOut();

            // Clear cookies
            CommonLogic.ClearCookie("MV5AuthToken");
            CommonLogic.ClearCookie(StringConstants.RememberUserName);
            CommonLogic.ClearCookie(StringConstants.RememberPassword);

            // Clear session
            Session.Clear();
            Session.Abandon();

            // Redirect to login page
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Forgot password view action.
        /// </summary>
        /// <returns>
        /// Returns forgot password action result.
        /// </returns>
       

        /// <summary>
        /// Forgot password view post action.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Returns forgot password action result.
        /// </returns>
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ForgotPassword(ForgotPasswordModel model)
        //{
        //    try
        //    {
        //        UserModel userModel = UserBLL.GetByEmail(model.Email.Trim());

        //        if (userModel != null)
        //        {
        //            EmailTemplateModel emailTemplate = EmailTemplateBLL.GetByCode("FORGOT_PASSWORD");

        //            if (emailTemplate != null)
        //            {
        //                string mailBody = emailTemplate.Message;
        //                mailBody = mailBody.Replace("##PASSWORD##", userModel.Password);
        //                mailBody = mailBody.Replace("##SIGNATURE_NAME##", emailTemplate.Signature);

        //                if (CommonLogic.SendMail(emailTemplate.FromEmail, userModel.Email, emailTemplate.Subject, mailBody, true))
        //                {
        //                    this.ViewBag.SuccessMsg = StringConstants.PasswordSentMsg;
        //                    model.Email = string.Empty;
        //                }
        //                else
        //                {
        //                    this.ViewBag.ErrorMsg = StringConstants.MailSendError;
        //                }
        //            }
        //            else
        //            {
        //                this.ViewBag.ErrorMsg = string.Format(StringConstants.RecordNotExist, "Email Template");
        //            }
        //        }
        //        else
        //        {
        //            this.ViewBag.ErrorMsg = string.Format(StringConstants.RecordNotExist, "Email");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        this.ViewBag.ErrorMsg = CommonLogic.GetExceptionMessage(ex);
        //    }

        //    return this.View(model);
        //}
        public ActionResult Reports(string ReportType, int RequestId)
        {
            var id = Convert.ToInt32(Session["Id"].ToString());
            LocalReport localreport = new LocalReport();
            localreport.ReportPath = Server.MapPath("~/Reports/LaundryReport.rdlc");
            ReportDataSource reportDataSource = new ReportDataSource();
            reportDataSource.Name = "DataSet1";
            reportDataSource.Value = "as";
            localreport.DataSources.Add(reportDataSource);
            string reportType = ReportType;
            string mimeType;
            string encoding;
            string fileNameExtension;
            if (reportType == "Excel")
            {
                fileNameExtension = ".xlsx";
            }
            else if (reportType == "word")
            {
                fileNameExtension = ".docx";
            }
            else if (reportType == "PDF")
            {
                fileNameExtension = ".pdf";
            }
            else
            {
                fileNameExtension = ".jpg";
            }
            string[] streams;
            Warning[] warnings;
            byte[] renderedByte;
            renderedByte = localreport.Render(reportType, "", out mimeType, out encoding, out fileNameExtension, out streams, out warnings);
            Response.AppendHeader("content-Disposition", "attachment; filename=RDLC." + fileNameExtension);
            return File(renderedByte, fileNameExtension);

        }
        //void loadme()
        //{
        //    Warning[] warnings;
        //    string[] streamids;
        //    string mimeType;
        //    string encoding;
        //    string extension;
        //    try
        //    {
        //        var reportViewer1 = new ReportViewer
        //        {
        //            ProcessingMode = ProcessingMode.Local
        //        };
        //        //reportViewer1.LocalReport.ReportPath = ReportPath;
        //        reportViewer1.LocalReport.ReportPath = AppDomain.CurrentDomain.BaseDirectory + @"\RDLCReport\quotation1.rdlc";


        //        reportViewer1.LocalReport.EnableExternalImages = true;
        //        reportViewer1.LocalReport.EnableHyperlinks = true;


        //        try
        //        {
        //            //ReportParameter p1 = new ReportParameter("CertificateNo", "CertificateNo");
        //            //ReportParameter p2 = new ReportParameter("EnrollMentNo", "EnrollMentNo");
        //            //ReportParameter p3 = new ReportParameter("name", "name");
        //            //ReportParameter p4 = new ReportParameter("mname", "mname");
        //            //ReportParameter p5 = new ReportParameter("fname", "fname");
        //            //ReportParameter p6 = new ReportParameter("address", "address");
        //            //ReportParameter p7 = new ReportParameter("fromclass", "fromclass");
        //            //ReportParameter p8 = new ReportParameter("toclass", "toclass");
        //            //ReportParameter p9 = new ReportParameter("status", "status");
        //            //ReportParameter p10 = new ReportParameter("examyear", "examyear");
        //            //ReportParameter p11 = new ReportParameter("dob", "dob");
        //            //ReportParameter p12 = new ReportParameter("dobinword", "dobinword");
        //            //ReportParameter p13 = new ReportParameter("paidupto", "paidupto");
        //            //ReportParameter p14 = new ReportParameter("date", "date");

        //            //reportViewer1.LocalReport.SetParameters(new ReportParameter[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14 });
        //        }
        //        catch (Exception ex1)
        //        {
        //            System.Web.HttpContext.Current.Response.Write("<H1>Network Error Try Again</H1>" + ex1.InnerException);
        //        }

        //        Byte[] bytes = reportViewer1.LocalReport.Render("PDF", null, out mimeType, out encoding, out extension, out streamids, out warnings);
        //        System.Web.HttpContext.Current.Response.ClearContent();
        //        System.Web.HttpContext.Current.Response.Buffer = true;
        //        System.Web.HttpContext.Current.Response.ContentType = "application/pdf";
        //        System.Web.HttpContext.Current.Response.BinaryWrite(bytes);
        //        System.Web.HttpContext.Current.Response.Flush(); // send it to the client to download
        //        System.Web.HttpContext.Current.Response.Clear();




        //    }
        //    catch (Exception ex)
        //    {
        //        System.Web.HttpContext.Current.Response.Write("<H1>Network Error Try Again</H1>" + ex.InnerException);
        //    }
        //}
        #endregion
    }
}
