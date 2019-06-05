using System.IO;
using System.Linq;
using System.Reflection;

namespace Samples.Helpers
{
    public static class EmbeddedResourceHelper
    {
        public static Stream Load(string resourceName, Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            var fullResourceName = assembly
                .GetManifestResourceNames()
                .First(resource => resource.EndsWith(resourceName));

            return assembly.GetManifestResourceStream(fullResourceName);
        }
    }
}
