using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class MyInvoiceController : BaseController
    {
        // GET: ClientListing
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


            List<InvoiceModel> invoiceList = InvoiceBLL.GetAllSelfInvoice(ViewBag.SearchField, ViewBag.SearchValue, ViewBag.SortField, ViewBag.SortOrder, ViewBag.PageNo, ViewBag.PageSize);
            int totalRecords = 0;

            if (invoiceList != null && invoiceList.Count > 0)
            {
                totalRecords = invoiceList[0].TotalRecordCount;
            }

            ViewBag.ListRecords = invoiceList.Count;
            ViewBag.PagedList = invoiceList.ToStaticPagedList((int)ViewBag.PageNo, (int)ViewBag.PageSize, totalRecords);

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", invoiceList) : this.View(invoiceList);
        }
    }
}