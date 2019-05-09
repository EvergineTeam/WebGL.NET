using WaveEngine.Common.Math;
using WebAssembly;

namespace Samples
{
    public interface ISample
    {
        string Description { get; }

        void Init(JSObject canvas, int canvasWidth, int canvasHeight, Vector4 vector4);

        void Run();

        void Update(double elapsedTime);

        void Draw();
    }
}