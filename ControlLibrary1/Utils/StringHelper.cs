using System;
using System.Text.RegularExpressions;

namespace AiTest
{
    public static class StringHelper
    {
        public static string Unicode2String(string source)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                source, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }


        public static int IndexOf_Reverse(this string source, char c)
        {
            int j = -1;
            for (int i = source.Length - 1; i >= 0; i--)
            {
                if (source[i] == c)
                {
                    j = i;
                    break;
                }
            }
          
            return j;
        }

    }
}