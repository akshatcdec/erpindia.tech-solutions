using ERPIndia.Class.DAL;

namespace ERPIndia.Class.BLL
{
    public class StudentUtilBLL
    {
        public static void SaveStudentBasic(StudentData studentData, int Role = 0, int SchoolCode = 0)
        {
            using (StudentUtilsDAL userDAL = new StudentUtilsDAL())
            {
                userDAL.SaveStudentBasic(studentData, Role, SchoolCode);
            }
        }
        public static void SaveStudentFamily(FamilyData familytData, int Role = 0, int SchoolCode = 0)
        {
            using (StudentUtilsDAL userDAL = new StudentUtilsDAL())
            {
                userDAL.SaveStudentFamily(familytData, Role, SchoolCode);
            }
        }
        public static void SaveStudentOther(OtherData othertData, int Role = 0, int SchoolCode = 0)
        {
            using (StudentUtilsDAL userDAL = new StudentUtilsDAL())
            {
                userDAL.SaveStudentOther(othertData, Role, SchoolCode);
            }
        }
        public static int IsStudentBasicDetailsExists(StudentData studentData)
        {
            using (StudentUtilsDAL userDAL = new StudentUtilsDAL())
            {
                return userDAL.IsStudentBasicDetailsExists(studentData);
            }
        }
        public static int IsStudentFamilyDetailsExists(FamilyData studentData)
        {
            using (StudentUtilsDAL userDAL = new StudentUtilsDAL())
            {
                return userDAL.IsStudentFamilyDetailsExists(studentData);
            }
        }
        public static int IsStudentOtherDetailsExists(OtherData studentData)
        {
            using (StudentUtilsDAL userDAL = new StudentUtilsDAL())
            {
                return userDAL.IsStudentOtherDetailsExists(studentData);
            }
        }
    }
}