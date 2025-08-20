using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{

    public class ServicesController : BaseController
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
            if (CommonLogic.GetQueryString("status").Equals("s", StringComparison.CurrentCultureIgnoreCase))
            {
                ViewBag.SuccessMsg = string.Format(StringConstants.RecordSave, "Service");
            }

            ViewBag.PageNo = page ?? 1;
            ViewBag.PageSize = pageSize ?? ERPIndiaApplication.Setting.PageSize;
            ViewBag.SearchField = (string.IsNullOrEmpty(searchField) && !string.IsNullOrEmpty(searchValue)) ? "ServiceName" : searchField;
            ViewBag.SearchValue = searchValue;
            ViewBag.SortField = !string.IsNullOrEmpty(sortField) ? sortField : "ServiceName";
            ViewBag.SortOrder = !string.IsNullOrEmpty(sortOrder) ? sortOrder : "ASC";

            string mode = CommonLogic.GetFormDataString("mode");
            string ids = CommonLogic.GetFormDataString("ids");

            if (!string.IsNullOrEmpty(mode) && !string.IsNullOrEmpty(ids))
            {
                try
                {
                    MultiOperationType operationType = (MultiOperationType)Enum.Parse(typeof(MultiOperationType), mode, true);
                    ServiceBLL.UpdateMultipleRecords(operationType, ids);
                }
                catch (Exception ex)
                {
                    return this.Json(new { IsError = true, ErrorMsg = CommonLogic.GetExceptionMessage(ex) });
                }
            }


            List<ServiceModel> ServiceList = ServiceBLL.GetAll(ViewBag.SearchField, ViewBag.SearchValue, ViewBag.SortField, ViewBag.SortOrder, ViewBag.PageNo, ViewBag.PageSize);
            int totalRecords = 0;

            if (ServiceList != null && ServiceList.Count > 0)
            {
                totalRecords = ServiceList[0].TotalRecordCount;
            }

            ViewBag.ListRecords = ServiceList.Count;
            ViewBag.PagedList = ServiceList.ToStaticPagedList((int)ViewBag.PageNo, (int)ViewBag.PageSize, totalRecords);

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", ServiceList) : this.View(ServiceList);
        }

        /// <summary>
        /// Add view action.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// Returns add action result.
        /// </returns>
        public ActionResult Add(long? id)
        {
            var user = new ServiceModel();
            ViewBag.Heading = "Add Service";

            if (id.HasValue && id > 0)
            {
                ViewBag.Heading = "Edit Service";
                user = ServiceBLL.GetById(id ?? 0);

                if (user == null)
                {
                    user = new ServiceModel();
                }
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Add", user) : this.View(user);
        }

        /// <summary>
        /// Add view post action.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Returns add action result.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(long? id, ServiceModel model)
        {
            try
            {
                ViewBag.Heading = "Add Service";

                if (model.ServiceId > 0)
                {
                    ViewBag.Heading = "Edit Service ";
                }

                if (ModelState.IsValid)
                {
                    model.CompanyId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.CompanyId));
                    model.CreatedBy = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId));
                    model.FYearId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.FinancialYear));
                    //model.RoleId = RoleType.Patient.GetHashCode();

                    ServiceModel returnUser = ServiceBLL.Save(model);

                    if (returnUser.ServiceId == -1)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordAlreadyExist, returnUser.DuplicateColumn);
                    }
                    else if (returnUser.ServiceId == 0)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordNotExist, returnUser.DuplicateColumn);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMsg = CommonLogic.GetExceptionMessage(ex);
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Add", model) : this.View(model);
        }

        #endregion
    }
}