using ERPIndia.Class.DAL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System.Collections.Generic;

namespace ERPIndia.Class.BLL
{
    public class ClientBLL
    {
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
        public static List<ClientModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (ClientDAL companyDAL = new ClientDAL())
            {
                return companyDAL.GetAll(searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }
        public static List<ClientModel> GetMyClient(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize, long ParentId)
        {
            using (ClientDAL companyDAL = new ClientDAL())
            {
                return companyDAL.GetMyClient(searchField, searchValue, sortField, sortOrder, pageNo, pageSize, ParentId);
            }
        }
        public static List<ClientModel> GetAllActive()
        {
            using (ClientDAL companyDAL = new ClientDAL())
            {
                return companyDAL.GetAll("IsActive", "1", "ClientName", "ASC", 0, 0);
            }
        }
        public static List<ClientModel> GetAllMyClientActive(long parentId)
        {
            using (ClientDAL companyDAL = new ClientDAL())
            {
                return companyDAL.GetMyClient("IsActive", "1", "ClientName", "ASC", 0, 0, parentId);
            }
        }
        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="companyId">The user id.</param>
        /// <returns>Returns user by id.</returns>
        public static ClientModel GetById(long companyId)
        {
            using (ClientDAL companyDAL = new ClientDAL())
            {
                return companyDAL.GetById(companyId);
            }
        }



        /// <summary>
        /// Saves the specified user.
        /// </summary>
        /// <param name="company">The user.</param>
        /// <returns>Returns user id if success else duplicate column name.</returns>
        public static ClientModel Save(ClientModel company)
        {
            using (ClientDAL companyDAL = new ClientDAL())
            {
                return companyDAL.Save(company);
            }
        }

        /// <summary>
        /// Updates the multiple records.
        /// </summary>
        /// <param name="operationType">Type of the operation.</param>
        /// <param name="multiIds">The multi ids.</param>
        /// <returns>Returns 1 if success else 0.</returns>
        public static int UpdateMultipleRecords(MultiOperationType operationType, string multiIds)
        {
            using (ClientDAL companyDAL = new ClientDAL())
            {
                return companyDAL.UpdateMultipleRecords(operationType, multiIds);
            }
        }



        #endregion
    }
}