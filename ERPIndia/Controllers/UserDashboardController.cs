using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class UserDashboardController : BaseController
    {
        // GET: UserDashboard
        public ActionResult Index()
        {
            return View();
        }
    }
}