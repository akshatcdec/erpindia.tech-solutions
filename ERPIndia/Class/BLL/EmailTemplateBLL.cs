using ERPIndia.Class.DAL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using System.Collections.Generic;

namespace ERPIndia.Class.BLL
{
    /// <summary>
    /// Email template BLL class.
    /// </summary>
    public class EmailTemplateBLL
    {
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
        public static List<EmailTemplateModel> GetAll(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (EmailTemplateDAL emailTemplateDAL = new EmailTemplateDAL())
            {
                return emailTemplateDAL.GetAll(searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="emailTemplateId">The email template id.</param>
        /// <returns>Returns email template by id.</returns>
        public static EmailTemplateModel GetById(long emailTemplateId)
        {
            using (EmailTemplateDAL emailTemplateDAL = new EmailTemplateDAL())
            {
                return emailTemplateDAL.GetById(emailTemplateId);
            }
        }

        /// <summary>
        /// Gets the by code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns>Returns email template by code.</returns>
        public static EmailTemplateModel GetByCode(string code)
        {
            using (EmailTemplateDAL emailTemplateDAL = new EmailTemplateDAL())
            {
                return emailTemplateDAL.GetByCode(code);
            }
        }

        /// <summary>
        /// Saves the specified email template.
        /// </summary>
        /// <param name="emailTemplate">The email template.</param>
        /// <returns>Returns email template id if success else error code (Error Code : -1 - template name already exists).</returns>
        public static EmailTemplateModel Save(EmailTemplateModel emailTemplate)
        {
            using (EmailTemplateDAL emailTemplateDAL = new EmailTemplateDAL())
            {
                return emailTemplateDAL.Save(emailTemplate);
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
            using (EmailTemplateDAL emailTemplateDAL = new EmailTemplateDAL())
            {
                return emailTemplateDAL.UpdateMultipleRecords(operationType, multiIds);
            }
        }

        #endregion
    }
}