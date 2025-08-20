using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.ViewModel;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    /// <summary>
    /// Dashboard controller class.
    /// </summary>
    [LogOnAuthorize]
    public class DashboardController : BaseController
    {
        #region Action Methods

        /// <summary>
        /// Index view action.
        /// </summary>
        /// <returns>
        /// Returns index action result.
        /// </returns>
        public ActionResult Index()
        {

            DashboardViewModel dashboardModel = SystemSettingBLL.GetDashboardDetails();
            if (dashboardModel == null)
            {
                dashboardModel = new DashboardViewModel();
                dashboardModel.RoleId = 0;
            }
            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", dashboardModel) : this.View(dashboardModel);
        }

        #endregion
    }
}
