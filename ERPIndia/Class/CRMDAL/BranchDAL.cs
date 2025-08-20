using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    public class BranchDAL : IDisposable
    {
        #region Variable Declaration

        private DBHelper databaseHelper;

        #endregion

        #region Branch

        #region Public Methods
        public List<BranchModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
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

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_Branch");

                return this.GetBranchData(dataReader);
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

        public BranchModel GetById(long BranchId)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@BranchId", BranchId);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_BranchById");

                List<BranchModel> list = this.GetBranchData(dataReader);

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

        public BranchModel Save(BranchModel Branch)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@BranchName", Branch.BranchName);
            this.databaseHelper.SetParameterToSQLCommand("@Description", Branch.Description);
            this.databaseHelper.SetParameterToSQLCommand("@IsActive", Branch.IsActive);
            this.databaseHelper.SetParameterToSQLCommand("@CreatedBy", Branch.CreatedBy);
            this.databaseHelper.SetParameterToSQLCommand("@CompanyId", Branch.CompanyId);
            this.databaseHelper.SetParameterToSQLCommand("@FYearId", Branch.FYearId);
            IDataReader dataReader;
            BranchModel tempUser = new BranchModel();

            if (Branch.BranchId > 0)
            {
                this.databaseHelper.SetParameterToSQLCommand("@BranchId", Branch.BranchId);
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spU_Branch");
            }
            else
            {
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spI_Branch");
            }

            if (dataReader != null)
            {
                using (dataReader)
                {
                    while (dataReader.Read())
                    {
                        tempUser.BranchId = SqlHelper.GetDBLongValue(dataReader["RETURNVAL"]);
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

            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_Branch").ToString());
        }

        #endregion

        #region Private Methods

        private List<BranchModel> GetBranchData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    BranchModel Branch;
                    List<BranchModel> BranchList = new List<BranchModel>();

                    while (dataReader.Read())
                    {
                        Branch = new BranchModel();

                        Branch.BranchId = SqlHelper.GetDBLongValue(dataReader["BranchId"]);
                        Branch.BranchName = SqlHelper.GetDBStringValue(dataReader["BranchName"]);
                        Branch.Description = SqlHelper.GetDBStringValue(dataReader["Description"]);
                        Branch.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);
                        BranchList.Add(Branch);
                    }

                    if (BranchList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            BranchList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return BranchList;
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
