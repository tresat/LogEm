using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogEm.Site.HTTPHandlers.Charts;

namespace LogEm.Site
{
    internal class ChartHandlerFactory : IHttpHandlerFactory
    {
        /// <summary>
        /// Returns an chart handler that implements the <see cref="IHttpHandler"/> 
        /// interface and which is responsible for serving the request.
        /// </summary>
        /// <returns>
        /// A new <see cref="IHttpHandler"/> chart handler that processes the request.
        /// </returns>
        public virtual IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            // Create the HTTP Handler which will service the request
            IHttpHandler handler = InstantiateHandler(context, requestType, url, pathTranslated);
            if (handler == null)
                throw new HttpException(404, "Resource not found.");

            // And return it
            return handler;
        }

        /// <summary>
        /// Determines the handler type from the URL.
        /// </summary>
        /// <param name="pUrl">The url being requested.</param>
        /// <returns>An instantiated handler, ready to service request.  
        /// Or <c>null</c> if the handler is not known.</returns>
        private static IHttpHandler InstantiateHandler(HttpContext pContext, string pRequestType, string pUrl, string pPathTranslated)
        {
            if (pContext == null)
                throw new ArgumentNullException("pContext");
            if (pContext.Request.QueryString.Count < 1 || pContext.Request.QueryString["type"] == null)
                throw new ArgumentNullException("QueryString must include type param when requesting a chart!");

            switch (pContext.Request.QueryString["type"].ToLowerInvariant())
            {
                case "browsers-by-request":
                    return new BrowsersByRequestChart();
                default:
                    return null;
            }
        }

        /// <summary>
        /// Enables the factory to reuse an existing handler instance.
        /// Not currently used.
        /// </summary>
        public virtual void ReleaseHandler(IHttpHandler handler)
        {
        }
    }
}
