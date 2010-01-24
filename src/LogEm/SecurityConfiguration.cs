using System;
using System.Collections;
using System.Globalization;

namespace LogEm
{
    [ Serializable ]
    internal sealed class SecurityConfiguration
    {
        public static readonly SecurityConfiguration Default;

        private readonly bool _allowRemoteAccess;

        private static readonly string[] _trues = new string[] { "true", "yes", "on", "1" };

        static SecurityConfiguration()
        {
            Default = new SecurityConfiguration((IDictionary)Configuration.GetSubsection("security"));
        }
        
        public SecurityConfiguration(IDictionary options)
        {
            _allowRemoteAccess = GetBoolean(options, "allowRemoteAccess");
        }
        
        public bool AllowRemoteAccess
        {
            get { return _allowRemoteAccess; }
        }

        private static bool GetBoolean(IDictionary options, string name)
        {
            string str = GetString(options, name).Trim().ToLower(CultureInfo.InvariantCulture);
            return Boolean.TrueString.Equals(StringTranslation.Translate(Boolean.TrueString, str, _trues));
        }

        private static string GetString(IDictionary options, string name)
        {
            Debug.Assert(name != null);

            if (options == null)
                return string.Empty;

            object value = options[name];

            if (value == null)
                return string.Empty;

            return Mask.NullString(value.ToString());
        }
    }
}