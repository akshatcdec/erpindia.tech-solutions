using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    /// <summary>
    /// Login history DAL class.
    /// </summary>
    public class LoginHistoryDAL : IDisposable
    {
        #region Variable Declaration

        private DBHelper databaseHelper;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the login history.
        /// </summary>
        /// <param name="searchField">The search field.</param>
        /// <param name="searchValue">The search value.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="pageNo">The page no.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>Returns login history.</returns>
        public List<SystemLoginHistoryModel> GetLoginHistory(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
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

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_LoginHistory");

                return this.GetLoginHistoryData(dataReader);
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
        /// Saves the login history.
        /// </summary>
        /// <param name="loginHistory">The login history.</param>
        /// <returns>Returns login history id if success else error code.</returns>
        public long SaveLoginHistory(SystemLoginHistoryModel loginHistory)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@UserId", loginHistory.UserId);
            this.databaseHelper.SetParameterToSQLCommand("@Action", (int)loginHistory.Action);
            this.databaseHelper.SetParameterToSQLCommand("@IpAddress", loginHistory.IPAddress);

            return SqlHelper.ParseNativeLong(this.databaseHelper.GetExecuteScalarByStoredProcedure("ISystemLoginHistory").ToString());
        }

        /// <summary>
        /// Updates the multiple records.
        /// </summary>
        /// <param name="operationType">Type of the operation.</param>
        /// <param name="multiIds">The multi ids.</param>
        /// <returns>Returns 1 if success else 0.</returns>
        public int UpdateMultipleRecords(MultiOperationType operationType, string multiIds)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@MultiIds", multiIds);
            this.databaseHelper.SetParameterToSQLCommand("@OperationType", (int)operationType);

            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_LoginHistory").ToString());
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
        /// Gets the login history data.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>Returns login history list.</returns>
        private List<SystemLoginHistoryModel> GetLoginHistoryData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    SystemLoginHistoryModel loginHistory;
                    List<SystemLoginHistoryModel> loginHistoryList = new List<SystemLoginHistoryModel>();

                    while (dataReader.Read())
                    {
                        loginHistory = new SystemLoginHistoryModel();
                        loginHistory.LoginHistoryId = SqlHelper.GetDBIntValue(dataReader["LoginHistoryId"]);
                        loginHistory.UserId = SqlHelper.GetDBIntValue(dataReader["UserId"]);
                        loginHistory.Action = (SystemLoginHistoryAction)Enum.Parse(typeof(SystemLoginHistoryAction), SqlHelper.GetDBIntValue(dataReader["Action"]).ToString());
                        loginHistory.IPAddress = SqlHelper.GetDBStringValue(dataReader["IpAddress"]);
                        loginHistory.UserName = SqlHelper.GetDBStringValue(dataReader["UserName"]);
                        loginHistory.ActionDate = SqlHelper.GetDBDateTimeValue(dataReader["ActionDate"]);

                        loginHistoryList.Add(loginHistory);
                    }

                    if (loginHistoryList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            loginHistoryList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return loginHistoryList;
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
