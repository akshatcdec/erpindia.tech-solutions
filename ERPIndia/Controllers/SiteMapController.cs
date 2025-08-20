using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    /// <summary>
    /// Site map controller class.
    /// </summary>
    [LogOnAuthorize]
    public class SiteMapController : BaseController
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
            List<SiteMenuModel> menuList = this.GetSiteMenuList();
            List<SiteMenuModel> currentList = menuList.FindAll(sm => !sm.MenuCode.Equals("SiteMap", StringComparison.OrdinalIgnoreCase) && sm.ParentMenuId == 0);

            if (currentList != null && menuList != null)
            {
                for (int i = 0; i < currentList.Count; i++)
                {
                    currentList[i].SubMenuList = menuList.FindAll(sm => sm.ParentMenuId == currentList[i].MenuId);
                }
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", currentList) : RedirectToAction("Index", "AdminDashboard");
        }

        #endregion
    }
}
