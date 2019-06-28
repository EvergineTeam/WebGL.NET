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
            var testsCount = testMethods.Count();

            Console.WriteLine($"{testsCount} tests found");

            var testsPassed = 0;

            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            using (var body = (JSObject)document.GetObjectProperty("body"))
            using (var canvas = (JSObject)document.Invoke("createElement", "canvas"))
            {
                foreach (var item in testMethods)
                {
                    Console.WriteLine($"Running '{item.Name}'...");

                    var isFailed = false;

                    try
                    {
                        var instance = Activator.CreateInstance(testsType, canvas);
                        item.Invoke(instance, null);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        isFailed = true;
                    }

                    if (isFailed)
                    {
                        Console.WriteLine($"Failed!");
                    }
                    else
                    {
                        Console.WriteLine($"Passed!");
                        testsPassed++;
                    }
                }
            }

            Console.WriteLine($"{testsPassed}/{testsCount} tests passed");
        }
    }
}
