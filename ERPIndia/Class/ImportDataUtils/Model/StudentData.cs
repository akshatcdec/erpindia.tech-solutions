using System;

namespace ERPIndia
{
    public class StudentData
    {
        public string AdmsnNo { get; set; }
        public string SchoolCode { get; set; }
        public string StudentNo { get; set; }
        public string SrNo { get; set; }
        public string RollNo { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public string FirstName { get; set; }
        public string Gender { get; set; }
        public string Dob { get; set; }
        public string Category { get; set; }
        public string Religion { get; set; }
        public string Caste { get; set; }
        public string Mobile { get; set; }
        public string WhatsAppNum { get; set; }
        public string AdmsnDate { get; set; }
        public string Photo { get; set; }
        public string BloodGroup { get; set; }
        public string House { get; set; }
        public string AsOnDt { get; set; }
        public string DiscountCategory { get; set; }
        public string OldBalance { get; set; }
        public string FeeCategory { get; set; }
        public string Active { get; set; }
        public string EnquiryData { get; set; }
        public string SendSms { get; set; }
        public string UserId { get; set; }
        public string EntryDate { get; set; }
        public void SetDefaultValue()
        {
            if (string.IsNullOrEmpty(OldBalance))
            {
                OldBalance = "0";
            }
            if (string.IsNullOrEmpty(AsOnDt))
            {
                AsOnDt = DateTime.Now.ToString("dd/MM/yyyy");
            }

            IsValidData = "N";
        }
        public string IsValidData { get; set; }
    }
}