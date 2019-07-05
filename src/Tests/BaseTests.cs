using WebGLDotNET;

namespace Tests
{
    public abstract class BaseTests
    {
        protected WebGLProgram SetUpProgram(
            WebGLRenderingContextBase gl,
            string vertexShaderSource = "void main() {}",
            string fragmentShaderSource = "void main() {}")
        {
            var vertexShader = gl.CreateShader(WebGLRenderingContextBase.VERTEX_SHADER);
            gl.ShaderSource(vertexShader, vertexShaderSource);
            gl.CompileShader(vertexShader);
            var fragmentShader = gl.CreateShader(WebGLRenderingContextBase.FRAGMENT_SHADER);
            gl.ShaderSource(fragmentShader, fragmentShaderSource);
            gl.CompileShader(fragmentShader);
            var program = gl.CreateProgram();
            gl.AttachShader(program, vertexShader);
            gl.AttachShader(program, fragmentShader);
            gl.LinkProgram(program);

            return program;
        }
    }
}
