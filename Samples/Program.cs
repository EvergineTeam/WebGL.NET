using Humanizer;
using Samples.Helpers;
using System;
using System.Reflection;
using WaveEngine.Common.Math;
using WebAssembly;

namespace Samples
{
    class Program
    {
        const int CanvasWidth = 640;
        const int CanvasHeight = 480;

        static ISample[] samples;
        static Action<double> loop = new Action<double>(Loop);
        static double previousMilliseconds;
        static JSObject window;

        static void Main()
        {
            HtmlHelper.AddHeader1("WebGL.NET Samples Gallery");

            // Let's first check if we can continue with WebGL2 instead of crashing.
            if (!isBrowserSupportsWebGL2())
            {
                HtmlHelper.AddParagraph("We are sorry, but your browser does not seem to support WebGL2.");
                HtmlHelper.AddParagraph("See the <a href=\"https://github.com/WaveEngine/WebGL.NET\">GitHub repo</a>.");
                return;
            }

            HtmlHelper.AddParagraph(
                "A collection of WebGL samples translated from .NET/C# into WebAssembly. " +
                "See the <a href=\"https://github.com/WaveEngine/WebGL.NET\">GitHub repo</a>.");

            samples = new ISample[]
            {
                new Triangle(),
                new RotatingCube(),
                new Texture2D(),
                new TexturedCubeFromHTMLImage(),
                new TexturedCubeFromAssets(),
                // TODO: Report issue with monolinker (remove Linker workaround project)
                new LoadGLTF(),
                new TransformFeedback(),
                new PointerLock(),
            };

            foreach (var item in samples)
            {
                var name = item.GetType().Name;

                HtmlHelper.AddHeader2(name);
                HtmlHelper.AddParagraph(item.Description);
                if (item is TransformFeedback)
                    HtmlHelper.AddButton("transformNext", "Next");

                if (item.LazyLoad)
                    HtmlHelper.AddButton($"load_{name}", "Load sample");

                using (var canvas = HtmlHelper.AddCanvas(name, CanvasWidth, CanvasHeight))
                {
                    item.Init(canvas, CanvasWidth, CanvasHeight, new Vector4(255, 0, 255, 255));
                    if (!item.LazyLoad)
                    {
                        item.Run();
                    }
                }
            }

            AddGenerationStamp();

            RequestAnimationFrame();
        }

        static void Loop(double milliseconds)
        {
            var elapsedMilliseconds = milliseconds - previousMilliseconds;
            previousMilliseconds = milliseconds;

            foreach (var item in samples)
            {
                item.Update(elapsedMilliseconds);
                item.Draw();
            }

            RequestAnimationFrame();
        }

        static void RequestAnimationFrame()
        {
            if (window == null)
            {
                window = (JSObject)Runtime.GetGlobalObject();
            }

            window.Invoke("requestAnimationFrame", loop);
        }

        static bool isBrowserSupportsWebGL2()
        {
            if (window == null)
            {
                window = (JSObject)Runtime.GetGlobalObject();
            }

            // This is a very simple check for WebGL2 support.
            return window.GetObjectProperty("WebGL2RenderingContext") != null;
        }

        static void AddGenerationStamp()
        {
            var buildDate = StampHelper.GetBuildDate(Assembly.GetExecutingAssembly());
            HtmlHelper.AddParagraph($"Generated on {buildDate.ToString()} ({buildDate.Humanize()})");

            var commitHash = StampHelper.GetCommitHash(Assembly.GetExecutingAssembly());
            if (!string.IsNullOrEmpty(commitHash))
            {
                HtmlHelper.AddParagraph($"From git commit: {commitHash}");
            }
        }
    }
}
