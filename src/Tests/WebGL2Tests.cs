using WebAssembly;
using WebGLDotNET;
using Xunit;

namespace Tests
{
    public class WebGL2Tests : BaseTests
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
            var program = SetUpProgram(gl);

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
            var program = SetUpProgram(
                gl,
@"attribute vec3 position;
attribute vec3 color;

uniform mat4 pMatrix;
uniform mat4 vMatrix;
uniform mat4 wMatrix;

varying vec3 vColor;

void main(void) {
    gl_Position = pMatrix * vMatrix * wMatrix * vec4(position, 1.0);
    vColor = color;
}");

            var uniform = gl.GetActiveUniform(program, 0);

            Assert.IsType(typeof(uint), uniform.Type);
        }

        public void GetSupportedExtensionsRegressionTest()
        {
            var extensions = gl.GetSupportedExtensions();

            Assert.NotEmpty(extensions);
        }

        public void GetUniformIndicesTest()
        {
            var program = SetUpProgram(
                gl,
@"attribute vec3 position;
attribute vec3 color;

uniform mat4 pMatrix;
uniform mat4 vMatrix;
uniform mat4 wMatrix;

varying vec3 vColor;

void main(void) {
    gl_Position = pMatrix * vMatrix * wMatrix * vec4(position, 1.0);
    vColor = color;
}");
            var names = new string[] { "pMatrix", "vMatrix", "wMatrix" };

            var indices = gl.GetUniformIndices(program, names);

            if (indices == null)
            {
                // This happens "only" in Safari
                throw new InconclusiveException("The indices array is empty");
            }

            Assert.Equal(names.Length, indices.Length);
        }

        public void SameBufferBindMultipleTargetsTest()
        {
            var buffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, buffer);
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, buffer);
            var error = gl.GetError();

            Assert.Equal(WebGLRenderingContextBase.INVALID_OPERATION, error);
        }
    }
}
