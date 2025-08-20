using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using ERPIndia.ViewModel;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    //[LogOnAuthorize(Roles = "1")]
    public class LedgerController : BaseController
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
                ViewBag.SuccessMsg = string.Format(StringConstants.RecordSave, "Ledger");
            }

            ViewBag.PageNo = page ?? 1;
            ViewBag.PageSize = pageSize ?? ERPIndiaApplication.Setting.PageSize;
            ViewBag.SearchField = (string.IsNullOrEmpty(searchField) && !string.IsNullOrEmpty(searchValue)) ? "LDate" : searchField;
            ViewBag.SearchValue = searchValue;
            ViewBag.SortField = !string.IsNullOrEmpty(sortField) ? sortField : "LDate";
            ViewBag.SortOrder = !string.IsNullOrEmpty(sortOrder) ? sortOrder : "DESC";

            string mode = CommonLogic.GetFormDataString("mode");
            string ids = CommonLogic.GetFormDataString("ids");

            if (!string.IsNullOrEmpty(mode) && !string.IsNullOrEmpty(ids))
            {
                try
                {
                    MultiOperationType operationType = (MultiOperationType)Enum.Parse(typeof(MultiOperationType), mode, true);
                    LedgerBLL.UpdateMultipleRecords(operationType, ids);
                }
                catch (Exception ex)
                {
                    return this.Json(new { IsError = true, ErrorMsg = CommonLogic.GetExceptionMessage(ex) });
                }
            }

            long UserId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId));
            List<LedgerModel> LedgerList = LedgerBLL.GetAllPayment(ViewBag.SearchField, ViewBag.SearchValue, ViewBag.SortField, ViewBag.SortOrder, ViewBag.PageNo, ViewBag.PageSize, UserId);
            int totalRecords = 0;

            if (LedgerList != null && LedgerList.Count > 0)
            {
                totalRecords = LedgerList[0].TotalRecordCount;
            }

            ViewBag.ListRecords = LedgerList.Count;
            ViewBag.PagedList = LedgerList.ToStaticPagedList((int)ViewBag.PageNo, (int)ViewBag.PageSize, totalRecords);

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", LedgerList) : this.View(LedgerList);
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
            var user = new LedgerViewModel();
            ViewBag.Heading = "Add Ledger";

            if (id.HasValue && id > 0)
            {
                ViewBag.Heading = "Edit Ledger";

                var ledger = LedgerBLL.GetById(id ?? 0);

                user.AccountId = ledger.AccountId;
                user.LDate = ledger.LDate;
                user.Amount = ledger.LCrAmt;
                user.LedgerId = ledger.LedgerId;
                user.LPayMode = ledger.LPayMode;
                user.LRemarks = ledger.LRemarks;
                user.RefNo = ledger.RefNo;
                user.LVoucherType = ledger.LVoucherType;
                if (user == null)
                {
                    user = new LedgerViewModel();
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
        public ActionResult Add(long? id, LedgerViewModel model)
        {
            try
            {
                ViewBag.Heading = "Add Ledger";

                if (model.LedgerId > 0)
                {
                    ViewBag.Heading = "Edit Ledger ";
                }

                if (ModelState.IsValid)
                {
                    LedgerModel ledgerModel = new LedgerModel();
                    ledgerModel.AccountId = model.AccountId;
                    ledgerModel.RefNo = model.RefNo;
                    ledgerModel.LVoucherType = model.LVoucherType;
                    ledgerModel.LDate = model.LDate;
                    ledgerModel.LCrAmt = model.Amount;
                    ledgerModel.LedgerId = model.LedgerId;
                    ledgerModel.LPayMode = model.LPayMode;
                    ledgerModel.LRemarks = model.LRemarks;
                    ledgerModel.CompanyId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.CompanyId));
                    ledgerModel.CreatedBy = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId));

                    LedgerModel returnUser = LedgerBLL.Save(ledgerModel);

                    if (returnUser.LedgerId == -1)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordAlreadyExist, returnUser.DuplicateColumn);
                    }
                    else if (returnUser.LedgerId == 0)
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