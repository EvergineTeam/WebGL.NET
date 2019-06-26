using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    // Based on:
    // https://developer.mozilla.org/es/docs/Web/API/WebGL_API/Tutorial/Wtilizando_texturas_en_WebGL
    // , more specifically:
    // https://mdn.github.io/webgl-examples/tutorial/sample6/webgl-demo.js
    public abstract class BaseTexturedCube : BaseSample
    {
        protected WebGLTexture texture;

        WebGLBuffer positionBuffer;
        WebGLBuffer textureCoordBuffer;
        ushort[] indices;
        WebGLBuffer indexBuffer;
        uint vertexPositionAttribute;
        uint textureCoordAttribute;
        WebGLUniformLocation projectionMatrixUniform;
        Matrix projectionMatrix;
        WebGLUniformLocation modelViewMatrixUniform;
        Matrix modelViewMatrix;
        WebGLUniformLocation samplerUniform;
        double totalElapsedTimeSeconds;

        protected bool textureLoaded = false;

        public override void Run()
        {
            base.Run();

            var shaderProgram = InitShaders();

            vertexPositionAttribute = (uint)gl.GetAttribLocation(shaderProgram, "aVertexPosition");
            textureCoordAttribute = (uint)gl.GetAttribLocation(shaderProgram, "aTextureCoord");
            projectionMatrixUniform = gl.GetUniformLocation(shaderProgram, "uProjectionMatrix");
            modelViewMatrixUniform = gl.GetUniformLocation(shaderProgram, "uModelViewMatrix");
            samplerUniform = gl.GetUniformLocation(shaderProgram, "uSampler");

            var positions = new float[]
            {
                // Front face
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                // Back face
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                // Top face
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f, -1.0f,
                // Bottom face
                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,
                // Right face
                 1.0f, -1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                // Left face
                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f
            };
            positionBuffer = gl.CreateArrayBuffer(positions);

            var textureCoordinates = new float[]
            {
                // Front
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Back
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Top
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Bottom
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Right
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                // Left
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f
            };
            textureCoordBuffer = gl.CreateArrayBuffer(textureCoordinates);

            indices = new ushort[]
            {
                0,  1,  2,    0,  2,  3,    // front
                4,  5,  6,    4,  6,  7,    // back
                8,  9,  10,   8,  10, 11,   // top
                12, 13, 14,   12, 14, 15,   // bottom
                16, 17, 18,   16, 18, 19,   // right
                20, 21, 22,   20, 22, 23    // left
            };
            indexBuffer = gl.CreateElementArrayBuffer(indices);

            texture = gl.CreateTexture();

            LoadImage();

            gl.VertexAttribPointer(vertexPositionAttribute, 3, WebGLRenderingContextBase.FLOAT, false, 0, 0);
            gl.EnableVertexAttribArray(vertexPositionAttribute);

            gl.EnableVertexAttribArray(textureCoordAttribute);
            gl.VertexAttribPointer(textureCoordAttribute, 2, WebGLRenderingContextBase.FLOAT, false, 0, 0);
        }

        private WebGLProgram InitShaders()
        {
            return gl.InitializeShaders(
                vertexShaderCode:
@"attribute vec4 aVertexPosition;
attribute vec2 aTextureCoord;

uniform mat4 uModelViewMatrix;
uniform mat4 uProjectionMatrix;

varying highp vec2 vTextureCoord;

void main(void) {
    gl_Position = uProjectionMatrix * uModelViewMatrix * aVertexPosition;
    vTextureCoord = aTextureCoord;
}",
                fragmentShaderCode:
@"varying highp vec2 vTextureCoord;

uniform sampler2D uSampler;

void main(void) {
    gl_FragColor = texture2D(uSampler, vTextureCoord);
}");
        }

        public override void Update(double elapsedTime)
        {
            base.Update(elapsedTime);

            var elapsedTimeSeconds = elapsedTime * 0.001;
            totalElapsedTimeSeconds += elapsedTimeSeconds;

            var aspectRatio = (float)canvasWidth / (float)canvasHeight;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45),
                aspectRatio,
                0.1f,
                100f);

            modelViewMatrix = Matrix.Identity;
            modelViewMatrix *= Matrix.CreateRotationZ((float)totalElapsedTimeSeconds);
            modelViewMatrix *= Matrix.CreateRotationY((float)totalElapsedTimeSeconds * 0.7f);
            modelViewMatrix *= Matrix.CreateTranslation(0f, 0f, -6f);
        }

        public override void Draw()
        {
            base.Draw();

            if (!textureLoaded) return;

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, positionBuffer);
            gl.VertexAttribPointer(vertexPositionAttribute, 3, WebGLRenderingContextBase.FLOAT, false, 0, 0);
            gl.EnableVertexAttribArray(vertexPositionAttribute);

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, textureCoordBuffer);
            gl.VertexAttribPointer(textureCoordAttribute, 2, WebGLRenderingContextBase.FLOAT, false, 0, 0);
            gl.EnableVertexAttribArray(textureCoordAttribute);

            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indexBuffer);

            gl.UniformMatrix4fv(projectionMatrixUniform, false, projectionMatrix.ToArray());
            gl.UniformMatrix4fv(modelViewMatrixUniform, false, modelViewMatrix.ToArray());

            gl.ActiveTexture(WebGLRenderingContextBase.TEXTURE0);
            gl.BindTexture(WebGLRenderingContextBase.TEXTURE_2D, texture);
            gl.Uniform1i(samplerUniform, 0);

            gl.DrawElements(WebGLRenderingContextBase.TRIANGLES, 36, WebGLRenderingContextBase.UNSIGNED_SHORT, 0);
        }

        protected abstract void LoadImage();
    }
}
