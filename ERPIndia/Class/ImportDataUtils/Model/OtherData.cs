namespace ERPIndia
{
    public class OtherData
    {
        public string AdmsnNo { get; set; }
        public string SchoolCode { get; set; }
        public string BankAcNo { get; set; }
        public string BankName { get; set; }
        public string IfscCode { get; set; }
        public string NADID { get; set; }
        public string IDentityLocal { get; set; }
        public string IdentityOther { get; set; }
        public string PreviousSchoolDtl { get; set; }
        public string Note { get; set; }
        public string UploadTitle1 { get; set; }
        public string UpldPath1 { get; set; }
        public string UploadTitle2 { get; set; }
        public string UpldPath2 { get; set; }
        public string UploadTitle3 { get; set; }
        public string UpldPath3 { get; set; }
        public string UploadTitle4 { get; set; }
        public string UpldPath4 { get; set; }
        public string UploadTitle5 { get; set; }
        public string UpldPath5 { get; set; }
        public string UploadTitle6 { get; set; }
        public string UpldPath6 { get; set; }

        public string IsValidData { get; set; }
        public OtherData()
        {
            UploadTitle1 = "Father Aadhar";
            UploadTitle2 = "Mother Aadhar";
            UploadTitle3 = "Student Aadhar";
            UploadTitle4 = "Birth Certificate";
            UploadTitle5 = "Transfer Certificate";
            UploadTitle6 = "Report Card";
            IsValidData = "N";
        }


    }
}