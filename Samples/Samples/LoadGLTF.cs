using System;
using System.Drawing;
using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;
using Samples.GLTF;
using System.Runtime.InteropServices;
using System.IO;

namespace Samples
{
    public class LoadGLTF : ISample
    {
        float canvasWidth;
        float canvasHeight;
        Vector4 clearColor;
        Matrix viewProj;
        Matrix worldViewProj;

        WebGLBuffer[] vertexBuffers;
        WebGLBuffer indexBuffer;
        int indexCount;

        WebGLShader vertexShader;
        WebGLShader fragmentShader;
        WebGLProgram shaderProgram;
        WebGLUniformLocation wvp;

        protected WebGLRenderingContextBase gl;

        public string Description => string.Empty;

        public double OldMilliseconds { get; set; }

        public void Run(JSObject canvas, float canvasWidth, float canvasHeight, Vector4 clearColor)
        {
            gl = new WebGL2RenderingContext(canvas);
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;
            this.clearColor = clearColor;

            this.InitializeAsync();
        }

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            var view = Matrix.CreateLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);
            var proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, canvasWidth / canvasHeight, 0.1f, 100f);
            this.viewProj = Matrix.Multiply(view, proj);

            // Load VertexShader
            var vertexShaderStream = await WasmResourceLoader.LoadAsync("Assets/VertexShader.essl", WasmResourceLoader.GetLocalAddress());
            using (StreamReader reader = new StreamReader(vertexShaderStream))
            {
                var vertexShaderCode = reader.ReadToEnd();
                this.vertexShader = gl.CreateShader(WebGLRenderingContextBase.VERTEX_SHADER);
                gl.ShaderSource(this.vertexShader, vertexShaderCode);
                gl.CompileShader(this.vertexShader);
            }

            // Load FragmentShader
            var fragmentShaderStream = await WasmResourceLoader.LoadAsync("Assets/FragmentShader.essl", WasmResourceLoader.GetLocalAddress());
            using (StreamReader reader = new StreamReader(vertexShaderStream))
            {
                var fragmentShaderCode = reader.ReadToEnd();
                this.fragmentShader = gl.CreateShader(WebGLRenderingContextBase.FRAGMENT_SHADER);
                gl.ShaderSource(this.vertexShader, fragmentShaderCode);
                gl.CompileShader(this.fragmentShader);
            }

            // Create Program
            this.shaderProgram = gl.CreateProgram();
            gl.AttachShader(this.shaderProgram, this.vertexShader);
            gl.AttachShader(this.shaderProgram, this.fragmentShader);
            gl.LinkProgram(this.shaderProgram);

            this.wvp = gl.GetUniformLocation(this.shaderProgram, "worldViewProj");

            //Load gltf mesh
            using (var gltf = new GLTFModelLoader("Assets/DamagedHelmet.gltf"))
            {
                var mesh = gltf.Meshes[0];

                // Index Buffer
                var indexBufferView = mesh.IndicesBufferView;
                this.indexCount = indexBufferView.ByteLength / sizeof(ushort);
                this.indexBuffer = gl.CreateBuffer();
                gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, this.indexBuffer);
                var indexPointer = gltf.Buffers[indexBufferView.Buffer].bufferPointer + indexBufferView.ByteOffset;
                var indices = new byte[indexBufferView.ByteLength];
                Marshal.Copy(indexPointer, indices, 0, indexBufferView.ByteLength);
                gl.BufferData(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indices, WebGLRenderingContextBase.STATIC_DRAW);

                // Vertex Buffer
                int vertexBufferCount = mesh.AttributeBufferView.Length;
                this.vertexBuffers = new WebGLBuffer[vertexBufferCount];
                for (int i = 0; i < vertexBufferCount; i++)
                {
                    var vertexBufferView = mesh.AttributeBufferView[i];

                    var buffer = gl.CreateBuffer();
                    gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, buffer);
                    var vertexPointer = gltf.Buffers[vertexBufferView.Buffer].bufferPointer + vertexBufferView.ByteOffset;
                    var vertices = new byte[vertexBufferView.ByteLength];
                    Marshal.Copy(vertexPointer, vertices, 0, vertexBufferView.ByteLength);
                    gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, vertices, WebGLRenderingContextBase.STATIC_DRAW);
                    this.vertexBuffers[i] = buffer;
                }
            }
        }

        public void Update(double elapsedTime)
        {
            var elapsedMillisecondsFloat = (float)elapsedTime;
            var rotation = Quaternion.CreateFromYawPitchRoll(
                0,
                elapsedMillisecondsFloat * 0.001f,
                0);
            var world = Matrix.CreateFromQuaternion(rotation);

            this.worldViewProj = world * this.viewProj;
        }

        public void Draw()
        {
            gl.Enable(WebGLRenderingContextBase.DEPTH_TEST);
            gl.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
            gl.Clear(WebGLRenderingContextBase.COLOR_BUFFER_BIT);

            // Bind indices
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indexBuffer);

            // Bind Normal
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, this.vertexBuffers[0]);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, WebGLRenderingContextBase.FLOAT, false, 12, 0);

            // Bind Position
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, this.vertexBuffers[1]);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(0, 3, WebGLRenderingContextBase.FLOAT, false, 12, 0);

            // Bind Texture
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, this.vertexBuffers[2]);
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(0, 2, WebGLRenderingContextBase.FLOAT, false, 8, 0);

            // Set Program
            gl.UseProgram(this.shaderProgram);

            // Set Shader resources
            gl.UniformMatrix4fv(this.wvp, false, this.worldViewProj.ToArray());

            // Draw
            gl.DrawElements(
                WebGLRenderingContextBase.TRIANGLES,
                this.indexCount,
                WebGLRenderingContextBase.UNSIGNED_SHORT,
                0);
        }
    }
}
