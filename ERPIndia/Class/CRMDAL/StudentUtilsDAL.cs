using ERPIndia.Class.Helper;
using System;

namespace ERPIndia.Class.DAL
{
    public class StudentUtilsDAL : IDisposable
    {
        private DBHelper databaseHelper;

        public int IsStudentBasicDetailsExists(StudentData user)
        {
            this.databaseHelper = new DBHelper();
            this.databaseHelper.SetParameterToSQLCommand("@AdmsnNo", user.AdmsnNo);
            this.databaseHelper.SetParameterToSQLCommand("@SchoolCode", user.SchoolCode);
            var result = this.databaseHelper.GetExecuteScalarByCommand("SELECT dbo.fn_IsStudentExist (@AdmsnNo,@SchoolCode)").ToString();
            return (Utils.ParseInt(result));
        }
        public int IsStudentFamilyDetailsExists(FamilyData user)
        {
            this.databaseHelper = new DBHelper();
            this.databaseHelper.SetParameterToSQLCommand("@AdmsnNo", user.AdmsnNo);
            this.databaseHelper.SetParameterToSQLCommand("@SchoolCode", user.SchoolCode);
            var result = this.databaseHelper.GetExecuteScalarByCommand("SELECT dbo.fn_IsFamilyExist (@AdmsnNo,@SchoolCode)").ToString();
            return (Utils.ParseInt(result));
        }
        public int IsStudentOtherDetailsExists(OtherData user)
        {
            this.databaseHelper = new DBHelper();
            this.databaseHelper.SetParameterToSQLCommand("@AdmsnNo", user.AdmsnNo);
            this.databaseHelper.SetParameterToSQLCommand("@SchoolCode", user.SchoolCode);
            var result = this.databaseHelper.GetExecuteScalarByCommand("SELECT dbo.fn_IsOtherExist (@AdmsnNo,@SchoolCode)").ToString();
            return (Utils.ParseInt(result));
        }
        public void SaveStudentBasic(StudentData user, int Role, int SchoolCode)
        {
            this.databaseHelper = new DBHelper();

            foreach (var prop in user.GetType().GetProperties())
            {
                string propName = prop.Name;
                object propValue = prop.GetValue(user);

                // Special handling only if this property is "SchoolCode"
                if (propName.Equals("SchoolCode", StringComparison.OrdinalIgnoreCase))
                {
                    if (Role == 12)
                    {
                        // For Role = 12, use the custom parameter name
                        this.databaseHelper.SetParameterToSQLCommand(
                            "@SchoolCode",
                            SchoolCode
                        );

                    }
                    else
                    {
                        this.databaseHelper.SetParameterToSQLCommand(
                            "@SchoolCode",
                            propValue
                        );

                    }
                }
                else
                {
                    // For all other properties
                    this.databaseHelper.SetParameterToSQLCommand(
                        "@" + propName,
                        propValue
                    );
                }
            }

            // Execute the stored procedure
            this.databaseHelper.GetExecuteNonQueryByStoredProcedure("sp_InsertStudentBasic");
        }
        public void SaveStudentFamily(FamilyData user, int Role, int SchoolCode)
        {
            this.databaseHelper = new DBHelper();

            foreach (var prop in user.GetType().GetProperties())
            {
                string propName = prop.Name;
                object propValue = prop.GetValue(user);

                // Special handling only if this property is "SchoolCode"
                if (propName.Equals("SchoolCode", StringComparison.OrdinalIgnoreCase))
                {
                    if (Role == 12)
                    {
                        // For Role = 12, use the custom parameter name
                        this.databaseHelper.SetParameterToSQLCommand(
                            "@SchoolCode",
                            SchoolCode
                        );

                    }
                    else
                    {
                        this.databaseHelper.SetParameterToSQLCommand(
                            "@SchoolCode",
                            propValue
                        );

                    }
                }
                else
                {
                    // For all other properties
                    this.databaseHelper.SetParameterToSQLCommand(
                        "@" + propName,
                        propValue
                    );
                }
            }
            this.databaseHelper.GetExecuteNonQueryByStoredProcedure("sp_InsertStudentFamily");
        }
        public void SaveStudentOther(OtherData user, int Role, int SchoolCode)
        {
            this.databaseHelper = new DBHelper();

            foreach (var prop in user.GetType().GetProperties())
            {
                string propName = prop.Name;
                object propValue = prop.GetValue(user);

                // Special handling only if this property is "SchoolCode"
                if (propName.Equals("SchoolCode", StringComparison.OrdinalIgnoreCase))
                {
                    if (Role == 12)
                    {
                        // For Role = 12, use the custom parameter name
                        this.databaseHelper.SetParameterToSQLCommand(
                            "@SchoolCode",
                            SchoolCode
                        );

                    }
                    else
                    {
                        this.databaseHelper.SetParameterToSQLCommand(
                            "@SchoolCode",
                            propValue
                        );

                    }
                }
                else
                {
                    // For all other properties
                    this.databaseHelper.SetParameterToSQLCommand(
                        "@" + propName,
                        propValue
                    );
                }
            }
            this.databaseHelper.GetExecuteNonQueryByStoredProcedure("sp_InsertStudentOther");
        }
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.databaseHelper.Dispose();
            }
        }

    }
}