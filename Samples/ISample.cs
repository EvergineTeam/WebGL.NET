using WebAssembly;
using System.Drawing;

namespace Samples
{
    public interface ISample
    {
        string Description { get; }

        void Run(JSObject canvas, int canvasWidth, int canvasHeight, Color clearColor);
    }
}