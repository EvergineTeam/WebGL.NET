using System.Drawing;
using WaveEngine.Common.Math;
using WebAssembly;

namespace Samples
{
    // Based on:
    // https://developer.mozilla.org/es/docs/Web/API/WebGL_API/Tutorial/Wtilizando_texturas_en_WebGL
    // , more specifically:
    // https://mdn.github.io/webgl-examples/tutorial/sample6/webgl-demo.js
    public class TexturedCube : BaseSample
    {
        object positionBuffer;
        object textureCoordBuffer;
        ushort[] indices;
        object indexBuffer;
        object texture;
        object vertexPositionAttribute;
        object textureCoordAttribute;
        object projectionMatrixUniform;
        Matrix projectionMatrix;
        object modelViewMatrixUniform;
        Matrix modelViewMatrix;
        object samplerUniform;
        double totalElapsedTimeSeconds;

        public override string Description => 
            "The image is passed as byte[] in ARGB and Wave Engine is ruling the matrices.";

        public override void Run(JSObject canvas, float canvasWidth, float canvasHeight, Color clearColor)
        {
            base.Run(canvas, canvasWidth, canvasHeight, clearColor);

            InitializeShaders(
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

            vertexPositionAttribute = gl.GetAttribLocation(shaderProgram, "aVertexPosition");
            textureCoordAttribute = gl.GetAttribLocation(shaderProgram, "aTextureCoord");
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
            positionBuffer = CreateArrayBuffer(positions);

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
            textureCoordBuffer = CreateArrayBuffer(textureCoordinates);

            indices = new ushort[]
            {
                0,  1,  2,    0,  2,  3,    // front
                4,  5,  6,    4,  6,  7,    // back
                8,  9,  10,   8,  10, 11,   // top
                12, 13, 14,   12, 14, 15,   // bottom
                16, 17, 18,   16, 18, 19,   // right
                20, 21, 22,   20, 22, 23    // left
            };
            indexBuffer = CreateElementArrayBuffer(indices);

            texture = CreateTexture();

            gl.VertexAttribPointer(vertexPositionAttribute, 3, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(vertexPositionAttribute);

            gl.EnableVertexAttribArray(textureCoordAttribute);
            gl.VertexAttribPointer(textureCoordAttribute, 2, gl.Float, false, 0, 0);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45), 
                canvasWidth / canvasHeight, 
                0.1f, 
                100f);
        }

        public override void Update(double elapsedTime)
        {
            base.Update(elapsedTime);

            var elapsedTimeSeconds = elapsedTime * 0.001;
            totalElapsedTimeSeconds += elapsedTimeSeconds;

            modelViewMatrix = Matrix.Identity;
            modelViewMatrix *= Matrix.CreateRotationZ((float)totalElapsedTimeSeconds);
            modelViewMatrix *= Matrix.CreateRotationY((float)totalElapsedTimeSeconds * 0.7f);
            modelViewMatrix *= Matrix.CreateTranslation(0f, 0f, -6f);
        }

        public override void Draw()
        {
            gl.ClearDepth(1.0);
            gl.DepthFunc(gl.LEqual);

            base.Draw();

            gl.BindBuffer(gl.ArrayBuffer, positionBuffer);
            gl.VertexAttribPointer(vertexPositionAttribute, 3, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(vertexPositionAttribute);

            gl.BindBuffer(gl.ArrayBuffer, textureCoordBuffer);
            gl.VertexAttribPointer(textureCoordAttribute, 2, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(textureCoordAttribute);

            gl.BindBuffer(gl.ElementArrayBuffer, indexBuffer);

            gl.UniformMatrix4fv(projectionMatrixUniform, false, projectionMatrix.ToArray());
            gl.UniformMatrix4fv(modelViewMatrixUniform, false, modelViewMatrix.ToArray());

            gl.ActiveTexture(gl.Texture0);
            gl.BindTexture(gl.Texture2D, texture);
            gl.Uniform1i(samplerUniform, 0);

            gl.DrawElements(gl.Triangles, 36, gl.UnsignedShort, 0);
        }
    }
}
