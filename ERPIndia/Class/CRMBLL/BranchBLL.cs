using ERPIndia.Class.DAL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System.Collections.Generic;
namespace ERPIndia.Class.BLL
{
    public class BranchBLL
    {
        #region Branch

        public static List<BranchModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (BranchDAL BranchDAL = new BranchDAL())
            {
                return BranchDAL.GetAll(searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        public static List<BranchModel> GetAllActive()
        {
            using (BranchDAL BranchDAL = new BranchDAL())
            {
                return BranchDAL.GetAll("IsActive", "1", "BranchName", "ASC", 0, 0);
            }
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="bedId">The user id.</param>
        /// <returns>Returns user by id.</returns>
        public static BranchModel GetById(long bedId)
        {
            using (BranchDAL BranchDAL = new BranchDAL())
            {
                return BranchDAL.GetById(bedId);
            }
        }



        /// <summary>
        /// Saves the specified user.
        /// </summary>
        /// <param name="company">The user.</param>
        /// <returns>Returns user id if success else duplicate column name.</returns>
        public static BranchModel Save(BranchModel company)
        {
            using (BranchDAL BranchDAL = new BranchDAL())
            {
                return BranchDAL.Save(company);
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
            using (BranchDAL BranchDAL = new BranchDAL())
            {
                return BranchDAL.UpdateMultipleRecords(operationType, multiIds);
            }
        }



        #endregion
    }
}
