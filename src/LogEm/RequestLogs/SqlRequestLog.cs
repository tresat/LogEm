#if !NET_1_0 && !NET_1_1
    #define ASYNC_ADONET
#endif

using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Xml;
using System.Linq;
using System.Data.Linq;
using System.Collections.Generic;

using IDictionary = System.Collections.IDictionary;
using IList = System.Collections.IList;

namespace LogEm.RequestLogs
{
    /// <summary>
    /// An <see cref="ErrorLog"/> implementation that uses Microsoft SQL 
    /// Server 2000 as its backing store.
    /// </summary>

    public class SqlRequestLog : RequestLog
    {
        protected const int _maxAppNameLength = 60;
        
        protected readonly string _connectionString;
        protected LogEmDataContext _dc;

#if ASYNC_ADONET
        protected delegate RV Function<RV, A>(A a);
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlErrorLog"/> class
        /// using a dictionary of configured settings.
        /// </summary>

        public SqlRequestLog(IDictionary config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            string connectionString = ConnectionStringHelper.GetConnectionString(config);

            //
            // If there is no connection string to use then throw an 
            // exception to abort construction.
            //

            if (connectionString.Length == 0)
                throw new ApplicationException("Connection string is missing for the SQL error log.");

            _connectionString = connectionString;
            _dc = new LogEmDataContext(_connectionString);

            //
            // Set the application name as this implementation provides
            // per-application isolation over a single store.
            //

            string appName = Mask.NullString((string)config["applicationName"]);

            if (appName.Length > _maxAppNameLength)
            {
                throw new ApplicationException(string.Format(
                    "Application name is too long. Maximum length allowed is {0} characters.",
                    _maxAppNameLength.ToString("N0")));
            }

            ApplicationName = appName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlErrorLog"/> class
        /// to use a specific connection string for connecting to the database.
        /// </summary>

        public SqlRequestLog(string connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");

            if (connectionString.Length == 0)
                throw new ArgumentException(null, "connectionString");

            _connectionString = connectionString;
            _dc = new LogEmDataContext(_connectionString);
            _dc.DeferredLoadingEnabled = true;
        }

        /// <summary>
        /// Gets the name of this error log implementation.
        /// </summary>

        public override string Name
        {
            get { return "Microsoft SQL Server Error Log"; }
        }

        /// <summary>
        /// Gets the connection string used by the log to connect to the database.
        /// </summary>

        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }

        /// <summary>
        /// Logs an error to the database.
        /// </summary>
        /// <remarks>
        /// Use the stored procedure called by this implementation to set a
        /// policy on how long errors are kept in the log. The default
        /// implementation stores all errors for an indefinite time.
        /// </remarks>

        public override string Log(ResourceRequestBase request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            Guid id = Guid.NewGuid();
            ResourceRequest rr = new ResourceRequest(request.Context);
            rr.ResourceRequestID = id;
            rr.Application = this.ApplicationName;
            rr.TimeUtc = DateTime.UtcNow;
            rr.Host = Environment.MachineName;

            _dc.ResourceRequests.InsertOnSubmit(rr);
            _dc.SubmitChanges();

            return id.ToString();
        }

        /// <summary>
        /// Returns a page of requests from the databse in descending order 
        /// of logged time.
        /// </summary>

        public override int GetRequests(int pageIndex, int pageSize, IList requestEntryList)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException("pageIndex", pageIndex, null);

            if (pageSize < 0)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, null);

            List<ResourceRequest> requestList = new List<ResourceRequest>();
            foreach (ResourceRequest rr in (from rr in _dc.ResourceRequests
                    select rr))
            {
                requestEntryList.Add(rr);
            }

            requestEntryList = requestList;

            return requestList.Count;
        }

#if ASYNC_ADONET

        /// <summary>
        /// Begins an asynchronous version of <see cref="GetErrors"/>.
        /// </summary>

        public override IAsyncResult BeginGetRequests(int pageIndex, int pageSize, IList requestEntryList,
            AsyncCallback asyncCallback, object asyncState)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException("pageIndex", pageIndex, null);

            if (pageSize < 0)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, null);

            //
            // Modify the connection string on the fly to support async 
            // processing otherwise the asynchronous methods on the
            // SqlCommand will throw an exception. This ensures the
            // right behavior regardless of whether configured
            // connection string sets the Async option to true or not.
            //

            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(this.ConnectionString);
            csb.AsynchronousProcessing = true;
            SqlConnection connection = new SqlConnection(csb.ConnectionString);

            //
            // Create the command object with input parameters initialized
            // and setup to call the stored procedure.
            //

            //SqlCommand command = Commands.GetErrorsXml(this.ApplicationName, pageIndex, pageSize);
            //command.Connection = connection;

            //
            // Create a closure to handle the ending of the async operation
            // and retrieve results.
            //

            AsyncResultWrapper asyncResult = null;

            Function<int, IAsyncResult> endHandler = delegate
            {
                Debug.Assert(asyncResult != null);

                //using (connection)
                //using (command)
                //{
                //    using (XmlReader reader = command.EndExecuteXmlReader(asyncResult.InnerResult))
                //        ErrorsXmlToList(reader, errorEntryList);

                //    int total;
                //    Commands.GetErrorsXmlOutputs(command, out total);
                //    return total;
                //}

                return 3;
            };

            //
            // Open the connenction and execute the command asynchronously,
            // returning an IAsyncResult that wrap the downstream one. This
            // is needed to be able to send our own AsyncState object to
            // the downstream IAsyncResult object. In order to preserve the
            // one sent by caller, we need to maintain and return it from
            // our wrapper.
            //

            try
            {
                connection.Open();

                //asyncResult = new AsyncResultWrapper(
                //    command.BeginExecuteXmlReader(
                //        asyncCallback != null ? /* thunk */ delegate { asyncCallback(asyncResult); } : (AsyncCallback)null,
                //        endHandler), asyncState);

                return asyncResult;
            }
            catch (Exception)
            {
                connection.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Ends an asynchronous version of <see cref="ErrorLog.GetErrors"/>.
        /// </summary>

        public override int EndGetRequests(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentNullException("asyncResult");

            AsyncResultWrapper wrapper = asyncResult as AsyncResultWrapper;

            if (wrapper == null)
                throw new ArgumentException("Unexepcted IAsyncResult type.", "asyncResult");

            Function<int, IAsyncResult> endHandler = (Function<int, IAsyncResult>)wrapper.InnerResult.AsyncState;
            return endHandler(wrapper.InnerResult);
        }

#endif

        /// <summary>
        /// Returns the specified error from the database, or null 
        /// if it does not exist.
        /// </summary>

        public override ResourceRequestBase GetRequest(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");

            if (id.Length == 0)
                throw new ArgumentException(null, "id");

            Guid errorGuid;

            try
            {
                errorGuid = new Guid(id);
            }
            catch (FormatException e)
            {
                throw new ArgumentException(e.Message, "id", e);
            }

            //string errorXml;

            //using (SqlConnection connection = new SqlConnection(this.ConnectionString))
            //using (SqlCommand command = Commands.GetErrorXml(this.ApplicationName, errorGuid))
            //{
            //    command.Connection = connection;
            //    connection.Open();
            //    errorXml = (string)command.ExecuteScalar();
            //}

            //if (errorXml == null)
            //    return null;

            //Error error = ErrorXml.DecodeString(errorXml);
            //return new RequestLogEntry(this, id, request);

            return null;
        }

        /// <summary>
        /// An <see cref="IAsyncResult"/> implementation that wraps another.
        /// </summary>

        private sealed class AsyncResultWrapper : IAsyncResult
        {
            private readonly IAsyncResult _inner;
            private readonly object _asyncState;

            public AsyncResultWrapper(IAsyncResult inner, object asyncState)
            {
                _inner = inner;
                _asyncState = asyncState;
            }

            public IAsyncResult InnerResult
            {
                get { return _inner; }
            }

            public bool IsCompleted
            {
                get { return _inner.IsCompleted; }
            }

            public WaitHandle AsyncWaitHandle
            {
                get { return _inner.AsyncWaitHandle; }
            }

            public object AsyncState
            {
                get { return _asyncState; }
            }

            public bool CompletedSynchronously
            {
                get { return _inner.CompletedSynchronously; }
            }
        }
    }
}
