using System;
using System.Drawing;
using WaveEngine.Common.Math;
using WebAssembly;

namespace Samples
{
    public class RotatingCube : BaseSample
    {
        object vertexBuffer;
        ushort[] indices;
        object indexBuffer;
        object colorBuffer;

        object pMatrixUniform;
        object vMatrixUniform;
        object wMatrixUniform;

        Matrix projectionMatrix;
        Matrix viewMatrix;
        Matrix worldMatrix;

        public override string Description =>
            "Every matrix calc relies in Wave Engine's Math library, consumed through NuGet. This will make @jcant0n " +
            "happy :-)";

        public override void Run(JSObject canvas, float canvasWidth, float canvasHeight, Color clearColor)
        {
            base.Run(canvas, canvasWidth, canvasHeight, clearColor);

            var vertices = new float[]
            {
                -1, -1, -1, 1, -1, -1, 1, 1, -1, -1, 1, -1,
                -1, -1, 1, 1, -1, 1, 1, 1, 1, -1, 1, 1,
                -1, -1, -1, -1, 1, -1, -1, 1, 1, -1, -1, 1,
                1, -1, -1, 1, 1, -1, 1, 1, 1, 1, -1, 1,
                -1, -1, -1, -1, -1, 1, 1, -1, 1, 1, -1, -1,
                -1, 1, -1, -1, 1, 1, 1, 1, 1, 1, 1, -1,
            };
            vertexBuffer = CreateArrayBuffer(vertices);

            indices = new ushort[]
            {
                0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7,
                8, 9, 10, 8, 10, 11, 12, 13, 14, 12, 14, 15,
                16, 17, 18, 16, 18, 19, 20, 21, 22, 20, 22, 23
            };
            indexBuffer = CreateElementArrayBuffer(indices);

            var colors = new float[]
            {
                5, 3, 7, 5, 3, 7, 5, 3, 7, 5, 3, 7,
                1, 1, 3, 1, 1, 3, 1, 1, 3, 1, 1, 3,
                0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1,
                1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0,
                1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0,
                0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0
            };
            colorBuffer = CreateArrayBuffer(colors);

            InitializeShaders(
                vertexShaderCode:
@"attribute vec3 position;
uniform mat4 pMatrix;
uniform mat4 vMatrix;
uniform mat4 wMatrix;
attribute vec3 color;
varying vec3 vColor;

void main(void) {
    gl_Position = pMatrix * vMatrix * wMatrix * vec4(position, 1.);
    vColor = color;
}",
                fragmentShaderCode:
@"precision mediump float;
varying vec3 vColor;

void main(void) {
    gl_FragColor = vec4(vColor, 1.);
}");

            pMatrixUniform = gl.GetUniformLocation(shaderProgram, "pMatrix");
            vMatrixUniform = gl.GetUniformLocation(shaderProgram, "vMatrix");
            wMatrixUniform = gl.GetUniformLocation(shaderProgram, "wMatrix");

            gl.BindBuffer(gl.ArrayBuffer, vertexBuffer);
            var positionAttribute = gl.GetAttribLocation(shaderProgram, "position");
            gl.VertexAttribPointer(positionAttribute, 3, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(positionAttribute);

            gl.BindBuffer(gl.ArrayBuffer, colorBuffer);
            var colorAttribute = gl.GetAttribLocation(shaderProgram, "color");
            gl.VertexAttribPointer(colorAttribute, 3, gl.Float, false, 0, 0);
            gl.EnableVertexAttribArray(colorAttribute);

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                (float)Math.PI / 4, canvasWidth / canvasHeight, 0.1f, 1000f);
            viewMatrix = Matrix.CreateLookAt(Vector3.UnitZ * 10, Vector3.Zero, Vector3.Up);
            worldMatrix = Matrix.Identity;
        }

        public override void Update(double elapsedTime)
        {
            base.Update(elapsedTime);

            var elapsedTimeFloat = (float)elapsedTime;
            var rotation = Quaternion.CreateFromYawPitchRoll(
                elapsedTimeFloat * 0.003f, 
                elapsedTimeFloat * 0.002f, 
                elapsedTimeFloat * 0.005f);
            worldMatrix *= Matrix.CreateFromQuaternion(rotation);
        }

        public override void Draw()
        {
            gl.DepthFunc(gl.LEqual);
            gl.ClearDepth(1.0);

            base.Draw();

            gl.UniformMatrix4fv(pMatrixUniform, false, projectionMatrix.ToArray());
            gl.UniformMatrix4fv(vMatrixUniform, false, viewMatrix.ToArray());
            gl.UniformMatrix4fv(wMatrixUniform, false, worldMatrix.ToArray());

            gl.BindBuffer(gl.ElementArrayBuffer, indexBuffer);
            gl.DrawElements(gl.Triangles, indices.Length, gl.UnsignedShort, 0);
        }
    }
}
