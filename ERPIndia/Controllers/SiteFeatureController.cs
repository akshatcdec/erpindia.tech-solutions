using ERPIndia.Class.BLL;
using ERPIndia.Class.Helper;
using ERPIndia.Models.SystemSettings;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    /// <summary>
    /// Site Feature controller class.
    /// </summary>
    [LogOnAuthorize(Roles = "1")]
    public class SiteFeaturesController : BaseController
    {
        /// <summary>
        /// Index view action.
        /// </summary>
        /// <returns>
        /// Returns index action result.
        /// </returns>
        public ActionResult Index()
        {
            SystemSettingModel settingModel = SystemSettingBLL.GetSetting();

            if (settingModel == null)
            {
                settingModel = new SystemSettingModel();
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", settingModel) : this.View(settingModel);
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
        public ActionResult Index(SystemSettingModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    long settingId = SystemSettingBLL.SaveSetting(model);

                    if (settingId > 0)
                    {
                        ERPIndiaApplication.SetGlobalVariable();
                        ViewBag.SuccessMsg = string.Format(StringConstants.RecordSave, "Site Features");
                    }
                    else
                    {
                        ViewBag.ErrorMsg = string.Format(StringConstants.RecordSaveError, "Site Features");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ViewBag.ErrorMsg = CommonLogic.GetExceptionMessage(ex);
            }

            return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", model) : this.View(model);
        }
    }
}
