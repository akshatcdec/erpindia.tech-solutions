using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace ERPIndia.Class.Helper
{

    /// <summary>
    /// Helper class for loading dropdown data from database
    /// </summary>
    public static class DropdownHelper
    {

        /// <summary>
        /// Generic model for dropdown items
        /// </summary>
        public class DropdownItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
            public int Count { get; set; }
            public int SortOrder { get; set; }
            public bool IsDefault { get; set; }
        }



        /// <summary>
        /// Gets a SelectList populated from the database for the specified list type
        /// </summary>
        /// <param name="listType">Type of list to retrieve (e.g., "Sections", "FeeCategories")</param>
        /// <param name="sessionId">Current session ID</param>
        /// <param name="tenantId">Current tenant ID</param>
        /// <param name="activeOnly">Whether to include only active items</param>
        /// <param name="includeCount">Whether to include count in the display text</param>
        /// <returns>SelectList populated with database values or hardcoded fallback</returns>
        public static SelectList GetDropdownList(string listType, Guid sessionId, Guid tenantId, bool activeOnly = true, bool includeCount = false)
        {
            try
            {
                // Get data from database
                var items = GetDropdownItemsFromDb(listType, sessionId, tenantId, activeOnly);

                if (items != null && items.Any())
                {
                    // Format text based on whether to include count
                    var formattedItems = items.Select(item => new
                    {
                        Value = item.Value,
                        Text = includeCount ? $"{item.Text} ({item.Count})" : item.Text
                    });

                    return new SelectList(formattedItems, "Value", "Text");
                }
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error loading {listType} dropdown: {ex.Message}");
            }

            // Fallback to hardcoded values
            return GetHardcodedSelectList(listType);
        }

        /// <summary>
        /// Gets all dropdown data for the specified session and tenant in a single database call
        /// </summary>
        /// <param name="sessionId">Current session ID</param>
        /// <param name="tenantId">Current tenant ID</param>
        /// <param name="activeOnly">Whether to include only active items</param>
        /// <returns>Dictionary of dropdown lists by type</returns>
        public static Dictionary<string, SelectList> GetAllDropdownLists(Guid sessionId, Guid tenantId, bool activeOnly = true)
        {
            var result = new Dictionary<string, SelectList>();

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Add each dropdown list to the dictionary
                    result.Add("Sections", GetSectionsDropdown(connection, sessionId, tenantId, activeOnly));
                    result.Add("FeeCategories", GetFeeCategoriesDropdown(connection, sessionId, tenantId, activeOnly));
                    result.Add("FeeDiscounts", GetFeeDiscountsDropdown(connection, sessionId, tenantId, activeOnly));
                    result.Add("FeeHeads", GetFeeHeadsDropdown(connection, sessionId, tenantId, activeOnly));

                    // Add other dropdown lists as needed
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading all dropdowns: {ex.Message}");

                // Add fallback values for critical dropdowns
                result.Add("Sections", GetHardcodedSelectList("Sections"));
                result.Add("FeeCategories", GetHardcodedSelectList("Categories"));
                result.Add("FeeDiscounts", GetHardcodedSelectList("Discounts"));
                result.Add("FeeHeads", GetHardcodedSelectList("FeeHeads"));
            }

            return result;
        }



        #region Private Helper Methods

        /// <summary>
        /// Gets dropdown items from the database based on list type
        /// </summary>
        private static List<DropdownItem> GetDropdownItemsFromDb(string listType, Guid sessionId, Guid tenantId, bool activeOnly)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                switch (listType.ToLower())
                {
                    case "sections":
                    case "academicsections":
                        return GetSectionItems(connection, sessionId, tenantId, activeOnly);

                    case "feecategories":
                    case "categories":
                        return GetFeeCategoryItems(connection, sessionId, tenantId, activeOnly);

                    case "feediscounts":
                    case "discounts":
                        return GetFeeDiscountItems(connection, sessionId, tenantId, activeOnly);

                    case "feeheads":
                    case "heads":
                        return GetFeeHeadItems(connection, sessionId, tenantId, activeOnly);

                    // Add other cases as needed

                    default:
                        return new List<DropdownItem>();
                }
            }
        }

        /// <summary>
        /// Gets section items from the database
        /// </summary>
        private static List<DropdownItem> GetSectionItems(SqlConnection connection, Guid sessionId, Guid tenantId, bool activeOnly)
        {
            string sql = @"
                SELECT 
                    SectionID AS Value, 
                    SectionName AS Text,
                    SortOrder,
                    (SELECT COUNT(*) FROM StudentSection ss 
                     WHERE ss.SectionID = s.SectionID 
                     AND ss.IsDeleted = 0) AS Count,
                    0 AS IsDefault
                FROM AcademicSectionMaster s
                WHERE s.TenantID = @TenantID
                AND s.SessionID = @SessionID
                AND s.IsDeleted = 0
                " + (activeOnly ? "AND s.IsActive = 1" : "") + @"
                ORDER BY s.SortOrder, s.SectionName";

            var parameters = new { TenantID = tenantId, SessionID = sessionId };
            return connection.Query<DropdownItem>(sql, parameters).ToList();
        }

        /// <summary>
        /// Gets fee category items from the database
        /// </summary>
        private static List<DropdownItem> GetFeeCategoryItems(SqlConnection connection, Guid sessionId, Guid tenantId, bool activeOnly)
        {
            string sql = @"
                SELECT 
                    FeeCategoryID AS Value, 
                    CategoryName AS Text,
                    SortOrder,
                    (SELECT COUNT(*) FROM FeeStructure fs 
                     WHERE fs.FeeCategoryID = fc.FeeCategoryID 
                     AND fs.IsDeleted = 0) AS Count,
                    0 AS IsDefault
                FROM FeeCategoryMaster fc
                WHERE fc.TenantID = @TenantID
                AND fc.SessionID = @SessionID
                AND fc.IsDeleted = 0
                " + (activeOnly ? "AND fc.IsActive = 1" : "") + @"
                ORDER BY fc.SortOrder, fc.CategoryName";

            var parameters = new { TenantID = tenantId, SessionID = sessionId };
            return connection.Query<DropdownItem>(sql, parameters).ToList();
        }

        /// <summary>
        /// Gets fee discount items from the database
        /// </summary>
        private static List<DropdownItem> GetFeeDiscountItems(SqlConnection connection, Guid sessionId, Guid tenantId, bool activeOnly)
        {
            string sql = @"
                SELECT 
                    FeeDiscountID AS Value, 
                    DiscountName AS Text,
                    SortOrder,
                    (SELECT COUNT(*) FROM FeeStructure fs 
                     WHERE fs.FeeDiscountID = fd.FeeDiscountID 
                     AND fs.IsDeleted = 0) AS Count,
                    0 AS IsDefault
                FROM FeeDiscountMaster fd
                WHERE fd.TenantID = @TenantID
                AND fd.SessionID = @SessionID
                AND fd.IsDeleted = 0
                " + (activeOnly ? "AND fd.IsActive = 1" : "") + @"
                ORDER BY fd.SortOrder, fd.DiscountName";

            var parameters = new { TenantID = tenantId, SessionID = sessionId };
            return connection.Query<DropdownItem>(sql, parameters).ToList();
        }

        /// <summary>
        /// Gets fee head items from the database
        /// </summary>
        private static List<DropdownItem> GetFeeHeadItems(SqlConnection connection, Guid sessionId, Guid tenantId, bool activeOnly)
        {
            string sql = @"
                SELECT 
                    FeeHeadsID AS Value, 
                    HeadsName AS Text,
                    SortOrder,
                    (SELECT COUNT(*) FROM FeeStructureDetail fsd 
                     WHERE fsd.FeeHeadsID = fh.FeeHeadsID 
                     AND fsd.IsDeleted = 0) AS Count,
                    0 AS IsDefault
                FROM FeeHeadsMaster fh
                WHERE fh.TenantID = @TenantID
                AND fh.SessionID = @SessionID
                AND fh.IsDeleted = 0
                " + (activeOnly ? "AND fh.IsActive = 1" : "") + @"
                ORDER BY fh.SortOrder, fh.HeadsName";

            var parameters = new { TenantID = tenantId, SessionID = sessionId };
            return connection.Query<DropdownItem>(sql, parameters).ToList();
        }

        /// <summary>
        /// Gets a SelectList for sections
        /// </summary>
        private static SelectList GetSectionsDropdown(SqlConnection connection, Guid sessionId, Guid tenantId, bool activeOnly)
        {
            var items = GetSectionItems(connection, sessionId, tenantId, activeOnly);
            return new SelectList(items.Select(i => new { Value = i.Value, Text = i.Text }), "Value", "Text");
        }

        /// <summary>
        /// Gets a SelectList for fee categories
        /// </summary>
        private static SelectList GetFeeCategoriesDropdown(SqlConnection connection, Guid sessionId, Guid tenantId, bool activeOnly)
        {
            var items = GetFeeCategoryItems(connection, sessionId, tenantId, activeOnly);
            return new SelectList(items.Select(i => new { Value = i.Value, Text = i.Text }), "Value", "Text");
        }

        /// <summary>
        /// Gets a SelectList for fee discounts
        /// </summary>
        private static SelectList GetFeeDiscountsDropdown(SqlConnection connection, Guid sessionId, Guid tenantId, bool activeOnly)
        {
            var items = GetFeeDiscountItems(connection, sessionId, tenantId, activeOnly);
            return new SelectList(items.Select(i => new { Value = i.Value, Text = i.Text }), "Value", "Text");
        }

        /// <summary>
        /// Gets a SelectList for fee heads
        /// </summary>
        private static SelectList GetFeeHeadsDropdown(SqlConnection connection, Guid sessionId, Guid tenantId, bool activeOnly)
        {
            var items = GetFeeHeadItems(connection, sessionId, tenantId, activeOnly);
            return new SelectList(items.Select(i => new { Value = i.Value, Text = i.Text }), "Value", "Text");
        }

        /// <summary>
        /// Gets hardcoded lists as fallback
        /// </summary>
        private static SelectList GetHardcodedSelectList(string listType)
        {
            switch (listType)
            {
                case "Sections":
                    return new SelectList(new[] {
                        new { Value = "A", Text = "A" },
                        new { Value = "B", Text = "B" }
                    }, "Value", "Text");

                case "Categories":
                    return new SelectList(new[] {
                        new { Value = "OBC", Text = "OBC" },
                        new { Value = "BC", Text = "BC" }
                    }, "Value", "Text");

                case "Discounts":
                    return new SelectList(new[] {
                        new { Value = "Sibling", Text = "Sibling Discount" },
                        new { Value = "Staff", Text = "Staff Discount" }
                    }, "Value", "Text");

                case "FeeHeads":
                    return new SelectList(new[] {
                        new { Value = "Tuition", Text = "Tuition Fee" },
                        new { Value = "Library", Text = "Library Fee" }
                    }, "Value", "Text");

                // Add other cases as needed

                default:
                    return new SelectList(new[] { new { Value = "", Text = "No data available" } }, "Value", "Text");
            }
        }

    }
}
#endregion


