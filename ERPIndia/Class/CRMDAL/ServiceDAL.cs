using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    public class ServiceDAL : IDisposable
    {
        #region Variable Declaration

        private DBHelper databaseHelper;

        #endregion

        #region Service

        #region Public Methods
        public List<ServiceModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
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

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_Service");

                return this.GetServiceData(dataReader);
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

        public ServiceModel GetById(long ServiceId)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@ServiceId", ServiceId);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_ServiceById");

                List<ServiceModel> list = this.GetServiceData(dataReader);

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

        public ServiceModel Save(ServiceModel Service)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@ServiceName", Service.ServiceName);
            this.databaseHelper.SetParameterToSQLCommand("@Description", Service.Description);
            this.databaseHelper.SetParameterToSQLCommand("@IsActive", Service.IsActive);
            this.databaseHelper.SetParameterToSQLCommand("@CreatedBy", Service.CreatedBy);
            this.databaseHelper.SetParameterToSQLCommand("@CompanyId", Service.CompanyId);
            this.databaseHelper.SetParameterToSQLCommand("@FYearId", Service.FYearId);
            IDataReader dataReader;
            ServiceModel tempUser = new ServiceModel();

            if (Service.ServiceId > 0)
            {
                this.databaseHelper.SetParameterToSQLCommand("@ServiceId", Service.ServiceId);
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spU_Service");
            }
            else
            {
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spI_Service");
            }

            if (dataReader != null)
            {
                using (dataReader)
                {
                    while (dataReader.Read())
                    {
                        tempUser.ServiceId = SqlHelper.GetDBLongValue(dataReader["RETURNVAL"]);
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

        public int UpdateMultipleRecords(MultiOperationType operationType, string multiIds)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@MultiIds", multiIds);
            this.databaseHelper.SetParameterToSQLCommand("@OperationType", (int)operationType);

            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_Service").ToString());
        }

        #endregion

        #region Private Methods

        private List<ServiceModel> GetServiceData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    ServiceModel Service;
                    List<ServiceModel> ServiceList = new List<ServiceModel>();

                    while (dataReader.Read())
                    {
                        Service = new ServiceModel();

                        Service.ServiceId = SqlHelper.GetDBLongValue(dataReader["ServiceId"]);
                        Service.ServiceName = SqlHelper.GetDBStringValue(dataReader["ServiceName"]);
                        Service.Description = SqlHelper.GetDBStringValue(dataReader["Description"]);
                        Service.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);

                        ServiceList.Add(Service);
                    }

                    if (ServiceList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            ServiceList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return ServiceList;
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
