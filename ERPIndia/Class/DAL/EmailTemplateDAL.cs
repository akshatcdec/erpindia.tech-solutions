using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ERPIndia.Class.DAL
{
    /// <summary>
    /// Email template DAL class.
    /// </summary>
    public class EmailTemplateDAL : IDisposable
    {
        #region Variable Declaration

        private DBHelper databaseHelper;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="searchField">The search field.</param>
        /// <param name="searchValue">The search value.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="pageNo">The page no.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>Return all email templates.</returns>
        public List<EmailTemplateModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
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

                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_EmailTemplate");

                return this.GetEmailTemplateData(dataReader);
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

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="emailTemplateId">The email template id.</param>
        /// <returns>Returns email template by id.</returns>
        public EmailTemplateModel GetById(long emailTemplateId)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@EmailTemplateId", emailTemplateId);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_EmailTemplateById");

                List<EmailTemplateModel> emailTemplateList = this.GetEmailTemplateData(dataReader);

                if (emailTemplateList != null && emailTemplateList.Count > 0)
                {
                    return emailTemplateList[0];
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

        /// <summary>
        /// Gets the by code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>Returns email template by code.</returns>
        public EmailTemplateModel GetByCode(string code)
        {
            try
            {
                this.databaseHelper = new DBHelper();

                this.databaseHelper.SetParameterToSQLCommand("@Code", code);
                IDataReader dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spS_EmailTemplateByCode");

                List<EmailTemplateModel> emailTemplateList = this.GetEmailTemplateData(dataReader);

                if (emailTemplateList != null && emailTemplateList.Count > 0)
                {
                    return emailTemplateList[0];
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

        /// <summary>
        /// Saves the specified email template.
        /// </summary>
        /// <param name="emailTemplate">The email template.</param>
        /// <returns>Returns email template id if success else error code (Error Code : -1 - template name already exists).</returns>
        public EmailTemplateModel Save(EmailTemplateModel emailTemplate)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@Code", emailTemplate.Code);
            this.databaseHelper.SetParameterToSQLCommand("@Title", emailTemplate.Title);
            this.databaseHelper.SetParameterToSQLCommand("@Subject", emailTemplate.Subject);
            this.databaseHelper.SetParameterToSQLCommand("@FromEmail", emailTemplate.FromEmail);
            this.databaseHelper.SetParameterToSQLCommand("@Message", emailTemplate.Message);
            this.databaseHelper.SetParameterToSQLCommand("@Signature", emailTemplate.Signature);
            this.databaseHelper.SetParameterToSQLCommand("@IsActive", emailTemplate.IsActive);


            IDataReader dataReader;
            EmailTemplateModel model = new EmailTemplateModel();

            if (emailTemplate.EmailTemplateId > 0)
            {
                this.databaseHelper.SetParameterToSQLCommand("@EmailTemplateId", emailTemplate.EmailTemplateId);
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spU_EmailTemplate");
            }
            else
            {
                dataReader = this.databaseHelper.GetReaderByStoredProcedure("CRM_spI_EmailTemplate");
            }

            if (dataReader != null)
            {
                using (dataReader)
                {
                    while (dataReader.Read())
                    {
                        model.EmailTemplateId = SqlHelper.GetDBLongValue(dataReader["RETURNVAL"]);
                        model.DuplicateColumn = SqlHelper.GetDBStringValue(dataReader["DuplicateColumn"]);
                    }

                    return model;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Updates the multiple records.
        /// </summary>
        /// <param name="operationType">Type of the operation.</param>
        /// <param name="multiIds">The multi ids.</param>
        /// <returns>Returns 1 if success else 0.</returns>
        public int UpdateMultipleRecords(MultiOperationType operationType, string multiIds)
        {
            this.databaseHelper = new DBHelper();

            this.databaseHelper.SetParameterToSQLCommand("@MultiIds", multiIds);
            this.databaseHelper.SetParameterToSQLCommand("@OperationType", (int)operationType);

            return SqlHelper.ParseNativeInt(this.databaseHelper.GetExecuteScalarByStoredProcedure("CRM_spM_EmailTemplate").ToString());
        }

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

        #region Helper Methods

        /// <summary>
        /// Gets the email template data.
        /// </summary>
        /// <param name="dataReader">The data reader.</param>
        /// <returns>Returns email template list.</returns>
        private List<EmailTemplateModel> GetEmailTemplateData(IDataReader dataReader)
        {
            if (dataReader != null)
            {
                using (dataReader)
                {
                    EmailTemplateModel emailTemplate;
                    List<EmailTemplateModel> emailTemplateList = new List<EmailTemplateModel>();

                    while (dataReader.Read())
                    {
                        emailTemplate = new EmailTemplateModel();
                        emailTemplate.EmailTemplateId = SqlHelper.GetDBLongValue(dataReader["EmailTemplateId"]);
                        emailTemplate.Code = SqlHelper.GetDBStringValue(dataReader["Code"]);
                        emailTemplate.Title = SqlHelper.GetDBStringValue(dataReader["Title"]);
                        emailTemplate.Subject = SqlHelper.GetDBStringValue(dataReader["Subject"]);
                        emailTemplate.FromEmail = SqlHelper.GetDBStringValue(dataReader["FromEmail"]);
                        emailTemplate.Message = SqlHelper.GetDBStringValue(dataReader["Message"]);
                        emailTemplate.Signature = SqlHelper.GetDBStringValue(dataReader["Signature"]);
                        emailTemplate.IsActive = SqlHelper.GetDBBoolValue(dataReader["IsActive"]);

                        emailTemplateList.Add(emailTemplate);
                    }

                    if (emailTemplateList.Count > 0)
                    {
                        dataReader.NextResult();

                        while (dataReader.Read())
                        {
                            emailTemplateList[0].TotalRecordCount = SqlHelper.GetDBIntValue(dataReader["TotalRecordCount"]);
                        }
                    }

                    return emailTemplateList;
                }
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}