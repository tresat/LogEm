using System;

namespace LogEm
{
    /// <summary>
    /// Collection of utility methods for masking values.
    /// </summary>

    internal sealed class Mask
    {
        public static string NullString(string s)
        {
            return s == null ? string.Empty : s;
        }

        public static string EmptyString(string s, string filler)
        {
            return Mask.NullString(s).Length == 0 ? filler : s;
        }

        private Mask() { }
    }
}
