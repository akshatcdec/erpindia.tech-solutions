using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using ERPIndia.ViewModel;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class ClientServiceController : BaseController
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
                ViewBag.SuccessMsg = string.Format(StringConstants.RecordSave, "Invoice");
            }

            ViewBag.PageNo = page ?? 1;
            ViewBag.PageSize = pageSize ?? ERPIndiaApplication.Setting.PageSize;
            ViewBag.SearchField = (string.IsNullOrEmpty(searchField) && !string.IsNullOrEmpty(searchValue)) ? "ClientName" : searchField;
            ViewBag.SearchValue = searchValue;
            ViewBag.SortField = !string.IsNullOrEmpty(sortField) ? sortField : "InvoiceDate";
            ViewBag.SortOrder = !string.IsNullOrEmpty(sortOrder) ? sortOrder : "DESC";

            string mode = CommonLogic.GetFormDataString("mode");
            string ids = CommonLogic.GetFormDataString("ids");

            if (!string.IsNullOrEmpty(mode) && !string.IsNullOrEmpty(ids))
            {
                try
                {
                    MultiOperationType operationType = (MultiOperationType)Enum.Parse(typeof(MultiOperationType), mode, true);
                    InvoiceBLL.UpdateMultipleRecords(operationType, ids);
                }
                catch (Exception ex)
                {
                    return this.Json(new { IsError = true, ErrorMsg = CommonLogic.GetExceptionMessage(ex) });
                }
            }


            List<InvoiceModel> invoiceList = InvoiceBLL.GetAll(ViewBag.SearchField, ViewBag.SearchValue, ViewBag.SortField, ViewBag.SortOrder, ViewBag.PageNo, ViewBag.PageSize);
            int totalRecords = 0;

            if (invoiceList != null && invoiceList.Count > 0)
            {
                totalRecords = invoiceList[0].TotalRecordCount;
            }

            ViewBag.ListRecords = invoiceList.Count;
            ViewBag.PagedList = invoiceList.ToStaticPagedList((int)ViewBag.PageNo, (int)ViewBag.PageSize, totalRecords);

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", invoiceList) : this.View(invoiceList);
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
            long UserId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId));
            var InvoiceVM = new InvoiceViewModel(id, UserId);
            ViewBag.Heading = "Add Invoice";

            ViewBag.Services = new SelectList(ServiceBLL.GetAllActive(), "ServiceId", "ServiceName");

            if (id.HasValue && id > 0)
            {
                ViewBag.Heading = "Edit Invoice";
                InvoiceVM.Invoice = InvoiceBLL.GetById(id ?? 0);

                if (InvoiceVM.Invoice == null)
                {
                    InvoiceVM.Invoice = new InvoiceModel();
                }
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Add", InvoiceVM) : this.View(InvoiceVM);
        }

        /// <summary>
        /// Add view post action.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="invoiceVM">The model.</param>
        /// <returns>
        /// Returns add action result.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(long? id, InvoiceViewModel invoiceVM, List<InvoiceDetailModel> patientTestDetailList)
        {
            try
            {
                ViewBag.Heading = "Add Invoice";

                invoiceVM.PatientTestDetails = patientTestDetailList;

                ViewBag.Services = new SelectList(ServiceBLL.GetAllActive(), "ServiceId", "ServiceName");

                if (invoiceVM.Invoice.InvoiceId > 0)
                {
                    ViewBag.Heading = "Edit Invoice";
                }

                if (ModelState.IsValid)
                {
                    invoiceVM.Invoice.CompanyId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.CompanyId));
                    invoiceVM.Invoice.CreatedBy = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId));
                    invoiceVM.Invoice.FYearId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.FinancialYear));
                    long patientTestId = InvoiceBLL.Save(invoiceVM);
                }
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMsg = CommonLogic.GetExceptionMessage(ex);
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Add", invoiceVM) : this.View(invoiceVM);
        }

        //public PartialViewResult BlankEditorRow()
        //{

        //    ViewBag.Services = new SelectList(ServiceBLL.GetAllActive(), "ServiceId", "ServiceName");
        //    return PartialView("ServiceRow", new InvoiceDetailModel());
        //}
        public PartialViewResult BlankEditorRow()
        {

            ViewBag.Services = new SelectList(ServiceBLL.GetAllActive(), "ServiceId", "ServiceName");
            return PartialView("LabTestRow", new InvoiceDetailModel());
        }
        public ActionResult Invoice(long? id)
        {
            long UserId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId));
            var InvoiceVM = new InvoiceViewModel(id, UserId);
            ViewBag.Heading = "View Invoice";
            if (id.HasValue && id > 0)
            {
                InvoiceVM.Invoice = InvoiceBLL.GetById(id ?? 0);
            }
            return View(InvoiceVM);
        }
        #endregion
    }
}