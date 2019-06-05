using System;
using System.IO;
using System.Runtime.InteropServices;
using glTFLoader;
using glTFLoader.Schema;
using Samples.Helpers;
using WaveEngine.Common.Math;
using WebGLDotNET;

namespace Samples
{
    public class LoadGLTF : BaseSample
    {
        Matrix viewProjectionMatrix;
        Matrix worldViewProjectionMatrix;
        WebGLUniformLocation worldViewProjectionUniformLocation;
        uint inVarNormalAttribute;
        uint inVarPositionAttribute;
        uint inVarTexCoordAttribute;
        int indexBufferCount;
        WebGLBuffer indexBuffer;
        WebGLBuffer[] vertexBuffers;
        double totalMilliseconds;

        public override void Run()
        {
            base.Run();

            Initialize();
        }

        void Initialize()
        {
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

            inVarNormalAttribute = (uint)gl.GetAttribLocation(shaderProgram, "in_var_NORMAL");
            inVarPositionAttribute = (uint)gl.GetAttribLocation(shaderProgram, "in_var_POSITION");
            inVarTexCoordAttribute = (uint)gl.GetAttribLocation(shaderProgram, "in_var_TEXCOORD");

            worldViewProjectionUniformLocation = gl.GetUniformLocation(shaderProgram, "worldViewProj");

            using (var gltfModelLoader = new GLTFModelLoader())
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

                gltfModelLoader.ReadImagesAsync();
            }
        }

        public override void Update(double elapsedMilliseconds)
        {
            totalMilliseconds += elapsedMilliseconds;

            var viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), Vector3.UnitY);
            var projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4,
                (float)canvasWidth / canvasHeight,
                0.1f, 100f);
            viewProjectionMatrix = Matrix.Multiply(viewMatrix, projectionMatrix);

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
            gl.EnableVertexAttribArray(inVarNormalAttribute);
            gl.VertexAttribPointer(inVarNormalAttribute, 3, WebGLRenderingContextBase.FLOAT, false, 12, 0);

            // Positions
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffers[1]);
            gl.EnableVertexAttribArray(inVarPositionAttribute);
            gl.VertexAttribPointer(inVarPositionAttribute, 3, WebGLRenderingContextBase.FLOAT, false, 12, 0);

            // Textures
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffers[2]);
            gl.EnableVertexAttribArray(inVarTexCoordAttribute);
            gl.VertexAttribPointer(inVarTexCoordAttribute, 2, WebGLRenderingContextBase.FLOAT, false, 8, 0);

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
            public byte[][] Images;

            public void ReadModel(Stream stream)
            {
                model = Interface.LoadModel(stream);
            }

            public async void ReadImagesAsync()
            {
                // Workaround while JPEG load performance is improved
                const string fileNameFormat = "DamagedHelmet_img{0}.bmp";
                var imagesLength = model.Images.Length;
                Images = new byte[imagesLength][];
                var baseAddress = $"{WasmResourceLoader.GetLocalAddress()}Assets/";

                for (var i = 0; i < imagesLength; i++)
                {
                    var fileName = string.Format(fileNameFormat, i);
                    var image = await WasmResourceLoader.LoadAsync(fileName, baseAddress);

                    using (var memoryStream = new MemoryStream())
                    {
                        await image.CopyToAsync(memoryStream);
                        Images[i] = memoryStream.ToArray();

#if DEBUG
                        Console.WriteLine($"Image {i}: {Images[i].Length} B");
#endif
                    }
                }
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
                    using (var stream = streamLoader())
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
