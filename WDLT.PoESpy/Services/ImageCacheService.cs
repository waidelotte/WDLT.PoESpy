using System;
using System.IO;
using WDLT.Utils.Extensions;

namespace WDLT.PoESpy.Services
{
    public static class ImageCacheService
    {
        public static void CreateDirectories()
        {
            Path.Combine(AppContext.BaseDirectory, "cache/images/").CreateDirectory();
        }

        public static string Get(string name)
        {
            return Path.Combine(AppContext.BaseDirectory, $"cache/images/{name}.png");
        }

        public static bool Exist(string name)
        {
            return File.Exists(Get(name));
        }
    }
}