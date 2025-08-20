using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ERPIndia
{
    public static class DateHelper
    {
        // Add or reorder formats to suit your data
        private static readonly string[] _formats = new[]
        {
        "dd-MMM-yyyy",   // 26-Apr-2025
        "yyyy-MM-dd",    // 2025-04-26
        "dd/MM/yyyy",    // 26/04/2025
        "M/d/yyyy",      // 4/26/2025
        "d-MMM-yy",      // 1-Jan-25
        "yyyyMMdd"       // 20250426
    };

        /// <summary>
        /// Safely converts any string to a nullable DateTime.
        /// Returns <c>null</c> when the input is blank or unrecognised.
        /// </summary>
        public static DateTime? ParseOrNull(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            DateTime parsed;

            // 1️⃣  First try the explicit format list (fast & culture-independent)
            if (DateTime.TryParseExact(text,
                                       _formats,
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.None,
                                       out parsed))
            {
                return (DateTime?)parsed;          // explicit cast needed in C# 7.3
            }

            // 2️⃣  Fallback to the current culture’s default parsing
            return DateTime.TryParse(text, out parsed) ? (DateTime?)parsed : null;
        }

        /// <summary>
        /// Same idea, but always returns a non-nullable DateTime.
        /// When parsing fails you get <see cref="DateTime.MinValue"/>.
        /// </summary>
        public static DateTime ParseOrMin(string text)
        {
            DateTime? temp = ParseOrNull(text);
            return temp ?? DateTime.MinValue;      // no CS8370 error in 7.3
        }
    }

    public static class Utils
    {

        public static DataTable ConvertToDataTable<T>(IEnumerable<T> data)
        {
            var properties = typeof(T).GetProperties();
            var table = new DataTable();

            foreach (var property in properties)
            {
                table.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            foreach (var item in data)
            {
                var row = table.NewRow();
                foreach (var property in properties)
                {
                    row[property.Name] = property.GetValue(item) ?? DBNull.Value;
                }
                table.Rows.Add(row);
            }

            return table;
        }
        public static string ToStringOrEmpty(this object value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.ToString();
        }
        public static DateTime? ConvertToDateOrNull(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return null;

            string[] formats = {
            "dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy",
            "d/M/yyyy", "d-M-yyyy", "d.M.yyyy",
            "dd/M/yyyy", "dd-M-yyyy", "dd.M.yyyy",
            "d/MM/yyyy", "d-MM-yyyy", "d.MM.yyyy"
        };

            if (DateTime.TryParseExact(
                dateStr,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime result
            ))
            {
                return result;
            }

            return null;
        }
        public static DateTime? SplitAndConvertDate(string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return null;

            var parts = dateStr.Split(new char[] { '/', '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                return null;

            if (int.TryParse(parts[0], out int day) &&
                int.TryParse(parts[1], out int month) &&
                int.TryParse(parts[2], out int year))
            {
                try
                {
                    return new DateTime(year, month, day);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
        public static Guid ParseGuid(string id)
        {
            // First check if string is null or empty
            if (string.IsNullOrEmpty(id))
            {
                return Guid.Empty;
            }

            // Then check if the string is a valid GUID
            if (Guid.TryParse(id, out Guid result))
            {
                return result;
            }

            // If not a valid GUID, return empty GUID
            return Guid.Empty;
        }
        public static Guid ParseGuid(Guid? id)
        {
            // If id is null or Guid.Empty, return empty Guid
            if (!id.HasValue || id.Value == Guid.Empty)
            {
                return Guid.Empty;
            }

            // Since it's already a Guid? (not a string), just return its value
            return id.Value;
        }

        public static int ParseInt(string value, int defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (int.TryParse(value, out int result))
                return result;

            return defaultValue;
        }

        public static decimal ParseDecimal(string value, decimal defaultValue = 0)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            if (decimal.TryParse(value, out decimal result))
                return result;

            return defaultValue;
        }
        public static string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Use TextInfo to properly handle title casing based on culture
            System.Globalization.TextInfo textInfo = new System.Globalization.CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }
        public static int ParseInt(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return 0;
                }
                string str = obj.ToString();
                if (string.IsNullOrEmpty(str))
                {
                    return 0;
                }
                return (int)decimal.Parse(str);
            }
            catch
            {
                return 0;
            }
        }
        public static List<T> ConvertDataTableToListWithHeaderRow<T>(DataTable dt) where T : new()
        {
            List<T> data = new List<T>();

            // Create a dictionary to store property info with case-insensitive keys
            Dictionary<string, PropertyInfo> properties = typeof(T).GetProperties()
                .ToDictionary(p => p.Name.ToLower(), p => p);

            // Get headers from the first row
            Dictionary<int, string> headers = new Dictionary<int, string>();
            if (dt.Rows.Count > 0)
            {
                DataRow headerRow = dt.Rows[0];
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    headers.Add(i, headerRow[i].ToString());
                }

                // Process data rows (skip the header row)
                for (int rowIndex = 1; rowIndex < dt.Rows.Count; rowIndex++)
                {
                    DataRow row = dt.Rows[rowIndex];
                    T item = new T();

                    for (int colIndex = 0; colIndex < dt.Columns.Count; colIndex++)
                    {
                        if (headers.ContainsKey(colIndex))
                        {
                            string headerName = headers[colIndex];
                            // Convert header name to lowercase for case-insensitive matching
                            string propertyNameLower = headerName.ToLower();

                            if (properties.ContainsKey(propertyNameLower) && row[colIndex] != DBNull.Value)
                            {
                                PropertyInfo prop = properties[propertyNameLower];
                                try
                                {
                                    // Handle type conversion
                                    object value = row[colIndex];

                                    // Handle different types appropriately
                                    if (prop.PropertyType == typeof(string))
                                    {
                                        prop.SetValue(item, value.ToString(), null);
                                    }
                                    else if (prop.PropertyType.IsEnum)
                                    {
                                        prop.SetValue(item, Enum.Parse(prop.PropertyType, value.ToString()), null);
                                    }
                                    else
                                    {
                                        prop.SetValue(item, Convert.ChangeType(value, prop.PropertyType), null);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // Log exception or set default value
                                    Console.WriteLine($"Error mapping {headerName}: {ex.Message}");
                                }
                            }
                        }
                    }

                    data.Add(item);
                }
            }

            return data;
        }
        public static string GenerateJsonFromDataTable(DataTable dt_)
        {
            // If you want proper JSON, use Newtonsoft.Json instead of manual string building
            if (dt_ == null || dt_.Rows.Count == 0)
            {
                return "[]";
            }

            StringBuilder finalBuilder = new StringBuilder();
            finalBuilder.Append("[");

            // Create a new table with headers from first row
            DataTable dt = new DataTable();
            for (int i = 0; i < dt_.Columns.Count; i++)
            {
                dt.Columns.Add(dt_.Rows[0][i].ToString());
            }

            // Copy data starting from second row
            for (int row_ = 1; row_ < dt_.Rows.Count; row_++)
            {
                DataRow row = dt.NewRow();
                for (int col = 0; col < dt_.Columns.Count; col++)
                {
                    row[col] = dt_.Rows[row_][col].ToString();
                }
                dt.Rows.Add(row);
            }

            string[] columnNames = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            bool firstRow = true;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // Check if row has a valid AdmsnNo
                var admno = dt.Rows[i]["AdmsnNo"].ToString();
                if (string.IsNullOrEmpty(admno))
                {
                    continue;
                }

                // Add comma if not first row
                if (!firstRow)
                {
                    finalBuilder.Append(",");
                }
                else
                {
                    firstRow = false;
                }

                StringBuilder rowBuilder = new StringBuilder();
                rowBuilder.Append("{");

                bool firstColumn = true;
                foreach (var item in columnNames)
                {
                    var itemValue = dt.Rows[i][item].ToString();

                    // Skip empty values to reduce JSON size
                    if (!string.IsNullOrEmpty(itemValue))
                    {
                        // Add comma if not first column
                        if (!firstColumn)
                        {
                            rowBuilder.Append(",");
                        }
                        else
                        {
                            firstColumn = false;
                        }

                        // For numeric values, don't use quotes
                        bool isNumeric = double.TryParse(itemValue, out _);
                        if (isNumeric)
                        {
                            rowBuilder.AppendFormat("\"{0}\":{1}", item, itemValue);
                        }
                        // For boolean values, don't use quotes
                        else if (bool.TryParse(itemValue, out bool boolResult))
                        {
                            rowBuilder.AppendFormat("\"{0}\":{1}", item, itemValue.ToLower());
                        }
                        // For everything else, use quotes (strings)
                        else
                        {
                            // Escape special characters in strings
                            string escapedValue = itemValue
                                .Replace("\\", "\\\\")
                                .Replace("\"", "\\\"")
                                .Replace("\n", "\\n")
                                .Replace("\r", "\\r")
                                .Replace("\t", "\\t");

                            rowBuilder.AppendFormat("\"{0}\":\"{1}\"", item, escapedValue);
                        }
                    }
                }

                rowBuilder.Append("}");

                // Only add this row if it has content
                if (rowBuilder.Length > 2) // "{}" is length 2
                {
                    finalBuilder.Append(rowBuilder);
                }
            }

            finalBuilder.Append("]");
            return finalBuilder.ToString();
        }

    }
}