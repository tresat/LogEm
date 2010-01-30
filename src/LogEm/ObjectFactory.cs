using System;
using IDictionary = System.Collections.IDictionary;

namespace LogEm.Logging
{
    /// <summary>
    /// A simple factory for creating instances of types specified in a 
    /// section of the configuration file.
    /// </summary>
    internal sealed class ObjectFactory
    {
        #region "Constructors"
        /// <summary>
        /// Class is not instantiable.
        /// </summary>
        private ObjectFactory() { }
        #endregion

        #region "Public Functionality"
        /// <summary>
        /// Creates a LogEm class from the type name string stored in the web.config.
        /// </summary>
        /// <param name="pSectionName">The name of the web.config file section 
        /// specifying a type to create</param>
        /// <returns></returns>
        public static object CreateFromConfigSection(string pSectionName)
        {
            if (String.IsNullOrEmpty(pSectionName))
                throw new ArgumentException("pSectionName cannot be null or empty!");

            // Get the configuration section with the settings.
            IDictionary config = (IDictionary)Configuration.GetSection(pSectionName);
            if (config == null)
                return null;

            // We modify the settings by removing items as we consume 
            // them so make a copy here.
            config = (IDictionary) ((ICloneable) config).Clone();

            // Get the type specification of the service provider.
            string typeSpec = Mask.NullString((string) config["type"]);
            if (typeSpec.Length == 0)
                return null;

            // Remove item from config (only want to create it once).
            config.Remove("type");

            // Locate, create and return the service provider object.
            Type type = Type.GetType(typeSpec, true);
            return Activator.CreateInstance(type, new object[] { config });
        }
        #endregion
    }
}
