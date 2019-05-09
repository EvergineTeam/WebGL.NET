using System;
using System.IO;
using System.Runtime.InteropServices;
using glTFLoader;
using glTFLoader.Schema;
using Samples.Helpers;
using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public class LoadGLTF : BaseSample
    {
        Matrix viewProjectionMatrix;
        Matrix worldViewProjectionMatrix;
        WebGLUniformLocation worldViewProjectionUniformLocation;
        int indexBufferCount;
        WebGLBuffer indexBuffer;
        WebGLBuffer[] vertexBuffers;
        double totalMilliseconds;

        private uint inVarNORMAL;
        private uint inVarPOSITION;
        private uint inVarTEXCOORD;

        public override void Run()
        {
            base.Run();

            Initialize();
        }

        void Initialize()
        {
            var aspectRatio = (float)canvasWidth / (float)canvasHeight;
            var viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);
            var projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                aspectRatio,
                0.1f, 100f);
            viewProjectionMatrix = Matrix.Multiply(viewMatrix, projectionMatrix);

            WebGLProgram shaderProgram;

            using (var vs = EmbeddedResourceHelper.Load("GLTFVertexShader.essl"))
            using (var fs = EmbeddedResourceHelper.Load("GLTFFragmentShader.essl"))
            using (var vsReader = new StreamReader(vs))
            using (var fsReader = new StreamReader(fs))
            {
                var vertexShader = vsReader.ReadToEnd();
                var fragmentShader = fsReader.ReadToEnd();

                shaderProgram = gl.InitializeShaders(vertexShader, fragmentShader);
            }

            this.inVarNORMAL = (uint)gl.GetAttribLocation(shaderProgram, "in_var_NORMAL");
            this.inVarPOSITION = (uint)gl.GetAttribLocation(shaderProgram, "in_var_POSITION");
            this.inVarTEXCOORD = (uint)gl.GetAttribLocation(shaderProgram, "in_var_TEXCOORD");

            worldViewProjectionUniformLocation = gl.GetUniformLocation(shaderProgram, "worldViewProj");

            using (var gltfModelLoader = new GLTFModelLoader())
            {
                using (var stream = EmbeddedResourceHelper.Load("DamagedHelmet.glb"))
                {
                    gltfModelLoader.ReadModel(stream);

                    gltfModelLoader.ReadBuffers(() => EmbeddedResourceHelper.Load("DamagedHelmet.glb"));

                    gltfModelLoader.ReadMeshes();

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
            base.Draw();

            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indexBuffer);

            // Normals
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffers[0]);
            gl.EnableVertexAttribArray(inVarNORMAL);
            gl.VertexAttribPointer(inVarNORMAL, 3, WebGLRenderingContextBase.FLOAT, false, 12, 0);

            // Positions
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffers[1]);
            gl.EnableVertexAttribArray(inVarPOSITION);
            gl.VertexAttribPointer(inVarPOSITION, 3, WebGLRenderingContextBase.FLOAT, false, 12, 0);

            // Textures
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffers[2]);
            gl.EnableVertexAttribArray(inVarTEXCOORD);
            gl.VertexAttribPointer(inVarTEXCOORD, 2, WebGLRenderingContextBase.FLOAT, false, 8, 0);

            gl.UniformMatrix4fv(worldViewProjectionUniformLocation, false, worldViewProjectionMatrix.ToArray());

            gl.DrawElements(
                WebGLRenderingContextBase.TRIANGLES,
                indexBufferCount,
                WebGLRenderingContextBase.UNSIGNED_SHORT,
                0);
        }

        class GLTFModelLoader : IDisposable
        {
            Gltf model;

            public BufferInfo[] Buffers;
            public MeshInfo[] Meshes;

            public void ReadModel(Stream stream)
            {
                model = Interface.LoadModel(stream);
            }

            public void ReadMeshes()
            {
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

            public void ReadBuffers(Func<Stream> streamLoader)
            {
                var buffersLength = model.Buffers.Length;
                Buffers = new BufferInfo[buffersLength];

                for (var i = 0; i < buffersLength; i++)
                {
                    using(var stream = streamLoader())
                    {
                        var bufferBytes = Interface.LoadBinaryBuffer(stream); 
                        Buffers[i] = new BufferInfo(bufferBytes);
                    }
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
