using WebAssembly;
using WebAssembly.Core;
using WebGLDotNET;
using Xunit;

namespace Tests
{
    public class WebGLBackendTests : BaseTests
    {
        private readonly JSObject canvas;

        public WebGLBackendTests(JSObject canvas)
        {
            this.canvas = canvas;
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/8
        public void UIntArrayCastRegressionTest()
        {
            var gl = new WebGLRenderingContext(canvas);
            var array = new uint[1];
            
            var result = gl.CastNativeArray(array);

            Assert.IsType<Uint32Array>(result);
        }

        public void ContextAttributesTest()
        {
            var contextAttributes = new WebGLContextAttributes { Stencil = true };
            var gl = new WebGLRenderingContext(canvas, contextAttributes);

            var incomingContextAttributes = gl.GetContextAttributes();

            Assert.Equal(true, incomingContextAttributes.Stencil);
        }
    }
}
