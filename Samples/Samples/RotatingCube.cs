using System;
using System.Drawing;
using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public class RotatingCube : BaseSample
    {
        WebGLBuffer vertexBuffer;
        ushort[] indices;
        WebGLBuffer indexBuffer;
        WebGLBuffer colorBuffer;

        WebGLUniformLocation pMatrixUniform;
        WebGLUniformLocation vMatrixUniform;
        WebGLUniformLocation wMatrixUniform;

        Matrix projectionMatrix;
        Matrix viewMatrix;
        Matrix worldMatrix;

        public override string Description =>
            "Every matrix calc relies in Wave Engine's Math library, consumed through NuGet. This will make @jcant0n " +
            "happy :-)";

        public override void Run()
        {
            base.Run();

            var vertices = new float[]
            {
                -1, -1, -1,
                 1, -1, -1,
                 1,  1, -1,

                -1,  1, -1,
                -1, -1,  1,
                 1, -1,  1,

                 1,  1,  1,
                -1,  1,  1,
                -1, -1, -1,

                -1,  1, -1,
                -1,  1,  1,
                -1, -1,  1,

                 1, -1, -1,
                 1,  1, -1,
                 1,  1,  1,

                 1, -1,  1,
                -1, -1, -1,
                -1, -1,  1,

                 1, -1,  1,
                 1, -1, -1,
                -1,  1, -1,

                -1,  1,  1,
                 1,  1,  1,
                 1,  1, -1
            };
            vertexBuffer = gl.CreateArrayBuffer(vertices);

            indices = new ushort[]
            {
                 0,  1,  2,
                 0,  2,  3,

                 4,  5,  6,
                 4,  6,  7,

                 8,  9, 10,
                 8, 10, 11,

                12, 13, 14,
                12, 14, 15,

                16, 17, 18,
                16, 18, 19,

                20, 21, 22,
                20, 22, 23
            };
            indexBuffer = gl.CreateElementArrayBuffer(indices);

            var colors = new float[]
            {
                1, 0, 0,
                1, 0, 0,
                1, 0, 0,
                1, 0, 0,

                0, 1, 0,
                0, 1, 0,
                0, 1, 0,
                0, 1, 0,

                0, 0, 1,
                0, 0, 1,
                0, 0, 1,
                0, 0, 1,

                1, 1, 0,
                1, 1, 0,
                1, 1, 0,
                1, 1, 0,

                0, 1, 1,
                0, 1, 1,
                0, 1, 1,
                0, 1, 1,

                1, 1, 1,
                1, 1, 1,
                1, 1, 1,
                1, 1, 1
            };
            colorBuffer = gl.CreateArrayBuffer(colors);

            var shaderProgram = gl.InitializeShaders(
                vertexShaderCode:
@"attribute vec3 position;
attribute vec3 color;

uniform mat4 pMatrix;
uniform mat4 vMatrix;
uniform mat4 wMatrix;

varying vec3 vColor;

void main(void) {
    gl_Position = pMatrix * vMatrix * wMatrix * vec4(position, 1.0);
    vColor = color;
}",
                fragmentShaderCode:
@"precision mediump float;

varying vec3 vColor;

void main(void) {
    gl_FragColor = vec4(vColor, 1.0);
}");

            pMatrixUniform = gl.GetUniformLocation(shaderProgram, "pMatrix");
            vMatrixUniform = gl.GetUniformLocation(shaderProgram, "vMatrix");
            wMatrixUniform = gl.GetUniformLocation(shaderProgram, "wMatrix");

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, vertexBuffer);
            var positionAttribute = (uint)gl.GetAttribLocation(shaderProgram, "position");
            gl.VertexAttribPointer(positionAttribute, 3, WebGLRenderingContextBase.FLOAT, false, 0, 0);
            gl.EnableVertexAttribArray(positionAttribute);

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, colorBuffer);
            var colorAttribute = (uint)gl.GetAttribLocation(shaderProgram, "color");
            gl.VertexAttribPointer(colorAttribute, 3, WebGLRenderingContextBase.FLOAT, false, 0, 0);
            gl.EnableVertexAttribArray(colorAttribute);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI / 4, canvasWidth / canvasHeight, 0.1f, 1000f);
            gl.UniformMatrix4fv(pMatrixUniform, false, projectionMatrix.ToArray());

            viewMatrix = Matrix.CreateLookAt(Vector3.UnitZ * 10, Vector3.Zero, Vector3.Up);
            gl.UniformMatrix4fv(vMatrixUniform, false, viewMatrix.ToArray());

            worldMatrix = Matrix.Identity;

            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, indexBuffer);
        }

        public override void Update(double elapsedMilliseconds)
        {
            base.Update(elapsedMilliseconds);

            var elapsedMillisecondsFloat = (float)elapsedMilliseconds;
            var rotation = Quaternion.CreateFromYawPitchRoll(
                elapsedMillisecondsFloat * 2 * 0.001f,
                elapsedMillisecondsFloat * 4 * 0.001f,
                elapsedMillisecondsFloat * 3 * 0.001f);
            worldMatrix *= Matrix.CreateFromQuaternion(rotation);
        }

        public override void Draw()
        {
            base.Draw();

            gl.UniformMatrix4fv(wMatrixUniform, false, worldMatrix.ToArray());

            gl.DrawElements(
                WebGLRenderingContextBase.TRIANGLES,
                indices.Length,
                WebGLRenderingContextBase.UNSIGNED_SHORT,
                0);
        }
    }
}