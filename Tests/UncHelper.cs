using Pri.LongPath;

namespace Tests
{
    public static class UncHelper
    {
        public static string GetUncFromPath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            return string.Format(@"\\localhost\{0}$\{1}", fullPath[0], fullPath.Substring(3));
        }
    }
}