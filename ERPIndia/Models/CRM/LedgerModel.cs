using System;
using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models
{
    public class LedgerModel
    {
        public LedgerModel()
        {
            this.IsActive = true;
        }
        public long LedgerId { get; set; }
        public long AccountId { get; set; }

        [Required]
        public DateTime LDate { get; set; }
        public decimal LDrAmt { get; set; }
        public decimal LCrAmt { get; set; }
        public decimal Amount { get; set; }
        public decimal LBalance { get; set; }
        public string RefNo { get; set; }
        public string LRemarks { get; set; }
        public string LPayMode { get; set; }
        public string LVoucherType { get; set; }

        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        public long CompanyId { get; set; }

        public long CreatedBy { get; set; }
        public int TotalRecordCount { get; set; }

        public string DuplicateColumn { get; set; }

    }
}