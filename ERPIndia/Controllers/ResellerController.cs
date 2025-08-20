using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    public class ResellerController : BaseController
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
                ViewBag.SuccessMsg = string.Format(StringConstants.RecordSave, "Reseller");
            }

            ViewBag.PageNo = page ?? 1;
            ViewBag.PageSize = pageSize ?? ERPIndiaApplication.Setting.PageSize;
            ViewBag.SearchField = (string.IsNullOrEmpty(searchField) && !string.IsNullOrEmpty(searchValue)) ? "FirstName" : searchField;
            ViewBag.SearchValue = searchValue;
            ViewBag.SortField = !string.IsNullOrEmpty(sortField) ? sortField : "FirstName";
            ViewBag.SortOrder = !string.IsNullOrEmpty(sortOrder) ? sortOrder : "ASC";

            string mode = CommonLogic.GetFormDataString("mode");
            string ids = CommonLogic.GetFormDataString("ids");

            if (!string.IsNullOrEmpty(mode) && !string.IsNullOrEmpty(ids))
            {
                try
                {
                    MultiOperationType operationType = (MultiOperationType)Enum.Parse(typeof(MultiOperationType), mode, true);
                    UserBLL.UpdateMultipleRecords(operationType, ids);
                }
                catch (Exception ex)
                {
                    return this.Json(new { IsError = true, ErrorMsg = CommonLogic.GetExceptionMessage(ex) });
                }
            }

            List<ResellerModel> adminUserList = UserBLL.GetAllReseller(RoleType.Agents, ViewBag.SearchField, ViewBag.SearchValue, ViewBag.SortField, ViewBag.SortOrder, ViewBag.PageNo, ViewBag.PageSize);
            int totalRecords = 0;

            if (adminUserList != null && adminUserList.Count > 0)
            {
                totalRecords = adminUserList[0].TotalRecordCount;
            }

            ViewBag.ListRecords = adminUserList.Count;
            ViewBag.PagedList = adminUserList.ToStaticPagedList((int)ViewBag.PageNo, (int)ViewBag.PageSize, totalRecords);

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", adminUserList) : this.View(adminUserList);
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
            var user = new ResellerModel();
            ViewBag.Heading = "Add Reseller";

            string path = CommonLogic.GetConfigValue(StringConstants.AppConfig_ProfilePicFolderPath);
            string defPath = CommonLogic.GetConfigValue(StringConstants.AppConfig_DefaultProfilePic);

            if (id.HasValue && id > 0)
            {
                ViewBag.Heading = "Edit Reseller";
                user = UserBLL.GetResellerById(id ?? 0);

                if (user == null)
                {
                    user = new ResellerModel();
                }

                if (!string.IsNullOrEmpty(user.ProfilePic))
                {
                    string physicalPath = Path.Combine(Server.MapPath(path), "Reseller", Convert.ToString(user.CompanyId), user.ProfilePic);
                    if (System.IO.File.Exists(physicalPath))
                    {
                        user.ProfilePic = Path.Combine(path, "Reseller", Convert.ToString(user.CompanyId), user.ProfilePic);
                    }
                    else
                    {
                        user.ProfilePic = defPath;
                    }
                }
                else
                {
                    user.ProfilePic = defPath;
                }
            }
            else
            {
                user.ProfilePic = defPath;
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
        public ActionResult Add(long? id, ResellerModel model, HttpPostedFileBase ProfileImg)
        {
            try
            {
                ViewBag.Heading = "Add Reseller";
                string defPath = CommonLogic.GetConfigValue(StringConstants.AppConfig_DefaultProfilePic);

                if (model.UserId > 0)
                {
                    ViewBag.Heading = "Edit Reseller";
                }

                if (ModelState.IsValid)
                {
                    model.CompanyId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.CompanyId));
                    model.CreatedBy = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId));
                    model.RoleId = RoleType.Agents.GetHashCode();

                    string oldFileName = model.ProfilePic;
                    string path = Path.Combine(Server.MapPath(CommonLogic.GetConfigValue(StringConstants.AppConfig_ProfilePicFolderPath)), "Reseller", Convert.ToString(model.CompanyId));

                    if (ProfileImg != null && ProfileImg.ContentLength > 0)
                    {
                        try
                        {
                            int MaxContentLength = CommonLogic.GetConfigIntValue(StringConstants.AppConfig_ProfilePicMaxSize);
                            string[] AllowedFileExtensions = CommonLogic.GetConfigValue(StringConstants.AppConfig_ProfilePicAllowedFileType).Split(',');

                            string fileName = Path.GetFileNameWithoutExtension(ProfileImg.FileName);
                            string fileExtension = Path.GetExtension(ProfileImg.FileName);

                            if (!AllowedFileExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError("ProfilePic", string.Format(StringConstants.ValidFileTypeMsg, string.Join(", ", AllowedFileExtensions)));
                                return this.View(model);
                            }
                            else if (ProfileImg.ContentLength > MaxContentLength)
                            {
                                ModelState.AddModelError("ProfilePic", string.Format(StringConstants.ValidFileSizeMsg, MaxContentLength / 1024.0));
                                return this.View(model);
                            }
                            else
                            {
                                fileName = string.Concat(DateTime.Now.ToString("yyyyMMddHHmmss"), fileExtension);

                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }

                                path = Path.Combine(path, fileName);

                                model.ProfilePic = fileName;

                                ProfileImg.SaveAs(path);
                            }
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        model.ProfilePic = "";
                    }

                    ResellerModel returnUser = UserBLL.SaveReseller(model);

                    if (returnUser.UserId > 0)
                    {

                        // Delete old file
                        try
                        {
                            if (!string.IsNullOrEmpty(oldFileName) && ProfileImg != null && ProfileImg.ContentLength > 0)
                            {
                                if (oldFileName != CommonLogic.GetConfigValue(StringConstants.AppConfig_DefaultProfilePic))
                                {
                                    System.IO.File.Delete(Server.MapPath(oldFileName));
                                }
                            }
                        }
                        catch
                        {
                        }


                        return RedirectToAction("Index", routeValues: new { status = "s" });
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(model.ProfilePic))
                        {
                            string physicalPath = Path.Combine(Server.MapPath(path), "Reseller", Convert.ToString(model.CompanyId), model.ProfilePic);
                            if (System.IO.File.Exists(physicalPath))
                            {
                                model.ProfilePic = Path.Combine(path, "Reseller", Convert.ToString(model.CompanyId), model.ProfilePic);
                            }
                            else
                            {
                                model.ProfilePic = defPath;
                            }
                        }
                        else
                        {
                            model.ProfilePic = defPath;
                        }
                        if (returnUser.UserId == -1)
                        {
                            ViewBag.ErrorMsg = string.Format(StringConstants.RecordAlreadyExist, returnUser.DuplicateColumn);
                        }
                        else if (returnUser.UserId == 0)
                        {
                            ViewBag.ErrorMsg = string.Format(StringConstants.RecordNotExist, returnUser.DuplicateColumn);
                        }
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