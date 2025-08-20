using Hangfire.Dashboard;
using Microsoft.Owin;
using ERPIndia.Class.Helper;
using System.Web.Mvc;
using System.Web;

public class MyAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // In case you need an OWIN context, use the next line, `OwinContext` class
        // is the part of the `Microsoft.Owin` package.
        var owinContext = new OwinContext(context.GetOwinEnvironment());
        var UserId = CommonLogic.GetSessionValue(StringConstants.UserId);
        if (string.IsNullOrEmpty(UserId))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    /*public bool Authorize(DashboardContext context)
    {
        // In case you need an OWIN context, use the next line, `OwinContext` class
        // is the part of the `Microsoft.Owin` package.
        var owinContext = new OwinContext(context.GetOwinEnvironment());

        // Allow all authenticated users to see the Dashboard (potentially dangerous).
        return owinContext.Authentication.User.Identity.IsAuthenticated;
    }
    */
}
//public class SessionTimeoutAttribute : ActionFilterAttribute
//{
//    public override void OnActionExecuting(ActionExecutingContext filterContext)
//    {
//        HttpContext ctx = HttpContext.Current;

//        if (HttpContext.Current.Session["UserIntId"] == null && HttpContext.Current.Session["UserId"] == null)
//        {
//            ctx.Session.Abandon();
//            ctx.Session.Clear();

//            filterContext.Result = new RedirectResult("~/Account/Login");
//            return;

//        }
//        base.OnActionExecuting(filterContext);
//    }
//}
