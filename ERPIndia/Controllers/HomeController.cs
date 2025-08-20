using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
           return RedirectToAction("Index", "AdminDashboard");
        }
    }
}