using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace LogEm.Site.HTTPHandlers.Data
{
    internal abstract class DataHandlerBase : IHttpHandler
    {
        #region Members
        protected JavaScriptSerializer _serializer = new JavaScriptSerializer();
        #endregion

        #region "Properties"
        public bool IsReusable
        {
            get { return false; }
        }
        #endregion

        #region Public Functionality
        public abstract void ProcessRequest(HttpContext context);
        #endregion
    }
}