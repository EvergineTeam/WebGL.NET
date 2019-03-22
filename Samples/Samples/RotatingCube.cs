using System;
using System.Drawing;
using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public class RotatingCube : ISample
    {
        static WebGL gl;
        static int width;
        static int height;
        static Color clearColor;

        static ushort[] indices;
        static object indexBuffer;

        static object pMatrix;
        static object vMatrix;
        static object wMatrix;

        static Matrix projectionMatrix;
        static Matrix viewMatrix;
        static Matrix worldMatrix;

        static double oldTime;

        public string Description =>
            "Every matrix calc relies in Wave Engine's Math library, consumed through NuGet. This will make @jcant0n " +
            "happy :-)";

        public void Run(JSObject canvas, int canvasWidth, int canvasHeight, Color clearColor)
        {
            gl = WebGL.GetContext(canvas);
            width = canvasWidth;
            height = canvasHeight;
            RotatingCube.clearColor = clearColor;

            var vertices = new float[]
            {
                -1, -1, -1, 1, -1, -1, 1, 1, -1, -1, 1, -1,
                -1, -1, 1, 1, -1, 1, 1, 1, 1, -1, 1, 1,
                -1, -1, -1, -1, 1, -1, -1, 1, 1, -1, -1, 1,
                1, -1, -1, 1, 1, -1, 1, 1, 1, 1, -1, 1,
                -1, -1, -1, -1, -1, 1, 1, -1, 1, 1, -1, -1,
                -1, 1, -1, -1, 1, 1, 1, 1, 1, 1, 1, -1,
            };
            var vertexBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ArrayBuffer, vertexBuffer);
            gl.BufferData(gl.ArrayBuffer, vertices, gl.StaticDraw);

            indices = new ushort[]
            {
                0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7,
                8, 9, 10, 8, 10, 11, 12, 13, 14, 12, 14, 15,
                16, 17, 18, 16, 18, 19, 20, 21, 22, 20, 22, 23
            };
            indexBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ElementArrayBuffer, indexBuffer);
            gl.BufferData(gl.ElementArrayBuffer, indices, gl.StaticDraw);

            var colors = new float[]
            {
                5, 3, 7, 5, 3, 7, 5, 3, 7, 5, 3, 7,
                1, 1, 3, 1, 1, 3, 1, 1, 3, 1, 1, 3,
                0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1,
                1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0,
                1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0,
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0
            };
            var colorBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ArrayBuffer, colorBuffer);
            gl.BufferData(gl.ArrayBuffer, colors, gl.StaticDraw);

            var vertexShaderCode =
@"attribute vec3 position;
uniform mat4 pMatrix;
uniform mat4 vMatrix;
uniform mat4 wMatrix;
attribute vec3 color;
varying vec3 vColor;

void main(void) {
    gl_Position = pMatrix * vMatrix * wMatrix * vec4(position, 1.);
    vColor = color;
}";
            var vertexShader = gl.CreateShader(gl.VertexShader);
            gl.ShaderSource(vertexShader, vertexShaderCode);
            gl.CompileShader(vertexShader);

            var fragmentShaderCode =
@"precision mediump float;
varying vec3 vColor;

void main(void) {
    gl_FragColor = vec4(vColor, 1.);
}";
            var fragmentShader = gl.CreateShader(gl.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentShaderCode);
            gl.CompileShader(fragmentShader);

            var shaderProgram = gl.CreateProgram();
            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.LinkProgram(shaderProgram);
            gl.UseProgram(shaderProgram);

            pMatrix = gl.GetUniformLocation(shaderProgram, "pMatrix");
            vMatrix = gl.GetUniformLocation(shaderProgram, "vMatrix");
            wMatrix = gl.GetUniformLocation(shaderProgram, "wMatrix");

            gl.BindBuffer(gl.ArrayBuffer, vertexBuffer);
            var position = gl.GetAttribLocation(shaderProgram, "position");
            gl.VertexAttribPointer(position, 3, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(position);

            gl.BindBuffer(gl.ArrayBuffer, colorBuffer);
            var color = gl.GetAttribLocation(shaderProgram, "color");
            gl.VertexAttribPointer(color, 3, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(color);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI / 4, width / (float)height, 0.1f, 1000f);
            viewMatrix = Matrix.CreateLookAt(Vector3.UnitZ * 10, Vector3.Zero, Vector3.Up);
            worldMatrix = Matrix.Identity;

            // Needed for linker preserve
            Loop(0);

            gl.RequestAnimationFrame(nameof(Loop), GetType());
        }

        static void Loop(double time)
        {
            var elapsedTime = time - oldTime;

            Update(elapsedTime);

            oldTime = time;

            Draw();
        }

        static void Update(double elapsedTime)
        {
            var rotation = Quaternion.CreateFromYawPitchRoll(
                (float)elapsedTime * 0.003f, (float)elapsedTime * 0.002f, (float)elapsedTime * 0.005f);
            worldMatrix *= Matrix.CreateFromQuaternion(rotation);
        }

        static void Draw()
        {
            gl.Enable(gl.DepthTest);
            gl.DepthFunc(gl.LEqual);
            gl.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
            gl.ClearDepth(1.0);
            gl.Viewport(0, 0, width, height);
            gl.Clear((int)gl.ColorBufferBit | (int)gl.DepthBufferBit);

            gl.UniformMatrix4fv(pMatrix, false, projectionMatrix.ToArray());
            gl.UniformMatrix4fv(vMatrix, false, viewMatrix.ToArray());
            gl.UniformMatrix4fv(wMatrix, false, worldMatrix.ToArray());

            gl.BindBuffer(gl.ElementArrayBuffer, indexBuffer);
            gl.DrawElements(gl.Triangles, indices.Length, gl.UnsignedShort, 0);
        }
    }
}
