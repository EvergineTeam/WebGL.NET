using System.Drawing;
using WebAssembly;

namespace Samples
{
    public interface ISample
    {
        string Description { get; }

        void Run(JSObject canvas, float canvasWidth, float canvasHeight, Color clearColor);

        void Update(double elapsedTime);

        void Draw();
    }
}