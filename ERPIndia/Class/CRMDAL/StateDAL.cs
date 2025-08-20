using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    public class StateDAL : IDisposable
    {
        #region Variable Declaration

        private DBHelper databaseHelper;

        #endregion

        #region State

        #region Public Methods
        public List<StateModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
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

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_State");

                return this.GetStateData(dataReader);
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

        public StateModel GetById(long StateId)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@StateId", StateId);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_StateById");

                List<StateModel> list = this.GetStateData(dataReader);

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

        public StateModel Save(StateModel State)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@StateName", State.StateName);
            this.databaseHelper.SetParameterToSQLCommand("@AlphaCode", State.AlphaCode);
            this.databaseHelper.SetParameterToSQLCommand("@GSTStateCode", State.GSTStateCode);
            this.databaseHelper.SetParameterToSQLCommand("@IsActive", State.IsActive);
            IDataReader dataReader;
            StateModel tempUser = new StateModel();

            if (State.StateId > 0)
            {
                this.databaseHelper.SetParameterToSQLCommand("@StateId", State.StateId);
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spU_State");
            }
            else
            {
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spI_State");
            }

            if (dataReader != null)
            {
                using (dataReader)
                {
                    while (dataReader.Read())
                    {
                        tempUser.StateId = SqlHelper.GetDBLongValue(dataReader["RETURNVAL"]);
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

            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_State").ToString());
        }

        #endregion

        #region Private Methods

        private List<StateModel> GetStateData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    StateModel State;
                    List<StateModel> StateList = new List<StateModel>();

                    while (dataReader.Read())
                    {
                        State = new StateModel();

                        State.StateId = SqlHelper.GetDBLongValue(dataReader["StateId"]);
                        State.StateName = SqlHelper.GetDBStringValue(dataReader["StateName"]);
                        State.AlphaCode = SqlHelper.GetDBStringValue(dataReader["AlphaCode"]);
                        State.GSTStateCode = SqlHelper.GetDBStringValue(dataReader["GSTStateCode"]);
                        State.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);

                        StateList.Add(State);
                    }

                    if (StateList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            StateList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return StateList;
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
