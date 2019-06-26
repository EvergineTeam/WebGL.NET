using System.Threading.Tasks;
using WaveEngine.Common.Math;
using WebAssembly;

namespace Samples
{
    public interface ISample
    {
        string Description { get; }

        bool EnableFullScreen { get; }

        Task InitAsync(JSObject canvas, Vector4 clearColor);

        void Resize(int width, int height);

        void Run();

        void Update(double elapsedTime);

        void Draw();
    }
}