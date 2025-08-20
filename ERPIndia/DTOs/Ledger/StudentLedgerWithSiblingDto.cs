using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERPIndia.DTOs.Ledger
{
    public class SchoolInfoDto
    {
        public string Name { get; set; }
        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string Line3 { get; set; }
        public string Line4 { get; set; }
       
    }

    /// <summary>
    /// DTO representing parent/guardian information
    /// </summary>
    public class ParentInfoDto
    {
        public string SiblingCode { get; set; }
        public string FatherName { get; set; }
        public string FatherAadhar { get; set; }  // New field
        public string Address { get; set; }
        public string Mobile { get; set; }
    }

    /// <summary>
    /// DTO representing fee details for a student
    /// </summary>
    public class FeeDetailsDto
    {
        public decimal OldBalance { get; set; }
        public decimal AcademicFee { get; set; }
        public decimal TotalAcademicFee { get; set; }  // New field
        public decimal TransportFee { get; set; }

        public decimal TotalTransportFee { get; set; }
        public decimal TotalReceiptDiscount { get; set; }
        public decimal TotalLateFee { get; set; }
        public decimal MonthlyDiscount { get; set; }  // New field
        public decimal HeadWiseDiscount { get; set; }  // New field
        public decimal LateFee { get; set; }  // New field
        public decimal FeeAdded { get; set; }  // New field
        public decimal Additions { get; set; }
        public decimal Deductions { get; set; }
        public decimal TotalRequired { get; set; }
        public decimal AcademicFeeReceived { get; set; }
        public decimal TransportFeeReceived { get; set; }
        public decimal TotalReceived { get; set; }
        public List<string> ReceiptNumbers { get; set; }
        public decimal TotalDues { get; set; }
        public decimal FinalDueAmount { get; set; }  // New field
    }

    /// <summary>
    /// DTO representing a student in the ledger
    /// </summary>
    public class StudentDto
    {
        public Guid StudentId { get; set; }
        public int SerialNumber { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public FeeDetailsDto FeeDetails { get; set; }
    }

    /// <summary>
    /// DTO representing fee summary information
    /// </summary>
    public class FeeSummaryDto
    {
        public decimal RemainingToPay { get; set; }
        public decimal TotalFeeRequired { get; set; }
        public decimal TotalCollectedFee { get; set; }
    }

    /// <summary>
    /// Main DTO representing the complete student ledger with siblings
    /// </summary>
    public class StudentLedgerWithSiblingDto
    {
        public SchoolInfoDto SchoolInfo { get; set; }
        public string Session { get; set; }
        public ParentInfoDto ParentInfo { get; set; }
        public decimal TotalAmount { get; set; }
        public List<StudentDto> Students { get; set; }
        public FeeSummaryDto FeeSummary { get; set; }
    }
}