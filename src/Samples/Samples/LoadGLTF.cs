using System;
using System.IO;
using System.Threading.Tasks;
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
        uint inVarNormalAttribute;
        uint inVarPositionAttribute;
        WebGLUniformLocation worldViewProjectionUniformLocation;
        Matrix viewProjectionMatrix;
        Matrix worldViewProjectionMatrix;
        int indexBufferCount;
        WebGLBuffer indexBuffer;
        WebGLBuffer[] vertexBuffers;
        double totalMilliseconds;

        public override async Task InitAsync(JSObject canvas, Vector4 clearColor)
        {
            await base.InitAsync(canvas, clearColor);

            InitializeShaders();

            LoadGltf("DamagedHelmet.glb", out Gltf model, out byte[][] buffers);

            LoadMesh(model, out BufferView indicesBufferView, out BufferView[] attributesBufferView);

            indexBufferCount = indicesBufferView.ByteLength / sizeof(ushort);
            indexBuffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indexBuffer);
            var indexBufferView = buffers[indicesBufferView.Buffer];
            var indices = new byte[indicesBufferView.ByteLength];
            Array.Copy(indexBufferView, indicesBufferView.ByteOffset, indices, 0, indicesBufferView.ByteLength);
            gl.BufferData(
                WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER,
                indices,
                WebGLRenderingContextBase.STATIC_DRAW);

            var vertexBufferCount = attributesBufferView.Length;
            vertexBuffers = new WebGLBuffer[vertexBufferCount];

            for (var i = 0; i < vertexBufferCount; i++)
            {
                var vertexBufferView = attributesBufferView[i];
                var buffer = gl.CreateBuffer();
                gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, buffer);
                var verticesBufferView = buffers[vertexBufferView.Buffer];
                var vertices = new byte[vertexBufferView.ByteLength];
                Array.Copy(
                    verticesBufferView,
                    vertexBufferView.ByteOffset,
                    vertices,
                    0,
                    vertexBufferView.ByteLength);
                gl.BufferData(
                    WebGLRenderingContextBase.ARRAY_BUFFER,
                    vertices,
                    WebGLRenderingContextBase.STATIC_DRAW);
                vertexBuffers[i] = buffer;
            }
        }

        private void LoadMesh(Gltf model, out BufferView indicesBufferView, out BufferView[] attributesBufferView)
        {
            var mesh = model.Meshes[0];
            indicesBufferView = null;
            attributesBufferView = null;

            for (var i = 0; i < mesh.Primitives.Length; i++)
            {
                var primitive = mesh.Primitives[i];

                if (primitive.Indices.HasValue)
                {
                    indicesBufferView = ReadAccessor(model, primitive.Indices.Value);
                }

                var attributesCount = primitive.Attributes.Values.Count;
                attributesBufferView = new BufferView[attributesCount];
                var insertIndex = 0;

                foreach (var attribute in primitive.Attributes)
                {
                    attributesBufferView[insertIndex++] = ReadAccessor(model, attribute.Value);
                }
            }
        }

        private void LoadGltf(string filename, out Gltf model, out byte[][] buffers)
        {
            using (var modelStream = EmbeddedResourceHelper.Load(filename))
            {
                model = Interface.LoadModel(modelStream);
            }

            var buffersLength = model.Buffers.Length;
            buffers = new byte[buffersLength][];

            for (var i = 0; i < buffersLength; i++)
            {
                byte[] bufferBytes;

                using (var modelStream = EmbeddedResourceHelper.Load(filename))
                {
                    bufferBytes = Interface.LoadBinaryBuffer(modelStream);
                }

                buffers[i] = bufferBytes;
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

            gl.UniformMatrix4fv(worldViewProjectionUniformLocation, false, worldViewProjectionMatrix.ToArray());

            gl.DrawElements(
                WebGLRenderingContextBase.TRIANGLES,
                indexBufferCount,
                WebGLRenderingContextBase.UNSIGNED_SHORT,
                0);
        }

        private BufferView ReadAccessor(Gltf model, int index)
        {
            var accessor = model.Accessors[index];

            if (!accessor.BufferView.HasValue)
            {
                return null;
            }

            return model.BufferViews[accessor.BufferView.Value];
        }

        private void InitializeShaders()
        {
            WebGLProgram shaderProgram;

            using (var vertexShaderStream = EmbeddedResourceHelper.Load("GLTFVertexShader.essl"))
            using (var fragmentShaderStream = EmbeddedResourceHelper.Load("GLTFFragmentShader.essl"))
            using (var vertexShaderReader = new StreamReader(vertexShaderStream))
            using (var fragmentShaderReader = new StreamReader(fragmentShaderStream))
            {
                var vertexShader = vertexShaderReader.ReadToEnd();
                var fragmentShader = fragmentShaderReader.ReadToEnd();
                shaderProgram = gl.InitializeShaders(vertexShader, fragmentShader);
            }

            inVarNormalAttribute = (uint)gl.GetAttribLocation(shaderProgram, "in_var_NORMAL");
            inVarPositionAttribute = (uint)gl.GetAttribLocation(shaderProgram, "in_var_POSITION");

            worldViewProjectionUniformLocation = gl.GetUniformLocation(shaderProgram, "worldViewProj");
        }
    }
}
