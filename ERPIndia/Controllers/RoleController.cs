using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    /// <summary>
    /// Role controller class.
    /// </summary>
    [LogOnAuthorize]
    public class RoleController : BaseController
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
                ViewBag.SuccessMsg = string.Format(StringConstants.RecordSave, "Role");
            }

            ViewBag.PageNo = page ?? 1;
            ViewBag.PageSize = pageSize ?? ERPIndiaApplication.Setting.PageSize;
            ViewBag.SearchField = (string.IsNullOrEmpty(searchField) && !string.IsNullOrEmpty(searchValue)) ? "RoleName" : searchField;
            ViewBag.SearchValue = searchValue;
            ViewBag.SortField = !string.IsNullOrEmpty(sortField) ? sortField : "RoleName";
            ViewBag.SortOrder = !string.IsNullOrEmpty(sortOrder) ? sortOrder : "ASC";

            string mode = CommonLogic.GetFormDataString("mode");
            string ids = CommonLogic.GetFormDataString("ids");

            if (!string.IsNullOrEmpty(mode) && !string.IsNullOrEmpty(ids))
            {
                try
                {
                    MultiOperationType operationType = (MultiOperationType)Enum.Parse(typeof(MultiOperationType), mode, true);
                    RoleBLL.UpdateMultipleRecords(operationType, ids);
                }
                catch (Exception ex)
                {
                    return this.Json(new { IsError = true, ErrorMsg = CommonLogic.GetExceptionMessage(ex) });
                }
            }

            List<RoleModel> roleList = RoleBLL.GetAll(ViewBag.SearchField, ViewBag.SearchValue, ViewBag.SortField, ViewBag.SortOrder, ViewBag.PageNo, ViewBag.PageSize);
            int totalRecords = 0;

            if (roleList != null && roleList.Count > 0)
            {
                totalRecords = roleList[0].TotalRecordCount;
            }

            ViewBag.ListRecords = roleList.Count;
            ViewBag.PagedList = roleList.ToStaticPagedList((int)ViewBag.PageNo, (int)ViewBag.PageSize, totalRecords);

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", roleList) : this.View(roleList);
        }

        /// <summary>
        /// Add view action..
        /// </summary>
        /// <param name="id">The The identifier..</param>
        /// <returns>Returns add action result.</returns>
        public ActionResult Add(long? id)
        {
            RoleModel roleModel = new RoleModel();
            ViewBag.Heading = "Add Role";

            if (id.HasValue && id > 0)
            {
                ViewBag.Heading = "Edit Role";
                roleModel = RoleBLL.GetById(id ?? 0);

                if (roleModel == null)
                {
                    roleModel = new RoleModel();
                }
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Add", roleModel) : this.View(roleModel);
        }

        /// <summary>
        /// Add view post action..
        /// </summary>
        /// <param name="id">The The identifier..</param>
        /// <param name="model">The The model..</param>
        /// <returns>Returns add action result.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(long? id, RoleModel model)
        {
            try
            {
                ViewBag.Heading = "Add Role";

                if (model.RoleId > 0)
                {
                    ViewBag.Heading = "Edit Role";
                }

                if (ModelState.IsValid)
                {
                    RoleModel returnRole = RoleBLL.Save(model);

                    if (returnRole.RoleId == -1)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordAlreadyExist, returnRole.DuplicateColumn);
                    }
                    else if (returnRole.RoleId == 0)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordNotExist, returnRole.DuplicateColumn);
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
