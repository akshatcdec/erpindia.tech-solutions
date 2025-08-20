using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    public class ClientDAL : IDisposable
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
        public List<ClientModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
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

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_Client");

                return this.GetClientData(dataReader);
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
        public List<ClientModel> GetMyClient(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize, long ParentId)
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
                this.databaseHelper.SetParameterToSQLCommand("@ParentId", ParentId);

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_MyClient");

                return this.GetClientData(dataReader);
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
        public ClientModel GetById(long ClientId)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@ClientId", ClientId);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_ClientById");

                List<ClientModel> userList = this.GetClientData(dataReader);

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
            this.databaseHelper.SetParameterToSQLCommand("@SMSId", Client.SMSId);
            this.databaseHelper.SetParameterToSQLCommand("@SMSPassword", Client.SMSPassword);
            this.databaseHelper.SetParameterToSQLCommand("@SMSLoginCode", Client.SMSLoginCode);
            this.databaseHelper.SetParameterToSQLCommand("@AMC", Client.AMC);
            this.databaseHelper.SetParameterToSQLCommand("@StudentLimit", Client.StudentLimit);
            this.databaseHelper.SetParameterToSQLCommand("@LoginLink", Client.LoginLink);
            this.databaseHelper.SetParameterToSQLCommand("@Username", Client.Username);
            this.databaseHelper.SetParameterToSQLCommand("@Sesssion", Client.Sesssion);
            this.databaseHelper.SetParameterToSQLCommand("@Increment", Client.Increment);
            this.databaseHelper.SetParameterToSQLCommand("@CustomerId", Client.CustomerId);
            this.databaseHelper.SetParameterToSQLCommand("@AgentName", Client.AgentName);
            this.databaseHelper.SetParameterToSQLCommand("@OldBalance", Client.OldBalance);
            this.databaseHelper.SetParameterToSQLCommand("@Note1", Client.Note1);
            this.databaseHelper.SetParameterToSQLCommand("@Note2", Client.Note2);

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
        private List<ClientModel> GetClientData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    ClientModel Client;
                    List<ClientModel> ClientList = new List<ClientModel>();

                    bool isDemoSite = CommonLogic.GetConfigBoolValue("IsDemoSite");

                    while (dataReader.Read())
                    {
                        Client = new ClientModel();

                        Client.ClientId = SqlHelper.GetDBLongValue(dataReader["ClientId"]);

                        // For Demo site prevent to display default Client
                        //if (isDemoSite)
                        //{
                        //    if (Client.ClientId == 1)
                        //        continue;
                        //}

                        Client.ClientName = SqlHelper.GetDBStringValue(dataReader["ClientName"]);
                        Client.FYearId = SqlHelper.GetDBLongValue(dataReader["FYearId"]);
                        Client.BranchId = SqlHelper.GetDBLongValue(dataReader["BranchId"]);
                        Client.BranchName = SqlHelper.GetDBStringValue(dataReader["BranchName"]);
                        Client.Address1 = SqlHelper.GetDBStringValue(dataReader["Address1"]);
                        Client.Address2 = SqlHelper.GetDBStringValue(dataReader["Address2"]);
                        Client.City = SqlHelper.GetDBStringValue(dataReader["City"]);
                        Client.State = SqlHelper.GetDBStringValue(dataReader["State"]);
                        Client.ZipCode = SqlHelper.GetDBStringValue(dataReader["ZipCode"]);
                        Client.Phone = SqlHelper.GetDBStringValue(dataReader["Phone"]);
                        Client.Fax = SqlHelper.GetDBStringValue(dataReader["Fax"]);
                        Client.Email = SqlHelper.GetDBStringValue(dataReader["Email"]);
                        Client.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);
                        Client.Whatsapp = SqlHelper.GetDBStringValue(dataReader["Whatsapp"]);
                        Client.WebSite = SqlHelper.GetDBStringValue(dataReader["WebSite"]);
                        Client.PrincipalName = SqlHelper.GetDBStringValue(dataReader["PrincipalName"]);
                        Client.PrincipalNumber = SqlHelper.GetDBStringValue(dataReader["PrincipalNumber"]);
                        Client.PrincipalPhoto = SqlHelper.GetDBStringValue(dataReader["PrincipalPhoto"]);
                        Client.ManagerName = SqlHelper.GetDBStringValue(dataReader["ManagerName"]);
                        Client.ManagerNumber = SqlHelper.GetDBStringValue(dataReader["ManagerNumber"]);
                        Client.ManagerPhoto = SqlHelper.GetDBStringValue(dataReader["ManagerPhoto"]);
                        Client.SchoolLogo = SqlHelper.GetDBStringValue(dataReader["SchoolLogo"]);
                        Client.FacebookAccountLink = SqlHelper.GetDBStringValue(dataReader["FacebookAccountLink"]);
                        Client.TwitterLink = SqlHelper.GetDBStringValue(dataReader["TwitterLink"]);
                        Client.OtherLink = SqlHelper.GetDBStringValue(dataReader["OtherLink"]);
                        Client.PANNo = SqlHelper.GetDBStringValue(dataReader["PANNo"]);
                        Client.GSTNo = SqlHelper.GetDBStringValue(dataReader["GSTNo"]);
                        Client.ClientCode = SqlHelper.GetDBStringValue(dataReader["ClientCode"]);
                        Client.Password = SqlHelper.GetDBStringValue(dataReader["Password"]);

                        Client.SMSId = SqlHelper.GetDBStringValue(dataReader["SMSId"]);
                        Client.SMSPassword = SqlHelper.GetDBStringValue(dataReader["SMSPassword"]);
                        Client.SMSLoginCode = SqlHelper.GetDBStringValue(dataReader["SMSLoginCode"]);
                        Client.AMC = SqlHelper.GetDBLongValue(dataReader["AMC"]);
                        Client.StudentLimit = SqlHelper.GetDBLongValue(dataReader["StudentLimit"]);
                        Client.LoginLink = SqlHelper.GetDBStringValue(dataReader["LoginLink"]);
                        Client.Username = SqlHelper.GetDBStringValue(dataReader["Username"]);
                        Client.Sesssion = SqlHelper.GetDBStringValue(dataReader["Sesssion"]);
                        Client.Increment = SqlHelper.GetDBLongValue(dataReader["Increment"]);
                        Client.CustomerId = SqlHelper.GetDBLongValue(dataReader["CustomerId"]);
                        Client.AgentName = SqlHelper.GetDBStringValue(dataReader["AgentName"]);
                        Client.OldBalance = SqlHelper.GetDBLongValue(dataReader["OldBalance"]);
                        Client.Note1 = SqlHelper.GetDBStringValue(dataReader["Note1"]);
                        Client.Note2 = SqlHelper.GetDBStringValue(dataReader["Note2"]);

                        ClientList.Add(Client);
                    }

                    if (ClientList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            ClientList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return ClientList;
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