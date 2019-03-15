using System.Drawing;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public class Triangle : ISample
    {
        public string Description => string.Empty;

        public void Run(JSObject canvas, int canvasWidth, int canvasHeight, Color clearColor)
        {
            var gl = WebGL.GetContext(canvas);

            var vertices = new float[]
            {
               -0.5f, 0.5f, 0.0f,
               -0.5f, -0.5f, 0.0f,
               0.5f, -0.5f, 0.0f,
            };
            var vertexBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ArrayBuffer, vertexBuffer);
            gl.BufferData(gl.ArrayBuffer, vertices, gl.StaticDraw);
            gl.BindBuffer(gl.ArrayBuffer, null);

            var indices = new ushort[] { 0, 1, 2 };
            var indexBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ElementArrayBuffer, indexBuffer);
            gl.BufferData(gl.ElementArrayBuffer, indices, gl.StaticDraw);
            gl.BindBuffer(gl.ElementArrayBuffer, null);

            var vertexShaderCode =
@"attribute vec3 coordinates;

void main(void) {
    gl_Position = vec4(coordinates, 1.0);
}";
            var vertexShader = gl.CreateShader(gl.VertexShader);
            gl.ShaderSource(vertexShader, vertexShaderCode);
            gl.CompileShader(vertexShader);

            var fragmentShaderCode =
@"void main(void) {
    gl_FragColor = vec4(0.0, 0.0, 0.0, 0.1);
}";
            var fragmentShader = gl.CreateShader(gl.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentShaderCode);
            gl.CompileShader(fragmentShader);

            var shaderProgram = gl.CreateProgram();
            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.LinkProgram(shaderProgram);
            gl.UseProgram(shaderProgram);

            gl.BindBuffer(gl.ArrayBuffer, vertexBuffer);
            gl.BindBuffer(gl.ElementArrayBuffer, indexBuffer);

            var coordinates = gl.GetAttribLocation(shaderProgram, "coordinates");
            gl.VertexAttribPointer(coordinates, 3, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(coordinates);

            gl.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
            gl.Enable(gl.DepthTest);
            gl.Clear(gl.ColorBufferBit);
            gl.Viewport(0, 0, canvasWidth, canvasHeight);
            gl.DrawElements(gl.Triangles, indices.Length, gl.UnsignedShort, 0);
        }
    }
}
