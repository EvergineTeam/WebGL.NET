using WebAssembly;
using WebGLDotNET;

namespace Tests
{
    public class WebGL2Tests
    {
        private readonly WebGL2RenderingContext gl;

        public WebGL2Tests(JSObject canvas)
        {
            gl = new WebGL2RenderingContext(canvas);
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/5
        public void GetUniformBlockIndexRegressionTest()
        {
            CheckWebGL2Support();

            var vertexShader = gl.CreateShader(WebGLRenderingContextBase.VERTEX_SHADER);
            gl.ShaderSource(vertexShader, "void main() {}");
            gl.CompileShader(vertexShader);
            var fragmentShader = gl.CreateShader(WebGLRenderingContextBase.FRAGMENT_SHADER);
            gl.ShaderSource(fragmentShader, "void main() {}");
            gl.CompileShader(fragmentShader);
            var program = gl.CreateProgram();
            gl.AttachShader(program, vertexShader);
            gl.AttachShader(program, fragmentShader);
            gl.LinkProgram(program);

            gl.GetUniformBlockIndex(program, "foo");
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/6
        public void BindBufferRangeRegressionTest()
        {
            CheckWebGL2Support();

            var buffer = gl.CreateBuffer();

            gl.BindBufferRange(WebGL2RenderingContextBase.UNIFORM_BUFFER, 0, buffer, 0, 4);
        }

        private void CheckWebGL2Support()
        {
            if (!WebGL2RenderingContextBase.IsSupported)
            {
                throw new InconclusiveException("WebGL 2 is not supported");
            }
        }
    }
}
