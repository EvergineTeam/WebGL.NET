using System.Drawing;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    // Based on: https://webglfundamentals.org/webgl/lessons/webgl-image-processing.html
    public class Texture2D : BaseSample
    {
        public override string Description => "The image is passed as byte[] in ARGB.";

        public override void Run(JSObject canvas, float canvasWidth, float canvasHeight, Color clearColor)
        {
            base.Run(canvas, canvasWidth, canvasHeight, clearColor);

            InitializeShaders(
                vertexShaderCode:
@"attribute vec2 position;
attribute vec2 textureCoordinate;

uniform vec2 resolution;

varying vec2 v_texCoord;

void main() {
   vec2 zeroToOne = position / resolution;
   vec2 zeroToTwo = zeroToOne * 2.0;
   vec2 clipSpace = zeroToTwo - 1.0;
   gl_Position = vec4(clipSpace * vec2(1.0, -1.0), 0.0, 1.0);

   v_texCoord = textureCoordinate;
}",
                fragmentShaderCode:
@"precision mediump float;

uniform sampler2D u_image;

varying vec2 v_texCoord;

void main() {
   gl_FragColor = texture2D(u_image, v_texCoord);
}");

            var positionAttribute = gl.GetAttribLocation(shaderProgram, "position");
            var textureCoordinateAttribute = gl.GetAttribLocation(shaderProgram, "textureCoordinate");

            var x1 = 0;
            var x2 = Image.Width;
            var y1 = 0;
            var y2 = Image.Height;
            var positions = new float[]
            {
                x1, y1,
                x2, y1,
                x1, y2,
                x1, y2,
                x2, y1,
                x2, y2
            };
            var positionBuffer = CreateArrayBuffer(positions);

            var textureCoordinateBuffer = CreateArrayBuffer(new float[]
            {
                0, 0, 
                0, 1, 
                1, 0, 
                1, 0, 
                0, 1, 
                1, 1
            });

            var texture = gl.CreateTexture();
            gl.BindTexture(gl.Texture2D, texture);

            gl.TexParameteri(gl.Texture2D, gl.TextureWrapS, gl.ClampToEdge);
            gl.TexParameteri(gl.Texture2D, gl.TextureWrapT, gl.ClampToEdge);
            gl.TexParameteri(gl.Texture2D, gl.TextureMinFilter, gl.Nearest);
            gl.TexParameteri(gl.Texture2D, gl.TextureMagFilter, gl.Nearest);

            var imageData = new ImageData(Image.ARGBColors, Image.Width, Image.Height);
            gl.TexImage2D(gl.Texture2D, 0, gl.RGB, gl.RGB, gl.UnsignedByte, imageData);

            var resolutionUniform = gl.GetUniformLocation(shaderProgram, "resolution");
            gl.Uniform2f(resolutionUniform, canvasWidth, canvasHeight);

            gl.EnableVertexAttribArray(positionAttribute);
            gl.BindBuffer(gl.ArrayBuffer, positionBuffer);
            gl.VertexAttribPointer(positionAttribute, 2, gl.Float, false, 0, 0);

            gl.EnableVertexAttribArray(textureCoordinateAttribute);
            gl.BindBuffer(gl.ArrayBuffer, textureCoordinateBuffer);
            gl.VertexAttribPointer(textureCoordinateAttribute, 2, gl.Float, false, 0, 0);
        }

        public override void Draw()
        {
            base.Draw();

            gl.DrawArrays(gl.Triangles, 0, 6);
        }
    }
}
