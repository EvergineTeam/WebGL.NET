using WebAssembly;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var canvas = CreateCanvasElement();
            var canvasWidth = canvas.GetObjectProperty("width");
            var canvasHeight = canvas.GetObjectProperty("height");

            //Triangle.Run(canvas, canvasWidth, canvasHeight);
            RotatingCube.Run(canvas, canvasWidth, canvasHeight);
        }

        static JSObject CreateCanvasElement()
        {
            var document = Runtime.GetGlobalObject("document") as JSObject;
            var canvas = (JSObject)document.Invoke("createElement", "canvas");
            var body = (JSObject)document.GetObjectProperty("body");
            body.Invoke("appendChild", canvas);

            return canvas;
        }
    }
}
