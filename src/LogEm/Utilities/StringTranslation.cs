using System;

namespace LogEm.Utilities
{
    internal static class StringUtils
    {
        #region "Public Functionality"
        /// <summary>
        /// Provides translation from multiple representations of a string to a
        /// single base representation.
        /// </summary>
        public static string Translate(string translation, string input, string[] faces)
        {
            return Array.IndexOf(faces, input) >= 0 ? translation : input;
        }
        #endregion
    }
}