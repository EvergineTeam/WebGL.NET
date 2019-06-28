using WebAssembly;
using WebGLDotNET;

namespace Tests
{
    public class TheTests
    {
        private readonly WebGL2RenderingContext gl;

        public TheTests(JSObject canvas)
        {
            gl = new WebGL2RenderingContext(canvas);
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/5
        public void GetErrorRegressionTest()
        {
            var error = gl.GetError();
            var test = error != WebGLRenderingContextBase.NO_ERROR;
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/5
        public void GetUniformBlockIndexRegressionTest()
        {
            var program = gl.CreateProgram();
            gl.GetUniformBlockIndex(program, "foo");
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/6
        public void BindBufferRangeRegressionTest()
        {
            var buffer = gl.CreateBuffer();
            gl.BindBufferRange(WebGL2RenderingContextBase.UNIFORM_BUFFER, 0, buffer, 0, 4);
        }

        public void BufferSubDataRegressionTest()
        {
            var data = new float[] { 0 };
            gl.BufferSubData(0, 0, data);
        }
    }
}
