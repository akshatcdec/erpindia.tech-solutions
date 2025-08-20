using System;
using System.Data;
using System.Data.SqlClient;

namespace ERPIndia.Class.Helper
{
    /// <summary>
    /// Database Helper class.
    /// </summary>
    public class DBHelper : IDisposable
    {
        #region Variable Declaration

        private int commandTimeout = 120;
        private string connectionString;
        private SqlConnection sqlConnection;
        private SqlCommand sqlCommand;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DBHelper"/> class.
        /// </summary>
        public DBHelper()
        {
            this.connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();

            this.sqlConnection = new SqlConnection(this.connectionString);
            this.sqlCommand = new SqlCommand();
            this.sqlCommand.CommandTimeout = this.commandTimeout;
            this.sqlCommand.Connection = this.sqlConnection;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DBHelper"/> class.
        /// </summary>
        /// <param name="connString">The conn string.</param>
        public DBHelper(string connString)
        {
            this.connectionString = connString;

            this.sqlConnection = new SqlConnection(this.connectionString);
            this.sqlCommand = new SqlCommand();
            this.sqlCommand.CommandTimeout = this.commandTimeout;
            this.sqlCommand.Connection = this.sqlConnection;
        }

        #endregion

        #region Public Methods

        #region Execute Methods

        /// <summary>
        /// Gets the execute scalar by command.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>Returns scalar value.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public object GetExecuteScalarByStoredProcedure(string procedureName)
        {
            object identity = 0;

            try
            {
                this.sqlConnection.Open();

                this.sqlCommand.CommandText = procedureName;
                this.sqlCommand.CommandType = CommandType.StoredProcedure;
                this.sqlCommand.Connection = this.sqlConnection;

                identity = this.sqlCommand.ExecuteScalar();
            }
            catch
            {
                throw;
            }
            finally
            {
                this.CloseConnection();
            }

            return identity;
        }

        /// <summary>
        /// Gets the execute scalar by command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>Returns scalar value.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public object GetExecuteScalarByCommand(string commandText)
        {
            object identity = 0;

            try
            {
                this.sqlConnection.Open();

                this.sqlCommand.CommandText = commandText;
                this.sqlCommand.CommandType = CommandType.Text;
                this.sqlCommand.Connection = this.sqlConnection;

                identity = this.sqlCommand.ExecuteScalar();
            }
            catch
            {
                throw;
            }
            finally
            {
                this.CloseConnection();
            }

            return identity;
        }

        /// <summary>
        /// Gets the execute non query by stored procedure.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void GetExecuteNonQueryByStoredProcedure(string procedureName)
        {
            try
            {
                this.sqlConnection.Open();

                this.sqlCommand.CommandText = procedureName;
                this.sqlCommand.CommandType = CommandType.StoredProcedure;
                this.sqlCommand.Connection = this.sqlConnection;

                this.sqlCommand.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            finally
            {
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Gets the execute non query by command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void GetExecuteNonQueryByCommand(string commandText)
        {
            try
            {
                this.sqlConnection.Open();

                this.sqlCommand.CommandText = commandText;
                this.sqlCommand.CommandType = CommandType.Text;
                this.sqlCommand.Connection = this.sqlConnection;

                this.sqlCommand.ExecuteNonQuery();
            }
            catch
            {
                throw;
            }
            finally
            {
                this.CloseConnection();
            }
        }

        #endregion

        #region Reader Methods

        /// <summary>
        /// Gets the reader by stored procedure.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>Returns data reader.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public IDataReader GetReaderByStoredProcedure(string procedureName)
        {
            this.sqlConnection.Open();

            this.sqlCommand.CommandText = procedureName;
            this.sqlCommand.CommandType = CommandType.StoredProcedure;

            return this.sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Gets the reader by command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>Returns data reader.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public IDataReader GetReaderByCommand(string commandText)
        {
            this.sqlConnection.Open();

            this.sqlCommand.CommandText = commandText;
            this.sqlCommand.CommandType = CommandType.Text;

            return this.sqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
        }

        #endregion

        #region Parameter Methods

        /// <summary>
        /// Sets the parameter to SQL command.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameterValue">The parameter value.</param>
        public void SetParameterToSQLCommand(string parameterName, object parameterValue)
        {
            this.sqlCommand.Parameters.Add(new SqlParameter(parameterName, parameterValue));
        }

        /// <summary>
        /// Sets the parameter to SQL command.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameterType">Type of the parameter.</param>
        /// <param name="parameterValue">The parameter value.</param>
        public void SetParameterToSQLCommand(string parameterName, SqlDbType parameterType, object parameterValue)
        {
            this.sqlCommand.Parameters.Add(new SqlParameter(parameterName, parameterType));
            this.sqlCommand.Parameters[parameterName].Value = parameterValue;
        }

        /// <summary>
        /// Sets the parameter to SQL command.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="parameterType">Type of the parameter.</param>
        /// <param name="parameterSize">Size of the parameter.</param>
        /// <param name="parameterValue">The parameter value.</param>
        public void SetParameterToSQLCommand(string parameterName, SqlDbType parameterType, int parameterSize, object parameterValue)
        {
            this.sqlCommand.Parameters.Add(new SqlParameter(parameterName, parameterType, parameterSize));
            this.sqlCommand.Parameters[parameterName].Value = parameterValue;
        }

        /// <summary>
        /// Clears the SQL command parameters.
        /// </summary>
        public void ClearSQLCommandParameters()
        {
            this.sqlCommand.Parameters.Clear();
        }

        #endregion

        #region DataTable Methods

        /// <summary>
        /// Gets the dataset by stored procedure.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>Returns data set.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public DataSet GetDatasetByStoredProcedure(string procedureName)
        {
            try
            {
                using (DataSet dataSet = new DataSet())
                {
                    dataSet.Locale = System.Globalization.CultureInfo.InvariantCulture;
                    this.sqlConnection.Open();

                    this.sqlCommand.CommandText = procedureName;
                    this.sqlCommand.CommandType = CommandType.StoredProcedure;
                    this.sqlCommand.Connection = this.sqlConnection;

                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(this.sqlCommand))
                    {
                        sqlDataAdapter.Fill(dataSet);
                    }

                    return dataSet;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                this.CloseConnection();
            }
        }

        /// <summary>
        /// Gets the dataset by command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>Returns data set.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public DataSet GetDatasetByCommand(string commandText)
        {
            try
            {
                using (DataSet dataSet = new DataSet())
                {
                    dataSet.Locale = System.Globalization.CultureInfo.InvariantCulture;
                    this.sqlConnection.Open();

                    this.sqlCommand.CommandText = commandText;
                    this.sqlCommand.CommandType = CommandType.Text;
                    this.sqlCommand.Connection = this.sqlConnection;

                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(this.sqlCommand))
                    {
                        sqlDataAdapter.Fill(dataSet);
                    }

                    return dataSet;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                this.CloseConnection();
            }
        }

        #endregion

        #region Connection Methods

        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        public void CloseConnection()
        {
            if (this.sqlConnection.State != ConnectionState.Closed)
            {
                this.sqlConnection.Close();
            }
        }

        #endregion

        #endregion

        #region Dispose Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.sqlCommand != null)
                {
                    this.sqlCommand.Dispose();
                }

                if (this.sqlConnection != null)
                {
                    if (this.sqlConnection.State != ConnectionState.Closed)
                    {
                        this.sqlConnection.Close();
                    }

                    this.sqlConnection.Dispose();
                }
            }
        }

        #endregion
    }
}