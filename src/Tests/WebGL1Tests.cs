using WebAssembly;
using WebGLDotNET;
using Xunit;

namespace Tests
{
    public class WebGL1Tests : BaseTests
    {
        private readonly WebGLRenderingContext gl;

        public WebGL1Tests(JSObject canvas)
        {
            gl = new WebGLRenderingContext(canvas);
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/5
        public void GetErrorRegressionTest() => this.AssertNoWebGLError();

        public void BufferSubDataRegressionTest()
        {
            var buffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, buffer);
            gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, 1024, WebGLRenderingContextBase.STATIC_DRAW);
            var data = new float[] { 0 };

            gl.BufferSubData(WebGLRenderingContextBase.ARRAY_BUFFER, 512, data);
        }

        public void GetSupportedExtensionsRegressionTest()
        {
            var extensions = gl.GetSupportedExtensions();

            Assert.NotEmpty(extensions);
        }

        public void GetAttachedShadersTest()
        {
            var program = SetUpProgram(gl);

            var shaders = gl.GetAttachedShaders(program);

            Assert.NotEmpty(shaders);
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/7
        public void CheckFramebufferStatusRegressionTest()
        {
            gl.CreateFramebuffer();
            
            var status = gl.CheckFramebufferStatus(WebGLRenderingContextBase.FRAMEBUFFER);

            Assert.IsType<uint>(status);
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/12
        public void BufferDataArrayTest()
        {
            var buffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, buffer);
            var data = new float[] { 1, 2, 3, 4 };

            gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, data, WebGLRenderingContextBase.STATIC_DRAW);

            this.AssertNoWebGLError();
        }

        private void AssertNoWebGLError()
        {
            var error = gl.GetError();

            Assert.Equal(WebGLRenderingContextBase.NO_ERROR, error);
        }
    }
}
