using ERPIndia.Class.Helper;
using ERPIndia.Models.SystemSettings;
using ERPIndia.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    /// <summary>
    /// Setting DAL class.
    /// </summary>
    public class SystemSettingDAL : IDisposable
    {
        #region Variable Declaration

        private DBHelper databaseHelper;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <returns>Returns setting.</returns>

        public SystemSettingModel GetSystemSetting()
        {
            try
            {
                this.databaseHelper = new DBHelper();

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("getSystemSettingSearch");

                List<SystemSettingModel> settingList = this.GetSystemSettingData(dataReader);

                if (settingList != null && settingList.Count > 0)
                {
                    return settingList[0];
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                this.databaseHelper.CloseConnection();
            }
        }


        /// <summary>
        /// Saves the setting.
        /// </summary>
        /// <param name="setting">The setting.</param>
        /// <returns>Returns setting id if success else error code.</returns>
        public long SaveSetting(SystemSettingModel setting)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@SiteTitle", setting.SiteTitle);
            this.databaseHelper.SetParameterToSQLCommand("@CopyrightText", setting.CopyrightText);
            this.databaseHelper.SetParameterToSQLCommand("@AdminEmail", setting.AdminEmail);
            this.databaseHelper.SetParameterToSQLCommand("@SupportEmail", setting.SupportEmail);
            this.databaseHelper.SetParameterToSQLCommand("@TollFreeNo", setting.TollFreeNo);
            this.databaseHelper.SetParameterToSQLCommand("@SMTPHost", setting.SMTPHost);
            this.databaseHelper.SetParameterToSQLCommand("@SMTPPort", setting.SMTPPort);
            this.databaseHelper.SetParameterToSQLCommand("@SMTPUserName", setting.SMTPUserName);
            this.databaseHelper.SetParameterToSQLCommand("@SMTPPassword", setting.SMTPPassword);
            this.databaseHelper.SetParameterToSQLCommand("@PageSize", setting.PageSize);
            this.databaseHelper.SetParameterToSQLCommand("@ContactNo", setting.ContactNo);
            this.databaseHelper.SetParameterToSQLCommand("@ContactEmail", setting.ContactEmail);

            return SqlHelper.ParseNativeLong(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spU_Setting").ToString());
        }

        /// <summary>
        /// Gets the dashboard details.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>Returns dashboard details.</returns>
        public DashboardViewModel GetDashboardDetails()
        {
            DashboardViewModel dashboard = null;

            try
            {
                this.databaseHelper = new DBHelper();

                long companyId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.CompanyId));
                this.databaseHelper.SetParameterToSQLCommand("@CompanyId ", companyId);

                int roleId = SqlHelper.ParseNativeInt(CommonLogic.GetSessionValue(StringConstants.RoleId));
                this.databaseHelper.SetParameterToSQLCommand("@RoleId ", roleId);

                //IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_Dashboard");

                //if (dataReader != null)
                //{
                //    using (dataReader)
                //    {
                //        while (dataReader.Read())
                //        {
                //            dashboard = new DashboardViewModel();

                //            dashboard.RoleId = roleId;

                //            //if(roleId == RoleType.SuperAdmin.GetHashCode())
                //            //{
                //            //    dashboard.CompanyCount = SqlHelper.GetDBStringValue(dataReader["CompanyCount"]);
                //            //    dashboard.AdminCount = SqlHelper.GetDBStringValue(dataReader["AdminCount"]);
                //            //}
                //            //else if (roleId == RoleType.Patient.GetHashCode())
                //            //{
                //            //}
                //            //else 
                //            //{
                //            //    dashboard.DoctorCount = SqlHelper.GetDBStringValue(dataReader["DoctorCount"]);
                //            //    dashboard.NurseCount = SqlHelper.GetDBStringValue(dataReader["NurseCount"]);
                //            //    dashboard.PharmacistCount = SqlHelper.GetDBStringValue(dataReader["PharmacistCount"]);
                //            //    dashboard.LaboratoristCount = SqlHelper.GetDBStringValue(dataReader["LaboratoristCount"]);
                //            //    dashboard.AccountantCount = SqlHelper.GetDBStringValue(dataReader["AccountantCount"]);
                //            //    dashboard.PatientCount = SqlHelper.GetDBStringValue(dataReader["PatientCount"]);
                //            //    dashboard.MedicineCount = SqlHelper.GetDBStringValue(dataReader["MedicineCount"]);
                //            //    dashboard.BedCount = SqlHelper.GetDBStringValue(dataReader["BedCount"]);
                //            //}

                //        }
                //    }
                //}
            }
            catch
            {
                throw;
            }
            finally
            {
                this.databaseHelper.CloseConnection();
            }

            return dashboard;
        }

        #endregion

        #region Dispose Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.databaseHelper.Dispose();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the setting data.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>Returns setting list.</returns>
        private List<SystemSettingModel> GetSettingData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    SystemSettingModel setting;
                    List<SystemSettingModel> settingList = new List<SystemSettingModel>();

                    while (dataReader.Read())
                    {
                        setting = new SystemSettingModel();
                        setting.SettingId = SqlHelper.GetDBIntValue(dataReader["SettingId"]);
                        setting.SiteTitle = SqlHelper.GetDBStringValue(dataReader["SiteTitle"]);
                        setting.CopyrightText = SqlHelper.GetDBStringValue(dataReader["CopyrightText"]);
                        setting.AdminEmail = SqlHelper.GetDBStringValue(dataReader["AdminEmail"]);
                        setting.SupportEmail = SqlHelper.GetDBStringValue(dataReader["SupportEmail"]);
                        setting.TollFreeNo = SqlHelper.GetDBStringValue(dataReader["TollFreeNo"]);
                        setting.SMTPHost = SqlHelper.GetDBStringValue(dataReader["SMTPHost"]);
                        setting.SMTPPort = SqlHelper.GetDBIntValue(dataReader["SMTPPort"]);
                        setting.SMTPUserName = SqlHelper.GetDBStringValue(dataReader["SMTPUserName"]);
                        setting.SMTPPassword = SqlHelper.GetDBStringValue(dataReader["SMTPPassword"]);
                        setting.PageSize = SqlHelper.GetDBIntValue(dataReader["PageSize"]);
                        setting.ContactNo = SqlHelper.GetDBStringValue(dataReader["ContactNo"]);
                        setting.ContactEmail = SqlHelper.GetDBStringValue(dataReader["ContactEmail"]);
                        setting.FYearId = SqlHelper.GetDBIntValue(dataReader["FYearId"]);
                        settingList.Add(setting);
                    }

                    return settingList;
                }
            }
            else
            {
                return null;
            }
        }
        private List<SystemSettingModel> GetSystemSettingData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    SystemSettingModel setting;
                    List<SystemSettingModel> settingList = new List<SystemSettingModel>();

                    while (dataReader.Read())
                    {
                        setting = new SystemSettingModel();
                        setting.SettingId = SqlHelper.GetDBIntValue(dataReader["SettingId"]);
                        setting.SiteTitle = SqlHelper.GetDBStringValue(dataReader["SiteTitle"]);
                        setting.CopyrightText = SqlHelper.GetDBStringValue(dataReader["CopyrightText"]);
                        setting.AdminEmail = SqlHelper.GetDBStringValue(dataReader["AdminEmail"]);
                        setting.SupportEmail = SqlHelper.GetDBStringValue(dataReader["SupportEmail"]);
                        setting.TollFreeNo = SqlHelper.GetDBStringValue(dataReader["TollFreeNo"]);
                        setting.SMTPHost = SqlHelper.GetDBStringValue(dataReader["SMTPHost"]);
                        setting.SMTPPort = SqlHelper.GetDBIntValue(dataReader["SMTPPort"]);
                        setting.SMTPUserName = SqlHelper.GetDBStringValue(dataReader["SMTPUserName"]);
                        setting.SMTPPassword = SqlHelper.GetDBStringValue(dataReader["SMTPPassword"]);
                        setting.PageSize = SqlHelper.GetDBIntValue(dataReader["PageSize"]);
                        setting.ContactNo = SqlHelper.GetDBStringValue(dataReader["ContactNo"]);
                        setting.ContactEmail = SqlHelper.GetDBStringValue(dataReader["ContactEmail"]);
                        setting.FYearId = SqlHelper.GetDBIntValue(dataReader["FYearId"]);
                        settingList.Add(setting);
                    }

                    return settingList;
                }
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}