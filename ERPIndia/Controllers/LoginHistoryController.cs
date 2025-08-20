using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    /// <summary>
    /// Login history controller class.
    /// </summary>
    [LogOnAuthorize]
    public class LoginHistoryController : BaseController
    {
        #region Action Methods

        /// <summary>
        /// Index view action.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="searchField">The search field.</param>
        /// <param name="searchValue">The search value.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <returns>
        /// Returns index action result.
        /// </returns>
        public ActionResult Index(int? page, int? pageSize, string searchField, string searchValue, string sortField, string sortOrder)
        {
            ViewBag.PageNo = page ?? 1;
            ViewBag.PageSize = pageSize ?? ERPIndiaApplication.Setting.PageSize;
            ViewBag.SearchField = (string.IsNullOrEmpty(searchField) && !string.IsNullOrEmpty(searchValue)) ? "ActionDate" : searchField;
            ViewBag.SearchValue = searchValue;
            ViewBag.SortField = !string.IsNullOrEmpty(sortField) ? sortField : "ActionDate";
            ViewBag.SortOrder = !string.IsNullOrEmpty(sortOrder) ? sortOrder : "DESC";

            string mode = CommonLogic.GetFormDataString("mode");
            string ids = CommonLogic.GetFormDataString("ids");

            if (!string.IsNullOrEmpty(mode) && !string.IsNullOrEmpty(ids))
            {
                try
                {
                    MultiOperationType operationType = (MultiOperationType)Enum.Parse(typeof(MultiOperationType), mode, true);
                    LoginHistoryBLL.UpdateMultipleRecords(operationType, ids);
                }
                catch (Exception ex)
                {
                    return this.Json(new { IsError = true, ErrorMsg = CommonLogic.GetExceptionMessage(ex) });
                }
            }

            List<SystemLoginHistoryModel> loginHistoryList = LoginHistoryBLL.GetLoginHistory(ViewBag.SearchField, ViewBag.SearchValue, ViewBag.SortField, ViewBag.SortOrder, ViewBag.PageNo, ViewBag.PageSize);
            int totalRecords = 0;

            if (loginHistoryList != null && loginHistoryList.Count > 0)
            {
                totalRecords = loginHistoryList[0].TotalRecordCount;
            }

            ViewBag.ListRecords = loginHistoryList.Count;
            ViewBag.PagedList = loginHistoryList.ToStaticPagedList((int)ViewBag.PageNo, (int)ViewBag.PageSize, totalRecords);

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", loginHistoryList) : this.View(loginHistoryList);
        }

        #endregion
    }
}
