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
        public event RequestLoggedEventHandler Logged;

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
            LogRequest(application.Context);
        }

        protected virtual void OnAuthenticate(object sender, EventArgs args)
        {
            HttpApplication application = (HttpApplication)sender;
            //LogAuthenticate();
        }
        /// <summary>
        /// Raises the <see cref="Logged"/> event.
        /// </summary>

        protected virtual void OnLogged(RequestLoggedEventArgs args)
        {
            RequestLoggedEventHandler handler = Logged;

            if (handler != null)
                handler(this, args);
        }

        /// <summary>
        /// Logs an exception and its context to the error log.
        /// </summary>

        protected virtual void LogRequest(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            //
            // Log away...
            //
            RequestLog log = GetRequestLog(context);
            UserRequest request = new UserRequest();
            string id = log.Log(request);
            RequestLogEntry entry = new RequestLogEntry(log, id, request);

            if (entry != null)
                OnLogged(new RequestLoggedEventArgs(entry));
        }
    }

    public delegate void RequestLoggedEventHandler(object sender, RequestLoggedEventArgs args);

    [Serializable]
    public sealed class RequestLoggedEventArgs : EventArgs
    {
        private readonly RequestLogEntry _entry;

        public RequestLoggedEventArgs(RequestLogEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException("entry");

            _entry = entry;
        }

        public RequestLogEntry Entry
        {
            get { return _entry; }
        }
    }
}
