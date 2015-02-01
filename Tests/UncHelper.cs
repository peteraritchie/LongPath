using Pri.LongPath;

namespace Tests
{
    public static class UncHelper
    {
        public static string GetUncFromPath(string path)
        {
            var fullPath = Path.GetFullPath(path);
            return fullPath;
        }
    }
}