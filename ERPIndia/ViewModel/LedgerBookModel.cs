using System;

namespace ERPIndia.ViewModel
{
    public class LedgerBookModel
    {
        public LedgerBookModel() { }
        public long LedgerId { get; set; }
        public long AccountId { get; set; }
        public DateTime LDate { get; set; }
        public decimal Balance { get; set; }
        public decimal LDrAmt { get; set; }
        public decimal LCrAmt { get; set; }
        public string BalanceType { get; set; }
        public string LvoucherType { get; set; }

    }
}