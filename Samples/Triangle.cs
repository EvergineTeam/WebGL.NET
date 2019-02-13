using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public class Triangle : ISample
    {
        public void Run(JSObject canvas, int canvasWidth, int canvasHeight)
        {
            WebGL.Init(canvas);

            /*======== Defining and storing the geometry ===========*/

            var vertices = new float[]
            {
               -0.5f, 0.5f, 0.0f,
               -0.5f, -0.5f, 0.0f,
               0.5f, -0.5f, 0.0f,
            };

            var indices = new ushort[] { 0, 1, 2 };

            // Create an empty buffer object to store vertex buffer
            var vertex_buffer = WebGL.CreateBuffer();

            // Bind appropriate array buffer to it
            WebGL.BindBuffer(WebGL.ArrayBuffer, vertex_buffer);

            // Pass the vertex data to the buffer
            WebGL.BufferData(WebGL.ArrayBuffer, vertices, WebGL.StaticDraw);

            // Unbind the buffer
            WebGL.BindBuffer(WebGL.ArrayBuffer, null);

            // Create an empty buffer object to store Index buffer
            var Index_Buffer = WebGL.CreateBuffer();

            // Bind appropriate array buffer to it
            WebGL.BindBuffer(WebGL.ElementArrayBuffer, Index_Buffer);

            // Pass the vertex data to the buffer
            WebGL.BufferData(WebGL.ElementArrayBuffer, indices, WebGL.StaticDraw);

            // Unbind the buffer
            WebGL.BindBuffer(WebGL.ElementArrayBuffer, null);

            /*================ Shaders ====================*/

            // Vertex shader source code
            var vertCode =
                "attribute vec3 coordinates;" +

                "void main(void) {" +
                    "gl_Position = vec4(coordinates, 1.0);" +
                "}";

            // Create a vertex shader object
            var vertShader = WebGL.CreateShader(WebGL.VertexShader);

            // Attach vertex shader source code
            WebGL.ShaderSource(vertShader, vertCode);

            // Compile the vertex shader
            WebGL.CompileShader(vertShader);

            //fragment shader source code
            var fragCode =
                "void main(void) {" +
                " gl_FragColor = vec4(0.0, 0.0, 0.0, 0.1);" +
                "}";

            // Create fragment shader object
            var fragShader = WebGL.CreateShader(WebGL.FragmentShader);

            // Attach fragment shader source code
            WebGL.ShaderSource(fragShader, fragCode);

            // Compile the fragmentt shader
            WebGL.CompileShader(fragShader);

            // Create a shader program object to store
            // the combined shader program
            var shaderProgram = WebGL.CreateProgram();

            // Attach a vertex shader
            WebGL.AttachShader(shaderProgram, vertShader);

            // Attach a fragment shader
            WebGL.AttachShader(shaderProgram, fragShader);

            // Link both the programs
            WebGL.LinkProgram(shaderProgram);

            // Use the combined shader program object
            WebGL.UseProgram(shaderProgram);

            /*======= Associating shaders to buffer objects =======*/

            // Bind vertex buffer object
            WebGL.BindBuffer(WebGL.ArrayBuffer, vertex_buffer);

            // Bind index buffer object
            WebGL.BindBuffer(WebGL.ElementArrayBuffer, Index_Buffer);

            // Get the attribute location
            var coord = WebGL.GetAttribLocation(shaderProgram, "coordinates");

            // Point an attribute to the currently bound VBO
            WebGL.VertexAttribPointer(coord, 3, WebGL.Float, false, 0, 0);

            // Enable the attribute
            WebGL.EnableVertexAttribArray(coord);

            /*=========Drawing the triangle===========*/

            // Clear the canvas
            WebGL.ClearColor(0.5, 0.5, 0.5, 0.9);

            // Enable the depth test
            WebGL.Enable(WebGL.DepthTest);

            // Clear the color buffer bit
            WebGL.Clear(WebGL.ColorBufferBit);

            // Set the view port
            WebGL.Viewport(0, 0, canvasWidth, canvasHeight);

            // Draw the triangle
            WebGL.DrawElements(WebGL.Triangles, indices.Length, WebGL.UnsignedShort, 0);
        }
    }
}
