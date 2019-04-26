using System;
using System.Drawing;
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

        static void Main(string[] args)
        {
            AddHeader1("WebGL.NET Samples Gallery");
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
                //new TransformFeedback(),
            };

            foreach (var item in samples)
            {
                AddHeader2(item.GetType().Name);
                AddParagraph(item.Description);

                using (var canvas = AddCanvas(CanvasWidth, CanvasHeight))
                {
                    item.Run(canvas, CanvasWidth, CanvasHeight, Color.Fuchsia);
                }
            }

            RequestAnimationFrame();
        }

        static JSObject AddCanvas(int width, int height)
        {
            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            using (var body = (JSObject)document.GetObjectProperty("body"))
            {
                var canvas = (JSObject)document.Invoke("createElement", "canvas");
                canvas.SetObjectProperty("width", width);
                canvas.SetObjectProperty("height", height);
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
