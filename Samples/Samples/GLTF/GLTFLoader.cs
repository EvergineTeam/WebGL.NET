﻿using System;
using System.Threading.Tasks;
using glTFLoader;
using glTFLoader.Schema;

namespace Samples.GLTF
{
    public class GLTFModelLoader : IDisposable
    {
        public Gltf model;

        public BufferInfo[] Buffers;
        public MeshInfo[] Meshes;
        private string filePath;

        public GLTFModelLoader()
        {
        }

        public async Task InitializeAsync(string filePath)
        {
            await this.ReadAsync(filePath);
        }

        private async Task ReadAsync(string filePath)
        {
            // Deserialize gltf
            this.model = await this.ObtainGLTFAsync(filePath);

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

        private async System.Threading.Tasks.Task<Gltf> ObtainGLTFAsync(string filePath)
        {
            var content = await WasmResourceLoader.LoadAsync(filePath, WasmResourceLoader.GetLocalAddress());

            var gltf = Interface.LoadModel(content);

            return gltf;
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
