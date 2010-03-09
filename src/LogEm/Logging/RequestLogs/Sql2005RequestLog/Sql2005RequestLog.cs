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
using System.Web.UI.DataVisualization.Charting;
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
    public class Sql2005RequestLog : RequestLogBase
    {
        #region "Constants"
        protected const int _MAX_APP_NAME_LENGTH = 256;
        #endregion

        #region "Member Vars"
        protected readonly string _connectionString;
        protected LogEmDataContext _dc;
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
            session.ActiveXControls = pContext.Request.Browser.ActiveXControls;
        	session.AOL	= pContext.Request.Browser.AOL;
        	session.BackgroundSounds = pContext.Request.Browser.BackgroundSounds;
        	session.Beta = pContext.Request.Browser.Beta;
        	session.Browser = pContext.Request.Browser.Browser;				 
        	session.BrowserInfo = String.Join(", ", pContext.Request.Browser.Browsers.ToArray());		 
        	session.CanCombineFormsInDeck = pContext.Request.Browser.CanCombineFormsInDeck;	
        	session.CanInitiateVoiceCall = pContext.Request.Browser.CanInitiateVoiceCall;	
        	session.CanRenderAfterInputOrSelectElement = pContext.Request.Browser.CanRenderInputAndSelectElementsTogether;	
        	session.CanRenderEmptySelects = pContext.Request.Browser.CanRenderEmptySelects;	
        	session.CanRenderInputAndSelectElementsTogether = pContext.Request.Browser.CanRenderInputAndSelectElementsTogether;	
        	session.CanRenderMixedSelects = pContext.Request.Browser.CanRenderMixedSelects;	
        	session.CanRenderOneventAndPrevElementsTogether = pContext.Request.Browser.CanRenderOneventAndPrevElementsTogether;	
        	session.CanRenderPostBackCars = pContext.Request.Browser.CanRenderPostBackCards;
        	session.CanRenderSetvarZeroWithMultiSelectionList = pContext.Request.Browser.CanRenderSetvarZeroWithMultiSelectionList;	
        	session.CanSendMail = pContext.Request.Browser.CanSendMail;			
        	session.Capabilities = pContext.Request.Browser.Capabilities.ToCSVString();			 
        	session.ClrVersion = pContext.Request.Browser.ClrVersion.ToString();			 
        	session.Cookies = pContext.Request.Browser.Cookies;				
        	session.Crawler = pContext.Request.Browser.Crawler;				
        	session.DefaultSubmitButtonLimit = pContext.Request.Browser.DefaultSubmitButtonLimit;	 
        	session.EcmaScriptVersion = pContext.Request.Browser.EcmaScriptVersion.ToString();		 
        	session.Frames = pContext.Request.Browser.Frames;				
        	session.HasBackButton = pContext.Request.Browser.HasBackButton;			
        	session.HidesRightAlignedMultiselectScrollbars = pContext.Request.Browser.HidesRightAlignedMultiselectScrollbars;	
        	session.Id = pContext.Request.Browser.Id;					 
        	session.InputType = pContext.Request.Browser.InputType;				 
        	session.IsColor = pContext.Request.Browser.IsColor;				
        	session.IsMobileDevice = pContext.Request.Browser.IsMobileDevice;		
        	session.JavaApplets = pContext.Request.Browser.JavaApplets;			
        	session.JScriptVersion = pContext.Request.Browser.JScriptVersion.ToString();		 
        	session.MajorVersion = pContext.Request.Browser.MajorVersion;			 
        	session.MaximumHrefLength = pContext.Request.Browser.MaximumHrefLength;;		 
        	session.MaximumRenderedPageSize = pContext.Request.Browser.MaximumRenderedPageSize;	 
        	session.MaximumSoftkeyLabelLength = pContext.Request.Browser.MaximumSoftkeyLabelLength;	 
        	session.MinorVersion = (Decimal)pContext.Request.Browser.MinorVersion; 
        	session.MobileDeviceManufacturer = pContext.Request.Browser.MobileDeviceManufacturer;	 
        	session.MobileDeviceModel = pContext.Request.Browser.MobileDeviceModel;		 	
        	session.MSDomVersion = pContext.Request.Browser.MSDomVersion.ToString();			 
        	session.NumberOfSoftKeys = pContext.Request.Browser.NumberOfSoftkeys;		 
        	session.Platform = pContext.Request.Browser.Platform;				 
        	session.PreferredImageMime = pContext.Request.Browser.PreferredImageMime;	 
        	session.PreferredRenderingMime = pContext.Request.Browser.PreferredRenderingMime;	 
        	session.PreferredRenderingType = pContext.Request.Browser.PreferredRenderingType;	 
        	session.PreferredRequestEncoding = pContext.Request.Browser.PreferredRequestEncoding;	 
        	session.PreferredResponseEncoding = pContext.Request.Browser.PreferredResponseEncoding;	 
        	session.RendersBreakBeforeWmlSelectAndInput = pContext.Request.Browser.RendersBreakBeforeWmlSelectAndInput;
            session.RendersBreaksAfterHtmlLists = pContext.Request.Browser.RendersBreaksAfterHtmlLists;
        	session.RendersBreaksAfterWmlAnchor = pContext.Request.Browser.RendersBreaksAfterWmlAnchor;
        	session.RendersBreaksAfterWmlInput = pContext.Request.Browser.RendersBreaksAfterWmlInput;	
        	session.RendersWmlDoAcceptsInline = pContext.Request.Browser.RendersWmlDoAcceptsInline;	
        	session.RendersWmlSelectsAsMenuCards = pContext.Request.Browser.RendersWmlSelectsAsMenuCards;	
        	session.RequiredMetaTagNameValue = pContext.Request.Browser.RequiredMetaTagNameValue;	 
        	session.RequiresAttributeColonSubstitution = pContext.Request.Browser.RequiresAttributeColonSubstitution;	
        	session.RequiresContentTypeMetaTag = pContext.Request.Browser.RequiresContentTypeMetaTag;	
        	session.RequiresContentStateInSession = pContext.Request.Browser.RequiresControlStateInSession;	
        	session.RequiresDBCSCharacter = pContext.Request.Browser.RequiresDBCSCharacter;	
        	session.RequiresHtmlAdaptiveErrorReporting = pContext.Request.Browser.RequiresHtmlAdaptiveErrorReporting; 
        	session.RequiresLeadingPageBreak = pContext.Request.Browser.RequiresLeadingPageBreak;	
        	session.RequiresNoBreakInFormatting = pContext.Request.Browser.RequiresNoBreakInFormatting;	
        	session.RequiresOutputOptimization = pContext.Request.Browser.RequiresOutputOptimization;	
        	session.RequiresPhoneNumbersAsPlainText = pContext.Request.Browser.RequiresPhoneNumbersAsPlainText;	
        	session.RequiresSpecialViewStateEncoding = pContext.Request.Browser.RequiresSpecialViewStateEncoding;	
        	session.RequiresUniqueFilePathSuffix = pContext.Request.Browser.RequiresUniqueFilePathSuffix;	
        	session.RequiresUniqueHtmlCheckboxNames = pContext.Request.Browser.RequiresUniqueHtmlCheckboxNames;	
        	session.RequiresUniqueHtmlInputNames = pContext.Request.Browser.RequiresUniqueHtmlInputNames;	
        	session.RequiresUrlEncodedPostfieldValues = pContext.Request.Browser.RequiresUrlEncodedPostfieldValues;	
        	session.ScreenBitDepth = pContext.Request.Browser.ScreenBitDepth;			 
        	session.ScreenCharactersHeight = pContext.Request.Browser.ScreenCharactersHeight;	 
        	session.ScreenCharactersWidth = pContext.Request.Browser.ScreenCharactersWidth;		 
        	session.ScreenPixelsHeight = pContext.Request.Browser.ScreenPixelsHeight;		 
        	session.ScreenPixelsWidth = pContext.Request.Browser.ScreenPixelsWidth;			 
        	session.SupportsAccesskeyAttribute = pContext.Request.Browser.SupportsAccesskeyAttribute;	
        	session.SupportsBodyColor = pContext.Request.Browser.SupportsBodyColor;			
        	session.SupportsBold = pContext.Request.Browser.SupportsBold;				
        	session.SupportsCacheControlMetaTag = pContext.Request.Browser.SupportsCacheControlMetaTag;	
        	session.SupportsCallback = pContext.Request.Browser.SupportsCallback;
        	session.SupportsCss = pContext.Request.Browser.SupportsCss;				
        	session.SupportsDivAlign = pContext.Request.Browser.SupportsDivAlign;			
        	session.SupportsDivNoWrap = pContext.Request.Browser.SupportsDivNoWrap;			
        	session.SupportsEmptyStringInCookieValue = pContext.Request.Browser.SupportsEmptyStringInCookieValue;	
        	session.SupportsFontColor = pContext.Request.Browser.SupportsFontColor;			
        	session.SupportsFontName = pContext.Request.Browser.SupportsFontName;			
        	session.SupportsFontSize = pContext.Request.Browser.SupportsFontSize;			
        	session.SupportsImageSubmit = pContext.Request.Browser.SupportsImageSubmit;		
        	session.SupportsIModeSymbols = pContext.Request.Browser.SupportsIModeSymbols;		
        	session.SupportsInputIStyle = pContext.Request.Browser.SupportsInputIStyle;		
        	session.SupportsInputMode = pContext.Request.Browser.SupportsInputMode;			
        	session.SupportsItalic = pContext.Request.Browser.SupportsItalic;			
        	session.SupportsJPhoneMultiMediaAttributes = pContext.Request.Browser.SupportsJPhoneMultiMediaAttributes;	
        	session.SupportsJPhoneSymbols = pContext.Request.Browser.SupportsJPhoneSymbols;		
        	session.SupportsQueryStringInFormAction = pContext.Request.Browser.SupportsQueryStringInFormAction;	
        	session.SupportsRedirectWithCookie = pContext.Request.Browser.SupportsRedirectWithCookie;
        	session.SupportsSelectMultiple = pContext.Request.Browser.SupportsSelectMultiple;	
        	session.SupportsUncheck = pContext.Request.Browser.SupportsUncheck;			
        	session.SupportsXmlHttp = pContext.Request.Browser.SupportsXmlHttp;			
        	session.Tables = pContext.Request.Browser.Tables;					
        	session.Type = pContext.Request.Browser.Type;						 
        	session.UseOptimizedCacheKey = pContext.Request.Browser.UseOptimizedCacheKey;		
        	session.VBScript = pContext.Request.Browser.VBScript;					
        	session.Version = pContext.Request.Browser.Version;					 
        	session.W3CDOMVersion = pContext.Request.Browser.W3CDomVersion.ToString();				 
        	session.Win16 = pContext.Request.Browser.Win16;						
        	session.Win32 = pContext.Request.Browser.Win32;						

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

        /// <summary>
        /// Returns a page of resource requests from the databse in descending order 
        /// of logged time.
        /// </summary>
        public override int GetRequests(int pageIndex, int pageSize, IList<ResourceRequestBase> requestEntryList)
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

            requestEntryList = (IList<ResourceRequestBase>)requestList;

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

        #region DataQueries
        /// <summary>
        /// Build the browsers by request series from the log data.
        /// </summary>
        /// <returns>Returns a series of percents, were each percent is the fraction
        /// of the total of requests made by each type and majorversion of
        /// browser in the log.</returns>
        public override Series SeriesBrowsersByRequest()
        {
            List<int> requestCount = new List<int>();
            List<string> browserNames = new List<string>();

            Series result = new Series("BrowsersByRequest");
            result.ChartType = SeriesChartType.Pie;

            // Query the database for the required grouping
            var browsers = (from rr in _dc.Sql2005ResourceRequests
                            group rr by new { rr.Sql2005Session.Browser, rr.Sql2005Session.MajorVersion } into grouping
                            orderby grouping.Count() descending
                            select new
                            {
                                Name = grouping.Key.Browser,
                                MajorVersion = grouping.Key.MajorVersion,
                                Count = grouping.Count()
                            });

            // Now iterate the browser groupings and add Points to the data
            // series for plotting on the chart
            foreach (var browser in browsers.ToList())
            {
                requestCount.Add(browser.Count);
                browserNames.Add(String.Format("{0}, version {1}", browser.Name, browser.MajorVersion));
            }

            // Populate series data
            result.Points.DataBindXY(browserNames, requestCount);

            return result;
        }
        #endregion
        #endregion

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
