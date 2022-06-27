using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Extensions
{
    public static class StringExtensions
    {
        public static string MakeFirstCharCapital(this string str)
        {
            if (String.IsNullOrEmpty(str)) return str;

            var firstCharToUpper = str.Take(1).First().ToString().ToUpper();

            if (str.Count() == 1) return firstCharToUpper;

            var remaingStr = String.Join("", str.Skip(1).ToList());

            return firstCharToUpper + remaingStr;
        }

          public static bool EqualsIgnoreCase(this string str, string str2)
        {
            if (String.IsNullOrEmpty(str) && String.IsNullOrEmpty(str2)) return true;
            if (String.IsNullOrEmpty(str) || String.IsNullOrEmpty(str2)) return false;
            return str.Equals(str2, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}