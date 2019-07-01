using WebAssembly;
using WebGLDotNET;
using Xunit;

namespace Tests
{
    public class WebGL1Tests
    {
        private readonly WebGLRenderingContext gl;

        public WebGL1Tests(JSObject canvas)
        {
            gl = new WebGLRenderingContext(canvas);
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/5
        public void GetErrorRegressionTest()
        {
            var error = gl.GetError();

            Assert.Equal(WebGLRenderingContextBase.NO_ERROR, error);
        }

        public void BufferSubDataRegressionTest()
        {
            var buffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, buffer);
            gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, 1024, WebGLRenderingContextBase.STATIC_DRAW);
            var data = new float[] { 0 };

            gl.BufferSubData(WebGLRenderingContextBase.ARRAY_BUFFER, 512, data);
        }
    }
}
