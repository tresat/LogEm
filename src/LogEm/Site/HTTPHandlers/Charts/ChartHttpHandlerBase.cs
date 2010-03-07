using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogEm.Site.HTTPHandlers.Charts
{
    /// <summary>
    /// A base class for all LogEm chart handlers (which are HTTPHandlers
    /// which create a logEm chart and output the binary result to the 
    /// Response stream.
    /// </summary>
    public abstract class ChartHttpHandlerBase : IHttpHandler
    {
        #region Properties
        public virtual bool IsReusable
        {
            // Defaults to true, unless overriden
            get { return true; }
        }
        #endregion

        #region Public Functionality
        /// <summary>
        /// Derived classes will have to implment this, it is the meat
        /// of the HTTP Handler, and where they will render their particular
        /// chart image byte streams.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        public abstract void ProcessRequest(HttpContext context);
        #endregion
    }
}
