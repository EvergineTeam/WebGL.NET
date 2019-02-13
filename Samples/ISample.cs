using WebAssembly;

namespace Samples
{
    public interface ISample
    {
        void Run(JSObject canvas, int canvasWidth, int canvasHeight);
    }
}