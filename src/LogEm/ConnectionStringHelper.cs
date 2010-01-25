using System;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Runtime.CompilerServices;

using IDictionary = System.Collections.IDictionary;

namespace LogEm
{
    /// <summary>
    /// Helper class for resolving connection strings.
    /// </summary>

    internal class ConnectionStringHelper
    {
        /// <summary>
        /// Gets the connection string from the given configuration 
        /// dictionary.
        /// </summary>

        public static string GetConnectionString(IDictionary config)
        {
            Debug.Assert(config != null);

#if !NET_1_1 && !NET_1_0
            //
            // First look for a connection string name that can be 
            // subsequently indexed into the <connectionStrings> section of 
            // the configuration to get the actual connection string.
            //

            string connectionStringName = (string)config["connectionStringName"] ?? string.Empty;

            if (connectionStringName.Length > 0)
            {
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];

                if (settings == null)
                    return string.Empty;

                return settings.ConnectionString ?? string.Empty;
            }
#endif

            //
            // Connection string name not found so see if a connection 
            // string was given directly.
            //

            string connectionString = Mask.NullString((string)config["connectionString"]);

            if (connectionString.Length > 0)
                return connectionString;

            //
            // As a last resort, check for another setting called 
            // connectionStringAppKey. The specifies the key in 
            // <appSettings> that contains the actual connection string to 
            // be used.
            //

            string connectionStringAppKey = Mask.NullString((string)config["connectionStringAppKey"]);

            if (connectionStringAppKey.Length == 0)
                return string.Empty;

            return Configuration.AppSettings[connectionStringAppKey];
        }

#if NET_1_1 || NET_1_0
        /// <summary>
        /// Extracts the Data Source file path from a connection string
        /// </summary>
        /// <param name="connectionString">The connection string</param>
        /// <returns>File path to the Data Source element of a connection string</returns>
        public static string GetDataSourceFilePath(string connectionString)
        {
            Debug.AssertStringNotEmpty(connectionString);

            string result = string.Empty;
            string loweredConnectionString = connectionString.ToLower();
            int dataSourcePosition = loweredConnectionString.IndexOf("data source");
            if (dataSourcePosition >= 0)
            {
                int equalsPosition = loweredConnectionString.IndexOf('=', dataSourcePosition);
                if (equalsPosition >= 0)
                {
                    int semiColonPosition = loweredConnectionString.IndexOf(';', equalsPosition);
                    if (semiColonPosition < equalsPosition)
                        result = connectionString.Substring(equalsPosition + 1);
                    else
                        result = connectionString.Substring(equalsPosition + 1, semiColonPosition - equalsPosition - 1);
                    result = result.Trim();
                    char firstChar = result[0];
                    char lastChar = result[result.Length - 1];
                    if (firstChar == lastChar && (firstChar == '\'' || firstChar == '\"') && result.Length > 1)
                    {
                        result = result.Substring(1, result.Length - 2);
                    }
                }
            }

            return result;
        }
#else
        /// <summary>
        /// Extracts the Data Source file path from a connection string
        /// ~/ gets resolved as does |DataDirectory|
        /// </summary>

        public static string GetDataSourceFilePath(string connectionString)
        {
            Debug.AssertStringNotEmpty(connectionString);

            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            return GetDataSourceFilePath(builder, connectionString);
        }

        /// <summary>
        /// Gets the connection string from the given configuration,
        /// resolving ~/ and DataDirectory if necessary.
        /// </summary>

        public static string GetConnectionString(IDictionary config, bool resolveDataSource)
        {
            string connectionString = GetConnectionString(config);
            return resolveDataSource ? GetResolvedConnectionString(connectionString) : connectionString;
        }

        /// <summary>
        /// Converts the supplied connection string so that the Data Source 
        /// specification contains the full path and not ~/ or DataDirectory.
        /// </summary>

        public static string GetResolvedConnectionString(string connectionString)
        {
            Debug.AssertStringNotEmpty(connectionString);

            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder["Data Source"] = GetDataSourceFilePath(builder, connectionString);
            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string MapPath(string path)
        {
            return System.Web.Hosting.HostingEnvironment.MapPath(path);
        }

        private static string GetDataSourceFilePath(DbConnectionStringBuilder builder, string connectionString)
        {
            builder.ConnectionString = connectionString;
            if (!builder.ContainsKey("Data Source"))
                throw new ArgumentException("A 'Data Source' parameter was expected in the supplied connection string, but it was not found.");
            string dataSource = builder["Data Source"].ToString();
            return ResolveDataSourceFilePath(dataSource);
        }

        private static readonly char[] _dirSeparators = new char[] { Path.DirectorySeparatorChar };

        private static string ResolveDataSourceFilePath(string path)
        {
            const string dataDirectoryMacroString = "|DataDirectory|";

            //
            // Check to see if it starts with a ~/ and if so map it and return it.
            //

            if (path.StartsWith("~/"))
                return MapPath(path);

            //
            // Else see if it uses the DataDirectory macro/substitution 
            // string, and if so perform the appropriate substitution.
            //

            if (!path.StartsWith(dataDirectoryMacroString, StringComparison.OrdinalIgnoreCase))
                return path;

            //
            // Look-up the data directory from the current AppDomain.
            // See "Working with local databases" for more:
            // http://blogs.msdn.com/smartclientdata/archive/2005/08/26/456886.aspx
            //

            string baseDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;

            //
            // If not, try the current AppDomain's base directory.
            //

            if (string.IsNullOrEmpty(baseDirectory))
                baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            //
            // Piece the file path back together, taking leading and 
            // trailing backslashes into account to avoid duplication.
            //

            return Mask.NullString(baseDirectory).TrimEnd(_dirSeparators)
                 + Path.DirectorySeparatorChar
                 + path.Substring(dataDirectoryMacroString.Length).TrimStart(_dirSeparators);
        }
#endif

        private ConnectionStringHelper() { }
    }
}
