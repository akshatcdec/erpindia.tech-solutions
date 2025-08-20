using ERPIndia.Class.Helper;
using ERPIndia.Models;
using ERPIndia.Models.SystemSettings;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    /// <summary>
    /// User DAL class.
    /// </summary>
    public class UserDAL : IDisposable
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
        public List<UserModel> GetAll(RoleType roleType, string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@RoleId", roleType.GetHashCode());
                this.databaseHelper.SetParameterToSQLCommand("@SearchField", searchField);
                this.databaseHelper.SetParameterToSQLCommand("@SearchValue", searchValue);
                this.databaseHelper.SetParameterToSQLCommand("@SortField", sortField);
                this.databaseHelper.SetParameterToSQLCommand("@SortOrder", sortOrder);
                this.databaseHelper.SetParameterToSQLCommand("@PageNo", pageNo);
                this.databaseHelper.SetParameterToSQLCommand("@PageSize", pageSize);

                long companyId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.CompanyId));

                if (companyId > 0)
                {
                    this.databaseHelper.SetParameterToSQLCommand("@CompanyId", CommonLogic.GetSessionValue(StringConstants.CompanyId));
                }

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_User");

                return this.GetUserData(dataReader);
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
        public List<ResellerModel> GetAllReseller(RoleType roleType, string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@RoleId", roleType.GetHashCode());
                this.databaseHelper.SetParameterToSQLCommand("@SearchField", searchField);
                this.databaseHelper.SetParameterToSQLCommand("@SearchValue", searchValue);
                this.databaseHelper.SetParameterToSQLCommand("@SortField", sortField);
                this.databaseHelper.SetParameterToSQLCommand("@SortOrder", sortOrder);
                this.databaseHelper.SetParameterToSQLCommand("@PageNo", pageNo);
                this.databaseHelper.SetParameterToSQLCommand("@PageSize", pageSize);

                long companyId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.CompanyId));

                if (companyId > 0)
                {
                    this.databaseHelper.SetParameterToSQLCommand("@CompanyId", CommonLogic.GetSessionValue(StringConstants.CompanyId));
                }

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_Reseller");

                return this.GetResellerUserData(dataReader);
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
        /// <param name="userId">The user id.</param>
        /// <returns>Returns user by id.</returns>
        public UserModel GetById(long userId, long roleId = 0)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@UserId", userId);
                this.databaseHelper.SetParameterToSQLCommand("@RoleId", roleId);

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("GetSystemUserById");

                List<UserModel> userList = this.GetUserData(dataReader);

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
        public SystemUsersModel GetSystemUserById(long userId, long roleId = 0)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@UserId", userId);
                this.databaseHelper.SetParameterToSQLCommand("@RoleId", roleId); // Always set this parameter


                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("GetSystemUserById");

                List<SystemUsersModel> userList = this.GetSystemUserData(dataReader);

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
        public ResellerModel GetResellerById(long userId, long roleId = 0)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@UserId", userId);

                if (roleId > 0)
                {
                    this.databaseHelper.SetParameterToSQLCommand("@RoleId", roleId);
                }

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_ResellerById");

                List<ResellerModel> userList = this.GetResellerUserData(dataReader);

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
        /// Gets the by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>Returns user by email.</returns>
        public UserModel GetByEmail(string email)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@Email", email);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_UserByEmail");

                List<UserModel> userList = this.GetUserData(dataReader);

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
        /// <param name="user">The user.</param>
        /// <returns>
        /// Returns user id if success else duplicate column name.
        /// </returns>
        public UserModel Save(UserModel user)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@FirstName", user.FirstName);
            this.databaseHelper.SetParameterToSQLCommand("@MiddleName", user.MiddleName);
            this.databaseHelper.SetParameterToSQLCommand("@UserName", user.UserName);
            this.databaseHelper.SetParameterToSQLCommand("@LastName", user.LastName);
            this.databaseHelper.SetParameterToSQLCommand("@Email", user.Email);
            this.databaseHelper.SetParameterToSQLCommand("@Password", user.Password);
            this.databaseHelper.SetParameterToSQLCommand("@Phone", user.Phone);
            this.databaseHelper.SetParameterToSQLCommand("@RoleId", user.RoleId);
            this.databaseHelper.SetParameterToSQLCommand("@IsActive", user.IsActive);
            this.databaseHelper.SetParameterToSQLCommand("@CreatedBy", user.CreatedBy);
            this.databaseHelper.SetParameterToSQLCommand("@CompanyId", user.CompanyId);

            if (!string.IsNullOrEmpty(user.ProfilePic))
            {
                this.databaseHelper.SetParameterToSQLCommand("@ProfilePic", user.ProfilePic);
            }


            IDataReader dataReader;
            UserModel tempUser = new UserModel();

            if (user.UserId > 0)
            {
                this.databaseHelper.SetParameterToSQLCommand("@UserId", user.UserId);
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("spU_SystemUsers");
            }
            else
            {
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("spU_SystemUsers");
            }

            if (dataReader != null)
            {
                using (dataReader)
                {
                    while (dataReader.Read())
                    {
                        tempUser.UserId = SqlHelper.GetDBLongValue(dataReader["RETURNVAL"]);
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
        public ResellerModel SaveReseller(ResellerModel user)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@FirstName", user.FirstName);
            this.databaseHelper.SetParameterToSQLCommand("@MiddleName", user.MiddleName);
            this.databaseHelper.SetParameterToSQLCommand("@LastName", user.LastName);
            this.databaseHelper.SetParameterToSQLCommand("@Email", user.Email);
            this.databaseHelper.SetParameterToSQLCommand("@Password", user.Password);
            this.databaseHelper.SetParameterToSQLCommand("@Phone", user.Phone);
            this.databaseHelper.SetParameterToSQLCommand("@RoleId", user.RoleId);
            this.databaseHelper.SetParameterToSQLCommand("@IsActive", user.IsActive);
            this.databaseHelper.SetParameterToSQLCommand("@CreatedBy", user.CreatedBy);
            this.databaseHelper.SetParameterToSQLCommand("@CompanyId", user.CompanyId);
            this.databaseHelper.SetParameterToSQLCommand("@BranchId", user.BranchId);

            if (!string.IsNullOrEmpty(user.ProfilePic))
            {
                this.databaseHelper.SetParameterToSQLCommand("@ProfilePic", user.ProfilePic);
            }


            IDataReader dataReader;
            ResellerModel tempUser = new ResellerModel();

            if (user.UserId > 0)
            {
                this.databaseHelper.SetParameterToSQLCommand("@UserId", user.UserId);
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spU_Reseller");
            }
            else
            {
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spI_Reseller");
            }

            if (dataReader != null)
            {
                using (dataReader)
                {
                    while (dataReader.Read())
                    {
                        tempUser.UserId = SqlHelper.GetDBLongValue(dataReader["RETURNVAL"]);
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

            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_User").ToString());
        }

        /// <summary>
        /// Validates the login.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns>Returns user if success otherwise user with less or equal to 0 value.</returns>
        public SystemUsersModel ValidateLogin(string userName, string password, string schoolcode)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@Email", userName);
                this.databaseHelper.SetParameterToSQLCommand("@Password", password);
                this.databaseHelper.SetParameterToSQLCommand("@SchoolCode", schoolcode);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("SystemUsersSearchLogin");

                if (dataReader != null)
                {
                    using (dataReader)
                    {
                        SystemUsersModel user = null;

                        while (dataReader.Read())
                        {
                            user = new SystemUsersModel();

                            user.SystemUserId = SqlHelper.GetDBLongValue(dataReader["SystemUserId"]);
                            user.FirstName = SqlHelper.GetDBStringValue(dataReader["FirstName"]);
                            user.MiddleName = SqlHelper.GetDBStringValue(dataReader["MiddleName"]);
                            user.LastName = SqlHelper.GetDBStringValue(dataReader["LastName"]);
                            user.Email = SqlHelper.GetDBStringValue(dataReader["Email"]);
                            user.Password = SqlHelper.GetDBStringValue(dataReader["Password"]);
                            user.Phone = SqlHelper.GetDBStringValue(dataReader["Phone"]);
                            user.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);
                            user.SystemRoleId = SqlHelper.GetDBIntValue(dataReader["SystemRoleId"]);
                            user.SystemRoleName = SqlHelper.GetDBStringValue(dataReader["SystemRoleName"]);
                            user.SchoolCode = SqlHelper.GetDBStringValue(dataReader["SchoolCode"]);
                            user.SchoolId = SqlHelper.GetDBLongValue(dataReader["SchoolId"]);
                            user.ProfilePic = SqlHelper.GetDBStringValue(dataReader["ProfilePic"]);
                            user.TenantID = SqlHelper.GetDBGuidValue(dataReader["TenantID"]);
                            user.TenantName = SqlHelper.GetDBStringValue(dataReader["TenantName"]);
                            user.TenantUserId = SqlHelper.GetDBGuidValue(dataReader["TenantUserId"]);
                            user.ActiveSessionID = SqlHelper.GetDBGuidValue(dataReader["ActiveSessionID"]);
                            user.ActiveSessionYear = SqlHelper.GetDBIntValue(dataReader["ActiveSessionYear"]);
                        }

                        return user;
                    }
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
        public UserModel ValidateLogin(string userName, string password)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@Email", userName);
                this.databaseHelper.SetParameterToSQLCommand("@Password", password);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_ValidateLogin");

                if (dataReader != null)
                {
                    using (dataReader)
                    {
                        UserModel user = null;

                        while (dataReader.Read())
                        {
                            user = new UserModel();

                            user.UserId = SqlHelper.GetDBLongValue(dataReader["UserId"]);
                            user.FirstName = SqlHelper.GetDBStringValue(dataReader["FirstName"]);
                            user.MiddleName = SqlHelper.GetDBStringValue(dataReader["MiddleName"]);
                            user.LastName = SqlHelper.GetDBStringValue(dataReader["LastName"]);
                            user.Email = SqlHelper.GetDBStringValue(dataReader["Email"]);
                            user.Password = SqlHelper.GetDBStringValue(dataReader["Password"]);
                            user.Phone = SqlHelper.GetDBStringValue(dataReader["Phone"]);
                            user.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);
                            user.RoleId = SqlHelper.GetDBIntValue(dataReader["RoleId"]);
                            user.RoleName = SqlHelper.GetDBStringValue(dataReader["RoleName"]);
                            user.CompanyId = SqlHelper.GetDBLongValue(dataReader["CompanyId"]);
                            user.ProfilePic = SqlHelper.GetDBStringValue(dataReader["ProfilePic"]);
                        }

                        return user;
                    }
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
        private List<UserModel> GetUserData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    UserModel user;
                    List<UserModel> userList = new List<UserModel>();

                    bool isDemoSite = CommonLogic.GetConfigBoolValue("IsDemoSite");

                    while (dataReader.Read())
                    {
                        user = new UserModel();

                        user.UserId = SqlHelper.GetDBLongValue(dataReader["SystemUserId"]);
                        user.RoleId = SqlHelper.GetDBIntValue(dataReader["SystemRoleId"]);

                        // For Demo site prevent to display default users
                        user.FirstName = SqlHelper.GetDBStringValue(dataReader["FirstName"]);
                        user.MiddleName = SqlHelper.GetDBStringValue(dataReader["MiddleName"]);
                        user.LastName = SqlHelper.GetDBStringValue(dataReader["LastName"]);
                        user.Email = SqlHelper.GetDBStringValue(dataReader["Email"]);
                        user.UserName = SqlHelper.GetDBStringValue(dataReader["UserName"]);
                        user.Password = SqlHelper.GetDBStringValue(dataReader["Password"]);
                        user.Phone = SqlHelper.GetDBStringValue(dataReader["Phone"]);
                        user.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);
                        user.CompanyId = SqlHelper.GetDBLongValue(dataReader["SchoolCode"]);
                        user.ProfilePic = SqlHelper.GetDBStringValue(dataReader["ProfilePic"]);

                        userList.Add(user);
                    }

                    if (userList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            userList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return userList;
                }
            }
            else
            {
                return null;
            }
        }
        private List<SystemUsersModel> GetSystemUserData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    SystemUsersModel user;
                    List<SystemUsersModel> userList = new List<SystemUsersModel>();

                    bool isDemoSite = CommonLogic.GetConfigBoolValue("IsDemoSite");

                    while (dataReader.Read())
                    {
                        user = new SystemUsersModel();

                        user.SystemUserId = SqlHelper.GetDBLongValue(dataReader["SystemUserId"]);
                        user.FirstName = SqlHelper.GetDBStringValue(dataReader["FirstName"]);
                        user.MiddleName = SqlHelper.GetDBStringValue(dataReader["MiddleName"]);
                        user.LastName = SqlHelper.GetDBStringValue(dataReader["LastName"]);
                        user.UserName = SqlHelper.GetDBStringValue(dataReader["UserName"]);
                        user.Email = SqlHelper.GetDBStringValue(dataReader["Email"]);
                        user.Password = SqlHelper.GetDBStringValue(dataReader["Password"]);
                        user.Phone = SqlHelper.GetDBStringValue(dataReader["Phone"]);
                        user.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);
                        user.SystemRoleId = SqlHelper.GetDBIntValue(dataReader["SystemRoleId"]);
                        user.SystemRoleName = SqlHelper.GetDBStringValue(dataReader["SystemRoleName"]);
                        user.SchoolCode = SqlHelper.GetDBStringValue(dataReader["SchoolCode"]);
                        user.SchoolId = SqlHelper.GetDBLongValue(dataReader["SchoolId"]);
                        user.ProfilePic = SqlHelper.GetDBStringValue(dataReader["ProfilePic"]);
                        user.TenantID = SqlHelper.GetDBGuidValue(dataReader["TenantID"]);
                        user.TenantName = SqlHelper.GetDBStringValue(dataReader["TenantName"]);
                        user.TenantUserId = SqlHelper.GetDBGuidValue(dataReader["TenantUserId"]);
                        user.ActiveSessionID = SqlHelper.GetDBGuidValue(dataReader["ActiveSessionID"]);
                        user.ActiveSessionYear = SqlHelper.GetDBIntValue(dataReader["ActiveSessionYear"]);

                        userList.Add(user);
                    }

                    if (userList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            userList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return userList;
                }
            }
            else
            {
                return null;
            }
        }
        private List<ResellerModel> GetResellerUserData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    ResellerModel user;
                    List<ResellerModel> userList = new List<ResellerModel>();

                    bool isDemoSite = CommonLogic.GetConfigBoolValue("IsDemoSite");

                    while (dataReader.Read())
                    {
                        user = new ResellerModel();

                        user.UserId = SqlHelper.GetDBLongValue(dataReader["UserId"]);
                        user.RoleId = SqlHelper.GetDBIntValue(dataReader["RoleId"]);

                        // For Demo site prevent to display default users
                        if (isDemoSite)
                        {
                            if (
                                    (user.RoleId == RoleType.Admin.GetHashCode() && user.UserId == 2)
                                || (user.RoleId == RoleType.Doctor.GetHashCode() && user.UserId == 3)
                                || (user.RoleId == RoleType.Nurse.GetHashCode() && user.UserId == 4)
                                || (user.RoleId == RoleType.Pharmacist.GetHashCode() && user.UserId == 5)
                                || (user.RoleId == RoleType.Laboratorist.GetHashCode() && user.UserId == 6)
                                || (user.RoleId == RoleType.Accountant.GetHashCode() && user.UserId == 7)

                              )
                            {
                                continue;
                            }
                        }

                        user.FirstName = SqlHelper.GetDBStringValue(dataReader["FirstName"]);
                        user.MiddleName = SqlHelper.GetDBStringValue(dataReader["MiddleName"]);
                        user.LastName = SqlHelper.GetDBStringValue(dataReader["LastName"]);
                        user.Email = SqlHelper.GetDBStringValue(dataReader["Email"]);
                        user.Password = SqlHelper.GetDBStringValue(dataReader["Password"]);
                        user.Phone = SqlHelper.GetDBStringValue(dataReader["Phone"]);
                        user.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);
                        user.CompanyId = SqlHelper.GetDBLongValue(dataReader["CompanyId"]);
                        user.ProfilePic = SqlHelper.GetDBStringValue(dataReader["ProfilePic"]);
                        user.BranchId = SqlHelper.GetDBLongValue(dataReader["BranchId"]);
                        user.BranchName = SqlHelper.GetDBStringValue(dataReader["BranchName"]);
                        userList.Add(user);
                    }

                    if (userList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            userList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return userList;
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