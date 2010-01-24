using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LogEm
{
    /// <summary>
    /// HTTP handler factory that dispenses handlers for rendering views and 
    /// resources needed to display the error log.
    /// </summary>

    public class RequestLogModule : HttpModuleBase
    {
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
        /// Gets the <see cref="RequestLog"/> instance to which the module
        /// will log requests.
        /// </summary>

        protected virtual RequestLog GetRequestLog(HttpContext context)
        {
            return RequestLog.GetDefault(context);
        }

        protected virtual void OnRequest(object sender, EventArgs args)
        {
            HttpApplication application = (HttpApplication)sender;
            //LogRequest();
        }

        protected virtual void OnAuthenticate(object sender, EventArgs args)
        {
            HttpApplication application = (HttpApplication)sender;
            //LogAuthenticate();
        }
    }   
}
