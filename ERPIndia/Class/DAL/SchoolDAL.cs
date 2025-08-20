using ERPIndia.Class.Helper;
using ERPIndia.Models.SystemSettings;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    /// <summary>
    /// User DAL class.
    /// </summary>
    public class SchoolDAL : IDisposable
    {
        #region Variable Declaration

        private DBHelper databaseHelper;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="roleType">Type of the role.</param>
        /// <param name="searchField">The search field.</param>
        /// <param name="searchValue">The search value.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="pageNo">The page no.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>
        /// Returns all users.
        /// </returns>
        public List<SchoolModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
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

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_School");

                return this.GetSchoolData(dataReader);
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
        /// <param name="SchoolId">The user id.</param>
        /// <returns>Returns user by id.</returns>
        public SchoolModel GetById(long SchoolId)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@SchoolId", SchoolId);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("GetSchoolsById");

                List<SchoolModel> userList = this.GetSchoolData(dataReader);

                if (userList != null && userList.Count > 0)
                {
                    return userList[0];
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
        /// Saves the specified user.
        /// </summary>
        /// <param name="School">The user.</param>
        /// <returns>
        /// Returns user id if success else duplicate column name.
        /// </returns>
        public SchoolModel Save(SchoolModel School)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@SchoolName", School.SchoolName);
            this.databaseHelper.SetParameterToSQLCommand("@Address1", School.Address1);
            this.databaseHelper.SetParameterToSQLCommand("@Address2", School.Address2);
            this.databaseHelper.SetParameterToSQLCommand("@City", School.City);
            this.databaseHelper.SetParameterToSQLCommand("@State", School.State);
            this.databaseHelper.SetParameterToSQLCommand("@ZipCode", School.ZipCode);
            this.databaseHelper.SetParameterToSQLCommand("@Email", School.Email);
            this.databaseHelper.SetParameterToSQLCommand("@Phone", School.Phone);
            this.databaseHelper.SetParameterToSQLCommand("@Fax", School.Fax);
            this.databaseHelper.SetParameterToSQLCommand("@IsActive", School.IsActive);
            this.databaseHelper.SetParameterToSQLCommand("@CreatedBy", School.CreatedBy);

            IDataReader dataReader;
            SchoolModel tempUser = new SchoolModel();

            if (School.SchoolId > 0)
            {
                this.databaseHelper.SetParameterToSQLCommand("@SchoolId", School.SchoolId);
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spU_School");
            }
            else
            {
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spI_School");
            }

            if (dataReader != null)
            {
                using (dataReader)
                {
                    while (dataReader.Read())
                    {
                        tempUser.SchoolId = SqlHelper.GetDBLongValue(dataReader["RETURNVAL"]);
                        tempUser.DuplicateColumn = SqlHelper.GetDBStringValue(dataReader["DuplicateColumn"]);
                    }

                    return tempUser;
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
        /// <param name="operationType">Type of the operation.</param>
        /// <param name="multiIds">The multi ids.</param>
        /// <returns>Returns 1 if success else 0.</returns>
        public int UpdateMultipleRecords(MultiOperationType operationType, string multiIds)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@MultiIds", multiIds);
            this.databaseHelper.SetParameterToSQLCommand("@OperationType", (int)operationType);

            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_School").ToString());
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
        /// Gets the user data.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>Returns user list.</returns>
        private List<SchoolModel> GetSchoolData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    SchoolModel School;
                    List<SchoolModel> SchoolList = new List<SchoolModel>();

                    bool isDemoSite = CommonLogic.GetConfigBoolValue("IsDemoSite");

                    while (dataReader.Read())
                    {
                        School = new SchoolModel();

                        School.SchoolId = SqlHelper.GetDBLongValue(dataReader["SchoolId"]);

                        // For Demo site prevent to display default School
                        if (isDemoSite)
                        {
                            if (School.SchoolId == 1)
                                continue;
                        }
                        School.SchoolCode = SqlHelper.GetDBStringValue(dataReader["SchoolCode"]);
                        School.SchoolName = SqlHelper.GetDBStringValue(dataReader["SchoolName"]);
                        School.Address1 = SqlHelper.GetDBStringValue(dataReader["Address1"]);
                        School.Address2 = SqlHelper.GetDBStringValue(dataReader["Address2"]);
                        School.City = SqlHelper.GetDBStringValue(dataReader["City"]);
                        School.State = SqlHelper.GetDBStringValue(dataReader["State"]);
                        School.ZipCode = SqlHelper.GetDBStringValue(dataReader["ZipCode"]);
                        School.Phone = SqlHelper.GetDBStringValue(dataReader["Phone"]);
                        School.Fax = SqlHelper.GetDBStringValue(dataReader["Fax"]);
                        School.Email = SqlHelper.GetDBStringValue(dataReader["Email"]);
                        School.ActiveHeaderImg = SqlHelper.GetDBStringValue(dataReader["HeaderImg"]);
                        School.LogoImg = SqlHelper.GetDBStringValue(dataReader["LogoImg"]);
                        School.PrintTitle = SqlHelper.GetDBStringValue(dataReader["PrintTitle"]);
                        School.Line1 = SqlHelper.GetDBStringValue(dataReader["Line1"]);
                        School.Line2 = SqlHelper.GetDBStringValue(dataReader["Line2"]);
                        School.Line3 = SqlHelper.GetDBStringValue(dataReader["Line3"]);
                        School.Line4 = SqlHelper.GetDBStringValue(dataReader["Line4"]);
                        School.ActiveSessionPrint = SqlHelper.GetDBStringValue(dataReader["ActiveSessionPrint"]);
                        School.ReceiptBannerImg = SqlHelper.GetDBStringValue(dataReader["ReceiptBannerImg"]);
                        School.AdmitCardBannerImg = SqlHelper.GetDBStringValue(dataReader["AdmitCardBannerImg"]);
                        School.ReportCardBannerImg = SqlHelper.GetDBStringValue(dataReader["ReportCardBannerImg"]);
                        School.TransferCertBannerImg = SqlHelper.GetDBStringValue(dataReader["TransferCertBannerImg"]);
                        School.SalarySlipBannerImg = SqlHelper.GetDBStringValue(dataReader["SalarySlipBannerImg"]);
                        School.ICardNameBannerImg = SqlHelper.GetDBStringValue(dataReader["ICardNameBannerImg"]);
                        School.ICardAddressBannerImg = SqlHelper.GetDBStringValue(dataReader["ICardAddressBannerImg"]);
                        School.PrincipalSignImg = SqlHelper.GetDBStringValue(dataReader["PrincipalSignImg"]);
                        School.ReceiptSignImg = SqlHelper.GetDBStringValue(dataReader["ReceiptSignImg"]);
                        var val = SqlHelper.GetDBStringValue(dataReader["IsSingleFee"]);
                        School.IsSingleFee = string.IsNullOrWhiteSpace(val)? 'N' : val[0];
                        var Feeval = SqlHelper.GetDBStringValue(dataReader["EnableOnlineFee"]);
                        School.EnableOnlineFee = string.IsNullOrWhiteSpace(Feeval) ? 'N' : Feeval[0];
                        School.TopBarName = SqlHelper.GetDBStringValue(dataReader["TopBarName"]);
                        School.TopBarAddress = SqlHelper.GetDBStringValue(dataReader["TopBarAddress"]);

                        SchoolList.Add(School);
                    }

                    if (SchoolList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            SchoolList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return SchoolList;
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