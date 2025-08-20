using ERPIndia.Class.Helper;
using ERPIndia.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ERPIndia.Models.TokenAuth
{
    public class AuthTokenModel
    {
        public long TokenId { get; set; }
        public long UserId { get; set; }
        public string Token { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public string UserAgent { get; set; }
        public string IPAddress { get; set; }
    }

    //===============================================================
    // 4. AUTH TOKEN BUSINESS LOGIC LAYER
    //===============================================================

    public static class AuthTokenBLL
    {
        public static void SaveToken(long userId, string token, DateTime expiryDate)
        {
            AuthTokenModel model = new AuthTokenModel
            {
                UserId = userId,
                Token = token,
                CreatedDate = DateTime.Now,
                ExpiryDate = expiryDate,
                IsActive = true,
                UserAgent = HttpContext.Current.Request.UserAgent,
                IPAddress = CommonLogic.GetClientIPAddress()
            };

            AuthTokenDAL.SaveToken(model);
        }

        public static bool ValidateToken(long userId, string token)
        {
            return AuthTokenDAL.ValidateToken(userId, token);
        }

        public static void InvalidateTokens(long userId)
        {
            AuthTokenDAL.InvalidateTokens(userId);
        }

        public static AuthTokenModel GetTokenByValue(string token)
        {
            return AuthTokenDAL.GetTokenByValue(token);
        }
    }

    //===============================================================
    // 5. AUTH TOKEN DATA ACCESS LAYER
    //===============================================================

    public static class AuthTokenDAL
    {
        public static string constr= ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        public static void SaveToken(AuthTokenModel model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_SaveAuthToken", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@UserId", model.UserId);
                        cmd.Parameters.AddWithValue("@Token", model.Token);
                        cmd.Parameters.AddWithValue("@CreatedDate", model.CreatedDate);
                        cmd.Parameters.AddWithValue("@ExpiryDate", model.ExpiryDate);
                        cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                        cmd.Parameters.AddWithValue("@UserAgent", model.UserAgent);
                        cmd.Parameters.AddWithValue("@IPAddress", model.IPAddress);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Logger.Error(ex.Message);
                throw;
            }
        }

        public static bool ValidateToken(long userId, string token)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_ValidateAuthToken", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.Parameters.AddWithValue("@Token", token);

                        conn.Open();
                        object result = cmd.ExecuteScalar();

                        return result != null && Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Logger.Error(ex.Message);
                return false;
            }
        }

        public static void InvalidateTokens(long userId)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_InvalidateAuthTokens", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Logger.Error(ex.Message);
                throw;
            }
        }

        public static AuthTokenModel GetTokenByValue(string token)
        {
            try
            {
                AuthTokenModel model = null;

                using (SqlConnection conn = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_GetAuthTokenByValue", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Token", token);

                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                model = new AuthTokenModel
                                {
                                    TokenId = Convert.ToInt64(reader["TokenId"]),
                                    UserId = Convert.ToInt64(reader["UserId"]),
                                    Token = reader["Token"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                                    ExpiryDate = Convert.ToDateTime(reader["ExpiryDate"]),
                                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                                    UserAgent = reader["UserAgent"].ToString(),
                                    IPAddress = reader["IPAddress"].ToString()
                                };
                            }
                        }
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                
                Logger.Error("Class :" + ex.Message);
                return null;
            }
        }
    }

}