using ERPIndia.Class.Helper;
using ERPIndia.Models;
using ERPIndia.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    public class LedgerDAL : IDisposable
    {
        #region Variable Declaration

        private DBHelper databaseHelper;

        #endregion

        #region Ledger

        #region Public Methods
        public List<LedgerModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
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

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_Ledger");

                return this.GetLedgerData(dataReader);
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
        public List<LedgerModel> GetAllPayment(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize, long ParentId)
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
                this.databaseHelper.SetParameterToSQLCommand("@ParentId", ParentId);

                long companyId = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.CompanyId));
                this.databaseHelper.SetParameterToSQLCommand("@CompanyId ", companyId);

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_LedgerPayment");

                return this.GetLedgerData(dataReader);
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
        public List<LedgerModel> GetLedgerByClientId(long AccountId)
        {
            try
            {
                this.databaseHelper = new DBHelper();
                this.databaseHelper.SetParameterToSQLCommand("@AccountId", AccountId);

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_LedgerByAccountId");

                return this.GetLedgerData(dataReader);
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
        public List<LedgerBookModel> GetLedgerBookByClientId(long AccountId)
        {
            try
            {
                this.databaseHelper = new DBHelper();
                this.databaseHelper.SetParameterToSQLCommand("@AccountId", AccountId);

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_sp_Ledger_LedgerBookByClientId");

                return this.GetLedgerBookData(dataReader);
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
        public LedgerModel GetById(long LedgerId)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@LedgerId", LedgerId);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_LedgerById");

                List<LedgerModel> list = this.GetLedgerData(dataReader);

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

        public LedgerModel Save(LedgerModel Ledger)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@AccountId", Ledger.AccountId);
            this.databaseHelper.SetParameterToSQLCommand("@LDate", Ledger.LDate);
            this.databaseHelper.SetParameterToSQLCommand("@RefNo", Ledger.RefNo);
            this.databaseHelper.SetParameterToSQLCommand("@Amount", Ledger.LCrAmt);
            this.databaseHelper.SetParameterToSQLCommand("@IsActive", Ledger.IsActive);
            this.databaseHelper.SetParameterToSQLCommand("@CreatedBy", Ledger.CreatedBy);
            this.databaseHelper.SetParameterToSQLCommand("@CompanyId", Ledger.CompanyId);
            this.databaseHelper.SetParameterToSQLCommand("@LRemarks", Ledger.LRemarks);
            this.databaseHelper.SetParameterToSQLCommand("@LPayMode", Ledger.LPayMode);
            this.databaseHelper.SetParameterToSQLCommand("@LVoucherType", Ledger.LVoucherType);

            IDataReader dataReader;
            LedgerModel tempUser = new LedgerModel();

            if (Ledger.LedgerId > 0)
            {
                this.databaseHelper.SetParameterToSQLCommand("@LedgerId", Ledger.LedgerId);
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spU_Ledger");
            }
            else
            {
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spI_Ledger");
            }

            if (dataReader != null)
            {
                using (dataReader)
                {
                    while (dataReader.Read())
                    {
                        tempUser.LedgerId = SqlHelper.GetDBLongValue(dataReader["RETURNVAL"]);
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

            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_Ledger").ToString());
        }

        #endregion

        #region Private Methods

        private List<LedgerModel> GetLedgerData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    LedgerModel Ledger;
                    List<LedgerModel> LedgerList = new List<LedgerModel>();

                    while (dataReader.Read())
                    {
                        Ledger = new LedgerModel();

                        Ledger.LedgerId = SqlHelper.GetDBLongValue(dataReader["LedgerId"]);
                        Ledger.AccountId = SqlHelper.GetDBLongValue(dataReader["AccountId"]);
                        Ledger.LDate = SqlHelper.GetDBDateTimeValue(dataReader["LDate"]);
                        Ledger.RefNo = SqlHelper.GetDBStringValue(dataReader["RefNo"]);
                        Ledger.LDrAmt = SqlHelper.GetDBDecimalValue(dataReader["LDrAmt"]);
                        Ledger.LCrAmt = SqlHelper.GetDBDecimalValue(dataReader["LCrAmt"]);
                        Ledger.Amount = SqlHelper.GetDBDecimalValue(dataReader["Amount"]);
                        Ledger.LBalance = SqlHelper.GetDBDecimalValue(dataReader["LBalance"]);
                        Ledger.LDate = SqlHelper.GetDBDateTimeValue(dataReader["LDate"]);
                        Ledger.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);
                        Ledger.LRemarks = SqlHelper.GetDBStringValue(dataReader["LRemarks"]);
                        Ledger.LPayMode = SqlHelper.GetDBStringValue(dataReader["LPayMode"]);
                        Ledger.LVoucherType = SqlHelper.GetDBStringValue(dataReader["LVoucherType"]);
                        LedgerList.Add(Ledger);
                    }

                    if (LedgerList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            LedgerList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return LedgerList;
                }
            }
            else
            {
                return null;
            }
        }
        private List<LedgerBookModel> GetLedgerBookData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    LedgerBookModel Ledger;
                    List<LedgerBookModel> LedgerList = new List<LedgerBookModel>();

                    while (dataReader.Read())
                    {
                        Ledger = new LedgerBookModel();

                        Ledger.AccountId = SqlHelper.GetDBLongValue(dataReader["AccountId"]);
                        Ledger.LDate = SqlHelper.GetDBDateTimeValue(dataReader["LDate"]);
                        Ledger.LDrAmt = SqlHelper.GetDBDecimalValue(dataReader["DrAmt"]);
                        Ledger.LCrAmt = SqlHelper.GetDBDecimalValue(dataReader["CrAmt"]);
                        Ledger.LDate = SqlHelper.GetDBDateTimeValue(dataReader["LDate"]);
                        Ledger.Balance = SqlHelper.GetDBDecimalValue(dataReader["Balance"]);
                        Ledger.LvoucherType = SqlHelper.GetDBStringValue(dataReader["LvoucherType"]);
                        Ledger.BalanceType = SqlHelper.GetDBStringValue(dataReader["BalanceType"]);
                        LedgerList.Add(Ledger);
                    }

                    if (LedgerList.Count > 0)
                    {
                        dataReader.NextResult();

                        //while (dataReader.Read())
                        //{
                        //    LedgerList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        //}
                    }

                    return LedgerList;
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
