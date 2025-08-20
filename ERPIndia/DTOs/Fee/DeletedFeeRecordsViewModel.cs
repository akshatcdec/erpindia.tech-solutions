using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERPIndia.DTOs.Fee
{
    public class DeletedFeeRecordsViewModel
    {
        public DeletedFeeRecordsViewModel()
        {
            DeletedRecords = new List<DeletedFeeRecordDto>();
        }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ErrorMessage { get; set; }
        public List<DeletedFeeRecordDto> DeletedRecords { get; set; }
        public string DisplayDateRange
        {
            get
            {
                return $"{FromDate:dd-MM-yyyy} to {ToDate:dd-MM-yyyy}";
            }
        }
    }
    public class DeletedFeeRecordDto
    {
        public int ReceiptNo { get; set; }
        public int Code { get; set; }
        public string DateAuto { get; set; }
        public decimal Amount { get; set; }
        public string Name { get; set; }
        public string Sr { get; set; }
        public string Roll { get; set; }
        public string Class { get; set; }
        public string Section { get; set; }
        public string Mobile { get; set; }
        public string DeletedBy { get; set; }
        public string DeletedDate { get; set; }
        public string DeleteReason { get; set; }
    }
}