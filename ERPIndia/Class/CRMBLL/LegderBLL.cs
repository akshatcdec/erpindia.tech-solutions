using ERPIndia.Class.DAL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using ERPIndia.ViewModel;
using System.Collections.Generic;

namespace ERPIndia.Class.BLL
{
    public class LedgerBLL
    {
        #region Ledger

        public static List<LedgerModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (LedgerDAL LedgerDAL = new LedgerDAL())
            {
                return LedgerDAL.GetAll(searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }
        public static List<LedgerModel> GetAllPayment(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize, long ParentId)
        {
            using (LedgerDAL LedgerDAL = new LedgerDAL())
            {
                return LedgerDAL.GetAllPayment(searchField, searchValue, sortField, sortOrder, pageNo, pageSize, ParentId);
            }
        }
        public static List<LedgerModel> GetLedgerByClientId(long AccountId)
        {
            using (LedgerDAL LedgerDAL = new LedgerDAL())
            {
                return LedgerDAL.GetLedgerByClientId(AccountId);
            }
        }
        public static List<LedgerBookModel> GetLedgerBookByClientId(long AccountId)
        {
            using (LedgerDAL LedgerDAL = new LedgerDAL())
            {
                return LedgerDAL.GetLedgerBookByClientId(AccountId);
            }
        }
        public static List<LedgerModel> GetAllActive()
        {
            using (LedgerDAL LedgerDAL = new LedgerDAL())
            {
                return LedgerDAL.GetAll("IsActive", "1", "LedgerName", "ASC", 0, 0);
            }
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="bedId">The user id.</param>
        /// <returns>Returns user by id.</returns>
        public static LedgerModel GetById(long bedId)
        {
            using (LedgerDAL LedgerDAL = new LedgerDAL())
            {
                return LedgerDAL.GetById(bedId);
            }
        }



        /// <summary>
        /// Saves the specified user.
        /// </summary>
        /// <param name="company">The user.</param>
        /// <returns>Returns user id if success else duplicate column name.</returns>
        public static LedgerModel Save(LedgerModel company)
        {
            using (LedgerDAL LedgerDAL = new LedgerDAL())
            {
                return LedgerDAL.Save(company);
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
            using (LedgerDAL LedgerDAL = new LedgerDAL())
            {
                return LedgerDAL.UpdateMultipleRecords(operationType, multiIds);
            }
        }



        #endregion
    }
}
