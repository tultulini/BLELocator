using System.Net;
using System.Text.RegularExpressions;

namespace BLELocator.Core.Utils
{
    public static class StringExtensions
    {
        readonly static Regex RegexMacAddress = new Regex(@"^[0-9a-fA-F]{2}(((:[0-9a-fA-F]{2}){5})|((:[0-9a-fA-F]{2}){5}))$");
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }
        public static bool HasSomething(this string value)
        {
            return !value.IsNullOrEmpty();
        }
        public static bool IsValidMacAddress(this string value)
        {
            return value.HasSomething() && RegexMacAddress.IsMatch(value);
        }
        public static bool IsValidIP(this string value)
        {
            IPAddress ip;
            return value.HasSomething() && IPAddress.TryParse(value,out ip);
        }
    }
}