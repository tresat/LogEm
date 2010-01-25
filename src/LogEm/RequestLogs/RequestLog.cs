using System;
using System.Web;
using IList = System.Collections.IList;

namespace LogEm
{
    /// <summary>
    /// Represents an request log capable of storing and retrieving requests
    /// generated in an ASP.NET Web application.
    /// </summary>

    public abstract class RequestLog
    {
        private string _appName;
        private bool _appNameInitialized;

        private static readonly object _contextKey = new object();

        /// <summary>
        /// Logs a request in the log for the application.
        /// </summary>
        
        public abstract string Log(UserRequest request);

        /// <summary>
        /// When overridden in a subclass, begins an asynchronous version 
        /// of <see cref="Log"/>.
        /// </summary>

        public virtual IAsyncResult BeginLog(UserRequest request, AsyncCallback asyncCallback, object asyncState)
        {
            return BeginSyncImpl(asyncCallback, asyncState, new LogHandler(Log), request);
        }

        /// <summary>
        /// When overridden in a subclass, ends an asynchronous version 
        /// of <see cref="Log"/>.
        /// </summary>

        public virtual string EndLog(IAsyncResult asyncResult)
        {
            return (string)EndSyncImpl(asyncResult);
        }

        private delegate string LogHandler(UserRequest request);

        /// <summary>
        /// Retrieves a single application request from log given its 
        /// identifier, or null if it does not exist.
        /// </summary>

        public abstract RequestLogEntry GetRequest(string id);

        /// <summary>
        /// When overridden in a subclass, begins an asynchronous version 
        /// of <see cref="GetRequest"/>.
        /// </summary>

        public virtual IAsyncResult BeginGetRequest(string id, AsyncCallback asyncCallback, object asyncState)
        {
            return BeginSyncImpl(asyncCallback, asyncState, new GetErrorHandler(GetRequest), id);
        }

        /// <summary>
        /// When overridden in a subclass, ends an asynchronous version 
        /// of <see cref="GetRequest"/>.
        /// </summary>

        public virtual RequestLogEntry EndGetRequest(IAsyncResult asyncResult)
        {
            return (RequestLogEntry)EndSyncImpl(asyncResult);
        }

        private delegate RequestLogEntry GetErrorHandler(string id);

        /// <summary>
        /// Retrieves a page of application requests from the log in 
        /// descending order of logged time.
        /// </summary>

        public abstract int GetRequests(int pageIndex, int pageSize, IList errorEntryList);

        /// <summary>
        /// When overridden in a subclass, begins an asynchronous version 
        /// of <see cref="GetErrors"/>.
        /// </summary>

        public virtual IAsyncResult BeginGetRequests(int pageIndex, int pageSize, IList requestEntryList, AsyncCallback asyncCallback, object asyncState)
        {
            return BeginSyncImpl(asyncCallback, asyncState, new GetRequestsHandler(GetRequests), pageIndex, pageSize, requestEntryList);
        }

        /// <summary>
        /// When overridden in a subclass, ends an asynchronous version 
        /// of <see cref="GetErrors"/>.
        /// </summary>

        public virtual int EndGetRequests(IAsyncResult asyncResult)
        {
            return (int)EndSyncImpl(asyncResult);
        }

        private delegate int GetRequestsHandler(int pageIndex, int pageSize, IList errorEntryList);

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
            get { return Mask.NullString(_appName); }

            set
            {
                if (_appNameInitialized)
                    throw new InvalidOperationException("The application name cannot be reset once initialized.");

                _appName = value;
                _appNameInitialized = Mask.NullString(value).Length > 0;
            }
        }

        /// <summary>
        /// Gets the default request log implementation specified in the 
        /// configuration file, or the in-memory log implemention if
        /// none is configured.
        /// </summary>

        [Obsolete("Use RequestLog.GetDefault(context) instead.")]
        public static RequestLog Default
        {
            get { return GetDefault(HttpContext.Current); }
        }

        /// <summary>
        /// Gets the default request log implementation specified in the 
        /// configuration file, or the in-memory log implemention if
        /// none is configured.
        /// </summary>

        public static RequestLog GetDefault(HttpContext context)
        {
            RequestLog log;

            if (context != null)
            {
                log = (RequestLog)context.Items[_contextKey];

                if (log != null)
                    return log;
            }

            //
            // Determine the default store type from the configuration and 
            // create an instance of it.
            //

            log = (RequestLog)SimpleServiceProviderFactory.CreateFromConfigSection(Configuration.GroupSlash + "requestLog");

            //
            // If no object got created (probably because the right 
            // configuration settings are missing) then default to 
            // the in-memory log implementation.
            //

            if (log == null)
                log = new MemoryRequestLog();

            if (context != null)
            {
                //
                // Infer the application name from the context if it has not
                // been initialized so far.
                //

                if (log.ApplicationName.Length == 0)
                    log.ApplicationName = InferApplicationName(context);

                //
                // Save into the context if context is there so retrieval is
                // quick next time.
                //

                context.Items[_contextKey] = log;
            }

            return log;
        }

        private static string InferApplicationName(HttpContext context)
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

        //
        // The following two methods are helpers that provide boilerplate 
        // implementations for implementing asnychronous BeginXXXX and 
        // EndXXXX methods over a default synchronous implementation.
        //

        private static IAsyncResult BeginSyncImpl(AsyncCallback asyncCallback, object asyncState, Delegate syncImpl, params object[] args)
        {
            Debug.Assert(syncImpl != null);

            SynchronousAsyncResult asyncResult;
            string syncMethodName = syncImpl.Method.Name;

            try
            {
                asyncResult = SynchronousAsyncResult.OnSuccess(syncMethodName, asyncState,
                    syncImpl.DynamicInvoke(args));
            }
            catch (Exception e)
            {
                asyncResult = SynchronousAsyncResult.OnFailure(syncMethodName, asyncState, e);
            }

            if (asyncCallback != null)
                asyncCallback(asyncResult);

            return asyncResult;
        }

        private static object EndSyncImpl(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            SynchronousAsyncResult syncResult = asyncResult as SynchronousAsyncResult;

            if (syncResult == null)
                throw new ArgumentException("IAsyncResult object did not come from the corresponding async method on this type.", "asyncResult");

            //
            // IMPORTANT! The End method on SynchronousAsyncResult will 
            // throw an exception if that's what Log did when 
            // BeginLog called it. The unforunate side effect of this is
            // the stack trace information for the exception is lost and 
            // reset to this point. There seems to be a basic failure in the 
            // framework to accommodate for this case more generally. One 
            // could handle this through a custom exception that wraps the 
            // original exception, but this assumes that an invocation will 
            // only throw an exception of that custom type.
            //

            return syncResult.End();
        }
    }
}
