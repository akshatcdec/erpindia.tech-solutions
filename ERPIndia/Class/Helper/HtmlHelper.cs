using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERPIndia.Class.Helper
{
        public static class DateTimeExtensions
        {
            public static string ToHtmlDateString(this DateTime? date)
            {
                return date?.ToString("yyyy-MM-dd") ?? string.Empty;
            }
        }
    
}