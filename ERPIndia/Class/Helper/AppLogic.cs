using System;
using System.IO;
using System.Web;
using CrystalDecisions.Web;
using CrystalDecisions.Shared;
namespace ERPIndia.Class.Helper
{
    public class AppLogic
    {

        public static string GetProfilePicture()
        {

            string defPath = CommonLogic.GetConfigValue(StringConstants.AppConfig_DefaultProfilePic);
            string picFolder = AppLogic.GetProfilePicFolder();
            string profilePic = CommonLogic.GetSessionValue(StringConstants.ProfilePic);
            string path = Path.Combine(CommonLogic.GetConfigValue(StringConstants.AppConfig_ProfilePicFolderPath), picFolder, CommonLogic.GetSessionValue(StringConstants.TenantCode));

            if (!string.IsNullOrEmpty(profilePic))
            {
                string physicalPath = Path.Combine(HttpContext.Current.Server.MapPath(path), profilePic);
                if (System.IO.File.Exists(physicalPath))
                {
                    return Path.Combine(path, profilePic);
                }
            }
            return defPath;
        }
        public static string GetHeaderImage(string tenantCode, string headerCardImg)
        {
            string defaultLogoPath = "/template/assets/img/logo.png";

            if (!string.IsNullOrEmpty(headerCardImg))
            {
                string headerImagePath = string.Format("/Documents/{0}/SchoolProfile/", tenantCode) + headerCardImg;
                string physicalPath = HttpContext.Current.Server.MapPath(headerImagePath);

                if (System.IO.File.Exists(physicalPath))
                {
                    return headerImagePath;
                }
            }

            return defaultLogoPath;
        }
        public static string GetLogoImage(string tenantCode, string idimg)
        {
            string defaultLogoPath = "/template/assets/img/id.jpg";

            if (!string.IsNullOrEmpty(idimg))
            {
                string headerImagePath = string.Format("/Documents/{0}/SchoolProfile/", tenantCode) + idimg;
                string physicalPath = HttpContext.Current.Server.MapPath(headerImagePath);

                if (System.IO.File.Exists(physicalPath))
                {
                    return headerImagePath;
                }
            }

            return defaultLogoPath;
        }
        public static string GetStudentImagePath(string tenantCode, string idimg)
        {
            string defaultLogoPath = "/img/default.jpg";

            if (!string.IsNullOrEmpty(idimg))
            {
                string physicalPath = HttpContext.Current.Server.MapPath(idimg);

                if (System.IO.File.Exists(physicalPath))
                {
                    return idimg;
                }
            }

            return defaultLogoPath;
        }

        public static string GetStudentImage(string path)
        {
            string defaultLogoPath = "/template/assets/img/noimgstu.png";

            if (!string.IsNullOrEmpty(path))
            {
                string physicalPath = HttpContext.Current.Server.MapPath(path);

                if (System.IO.File.Exists(physicalPath))
                {
                    return path;
                }
            }

            return defaultLogoPath;
        }
        public static string GetProfilePicFolder()
        {
            string id = CommonLogic.GetSessionValue(StringConstants.RoleId);
           
                    return string.Empty;
            
        }
    }
}