using WebAssembly;
using System.Drawing;

namespace Samples
{
    public interface ISample
    {
        string Description { get; }

        void Run(JSObject canvas, float canvasWidth, float canvasHeight, Color clearColor);
    }
}