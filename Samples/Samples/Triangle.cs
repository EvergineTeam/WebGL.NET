using System.Drawing;
using WebAssembly;

namespace Samples
{
    public class Triangle : BaseSample
    {
        object vertexBuffer;
        ushort[] indices;
        object indexBuffer;
        object coordinatesAttribute;

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
@"attribute vec3 coordinates;

void main(void) {
    gl_Position = vec4(coordinates, 1.0);
}",
                fragmentShaderCode: 
@"void main(void) {
    gl_FragColor = vec4(0.0, 0.0, 0.0, 0.1);
}");

            coordinatesAttribute = gl.GetAttribLocation(shaderProgram, "coordinates");
        }

        public override void Draw()
        {
            base.Draw();

            gl.BindBuffer(gl.ArrayBuffer, vertexBuffer);
            gl.BindBuffer(gl.ElementArrayBuffer, indexBuffer);

            gl.VertexAttribPointer(coordinatesAttribute, 3, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(coordinatesAttribute);

            gl.DrawElements(gl.Triangles, indices.Length, gl.UnsignedShort, 0);
        }
    }
}
