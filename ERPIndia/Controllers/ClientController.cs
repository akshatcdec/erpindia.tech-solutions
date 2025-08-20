using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using ERPIndia.ViewModel;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{

    public class ClientController : BaseController
    {
        #region Action Methods

        /// <summary>
        /// Add view action..
        /// </summary>
        /// <param name="id">The The identifier..</param>
        /// <returns>Returns add action result.</returns>
        public ActionResult Index(int? page, int? pageSize, string searchField, string searchValue, string sortField, string sortOrder)
        {
            long UserId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId));
            if (CommonLogic.GetQueryString("status").Equals("s", StringComparison.CurrentCultureIgnoreCase))
            {
                ViewBag.SuccessMsg = string.Format(StringConstants.RecordSave, "Client");
            }

            ViewBag.PageNo = page ?? 1;
            ViewBag.PageSize = pageSize ?? ERPIndiaApplication.Setting.PageSize;
            ViewBag.SearchField = (string.IsNullOrEmpty(searchField) && !string.IsNullOrEmpty(searchValue)) ? "ClientName" : searchField;
            ViewBag.SearchValue = searchValue;
            ViewBag.SortField = !string.IsNullOrEmpty(sortField) ? sortField : "ClientName";
            ViewBag.SortOrder = !string.IsNullOrEmpty(sortOrder) ? sortOrder : "ASC";

            string mode = CommonLogic.GetFormDataString("mode");
            string ids = CommonLogic.GetFormDataString("ids");

            if (!string.IsNullOrEmpty(mode) && !string.IsNullOrEmpty(ids))
            {
                try
                {
                    MultiOperationType operationType = (MultiOperationType)Enum.Parse(typeof(MultiOperationType), mode, true);
                    ClientBLL.UpdateMultipleRecords(operationType, ids);
                }
                catch (Exception ex)
                {
                    return this.Json(new { IsError = true, ErrorMsg = CommonLogic.GetExceptionMessage(ex) });
                }
            }

            List<ClientModel> ClientList = ClientBLL.GetMyClient(ViewBag.SearchField, ViewBag.SearchValue, ViewBag.SortField, ViewBag.SortOrder, ViewBag.PageNo, ViewBag.PageSize, UserId);
            int totalRecords = 0;

            if (ClientList != null && ClientList.Count > 0)
            {
                totalRecords = ClientList[0].TotalRecordCount;
            }

            ViewBag.ListRecords = ClientList.Count;
            ViewBag.PagedList = ClientList.ToStaticPagedList((int)ViewBag.PageNo, (int)ViewBag.PageSize, totalRecords);

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", ClientList) : this.View(ClientList);
        }

        /// <summary>
        /// Add view action..
        /// </summary>
        /// <param name="id">The The identifier..</param>
        /// <returns>Returns add action result.</returns>
        public ActionResult Add(long? id)
        {
            ClientViewModel ClientModel = new ClientViewModel();
            ViewBag.Heading = "Add Client";

            if (id.HasValue && id > 0)
            {
                ViewBag.Heading = "Edit Client";
                ClientModel.ClientModel = ClientBLL.GetById(id ?? 0);

                if (ClientModel == null)
                {
                    ClientModel = new ClientViewModel();
                }
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Add", ClientModel) : this.View(ClientModel);
        }
        public ActionResult Details(long? id)
        {
            ClientDetailsModel ClientModel = new ClientDetailsModel();
            ViewBag.Heading = "View Client";

            if (id.HasValue && id > 0)
            {
                ClientModel.ClientModel = ClientBLL.GetById(id ?? 0);
                ClientModel.LedgerBook = LedgerBLL.GetLedgerBookByClientId(id ?? 0);
                ClientModel.Ledgers = LedgerBLL.GetLedgerByClientId(id ?? 0);
                if (ClientModel == null)
                {
                    ClientModel = new ClientDetailsModel();
                }
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Details", ClientModel) : this.View(ClientModel);
        }
        /// <summary>
        /// Add view post action..
        /// </summary>
        /// <param name="id">The The identifier..</param>
        /// <param name="model">The The model..</param>
        /// <returns>Returns add action result.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(long? id, ClientViewModel model)
        {
            try
            {
                ViewBag.Heading = "Add Client";

                if (model.ClientModel.ClientId > 0)
                {
                    ViewBag.Heading = "Edit Client";
                }

                if (ModelState.IsValid)
                {
                    model.ClientModel.FYearId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.FinancialYear));
                    model.ClientModel.ParentId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId));
                    ClientModel returnClient = ClientBLL.Save(model.ClientModel);
                    //UserModel user = new UserModel();
                    //user.FirstName = model.ClientModel.ClientName;
                    //user.LastName = "-";
                    //user.Email = model.ClientModel.Email;
                    //user.Password = "123456";
                    //user.Phone= model.ClientModel.Phone;
                    //user.RoleId=RoleType.Client.GetHashCode();
                    //UserModel returnUser = UserBLL.Save(user);

                    if (returnClient.ClientId == -1)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordAlreadyExist, returnClient.DuplicateColumn);
                    }
                    else if (returnClient.ClientId == 0)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordNotExist, returnClient.DuplicateColumn);
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
