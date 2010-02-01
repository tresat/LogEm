using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Xml;
using System.Linq;
using System.Data.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Collections;
using System.Web;
using LogEm.Logging;
using LogEm.Logging.RequestLogs;
using LogEm.ExtensionMethods;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;

namespace LogEm.Logging.RequestLogs.Sql2005RequestLog
{
    /// <summary>
    /// An <see cref="ErrorLog"/> implementation that uses Microsoft SQL 
    /// Server 2000 as its backing store.
    /// </summary>
    public class Sql2005RequestLog : RequestLog
    {
        #region "Constants"
        protected const int _MAX_APP_NAME_LENGTH = 256;
        #endregion

        #region "Member Vars"
        protected readonly string _connectionString;
        protected LogEmDataContext _dc;
         #endregion

        #region "Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Sql2005RequestLog"/> class
        /// using a dictionary of configured settings.
        /// </summary>
        public Sql2005RequestLog(IDictionary pConfig)
        {
            if (pConfig == null)
                throw new ArgumentNullException("config");

            string connectionString = ConnectionStringHelper.GetConnectionString(pConfig);

            // If there is no connection string to use then throw an 
            // exception to abort construction.
            if (connectionString.Length == 0)
                throw new ApplicationException("Connection string is empty for the SQL Request log.");

            _connectionString = connectionString;
            _dc = new LogEmDataContext(_connectionString);

            // Set the application name as this implementation provides
            // per-application isolation over a single store.
            SetupAppName(pConfig["applicationName"] as string);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sql2005RequestLog"/> class
        /// to use a specific connection string for connecting to the database.
        /// </summary>
        public Sql2005RequestLog(string connectionString, string applicationName)
        {
            // If there is no connection string to use then throw an 
            // exception to abort construction.
            if (connectionString == null)
                throw new ArgumentNullException("Connection string is null for the SQL Request log.");

            if (connectionString.Length == 0)
                throw new ArgumentException("Connection string is empty for the SQL Request log.");

            _connectionString = connectionString;
            _dc = new LogEmDataContext(_connectionString);

            // Set the application name as this implementation provides
            // per-application isolation over a single store.
            SetupAppName(applicationName);
        }
        #endregion

        #region "Properties"
        /// <summary>
        /// Gets the name of this Request Log implementation.
        /// </summary>
        public override string Name
        {
            get { return "Microsoft SQL Server Request Log"; }
        }

        /// <summary>
        /// Gets the connection string used by the log to connect to the database.
        /// </summary>
        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }
        #endregion

        #region "Public Functionality"
        /// <summary>
        /// Override of the ResourceRequest creation function.
        /// Produces a populated Sql2005ResourceRequest object.
        /// </summary>
        /// <param name="pContext">HttpContext of request (will be used to populate RR object.</param>
        /// <returns>New ResourceRequestBase object, populated.</returns>
        public override ResourceRequestBase CreateNewResourceRequest(HttpContext pContext)
        {
            if (pContext == null)
                throw new ArgumentNullException("pContext");

            // Check to get the existing session ID, if it is already stored
            Guid? sessionID = GetSessionID(pContext.Session != null ? pContext.Session.SessionID : pContext.Request.Params["ASP.Net_SessionId"]);;
            if (!sessionID.HasValue)
            {
                sessionID = LogSession(pContext);
            }

            Sql2005ResourceRequest request = new Sql2005ResourceRequest();

            request.ResourceRequestID = Guid.NewGuid();
            request.Application = this.ApplicationName;
            request.Host = Environment.MachineName;
            request.User = (pContext.User != null ? pContext.User.Identity.Name : null);
            request.fkSessionID = sessionID;
            request.ResourceRequestTimeUtc = DateTime.UtcNow;
            request.RequestAcceptTypes = String.Join(", ", pContext.Request.AcceptTypes);
            request.AnonymousID = (pContext.Request.AnonymousID != null ? pContext.Request.AnonymousID : null);
            request.ApplicationPath = pContext.Request.ApplicationPath;
            request.RequestEncoding = pContext.Request.ContentEncoding.EncodingName;
            request.RequestType = pContext.Request.ContentType;
            request.RequestCookies = pContext.Request.Cookies.ToCSVString();
            request.RequestFormValues = pContext.Request.Form.ToCSVString();
            request.RequestHttpMethod = pContext.Request.HttpMethod;
            request.RequestIsAuthenticated = pContext.Request.IsAuthenticated;
            request.RequestIsLocal = pContext.Request.IsLocal;
            request.RequestIsSecure = pContext.Request.IsSecureConnection;
            request.RequestQueryString = pContext.Request.QueryString.ToCSVString();
            request.RequestServerVariables = pContext.Request.ServerVariables.ToCSVString();
            request.RequestBytes = pContext.Request.TotalBytes;
            request.URL = pContext.Request.RawUrl;
            request.UserAgent = pContext.Request.UserAgent;
            request.UserHost = pContext.Request.UserHostAddress;
            request.UserHostName = pContext.Request.UserHostName;
            request.UserRequestTime = pContext.Timestamp;
            request.ResponseEncoding = pContext.Response.ContentEncoding.EncodingName;
            request.ResponseType = pContext.Response.ContentType;
            request.ResponseCookies = pContext.Response.Cookies.ToCSVString();
            request.ResponseStatus = pContext.Response.Status;

            request.Handler = (pContext.Handler != null ? pContext.Handler.GetType().Name : null);

            return request;
        }

        /// <summary>
        /// Logs a resouce request to the database.
        /// </summary>
        /// <param name="pRequest">Should be of type Sql2005ResourceRequest.</param>
        /// <returns>Guid of log entry.</returns>
        public override Guid LogResourceRequest(ResourceRequestBase pRequest)
        {
            if (pRequest == null)
                throw new ArgumentNullException("pRequest");

            // Since we're implementing an abstraction, need to 
            // keep param type of RRBase, but can't actually use that
            // here, so check to convert it
            if (pRequest.GetType().Name != Type.GetType("LogEm.Logging.RequestLogs.Sql2005RequestLog.Sql2005ResourceRequest").Name)
                throw new ArgumentException("request must be a Sql2005ResourceRequest");

            // Cast argument to Sql2005ResourceRequest, and save it
            Sql2005ResourceRequest request = pRequest as Sql2005ResourceRequest;
            _dc.Sql2005ResourceRequests.InsertOnSubmit(request);
            _dc.SubmitChanges();

            return request.ResourceRequestID;
        }

        /// <summary>
        /// Override of the Session creation function.
        /// Produces a populated Sql2005Session object.
        /// </summary>
        /// <param name="pContext">HttpContext of request (will be used to populate Session object.</param>
        /// <returns>New ResourceRequestBase object, populated.</returns>
        public override SessionBase CreateNewSession(HttpContext pContext)
        {
            if (pContext == null)
                throw new ArgumentNullException("pContext");

            Sql2005Session session = new Sql2005Session();

            session.SessionID = Guid.NewGuid();
            session.Application = this.ApplicationName;
            session.Host = Environment.MachineName;
            session.User = (pContext.User != null ? pContext.User.Identity.Name : null);
            session.ASPSessionID = (pContext.Session != null ? pContext.Session.SessionID : pContext.Request.Params["ASP.Net_SessionId"]) ?? Guid.NewGuid().ToString();
            session.SessionBeginTimeUtc = DateTime.UtcNow;
            String s = "";
            foreach (var k in pContext.Request.Params.AllKeys)
            {
                s += k + "         ";
            }
            return session;
        }

        /// <summary>
        /// Logs a session to the database.
        /// </summary>
        /// <param name="pSession">Should be of type Sql2005Session.</param>
        /// <returns>Guid of log entry.</returns>
        public override Guid LogSession(SessionBase pSession)
        {
            if (pSession == null)
                throw new ArgumentNullException("pSession");

            // Since we're implementing an abstraction, need to 
            // keep param type of SessionBase, but can't actually use that
            // here, so check to convert it
            if (pSession.GetType().Name != Type.GetType("LogEm.Logging.RequestLogs.Sql2005RequestLog.Sql2005Session").Name)
                throw new ArgumentException("request must be a Sql2005Session");

            // Cast argument to Sql2005ResourceRequest, and save it
            Sql2005Session session = pSession as Sql2005Session;
            _dc.Sql2005Sessions.InsertOnSubmit(session);
            _dc.SubmitChanges();

            return session.SessionID;
        }

        #endregion

        /// <summary>
        /// Returns a page of resource requests from the databse in descending order 
        /// of logged time.
        /// </summary>
        public override int GetRequests(int pageIndex, int pageSize, IList requestEntryList)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException("pageIndex", pageIndex, null);

            if (pageSize < 0)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, null);

            List<Sql2005ResourceRequest> requestList = new List<Sql2005ResourceRequest>();
            foreach (Sql2005ResourceRequest rr in (from rr in _dc.Sql2005ResourceRequests
                    select rr))
            {
                requestEntryList.Add(rr);
            }

            requestEntryList = requestList;

            return requestList.Count;
        }

        /// <summary>
        /// Returns the specified resource request from the database, or null 
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

            return null;
        }

        /// <summary>
        /// Determines if the ASP session from the request is new, or has
        /// already been logged.
        /// </summary>
        /// <param name="pASPSessionID">The current Session ID.</param>
        /// <returns><c>true/false</c> whether or not the session is new.</returns>
        public override Boolean IsNewSession(String pASPSessionID)
        {
            return (GetSessionID(pASPSessionID) == null);
        }

#region "Protected Helpers"
        /// <summary>
        /// Stores the application name (first verifies it is valid)
        /// </summary>
        /// <param name="pAppName">The application name</param>
        protected void SetupAppName(String pAppName)
        {
            if (pAppName == null)
                throw new ArgumentNullException("pAppName");
            
            if (pAppName.Length > _MAX_APP_NAME_LENGTH)
                throw new ApplicationException(string.Format("Application name is too long. Maximum length allowed is {0} characters.", _MAX_APP_NAME_LENGTH));
            
            // Call own (base class) property, to set name and init status
            this.ApplicationName = pAppName;
        }

        /// <summary>
        /// Gets the sessionID fk for an existing session
        /// </summary>
        /// <param name="pASPSessionID">The ASP Session ID</param>
        /// <returns>The sessionID foreign key.</returns>
        protected Guid? GetSessionID(String pASPSessionID)
        {
            Guid? sessionID = null;
            if (pASPSessionID != null)
            {
                // Find the session ID for this ASP session ID
                var sessionIDs = (from s in _dc.Sql2005Sessions
                                  where s.ASPSessionID == pASPSessionID
                                  select s.SessionID).ToList<Guid>();
                if (sessionIDs.Count > 0)
                {
                    sessionID = sessionIDs[0];
                }
            }

            return sessionID;
        }
#endregion
    }
}
