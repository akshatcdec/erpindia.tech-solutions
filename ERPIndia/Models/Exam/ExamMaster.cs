using System;

namespace ERPIndia.Models.Exam
{
    public class ExamMaster
    {
        public Guid ExamID { get; set; }
        public int SerialNumber { get; set; }
        public string ExamMonth { get; set; }
        public string ExamName { get; set; }
        public string ExamType { get; set; }
        public string Remarks { get; set; }
        public int SessionYear { get; set; }
        public Guid SessionID { get; set; }
        public Guid TenantID { get; set; }
        public int TenantCode { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        // New fields with correct data types
        public bool? Num { get; set; }              // BIT field
        public bool? MS { get; set; }               // BIT field
        public string AdmitCard { get; set; }       // VARCHAR(100) field
        public bool? AC { get; set; }
    }
}