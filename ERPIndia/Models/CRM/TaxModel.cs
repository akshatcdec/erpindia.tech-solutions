using System.ComponentModel.DataAnnotations;

namespace ERPIndia.Models
{
    public class TaxModel
    {
        public TaxModel()
        {
            this.IsActive = true;
        }
        public long TaxId { get; set; }
        public string TaxName { get; set; }
        public string TaxSeries { get; set; }
        public decimal TaxPercent { get; set; }
        public long CompanyId { get; set; }

        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        public long CreatedBy { get; set; }

        public int TotalRecordCount { get; set; }

        public string DuplicateColumn { get; set; }
    }
}