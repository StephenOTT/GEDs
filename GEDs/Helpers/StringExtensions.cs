using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GEDs.Helpers
{
    public static class StringExtensions
    {
        public static string Elipse(this string s, int maxLength, string trail = "")
        {
            if (s.Length > maxLength)
            {
                return s.Substring(0, maxLength) + trail;
            }

            return s;
        }
    }
}