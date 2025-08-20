using System;
using System.IO;

namespace ERPIndia.Class.Helper
{

    /// <summary>
    /// Static utility class for managing school folder structures
    /// </summary>
    public static class SchoolFolderUtils
    {
        /// <summary>
        /// Creates the entire folder structure for a given school code in the Documents folder
        /// </summary>
        /// <param name="schoolCode">The school code (e.g., "5000")</param>
        /// <returns>True if successful, false if an error occurred</returns>
        public static bool CreateSchoolFolderStructure(string schoolCode)
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string schoolCodeFolder = Path.Combine(documentsPath, schoolCode);

                // Create the main school code folder
                Directory.CreateDirectory(schoolCodeFolder);

                // Create EmployeeProfile folder
                Directory.CreateDirectory(Path.Combine(schoolCodeFolder, "employeeProfile"));

                // Create Reports folder and its subfolders
                string reportsFolder = Path.Combine(schoolCodeFolder, "reports");
                Directory.CreateDirectory(reportsFolder);
                Directory.CreateDirectory(Path.Combine(reportsFolder, "crystal"));
                Directory.CreateDirectory(Path.Combine(reportsFolder, "rdlc"));

                // Create SchoolProfile folder
                Directory.CreateDirectory(Path.Combine(schoolCodeFolder, "schoolprofile"));

                // Create StudentProfile folder
                Directory.CreateDirectory(Path.Combine(schoolCodeFolder, "studentprofile"));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating folder structure: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets the path to a specific folder within the school structure
        /// </summary>
        /// <param name="schoolCode">The school code</param>
        /// <param name="folderType">The folder type to retrieve</param>
        /// <returns>The full path to the requested folder</returns>
        public static string GetFolderPath(string schoolCode, string folderType)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string schoolCodeFolder = Path.Combine(documentsPath, schoolCode);

            switch (folderType.ToLower())
            {
                case "root":
                    return schoolCodeFolder;
                case "employeeprofile":
                    return Path.Combine(schoolCodeFolder, "employeeprofile");
                case "reports":
                    return Path.Combine(schoolCodeFolder, "reports");
                case "crystal":
                    return Path.Combine(schoolCodeFolder, "reports", "crystal");
                case "rdlc":
                    return Path.Combine(schoolCodeFolder, "reports", "rdlc");
                case "schoolprofile":
                    return Path.Combine(schoolCodeFolder, "schoolprofile");
                case "studentprofile":
                    return Path.Combine(schoolCodeFolder, "studentprofile");
                default:
                    throw new ArgumentException($"Unknown folder type: {folderType}");
            }
        }
    }

}