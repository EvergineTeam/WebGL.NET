using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using glTFLoader;
using glTFLoader.Schema;
using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public class LoadGLTF : BaseSample
    {
        bool isReady;
        Matrix viewProjectionMatrix;
        Matrix worldViewProjectionMatrix;
        WebGLUniformLocation worldViewProjectionUniformLocation;
        int indexBufferCount;
        WebGLBuffer indexBuffer;
        WebGLBuffer[] vertexBuffers;
        double totalMilliseconds;

        public override async void Run(JSObject canvas, float canvasWidth, float canvasHeight, Vector4 clearColor)
        {
            base.Run(canvas, canvasWidth, canvasHeight, clearColor);

            await InitializeAsync();

            isReady = true;
        }

        public override void Update(double elapsedMilliseconds)
        {
            totalMilliseconds += elapsedMilliseconds;

            var totalMillisecondsFloat = (float)totalMilliseconds;
            var offsetQuaternion = Quaternion.CreateFromAxisAngle(Vector3.Right, MathHelper.PiOver2);
            var rotationQuaternion = Quaternion.CreateFromAxisAngle(Vector3.Forward, totalMillisecondsFloat * 0.001f);
            var worldMatrix = Matrix.CreateFromQuaternion(offsetQuaternion * rotationQuaternion);
            worldViewProjectionMatrix = worldMatrix * viewProjectionMatrix;
        }

        public override void Draw()
        {
            if (!isReady)
            {
                return;
            }

            base.Draw();

            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indexBuffer);

            // Normals
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffers[0]);
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 3, WebGLRenderingContextBase.FLOAT, false, 12, 0);

            // Positions
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffers[1]);
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, WebGLRenderingContextBase.FLOAT, false, 12, 0);

            // Textures
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffers[2]);
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 2, WebGLRenderingContextBase.FLOAT, false, 8, 0);

            gl.UniformMatrix4fv(worldViewProjectionUniformLocation, false, worldViewProjectionMatrix.ToArray());

            gl.DrawElements(
                WebGLRenderingContextBase.TRIANGLES,
                indexBufferCount,
                WebGLRenderingContextBase.UNSIGNED_SHORT,
                0);
        }

        async Task InitializeAsync()
        {
            var viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);
            var projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                canvasWidth / canvasHeight,
                0.1f, 100f);
            viewProjectionMatrix = Matrix.Multiply(viewMatrix, projectionMatrix);

            await InitializeShadersFromAssetsAsync("Assets/GLTFVertexShader.essl", "Assets/GLTFFragmentShader.essl");

            worldViewProjectionUniformLocation = gl.GetUniformLocation(this.shaderProgram, "worldViewProj");

            using (var gltfModelLoader = new GLTFModelLoader())
            {
                await gltfModelLoader.ReadAsync("Assets/DamagedHelmet.glb");

                var mesh = gltfModelLoader.Meshes[0];

                var indexBufferView = mesh.IndicesBufferView;
                indexBufferCount = indexBufferView.ByteLength / sizeof(ushort);
                indexBuffer = gl.CreateBuffer();
                gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indexBuffer);
                var indexBufferPointer = gltfModelLoader.Buffers[indexBufferView.Buffer].Pointer +
                    indexBufferView.ByteOffset;
                var indices = new byte[indexBufferView.ByteLength];
                Marshal.Copy(indexBufferPointer, indices, 0, indexBufferView.ByteLength);
                gl.BufferData(
                    WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER,
                    indices,
                    WebGLRenderingContextBase.STATIC_DRAW);

                var vertexBufferCount = mesh.AttributesBufferView.Length;
                vertexBuffers = new WebGLBuffer[vertexBufferCount];

                for (var i = 0; i < vertexBufferCount; i++)
                {
                    var vertexBufferView = mesh.AttributesBufferView[i];
                    var buffer = gl.CreateBuffer();
                    gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, buffer);
                    var vertexBufferPointer = gltfModelLoader.Buffers[vertexBufferView.Buffer].Pointer +
                        vertexBufferView.ByteOffset;
                    var vertices = new byte[vertexBufferView.ByteLength];
                    Marshal.Copy(vertexBufferPointer, vertices, 0, vertexBufferView.ByteLength);
                    gl.BufferData(
                        WebGLRenderingContextBase.ARRAY_BUFFER,
                        vertices,
                        WebGLRenderingContextBase.STATIC_DRAW);
                    vertexBuffers[i] = buffer;
                }
            }
        }

        class GLTFModelLoader : IDisposable
        {
            Gltf model;

            public BufferInfo[] Buffers;
            public MeshInfo[] Meshes;

            public async Task ReadAsync(string filePath)
            {
                var content = await WasmResourceLoader.LoadAsync(filePath, WasmResourceLoader.GetLocalAddress());
                model = Interface.LoadModel(content);

                var buffersLength = model.Buffers.Length;
                Buffers = new BufferInfo[buffersLength];

                for (var i = 0; i < buffersLength; i++)
                {
                    // TODO copy stream instead of re-loading it :-S
                    content = await WasmResourceLoader.LoadAsync(filePath, WasmResourceLoader.GetLocalAddress());
                    var bufferBytes = Interface.LoadBinaryBuffer(content); //this.model.LoadBinaryBuffer(i, filePath);
                    Buffers[i] = new BufferInfo(bufferBytes);
                }

                var meshesLength = model.Meshes.Length;
                Meshes = new MeshInfo[meshesLength];

                for (var m = 0; m < meshesLength; m++)
                {
                    var mesh = this.model.Meshes[m];
                    BufferView indices = null;
                    BufferView[] attributes = null;

                    for (var p = 0; p < mesh.Primitives.Length; p++)
                    {
                        var primitive = mesh.Primitives[p];

                        if (primitive.Indices.HasValue)
                        {
                            indices = ReadAccessor(primitive.Indices.Value);
                        }

                        var attributesCount = primitive.Attributes.Values.Count;
                        attributes = new BufferView[attributesCount];
                        var insertIndex = 0;

                        foreach (var attribute in primitive.Attributes)
                        {
                            attributes[insertIndex++] = ReadAccessor(attribute.Value);
                        }
                    }

                    Meshes[m] = new MeshInfo(indices, attributes);
                }
            }

            public void Dispose()
            {
                if (Buffers == null)
                {
                    return;
                }

                for (int i = 0; i < this.Buffers.Length; i++)
                {
                    Buffers[i]?.Dispose();
                }

                Buffers = null;
            }

            BufferView ReadAccessor(int index)
            {
                var accessor = model.Accessors[index];

                if (accessor.BufferView.HasValue)
                {
                    return model.BufferViews[accessor.BufferView.Value];
                }
                else
                {
                    return null;
                }
            }
        }

        class BufferInfo : IDisposable
        {
            readonly byte[] bytes;
            readonly GCHandle handle;

            public IntPtr Pointer;

            public BufferInfo(byte[] bufferBytes)
            {
                bytes = bufferBytes;
                handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                Pointer = Marshal.UnsafeAddrOfPinnedArrayElement(bytes, 0);
            }

            public void Dispose()
            {
                handle.Free();
            }
        }

        class MeshInfo
        {
            public BufferView IndicesBufferView;
            public BufferView[] AttributesBufferView;

            public MeshInfo(BufferView indices, BufferView[] attributes)
            {
                IndicesBufferView = indices;
                AttributesBufferView = attributes;
            }
        }
    }
}
