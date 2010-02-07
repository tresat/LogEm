using System;
using System.Collections.Generic;

namespace LogEm.Utilities
{
    internal static class StringUtils
    {
        #region "Public Functionality"
        /// <summary>
        /// Provides translation from multiple representations of a string/concept to a
        /// single canonical representation.
        /// </summary>
        /// <param name="canonical">The canonical representation</param>
        /// <param name="input">The string to check</param>
        /// <param name="faces">The possibilities (synonyms)</param>
        /// <returns>The canonical representation if a match, else the input.</returns>
        public static string Translate(string canonical, string input, String[] faces)
        {
            return Array.IndexOf(faces, input) >= 0 ? canonical : input;
        }

        public static string IfNull(string s)
        {
            return s == null ? string.Empty : s;
        }

        public static string IfNull(string s, string replacement)
        {
            return s == null ? replacement : s;
        }

        public static string IfEmpty(string s, string replacement)
        {
            return s.Length == 0 ? replacement : s;
        }

        public static string IfNullOrEmpty(string s)
        {
            return StringUtils.IfNull(s).Length == 0 ? string.Empty : s;
        }

        public static string IfNullOrEmpty(string s, string replacement)
        {
            return StringUtils.IfNull(s).Length == 0 ? replacement : s;
        }
        #endregion
    }
}