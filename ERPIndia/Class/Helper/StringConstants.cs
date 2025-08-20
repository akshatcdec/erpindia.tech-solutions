namespace ERPIndia.Class.Helper
{
    /// <summary>
    /// String constants class.
    /// </summary>
    public class StringConstants
    {
        #region App Setting Keys

        public const string AESPassword = "AESPassword";
        public const string AESBits = "AESBits";

        public const string AppConfig_ProfilePicFolderPath = "ProfilePicFolder";
        public const string AppConfig_ExcelFolderPath = "ExcelFolder";
        public const string AppConfig_DefaultProfilePic = "DefaultProfilePic";
        public const string AppConfig_ProfilePicAllowedFileType = "ProfilePicAllowedFileType";
        public const string AppConfig_ProfilePicMaxSize = "ProfilePicMaxSize";

        public const string AppConfig_ReportDocFolderPath = "ReportDocFolder";
        public const string AppConfig_ReportDocAllowedFileType = "ReportDocAllowedFileType";
        public const string AppConfig_ReportDocMaxSize = "ReportDocMaxSize";

        public const string AppConfig_ReportExcelAllowedFileType = "ReportExcelAllowedFileType";
        public const string AppConfig_RazorKey = "RazorKey";
        public const string AppConfig_RazorSecret = "RazorSecret";
        #endregion

        #region Login Constants

        public const int RememberCookieExpiration = 15; //// in days

        public const string UserId = "UserId";
        public const string UserName = "UserName";
        public const string FullName = "FullName";
        public const string RoleId = "RoleId";
        public const string RoleName = "RoleName";
        public const string CompanyId = "CompanyId";
        public const string SchoolCode = "SchoolCode";
        public const string SessionId = "SessionId";
        public const string TenantId = "TenantId";
        public const string TenantCode = "TenantCode";
        public const string SchoolName = "SchoolName";
        public const string ProfilePic = "ProfilePic";
        public const string MenuList = "MenuList";
        public const string RememberUserName = "username";
        public const string RememberPassword = "password";
        public const string FinancialYear = "fyear";
        public const string EmailId = "email";
        // Add these definitions to your StringConstants class
        public static readonly string TenantID = "TenantID";
        public static readonly string TenantName = "TenantName";
        public static readonly string TenantUserId = "TenantUserId";
        public static readonly string ActiveSessionID = "ActiveSessionID";
        public static readonly string ActiveSessionYear = "ActiveSessionYear";
        public static readonly string ActiveSessionPrint = "ActiveSessionPrint";
        public static readonly string ActiveHeaderImg = "ActiveHeaderImg";
        public static readonly string LogoImg = "LogoImg";
        // Add these definitions to your StringConstants class
        public static readonly string PrintTitle = "PrintTitle";
        public static readonly string Line1 = "Line1";
        public static readonly string Line2 = "Line2";
        public static readonly string Line3 = "Line3";
        public static readonly string Line4 = "Line4";
        public static readonly string ReceiptBannerImg = "ReceiptBannerImg";
        public static readonly string AdmitCardBannerImg = "AdmitCardBannerImg";
        public static readonly string ReportCardBannerImg = "ReportCardBannerImg";
        public static readonly string TransferCertBannerImg = "TransferCertBannerImg";
        public static readonly string SalarySlipBannerImg = "SalarySlipBannerImg";
        public static readonly string ICardNameBannerImg = "ICardNameBannerImg";
        public static readonly string ICardAddressBannerImg = "ICardAddressBannerImg";
        public static readonly string PrincipalSignImg = "schoolModel.PrincipalSignImg";
        public static readonly string ReceiptSignImg = "ReceiptSignImg";
        public static readonly string IsSingleFee = "IsSingleFee";
        public static readonly string EnableOnlineFee = "EnableOnlineFee";
        public static readonly string TopBarName = "TopBarName";
        public static readonly string TopBarAddress = "TopBarAddress";
        #endregion Login Constants

        #region Page Messages

        public const string RecordAlreadyExist = "{0} already exists.";
        public const string RecordNotExist = "{0} does not exists.";
        public const string RecordSave = "{0} saved successfully.";
        public const string RecordActive = "{0} record(s) active successfully.";
        public const string RecordInactive = "{0} record(s) inactive successfully.";
        public const string RecordDelete = "{0} record(s) deleted successfully.";
        public const string RecordStatus = "Showing {0} - {1} record(s) of {2}";
        public const string RecordSearch = "Your search for <b> {0} </b>has found<b> {1} </b>matches.";
        public const string RecordSaveError = "There are some problems while saving {0}.";
        public const string DeleteError = "You can't delete {0}. It is referenced by another location.";
        public const string RecordStatusMsg = "notifyMessage('{0}','{1}');";
        public const string PasswordSentMsg = "Your password sent successfully to your email account.";
        public const string MailSendError = "There are some problems while sending mail. Please try again later.";
        public const string NoRecordExists = "No record(s) found.";

        public const string ValidFileTypeMsg = "Valid File types:{0}";
        public const string ValidFileSizeMsg = "Valid file size upto {0} MB.";

        #endregion

        #region Login Messages

        public const string UserNotExist = "Your login failed due to invalid username.";
        public const string UserNotActive = "Your login temporarily inactivated. Please contact administrator.";
        public const string UserLoginFailed = "Your login failed due to invalid password.";

        #endregion

        #region Paging Messages

        public const string DefaultPageSize = "Default ({0})";

        #endregion

        #region Javascript Messages

        public const string SearchFieldValidation = "Please select any value from filter criteria!";
        public const string SearchValueValidation = "Please enter any value for filter!";

        public const string MultiProcessConfirmation = "Are you sure you want to {0} record?";
        public const string MultiProcessValidation = "Please select record(s) to {0}!";

        #endregion

        #region Error Messages

        public const string DatabaseFailedToConnect = "Database connection lost.";
        public const string DatabaseFailedToLogin = "Invalid database login credentials.";
        public const string DatabaseForeignKeyViolation = "Record is being referenced by another location.";
        public const string DatabaseUniqueConstraintViolation = "Record already exists with same primary details.";
        public const string DatabaseIncorrectSyntax = "Incorrect syntax error in query.";
        public const string DatabaseUnknownError = "Unknown error occured in database.";

        public const string DBErrorMsg = "Sorry for inconvenience. Following error occured :";
        public const string CodeUnknownError = "Sorry for inconvenience. Unknown error occured. Please try again later.";
        public const string PageNotFound = "Page not found.";

        #endregion

        #region General Constants

        public const string EmailRegEx = @"^[\w\+\-\._]+@[\w\-\._]+\.\w{2,}$";
        public const string DigitRegEx = @"^[0-9]+$";
        public const string MultipleEmailRegEx = @"^(\w([-_+.']*\w+)+@(\w(-*\w+)+\.)+[a-zA-Z]{2,4}[,])*\w([-_+.']*\w+)+@(\w(-*\w+)+\.)+[a-zA-Z]{2,4}$";
        public const string DateFormat = "{0:dd/MM/yyyy}";
        public const string DisplayDateFormat = "dd/MM/yyyy";

        #endregion General Constants
    }
}
