using Dapper;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Wordprocessing;
using ERPIndia.Class.Helper;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ERPIndia.Controllers
{
    
    [LogOnAuthorize]

    public class SchoolProfileController : BaseController
    {
        private readonly TenantRepository _tenantRepository;

        public SchoolProfileController()
        {
            _tenantRepository = new TenantRepository();
        }
        private string HandleFileUpload(HttpPostedFileBase file, Guid tenantId, int schoolCode, string fileType)
        {
            if (file == null || file.ContentLength <= 0)
            {
                return null;
            }

            // Define paths
            string uploadFolder =string.Format("/Documents/{0}/SchoolProfile",schoolCode);

            // Ensure upload directory exists
            string physicalUploadPath = Server.MapPath(uploadFolder);

            if (!Directory.Exists(physicalUploadPath))
            {
                Directory.CreateDirectory(physicalUploadPath);
            }

            // Create file name with school code
            string fileExtension = Path.GetExtension(file.FileName);
            string fileName = fileType + fileExtension;
            string path = Path.Combine(physicalUploadPath, fileName);

            // Save file
            file.SaveAs(path);

            // Return just the filename
            return fileName;
        }
        private string HandleFeeFileUpload(HttpPostedFileBase file,string code)
        {
            if (file == null || file.ContentLength <= 0)
            {
                return null;
            }

            // Define paths
            string uploadFolder = string.Format("~/Reports/CrystalReport/{0}/",code);

            // Ensure upload directory exists
            string physicalUploadPath = Server.MapPath(uploadFolder);
            if (!Directory.Exists(physicalUploadPath))
            {
                Directory.CreateDirectory(physicalUploadPath);
            }

            // Set the specific filename we want to use
            string fileName = "fees.rpt";

            // Get file extension from the uploaded file
            string fileExtension = Path.GetExtension(file.FileName);

            // Check if it's an .rpt file
            if (!string.Equals(fileExtension, ".rpt", StringComparison.OrdinalIgnoreCase))
            {
                // You might want to handle this differently depending on your requirements
                return null;
            }

            // Create the full path
            string path = Path.Combine(physicalUploadPath, fileName);

            // Delete the existing file if it exists
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            // Save file
            file.SaveAs(path);

            // Return just the filename
            return fileName;
        }

        /// <summary>
        /// Index view action.
        /// </summary>
        /// <returns>
        /// Returns index action result.
        /// </returns>
        public async Task<ActionResult> Index()
        {
            try
            {
                Guid tenantId;
                if (Guid.TryParse(CommonLogic.GetSessionValue(StringConstants.TenantId), out tenantId))
                {
                    var tenant = await _tenantRepository.GetByIdAsync(tenantId);
                    if (tenant == null)
                    {
                        tenant = new TenantModel
                        {
                            TenantID = Guid.NewGuid(),
                            IsActive = true,
                            IsDeleted = false
                        };
                    }

                    return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", tenant) : View(tenant);
                }
                else
                {
                    // Invalid Tenant ID in session
                    return RedirectToAction("Index", "Error");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = CommonLogic.GetExceptionMessage(ex);
                return Request.IsAjaxRequest() ? (ActionResult)PartialView("Index", new TenantModel()) : View(new TenantModel());
            }
        }

        /// <summary>
        /// Index view post action.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>
        /// Returns index action result.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(
     TenantModel model,
     HttpPostedFileBase FeeRPT,
     HttpPostedFileBase logoImage,
     HttpPostedFileBase signImage,
     HttpPostedFileBase idCardImage,
     HttpPostedFileBase idHeaderImg,
     HttpPostedFileBase receiptBanner,
     HttpPostedFileBase admitCardBanner,
     HttpPostedFileBase reportCardBanner,
     HttpPostedFileBase transferCertBanner,
     HttpPostedFileBase salarySlipBanner,
     HttpPostedFileBase icardNameBanner,
     HttpPostedFileBase icardAddressBanner,
     HttpPostedFileBase principalSign,
     HttpPostedFileBase receiptSign)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                // ensure new tenants get an ID
                if (model.TenantID == Guid.Empty)
                    model.TenantID = Guid.NewGuid();
                model.IsActive = true;
                model.IsSingleFee = model.IsSingleFee ?? 'N';
                model.IsDeleted = false;
                model.ModifiedDate = DateTime.Now;
                model.CreatedBy = SqlHelper.ParseNativeLong(CommonLogic.GetSessionValue(StringConstants.UserId));

                // === FILE UPLOADS ===
                model.HeaderImg = HandleFileUpload(idHeaderImg, model.TenantID, model.TenantCode, "header") ?? model.HeaderImg;
                model.LOGOImg = HandleFileUpload(logoImage, model.TenantID, model.TenantCode, "logo") ?? model.LOGOImg;
                model.SIGNImg = HandleFileUpload(signImage, model.TenantID, model.TenantCode, "sign") ?? model.SIGNImg;
                model.IdCardImg = HandleFileUpload(idCardImage, model.TenantID, model.TenantCode, "id") ?? model.IdCardImg;

                model.ReceiptBannerImg = HandleFileUpload(receiptBanner, model.TenantID, model.TenantCode, "receiptBanner") ?? model.ReceiptBannerImg;
                model.AdmitCardBannerImg = HandleFileUpload(admitCardBanner, model.TenantID, model.TenantCode, "admitCardBanner") ?? model.AdmitCardBannerImg;
                model.ReportCardBannerImg = HandleFileUpload(reportCardBanner, model.TenantID, model.TenantCode, "reportCardBanner") ?? model.ReportCardBannerImg;
                model.TransferCertBannerImg = HandleFileUpload(transferCertBanner, model.TenantID, model.TenantCode, "transferCertBanner")?? model.TransferCertBannerImg;
                model.SalarySlipBannerImg = HandleFileUpload(salarySlipBanner, model.TenantID, model.TenantCode, "salarySlipBanner") ?? model.SalarySlipBannerImg;
                model.ICardNameBannerImg = HandleFileUpload(icardNameBanner, model.TenantID, model.TenantCode, "icardNameBanner") ?? model.ICardNameBannerImg;
                model.ICardAddressBannerImg = HandleFileUpload(icardAddressBanner, model.TenantID, model.TenantCode, "icardAddressBanner") ?? model.ICardAddressBannerImg;
                model.PrincipalSignImg = HandleFileUpload(principalSign, model.TenantID, model.TenantCode, "principalSign") ?? model.PrincipalSignImg;
                model.ReceiptSignImg = HandleFileUpload(receiptSign, model.TenantID, model.TenantCode, "receiptSign") ?? model.ReceiptSignImg;

                // Crystal Report upload (unchanged)
                HandleFeeFileUpload(FeeRPT, model.TenantCode.ToString());

                // === SAVE ===
                var result = await _tenantRepository.SaveAsync(model);
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Saved successfully!";
                    return RedirectToAction("Index");
                }

                ViewBag.ErrorMsg = result.Message;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMsg = CommonLogic.GetExceptionMessage(ex);
            }

            return View(model);
        }
    }
}
public class TenantRepository
{
    private readonly string _connectionString;

    public TenantRepository()
    {
        _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
    }

    /// <summary>
    /// Gets tenant by ID asynchronously
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>Tenant model</returns>
    public async Task<TenantModel> GetByIdAsync(Guid tenantId)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var query = @"
SELECT
    TenantID,
    TenantName,
    Address1,
    Address2,
    City,
    State,
    ZipCode,
    Email,
    Phone,
    Fax,
    IsActive,
    IsDeleted,
    CreatedDate,
    ModifiedDate,
    CreatedBy,
    TenantCode,
    FeeSrNo,
    TCSrNo,
    CCSrNo,
    PrintTitle,
    Line1,
    Line2,
    Line3,
    Line4,
    FeeNote1,
    FeeNote2,
    FeeNote3,
    FeeNote4,
    FeeNote5,
    SIGNImg,
    LOGOImg,
    IdCardImg,
    HeaderImg,
    ReceiptBannerImg,
    AdmitCardBannerImg,
    ReportCardBannerImg,
    TransferCertBannerImg,
    SalarySlipBannerImg,
    ICardNameBannerImg,
    ICardAddressBannerImg,
    PrincipalSignImg,
    ReceiptSignImg,
    MgrName,
    ManagerContactNo,
    DiseCode,
    RegNo,
    website,
    schoolnote1,
    schoolnote2,
    schoolnote3,
    schoolnote4,
    schoolnote5,
    admitnote1,
    admitnote2,
    admitnote3,
    admitnote4,
    admitnote5,
    TopBarName,
    TopBarAddress,
    EnableOnlineFee,
    IsSingleFee
FROM dbo.Tenants
WHERE TenantID = @TenantId
  AND IsDeleted = 0";

            return await connection.QueryFirstOrDefaultAsync<TenantModel>(
                query,
                new { TenantId = tenantId }
            );
        }
    }


    /// <summary>
    /// Saves tenant data asynchronously
    /// </summary>
    /// <param name="tenant">The tenant model to save</param>
    /// <returns>Result of the save operation</returns>
    public async Task<(bool Success, string Message, Guid TenantId)> SaveAsync(TenantModel tenant)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            // Check if TenantCode is unique
            var existingTenant = await connection.QueryFirstOrDefaultAsync<TenantModel>(
                "SELECT TenantID FROM dbo.Tenants WHERE TenantCode = @TenantCode AND TenantID != @TenantID AND IsDeleted = 0",
                new { tenant.TenantCode, tenant.TenantID });

            if (existingTenant != null)
            {
                return (false, "Tenant Code already exists", Guid.Empty);
            }

            // Check if tenant exists
            var tenantExists = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM dbo.Tenants WHERE TenantID = @TenantID",
                new { tenant.TenantID });

            if (tenantExists > 0)
            {
                // UPDATE existing tenant
                const string updateQuery = @"
UPDATE dbo.Tenants
SET
    TenantName               = @TenantName,
    Address1                 = @Address1,
    Address2                 = @Address2,
    City                     = @City,
    State                    = @State,
    ZipCode                  = @ZipCode,
    Email                    = @Email,
    Phone                    = @Phone,
    Fax                      = @Fax,
    IsActive                 = @IsActive,
    ModifiedDate             = @ModifiedDate,
    TenantCode               = @TenantCode,
    FeeSrNo                  = @FeeSrNo,
    TCSrNo                   = @TCSrNo,
    CCSrNo                   = @CCSrNo,
    PrintTitle               = @PrintTitle,
    Line1                    = @Line1,
    Line2                    = @Line2,
    Line3                    = @Line3,
    Line4                    = @Line4,
    FeeNote1                 = @FeeNote1,
    FeeNote2                 = @FeeNote2,
    FeeNote3                 = @FeeNote3,
    FeeNote4                 = @FeeNote4,
    FeeNote5                 = @FeeNote5,
    SIGNImg                  = @SIGNImg,
    LOGOImg                  = @LOGOImg,
    IdCardImg                = @IdCardImg,
    HeaderImg                = @HeaderImg,
    ReceiptBannerImg         = @ReceiptBannerImg,
    AdmitCardBannerImg       = @AdmitCardBannerImg,
    ReportCardBannerImg      = @ReportCardBannerImg,
    TransferCertBannerImg    = @TransferCertBannerImg,
    SalarySlipBannerImg      = @SalarySlipBannerImg,
    ICardNameBannerImg       = @ICardNameBannerImg,
    ICardAddressBannerImg    = @ICardAddressBannerImg,
    PrincipalSignImg         = @PrincipalSignImg,
    ReceiptSignImg           = @ReceiptSignImg,
    MgrName                  = @MgrName,
    ManagerContactNo         = @ManagerContactNo,
    DiseCode                 = @DiseCode,
    RegNo                    = @RegNo,
    website                  = @website,
    schoolnote1              = @schoolnote1,
    schoolnote2              = @schoolnote2,
    schoolnote3              = @schoolnote3,
    schoolnote4              = @schoolnote4,
    schoolnote5              = @schoolnote5,
    admitnote1               = @admitnote1,
    admitnote2               = @admitnote2,
    admitnote3               = @admitnote3,
    admitnote4               = @admitnote4,
    admitnote5               = @admitnote5,
    TopBarName               = @TopBarName,
    TopBarAddress            = @TopBarAddress,
    EnableOnlineFee          = @EnableOnlineFee,
    IsSingleFee              = @IsSingleFee
WHERE TenantID = @TenantID;
";

                await connection.ExecuteAsync(updateQuery, tenant);
                return (true, "Tenant updated successfully", tenant.TenantID);
            }
            else
            {
                // Insert new tenant
                // INSERT new tenant
                const string insertQuery = @"
INSERT INTO dbo.Tenants (
    TenantID, TenantName, Address1, Address2, City, State, ZipCode,
    Email, Phone, Fax, IsActive, IsDeleted, CreatedDate, ModifiedDate,
    CreatedBy, TenantCode, FeeSrNo, TCSrNo, CCSrNo, PrintTitle,
    Line1, Line2, Line3, Line4,
    FeeNote1, FeeNote2, FeeNote3, FeeNote4, FeeNote5,
    SIGNImg, LOGOImg, IdCardImg, HeaderImg,
    ReceiptBannerImg, AdmitCardBannerImg, ReportCardBannerImg,
    TransferCertBannerImg, SalarySlipBannerImg,
    ICardNameBannerImg, ICardAddressBannerImg,
    PrincipalSignImg, ReceiptSignImg,
    MgrName, ManagerContactNo, DiseCode, RegNo, website,
    schoolnote1, schoolnote2, schoolnote3, schoolnote4, schoolnote5,
    admitnote1, admitnote2, admitnote3, admitnote4, admitnote5,TopBarName,TopBarAddress,EnableOnlineFee,IsSingleFee
)
VALUES (
    @TenantID, @TenantName, @Address1, @Address2, @City, @State, @ZipCode,
    @Email, @Phone, @Fax, @IsActive, @IsDeleted, @CreatedDate, @ModifiedDate,
    @CreatedBy, @TenantCode, @FeeSrNo, @TCSrNo, @CCSrNo, @PrintTitle,
    @Line1, @Line2, @Line3, @Line4,
    @FeeNote1, @FeeNote2, @FeeNote3, @FeeNote4, @FeeNote5,
    @SIGNImg, @LOGOImg, @IdCardImg, @HeaderImg,
    @ReceiptBannerImg, @AdmitCardBannerImg, @ReportCardBannerImg,
    @TransferCertBannerImg, @SalarySlipBannerImg,
    @ICardNameBannerImg, @ICardAddressBannerImg,
    @PrincipalSignImg, @ReceiptSignImg,
    @MgrName, @ManagerContactNo, @DiseCode, @RegNo, @website,
    @schoolnote1, @schoolnote2, @schoolnote3, @schoolnote4, @schoolnote5,
    @admitnote1, @admitnote2, @admitnote3, @admitnote4, @admitnote5,@TopBarName,@TopBarAddress,@EnableOnlineFee,@IsSingleFee
);
";

                // Set creation date for new tenants
                tenant.CreatedDate = DateTime.Now;

                await connection.ExecuteAsync(insertQuery, tenant);
                return (true, "Tenant created successfully", tenant.TenantID);
            }
        }
    }
}
public class TenantModel
{
    /// <summary>
    /// Gets or sets the tenant id.
    /// </summary>
    public Guid TenantID { get; set; }

    /// <summary>
    /// Gets or sets the tenant name.
    /// </summary>
    [Required(ErrorMessage = "School Name is required.")]
    [Display(Name = "School Name")]
    [StringLength(500, ErrorMessage = "School Name cannot be longer than 500 characters.")]
    public string TenantName { get; set; }

    /// <summary>
    /// Gets or sets the address1.
    /// </summary>
    [Display(Name = "Address Line 1")]
    [StringLength(250, ErrorMessage = "Address Line 1 cannot be longer than 250 characters.")]
    public string Address1 { get; set; }

    /// <summary>
    /// Gets or sets the address2.
    /// </summary>
    [Display(Name = "Address Line 2")]
    [StringLength(250, ErrorMessage = "Address Line 2 cannot be longer than 250 characters.")]
    public string Address2 { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    [Display(Name = "City")]
    [StringLength(50, ErrorMessage = "City cannot be longer than 50 characters.")]
    public string City { get; set; }

    /// <summary>
    /// Gets or sets the state.
    /// </summary>
    [Display(Name = "State")]
    [StringLength(50, ErrorMessage = "State cannot be longer than 50 characters.")]
    public string State { get; set; }

    /// <summary>
    /// Gets or sets the zip code.
    /// </summary>
    [Display(Name = "Zip Code")]
    [StringLength(20, ErrorMessage = "Zip Code cannot be longer than 20 characters.")]
    public string ZipCode { get; set; }

    /// <summary>
    /// Gets or sets the email.
    /// </summary>
    [Display(Name = "Email")]
    [StringLength(255, ErrorMessage = "Email cannot be longer than 255 characters.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }

    /// <summary>
    /// Gets or sets the phone.
    /// </summary>
    [Display(Name = "Phone")]
    [StringLength(20, ErrorMessage = "Phone cannot be longer than 20 characters.")]
    public string Phone { get; set; }

    /// <summary>
    /// Gets or sets the fax.
    /// </summary>
    [Display(Name = "Fax")]
    [StringLength(20, ErrorMessage = "Fax cannot be longer than 20 characters.")]
    public string Fax { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is active.
    /// </summary>
    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is deleted.
    /// </summary>
    [DefaultValue(false)]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the created date.
    /// </summary>
    public DateTime? CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the modified date.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets the created by.
    /// </summary>
    public long CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the tenant code.
    /// </summary>
    [Required(ErrorMessage = "School Code is required.")]
    [Display(Name = "School Code")]
    public int TenantCode { get; set; }

    /// <summary>
    /// Gets or sets the fee serial number.
    /// </summary>
    [Display(Name = "Fee Serial No")]
    [DefaultValue(1)]

    public int? FeeSrNo { get; set; }

    /// <summary>
    /// Gets or sets the TC serial number.
    /// </summary>
    [Display(Name = "TC Serial No")]
    [DefaultValue(1)]
    public int? TCSrNo { get; set; }

    /// <summary>
    /// Gets or sets the CC serial number.
    /// </summary>
    [Display(Name = "CC Serial No")]
    [DefaultValue(1)]
    public int? CCSrNo { get; set; }

    /// <summary>
    /// Gets or sets the print title.
    /// </summary>
    [Required]
    [Display(Name = "Print Title")]
    [StringLength(100, ErrorMessage = "Print Title cannot be longer than 100 characters.")]
    public string PrintTitle { get; set; }

    /// <summary>
    /// Gets or sets line1.
    /// </summary>
    [Required]
    [Display(Name = "Line 1")]
    [StringLength(100, ErrorMessage = "Line 1 cannot be longer than 100 characters.")]
    public string Line1 { get; set; }

    /// <summary>
    /// Gets or sets line2.
    /// </summary>
    [Required]
    [Display(Name = "Line 2")]
    [StringLength(100, ErrorMessage = "Line 2 cannot be longer than 100 characters.")]
    public string Line2 { get; set; }

    /// <summary>
    /// Gets or sets line3.
    /// </summary>
    [Required]
    [Display(Name = "Line 3")]
    [StringLength(100, ErrorMessage = "Line 3 cannot be longer than 100 characters.")]
    public string Line3 { get; set; }

    /// <summary>
    /// Gets or sets line4.
    /// </summary>
    [Required]
    [Display(Name = "Line 4")]
    [StringLength(100, ErrorMessage = "Line 4 cannot be longer than 100 characters.")]
    public string Line4 { get; set; }

    /// <summary>
    /// Gets or sets fee note 1.
    /// </summary>
    [Display(Name = "FeeNote1")]
    [StringLength(50, ErrorMessage = "FeeNote1 cannot be longer than 50 characters.")]
    public string FeeNote1 { get; set; }

    /// <summary>
    /// Gets or sets fee note 2.
    /// </summary>
    [Display(Name = "FeeNote2")]
    [StringLength(50, ErrorMessage = "FeeNote2 cannot be longer than 50 characters.")]
    public string FeeNote2 { get; set; }

    /// <summary>
    /// Gets or sets fee note 3.
    /// </summary>
    [Display(Name = "FeeNote3")]
    [StringLength(50, ErrorMessage = "FeeNote3 cannot be longer than 50 characters.")]
    public string FeeNote3 { get; set; }

    /// <summary>
    /// Gets or sets fee note 4.
    /// </summary>
    [Display(Name = "FeeNote4")]
    [StringLength(50, ErrorMessage = "FeeNote4 cannot be longer than 50 characters.")]
    public string FeeNote4 { get; set; }

    /// <summary>
    /// Gets or sets fee note 5.
    /// </summary>
    [Display(Name = "FeeNote5")]
    [StringLength(50, ErrorMessage = "FeeNote5 cannot be longer than 50 characters.")]
    public string FeeNote5 { get; set; }

    /// <summary>
    /// Gets or sets the manager name.
    /// </summary>
    [Display(Name = "Manager Name")]
    [StringLength(100, ErrorMessage = "Manager Name cannot be longer than 100 characters.")]
    public string MgrName { get; set; }

    /// <summary>
    /// Gets or sets the manager contact.
    /// </summary>
    [Display(Name = "Manager Contact")]
    [StringLength(100, ErrorMessage = "Manager Contact cannot be longer than 100 characters.")]
    public string ManagerContactNo { get; set; }

    /// <summary>
    /// Gets or sets the U-DISE code.
    /// </summary>
    [Display(Name = "U-DISE Code")]
    [StringLength(100, ErrorMessage = "U-DISE Code cannot be longer than 100 characters.")]
    public string DiseCode { get; set; }

    /// <summary>
    /// Gets or sets the registration number.
    /// </summary>
    [Display(Name = "RegNo")]
    [StringLength(100, ErrorMessage = "RegNo cannot be longer than 100 characters.")]
    public string RegNo { get; set; }

    /// <summary>
    /// Gets or sets the website.
    /// </summary>
    [Display(Name = "Website")]
    [StringLength(100, ErrorMessage = "Website cannot be longer than 100 characters.")]
    public string website { get; set; }

    /// <summary>
    /// Gets or sets school note 1.
    /// </summary>
    [Display(Name = "School Note 1")]
    [StringLength(100, ErrorMessage = "School Note 1 cannot be longer than 100 characters.")]
    public string schoolnote1 { get; set; }

    /// <summary>
    /// Gets or sets school note 2.
    /// </summary>
    [Display(Name = "School Note 2")]
    [StringLength(100, ErrorMessage = "School Note 2 cannot be longer than 100 characters.")]
    public string schoolnote2 { get; set; }

    /// <summary>
    /// Gets or sets school note 3.
    /// </summary>
    [Display(Name = "School Note 3")]
    [StringLength(100, ErrorMessage = "School Note 3 cannot be longer than 100 characters.")]
    public string schoolnote3 { get; set; }

    /// <summary>
    /// Gets or sets school note 4.
    /// </summary>
    [Display(Name = "School Note 4")]
    [StringLength(100, ErrorMessage = "School Note 4 cannot be longer than 100 characters.")]
    public string schoolnote4 { get; set; }

    /// <summary>
    /// Gets or sets school note 5.
    /// </summary>
    [Display(Name = "School Note 5")]
    [StringLength(100, ErrorMessage = "School Note 5 cannot be longer than 100 characters.")]
    public string schoolnote5 { get; set; }
    
    [Display(Name = "Admit Card Note1")]
    [StringLength(140, ErrorMessage = "Admit Note 1 cannot be longer than 140 characters.")]
    public string admitnote1 { get; set; }

    [Display(Name = "Admit Card Note2")]
    [StringLength(140, ErrorMessage = "Admit Note 2 cannot be longer than 140 characters.")]
    public string admitnote2 { get; set; }


    [Display(Name = "Admit Card Note3")]
    [StringLength(140, ErrorMessage = "Admit Note 3 cannot be longer than 140 characters.")]
    public string admitnote3 { get; set; }


    [Display(Name = "Admit Card Note4")]
    [StringLength(140, ErrorMessage = "Admit Note 4 cannot be longer than 140 characters.")]
    public string admitnote4 { get; set; }


    [Display(Name = "Admit Card Note5")]
    [StringLength(140, ErrorMessage = "Admit Note 5 cannot be longer than 140 characters.")]
    public string admitnote5 { get; set; }
    /// <summary>
    /// Gets or sets the signature image path.
    /// </summary>
    [Display(Name = "Signature Image")]
    [StringLength(100, ErrorMessage = "Signature Image path cannot be longer than 100 characters.")]
    public string SIGNImg { get; set; }

    /// <summary>
    /// Gets or sets the logo image path.
    /// </summary>
    [Display(Name = "Logo Image")]
    [StringLength(100, ErrorMessage = "Logo Image path cannot be longer than 100 characters.")]
    public string LOGOImg { get; set; }

    /// <summary>
    /// Gets or sets the ID card image path.
    /// </summary>
    [Display(Name = "ID Card Image")]
    [StringLength(100, ErrorMessage = "ID Card Image path cannot be longer than 100 characters.")]
    public string IdCardImg { get; set; }

    [Display(Name = "Header Image")]
    [StringLength(100, ErrorMessage = "Header Image path cannot be longer than 100 characters.")]
    public string HeaderImg { get; set; }

    [Display(Name = "Fee RPT")]
   public string Feerpt { get; set; }

    [Display(Name = "Receipt Signature")]
    [StringLength(100, ErrorMessage = "Size: 200x130 pixels (JPG only)")]
    public string ReceiptSignImg { get; set; }

    [Display(Name = "Principal Signature")]
    [StringLength(100, ErrorMessage = "Size: 260x170 pixels (JPG only)")]
    public string PrincipalSignImg { get; set; }
    public string ICardAddressBannerImg { get; set; }
    public string ICardNameBannerImg { get; set; }
    public string SalarySlipBannerImg { get; set; }
    public string TransferCertBannerImg { get; set; }
    public string AdmitCardBannerImg { get; set; }
    public string ReportCardBannerImg { get; set; }
    public string ReceiptBannerImg { get; set; }
    [Display(Name = "Single-Fee Mode")]
    public char? IsSingleFee { get; set; } = 'N';

    [Display(Name = "Online Fee Allow")]
    public char? EnableOnlineFee { get; set; } = 'N';

    [Required]
    [Display(Name = "TopBar School Name")]
    [StringLength(100, ErrorMessage = "TopBar School Name cannot be longer than 100 characters.")]
    public string TopBarName { get; set; }
    
    [Required]
    [Display(Name = "TopBar School Address")]
    [StringLength(100, ErrorMessage = "TopBar School Address cannot be longer than 100 characters.")]
    public string TopBarAddress { get; set; }

}