using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    /// <summary>
    /// Company controller class.
    /// </summary>
    [LogOnAuthorize(Roles = "1")]
    public class EmailTemplateController : BaseController
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
                ViewBag.SuccessMsg = string.Format(StringConstants.RecordSave, "Email Template");
            }

            ViewBag.PageNo = page ?? 1;
            ViewBag.PageSize = pageSize ?? ERPIndiaApplication.Setting.PageSize;
            ViewBag.SearchField = (string.IsNullOrEmpty(searchField) && !string.IsNullOrEmpty(searchValue)) ? "Title" : searchField;
            ViewBag.SearchValue = searchValue;
            ViewBag.SortField = !string.IsNullOrEmpty(sortField) ? sortField : "Title";
            ViewBag.SortOrder = !string.IsNullOrEmpty(sortOrder) ? sortOrder : "ASC";

            string mode = CommonLogic.GetFormDataString("mode");
            string ids = CommonLogic.GetFormDataString("ids");

            if (!string.IsNullOrEmpty(mode) && !string.IsNullOrEmpty(ids))
            {
                try
                {
                    MultiOperationType operationType = (MultiOperationType)Enum.Parse(typeof(MultiOperationType), mode, true);
                    EmailTemplateBLL.UpdateMultipleRecords(operationType, ids);
                }
                catch (Exception ex)
                {
                    return this.Json(new { IsError = true, ErrorMsg = CommonLogic.GetExceptionMessage(ex) });
                }
            }

            List<EmailTemplateModel> list = EmailTemplateBLL.GetAll(ViewBag.SearchField, ViewBag.SearchValue, ViewBag.SortField, ViewBag.SortOrder, ViewBag.PageNo, ViewBag.PageSize);
            int totalRecords = 0;

            if (list != null && list.Count > 0)
            {
                totalRecords = list[0].TotalRecordCount;
            }

            ViewBag.ListRecords = list.Count;
            ViewBag.PagedList = list.ToStaticPagedList((int)ViewBag.PageNo, (int)ViewBag.PageSize, totalRecords);

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", list) : this.View(list);
        }

        /// <summary>
        /// Add view action..
        /// </summary>
        /// <param name="id">The The identifier..</param>
        /// <returns>Returns add action result.</returns>
        public ActionResult Add(long? id)
        {
            EmailTemplateModel model = new EmailTemplateModel();
            ViewBag.Heading = "Add Email Template";

            if (id.HasValue && id > 0)
            {
                ViewBag.Heading = "Edit Email Template";
                model = EmailTemplateBLL.GetById(id ?? 0);

                if (model == null)
                {
                    model = new EmailTemplateModel();
                }
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Add", model) : this.View(model);
        }

        /// <summary>
        /// Add view post action..
        /// </summary>
        /// <param name="id">The The identifier..</param>
        /// <param name="model">The The model..</param>
        /// <returns>Returns add action result.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(long? id, EmailTemplateModel model)
        {
            try
            {
                ViewBag.Heading = "Add Email Template";

                if (model.EmailTemplateId > 0)
                {
                    ViewBag.Heading = "Edit Email Template";
                }

                if (ModelState.IsValid)
                {
                    EmailTemplateModel returnCompany = EmailTemplateBLL.Save(model);

                    if (returnCompany.EmailTemplateId == -1)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordAlreadyExist, returnCompany.DuplicateColumn);
                    }
                    else if (returnCompany.EmailTemplateId == 0)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordNotExist, returnCompany.DuplicateColumn);
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
