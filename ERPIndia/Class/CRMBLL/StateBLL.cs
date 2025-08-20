using ERPIndia.Class.DAL;
using ERPIndia.Models;
using System.Collections.Generic;
namespace ERPIndia.Class.BLL
{
    public class StateBLL
    {
        public static List<StateModel> GetAllActive()
        {
            using (StateDAL stateDAL = new StateDAL())
            {
                return stateDAL.GetAll("IsActive", "1", "StateName", "ASC", 0, 0);
            }
        }
    }
}
