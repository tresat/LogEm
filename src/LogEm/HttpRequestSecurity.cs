using System;
using System.Web;

namespace LogEm 
{
    /// <summary>
    /// Security-related helper methods for web requests.
    /// </summary>

    public sealed class HttpRequestSecurity
    {
        /// <summary>
        /// Determines whether the request is from the local computer or not.
        /// </summary>
        /// <remarks>
        /// This method is primarily for .NET Framework 1.x where the
        /// <see cref="HttpRequest.IsLocal"/> was not available.
        /// </remarks>

        public static bool IsLocal(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

#if NET_1_0 || NET_1_1

            string userHostAddress = Mask.NullString(request.UserHostAddress);
            
            return userHostAddress.Equals("127.0.0.1") /* IP v4 */ || 
                userHostAddress.Equals("::1") /* IP v6 */ ||
                userHostAddress.Equals(request.ServerVariables["LOCAL_ADDR"]);
#else
            return request.IsLocal;
#endif
        }

        private HttpRequestSecurity()
        {
            throw new NotSupportedException();
        }
    }
}
