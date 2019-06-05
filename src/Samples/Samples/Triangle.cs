using WebGLDotNET;

namespace Samples
{
    public class Triangle : BaseSample
    {
        ushort[] indices;

        public override void Run()
        {
            base.Run();

            var vertices = new float[]
            {
               -0.5f,  0.5f, 0.0f,
               -0.5f, -0.5f, 0.0f,
                0.5f, -0.5f, 0.0f
            };
            var vertexBuffer = gl.CreateArrayBuffer(vertices);

            indices = new ushort[] { 0, 1, 2 };
            var indexBuffer = gl.CreateElementArrayBuffer(indices);

            var shaderProgram = gl.InitializeShaders(
                vertexShaderCode: 
@"attribute vec3 position;

void main(void) {
    gl_Position = vec4(position, 1.0);
}",
                fragmentShaderCode: 
@"void main(void) {
    gl_FragColor = vec4(0.0, 0.0, 1.0, 1.0);
}");

            var positionAttribute = (uint)gl.GetAttribLocation(shaderProgram, "position");

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffer);
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indexBuffer);

            gl.VertexAttribPointer(positionAttribute, 3, WebGLRenderingContextBase.FLOAT, false, 0, 0);
            gl.EnableVertexAttribArray(positionAttribute);
        }

        public override void Draw()
        {
            base.Draw();

            gl.DrawElements(
                WebGLRenderingContextBase.TRIANGLES, 
                indices.Length, 
                WebGLRenderingContextBase.UNSIGNED_SHORT, 
                0);
        }
    }
}
