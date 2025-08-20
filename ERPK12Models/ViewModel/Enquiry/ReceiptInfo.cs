using System;

namespace ERPK12Models.ViewModel.Enquiry
{
    public class ReceiptInfo
    {
        public string ReceiptNo { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public decimal? Amount { get; set; }
        public string PaymentMode { get; set; }
    }
    public class ConversionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? AdmissionNo { get; set; }
    }
}
