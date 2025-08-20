using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    public class TaxDAL : IDisposable
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
        public List<TaxModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
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

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_Tax");

                return this.GetTaxData(dataReader);
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
        /// <param name="ClientId">The user id.</param>
        /// <returns>Returns user by id.</returns>
        public TaxModel GetById(long ClientId)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@ClientId", ClientId);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_TaxById");

                List<TaxModel> taxList = this.GetTaxData(dataReader);

                if (taxList != null && taxList.Count > 0)
                {
                    return taxList[0];
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
        /// <param name="Client">The user.</param>
        /// <returns>
        /// Returns user id if success else duplicate column name.
        /// </returns>
        public ClientModel Save(ClientModel Client)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@ClientName", Client.ClientName);
            this.databaseHelper.SetParameterToSQLCommand("@BranchId", Client.BranchId);
            this.databaseHelper.SetParameterToSQLCommand("@FYearId", Client.FYearId);
            this.databaseHelper.SetParameterToSQLCommand("@ParentId", Client.ParentId);
            this.databaseHelper.SetParameterToSQLCommand("@Address1", Client.Address1);
            this.databaseHelper.SetParameterToSQLCommand("@Address2", Client.Address2);
            this.databaseHelper.SetParameterToSQLCommand("@City", Client.City);
            this.databaseHelper.SetParameterToSQLCommand("@State", Client.State);
            this.databaseHelper.SetParameterToSQLCommand("@ZipCode", Client.ZipCode);
            this.databaseHelper.SetParameterToSQLCommand("@Email", Client.Email);
            this.databaseHelper.SetParameterToSQLCommand("@Phone", Client.Phone);
            this.databaseHelper.SetParameterToSQLCommand("@Whatsapp", Client.Whatsapp);
            this.databaseHelper.SetParameterToSQLCommand("@WebSite", Client.WebSite);
            this.databaseHelper.SetParameterToSQLCommand("@PrincipalName", Client.PrincipalName);
            this.databaseHelper.SetParameterToSQLCommand("@PrincipalNumber", Client.PrincipalNumber);
            this.databaseHelper.SetParameterToSQLCommand("@PrincipalPhoto", Client.PrincipalPhoto);
            this.databaseHelper.SetParameterToSQLCommand("@ManagerName", Client.ManagerName);
            this.databaseHelper.SetParameterToSQLCommand("@ManagerNumber", Client.ManagerNumber);
            this.databaseHelper.SetParameterToSQLCommand("@ManagerPhoto", Client.ManagerPhoto);
            this.databaseHelper.SetParameterToSQLCommand("@SchoolLogo", Client.SchoolLogo);
            this.databaseHelper.SetParameterToSQLCommand("@FacebookAccountLink", Client.FacebookAccountLink);
            this.databaseHelper.SetParameterToSQLCommand("@TwitterLink", Client.TwitterLink);
            this.databaseHelper.SetParameterToSQLCommand("@OtherLink", Client.OtherLink);
            this.databaseHelper.SetParameterToSQLCommand("@Fax", Client.Fax);
            this.databaseHelper.SetParameterToSQLCommand("@PANNo", Client.PANNo);
            this.databaseHelper.SetParameterToSQLCommand("@GSTNo", Client.GSTNo);
            this.databaseHelper.SetParameterToSQLCommand("@ClientCode", Client.ClientCode);
            this.databaseHelper.SetParameterToSQLCommand("@IsActive", Client.IsActive);
            this.databaseHelper.SetParameterToSQLCommand("@CreatedBy", Client.CreatedBy);
            this.databaseHelper.SetParameterToSQLCommand("@Password", Client.Password);


            IDataReader dataReader;
            ClientModel tempUser = new ClientModel();

            if (Client.ClientId > 0)
            {
                this.databaseHelper.SetParameterToSQLCommand("@ClientId", Client.ClientId);
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spU_Client");
            }
            else
            {
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spI_Client");
            }

            if (dataReader != null)
            {
                using (dataReader)
                {
                    while (dataReader.Read())
                    {
                        tempUser.ClientId = SqlHelper.GetDBLongValue(dataReader["RETURNVAL"]);
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

            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_Client").ToString());
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
        private List<TaxModel> GetTaxData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    TaxModel tax;
                    List<TaxModel> taxtList = new List<TaxModel>();
                    while (dataReader.Read())
                    {
                        tax = new TaxModel();
                        tax.TaxId = SqlHelper.GetDBLongValue(dataReader["TaxId"]);
                        tax.TaxName = SqlHelper.GetDBStringValue(dataReader["TaxName"]);
                        tax.TaxPercent = SqlHelper.GetDBDecimalValue(dataReader["TaxPercent"]);
                        tax.TaxSeries = SqlHelper.GetDBStringValue(dataReader["TaxSeries"]);
                        taxtList.Add(tax);
                    }

                    if (taxtList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            taxtList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return taxtList;
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