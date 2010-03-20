using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
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

        /// <summary>
        /// Creates a link to a CSS file, to be included in the files HTML header.
        /// </summary>
        /// <param name="pCssName">Name of the css file or URL of the file.</param>
        /// <param name="pEmbedded">Is the css file an embedded resource.</param>
        /// <returns>Link HTML element w/ appropriate elements set.</returns>
        public static HtmlLink CreateLinkToCSSFile(String pCssName, Boolean pEmbedded) {
            HtmlLink cssLink = new HtmlLink();

            cssLink.Attributes.Add("rel", "stylesheet");
            cssLink.Attributes.Add("type", "text/css");
            if (pEmbedded)
            {
                cssLink.Attributes.Add("href", String.Format("{0}{1}/{2}", HtmlUtils.BaseLogEmUrl(), "stylesheet", pCssName.Split(new Char[] { '.' })[0]));
            }
            else
            {
                cssLink.Attributes.Add("href", pCssName);
            }

            return cssLink;
        }

        /// <summary>
        /// Creates a script to the JS file, to be included in the files HTML header.
        /// </summary>
        /// <param name="pJavaScriptName">Name of the js file or URL of the file.</param>
        /// <param name="pEmbedded">Is the js file an embedded resource.</param>
        /// <returns>Script HTML element w/ appropriate elements set.</returns>
        public static HtmlGenericControl CreateLinkToJavaScriptFile(String pJavaScriptName, Boolean pEmbedded) {
            HtmlGenericControl scriptFile = new HtmlGenericControl("script");
            
            scriptFile.Attributes.Add("type", "text/javascript");
            if (pEmbedded)
            {
                scriptFile.Attributes.Add("src", String.Format("{0}{1}/{2}", HtmlUtils.BaseLogEmUrl(), "javascript", pJavaScriptName.Split(new Char[] { '.' })[0]));
            }
            else
            {
                scriptFile.Attributes.Add("src", pJavaScriptName);
            }

            return scriptFile;
        }
    }
}
