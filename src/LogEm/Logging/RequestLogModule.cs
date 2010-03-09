using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using LogEm.Logging;
using LogEm.Logging.RequestLogs;

namespace LogEm.Logging
{
    /// <summary>
    /// HTTP handler factory that dispenses handlers for rendering views and 
    /// resources needed to display the error log.
    /// </summary>

    public class RequestLogModule : HttpModuleBase
    {
        #region "Constants"
        protected const String LOGEM_URL = "LOGEM.AXD";
        #endregion

        #region "Application Event Handling"
        /// <summary>
        /// Initializes the module and prepares it to handle requests.
        /// </summary>
        protected override void OnInit(HttpApplication application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            application.BeginRequest += new EventHandler(OnRequest);
            application.AuthenticateRequest += new EventHandler(OnAuthenticate);
        }

        /// <summary>
        /// A request has come in for logem.axd, or an embedded resource like
        /// a stylesheet.
        /// </summary>
        protected virtual void OnRequest(object sender, EventArgs args)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");

            // Don't log logEm requests
            HttpApplication application = (HttpApplication)sender;
            if (!IsLogEmRequest(application.Request.Url))
                LogRequest(application.Context);
        }

        /// <summary>
        /// A user has just been authorized for the application.
        /// </summary>
        protected virtual void OnAuthenticate(object sender, EventArgs args)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");

            // Don't log logEm requests
            HttpApplication application = (HttpApplication)sender;
            if (!IsLogEmRequest(application.Request.Url))
                LogAuthentication(application.Context);
        }
        #endregion

        #region "Protected Helpers"
        /// <summary>
        /// Check if the request is for LogEm, if so, ignore.
        /// </summary>
        /// <param name="pUrl">The request Uri.</param>
        /// <returns><c>true/false</c> for whether or not the request is for logEm.</returns>
        protected virtual Boolean IsLogEmRequest(Uri pUrl)
        {
            if (pUrl == null)
                throw new ArgumentNullException("pUrl");

            foreach (String seg in pUrl.Segments)
            {
                if (seg.ToUpper().StartsWith(LOGEM_URL, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the <see cref="RequestLogBase"/> instance to which the module
        /// will log requests.
        /// </summary>
        protected virtual RequestLogBase GetRequestLog(HttpContext pContext)
        {
            return RequestLogBase.GetLog(pContext);
        }

        /// <summary>
        /// Logs a request for a resource and its context to the error log.
        /// </summary>
        protected virtual void LogRequest(HttpContext pContext)
        {
            if (pContext == null)
                throw new ArgumentNullException("context");

            // Get the log
            RequestLogBase log = GetRequestLog(pContext);

            // Log the ResourceRequest
            log.LogResourceRequest(pContext);
        }

        /// <summary>
        /// Logs a user logging in to the application.
        /// </summary>
        protected virtual void LogAuthentication(HttpContext pContext)
        {
            if (pContext == null)
                throw new ArgumentNullException("context");

            // Get the log
            RequestLogBase log = GetRequestLog(pContext);

            // Log new sessions only
            if (log.IsNewSession(pContext.Session != null ? pContext.Session.SessionID : pContext.Request.Params["ASP.Net_SessionId"]))
            {
                // Log the session
                log.LogSession(pContext);
            }
        }
        #endregion
    }
}