using System;

namespace LogEm 
{
    /// <summary>
    /// Provides translation from multiple representations of a string to a
    /// single base representation.
    /// </summary>

    internal sealed class StringTranslation
    {
        public static string Translate(string translation, string input, string[] faces)
        {
            return Array.IndexOf(faces, input) >= 0 ? translation : input;
        }

        private StringTranslation() { }
    }
}