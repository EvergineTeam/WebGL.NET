using System.Drawing;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public class Triangle : BaseSample
    {
        WebGLBuffer vertexBuffer;
        ushort[] indices;
        WebGLBuffer indexBuffer;
        int positionAttribute;

        public override void Run(JSObject canvas, float canvasWidth, float canvasHeight, Color clearColor)
        {
            base.Run(canvas, canvasWidth, canvasHeight, clearColor);

            var vertices = new float[]
            {
               -0.5f,  0.5f, 0.0f,
               -0.5f, -0.5f, 0.0f,
                0.5f, -0.5f, 0.0f
            };
            vertexBuffer = CreateArrayBuffer(vertices);

            indices = new ushort[] { 0, 1, 2 };
            indexBuffer = CreateElementArrayBuffer(indices);

            InitializeShaders(
                vertexShaderCode: 
@"attribute vec3 position;

void main(void) {
    gl_Position = vec4(position, 1.0);
}",
                fragmentShaderCode: 
@"void main(void) {
    gl_FragColor = vec4(0.0, 0.0, 1.0, 1.0);
}");

            positionAttribute = gl.GetAttribLocation(shaderProgram, "position");

            gl.BindBuffer(gl.ArrayBuffer, vertexBuffer);
            gl.BindBuffer(gl.ElementArrayBuffer, indexBuffer);

            gl.VertexAttribPointer(positionAttribute, 3, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(positionAttribute);
        }

        public override void Draw()
        {
            base.Draw();

            gl.DrawElements(gl.Triangles, indices.Length, gl.UnsignedShort, 0);
        }
    }
}
