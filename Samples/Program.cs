using WebAssembly;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var canvas = CreateCanvasElement();
            Triangle.Run(canvas);
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
