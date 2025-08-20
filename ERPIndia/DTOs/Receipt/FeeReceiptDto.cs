using ERPIndia.DTOs.Ledger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERPIndia.DTOs.Receipt
{
    public class FeeReceiptDto
    {
        // School Information
        public ERPIndia.DTOs.Receipt.SchoolInfoDto SchoolInfo { get; set; }

        // Receipt Information
        public string ReceiptNumber { get; set; }
        public string TransportMonth { get; set; }
        public string TransportAmount { get; set; }
        public string TransportRoutes { get; set; }
        public string Session { get; set; }
        public DateTime ReceiptDate { get; set; }
        public string FormattedReceiptDate => ReceiptDate.ToString("dd-MM-yyyy");

        // Student Information
        public string RegistrationNumber { get; set; }
        public string PaymentMode { get; set; }
        
        public string PrintedByDateTime { get; set; }
        public string StudentName { get; set; }
        public string FatherName { get; set; }
        public string ClassStandard { get; set; }
        public string Months { get; set; }
        public string Section { get; set; }

        // Fee Details
        public List<FeeItemDto> FeeItems { get; set; }

        // Transport Details
        public List<TransportDetailsDto> TransportDetails { get; set; }

        // Financial Summary
        public FinancialSummaryDto FinancialSummary { get; set; }

        // Payment Information
        public decimal ReceivedAmount { get; set; }
        public string ReceivedAmountInWords { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentTimestamp { get; set; }

        // Default constructor initializes all collections
        public FeeReceiptDto()
        {
            SchoolInfo = new SchoolInfoDto();
            FeeItems = new List<FeeItemDto>();
            TransportDetails = new List<TransportDetailsDto>();
            FinancialSummary = new FinancialSummaryDto();
            ReceiptDate = DateTime.Today;
        }
    }
    public class SchoolInfoDto
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Website { get; set; } = "";
        public string Email { get; set; } = "";
        public string LOGOImg { get; set; } = "";
        public string SIGNImg { get; set; } = "";
        public string FeeNote1 { get; set; } = "";
        public string FeeNote2 { get; set; } = "";
        public string FeeNote3 { get; set; } = "";
        public string FeeNote4 { get; set; } = "";
        public string FeeNote5 { get; set; } = "";
        
    }

    /// <summary>
    /// Fee Item DTO representing a single fee line item
    /// </summary>
    public class FeeItemDto
    {
        public string FeeName { get; set; }
        public decimal Amount { get; set; }
        public bool IsHighlighted { get; set; } = false;

        public FeeItemDto() { }

        public FeeItemDto(string feeName, decimal amount, bool isHighlighted = false)
        {
            FeeName = feeName;
            Amount = amount;
            IsHighlighted = isHighlighted;
        }
    }

    /// <summary>
    /// Transport Details DTO
    /// </summary>
    public class TransportDetailsDto
    {
        public bool UsesTransport { get; set; } = false;
        public string Route { get; set; }
        public string Month { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Financial Summary DTO
    /// </summary>
    public class FinancialSummaryDto
    {
        public decimal OldBalance { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Concession { get; set; }
        public decimal OtherCharge { get; set; }
        public decimal RemainingAmount { get; set; }

        // Calculated property to get the grand total
        public decimal GrandTotal => TotalAmount - Concession + OtherCharge;
    }
}