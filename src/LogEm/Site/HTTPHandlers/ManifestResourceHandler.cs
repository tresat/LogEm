using System;
using System.Web;
using System.Diagnostics;
using System.Text;

using Stream = System.IO.Stream;
using Debug = LogEm.Debug;
using ManifestResourceHelper = LogEm.ManifestResourceHelper;

namespace LogEm.Site.HTTPHandlers
{
    /// <summary>
    /// Reads a resource from the assembly manifest and returns its contents
    /// as the response entity.
    /// </summary>
    internal class ManifestResourceHandler : IHttpHandler
    {
        #region "Member Vars"
        private readonly string _resourceName;
        private readonly string _contentType;
        private readonly Encoding _responseEncoding;
        #endregion

        #region "Properties"
        public bool IsReusable
        {
            get { return false; }
        }
        #endregion

        #region "Constructors"
        public ManifestResourceHandler(string pResourceName, string pContentType) :
            this(pResourceName, pContentType, null) {}

        public ManifestResourceHandler(string pResourceName, string pContentType, Encoding pResponseEncoding)
        {
            if (pResourceName == null)
                throw new ArgumentNullException("pResourceName");
            if (pResourceName.Length == 0)
                throw new ArgumentException("pResourceName is empty");

            if (pContentType == null)
                throw new ArgumentNullException("pContentType");
            if (pContentType.Length == 0)
                throw new ArgumentException("pContentType is empty");

            // Response Encoding Allowed to be null

            _resourceName = GetFullResourceName(pResourceName, pContentType);
            _contentType = pContentType;
            _responseEncoding = pResponseEncoding;
        }
        #endregion

        #region "Public Functionality"
        public void ProcessRequest(HttpContext context)
        {
            // Set the response headers for indicating the content type 
            // and encoding (if specified).
            HttpResponse response = context.Response;
            response.ContentType = _contentType;

            if (_responseEncoding != null)
                response.ContentEncoding = _responseEncoding;

            ManifestResourceHelper.WriteResourceToStream(response.OutputStream, _resourceName);
        }
        #endregion

        #region "Protected Helpers"
        protected String GetFullResourceName(String pFileName, String pContentType)
        {
            if (pFileName == null)
                throw new ArgumentNullException("pFileName");

            // pResponseEncoding allowed to be null

            // Build the path/namespace to the resouce file
            StringBuilder name = new StringBuilder("LogEm.EmbeddedResources.");
            if (pContentType != null)
            {
                switch (pContentType.ToUpper())
                {
                    case "TEXT/CSS":
                        name.Append("css.");
                        break;
                    default:
                        throw new ArgumentException("pContentType is not a known encoding type.");
                }
            }
            name.Append(pFileName);

            return name.ToString();
        }
        #endregion
    }
}
