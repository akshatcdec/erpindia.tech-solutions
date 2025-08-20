using System;
using System.Globalization;
using System.Threading;

namespace ERPIndia.Class.Helper
{
    /// <summary>
    /// SQL helper class
    /// </summary>
    public static class SqlHelper
    {
        #region DB Value Conversion Methods

        /// <summary>
        /// Gets the DB long value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// Returns long value
        /// </returns>
        public static long GetDBLongValue(object value, long defaultValue = default(long))
        {
            return Convert.IsDBNull(value) ? defaultValue : Convert.ToInt64(value);
        }

        /// <summary>
        /// Gets the DB integer value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// Returns integer value
        /// </returns>
        public static int GetDBIntValue(object value, int defaultValue = default(int))
        {
            return Convert.IsDBNull(value) ? defaultValue : Convert.ToInt32(value);
        }

        /// <summary>
        /// Gets the DB string value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// Returns string value
        /// </returns>
        public static string GetDBStringValue(object value, string defaultValue = default(string))
        {
            return Convert.IsDBNull(value) ? defaultValue : Convert.ToString(value);
        }

        /// <summary>
        /// Gets the DB GUID value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// Returns GUID value
        /// </returns>
        public static Guid GetDBGuidValue(object value, Guid defaultValue = default(Guid))
        {
            return Convert.IsDBNull(value) ? defaultValue : (Guid)value;
        }

        /// <summary>
        /// Gets the DB boolean value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns>
        /// Returns boolean value
        /// </returns>
        public static bool GetDBBoolValue(object value, bool defaultValue = default(bool))
        {
            return Convert.IsDBNull(value) ? defaultValue : Convert.ToBoolean(value);
        }

        /// <summary>
        /// Gets the DB decimal value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="roundNum">The round number.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// Returns decimal value
        /// </returns>
        public static decimal GetDBDecimalValue(object value, int roundNum = 2, decimal defaultValue = default(decimal))
        {
            return Convert.IsDBNull(value) ? Math.Round(defaultValue, roundNum) : Math.Round(Convert.ToDecimal(value), roundNum);
        }

        /// <summary>
        /// Gets the DB double value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="roundNum">The round number.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// Returns double value
        /// </returns>
        public static double GetDBDoubleValue(object value, int roundNum = 2, double defaultValue = default(double))
        {
            return Convert.IsDBNull(value) ? Math.Round(defaultValue, roundNum) : Math.Round(Convert.ToDouble(value), roundNum);
        }

        /// <summary>
        /// Gets the database date time value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns date time value.</returns>
        public static DateTime GetDBDateTimeValue(object value, DateTime defaultValue = default(DateTime))
        {
            if (value == null || Convert.IsDBNull(value))
            {
                return defaultValue;
            }
            else
            {
                try
                {
                    return System.DateTime.Parse(value.ToString());
                }
                catch
                {
                    return defaultValue;
                }
            }
        }

        /// <summary>
        /// Gets the DB time span.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// Returns timespan value
        /// </returns>
        public static TimeSpan GetDBTimeSpan(object value, TimeSpan defaultValue = default(TimeSpan))
        {
            return Convert.IsDBNull(value) ? defaultValue : (TimeSpan)value;
        }

        /// <summary>
        /// Gets the DB short value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// Returns short value
        /// </returns>
        public static short GetDBShortValue(object value, short defaultValue = default(short))
        {
            return Convert.IsDBNull(value) ? defaultValue : Convert.ToInt16(value);
        }

        /// <summary>
        /// Gets the DB byte array value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// Returns byte array
        /// </returns>
        public static byte[] GetDBByteArrayValue(object value)
        {
            return Convert.IsDBNull(value) ? null : (byte[])value;
        }

        #endregion

        #region Parsing Methods

        /// <summary>
        /// Parses the native integer.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns integer.</returns>
        public static int ParseNativeInt(string value)
        {
            int ni;
            int.TryParse(value, NumberStyles.Integer, Thread.CurrentThread.CurrentUICulture, out ni);
            return ni;
        }

        /// <summary>
        /// Parses the native long.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns long.</returns>
        public static long ParseNativeLong(string value)
        {
            long nl;
            long.TryParse(value, NumberStyles.Integer, Thread.CurrentThread.CurrentUICulture, out nl);
            return nl;
        }

        /// <summary>
        /// Parses the native decimal.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns decimal.</returns>
        public static decimal ParseNativeDecimal(string value)
        {
            decimal nd;
            decimal.TryParse(value, NumberStyles.Number, Thread.CurrentThread.CurrentUICulture, out nd);
            return nd;
        }

        /// <summary>
        /// Parses the native date time.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns date time</returns>
        public static DateTime ParseNativeDateTime(string value)
        {
            DateTime ndt;
            System.DateTime.TryParse(value, out ndt);
            return ndt;
        }

        /// <summary>
        /// Parses the boolean.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Returns boolean</returns>
        public static bool ParseBoolean(string value)
        {
            bool nb;
            bool.TryParse(value, out nb);
            return nb;
        }

        #endregion
    }
}
