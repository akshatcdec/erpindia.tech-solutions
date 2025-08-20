using ERPIndia.Class.BLL;
using ERPIndia.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ERPIndia.ViewModel
{
    public class LedgerViewModel
    {
        public LedgerViewModel()
        {
            this.Accounts = ClientBLL.GetAllActive();
        }
        public long LedgerId { get; set; }
        public long AccountId { get; set; }

        [Required]
        [Display(Name = "Date")]
        public DateTime LDate { get; set; }
        public string RefNo { get; set; }

        [Required]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }
        public string LRemarks { get; set; }
        [Required]
        [Display(Name = "Payment Mode")]
        public string LPayMode { get; set; }
        [Required]
        [Display(Name = "Type")]
        public string LVoucherType { get; set; }
        public List<ClientModel> Accounts { get; set; }
    }
}