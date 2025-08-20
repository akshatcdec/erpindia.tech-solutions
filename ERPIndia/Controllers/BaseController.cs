using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace ERPIndia.Controllers
{
    /// <summary>
    /// URL helper extension class.
    /// </summary>
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Actions the specified helper.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="helper">The helper.</param>
        /// <param name="action">The action.</param>
        /// <returns>Returns action name.</returns>
        public static string Action<TController>(this UrlHelper helper, Expression<Func<TController, object>> action) where TController : ControllerBase
        {
            RouteValueDictionary rv = new RouteValueDictionary();
            return helper.Action(action.GetMethodName(), BaseController.GetControllerName<TController>(), rv);
        }

        /// <summary>
        /// Actions the specified helper.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="helper">The helper.</param>
        /// <param name="action">The action.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>Returns action name.</returns>
        public static string Action<TController>(this UrlHelper helper, Expression<Func<TController, object>> action, object routeValues) where TController : ControllerBase
        {
            RouteValueDictionary rv = new RouteValueDictionary(routeValues);
            return helper.Action(action.GetMethodName(), BaseController.GetControllerName<TController>(), rv);
        }

        /// <summary>
        /// Actions the specified helper.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="helper">The helper.</param>
        /// <param name="action">The action.</param>
        /// <param name="routeValues">The route values.</param>
        /// <param name="removeKeys">The remove keys.</param>
        /// <returns>Returns action name.</returns>
        public static string Action<TController>(this UrlHelper helper, Expression<Func<TController, object>> action, object routeValues, string[] removeKeys) where TController : ControllerBase
        {
            UrlHelper urlHelper = new UrlHelper(helper.RequestContext);

            if (removeKeys != null)
            {
                foreach (string key in removeKeys)
                {
                    if (urlHelper.RequestContext.RouteData.Values.ContainsKey(key))
                    {
                        urlHelper.RequestContext.RouteData.Values.Remove(key);
                    }
                }
            }

            return urlHelper.Action(action.GetMethodName(), BaseController.GetControllerName<TController>(), routeValues);
        }
    }

    /// <summary>
    /// Expression extension class.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <typeparam name="TReturn">The type of the return.</typeparam>
        /// <param name="method">The method.</param>
        /// <returns>Returns method name.</returns>
        public static string GetMethodName<T, TReturn>(this Expression<Func<T, TReturn>> method)
        {
            if (!(method.Body is MethodCallExpression))
            {
                return string.Empty;
            }

            return ((MethodCallExpression)method.Body).Method.Name;
        }

        /// <summary>
        /// Expression missing method exception class.
        /// </summary>
        [SerializableAttribute]
        public class ExpressionMissingMethodException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ExpressionMissingMethodException"/> class.
            /// </summary>
            public ExpressionMissingMethodException()
                : base("Expression must call a method.")
            {
            }
        }
    }

    /// <summary>
    /// Base controller class.
    /// </summary>
    public class BaseController : Controller
    {
        /// <summary>
        /// Checks if tenant code is empty or zero and redirects to login if it is
        /// </summary>
        /// <returns>True if redirect is needed, false otherwise</returns>
        protected bool CheckAndRedirectIfTenantCodeInvalid()
        {
            string tenantCode = CurrentTenantCode;

            if (string.IsNullOrEmpty(tenantCode) || tenantCode == "0")
            {
                // Redirect to login page - assuming your login controller is named "Account" with "Login" action
                Response.Redirect("~/Account/Login");
                return true;
            }

            return false;
        }
        #region Public Properties

        /// <summary>
        /// Gets the current user's TenantID from session
        /// </summary>
        public Guid CurrentTenantID
        {
            get
            {
                string tenantIDStr = CommonLogic.GetSessionValue(StringConstants.TenantID);
                if (!string.IsNullOrEmpty(tenantIDStr))
                {
                    return Guid.Parse(tenantIDStr);
                }
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Gets the current user's TenantName from session
        /// </summary>
        public string CurrentTenantName
        {
            get
            {
                return CommonLogic.GetSessionValue(StringConstants.TenantName);
            }
        }

        /// <summary>
        /// Gets the current user's TenantUserID from session
        /// </summary>
        public Guid CurrentTenantUserID
        {
            get
            {
                string tenantUserIDStr = CommonLogic.GetSessionValue(StringConstants.TenantUserId);
                if (!string.IsNullOrEmpty(tenantUserIDStr))
                {
                    return Guid.Parse(tenantUserIDStr);
                }
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Gets the current active SessionID from session
        /// </summary>
        public Guid CurrentSessionID
        {
            get
            {
                string sessionIDStr = CommonLogic.GetSessionValue(StringConstants.ActiveSessionID);
                if (!string.IsNullOrEmpty(sessionIDStr))
                {
                    return Guid.Parse(sessionIDStr);
                }
                return Guid.Empty;
            }
        }
        #endregion Public Properties
        /// <summary>
        /// Gets the current active SessionYear from session
        /// </summary>
        public int CurrentSessionYear
        {
            get
            {
                string sessionYearStr = CommonLogic.GetSessionValue(StringConstants.ActiveSessionYear);
                if (!string.IsNullOrEmpty(sessionYearStr))
                {
                    return int.Parse(sessionYearStr);
                }
                return 0;
            }
        }

        /// <summary>
        /// Gets the current SchoolCode from session
        /// </summary>
        public string CurrentSchoolCode
        {
            get
            {
                return CommonLogic.GetSessionValue(StringConstants.SchoolCode);
            }
        }
        public string CurrentTenantCode
        {
            get
            {
                return CommonLogic.GetSessionValue(StringConstants.TenantCode);
            }
        }
        public int TenantCode
        {
            get
            {
                return Utils.ParseInt(CommonLogic.GetSessionValue(StringConstants.TenantCode));
            }
        }
        #region Public Methods

        /// <summary>
        /// Gets the name of the controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <returns>Returns controller name.</returns>
        public static string GetControllerName<TController>()
        {
            var type = typeof(TController);
            return type.Name.Remove(type.Name.Length - 10);
        }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>Returns action name.</returns>
        public static string GetActionName<TController>(Expression<Func<TController, object>> action)
        {
            try
            {
                return action.GetMethodName();
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Redirects to action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="action">The action.</param>
        /// <returns>Returns route result.</returns>
        public RedirectToRouteResult RedirectToAction<TController>(Expression<Func<TController, object>> action)
        {
            return this.RedirectToAction(BaseController.GetActionName(action), BaseController.GetControllerName<TController>());
        }

        /// <summary>
        /// Redirects to action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="action">The action.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>Returns route result.</returns>
        public RedirectToRouteResult RedirectToAction<TController>(Expression<Func<TController, object>> action, object routeValues)
        {
            return this.RedirectToAction(BaseController.GetActionName(action), BaseController.GetControllerName<TController>(), routeValues);
        }

        /// <summary>
        /// Gets the site menu list.
        /// </summary>
        /// <returns>
        /// Returns site menu list.
        /// </returns>
        public List<SiteMenuModel> GetSiteMenuList()
        {
            try
            {
                List<SiteMenuModel> menuList = new List<SiteMenuModel>();

                if (CommonLogic.GetSessionObject("MenuList") != null)
                {
                    menuList = CommonLogic.GetSessionObject("MenuList") as List<SiteMenuModel>;
                }
                else
                {
                    menuList = SiteMenuBLL.GetAll(SqlHelper.ParseNativeInt(CommonLogic.GetSessionValue("RoleId")), true);

                    if (menuList != null)
                    {
                        CommonLogic.SetSessionValue("MenuList", menuList);
                    }
                }

                return menuList;
            }
            catch
            {
                return new List<SiteMenuModel>();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Initializes data that might not be available when the constructor is called.
        /// </summary>
        /// <param name="requestContext">The HTTP context and route data.</param>
        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            base.Initialize(requestContext);

            // Check if tenant code is valid before proceeding
            // Skip the check for the Login page to avoid redirect loops
            if (RouteData.Values["controller"].ToString().ToLower() != "account" &&
                RouteData.Values["action"].ToString().ToLower() != "login")
            {
                // Check tenant code and redirect if needed
                if (CheckAndRedirectIfTenantCodeInvalid())
                {
                    // Return early if redirect is needed
                    return;
                }
            }

            if (!string.IsNullOrEmpty(CommonLogic.GetSessionValue(StringConstants.UserId)))
            {
                List<SiteMenuModel> menuList = this.GetSiteMenuList();
                this.ViewBag.MenuList = menuList;
                SiteMenuModel sitemenu = menuList.Find(m => m.MenuPageName.Equals(RouteData.Values["controller"].ToString(), StringComparison.OrdinalIgnoreCase));

                if (sitemenu != null)
                {
                    CommonLogic.SetSessionValue("MainMenuId", sitemenu.ParentMenuId == 0 ? sitemenu.MenuId : sitemenu.ParentMenuId);
                }
            }
        }
        #endregion
    }
}