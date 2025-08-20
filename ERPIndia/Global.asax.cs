using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Hangfire;
using Hangfire.SqlServer;
using System.Diagnostics;
using System.Threading;
using AutoMapper;
using ERPIndia.Models.SystemSettings;
using log4net.Config;
using System.IO;
using ERPIndia.Utilities;
using ERPIndia.Models.TokenAuth;
using System.Web.Security;

namespace ERPIndia
{
    public class ERPIndiaApplication : System.Web.HttpApplication
    {
        #region Variable Declaration
        private IEnumerable<IDisposable> GetHangfireServers()
        {
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ToString(), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });

            yield return new BackgroundJobServer();
        }
       
        private static SystemSettingModel setting;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the setting.
        /// </summary>
        /// <value>
        /// The setting.
        /// </value>
        public static SystemSettingModel Setting
        {
            get
            {
                if (setting == null)
                {
                    SetGlobalVariable();
                }

                return setting;
            }

            set
            {
                setting = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the global variable.
        /// </summary>
        public static void SetGlobalVariable()
        {
            setting = SystemSettingBLL.GetSetting();

            if (setting != null)
            {
                CommonLogic.SetApplicationValue("SMTPHost", setting.SMTPHost);
                CommonLogic.SetApplicationValue("SMTPPort", setting.SMTPPort);
                CommonLogic.SetApplicationValue("SMTPUserName", setting.SMTPUserName);
                CommonLogic.SetApplicationValue("SMTPPassword", setting.SMTPPassword);
            }
            else
            {
                setting = new SystemSettingModel();
            }
        }

        #endregion
        protected void Application_Start()
        {
            XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/log4net.config")));
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());
            

            //build the mapper
         
            //..register mapper with the dependency container used by your application.

           

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //HangfireAspNet.Use(GetHangfireServers);

            // Let's also create a sample background job
            //BackgroundJob.Enqueue(() => Debug.WriteLine("Hello world from Hangfire!"));
            //BackgroundJob.Schedule(() => Utils.UpdateData(),TimeSpan.FromMinutes(1));
        }
        protected void Session_Start()
        {
            // Your existing Session_Start code

            // Check if we need to restore session data from a persistent login
            HttpCookie userIdCookie = Request.Cookies["MV5UserId"];
            if (userIdCookie != null)
            {
                try
                {
                    long userId = Convert.ToInt64(userIdCookie.Value);

                    // Now that Session is available, restore user data
                    RestoreUserSession(userId);

                    // Remove the temporary cookie
                    Response.Cookies["MV5UserId"].Expires = DateTime.Now.AddDays(-1);
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Logger.Error(ex.Message);
                }
            }
        }
        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            HttpContext context = HttpContext.Current;

            // Check if the request is for the login page
            if (context.Request.Path.ToLower().EndsWith("/account/login") ||
                context.Request.Path.ToLower().EndsWith("/login"))
            {
                // Check for our auth token even if not authenticated yet
                HttpCookie authCookie = Request.Cookies["MV5AuthToken"];
                if (authCookie != null)
                {
                    string token = authCookie.Value;

                    // Get the auth token from database
                    AuthTokenModel tokenModel = AuthTokenBLL.GetTokenByValue(token);

                    if (tokenModel != null && tokenModel.IsActive && DateTime.Now < tokenModel.ExpiryDate)
                    {
                        // Valid token found - user should be authenticated

                        // Get and authenticate user
                        SystemUsersModel user = UserBLL.GetSystemUserById(tokenModel.UserId);
                        if (user != null)
                        {
                            // Force authentication
                            FormsAuthentication.SetAuthCookie(user.Email, true);

                            // Store user ID in cookie for session restoration
                            HttpCookie userIdCookie = new HttpCookie("MV5UserId");
                            userIdCookie.Value = tokenModel.UserId.ToString();
                            userIdCookie.Expires = DateTime.Now.AddMinutes(5);
                            Response.Cookies.Add(userIdCookie);

                            // Redirect to dashboard - prevent login page from showing
                            if (!context.Response.IsRequestBeingRedirected)
                            {
                                // Redirect to SiteMap index (your dashboard)
                                context.Response.Redirect("/AdminDashboard");
                            }
                        }
                    }
                    else if (tokenModel != null && !tokenModel.IsActive)
                    {
                        // Invalid token, clear it
                        Response.Cookies["MV5AuthToken"].Expires = DateTime.Now.AddDays(-1);
                    }
                }
            }
            // For non-login pages, handle authenticated users
            else if (context.User != null && context.User.Identity.IsAuthenticated)
            {
                // Check if we have our custom auth token cookie
                HttpCookie authCookie = Request.Cookies["MV5AuthToken"];
                if (authCookie != null)
                {
                    string token = authCookie.Value;

                    // Get the auth token from database
                    AuthTokenModel tokenModel = AuthTokenBLL.GetTokenByValue(token);

                    if (tokenModel != null && tokenModel.IsActive && DateTime.Now < tokenModel.ExpiryDate)
                    {
                        // Store user ID in cookie for session restoration
                        HttpCookie userIdCookie = new HttpCookie("MV5UserId");
                        userIdCookie.Value = tokenModel.UserId.ToString();
                        userIdCookie.Expires = DateTime.Now.AddMinutes(5);
                        Response.Cookies.Add(userIdCookie);
                    }
                    else
                    {
                        // Invalid or expired token, clear it
                        Response.Cookies["MV5AuthToken"].Expires = DateTime.Now.AddDays(-1);
                    }
                }
            }
        }
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
                    CommonLogic.SetSessionValue(StringConstants.ActiveSessionYear, user.SchoolCode);
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
        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            Logger.Error(ex?.ToString());

            if (ex is ArgumentException)
            {
                // Stop ASP.NET from re-throwing / custom error handling
                Server.ClearError();

                // Make sure nothing has been written yet
                Response.Clear();
                Response.TrySkipIisCustomErrors = true;

                // Redirect without ending the response
                Response.Redirect("~/Account/Login", false);

                // Short-circuit the pipeline safely
                Context.ApplicationInstance.CompleteRequest();
            }
        }

    }

}
