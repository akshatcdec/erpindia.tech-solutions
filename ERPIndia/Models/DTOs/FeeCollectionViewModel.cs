using System;
using System.Collections.Generic;

namespace ERPIndia.Models.DTOs
{
    /// <summary>
    /// DTO for fee collection view model that encapsulates all data needed for the fee collection page
    /// </summary>
    public class FeeCollectionViewModel
    {
        /// <summary>
        /// Student information
        /// </summary>
        public StudentInfoDto StudentInfo { get; set; }

        /// <summary>
        /// Fee details for the student
        /// </summary>
        public List<FeeDetailDto> FeeDetails { get; set; }

        /// <summary>
        /// Fee summary for quick view
        /// </summary>
        public FeeSummaryDto FeeSummary { get; set; }

        /// <summary>
        /// Available payment methods
        /// </summary>
        public List<PaymentMethodDto> PaymentMethods { get; set; }

        /// <summary>
        /// Available receipt templates
        /// </summary>
        public List<ReceiptTemplateDto> ReceiptTemplates { get; set; }

        /// <summary>
        /// List of months for fee collection
        /// </summary>
        public List<MonthDto> Months { get; set; }

        /// <summary>
        /// Student ID for reference
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Academic session ID
        /// </summary>
        public string SessionId { get; set; }
    }

    /// <summary>
    /// Student information DTO
    /// </summary>
    public class StudentInfoDto
    {
        /// <summary>
        /// Student ID (Admission Number)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Student name
        /// </summary>
        public string Name { get; set; }
        public string AdmNo { get; set; }

        /// <summary>
        /// Class/Grade of the student
        /// </summary>
        public string Class { get; set; }

        /// <summary>
        /// Section of the student
        /// </summary>
        public string Section { get; set; }

        /// <summary>
        /// Roll number of the student
        /// </summary>
        public string RollNo { get; set; }

        /// <summary>
        /// Father's name
        /// </summary>
        public string Father { get; set; }

        /// <summary>
        /// Contact number
        /// </summary>
        public string Contact { get; set; }

        /// <summary>
        /// Discount category the student belongs to
        /// </summary>
        public string DiscountCategory { get; set; }

        /// <summary>
        /// URL or path to student photo
        /// </summary>
        public string PhotoUrl { get; set; }
        public decimal OldYearBalance { get; set; }
        public string BalanceType { get; set; }
    }

    /// <summary>
    /// Fee detail DTO for each fee type
    /// </summary>
    public class FeeDetailDto
    {
        /// <summary>
        /// Fee category ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Fee category name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Monthly fee amounts
        /// </summary>
        public Dictionary<string, decimal> Months { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// Regular amounts before any discounts
        /// </summary>
        public Dictionary<string, decimal> RegularAmounts { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// Discounts applied per month
        /// </summary>
        public Dictionary<string, decimal> Discounts { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> paidAmounts { get; set; } = new Dictionary<string, decimal>();
        
    }

    /// <summary>
    /// Month DTO for display in fee table
    /// </summary>
    public class MonthDto
    {
        /// <summary>
        /// Full name of month
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Short name (3 letters)
        /// </summary>
        public string Short { get; set; }

        /// <summary>
        /// Display order (1-12)
        /// </summary>
        public int Order { get; set; }
    }

    /// <summary>
    /// Fee summary DTO for quick overview
    /// </summary>
    public class FeeSummaryDto
    {
        /// <summary>
        /// Total discount applied
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Late fee if applicable
        /// </summary>
        public decimal LateFee { get; set; }

        /// <summary>
        /// Old balance from previous years
        /// </summary>
        public decimal OldBalance { get; set; }
    }

    /// <summary>
    /// Payment method DTO
    /// </summary>
    public class PaymentMethodDto
    {
        /// <summary>
        /// Payment method ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Payment method name (Cash, Check, etc.)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether this is the default payment method
        /// </summary>
        public bool Default { get; set; }
    }

    /// <summary>
    /// Receipt template DTO
    /// </summary>
    public class ReceiptTemplateDto
    {
        /// <summary>
        /// Template ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Template content/name
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Whether this is the default template
        /// </summary>
        public bool Default { get; set; }
    }

    /// <summary>
    /// DTO for fees added to the payment
    /// </summary>
    public class AddedFeeDto
    {
        /// <summary>
        /// Unique ID for the added fee (counter)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Month of the fee (short name)
        /// </summary>
        public string Month { get; set; }

        /// <summary>
        /// Fee name/category
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Regular amount before discount
        /// </summary>
        public decimal RegularAmount { get; set; }

        /// <summary>
        /// Discount amount
        /// </summary>
        public decimal Discount { get; set; }

        /// <summary>
        /// Final amount after discount
        /// </summary>
        public decimal FinalAmount { get; set; }
    }

    /// <summary>
    /// DTO for submitting a fee payment
    /// </summary>
    public class FeePaymentRequestDto
    {
        /// <summary>
        /// Student ID
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// List of fees being paid
        /// </summary>
        public List<AddedFeeDto> Fees { get; set; } = new List<AddedFeeDto>();

        /// <summary>
        /// Subtotal amount before additional discount and late fee
        /// </summary>
        public decimal SubtotalAmount { get; set; }

        /// <summary>
        /// Additional discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Late fee amount
        /// </summary>
        public decimal LateFeeAmount { get; set; }

        /// <summary>
        /// Total amount after all adjustments
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Amount actually received
        /// </summary>
        public decimal ReceivedAmount { get; set; }

        /// <summary>
        /// Remaining amount (if partial payment)
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Selected payment method ID
        /// </summary>
        public int PaymentMethodId { get; set; }

        /// <summary>
        /// Selected receipt template ID
        /// </summary>
        public int ReceiptTemplateId { get; set; }

        /// <summary>
        /// Whether to send SMS notification
        /// </summary>
        public bool SendSms { get; set; }

        /// <summary>
        /// Additional note for the payment
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Receipt date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Session ID for academic year
        /// </summary>
        public string SessionId { get; set; }
    }

    /// <summary>
    /// DTO for payment response
    /// </summary>
    public class FeePaymentResponseDto
    {
        /// <summary>
        /// Generated receipt number
        /// </summary>
        public string ReceiptNumber { get; set; }

        /// <summary>
        /// Receipt date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Paid amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Whether payment was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Success or error message
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// DTO for monthly total calculations
    /// </summary>
    public class MonthlyTotalDto
    {
        /// <summary>
        /// Month short name (e.g., Apr, May)
        /// </summary>
        public string Month { get; set; }

        /// <summary>
        /// Total amount for the month
        /// </summary>
        public decimal Total { get; set; }
    }

    /// <summary>
    /// DTO for student ledger
    /// </summary>
    public class StudentLedgerDto
    {
        /// <summary>
        /// Student ID
        /// </summary>
        public string StudentId { get; set; }

        /// <summary>
        /// Student name
        /// </summary>
        public string StudentName { get; set; }

        /// <summary>
        /// List of payment transactions
        /// </summary>
        public List<PaymentTransactionDto> Transactions { get; set; } = new List<PaymentTransactionDto>();

        /// <summary>
        /// Total amount paid
        /// </summary>
        public decimal TotalPaid { get; set; }

        /// <summary>
        /// Total amount due
        /// </summary>
        public decimal TotalDue { get; set; }
    }

    /// <summary>
    /// DTO for payment transaction history
    /// </summary>
    public class PaymentTransactionDto
    {
        /// <summary>
        /// Receipt number
        /// </summary>
        public string ReceiptNumber { get; set; }

        /// <summary>
        /// Payment date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Payment method
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// List of fees paid in this transaction
        /// </summary>
        public List<AddedFeeDto> Fees { get; set; } = new List<AddedFeeDto>();

        /// <summary>
        /// Payment note
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// User who processed the payment
        /// </summary>
        public string ProcessedBy { get; set; }
    }
}