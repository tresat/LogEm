using System;
using System.Collections.Specialized;
using System.Text;

namespace LogEm.ExtensionMethods
{
    public static class NameValueCollectionExtensions
    {
        /// <summary>
        /// Converts a NameValueCollection to a CSV dictionary string.
        /// </summary>
        /// <param name="me">The NameValueCollection to convert.</param>
        /// <returns>A string "key1=value1;key2=value2;".</returns>
        public static string ToCSVString(this NameValueCollection me)
        {
            StringBuilder result = new StringBuilder();

            foreach (String key in me.AllKeys)
            {
                result.Append(me).Append("=").Append(me[key]).Append(";");
            }

            if (result.Length > 0)
            {
                result.Remove(result.Length - 1, 1);
            }

            return result.ToString();
        }
    }
}
