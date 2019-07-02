using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAssembly;
using WebGLDotNET;

namespace Tests
{
    internal class Program
    {
        private static void Main()
        {
            var testTypes = new Type[] { typeof(WebGL1Tests), typeof(WebGL2Tests) };
            var dictionary = DiscoverTests(testTypes);
            var testCount = dictionary.Values.Sum(tests => tests.Count());

            Print($"{testCount} tests found");

            var testsPassed = 0;

            foreach (var type in dictionary.Keys)
            {
                var tests = dictionary[type];

                foreach (var test in tests)
                {
                    Print($"Running '{type.Name}.{test.Name}'...");

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
                        Print($"Inconclusive: {exception.InnerException.Message}");
                        isInconclusive = true;
                    }
                    catch (Exception exception)
                    {
                        Print(exception.ToString());
                        isFailed = true;
                    }

                    if (isInconclusive)
                    {
                        continue;
                    }

                    if (isFailed)
                    {
                        Print($"Failed!");
                    }
                    else
                    {
                        Print($"Passed!");
                        testsPassed++;
                    }
                }
            }

            Print($"{testsPassed}/{testCount} tests passed");
        }

        private static void Print(string message)
        {
            HtmlHelper.AddParagraph(message);

            Console.WriteLine(message);
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
