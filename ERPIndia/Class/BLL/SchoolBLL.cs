using ERPIndia.Class.DAL;
using ERPIndia.Class.Helper;
using ERPIndia.Models.SystemSettings;
using System.Collections.Generic;

namespace ERPIndia.Class.BLL
{
    /// <summary>
    /// User BLL class.
    /// </summary>
    public class SchoolBLL
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
        public static List<SchoolModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (SchoolDAL SchoolDAL = new SchoolDAL())
            {
                return SchoolDAL.GetAll(searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        public static List<SchoolModel> GetAllActive()
        {
            using (SchoolDAL SchoolDAL = new SchoolDAL())
            {
                return SchoolDAL.GetAll("IsActive", "1", "SchoolName", "ASC", 0, 0);
            }
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="SchoolId">The user id.</param>
        /// <returns>Returns user by id.</returns>
        public static SchoolModel GetById(long SchoolId)
        {
            using (SchoolDAL SchoolDAL = new SchoolDAL())
            {
                return SchoolDAL.GetById(SchoolId);
            }
        }



        /// <summary>
        /// Saves the specified user.
        /// </summary>
        /// <param name="School">The user.</param>
        /// <returns>Returns user id if success else duplicate column name.</returns>
        public static SchoolModel Save(SchoolModel School)
        {
            using (SchoolDAL SchoolDAL = new SchoolDAL())
            {
                return SchoolDAL.Save(School);
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
            using (SchoolDAL SchoolDAL = new SchoolDAL())
            {
                return SchoolDAL.UpdateMultipleRecords(operationType, multiIds);
            }
        }



        #endregion
    }
}