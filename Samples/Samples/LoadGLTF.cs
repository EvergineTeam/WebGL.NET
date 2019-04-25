using System;
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
        public bool IsReady { get; set; }

        public async void Run(JSObject canvas, float canvasWidth, float canvasHeight, Vector4 clearColor)
        {
            gl = new WebGL2RenderingContext(canvas);
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;
            this.clearColor = clearColor;

            await this.InitializeAsync();
        }

        private async System.Threading.Tasks.Task InitializeAsync()
        {
            var view = Matrix.CreateLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);
            var proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, canvasWidth / canvasHeight, 0.1f, 100f);
            this.viewProj = Matrix.Multiply(view, proj);

            // Load shaders
            await InitializeShadersFromAssetsAsync("Assets/GLTFVertexShader.essl", "Assets/GLTFFragmentShader.essl");

            this.wvp = gl.GetUniformLocation(this.shaderProgram, "worldViewProj");

            //Load gltf mesh
            using (var gltf = new GLTFModelLoader())
            {
                gltf.Initialize("Assets/DamagedHelmet.glb");

                Console.WriteLine($"{gltf != null} and {gltf.Meshes != null} and {gltf.Meshes.Length}");
                Console.WriteLine("ERROR 1 $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");
                var mesh = gltf.Meshes[0];
                Console.WriteLine("ERROR 2 $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");

                // Index Buffer
                var indexBufferView = mesh.IndicesBufferView;
                Console.WriteLine("ERROR 3 $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$");
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

            this.IsReady = true;
        }

        protected void InitializeShaders(string vertexShaderCode, string fragmentShaderCode)
        {
            vertexShader = gl.CreateShader(WebGLRenderingContextBase.VERTEX_SHADER);
            gl.ShaderSource(vertexShader, vertexShaderCode);
            gl.CompileShader(vertexShader);

            var message = gl.GetShaderInfoLog(vertexShader);
            if (message.Length > 0)
            {
                throw new InvalidOperationException($"Shader Error: {message}");
            }

            fragmentShader = gl.CreateShader(WebGLRenderingContextBase.FRAGMENT_SHADER);
            gl.ShaderSource(fragmentShader, fragmentShaderCode);
            gl.CompileShader(fragmentShader);

            message = gl.GetShaderInfoLog(fragmentShader);
            if (message.Length > 0)
            {
                throw new InvalidOperationException($"Shader Error: {message}");
            }

            shaderProgram = gl.CreateProgram();
            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.LinkProgram(shaderProgram);
            gl.UseProgram(shaderProgram);
        }

        protected async System.Threading.Tasks.Task InitializeShadersFromAssetsAsync(string vertexShaderPath, string fragmentShaderPath)
        {
            var vertexShaderStream = await WasmResourceLoader.LoadAsync(vertexShaderPath, WasmResourceLoader.GetLocalAddress());
            string vertexShaderCode;
            using (StreamReader reader = new StreamReader(vertexShaderStream))
            {
                vertexShaderCode = reader.ReadToEnd();
                Console.WriteLine($"VertexShaderCode: {vertexShaderCode}");
            }

            // Load FragmentShader
            var fragmentShaderStream = await WasmResourceLoader.LoadAsync(fragmentShaderPath, WasmResourceLoader.GetLocalAddress());
            string fragmentShaderCode;
            using (StreamReader reader = new StreamReader(fragmentShaderStream))
            {
                fragmentShaderCode = reader.ReadToEnd();
                Console.WriteLine($"FragmentShaderCode: {fragmentShaderCode}");
            }

            this.InitializeShaders(vertexShaderCode, fragmentShaderCode);
        }

        public void Update(double elapsedTime)
        {
            if (!this.IsReady)
                return;

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
            if (!this.IsReady)
                return;

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
