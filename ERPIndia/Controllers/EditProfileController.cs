using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    /// <summary>
    /// Edit profile controller class.
    /// </summary>
    [LogOnAuthorize]
    public class EditProfileController : BaseController
    {
        #region Action Methods

        /// <summary>
        /// Index view action.
        /// </summary>
        /// <returns>
        /// Returns index action result.
        /// </returns>
        /// 
        public ActionResult Demo()
        {
            return View();
        }
        public ActionResult Index()
        {
            UserModel user = UserBLL.GetById(SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId)));

            string path = CommonLogic.GetConfigValue(StringConstants.AppConfig_ProfilePicFolderPath);
            string defPath = CommonLogic.GetConfigValue(StringConstants.AppConfig_DefaultProfilePic);
            string picFolder = AppLogic.GetProfilePicFolder();
            string tenantcode = CommonLogic.GetSessionValue(StringConstants.TenantCode);
            if (user == null)
            {
                user = new UserModel();
            }

            if (!string.IsNullOrEmpty(user.ProfilePic))
            {
                string physicalPath = Path.Combine(Server.MapPath(path), picFolder, tenantcode, user.ProfilePic);
                if (System.IO.File.Exists(physicalPath))
                {
                    user.ProfilePic = Path.Combine(path, picFolder, tenantcode, user.ProfilePic);
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
            if (user != null)
            {
                if (user.TransportMonths == null)
                {
                    user.TransportMonths = user.GetDefaultMonths();
                }

                // Get saved transport months for this user
                //var transportMonths = _transportService.GetTransportMonthsByUser(userId);
                //if (transportMonths != null && transportMonths.Any())
                //{
                //    user.SetTransportMonths(transportMonths);
                //}
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", user) : this.View(user);
        }

        /// <summary>
        /// Index view post action.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Returns index action result.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(UserModel model, HttpPostedFileBase ProfileImg)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    model.UserId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId));
                    model.RoleId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.RoleId));
                    model.CompanyId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.CompanyId));
                    model.IsActive = true;
                    string tenantcode = CommonLogic.GetSessionValue(StringConstants.TenantCode);

                    string picFolder = AppLogic.GetProfilePicFolder();
                    string defPath = CommonLogic.GetConfigValue(StringConstants.AppConfig_DefaultProfilePic);
                    string oldFileName = model.ProfilePic;
                    string path = Path.Combine(Server.MapPath(CommonLogic.GetConfigValue(StringConstants.AppConfig_ProfilePicFolderPath)), picFolder, tenantcode);

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

                    UserModel returnUser = UserBLL.Save(model);

                    if (returnUser.UserId > 0)
                    {
                        ViewBag.SuccessMsg = string.Format(StringConstants.RecordSave, "Profile");
                        TempData["SuccessMessage"] = "Saved successfully!";
                        // Delete old file
                        try
                        {
                            if (!string.IsNullOrEmpty(oldFileName) && ProfileImg != null && ProfileImg.ContentLength > 0)
                            {
                                CommonLogic.SetSessionValue(StringConstants.ProfilePic, model.ProfilePic);
                                if (oldFileName != CommonLogic.GetConfigValue(StringConstants.AppConfig_DefaultProfilePic))
                                {
                                    System.IO.File.Delete(Server.MapPath(oldFileName));
                                }
                            }
                        }
                        catch
                        {
                        }



                        return RedirectToAction("Index");
                    }
                    else if (returnUser.UserId == -1)
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordAlreadyExist, returnUser.DuplicateColumn);
                    }
                    else
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordNotExist, returnUser.DuplicateColumn);
                    }

                    if (!string.IsNullOrEmpty(model.ProfilePic))
                    {
                        if (System.IO.File.Exists(path))
                        {
                            model.ProfilePic = Path.Combine(CommonLogic.GetConfigValue(StringConstants.AppConfig_ProfilePicFolderPath), picFolder, Convert.ToString(model.CompanyId), model.ProfilePic);
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
                }
                else
                {
                    var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

                    // Add all errors to ViewBag for display
                    ViewBag.ValidationErrors = errors;

                    // You can also add a generic error message
                    ViewBag.ErrorMsg = "Please correct the errors and try again.";
                }
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMsg = CommonLogic.GetExceptionMessage(ex);
            }



            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", model) : this.View(model);
        }



        #endregion
    }
}
