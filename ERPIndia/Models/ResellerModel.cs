using ERPIndia.Class.BLL;
using System.Collections.Generic;

namespace ERPIndia.Models
{
    public class ResellerModel : UserModel
    {
        public long BranchId { get; set; }
        public string BranchName { get; set; }
        public ResellerModel() : base()
        {
            this.Branches = BranchBLL.GetAllActive();
        }
        public List<BranchModel> Branches { get; set; }
    }
}