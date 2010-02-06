using System;
using System.Web;

namespace LogEm 
{
    /// <summary>
    /// Provides an abstract base class for <see cref="IHttpModule"/> that
    /// supports discovery from within partial trust environments.
    /// </summary>
    public abstract class HttpModuleBase : IHttpModule
    {
        void IHttpModule.Init(HttpApplication context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (SupportDiscoverability)
                HttpModuleRegistry.RegisterInPartialTrust(context, this);

            OnInit(context);
        }

        void IHttpModule.Dispose()
        {
            OnDispose();
        }

        /// <summary>
        /// Determines whether the module will be registered for discovery
        /// in partial trust environments or not.
        /// </summary>
        protected virtual bool SupportDiscoverability
        {
            get { return false; }
        }

        /// <summary>
        /// Initializes the module and prepares it to handle requests.
        /// </summary>
        protected virtual void OnInit(HttpApplication application) {}

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module.
        /// </summary>
        protected virtual void OnDispose() {}
    }
}