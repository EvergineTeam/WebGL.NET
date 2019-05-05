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
            AddHeader1("WebGL.NET Samples Gallery");

            // Let's first check if we can continue with WebGL2 instead of crashing.
            if (!isBrowserSupportsWebGL2())
            {
                AddParagraph("We are sorry, but your browser does not seem to support WebGL2.");
                AddParagraph("See the <a href=\"https://github.com/WaveEngine/WebGL.NET\">GitHub repo</a>.");
                return;
            }

            AddParagraph(
                "A collection of WebGL samples translated from .NET/C# into WebAssembly. " +
                "See the <a href=\"https://github.com/WaveEngine/WebGL.NET\">GitHub repo</a>.");

            samples = new ISample[]
            {
                new Triangle(),
                new RotatingCube(),
                new Texture2D(),
                new TexturedCubeFromHTMLImage(),
                new TexturedCubeFromAssets(),
                //new LoadGLTF(), // json.net issue
                new TransformFeedback(),
                new PointerLock(),
            };

            foreach (var item in samples)
            {
                var name = item.GetType().Name;

                AddHeader2(name);
                AddParagraph(item.Description);
                if (item is TransformFeedback)
                    AddButton("transformNext", "Next");

                if (item.LazyLoad)
                    AddButton($"load_{name}", "Load sample");

                using (var canvas = AddCanvas(name, CanvasWidth, CanvasHeight))
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

        private static void AddGenerationStamp()
        {
            var buildDate = TimestampHelper.GetBuildDate(Assembly.GetExecutingAssembly());
            AddParagraph($"Generated on {buildDate.ToString()} ({buildDate.Humanize()})");
            AddParagraph($"From commit: {ThisAssembly.GitCommitId}");
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

        static JSObject AddCanvas(string id, int width, int height)
        {
            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            using (var body = (JSObject)document.GetObjectProperty("body"))
            {
                var canvas = (JSObject)document.Invoke("createElement", "canvas");
                canvas.SetObjectProperty("width", width);
                canvas.SetObjectProperty("height", height);
                canvas.SetObjectProperty("id", id);
                body.Invoke("appendChild", canvas);

                return canvas;
            }
        }

        static void AddHeader(int headerIndex, string text)
        {
            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            using (var body = (JSObject)document.GetObjectProperty("body"))
            using (var header = (JSObject)document.Invoke("createElement", $"h{headerIndex}"))
            using (var headerText = (JSObject)document.Invoke("createTextNode", text))
            {
                header.Invoke("appendChild", headerText);
                body.Invoke("appendChild", header);
            }
        }

        static void AddHeader1(string text) => AddHeader(1, text);

        static void AddHeader2(string text) => AddHeader(2, text);

        static void AddParagraph(string text)
        {
            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            using (var body = (JSObject)document.GetObjectProperty("body"))
            using (var paragraph = (JSObject)document.Invoke("createElement", "p"))
            {
                paragraph.SetObjectProperty("innerHTML", text);
                body.Invoke("appendChild", paragraph);
            }
        }

        static void AddButton(string id, string text)
        {
            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            using (var body = (JSObject)document.GetObjectProperty("body"))
            using (var button = (JSObject)document.Invoke("createElement", "button"))
            {
                button.SetObjectProperty("innerHTML", text);
                button.SetObjectProperty("id", id);
                body.Invoke("appendChild", button);
            }
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
    }
}
