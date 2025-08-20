using System;
using System.IO;
using System.Web;
using CrystalDecisions.CrystalReports.Engine;
using System.Data;
using System.Data.SqlClient;
namespace ERPIndia.Class.Helper
{
    public class ReportUtils
    {
        private static DataTable GetDt1()
        {
            DataTable dt = new DataTable();
            string connString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connString))
            {

                // string query = "EXEC dbo.GetStudentLedgerWithNarrations '5AE0C8BB-2832-4408-A694-8482BBAA55D8', '8DE283DC-5BAE-4279-9F75-C267872489B6','2024-04-01', '2025-03-31' ";
                string query = "select * from StudentInfoBasic";//"SELECT * FROM itemstock";// dbo.GetStudentLedgerWithNarrations '5AE0C8BB-2832-4408-A694-8482BBAA55D8', '8DE283DC-5BAE-4279-9F75-C267872489B6','2024-04-01', '2025-03-31' ";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 0;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }
        public static void LoadReport()
        {

          

        }
    }
}