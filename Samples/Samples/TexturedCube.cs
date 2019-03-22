using System.Drawing;
using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;
using System;

namespace Samples
{
    // Based on:
    // https://developer.mozilla.org/es/docs/Web/API/WebGL_API/Tutorial/Wtilizando_texturas_en_WebGL
    // view-source:https://mdn.github.io/webgl-examples/tutorial/sample6/webgl-demo.js
    public class TexturedCube : ISample
    {
        static WebGL gl;
        static int width;
        static int height;
        static Color clearColor;
        static object shaderProgram;
        static object positionBuffer;
        static object textureCoordBuffer;
        static ushort[] indices;
        static object indexBuffer;
        static object texture;
        static object vertexPositionShader;
        static object textureCoordShader;
        static object projectionMatrixShader;
        static Matrix projectionMatrix;
        static object modelViewMatrixShader;
        static Matrix modelViewMatrix;
        static object samplerShader;
        static double oldTimeMilliseconds;
        static double totalElapsedTimeSeconds;

        public string Description => string.Empty;

        public void Run(JSObject canvas, int canvasWidth, int canvasHeight, Color clearColor)
        {
            gl = WebGL.GetContext(canvas);
            width = canvasWidth;
            height = canvasHeight;
            TexturedCube.clearColor = clearColor;

            var vertexShaderCode =
@"attribute vec4 aVertexPosition;
attribute vec2 aTextureCoord;

uniform mat4 uModelViewMatrix;
uniform mat4 uProjectionMatrix;

varying highp vec2 vTextureCoord;

void main(void) {
    gl_Position = uProjectionMatrix * uModelViewMatrix * aVertexPosition;
    vTextureCoord = aTextureCoord;
}";
            var vertexShader = gl.CreateShader(gl.VertexShader);
            gl.ShaderSource(vertexShader, vertexShaderCode);
            gl.CompileShader(vertexShader);

            var fragmentShaderCode =
@"varying highp vec2 vTextureCoord;

uniform sampler2D uSampler;

void main(void) {
    gl_FragColor = texture2D(uSampler, vTextureCoord);
}";
            var fragmentShader = gl.CreateShader(gl.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentShaderCode);
            gl.CompileShader(fragmentShader);

            shaderProgram = gl.CreateProgram();
            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.LinkProgram(shaderProgram);

            vertexPositionShader = gl.GetAttribLocation(shaderProgram, "aVertexPosition");
            textureCoordShader = gl.GetAttribLocation(shaderProgram, "aTextureCoord");
            projectionMatrixShader = gl.GetUniformLocation(shaderProgram, "uProjectionMatrix");
            modelViewMatrixShader = gl.GetUniformLocation(shaderProgram, "uModelViewMatrix");
            samplerShader = gl.GetUniformLocation(shaderProgram, "uSampler");

            var positions = new float[]
            {
                // Front face
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                // Back face
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                // Top face
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f, -1.0f,
                // Bottom face
                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,
                // Right face
                 1.0f, -1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                // Left face
                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f
            };
            positionBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ArrayBuffer, positionBuffer);
            gl.BufferData(gl.ArrayBuffer, positions, gl.StaticDraw);

            var textureCoordinates = new float[]
            {
                // Front
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Back
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Top
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Bottom
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Right
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Left
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f
            };
            textureCoordBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ArrayBuffer, textureCoordBuffer);
            gl.BufferData(gl.ArrayBuffer, textureCoordinates, gl.StaticDraw);

            indices = new ushort[]
            {
                0,  1,  2,    0,  2,  3,    // front
                4,  5,  6,    4,  6,  7,    // back
                8,  9,  10,   8,  10, 11,   // top
                12, 13, 14,   12, 14, 15,   // bottom
                16, 17, 18,   16, 18, 19,   // right
                20, 21, 22,   20, 22, 23    // left
            };
            indexBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ElementArrayBuffer, indexBuffer);
            gl.BufferData(gl.ElementArrayBuffer, indices, gl.StaticDraw);

            texture = gl.CreateTexture();
            gl.BindTexture(gl.Texture2D, texture);
            //gl.GenerateMipmap(gl.Texture2D);
            gl.TexParameteri(gl.Texture2D, gl.TextureWrapS, gl.ClampToEdge);
            gl.TexParameteri(gl.Texture2D, gl.TextureWrapT, gl.ClampToEdge);
            gl.TexParameteri(gl.Texture2D, gl.TextureMinFilter, gl.Nearest);
            var imageData = new ImageData(Image.ARGBColors, Image.Width, Image.Height);
            gl.TexImage2D(gl.Texture2D, 0, gl.RGB, gl.RGB, gl.UnsignedByte, imageData);

            gl.VertexAttribPointer(vertexPositionShader, 3, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(vertexPositionShader);

            gl.EnableVertexAttribArray(textureCoordShader);
            gl.VertexAttribPointer(textureCoordShader, 2, gl.Float, false, 0, 0);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), 
                width / (float)height, 
                0.1f, 
                100f);

            // Needed for linker preserve
            Loop(0);

            gl.RequestAnimationFrame(nameof(Loop), GetType());
        }

        static void Loop(double timeMilliseconds)
        {
            var elapsedTimeMilliseconds = timeMilliseconds - oldTimeMilliseconds;

            Update(elapsedTimeMilliseconds);

            oldTimeMilliseconds = timeMilliseconds;

            Draw();
        }

        static void Update(double elapsedTimeMilliseconds)
        {
            var elapsedTimeSeconds = elapsedTimeMilliseconds * 0.001;
            totalElapsedTimeSeconds += elapsedTimeSeconds;

            modelViewMatrix = Matrix.Identity;
            modelViewMatrix *= Matrix.CreateRotationZ((float)totalElapsedTimeSeconds);
            modelViewMatrix *= Matrix.CreateRotationY((float)totalElapsedTimeSeconds * 0.7f);
            modelViewMatrix *= Matrix.CreateTranslation(0f, 0f, -6f);
        }

        static void Draw()
        {
            gl.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
            gl.ClearDepth(1.0);
            gl.Enable(gl.DepthTest);
            gl.DepthFunc(gl.LEqual);
            gl.Viewport(0, 0, width, height);

            gl.Clear((int)gl.ColorBufferBit | (int)gl.DepthBufferBit);

            gl.BindBuffer(gl.ArrayBuffer, positionBuffer);
            gl.VertexAttribPointer(vertexPositionShader, 3, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(vertexPositionShader);

            gl.BindBuffer(gl.ArrayBuffer, textureCoordBuffer);
            gl.VertexAttribPointer(textureCoordShader, 2, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(textureCoordShader);

            gl.BindBuffer(gl.ElementArrayBuffer, indexBuffer);

            gl.UseProgram(shaderProgram);

            gl.UniformMatrix4fv(projectionMatrixShader, false, projectionMatrix.ToArray());
            gl.UniformMatrix4fv(modelViewMatrixShader, false, modelViewMatrix.ToArray());

            gl.ActiveTexture(gl.Texture0);
            gl.BindTexture(gl.Texture2D, texture);
            gl.Uniform1i(samplerShader, 0);

            gl.DrawElements(gl.Triangles, 36, gl.UnsignedShort, 0);
        }
    }
}
