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

        private static string fullscreenDivCanvasName;
        private static string fullscreenCanvasName;
        private static ISample fullscreenSample;

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

                var divCanvasName = $"div_canvas_{sampleName}";
                var canvasName = $"canvas_{sampleName}";
                using (var canvas = HtmlHelper.AddCanvas(divCanvasName, canvasName, CanvasWidth, CanvasHeight))
                {
                    sample.Init(canvas, CanvasColor);
                    sample.Run();

                    if (sample.EnableFullScreen)
                    {
                        var fullscreenButtonName = $"fullscreen_{sampleName}";
                        HtmlHelper.AddButton(fullscreenButtonName, "Enter fullscreen");
                        AddFullScreenHandler(sample, fullscreenButtonName, divCanvasName, canvasName);
                    }
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
                        var fullscreenElement = (JSObject)d.GetObjectProperty("fullscreenElement");
                        var divCanvasObject = (JSObject)d.Invoke("getElementById", fullscreenDivCanvasName);
                        var canvasObject = (JSObject)d.Invoke("getElementById", fullscreenCanvasName);

                        if (fullscreenElement != null)
                        {
                            //TODO: Not working :S
                            //var rect = (JSObject)divCanvasObject.Invoke("getBoundingClientRect");

                            //var width = (JSObject)rect.GetObjectProperty("width");
                            //var height = (JSObject)rect.GetObjectProperty("height");

                            var newWidth = 1024;
                            var newHeight = 768;

                            SetNewCanvasSize(canvasObject, newWidth, newHeight);
                            fullscreenSample.Resize(newWidth, newHeight);
                        }
                        else
                        {
                            SetNewCanvasSize(canvasObject, CanvasWidth, CanvasHeight);
                            fullscreenSample.Resize(CanvasWidth, CanvasHeight);
                        }
                    }

                    o.Dispose();
                }), false);
            }
        }

        private static void SetNewCanvasSize(JSObject canvasObject, int newWidth, int newHeight)
        {
            canvasObject.SetObjectProperty("width", newWidth);
            canvasObject.SetObjectProperty("height", newHeight);
        }

        private static void AddFullScreenHandler(ISample sample, string fullscreenButtonName, string divCanvasName, string canvasName)
        {
            HtmlHelper.AttachButtonOnClickEvent(fullscreenButtonName, new Action<JSObject>((o) =>
            {
                fullscreenSample = sample;
                fullscreenCanvasName = canvasName;
                fullscreenDivCanvasName = divCanvasName;

                using (var document = (JSObject)Runtime.GetGlobalObject("document"))
                using (var div = (JSObject)document.Invoke("getElementById", divCanvasName))
                {
                    div.Invoke("requestFullscreen");
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
