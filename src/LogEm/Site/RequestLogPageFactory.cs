using System;
using System.Collections;
using System.Web;
using LogEm.Logging;
using LogEm.Site.HTTPHandlers;
using LogEm.Site.Pages;
using CultureInfo = System.Globalization.CultureInfo;
using Encoding = System.Text.Encoding;

namespace LogEm.Site
{
    public interface IRequestAuthorizationHandler
    {
        bool Authorize(HttpContext context);
    }

    /// <summary>
    /// HTTP handler factory that dispenses handlers for rendering views and 
    /// resources needed to display and visualize the request log.
    /// </summary>
    public class RequestLogPageFactory : IHttpHandlerFactory
    {
        protected const String BASE_HANDLER_NAME = "LOGEM.AXD";
        protected const String BASE_HANDLER_NAME_WITH_TRAILING_SLASH = BASE_HANDLER_NAME + "/";

        protected static readonly object _authorizationHandlersKey = new object();
        protected static readonly IRequestAuthorizationHandler[] _zeroAuthorizationHandlers = new IRequestAuthorizationHandler[0];

        /// <summary>
        /// Returns an object that implements the <see cref="IHttpHandler"/> 
        /// interface and which is responsible for serving the request.
        /// </summary>
        /// <returns>
        /// A new <see cref="IHttpHandler"/> object that processes the request.
        /// </returns>
        public virtual IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            // Check if authorized then grant or deny request.
            int authorized = IsAuthorized(context);
            if (authorized == 0
                || (authorized < 0 // Compatibility case...
                    && !HttpRequestSecurity.IsLocal(context.Request)
                    && !SecurityConfiguration.Default.AllowRemoteAccess))
            { // not authorized
                (new ManifestResourceHandler("RemoteAccessError.htm", "text/html")).ProcessRequest(context);
                HttpResponse response = context.Response;
                response.Status = "403 Forbidden";
                response.End();

                // HttpResponse.End docs say that it throws 
                // ThreadAbortException and so should never end up here but
                // that's not been the observation in the debugger. So as a
                // precautionary measure, bail out anyway.
                return null;
            } else { // authorized
                // Create the HTTP Handler which will service the request
                IHttpHandler handler = InstantiateHandler(context, requestType, url, pathTranslated);
                if (handler == null)
                    throw new HttpException(404, "Resource not found.");

                // And return it
                return handler;
            }
        }

        /// <summary>
        /// Determines the handler type from the URL.  Takes same params from
        /// GetHandler() call.  (Only uses context.)
        /// </summary>
        /// <returns>An instantiated handler, ready to service request.  
        /// Or <c>null</c> if the handler is not known.</returns>
        protected static IHttpHandler InstantiateHandler(HttpContext pContext, string pRequestType, string pUrl, string pPathTranslated)
        {
            if (pContext == null)
                throw new ArgumentNullException("pContext");

            // Switch off last segment of URL
            String resourceType = GetResourceType(pContext.Request.Url.Segments);
            switch (resourceType.ToLowerInvariant().TrimEnd(new Char[]{'/'}))
            {
                case "browser-info":
                    return new BrowserInfoPage();

                case "chart":
                    return (new ChartHandlerFactory()).GetHandler(pContext, pRequestType, pUrl, pPathTranslated);

                case "stylesheet":
                    return new ManifestResourceHandler(GetResourceName(pContext.Request.Url.Segments), "text/css", Encoding.GetEncoding("Windows-1252"));

                case "javascript":
                    return new ManifestResourceHandler(GetResourceName(pContext.Request.Url.Segments), "text/javascript", Encoding.GetEncoding("Windows-1252"));
                
                case "data":
                    return (new DataHandlerFactory()).GetHandler(pContext, pRequestType, pUrl, pPathTranslated);

                default:
                    throw new Exception(String.Format("Unknown request type: {0}", resourceType));
            }
        }

        /// <summary>
        /// Enables the factory to reuse an existing handler instance.
        /// Not currently used.
        /// </summary>
        public virtual void ReleaseHandler(IHttpHandler handler)
        {
        }

        #region Helpers
        protected static String GetResourceType(String[] pSegments)
        {
            for(int i = 0; i< pSegments.Length; i++) {
                if (pSegments[i].Equals(BASE_HANDLER_NAME_WITH_TRAILING_SLASH, StringComparison.InvariantCultureIgnoreCase))
                {
                    Debug.Assert(i + 1 < pSegments.Length);
                    return pSegments[i + 1];
                }
            }

            throw new Exception(String.Format("Couldn't find base handler name: {0} in request string.", BASE_HANDLER_NAME));
        }

        protected static String GetResourceName(String[] pSegments)
        {
            for (int i = 0; i < pSegments.Length; i++)
            {
                if (pSegments[i].Equals(BASE_HANDLER_NAME_WITH_TRAILING_SLASH, StringComparison.InvariantCultureIgnoreCase))
                {
                    Debug.Assert(i + 2 < pSegments.Length);
                    return pSegments[i + 2];
                }
            }

            throw new Exception(String.Format("Couldn't find base handler name: {0} in request string.", BASE_HANDLER_NAME));
        }
        #endregion

        #region Authorization
        /// <summary>
        /// Determines if the request is authorized by objects implementing
        /// <see cref="IRequestAuthorizationHandler" />.
        /// </summary>
        /// <returns>
        /// Returns zero if unauthorized, a value greater than zero if 
        /// authorized otherwise a value less than zero if no handlers
        /// were available to answer.
        /// </returns>
        protected static int IsAuthorized(HttpContext context)
        {
            Debug.Assert(context != null);

            int authorized = /* uninitialized */ -1;
            IEnumerator authorizationHandlers = GetAuthorizationHandlers(context).GetEnumerator();
            while (authorized != 0 && authorizationHandlers.MoveNext())
            {
                IRequestAuthorizationHandler authorizationHandler = (IRequestAuthorizationHandler)authorizationHandlers.Current;
                authorized = authorizationHandler.Authorize(context) ? 1 : 0;
            }
            return authorized;
        }

        protected static IList GetAuthorizationHandlers(HttpContext context)
        {
            Debug.Assert(context != null);

            object key = _authorizationHandlersKey;
            IList handlers = (IList)context.Items[key];

            if (handlers == null)
            {
                const int capacity = 4;
                ArrayList list = null;

                HttpApplication application = context.ApplicationInstance;
                if (application is IRequestAuthorizationHandler)
                {
                    list = new ArrayList(capacity);
                    list.Add(application);
                }

                foreach (IHttpModule module in HttpModuleRegistry.GetModules(application))
                {
                    if (module is IRequestAuthorizationHandler)
                    {
                        if (list == null)
                            list = new ArrayList(capacity);
                        list.Add(module);
                    }
                }

                context.Items[key] = handlers = ArrayList.ReadOnly(
                    list != null
                    ? list.ToArray(typeof(IRequestAuthorizationHandler))
                    : _zeroAuthorizationHandlers);
            }

            return handlers;
        }
        #endregion
    }
}
