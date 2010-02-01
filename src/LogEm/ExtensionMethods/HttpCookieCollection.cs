using System;
using System.Web;
using System.Text;

namespace LogEm.ExtensionMethods
{
    public static class HttpCookieCollectionExtensions
    {
        /// <summary>
        /// Converts a HttpCookie collection to a CSV dictionary string.
        /// </summary>
        /// <param name="me">The HttpCookieCollection to convert.</param>
        /// <returns>A string "key1=value1;key2=value2;".</returns>
        public static string ToCSVString(this HttpCookieCollection me)
        {
            StringBuilder result = new StringBuilder();

            foreach (String key in me.AllKeys)
            {
                result.Append(key).Append("=").Append(me[key].Value).Append(";");
            }

            if (result.Length > 0) 
            {
                result.Remove(result.Length - 1, 1);
            }

            return result.ToString();
        }
    }
}