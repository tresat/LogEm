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
            const String LOGEM_URL = "LOGEM.AXD";

            if (sender == null)
                throw new ArgumentNullException("sender");

            // Check each URL segment, if one of them matches logem.axd
            // then we DON'T need to log it, since it's a request to LOGEM
            HttpApplication application = (HttpApplication)sender;
            foreach (String seg in application.Request.Url.Segments)
            {
                if (seg.ToUpper().StartsWith(LOGEM_URL, StringComparison.InvariantCultureIgnoreCase))
                {
                    return;
                }
            }

            LogRequest(application.Context);
        }

        /// <summary>
        /// A user has just been authorized for the application.
        /// </summary>
        protected virtual void OnAuthenticate(object sender, EventArgs args)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");

            HttpApplication application = (HttpApplication)sender;
            LogAuthentication(application.Context);
        }
#endregion

#region "Protected Helpers"
        /// <summary>
        /// Gets the <see cref="RequestLog"/> instance to which the module
        /// will log requests.
        /// </summary>
        protected virtual RequestLog GetRequestLog(HttpContext context)
        {
            return RequestLog.GetLog(context);
        }

        /// <summary>
        /// Logs a request for a resource and its context to the error log.
        /// </summary>
        protected virtual void LogRequest(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            // Get the log
            RequestLog log = GetRequestLog(context);

            // Create the ResourceRequest object
            ResourceRequestBase request = log.CreateNewResourceRequest(context);

            // Log the user request
            string id = log.Log(request);
        }

        /// <summary>
        /// Logs a user logging in to the application.
        /// </summary>
        protected virtual void LogAuthentication(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            // Create the UserAuthentication object
            //UserRequest request = new UserRequest();
            //request.Application = 

            // Log the user request
            RequestLog log = GetRequestLog(context);

            //context.Session.SessionID;
        }
#endregion
    }
}