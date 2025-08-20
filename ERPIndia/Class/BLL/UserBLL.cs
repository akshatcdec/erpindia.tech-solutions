using ERPIndia.Class.DAL;
using ERPIndia.Class.Helper;
using ERPIndia.Models;
using ERPIndia.Models.SystemSettings;
using System.Collections.Generic;

namespace ERPIndia.Class.BLL
{
    /// <summary>
    /// User BLL class.
    /// </summary>
    public class UserBLL
    {
        #region Public Methods

        /// <summary>
        /// Gets all.
        /// </summary>
        /// <param name="roleType">Type of the role.</param>
        /// <param name="searchField">The search field.</param>
        /// <param name="searchValue">The search value.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="sortOrder">The sort order.</param>
        /// <param name="pageNo">The page no.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>
        /// Returns all users.
        /// </returns>
        public static List<UserModel> GetAll(RoleType roleType, string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetAll(roleType, searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }
        public static List<ResellerModel> GetAllReseller(RoleType roleType, string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetAllReseller(roleType, searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }
        /// <summary>
        /// Gets the by id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>Returns user by id.</returns>
        public static UserModel GetById(long userId)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetById(userId);
            }
        }
        public static SystemUsersModel GetSystemUserById(long userId)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetSystemUserById(userId);
            }
        }
        /// <summary>
        /// Gets the by email.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>Returns user by email.</returns>
        public static UserModel GetByEmail(string email)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetByEmail(email);
            }
        }

        /// <summary>
        /// Saves the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>Returns user id if success else duplicate column name.</returns>
        public static UserModel Save(UserModel user)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.Save(user);
            }
        }
        public static ResellerModel SaveReseller(ResellerModel user)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.SaveReseller(user);
            }
        }
        /// <summary>
        /// Updates the multiple records.
        /// </summary>
        /// <param name="operationType">Type of the operation.</param>
        /// <param name="multiIds">The multi ids.</param>
        /// <returns>Returns 1 if success else 0.</returns>
        public static int UpdateMultipleRecords(MultiOperationType operationType, string multiIds)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.UpdateMultipleRecords(operationType, multiIds);
            }
        }

        /// <summary>
        /// Validates the login.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns>Returns user if success otherwise user with less or equal to 0 value.</returns>
        public static UserModel ValidateLogin(string userName, string password)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.ValidateLogin(userName, password);
            }
        }
        public static SystemUsersModel ValidateLoginWithSchoolCode(string userName, string password, string schoolcode)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.ValidateLogin(userName, password, schoolcode);
            }
        }


        public static List<UserModel> GetAllDoctor(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetAll(RoleType.Doctor, searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }
        public static List<UserModel> GetAllAgents(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetAll(RoleType.Agents, searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        public static List<UserModel> GetAllActiveDoctor()
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetAll(RoleType.Doctor, "IsActive", "1", "FirstName", "ASC", 0, 0);
            }
        }

        public static List<UserModel> GetAllNurse(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetAll(RoleType.Nurse, searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        public static List<UserModel> GetAllLaboratorist(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetAll(RoleType.Laboratorist, searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        public static List<UserModel> GetAllPharmacist(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetAll(RoleType.Pharmacist, searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        public static List<UserModel> GetAllAccountant(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetAll(RoleType.Accountant, searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }

        public static List<UserModel> GetAllPatients(string searchField, string searchValue, string sortField, string sortOrder, int pageNo, int pageSize)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetAll(RoleType.Accountant, searchField, searchValue, sortField, sortOrder, pageNo, pageSize);
            }
        }


        public static UserModel GetDoctorById(long userId)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetById(userId, RoleType.Doctor.GetHashCode());
            }
        }
        public static ResellerModel GetResellerById(long userId)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetResellerById(userId, RoleType.Agents.GetHashCode());
            }
        }
        public static UserModel GetNurseById(long userId)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetById(userId, RoleType.Nurse.GetHashCode());
            }
        }

        public static UserModel GetLaboratoristById(long userId)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetById(userId, RoleType.Laboratorist.GetHashCode());
            }
        }

        public static UserModel GetPharmacistById(long userId)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetById(userId, RoleType.Pharmacist.GetHashCode());
            }
        }

        public static UserModel GetAccountantById(long userId)
        {
            using (UserDAL userDAL = new UserDAL())
            {
                return userDAL.GetById(userId, RoleType.Accountant.GetHashCode());
            }
        }



        #endregion
    }
}