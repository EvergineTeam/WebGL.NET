using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebAssembly;

namespace Samples
{
    public static class WasmResourceLoader
    {
        public static string GetBaseAddress()
        {
            using (var window = (JSObject)Runtime.GetGlobalObject("window"))
            using (var location = (JSObject)window.GetObjectProperty("location"))
            {
                return (string)location.GetObjectProperty("origin");
            }
        }

        public static async Task<byte[]> LoadResource(string file, string baseAddress)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri(baseAddress) };

            try
            {
                var rspMsg = await httpClient.GetAsync(file);
                if (rspMsg.IsSuccessStatusCode)
                {
                    var content = await rspMsg.Content.ReadAsByteArrayAsync();
                    return content;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.ToString()}");
            }

            return null;
        }
    }
}
