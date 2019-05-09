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
        static readonly Vector4 CanvasColor = new Vector4(255, 0, 255, 255);

        static ISample[] samples;

        static Action<double> loop = new Action<double>(Loop);
        static double previousMilliseconds;

        static JSObject window;
        private static string fullscreenCanvasName;

        static void Main()
        {
            // Let's first check if we can continue with WebGL2 instead of crashing.
            if (!isBrowserSupportsWebGL2())
            {
                HtmlHelper.AddParagraph("We are sorry, but your browser does not seem to support WebGL2.");
                HtmlHelper.AddParagraph("See the <a href=\"https://github.com/WaveEngine/WebGL.NET\">GitHub repo</a>.");
                return;
            }

            HtmlHelper.AddHeader1("WebGL.NET Samples Gallery");

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

            foreach (var sample in samples)
            {
                var sampleName = sample.GetType().Name;

                HtmlHelper.AddHeader2(sampleName);
                HtmlHelper.AddParagraph(sample.Description);

                var canvasName = $"canvas_{sampleName}";
                using (var canvas = HtmlHelper.AddCanvas(canvasName, CanvasWidth, CanvasHeight))
                {
                    sample.Init(canvas, CanvasColor);
                    sample.Run();

                    var fullscreenButtonName = $"fullscreen_{sampleName}";
                    HtmlHelper.AddButton(fullscreenButtonName, "Enter fullscreen");
                    AddFullScreenHandler(fullscreenButtonName, canvasName);
                }
            }

            AddEnterFullScreenHandler();

            AddGenerationStamp();

            RequestAnimationFrame();
        }

        private static void AddEnterFullScreenHandler()
        {
            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            {
                document.Invoke("addEventListener", "fullscreenchange", new Action<JSObject>((o) =>
                {
                    using (var d = (JSObject)Runtime.GetGlobalObject("document"))
                    {
                        var canvasObject = (JSObject)d.Invoke("getElementById", fullscreenCanvasName);

                        //if (document.mozFullScreen || document.webkitIsFullScreen)
                        //{
                        //    var rect = (JSObject)canvasObject.Invoke("getBoundingClientRect");

                        //    canvasObject.SetObjectProperty("width", rect.GetObjectProperty("width"));
                        //    canvasObject.SetObjectProperty("height", rect.GetObjectProperty("height"));
                        //}
                        //else
                        //{
                        //    canvasObject.SetObjectProperty("width", CanvasWidth);
                        //    canvasObject.SetObjectProperty("height", CanvasHeight);
                        //}
                    }

                    o.Dispose();
                }), false);
            }
        }

        private static void AddFullScreenHandler(string fullscreenButtonName, string canvasName)
        {
            HtmlHelper.AttachButtonOnClickEvent(fullscreenButtonName, new Action<JSObject>((o) =>
            {
                using (var document = (JSObject)Runtime.GetGlobalObject("document"))
                using (var c = (JSObject)document.Invoke("getElementById", canvasName))
                {
                    c.Invoke("requestFullscreen");

                    fullscreenCanvasName = canvasName;
                }

                o.Dispose();
            }));
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
