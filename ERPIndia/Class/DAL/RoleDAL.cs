using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    /// <summary>
    /// Role DAL class.
    /// </summary>
    public class RoleDAL : IDisposable
    {
        #region Variable Declaration

        private DBHelper databaseHelper;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="searchField">The search field.</param>
        /// <param name="searchValue">The search value.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="pageNo">The page no.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>Returns all Roles.</returns>
        public List<RoleModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@SearchField", searchField);
                this.databaseHelper.SetParameterToSQLCommand("@SearchValue", searchValue);
                this.databaseHelper.SetParameterToSQLCommand("@SortField", sortField);
                this.databaseHelper.SetParameterToSQLCommand("@SortOrder", sortOrder);
                this.databaseHelper.SetParameterToSQLCommand("@PageNo", pageNo);
                this.databaseHelper.SetParameterToSQLCommand("@PageSize", pageSize);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_Role");
                return this.GetRoleData(dataReader);
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
        /// Gets the by id.
        /// </summary>
        /// <param name="roleId">The role id.</param>
        /// <returns>Returns Role by id.</returns>
        public RoleModel GetById(long roleId)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@RoleId", roleId);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_RoleById");
                List<RoleModel> roleList = this.GetRoleData(dataReader);
                if (roleList != null && roleList.Count > 0)
                {
                    return roleList[0];
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
        /// Saves the specified Role.
        /// </summary>
        /// <param name="roleModel">The role model.</param>
        /// <returns>Returns Role model if success else duplicate column name.</returns>
        public RoleModel Save(RoleModel roleModel)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@RoleName", roleModel.RoleName);

            IDataReader dataReader;
            RoleModel tempModel = new RoleModel();

            if (roleModel.RoleId > 0)
            {
                this.databaseHelper.SetParameterToSQLCommand("@RoleId", roleModel.RoleId);
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spU_Role");
            }
            else
            {
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spI_Role");
            }

            if (dataReader != null)
            {
                using (dataReader)
                {
                    while (dataReader.Read())
                    {
                        tempModel.RoleId = SqlHelper.GetDBLongValue(dataReader["IdentityColumn"]);
                        tempModel.DuplicateColumn = SqlHelper.GetDBStringValue(dataReader["DuplicateColumn"]);
                    }

                    return tempModel;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Updates the multiple records.
        /// </summary>
        /// <param name="operationType">The operation type.</param>
        /// <param name="multiIds">The multi ids.</param>
        /// <returns>Returns 1 if success else 0.</returns>
        public int UpdateMultipleRecords(MultiOperationType operationType, string multiIds)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@OperationType", operationType);
            this.databaseHelper.SetParameterToSQLCommand("@MultiIds", multiIds);
            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_Role").ToString());
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
        /// Gets the Role data..
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>Returns Role list.</returns>
        public List<RoleModel> GetRoleData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    RoleModel roleModel;
                    List<RoleModel> roleList = new List<RoleModel>();
                    while (dataReader.Read())
                    {
                        roleModel = new RoleModel();
                        roleModel.RoleId = SqlHelper.GetDBLongValue(dataReader["RoleId"]);
                        roleModel.RoleName = SqlHelper.GetDBStringValue(dataReader["RoleName"]);
                        roleList.Add(roleModel);
                    }
                    if (roleList.Count > 0)
                    {
                        dataReader.NextResult();
                        while (dataReader.Read())
                        {
                            roleList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"], 0);
                        }
                    }
                    return roleList;
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
