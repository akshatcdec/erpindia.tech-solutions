using ERPIndia.Class.BLL;
using ERPIndia.Models;
using System.Collections.Generic;
namespace ERPIndia.ViewModel
{
    public class UserViewModel
    {
        public UserViewModel()
        {
            this.User = new UserModel();

            this.Schools = ClientBLL.GetAllActive();
        }
        public UserModel User { get; set; }
        public List<ClientModel> Schools { get; set; }
    }
}