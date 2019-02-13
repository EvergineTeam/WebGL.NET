using System;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public class RotatingCube : ISample
    {
        static int width;
        static int height;

        static double time_old;

        static object Pmatrix;
        static object Vmatrix;
        static object Mmatrix;

        static float[] proj_matrix;
        static float[] mov_matrix;
        static float[] view_matrix;

        static ushort[] indices;
        static object index_buffer;

        public void Run(JSObject canvas, int canvasWidth, int canvasHeight)
        {
            WebGL.Init(canvas);

            width = (int)canvasWidth;
            height = (int)canvasHeight;

            PrepareIndexBuffer();

            var vertex_buffer = PrepareVertexBuffer();
            var color_buffer = PrepareColorBuffer();

            InitializeProgram(vertex_buffer, color_buffer);

            PrepareMatrices();

            // Needed for linker preserve
            Loop(0);

            WebGL.RequestAnimationFrame(nameof(Loop), GetType());
        }

        static void Loop(double time)
        {
            var dt = time - time_old;

            Update(dt);

            time_old = time;

            Draw();
        }

        static void InitializeProgram(object vertex_buffer, object color_buffer)
        {
            var shaderProgram = PrepareShaderProgram();

            Pmatrix = WebGL.GetUniformLocation(shaderProgram, "Pmatrix");
            Vmatrix = WebGL.GetUniformLocation(shaderProgram, "Vmatrix");
            Mmatrix = WebGL.GetUniformLocation(shaderProgram, "Mmatrix");

            WebGL.BindBuffer(WebGL.ArrayBuffer, vertex_buffer);

            var position = WebGL.GetAttribLocation(shaderProgram, "position");
            WebGL.VertexAttribPointer(position, 3, WebGL.Float, false, 0, 0);
            WebGL.EnableVertexAttribArray(position);
            WebGL.BindBuffer(WebGL.ArrayBuffer, color_buffer);

            var color = WebGL.GetAttribLocation(shaderProgram, "color");
            WebGL.VertexAttribPointer(color, 3, WebGL.Float, false, 0, 0);
            WebGL.EnableVertexAttribArray(color);

            WebGL.UseProgram(shaderProgram);
        }

        static void Update(double dt)
        {
            RotateZ(mov_matrix, (float)dt * 0.005f);
            RotateY(mov_matrix, (float)dt * 0.002f);
            RotateX(mov_matrix, (float)dt * 0.003f);
        }

        static void Draw()
        {
            WebGL.Enable(WebGL.DepthTest);
            WebGL.DepthFunc(WebGL.LEqual);
            WebGL.ClearColor(0.5, 0.5, 0.5, 0.9);
            WebGL.ClearDepth(1.0);

            WebGL.Viewport(0, 0, width, height);

            var flag1 = (int)WebGL.ColorBufferBit;
            var flag2 = (int)WebGL.DepthBufferBit;

            WebGL.Clear(flag1 | flag2);

            WebGL.UniformMatrix4fv(Pmatrix, false, proj_matrix);
            WebGL.UniformMatrix4fv(Vmatrix, false, view_matrix);
            WebGL.UniformMatrix4fv(Mmatrix, false, mov_matrix);

            WebGL.BindBuffer(WebGL.ElementArrayBuffer, index_buffer);

            WebGL.DrawElements(WebGL.Triangles, indices.Length, WebGL.UnsignedShort, 0);
        }

        static object PrepareColorBuffer()
        {
            var colors = new float[]
            {
                5,3,7, 5,3,7, 5,3,7, 5,3,7,
                1,1,3, 1,1,3, 1,1,3, 1,1,3,
                0,0,1, 0,0,1, 0,0,1, 0,0,1,
                1,0,0, 1,0,0, 1,0,0, 1,0,0,
                1,1,0, 1,1,0, 1,1,0, 1,1,0,
                0,1,0, 0,1,0, 0,1,0, 0,1,0
            };

            var color_buffer = WebGL.CreateBuffer();
            WebGL.BindBuffer(WebGL.ArrayBuffer, color_buffer);
            WebGL.BufferData(WebGL.ArrayBuffer, colors, WebGL.StaticDraw);

            return color_buffer;
        }

        static object PrepareFragmentShader()
        {
            var fragCode =
                "precision mediump float;" +
                "varying vec3 vColor;" +
                "void main(void) {" +
                    "gl_FragColor = vec4(vColor, 1.);" +
                "}";

            var fragShader = WebGL.CreateShader(WebGL.FragmentShader);
            WebGL.ShaderSource(fragShader, fragCode);
            WebGL.CompileShader(fragShader);

            return fragShader;
        }

        static void PrepareIndexBuffer()
        {
            indices = new ushort[]
            {
                0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7,
                8, 9, 10, 8, 10, 11, 12, 13, 14, 12, 14, 15,
                16, 17, 18, 16, 18, 19, 20, 21, 22, 20, 22, 23
            };

            index_buffer = WebGL.CreateBuffer();
            WebGL.BindBuffer(WebGL.ElementArrayBuffer, index_buffer);
            WebGL.BufferData(WebGL.ElementArrayBuffer, indices, WebGL.StaticDraw);
        }

        static void PrepareMatrices()
        {
            var angle = 40.0f;
            var a = (float)(width / height);
            var zMin = 1.0f;
            var zMax = 100.0f;

            var ang = (float)(Math.Tan((angle * .5) * Math.PI / 180));

            proj_matrix = new float[]
            {
                0.5f / ang, 0, 0, 0,
                0, 0.5f * a / ang, 0, 0,
                0, 0, -(zMax + zMin) / (zMax - zMin), -1,
                0, 0, (-2 * zMax * zMin) / (zMax - zMin), 0
            };

            mov_matrix = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            view_matrix = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };

            // translating z
            view_matrix[14] = view_matrix[14] - 6.0f; // zoom
        }

        static object PrepareShaderProgram()
        {
            var vertShader = PrepareVertexShader();
            var fragShader = PrepareFragmentShader();

            var shaderProgram = WebGL.CreateProgram();
            WebGL.AttachShader(shaderProgram, vertShader);
            WebGL.AttachShader(shaderProgram, fragShader);
            WebGL.LinkProgram(shaderProgram);

            return shaderProgram;
        }

        static object PrepareVertexBuffer()
        {
            var vertices = new float[]
            {
                -1, -1, -1, 1, -1, -1, 1, 1, -1, -1, 1, -1,
                -1, -1, 1, 1, -1, 1, 1, 1, 1, -1, 1, 1,
                -1, -1, -1, -1, 1, -1, -1, 1, 1, -1, -1, 1,
                1, -1, -1, 1, 1, -1, 1, 1, 1, 1, -1, 1,
                -1, -1, -1, -1, -1, 1, 1, -1, 1, 1, -1, -1,
                -1, 1, -1, -1, 1, 1, 1, 1, 1, 1, 1, -1,
            };

            var vertex_buffer = WebGL.CreateBuffer();
            WebGL.BindBuffer(WebGL.ArrayBuffer, vertex_buffer);
            WebGL.BufferData(WebGL.ArrayBuffer, vertices, WebGL.StaticDraw);

            return vertex_buffer;
        }

        static object PrepareVertexShader()
        {
            var vertCode =
                "attribute vec3 position;" +
                "uniform mat4 Pmatrix;" +
                "uniform mat4 Vmatrix;" +
                "uniform mat4 Mmatrix;" +
                "attribute vec3 color;" +
                "varying vec3 vColor;" +

                "void main(void) { " +
                   "gl_Position = Pmatrix*Vmatrix*Mmatrix*vec4(position, 1.);" +
                   "vColor = color;" +
                "}";

            var vertShader = WebGL.CreateShader(WebGL.VertexShader);
            WebGL.ShaderSource(vertShader, vertCode);
            WebGL.CompileShader(vertShader);

            return vertShader;
        }

        static void RotateX(float[] m, float angle)
        {
            var c = (float)Math.Cos(angle);
            var s = (float)Math.Sin(angle);

            var mv1 = m[1];
            var mv5 = m[5];
            var mv9 = m[9];

            m[1] = m[1] * c - m[2] * s;
            m[5] = m[5] * c - m[6] * s;
            m[9] = m[9] * c - m[10] * s;

            m[2] = m[2] * c + mv1 * s;
            m[6] = m[6] * c + mv5 * s;
            m[10] = m[10] * c + mv9 * s;
        }

        static void RotateY(float[] m, float angle)
        {
            var c = (float)Math.Cos(angle);
            var s = (float)Math.Sin(angle);

            var mv0 = m[0];
            var mv4 = m[4];
            var mv8 = m[8];

            m[0] = c * m[0] + s * m[2];
            m[4] = c * m[4] + s * m[6];
            m[8] = c * m[8] + s * m[10];

            m[2] = c * m[2] - s * mv0;
            m[6] = c * m[6] - s * mv4;
            m[10] = c * m[10] - s * mv8;
        }

        static void RotateZ(float[] m, float angle)
        {
            var c = (float)Math.Cos(angle);
            var s = (float)Math.Sin(angle);

            var mv0 = m[0];
            var mv4 = m[4];
            var mv8 = m[8];

            m[0] = c * m[0] - s * m[1];
            m[4] = c * m[4] - s * m[5];
            m[8] = c * m[8] - s * m[9];

            m[1] = c * m[1] + s * mv0;
            m[5] = c * m[5] + s * mv4;
            m[9] = c * m[9] + s * mv8;
        }
    }
}
