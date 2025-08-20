using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ERPIndia.Class.Repo
{
    public abstract class BaseRepository
    {
        protected readonly string _connectionString;

        protected BaseRepository()
        {
            _connectionString = System.Configuration.ConfigurationManager
                .ConnectionStrings["ConnectionString"].ConnectionString;
        }

        protected IDbConnection GetConnection()
        {
            return new System.Data.SqlClient.SqlConnection(_connectionString);
        }
    }
}