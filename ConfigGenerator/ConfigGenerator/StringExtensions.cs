namespace ConfigGenerator
{
    public static class StringExtensions
    {
        public static string ToServiceName(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var parts = s.Split('-');
            if (parts.Length < 2) return string.Empty;
            return parts[1];
        }

        public static string ToConfigKey(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var parts = s.Split(" ");
            if (parts.Length < 2) return string.Empty;
            return parts[1];
        }

        public static string ToEnvKey(this string s)
        {
            return $"ENV_{s}";
        }

        public static string ToExportKey(this string s)
        {
            return $"export {s}";
        }
    }
}
