using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace LogEm.Utilities
{
    internal sealed class BuildInfo
    {
        // Was this built as a debug build?
#if DEBUG
        public const bool IsDebug = true;
        public const string Type = "Debug";
        public const string TypeUppercase = "DEBUG";
        public const string TypeLowercase = "debug";
#else
        public const bool IsDebug = false;
        public const string Type = "Release";
        public const string TypeUppercase = "RELEASE";
        public const string TypeLowercase = "release";
#endif

        // What version of .net was it built under?
#if NET_1_0
        public const string NETFrameworkVersion = ".NET 1.0";
#elif NET_1_1
        public const string NETFrameworkVersion = ".NET 1.1";
#elif NET_2_0
        public const string NETFrameworkVersion = ".NET 2.0";
#elif NET_3_5
        public const string NETFrameworkVersion = ".NET 3.5";
#elif NET_4_0
        public const string NETFrameworkVersion = ".NET 4.0";
#else
        public const string NETFrameworkVersion = ".NET version unknown";
#endif
        
        // What specific version of the CLR was it built under?
#if NET_1_0
        // As Assembly.ImageRuntimeVersion property was not available
        // under .NET Framework 1.0, we just return the version 
        // hard-coded based on conditional compilation symbol.
        public static readonly ImageRuntimeVersion = "Runtime version v1.0.3705";
#else
        public static readonly string ImageRuntimeVersion = "Runtime version " + Assembly.GetExecutingAssembly().ImageRuntimeVersion;
#endif

        // This is the status or milestone of the build. Examples are
        // M1, M2, ..., Mn, BETA1, BETA2, RC1, RC2, RTM.
        public const string Milestone = "Pre-Alpha";

        // This string is read by AssemblyInfo class, and must remain constant
        public const string Configuration = Type + "; " + Milestone + "; " + NETFrameworkVersion;

        // This string has more info, but cannot be made constant
        public static readonly string ExtendedConfiguration = Type + "; " + Milestone + "; " + NETFrameworkVersion + "; " + ImageRuntimeVersion;
    
        /// <summary>
        /// Be careful when relying on version to predict build date in 
        /// Visual Studio .NET. For some reason, the IDE does not update 
        /// the build number every time you build a solution. Visual Studio 
        /// only increments the build and revision number when the solution 
        /// is closed and reopened. If you build fifty times throughout the 
        /// day in the same solution, every single one of your builds will
        /// have the same version. Close and reopen that solution, though,
        /// and you'll get a new version immediately. Go figure.
        /// </summary>
        /// <returns>Build date of the assembly.</returns>
        public static DateTime BuildDate()
        {
            // Build dates start from 1/1/2000
            DateTime result = new DateTime(2000, 1, 1);

            // Retrieve the version information from the assembly from which this code is being executed
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
        
            // Add the number of days (build)
            result = result.AddDays(version.Build);

            // Add the number of seconds since midnight (revision) multiplied by 2
            result = result.AddSeconds(version.Revision * 2);
        
            // If we're currently in daylight saving time add an extra hour
            DateTime now = DateTime.Now;
            if (TimeZone.IsDaylightSavingTime(now, TimeZone.CurrentTimeZone.GetDaylightChanges(now.Year))) {
                result.AddHours(1);
            }

            return result;
        }
       
        /// <summary>
        /// There's another third way to calculate build date that's much more 
        /// reliable. Dustin Aleksiuk recently posted a clever blog entry 
        /// describing how to retrieve the embedded linker timestamp from 
        /// the IMAGE_FILE_HEADER section of the Portable Executable header...
        /// </summary>
        /// <returns>Build date of the assembly.</returns>
        public static DateTime LinkerDate()
        {
            const int PEHeaderOffset = 60;
            const int linkerTimestampOffset = 8;

            Byte[] b = new Byte[2048] ;
            Stream s = null;

            try {
                // Get the first file in the assembly (there should always be at least 1)
                s = new FileStream(Assembly.GetCallingAssembly().Location, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            } finally {
                if (s != null) {
                    s.Close();
                }
            };

            int i = BitConverter.ToInt32(b, PEHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + linkerTimestampOffset);
            
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
              
            return dt;
        }
    }
}