using System;
using System.Web;
using System.Linq;

namespace LogEm.Site.HTTPHandlers.Data
{
    internal class RequestsByBrowserHandler : DataHandlerBase
    {
        #region Public Functionality
        public override void ProcessRequest(HttpContext context)
        {
            var response = new { Type = "answer", Hey = "yo" };

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.Output.Write(_serializer.Serialize(response));
            context.Response.End();
        }
        #endregion
    }
}
