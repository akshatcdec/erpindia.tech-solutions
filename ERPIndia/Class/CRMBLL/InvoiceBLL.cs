using ERPIndia.Class.DAL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using ERPIndia.ViewModel;
using System.Collections.Generic;

namespace ERPIndia.Class.BLL
{
    public class InvoiceBLL
    {
        #region Invoice

        public static List<InvoiceModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (InvoiceDAL InvoiceDAL = new InvoiceDAL())
            {
                return InvoiceDAL.GetAll(searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }
        public static List<InvoiceModel> GetAllSelfInvoice(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (InvoiceDAL InvoiceDAL = new InvoiceDAL())
            {
                return InvoiceDAL.GetAllSelfInvoice(searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        public static List<InvoiceModel> GetAllActive()
        {
            using (InvoiceDAL InvoiceDAL = new InvoiceDAL())
            {
                return InvoiceDAL.GetAll("IsActive", "1", "PatientTestDate", "ASC", 0, 0);
            }
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="patientTestId">The user id.</param>
        /// <returns>Returns user by id.</returns>
        public static InvoiceModel GetById(long patientTestId)
        {
            using (InvoiceDAL InvoiceDAL = new InvoiceDAL())
            {
                return InvoiceDAL.GetById(patientTestId);
            }
        }



        /// <summary>
        /// Saves the specified user.
        /// </summary>
        /// <param name="company">The user.</param>
        /// <returns>Returns user id if success else duplicate column name.</returns>
        public static long Save(InvoiceViewModel invoiceVM)
        {
            using (InvoiceDAL invoiceDAL = new InvoiceDAL())
            {

                long invoiceId = invoiceDAL.Save(invoiceVM.Invoice);
                SaveInvoiceServiceDetail(invoiceId, invoiceVM.PatientTestDetails);
                return invoiceId;
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
            using (InvoiceDAL InvoiceDAL = new InvoiceDAL())
            {
                return InvoiceDAL.UpdateMultipleRecords(operationType, multiIds);
            }
        }

        public static int UpdatePatientTestStatus(PatientTestStatusType patientTestStatusType, string multiIds)
        {
            using (InvoiceDAL InvoiceDAL = new InvoiceDAL())
            {
                return InvoiceDAL.UpdatePatientTestStatus(patientTestStatusType, multiIds);
            }
        }



        #endregion

        #region InvoiceServiceDetail 

        public static List<InvoiceDetailModel> GetInvoiceDetailByInvoiceId(long patientTestId)
        {
            using (InvoiceDAL InvoiceDAL = new InvoiceDAL())
            {
                return InvoiceDAL.GetInvoiceDetailByInvoiceId(patientTestId);
            }
        }

        public static void SaveInvoiceServiceDetail(long patientTestId, List<InvoiceDetailModel> patientTestDetailList)
        {
            string testData = string.Empty;

            if (patientTestDetailList != null)
            {
                foreach (var test in patientTestDetailList)
                {
                    testData = string.Concat(testData, test.ServiceId, ",", test.Amount, ",", test.Remarks, "#");
                }
            }

            testData = testData.TrimEnd('#');

            using (InvoiceDAL InvoiceDAL = new InvoiceDAL())
            {
                InvoiceDAL.SaveServiceDetail(patientTestId, testData);
            }


        }

        #endregion
    }
}