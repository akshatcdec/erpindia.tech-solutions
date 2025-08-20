using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    /// <summary>
    /// Site menu DAL class.
    /// </summary>
    public class SiteMenuDAL : IDisposable
    {
        #region Variable Declaration

        private DBHelper databaseHelper;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="roleId">The role id.</param>
        /// <param name="isExcludeInactive">if set to <c>true</c> [is exclude inactive].</param>
        /// <returns>Returns all site menus.</returns>
        public List<SiteMenuModel> GetAll(int roleId, bool isExcludeInactive)
        {
            try
            {
                this.databaseHelper = new DBHelper();
                this.databaseHelper.SetParameterToSQLCommand("@RoleId", roleId);

                if (isExcludeInactive)
                {
                    this.databaseHelper.SetParameterToSQLCommand("@IsExcludeInactive", isExcludeInactive);
                }

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("System_spS_SiteMenuByStatus");

                return this.GetSiteMenuData(dataReader);
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
        /// Gets the site menu data.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>Returns site menu list.</returns>
        private List<SiteMenuModel> GetSiteMenuData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    SiteMenuModel menu;
                    List<SiteMenuModel> siteMenuList = new List<SiteMenuModel>();
                    bool isParentMenuIdExists = dataReader.GetOrdinal("ParentMenuId") > 0;

                    while (dataReader.Read())
                    {
                        menu = new SiteMenuModel();
                        menu.MenuId = SqlHelper.GetDBLongValue(dataReader["MenuId"]);

                        if (isParentMenuIdExists)
                        {
                            menu.ParentMenuId = SqlHelper.GetDBLongValue(dataReader["ParentMenuId"]);
                        }

                        menu.MenuCode = SqlHelper.GetDBStringValue(dataReader["MenuCode"]);
                        menu.MenuName = SqlHelper.GetDBStringValue(dataReader["MenuName"]);
                        menu.MenuPageName = SqlHelper.GetDBStringValue(dataReader["MenuPageName"]);
                        menu.MenuImageName = SqlHelper.GetDBStringValue(dataReader["MenuImageName"]);
                        menu.MenuOrderNo = SqlHelper.GetDBIntValue(dataReader["MenuOrderNo"]);
                        menu.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);

                        siteMenuList.Add(menu);
                    }

                    return siteMenuList;
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