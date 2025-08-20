using System;
using System.Collections.Generic;
using System.Text;

namespace ERPK12Models
{

    /// <summary>
    /// DTO class for the vwStudentFeeComplete view
    /// </summary>
    public class StudentFeeCompleteDto
    {
        #region Student Information Properties

        public decimal? PBal { get; set; }
        public DateTime? AdmsnDate { get; set; }
        public string AadharNo { get; set; }
        public string PENNo { get; set; }
        public string UdiseCode { get; set; }
        public string GName { get; set; }
        public string PickupName { get; set; }
        public decimal? Fee { get; set; }
        public string StCurrentAddress { get; set; }
        public string StPermanentAddress { get; set; }
        public string VillageName { get; set; }
        public decimal? StudentOldBalance { get; set; }
        public string AdmsnNo { get; set; }
        public string SchoolCode { get; set; }
        public string StudentNo { get; set; }
        public int? SrNo { get; set; }
        public string RollNo { get; set; }
        public string Class { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public string Section { get; set; }
        public string FirstName { get; set; }
        public string FatherName { get; set; }
        public string FatherAadhar { get; set; }
        public string MotherName { get; set; }
        public string MotherAadhar { get; set; }
        public string DiscountName { get; set; }
        public string CategoryName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public byte[] Photo { get; set; }
        public string Category { get; set; }
        public string Religion { get; set; }
        public string Caste { get; set; }
        public string Mobile { get; set; }
        public string DiscountCategory { get; set; }
        public string FeeCategory { get; set; }
        public int? ClassId { get; set; }
        public string PickupPoint { get; set; }
        public int? SectionId { get; set; }
        public int? HouseId { get; set; }
        public Guid FeeCategoryId { get; set; }
        public Guid FeeDiscountId { get; set; }
        public Guid TenantID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public Guid SessionID { get; set; }
        public Guid StudentId { get; set; }
        public string TenantCode { get; set; }

        #endregion

        #region Fee Information Properties

        public string ReceiptNo { get; set; }
        public string FeeSchoolCode { get; set; }
        public string AdmissionNo { get; set; }
        public decimal? ConcessinAuto { get; set; }
        public string LastDepositMonth { get; set; }
        public decimal? ConcessinMannual { get; set; }
        public DateTime? DateAuto { get; set; }
        public DateTime? DateMannual { get; set; }
        public decimal? FeeAdded { get; set; }
        public decimal? FeeBalance { get; set; }
        public decimal? LateFee { get; set; }
        public decimal? LateFeeAuto { get; set; }
        public string LateFeeNote { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public decimal? FeeOldBalance { get; set; }
        public decimal? Received { get; set; }
        public decimal? Remain { get; set; }
        public decimal? TotalFee { get; set; }
        public decimal? TotalTransport { get; set; }
        public decimal? TransportPaid { get; set; }
        public string TransportRoute { get; set; }
        public int? UserId { get; set; }
        public DateTime? EntryTime { get; set; }
        public string SessionYear { get; set; }
        public string PaymentMode { get; set; }
        public decimal? LateTotal { get; set; }
        public decimal? ConcessinTotal { get; set; }
        public int? ReceiptId { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public decimal? AcademicPaid { get; set; }
        public decimal? TransportCharge { get; set; }
        public decimal? AcademicCharge { get; set; }

        #endregion

        #region Additional Fields

        public string D1 { get; set; }
        public string D2 { get; set; }
        public string D3 { get; set; }
        public string D4 { get; set; }
        public string D5 { get; set; }
        public string D6 { get; set; }
        public string D7 { get; set; }
        public string D8 { get; set; }
        public string D9 { get; set; }
        public string D10 { get; set; }
        public decimal D11 { get; set; }
        public decimal D12 { get; set; }
        public decimal D13 { get; set; }
        public decimal D14 { get; set; }
        public decimal D15 { get; set; }
        public int D16 { get; set; }
        public int D17 { get; set; }
        public int D18 { get; set; }
        public int D19 { get; set; }
        public int D20 { get; set; }

        #endregion
    }

}
