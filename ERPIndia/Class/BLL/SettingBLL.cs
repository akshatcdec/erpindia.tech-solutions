using ERPIndia.Class.DAL;
using ERPIndia.Models.SystemSettings;
using ERPIndia.ViewModel;

namespace ERPIndia.Class.BLL
{
    /// <summary>
    /// Setting BLL class.
    /// </summary>
    public class SystemSettingBLL
    {
        #region Public Methods

        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <returns>Returns setting.</returns>
        public static SystemSettingModel GetSetting()
        {
            using (SystemSettingDAL settingDAL = new SystemSettingDAL())
            {
                return settingDAL.GetSystemSetting();
            }
        }

        /// <summary>
        /// Saves the setting.
        /// </summary>
        /// <param name="setting">The setting.</param>
        /// <returns>Returns setting id if success else error code.</returns>
        public static long SaveSetting(SystemSettingModel setting)
        {
            using (SystemSettingDAL settingDAL = new SystemSettingDAL())
            {
                return settingDAL.SaveSetting(setting);
            }
        }

        /// <summary>
        /// Gets the dashboard details.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>Returns dashboard details.</returns>
        public static DashboardViewModel GetDashboardDetails()
        {
            using (SystemSettingDAL settingDAL = new SystemSettingDAL())
            {
                return settingDAL.GetDashboardDetails();
            }
        }

        #endregion
    }
}