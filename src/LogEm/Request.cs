using System;
using System.Web;
using System.Xml;
using Thread = System.Threading.Thread;
using NameValueCollection = System.Collections.Specialized.NameValueCollection;

namespace LogEm
{
    /// <summary>
    /// Represents a user request to the site.
    /// </summary>
    [ Serializable ]
    public sealed class Request : ICloneable
    {
        private readonly Exception _exception;
        private string _applicationName;
        private string _hostName;
        private string _typeName;
        private string _source;
        private string _message;
        private string _detail;
        private string _user;
        private DateTime _time;
        private int _statusCode;
        private string _webHostHtmlMessage;
        private NameValueCollection _serverVariables;
        private NameValueCollection _queryString;
        private NameValueCollection _form;
        private NameValueCollection _cookies;

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>

        public Request() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class.
        /// </summary>

        public Request(HttpContext context) {
            if (context.User != null)
            {
                _user = context.User.Identity.Name;
            }
            else
            {
                _user = "anonymous";
            }
        }

        /// <summary>
        /// Gets or sets the name of application in which this request occurred.
        /// </summary>

        public string ApplicationName
        {
            get { return Mask.NullString(_applicationName); }
            set { _applicationName = value; }
        }

    //    /// <summary>
    //    /// Gets or sets name of host machine where this error occurred.
    //    /// </summary>
        
    //    public string HostName
    //    { 
    //        get { return Mask.NullString(_hostName); }
    //        set { _hostName = value; }
    //    }

    //    /// <summary>
    //    /// Gets or sets the type, class or category of the error.
    //    /// </summary>
        
    //    public string Type
    //    { 
    //        get { return Mask.NullString(_typeName); }
    //        set { _typeName = value; }
    //    }

    //    /// <summary>
    //    /// Gets or sets the source that is the cause of the error.
    //    /// </summary>
        
    //    public string Source
    //    { 
    //        get { return Mask.NullString(_source); }
    //        set { _source = value; }
    //    }

    //    /// <summary>
    //    /// Gets or sets a brief text describing the error.
    //    /// </summary>
        
    //    public string Message 
    //    { 
    //        get { return Mask.NullString(_message); }
    //        set { _message = value; }
    //    }

    //    /// <summary>
    //    /// Gets or sets a detailed text describing the error, such as a
    //    /// stack trace.
    //    /// </summary>

    //    public string Detail
    //    { 
    //        get { return Mask.NullString(_detail); }
    //        set { _detail = value; }
    //    }

    //    /// <summary>
    //    /// Gets or sets the user logged into the application at the time 
    //    /// of the error.
    //    /// </summary>
        
    //    public string User 
    //    { 
    //        get { return Mask.NullString(_user); }
    //        set { _user = value; }
    //    }

    //    /// <summary>
    //    /// Gets or sets the date and time (in local time) at which the 
    //    /// error occurred.
    //    /// </summary>
        
    //    public DateTime Time 
    //    { 
    //        get { return _time; }
    //        set { _time = value; }
    //    }

    //    /// <summary>
    //    /// Gets or sets the HTTP status code of the output returned to the 
    //    /// client for the error.
    //    /// </summary>
    //    /// <remarks>
    //    /// For cases where this value cannot always be reliably determined, 
    //    /// the value may be reported as zero.
    //    /// </remarks>
        
    //    public int StatusCode 
    //    { 
    //        get { return _statusCode; }
    //        set { _statusCode = value; }
    //    }

    //    /// <summary>
    //    /// Gets or sets the HTML message generated by the web host (ASP.NET) 
    //    /// for the given error.
    //    /// </summary>
        
    //    public string WebHostHtmlMessage
    //    {
    //        get { return Mask.NullString(_webHostHtmlMessage); }
    //        set { _webHostHtmlMessage = value; }
    //    }

    //    /// <summary>
    //    /// Gets a collection representing the Web server variables
    //    /// captured as part of diagnostic data for the error.
    //    /// </summary>
        
    //    public NameValueCollection ServerVariables 
    //    { 
    //        get { return FaultIn(ref _serverVariables);  }
    //    }

    //    /// <summary>
    //    /// Gets a collection representing the Web query string variables
    //    /// captured as part of diagnostic data for the error.
    //    /// </summary>
        
    //    public NameValueCollection QueryString 
    //    { 
    //        get { return FaultIn(ref _queryString); } 
    //    }

    //    /// <summary>
    //    /// Gets a collection representing the form variables captured as 
    //    /// part of diagnostic data for the error.
    //    /// </summary>
        
    //    public NameValueCollection Form 
    //    { 
    //        get { return FaultIn(ref _form); }
    //    }

    //    /// <summary>
    //    /// Gets a collection representing the client cookies
    //    /// captured as part of diagnostic data for the error.
    //    /// </summary>

    //    public NameValueCollection Cookies 
    //    {
    //        get { return FaultIn(ref _cookies); }
    //    }

    //    /// <summary>
    //    /// Returns the value of the <see cref="Message"/> property.
    //    /// </summary>

    //    public override string ToString()
    //    {
    //        return this.Message;
    //    }

    //    /// <summary>
    //    /// Creates a new object that is a copy of the current instance.
    //    /// </summary>

        object ICloneable.Clone()
        {
            //
            // Make a base shallow copy of all the members.
            //

            Request copy = (Request)MemberwiseClone();

            //
            // Now make a deep copy of items that are mutable.
            //

            copy._serverVariables = CopyCollection(_serverVariables);
            copy._queryString = CopyCollection(_queryString);
            copy._form = CopyCollection(_form);
            copy._cookies = CopyCollection(_cookies);

            return copy;
        }

        private static NameValueCollection CopyCollection(NameValueCollection collection)
        {
            if (collection == null || collection.Count == 0)
                return null;

            return new NameValueCollection(collection);
        }

        private static NameValueCollection CopyCollection(HttpCookieCollection cookies)
        {
            if (cookies == null || cookies.Count == 0)
                return null;

            NameValueCollection copy = new NameValueCollection(cookies.Count);

            for (int i = 0; i < cookies.Count; i++)
            {
                HttpCookie cookie = cookies[i];

                //
                // NOTE: We drop the Path and Domain properties of the 
                // cookie for sake of simplicity.
                //

                copy.Add(cookie.Name, cookie.Value);
            }

            return copy;
        }
    }
}
