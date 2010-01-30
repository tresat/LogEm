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
using LogEm.Logging;
using LogEm.Logging.RequestLogs;

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

        /// <summary>
        /// Logs a request to the database.
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
            SQL2005ResourceRequest rr = new SQL2005ResourceRequest();
            rr.ResourceRequestID = id;
            rr.Application = this.ApplicationName;
            rr.TimeUtc = DateTime.UtcNow;
            rr.Host = Environment.MachineName;

            _dc.SQL2005ResourceRequests.InsertOnSubmit(rr);
            _dc.SubmitChanges();

            return id.ToString();
        }

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

            List<SQL2005ResourceRequest> requestList = new List<SQL2005ResourceRequest>();
            foreach (SQL2005ResourceRequest rr in (from rr in _dc.SQL2005ResourceRequests
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
#endregion
    }
}
