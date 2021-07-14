using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Assets.Scripts.Extensions
{
    public static class StringExtensions
    {
        public static T ParseEnum<T>(this string str)
        {
            return (T)System.Enum.Parse(typeof(T), str);
        }
        
        public static string Replace(this string input, Regex regex, string replacement)
        {
            return regex.Replace(input, replacement);
        }
    }
}
