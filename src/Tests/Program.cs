using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAssembly;

namespace Tests
{
    public class Program
    {
        public static void Main()
        {
            var testTypes = new Type[] { typeof(WebGL1Tests), typeof(WebGL2Tests) };
            var dictionary = DiscoverTests(testTypes);
            var testCount = dictionary.Values.Sum(tests => tests.Count());

            Console.WriteLine($"{testCount} tests found");

            var testsPassed = 0;

            foreach (var type in dictionary.Keys)
            {
                var tests = dictionary[type];

                foreach (var test in tests)
                {
                    Console.WriteLine($"Running '{type.Name}.{test.Name}'...");

                    var isInconclusive = false;
                    var isFailed = false;

                    try
                    {
                        using (var document = (JSObject)Runtime.GetGlobalObject("document"))
                        using (var body = (JSObject)document.GetObjectProperty("body"))
                        using (var canvas = (JSObject)document.Invoke("createElement", "canvas"))
                        {
                            var instance = Activator.CreateInstance(type, canvas);
                            test.Invoke(instance, null);
                        }
                    }
                    catch (TargetInvocationException exception) when (exception.InnerException is InconclusiveException)
                    {
                        Console.WriteLine($"Inconclusive: {exception.InnerException.Message}");
                        isInconclusive = true;
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                        isFailed = true;
                    }

                    if (isInconclusive)
                    {
                        continue;
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

            Console.WriteLine($"{testsPassed}/{testCount} tests passed");
        }

        private static Dictionary<Type, IEnumerable<MethodInfo>> DiscoverTests(Type[] testsTypes)
        {
            var dictionary = new Dictionary<Type, IEnumerable<MethodInfo>>();

            foreach (var item in testsTypes)
            {
                var methods = item
                    .GetMethods()
                    .Where(method => method.Name.EndsWith("Test", StringComparison.InvariantCulture));
                dictionary.Add(item, methods);
            }

            return dictionary;
        }
    }
}
