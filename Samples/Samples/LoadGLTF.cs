using System;
using System.Drawing;
using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;
using Samples.GLTF;

namespace Samples
{
    public class LoadGLTF : ISample
    {
        float canvasWidth;
        float canvasHeight;
        Vector4 clearColor;
        Matrix viewProj;
        Matrix worldViewProj;
        float time;

        WebGLBuffer[] vertexBuffers;
        WebGLBuffer indexBuffer;
        int indexCount;

        protected WebGLRenderingContextBase gl;

        public string Description => string.Empty;

        public double OldMilliseconds { get; set; }

        public void Run(JSObject canvas, float canvasWidth, float canvasHeight, Vector4 clearColor)
        {
            gl = new WebGL2RenderingContext(canvas);
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;
            this.clearColor = clearColor;

            this.Initialize();
        }

        private void Initialize()
        {
            var view = Matrix.CreateLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);
            var proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, canvasWidth / canvasHeight, 0.1f, 100f);
            this.viewProj = Matrix.Multiply(view, proj);

            // Load gltf mesh
            ////using (var gltf = new GLTFModelLoader($"{this.assetsDirectory.RootPath}/Resources/DamagedHelmet.gltf"))
            ////{
            ////    var mesh = gltf.Meshes[0];

            ////    // Index Buffer
            ////    var indexBufferView = mesh.IndicesBufferView;
            ////    var indexBufferDescription = new BufferDescription((uint)indexBufferView.ByteLength, BufferFlags.IndexBuffer, ResourceUsage.Default);
            ////    var indexPointer = gltf.Buffers[indexBufferView.Buffer].bufferPointer + indexBufferView.ByteOffset;
            ////    this.indexCount = indexBufferView.ByteLength / sizeof(ushort);
            ////    this.indexBuffer = this.graphicsContext.Factory.CreateBuffer(indexPointer, ref indexBufferDescription);

            ////    // Vertex Buffer
            ////    int vertexBufferCount = mesh.AttributeBufferView.Length;
            ////    this.vertexBuffers = new Buffer[vertexBufferCount];
            ////    for (int i = 0; i < vertexBufferCount; i++)
            ////    {
            ////        var vertexBufferView = mesh.AttributeBufferView[i];
            ////        var vertexBufferDescription = new BufferDescription((uint)vertexBufferView.ByteLength, BufferFlags.VertexBuffer, ResourceUsage.Default);
            ////        var vertexPointer = gltf.Buffers[vertexBufferView.Buffer].bufferPointer + vertexBufferView.ByteOffset;
            ////        this.vertexBuffers[i] = this.graphicsContext.Factory.CreateBuffer(vertexPointer, ref vertexBufferDescription);
            ////    }
            ////}
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

            gl.DrawElements(
                WebGLRenderingContextBase.TRIANGLES,
                this.indexCount,
                WebGLRenderingContextBase.UNSIGNED_SHORT,
                0);
        }
    }
}
