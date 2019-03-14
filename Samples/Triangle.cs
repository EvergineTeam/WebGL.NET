using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public class Triangle : ISample
    {
        public void Run(JSObject canvas, int canvasWidth, int canvasHeight)
        {
            var gl = WebGL.Init(canvas);

            /*======== Defining and storing the geometry ===========*/

            var vertices = new float[]
            {
               -0.5f, 0.5f, 0.0f,
               -0.5f, -0.5f, 0.0f,
               0.5f, -0.5f, 0.0f,
            };

            var indices = new ushort[] { 0, 1, 2 };

            // Create an empty buffer object to store vertex buffer
            var vertex_buffer = gl.CreateBuffer();

            // Bind appropriate array buffer to it
            gl.BindBuffer(gl.ArrayBuffer, vertex_buffer);

            // Pass the vertex data to the buffer
            gl.BufferData(gl.ArrayBuffer, vertices, gl.StaticDraw);

            // Unbind the buffer
            gl.BindBuffer(gl.ArrayBuffer, null);

            // Create an empty buffer object to store Index buffer
            var Index_Buffer = gl.CreateBuffer();

            // Bind appropriate array buffer to it
            gl.BindBuffer(gl.ElementArrayBuffer, Index_Buffer);

            // Pass the vertex data to the buffer
            gl.BufferData(gl.ElementArrayBuffer, indices, gl.StaticDraw);

            // Unbind the buffer
            gl.BindBuffer(gl.ElementArrayBuffer, null);

            /*================ Shaders ====================*/

            // Vertex shader source code
            var vertCode =
                "attribute vec3 coordinates;" +

                "void main(void) {" +
                    "gl_Position = vec4(coordinates, 1.0);" +
                "}";

            // Create a vertex shader object
            var vertShader = gl.CreateShader(gl.VertexShader);

            // Attach vertex shader source code
            gl.ShaderSource(vertShader, vertCode);

            // Compile the vertex shader
            gl.CompileShader(vertShader);

            //fragment shader source code
            var fragCode =
                "void main(void) {" +
                " gl_FragColor = vec4(0.0, 0.0, 0.0, 0.1);" +
                "}";

            // Create fragment shader object
            var fragShader = gl.CreateShader(gl.FragmentShader);

            // Attach fragment shader source code
            gl.ShaderSource(fragShader, fragCode);

            // Compile the fragmentt shader
            gl.CompileShader(fragShader);

            // Create a shader program object to store
            // the combined shader program
            var shaderProgram = gl.CreateProgram();

            // Attach a vertex shader
            gl.AttachShader(shaderProgram, vertShader);

            // Attach a fragment shader
            gl.AttachShader(shaderProgram, fragShader);

            // Link both the programs
            gl.LinkProgram(shaderProgram);

            // Use the combined shader program object
            gl.UseProgram(shaderProgram);

            /*======= Associating shaders to buffer objects =======*/

            // Bind vertex buffer object
            gl.BindBuffer(gl.ArrayBuffer, vertex_buffer);

            // Bind index buffer object
            gl.BindBuffer(gl.ElementArrayBuffer, Index_Buffer);

            // Get the attribute location
            var coord = gl.GetAttribLocation(shaderProgram, "coordinates");

            // Point an attribute to the currently bound VBO
            gl.VertexAttribPointer(coord, 3, gl.Float, false, 0, 0);

            // Enable the attribute
            gl.EnableVertexAttribArray(coord);

            /*=========Drawing the triangle===========*/

            // Clear the canvas
            gl.ClearColor(0.5, 0.5, 0.5, 0.9);

            // Enable the depth test
            gl.Enable(gl.DepthTest);

            // Clear the color buffer bit
            gl.Clear(gl.ColorBufferBit);

            // Set the view port
            gl.Viewport(0, 0, canvasWidth, canvasHeight);

            // Draw the triangle
            gl.DrawElements(gl.Triangles, indices.Length, gl.UnsignedShort, 0);
        }
    }
}
