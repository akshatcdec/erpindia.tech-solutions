using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ERPIndia.Class.Helper
{
    public static class IndianCurrencyConverter
    {
        /// <summary>
        /// Converts a decimal amount to words in Indian currency format
        /// </summary>
        /// <param name="amount">The amount to convert</param>
        /// <returns>The amount in words (e.g., "Six Thousand Seven Hundred Rupees and Fifty Paise only")</returns>
        public static string ConvertToWords(decimal amount)
        {
            try
            {
                // Handle negative amounts
                bool isNegative = false;
                if (amount < 0)
                {
                    isNegative = true;
                    amount = Math.Abs(amount);
                }

                // Round to 2 decimal places
                amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);

                // Extract rupees and paise
                int rupees = (int)amount;
                int paise = (int)((amount - rupees) * 100);

                StringBuilder result = new StringBuilder();

                // Add negative indicator if necessary
                if (isNegative)
                    result.Append("Negative ");

                // Convert rupees to words
                if (rupees > 0)
                {
                    result.Append(ConvertAmountToIndianWords(rupees));
                    result.Append(" Rupee");

                    // Add plural 's' if more than one rupee
                    if (rupees != 1)
                        result.Append("s");
                }

                // Convert paise to words if applicable
                if (paise > 0)
                {
                    // If we have both rupees and paise, add the conjunction
                    if (rupees > 0)
                        result.Append(" and ");

                    result.Append(ConvertAmountToIndianWords(paise));
                    result.Append(" Paise");
                }

                // If the amount is zero (both rupees and paise)
                if (rupees == 0 && paise == 0)
                    result.Append("Zero Rupees");

                // Add 'only' at the end
                result.Append(" only");

                return result.ToString();
            }
            catch (Exception ex)
            {
                // In case of any error, return a default message
                return $"Error converting amount: {ex.Message}";
            }
        }

        /// <summary>
        /// Converts a number to words using Indian numbering system (with lakhs and crores)
        /// </summary>
        /// <param name="number">The number to convert</param>
        /// <returns>The number in words</returns>
        private static string ConvertAmountToIndianWords(int number)
        {
            if (number == 0)
                return "Zero";

            // Arrays for Indian number system denominations
            string[] units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                              "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            StringBuilder words = new StringBuilder();

            // Handle Crores (10,000,000)
            if (number >= 10000000)
            {
                words.Append(ConvertAmountToIndianWords(number / 10000000)).Append(" Crore ");
                number %= 10000000;
            }

            // Handle Lakhs (100,000)
            if (number >= 100000)
            {
                words.Append(ConvertAmountToIndianWords(number / 100000)).Append(" Lakh ");
                number %= 100000;
            }

            // Handle Thousands
            if (number >= 1000)
            {
                words.Append(ConvertAmountToIndianWords(number / 1000)).Append(" Thousand ");
                number %= 1000;
            }

            // Handle Hundreds
            if (number >= 100)
            {
                words.Append(ConvertAmountToIndianWords(number / 100)).Append(" Hundred ");
                number %= 100;
            }

            // Handle Tens and Units
            if (number > 0)
            {
                // Add "and" for amounts less than 100 but greater than 0
                if (words.Length > 0)
                    words.Append("and ");

                // Handle numbers 1-19
                if (number < 20)
                {
                    words.Append(units[number]);
                }
                // Handle numbers 20-99
                else
                {
                    words.Append(tens[number / 10]);
                    if (number % 10 > 0)
                        words.Append("-").Append(units[number % 10]);
                }
            }

            return words.ToString().Trim();
        }
    }
}