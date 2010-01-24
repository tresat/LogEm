using System.Collections.Specialized;
using System.Configuration;

namespace LogEm
{
    internal sealed class Configuration
    {
        internal const string GroupName = "logEm";
        internal const string GroupSlash = GroupName + "/";

        public static NameValueCollection AppSettings
        {
            get
            {
#if NET_1_0 || NET_1_1
                return ConfigurationSettings.AppSettings;
#else
                return ConfigurationManager.AppSettings;
#endif
            }
        }

        public static object GetSubsection(string name)
        {
            return GetSection(GroupSlash + name);
        }

        public static object GetSection(string name)
        {
#if NET_1_0 || NET_1_1
            return ConfigurationSettings.GetConfig(name);
#else
            return ConfigurationManager.GetSection(name);
#endif
        }

        private Configuration() { }
    }
}
