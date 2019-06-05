using WaveEngine.Common.Math;
using WebAssembly;

namespace Samples
{
    public interface ISample
    {
        string Description { get; }

        bool EnableFullScreen { get; }

        void Init(JSObject canvas, Vector4 vector4);

        void Resize(int width, int height);

        void Run();

        void Update(double elapsedTime);

        void Draw();
    }
}