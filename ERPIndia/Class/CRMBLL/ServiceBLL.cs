using ERPIndia.Class.DAL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System.Collections.Generic;
namespace ERPIndia.Class.BLL
{
    public class ServiceBLL
    {
        #region Service

        public static List<ServiceModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (ServiceDAL ServiceDAL = new ServiceDAL())
            {
                return ServiceDAL.GetAll(searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        public static List<ServiceModel> GetAllActive()
        {
            using (ServiceDAL ServiceDAL = new ServiceDAL())
            {
                return ServiceDAL.GetAll("IsActive", "1", "ServiceName", "ASC", 0, 0);
            }
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="bedId">The user id.</param>
        /// <returns>Returns user by id.</returns>
        public static ServiceModel GetById(long bedId)
        {
            using (ServiceDAL ServiceDAL = new ServiceDAL())
            {
                return ServiceDAL.GetById(bedId);
            }
        }



        /// <summary>
        /// Saves the specified user.
        /// </summary>
        /// <param name="company">The user.</param>
        /// <returns>Returns user id if success else duplicate column name.</returns>
        public static ServiceModel Save(ServiceModel company)
        {
            using (ServiceDAL ServiceDAL = new ServiceDAL())
            {
                return ServiceDAL.Save(company);
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
            using (ServiceDAL ServiceDAL = new ServiceDAL())
            {
                return ServiceDAL.UpdateMultipleRecords(operationType, multiIds);
            }
        }



        #endregion
    }
}