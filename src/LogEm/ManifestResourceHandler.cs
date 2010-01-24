using System;
using System.Web;
using System.Diagnostics;

using Stream = System.IO.Stream;
using Encoding = System.Text.Encoding;

namespace LogEm
{
    /// <summary>
    /// Reads a resource from the assembly manifest and returns its contents
    /// as the response entity.
    /// </summary>
    internal sealed class ManifestResourceHandler : IHttpHandler
    {
        private readonly string _resourceName;
        private readonly string _contentType;
        private readonly Encoding _responseEncoding;

        public ManifestResourceHandler(string resourceName, string contentType) :
            this(resourceName, contentType, null) {}

        public ManifestResourceHandler(string resourceName, string contentType, Encoding responseEncoding)
        {
            Debug.AssertStringNotEmpty(resourceName);
            Debug.AssertStringNotEmpty(contentType);

            _resourceName = resourceName;
            _contentType = contentType;
            _responseEncoding = responseEncoding;
        }

        public void ProcessRequest(HttpContext context)
        {
            //
            // Set the response headers for indicating the content type 
            // and encoding (if specified).
            //

            HttpResponse response = context.Response;
            response.ContentType = _contentType;

            if (_responseEncoding != null)
                response.ContentEncoding = _responseEncoding;

            ManifestResourceHelper.WriteResourceToStream(response.OutputStream, _resourceName);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
