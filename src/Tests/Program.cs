using System;
using System.Linq;
using WebAssembly;

namespace Tests
{
    public class Program
    {
        public static void Main()
        {
            var testsType = typeof(TheTests);
            var testMethods = testsType
                .GetMethods()
                .Where(method => method.Name.EndsWith("Test", StringComparison.InvariantCulture));

            Console.WriteLine($"{testMethods.Count()} tests found");

            var testsPassed = 0;

            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            using (var body = (JSObject)document.GetObjectProperty("body"))
            using (var canvas = (JSObject)document.Invoke("createElement", "canvas"))
            {
                var instance = Activator.CreateInstance(testsType, canvas);

                foreach (var item in testMethods)
                {
                    Console.WriteLine($"Running '{item.Name}'...");

                    item.Invoke(instance, null);

                    Console.WriteLine($"Passed!");

                    testsPassed++;
                }
            }

            Console.WriteLine($"{testsPassed} tests passed");
        }
    }
}
