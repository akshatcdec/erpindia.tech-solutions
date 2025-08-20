using ERPIndia.Models;
using System.Collections.Generic;

namespace ERPIndia.ViewModel
{
    public class ClientDetailsModel
    {
        public ClientModel ClientModel { get; set; }
        public List<LedgerModel> Ledgers { get; set; }
        public List<LedgerBookModel> LedgerBook { get; set; }
    }
}