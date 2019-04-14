using System;
using System.IO;
using glTFLoader;
using glTFLoader.Schema;

namespace Samples.GLTF
{
    public class GLTFModelLoader : IDisposable
    {
        public Gltf model;

        public BufferInfo[] Buffers;
        public MeshInfo[] Meshes;

        public GLTFModelLoader(string filePath)
        {
            this.Read(filePath);
        }

        private void Read(string filePath)
        {
            // Deserialize gltf
            this.model = this.ObtainGLTF(filePath);

            // read all buffers
            int numBuffers = this.model.Buffers.Length;
            this.Buffers = new BufferInfo[numBuffers];
            for (int i = 0; i < numBuffers; ++i)
            {
                var bufferBytes = this.model.LoadBinaryBuffer(i, filePath);

                this.Buffers[i] = new BufferInfo(bufferBytes);
            }

            // Read meshes
            int meshCount = this.model.Meshes.Length;
            this.Meshes = new MeshInfo[meshCount];
            for (int m = 0; m < meshCount; m++)
            {
                var mesh = this.model.Meshes[m];

                BufferView indices = null;
                BufferView[] attributes = null;
                for (int p = 0; p < mesh.Primitives.Length; p++)
                {
                    var primitive = mesh.Primitives[p];

                    if (primitive.Indices.HasValue)
                    {
                        indices = this.ReadAccessor(primitive.Indices.Value);
                    }

                    int attributeCount = primitive.Attributes.Values.Count;
                    attributes = new BufferView[attributeCount];
                    int insertIndex = 0;
                    foreach (var attribute in primitive.Attributes)
                    {
                        attributes[insertIndex++] = this.ReadAccessor(attribute.Value);
                    }
                }

                this.Meshes[m] = new MeshInfo(indices, attributes);
            }
        }

        private Gltf ObtainGLTF(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new InvalidOperationException($"The asset file \"{filePath}\"");
                }

                return Interface.LoadModel(filePath);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private BufferView ReadAccessor(int index)
        {
            var accessor = this.model.Accessors[index];

            if (accessor.BufferView.HasValue)
            {
                return this.model.BufferViews[accessor.BufferView.Value];
            }
            else
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (this.Buffers == null)
            {
                return;
            }

            for (int i = 0; i < this.Buffers.Length; i++)
            {
                this.Buffers[i].Dispose();
            }

            this.Buffers = null;
        }
    }
}
