using System.Web.Mvc;
using System.Web.Routing;

namespace ERPIndia
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
           routes.MapRoute(
           name: "StudentLedger",
           url: "CollectFee/Ledger/{id}/{sessionId}/{tenantId}",
           defaults: new { controller = "CollectFee", action = "Ledger" },
           constraints: new
           {
               id = @"\b[A-Fa-f0-9]{8}(?:-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12}\b",
               sessionId = @"\b[A-Fa-f0-9]{8}(?:-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12}\b",
               tenantId = @"\b[A-Fa-f0-9]{8}(?:-[A-Fa-f0-9]{4}){3}-[A-Fa-f0-9]{12}\b"
           }
       );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "SiteMap", action = "Index", id = UrlParameter.Optional }
            );
         
        }
    }

}
