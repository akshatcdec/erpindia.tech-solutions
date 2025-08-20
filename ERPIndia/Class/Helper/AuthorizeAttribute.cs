using ERPIndia.Controllers;
using System.Web.Mvc;

namespace ERPIndia.Class.Helper
{
    /// <summary>
    /// Logon authorize class.
    /// </summary>
    public class LogOnAuthorize : AuthorizeAttribute
    {
        /// <summary>
        /// Called when a process requests authorization.
        /// </summary>
        /// <param name="filterContext">The filter context, which encapsulates information for using <see cref="T:System.Web.Mvc.AuthorizeAttribute" />.</param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (string.IsNullOrEmpty(CommonLogic.GetSessionValue(StringConstants.UserId)) || !CommonLogic.HasRoles(this.Roles))
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.HttpContext.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                    filterContext.Result = new JsonResult { Data = new { Error = "NotAuthorized" }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                    filterContext.HttpContext.Response.End();
                }
                else
                {
                    CommonLogic.SessionSignOut();
                    filterContext.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary { { "controller", BaseController.GetControllerName<AccountController>() }, { "action", BaseController.GetActionName<AccountController>(a => a.Login(string.Empty)) }, { "returnUrl", filterContext.HttpContext.Request.RawUrl } });
                }
            }
        }
    }
}