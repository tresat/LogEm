using System;
using System.Web;
using IList = System.Collections.IList;

namespace LogEm.Logging.RequestLogs
{
    /// <summary>
    /// Represents an request log capable of storing and retrieving request
    /// and session information generated in an ASP.NET Web application.
    /// </summary>
    public abstract class RequestLog
    {
        #region "Member Vars"
        private string _appName;
        private bool _appNameInitialized;

        private static readonly object _contextKey = new object();
        #endregion

        #region "Properties"
        /// <summary>
        /// Get the name of this log.
        /// </summary>
        public virtual string Name
        {
            get { return this.GetType().Name; }
        }

        /// <summary>
        /// Gets the name of the application to which the log is scoped.
        /// </summary>
        public string ApplicationName
        {
            get { return _appName; }

            set
            {
                if (_appNameInitialized)
                    throw new InvalidOperationException("The application name cannot be reset once initialized.");

                _appName = value;
                _appNameInitialized = true;
            }
        }
        #endregion

        #region "Public Functionality"
        /// <summary>
        /// Basic implementation of the ResourceRequest creation function.
        /// Most Log classes will probably have to override this.
        /// </summary>
        /// <param name="pContext">HttpContext of request (will be used to populate RR object.</param>
        /// <returns>New ResourceRequestBase object, populated.</returns>
        public virtual ResourceRequestBase CreateNewResourceRequest(HttpContext pContext)
        {
            if (pContext == null)
                throw new ArgumentNullException("pContext");

            ResourceRequestBase request = new ResourceRequestBase();
            return request;
        }

        /// Most Log classes will probably have to override this.
        /// </summary>
        /// <param name="pContext">HttpContext of request (will be used to populate Session object.</param>
        /// <returns>New SessionBase object, populated.</returns>
        public virtual SessionBase CreateNewSession(HttpContext pContext)
        {
            if (pContext == null)
                throw new ArgumentNullException("pContext");

            SessionBase session = new SessionBase();
            return session;
        }

        /// <summary>
        /// Logs a request in the log for the application (creates request from Context).
        /// </summary>
        /// <param name="pContext">HttpContext of request (will be used to populate RR object.</param>
        /// <returns>New ResourceRequestBase object, populated, which has been added to the log.</returns>
        public virtual Guid LogResourceRequest(HttpContext pContext)
        {
            return LogResourceRequest(CreateNewResourceRequest(pContext));
        }

        /// <summary>
        /// Logs a session in the log for the application (creates session from Context).
        /// </summary>
        /// <param name="pContext">HttpContext of session (will be used to populate Session object.</param>
        /// <returns>New SessionBase object, populated, which has been added to the log.</returns>
        public virtual Guid LogSession(HttpContext pContext)
        {
            return LogSession(CreateNewSession(pContext));
        }

        /// <summary>
        /// Logs a request in the log for the application.
        /// </summary>
        /// <param name="pRequest">The resouce request to log.</param>
        /// <returns>The</returns>
        public abstract Guid LogResourceRequest(ResourceRequestBase pRequest);

        /// <summary>
        /// Logs a session in the log for the application.
        /// </summary>
        /// <param name="pRequest">The session to log.</param>
        /// <returns>The</returns>
        public abstract Guid LogSession(SessionBase pSession);

        /// <summary>
        /// Determines if the ASP session from the request is new, or has
        /// already been logged.
        /// </summary>
        /// <param name="pASPSessionID">The current Session ID.</param>
        /// <returns><c>true/false</c> whether or not the session is new.</returns>
        public abstract Boolean IsNewSession(String pASPSessionID);

        #endregion
        // The line of Tom approval
        




        private delegate string LogHandler(ResourceRequestBase request);

        /// <summary>
        /// Retrieves a single application request from log given its 
        /// identifier, or null if it does not exist.
        /// </summary>
        public abstract ResourceRequestBase GetRequest(string id);
        private delegate ResourceRequestBase GetRequestHandler(string id);

        /// <summary>
        /// Retrieves a page of application requests from the log in 
        /// descending order of logged time.
        /// </summary>
        public abstract int GetRequests(int pageIndex, int pageSize, IList errorEntryList);
        private delegate int GetRequestsHandler(int pageIndex, int pageSize, IList errorEntryList);


        #region "Public Functionality"
        /// <summary>
        /// Gets the currently running RequestLog in the given HttpContext.
        /// </summary>
        /// <param name="context">Current Http Context (where a log should be stored, if a request has already caused one to be created).</param>
        /// <returns>The current active implementation of the RequestLog (a subclass of <see cref="LogEm.Logging.RequestLogs.RequestLog"/>being used for this HttpContext.</returns>
        public static RequestLog GetLog(HttpContext context)
        {
            // TODO: this feels VERY weird to me as a static of the base class...move it somewhere else?
            if (context == null)
                throw new ArgumentNullException("context");

            // Check for the log in the Context store
            RequestLog log = (RequestLog)context.Items[_contextKey];
            if (log == null)
            {
                // Log not found in context, have to create a logger

                // Determine the default store type from the configuration and 
                // create an instance of it.
                log = (RequestLog)ObjectFactory.CreateFromConfigSection(Configuration.GroupSlash + "requestLog");

                // If no object got created (probably because the right 
                // configuration settings are missing) then throw an exception.
                if (log == null)
                    throw new ApplicationException("Log type not set!");

                // Save into the context so retrieval is quick next time.
                context.Items[_contextKey] = log;
            }

            return log;
        }
        #endregion

        #region "Protected Helpers"
        protected static string InferApplicationName(HttpContext context)
        {
            Debug.Assert(context != null);

#if NET_1_1 || NET_1_0
            return HttpRuntime.AppDomainAppId;
#else
            //
            // Setup the application name (ASP.NET 2.0 or later).
            //

            string appName = null;

            if (context.Request != null)
            {
                //
                // ASP.NET 2.0 returns a different and more cryptic value
                // for HttpRuntime.AppDomainAppId comared to previous 
                // versions. Also HttpRuntime.AppDomainAppId is not available
                // in partial trust environments. However, the APPL_MD_PATH
                // server variable yields the same value as 
                // HttpRuntime.AppDomainAppId did previously so we try to
                // get to it over here for compatibility reasons (otherwise
                // folks upgrading to this version of ELMAH could find their
                // error log empty due to change in application name.
                //

                appName = context.Request.ServerVariables["APPL_MD_PATH"];
            }

            if (string.IsNullOrEmpty(appName))
            {
                //
                // Still no luck? Try HttpRuntime.AppDomainAppVirtualPath,
                // which is available even under partial trust.
                //

                appName = HttpRuntime.AppDomainAppVirtualPath;
            }

            return Mask.EmptyString(appName, "/");
#endif
        }
        #endregion
    }
}
