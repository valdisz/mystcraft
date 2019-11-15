namespace atlantis
{
    using System;

    public static class StringExtensions {
        public static bool StartsWithIC(this string s, string substr) {
            return s.StartsWith(substr, StringComparison.OrdinalIgnoreCase);
        }
    }
}
