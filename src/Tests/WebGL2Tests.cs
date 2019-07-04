using WebAssembly;
using WebGLDotNET;
using Xunit;

namespace Tests
{
    public class WebGL2Tests
    {
        private readonly WebGL2RenderingContext gl;

        public WebGL2Tests(JSObject canvas)
        {
            if (!WebGL2RenderingContextBase.IsSupported)
            {
                throw new InconclusiveException("WebGL 2 is not supported");
            }

            gl = new WebGL2RenderingContext(canvas);
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/5
        public void GetUniformBlockIndexRegressionTest()
        {
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
            var buffer = gl.CreateBuffer();

            gl.BindBufferRange(WebGL2RenderingContextBase.UNIFORM_BUFFER, 0, buffer, 0, 4);
        }

        public void GetActiveUniformRegressionTest()
        {
            var vertexShader = gl.CreateShader(WebGLRenderingContextBase.VERTEX_SHADER);
            gl.ShaderSource(vertexShader, @"attribute vec3 position;
attribute vec3 color;

uniform mat4 pMatrix;
uniform mat4 vMatrix;
uniform mat4 wMatrix;

varying vec3 vColor;

void main(void) {
    gl_Position = pMatrix * vMatrix * wMatrix * vec4(position, 1.0);
    vColor = color;
}");
            gl.CompileShader(vertexShader);
            var fragmentShader = gl.CreateShader(WebGLRenderingContextBase.FRAGMENT_SHADER);
            gl.ShaderSource(fragmentShader, "void main() {}");
            gl.CompileShader(fragmentShader);
            var program = gl.CreateProgram();
            gl.AttachShader(program, vertexShader);
            gl.AttachShader(program, fragmentShader);
            gl.LinkProgram(program);

            var uniform = gl.GetActiveUniform(program, 0);

            Assert.IsType(typeof(uint), uniform.Type);
        }

        public void GetSupportedExtensionsRegressionTest()
        {
            var extensions = gl.GetSupportedExtensions();
            Assert.NotEmpty(extensions);
        }
    }
}
