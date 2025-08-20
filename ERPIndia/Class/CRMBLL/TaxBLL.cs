using ERPIndia.Class.DAL;
using ERPIndia.Models;
using System.Collections.Generic;
namespace ERPIndia.Class.BLL
{
    public class TaxBLL
    {
        public static List<TaxModel> GetAllActive()
        {
            using (TaxDAL taxDAL = new TaxDAL())
            {
                return taxDAL.GetAll("IsActive", "1", "TaxName", "ASC", 0, 0);
            }
        }
    }
}
