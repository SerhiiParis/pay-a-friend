using System.Linq;

namespace Pay.Shared.Extensions
{
    public static class StringExtensions
    {
        public static string Combine(this string uri, params string[] segments)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return null;

            if (segments == null || segments.Length == 0)
                return uri;

            return segments.Aggregate(
                uri,
                (current, segment) => $"{current.TrimEnd('/')}/{segment.TrimStart('/')}");
        }
    }
}