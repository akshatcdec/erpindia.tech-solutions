using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    public class InvoiceDAL : IDisposable
    {
        #region Variable Declaration

        private DBHelper databaseHelper;

        #endregion

        #region Invoice

        #region Public Methods
        public List<InvoiceModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
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

                long companyId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.CompanyId));
                this.databaseHelper.SetParameterToSQLCommand("@CompanyId ", companyId);

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_Invoice");

                return this.GetInvoiceData(dataReader);
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
        public List<InvoiceModel> GetAllSelfInvoice(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
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

                string EmailId = SqlHelper.GetDBStringValue(CommonLogic.GetSessionValue(StringConstants.UserName));
                this.databaseHelper.SetParameterToSQLCommand("@EMail ", EmailId);

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_SelfInvoice");

                return this.GetInvoiceData(dataReader);
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

        public InvoiceModel GetById(long InvoiceId)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@InvoiceId", InvoiceId);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_InvoiceById");

                List<InvoiceModel> list = this.GetInvoiceData(dataReader);

                if (list != null && list.Count > 0)
                {
                    return list[0];
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

        public long Save(InvoiceModel invoice)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@ClientId", invoice.ClientId);
            this.databaseHelper.SetParameterToSQLCommand("@InvoiceDate", invoice.InvoiceDate);
            this.databaseHelper.SetParameterToSQLCommand("@DueDate", invoice.DueDate);
            this.databaseHelper.SetParameterToSQLCommand("@InvoiceStatus", invoice.InvoiceStatus);
            this.databaseHelper.SetParameterToSQLCommand("@ServiceTotal", invoice.ServiceTotal);
            this.databaseHelper.SetParameterToSQLCommand("@FYearId", invoice.FYearId);
            this.databaseHelper.SetParameterToSQLCommand("@TaxId", invoice.TaxId);
            this.databaseHelper.SetParameterToSQLCommand("@CompanyId", invoice.CompanyId);

            if (invoice.InvoiceId > 0)
            {
                this.databaseHelper.SetParameterToSQLCommand("@InvoiceId", invoice.InvoiceId);
                return SqlHelper.ParseNativeLong(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spU_Invoice").ToString());
            }
            else
            {
                return SqlHelper.ParseNativeLong(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spI_Invoice").ToString());
            }
        }

        public int UpdateMultipleRecords(MultiOperationType operationType, string multiIds)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@MultiIds", multiIds);
            this.databaseHelper.SetParameterToSQLCommand("@OperationType", (int)operationType);

            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_Invoice").ToString());
        }

        public int UpdatePatientTestStatus(PatientTestStatusType patientTestStatusType, string multiIds)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@MultiIds", multiIds);
            this.databaseHelper.SetParameterToSQLCommand("@OperationType", (int)patientTestStatusType);

            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_Invoice").ToString());
        }

        #endregion

        #region PatientTest Detail

        public List<InvoiceDetailModel> GetInvoiceDetailByInvoiceId(long invoiceDetailId)
        {

            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@InvoiceId", invoiceDetailId);

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_InvoiceDetailByInvoiceId");


                if (dataReader != null)
                {
                    using (dataReader)
                    {
                        InvoiceDetailModel invoiceDetail;
                        List<InvoiceDetailModel> InvoiceDetailList = new List<InvoiceDetailModel>();

                        while (dataReader.Read())
                        {
                            invoiceDetail = new InvoiceDetailModel();

                            invoiceDetail.InvoiceDetailId = SqlHelper.GetDBLongValue(dataReader["InvoiceDetailId"]);
                            invoiceDetail.InvoiceId = SqlHelper.GetDBLongValue(dataReader["InvoiceId"]);
                            invoiceDetail.ServiceId = SqlHelper.GetDBLongValue(dataReader["ServiceId"]);
                            invoiceDetail.Amount = SqlHelper.GetDBStringValue(dataReader["Amount"]);
                            invoiceDetail.Remarks = SqlHelper.GetDBStringValue(dataReader["Remarks"]);
                            invoiceDetail.ServiceName = SqlHelper.GetDBStringValue(dataReader["ServiceName"]);
                            InvoiceDetailList.Add(invoiceDetail);
                        }

                        return InvoiceDetailList;
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

        public void SaveServiceDetail(long patientTestId, string testData)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@InvoiceId", patientTestId);
            this.databaseHelper.SetParameterToSQLCommand("@TestData", testData);

            SqlHelper.ParseNativeLong(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spU_InvoiceDetail").ToString());

        }
        #endregion

        #region Private Methods

        private List<InvoiceModel> GetInvoiceData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    InvoiceModel invoice;
                    List<InvoiceModel> invoiceList = new List<InvoiceModel>();

                    while (dataReader.Read())
                    {
                        invoice = new InvoiceModel();

                        invoice.InvoiceId = SqlHelper.GetDBLongValue(dataReader["InvoiceId"]);
                        invoice.ClientId = SqlHelper.GetDBLongValue(dataReader["ClientId"]);
                        invoice.CompanyId = SqlHelper.GetDBLongValue(dataReader["CompanyId"]);
                        invoice.InvoiceDate = SqlHelper.GetDBDateTimeValue(dataReader["InvoiceDate"]);
                        invoice.DueDate = SqlHelper.GetDBDateTimeValue(dataReader["DueDate"]);
                        invoice.TaxId = SqlHelper.GetDBLongValue(dataReader["TaxId"]);
                        invoice.DiscountAmount = SqlHelper.GetDBDecimalValue(dataReader["DiscountAmount"]);
                        invoice.CGST = SqlHelper.GetDBDecimalValue(dataReader["CGST"]);
                        invoice.SGST = SqlHelper.GetDBDecimalValue(dataReader["SGST"]);
                        invoice.CESS = SqlHelper.GetDBDecimalValue(dataReader["CESS"]);
                        invoice.TaxAmount = SqlHelper.GetDBDecimalValue(dataReader["TaxAmount"]);
                        invoice.InvoiceStatus = SqlHelper.GetDBStringValue(dataReader["InvoiceStatus"]);
                        invoice.ClientName = SqlHelper.GetDBStringValue(dataReader["ClientName"]);
                        invoice.ServiceTotal = SqlHelper.GetDBDecimalValue(dataReader["ServiceTotal"]);
                        invoice.InvoiceTotal = SqlHelper.GetDBDecimalValue(dataReader["InvoiceTotal"]);
                        invoice.IGST = SqlHelper.GetDBDecimalValue(dataReader["IGST"]);
                        invoice.TaxPercent = SqlHelper.GetDBDecimalValue(dataReader["TaxPercent"]);
                        invoiceList.Add(invoice);
                    }

                    if (invoiceList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            invoiceList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return invoiceList;
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

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
    }
}
