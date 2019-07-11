using WebAssembly;
using WebAssembly.Core;
using WebGLDotNET;
using Xunit;

namespace Tests
{
    public class WebGLBackendTests : BaseTests
    {
        private readonly WebGLRenderingContext gl;

        public WebGLBackendTests(JSObject canvas)
        {
            gl = new WebGLRenderingContext(canvas);
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/8
        public void UIntArrayCastRegressionTest()
        {
            var array = new uint[1];
            
            var result = gl.CastNativeArray(array);

            Assert.IsType<Uint32Array>(result);
        }
    }
}
