using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer
{
    public static class StringExtension
    {
        public static bool ContainsAny(this string str, string[] values)
        {
            foreach (string v in values)
                if (str.Contains(v))
                    return true;
            return false;
        }
    }
}
