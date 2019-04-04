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

        static void Main(string[] args)
        {
            AddHeader1("WebGL.NET Samples Gallery");
            AddParagraph(
                "A collection of WebGL samples translated from .NET/C# into WebAssembly. " +
                "See the <a href=\"https://github.com/MarcosCobena/WebGL.NET\">GitHub repo</a>.");

            samples = new ISample[]
            {
                new Triangle(),
                new RotatingCube(),
                new Texture2D(),
                new TexturedCubeFromHTMLImage(),
                new TexturedCubeFromAssets(),
            };

            foreach (var item in samples)
            {
                AddHeader2(item.GetType().Name);
                AddParagraph(item.Description);
                var canvas = AddCanvas(CanvasWidth, CanvasHeight);
                item.Run(canvas, CanvasWidth, CanvasHeight, Color.Fuchsia);
            }

            RequestAnimationFrame();
        }

        static JSObject AddCanvas(int width, int height)
        {
            var document = (JSObject)Runtime.GetGlobalObject("document");
            var body = (JSObject)document.GetObjectProperty("body");
            var canvas = (JSObject)document.Invoke("createElement", "canvas");
            canvas.SetObjectProperty("width", width);
            canvas.SetObjectProperty("height", height);
            body.Invoke("appendChild", canvas);

            return canvas;
        }

        static void AddHeader(int headerIndex, string text)
        {
            var document = (JSObject)Runtime.GetGlobalObject("document");
            var body = (JSObject)document.GetObjectProperty("body");
            var header = (JSObject)document.Invoke("createElement", $"h{headerIndex}");
            var headerText = (JSObject)document.Invoke("createTextNode", text);
            header.Invoke("appendChild", headerText);
            body.Invoke("appendChild", header);
        }

        static void AddHeader1(string text) => AddHeader(1, text);

        static void AddHeader2(string text) => AddHeader(2, text);

        static void AddParagraph(string text)
        {
            var document = (JSObject)Runtime.GetGlobalObject("document");
            var body = (JSObject)document.GetObjectProperty("body");
            var paragraph = (JSObject)document.Invoke("createElement", "p");
            paragraph.SetObjectProperty("innerHTML", text);
            body.Invoke("appendChild", paragraph);
        }

        static void Loop(double milliseconds)
        {
            if (milliseconds < 0)
            {
                return;
            }

            foreach (var item in samples)
            {
                var elapsedMilliseconds = milliseconds - item.OldMilliseconds;
                item.Update(elapsedMilliseconds);
                item.OldMilliseconds = milliseconds;
                item.Draw();
            }
        }

        static void AnimateLoop(double milliseconds)
        {
            if (milliseconds < 0)
            {
                // Should this request an animation frame?
                return;
            }

            foreach (var item in samples)
            {
                var elapsedMilliseconds = milliseconds - item.OldMilliseconds;
                item.Update(elapsedMilliseconds);
                item.OldMilliseconds = milliseconds;
                item.Draw();
            }
            RequestAnimationFrame();
        }

        static Action<double> animate = new Action<double>(AnimateLoop);
        static JSObject window = null;
        static void RequestAnimationFrame()
        {
            if (window == null)
                window = (JSObject)Runtime.GetGlobalObject();
            window.Invoke("requestAnimationFrame", animate);
        }
    }
}
