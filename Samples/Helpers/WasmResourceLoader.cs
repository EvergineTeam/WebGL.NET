using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using WebAssembly;

namespace Samples
{
    public static class WasmResourceLoader
    {
        public static string GetLocalAddress()
        {
            using (var window = (JSObject)Runtime.GetGlobalObject("window"))
            using (var location = (JSObject)window.GetObjectProperty("location"))
            {
                var address = (string)location.GetObjectProperty("href");

                if (address.Contains("/"))
                {
                    address = address.Substring(0, address.LastIndexOf('/') + 1);
                }

                return address;
            }
        }

        public static async Task<Stream> LoadAsync(string relativePath, string baseAddress)
        {
            Stream content;

            var httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };

#if DEBUG
            Console.WriteLine($"Requesting '{relativePath}' at '{baseAddress}'...");
#endif

            try
            {
                var response = await httpClient.GetAsync(relativePath);
                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStreamAsync();
            }
            catch (Exception exception)
            {
                content = null;

                Console.WriteLine($"[Error] {nameof(WasmResourceLoader)}.{nameof(LoadAsync)}(): {exception}");
            }

#if DEBUG
            if (content != null)
            {
                Console.WriteLine($"Content length: {content.Length} B");
            }
#endif

            return content;
        }
    }
}
