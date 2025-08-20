using ERPIndia.Class.BLL;
using ERPIndia.Models;
using System.Collections.Generic;

namespace ERPIndia.ViewModel
{
    public class ClientViewModel
    {
        public ClientViewModel()
        {
            this.ClientModel = new ClientModel();
            this.States = StateBLL.GetAllActive();
            this.Branches = BranchBLL.GetAllActive();
        }
        public ClientModel ClientModel { get; set; }
        public List<BranchModel> Branches { get; set; }
        public List<StateModel> States { get; set; }
    }

}