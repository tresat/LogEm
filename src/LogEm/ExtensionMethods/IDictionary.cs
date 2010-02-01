using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace LogEm.ExtensionMethods
{
    public static class IDictionaryExtensions
    {
        /// <summary>
        /// Converts a Dictionary to a CSV dictionary string.
        /// </summary>
        /// <param name="me">The IDictionary to convert.</param>
        /// <returns>A string "key1=value1;key2=value2;".</returns>
        public static String ToCSVString(this IDictionary me)
        {
            StringBuilder result = new StringBuilder();

            foreach (String key in me.Keys)
            {
                result.Append(key).Append("=").Append(me[key]).Append(";");
            }

            if (result.Length > 0)
            {
                result.Remove(result.Length - 1, 1);
            }

            return result.ToString();
        }
    }
}
