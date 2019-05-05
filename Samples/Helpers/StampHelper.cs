using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Samples.Helpers
{
    public static class StampHelper
    {
        // https://www.meziantou.net/2018/09/24/getting-the-date-of-build-of-a-net-assembly-at-runtime
        public static DateTime GetBuildDate(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(x => x.Key == "BuildDate");
            if (attribute != null)
            {
                if (DateTime.TryParseExact(
                    attribute.Value, 
                    "yyyyMMddHHmmss", 
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }

            return default(DateTime);
        }

        public static string GetCommitHash(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(x => x.Key == "CommitHash");
            if (attribute != null)
            {
                return attribute.Value;
            }

            return string.Empty;
        }
    }
}
