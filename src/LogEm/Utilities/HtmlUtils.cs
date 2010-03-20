using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogEm.Utilities
{
    internal sealed class HtmlUtils
    {
        /// <summary>
        /// Gets the base path to logEm HTTP handler in the current HTTP context,
        /// including trailing "/".
        /// </summary>
        /// <returns>"http://<host/>/<application/>/logEm.axd/"</returns>
        public static string BaseLogEmUrl()
        {
            String[] urlParts = HttpContext.Current.Request.Url.Segments;
            StringBuilder sb = new StringBuilder();

            int idx = 0;
            while (idx < urlParts.Length && !urlParts[idx].ToUpperInvariant().StartsWith("LOGEM.AXD")) {
                sb.Append(urlParts[idx]);
                idx++;
            }
            sb.Append("logEm.axd/");

            return sb.ToString();
        }
    }
}
