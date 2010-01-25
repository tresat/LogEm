using System;

namespace LogEm
{
    /// <summary>
    /// Binds an <see cref="Request"/> instance with the <see cref="RequestLog"/>
    /// instance from where it was served.
    /// </summary>

    [Serializable]
    public class RequestLogEntry
    {
        private readonly string _id;
        private readonly RequestLog _log;
        private readonly UserRequest _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLogEntry"/> class
        /// for a given unique error entry in an error log.
        /// </summary>

        public RequestLogEntry(RequestLog log, string id, UserRequest request)
        {
            if (log == null)
                throw new ArgumentNullException("log");

            if (id == null)
                throw new ArgumentNullException("id");

            if (id.Length == 0)
                throw new ArgumentException(null, "id");

            if (request == null)
                throw new ArgumentNullException("request");

            _log = log;
            _id = id;
            _request = request;
        }

        /// <summary>
        /// Gets the <see cref="RequestLog"/> instance where this entry 
        /// originated from.
        /// </summary>

        public RequestLog Log
        {
            get { return _log; }
        }

        /// <summary>
        /// Gets the unique identifier that identifies the error entry 
        /// in the log.
        /// </summary>

        public string Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the <see cref="Request"/> object held in the entry.
        /// </summary>

        public UserRequest Request
        {
            get { return _request; }
        }
    }
}
