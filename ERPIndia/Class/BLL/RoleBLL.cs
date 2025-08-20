using ERPIndia.Class.DAL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System.Collections.Generic;

namespace ERPIndia.Class.BLL
{
    /// <summary>
    /// Role BLL class.
    /// </summary>
    public class RoleBLL
    {
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
        public static List<RoleModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (RoleDAL roleDAL = new RoleDAL())
            {
                return roleDAL.GetAll(searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="roleId">The role id.</param>
        /// <returns>Returns Role by id.</returns>
        public static RoleModel GetById(long roleId)
        {
            using (RoleDAL roleDAL = new RoleDAL())
            {
                return roleDAL.GetById(roleId);
            }
        }

        /// <summary>
        /// Saves the specified Role.
        /// </summary>
        /// <param name="roleModel">The role model.</param>
        /// <returns>Returns Role model if success else duplicate column name.</returns>
        public static RoleModel Save(RoleModel roleModel)
        {
            using (RoleDAL roleDAL = new RoleDAL())
            {
                return roleDAL.Save(roleModel);
            }
        }

        /// <summary>
        /// Updates the multiple records.
        /// </summary>
        /// <param name="operationType">The operation type.</param>
        /// <param name="multiIds">The multi ids.</param>
        /// <returns>Returns 1 if success else 0.</returns>
        public static int UpdateMultipleRecords(MultiOperationType operationType, string multiIds)
        {
            using (RoleDAL roleDAL = new RoleDAL())
            {
                return roleDAL.UpdateMultipleRecords(operationType, multiIds);
            }
        }

        #endregion

    }
}
