using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models.SystemSettings;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    /// <summary>
    /// School controller class.
    /// </summary>
    [LogOnAuthorize(Roles = "1")]
    public class CompanyController : BaseController
    {
        #region Action Methods

        /// <summary>
        /// Add view action..
        /// </summary>
        /// <param name="id">The The identifier..</param>
        /// <returns>Returns add action result.</returns>
        public ActionResult Index(int? page, int? pageSize, string searchField, string searchValue, string sortField, string sortOrder)
        {
            if (CommonLogic.GetQueryString("status").Equals("s", StringComparison.CurrentCultureIgnoreCase))
            {
                ViewBag.SuccessMsg = string.Format(StringConstants.RecordSave, "Hospital");
            }

            ViewBag.PageNo = page ?? 1;
            ViewBag.PageSize = pageSize ?? ERPIndiaApplication.Setting.PageSize;
            ViewBag.SearchField = (string.IsNullOrEmpty(searchField) && !string.IsNullOrEmpty(searchValue)) ? "SchoolName" : searchField;
            ViewBag.SearchValue = searchValue;
            ViewBag.SortField = !string.IsNullOrEmpty(sortField) ? sortField : "SchoolName";
            ViewBag.SortOrder = !string.IsNullOrEmpty(sortOrder) ? sortOrder : "ASC";

            string mode = CommonLogic.GetFormDataString("mode");
            string ids = CommonLogic.GetFormDataString("ids");

            if (!string.IsNullOrEmpty(mode) && !string.IsNullOrEmpty(ids))
            {
                try
                {
                    MultiOperationType operationType = (MultiOperationType)Enum.Parse(typeof(MultiOperationType), mode, true);
                    SchoolBLL.UpdateMultipleRecords(operationType, ids);
                }
                catch (Exception ex)
                {
                    return this.Json(new { IsError = true, ErrorMsg = CommonLogic.GetExceptionMessage(ex) });
                }
            }

            List<SchoolModel> SchoolList = SchoolBLL.GetAll(ViewBag.SearchField, ViewBag.SearchValue, ViewBag.SortField, ViewBag.SortOrder, ViewBag.PageNo, ViewBag.PageSize);
            int totalRecords = 0;

            if (SchoolList != null && SchoolList.Count > 0)
            {
                totalRecords = SchoolList[0].TotalRecordCount;
            }

            ViewBag.ListRecords = SchoolList.Count;
            ViewBag.PagedList = SchoolList.ToStaticPagedList((int)ViewBag.PageNo, (int)ViewBag.PageSize, totalRecords);

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", SchoolList) : this.View(SchoolList);
        }

        /// <summary>
        /// Add view action..
        /// </summary>
        /// <param name="id">The The identifier..</param>
        /// <returns>Returns add action result.</returns>
        public ActionResult Add(long? id)
        {
            SchoolModel SchoolModel = new SchoolModel();
            ViewBag.Heading = "Add Hospital";

            if (id.HasValue && id > 0)
            {
                ViewBag.Heading = "Edit Hospital";
                SchoolModel = SchoolBLL.GetById(id ?? 0);

                if (SchoolModel == null)
                {
                    SchoolModel = new SchoolModel();
                }
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Add", SchoolModel) : this.View(SchoolModel);
        }

        /// <summary>
        /// Add view post action..
        /// </summary>
        /// <param name="id">The The identifier..</param>
        /// <param name="model">The The model..</param>
        /// <returns>Returns add action result.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(long? id, SchoolModel model)
        {
            try
            {
                ViewBag.Heading = "Add Hospital";

                if (model.SchoolId > 0)
                {
                    ViewBag.Heading = "Edit Hospital";
                }

                if (ModelState.IsValid)
                {
                    SchoolModel returnSchool = SchoolBLL.Save(model);

                    if (returnSchool.SchoolId == -1)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordAlreadyExist, returnSchool.DuplicateColumn);
                    }
                    else if (returnSchool.SchoolId == 0)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordNotExist, returnSchool.DuplicateColumn);
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
        #region Helper Methods

        #endregion
    }
}
