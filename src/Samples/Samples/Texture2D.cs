using WebGLDotNET;

namespace Samples
{
    // Based on: https://webglfundamentals.org/webgl/lessons/webgl-image-processing.html
    public class Texture2D : BaseSample
    {
        private WebGLUniformLocation resolutionUniform;

        public override string Description => "The image is passed as byte[] in ARGB.";

        public override void Run()
        {
            base.Run();

            var shaderProgram = gl.InitializeShaders(
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

            var positionAttribute = (uint)gl.GetAttribLocation(shaderProgram, "position");
            var textureCoordinateAttribute = (uint)gl.GetAttribLocation(shaderProgram, "textureCoordinate");

            var x1 = 0;
            var x2 = DemoImage.Width;
            var y1 = 0;
            var y2 = DemoImage.Height;
            var positions = new float[]
            {
                x1, y1,
                x2, y1,
                x1, y2,
                x1, y2,
                x2, y1,
                x2, y2
            };
            var positionBuffer = gl.CreateArrayBuffer(positions);

            var textureCoordinateBuffer = gl.CreateArrayBuffer(new float[]
            {
                0, 0, 
                0, 1, 
                1, 0, 
                1, 0, 
                0, 1, 
                1, 1
            });

            var texture = gl.CreateTexture();
            gl.BindTexture(WebGLRenderingContextBase.TEXTURE_2D, texture);

            gl.TexParameteri(
                WebGLRenderingContextBase.TEXTURE_2D, 
                WebGLRenderingContextBase.TEXTURE_WRAP_S, 
                (int)WebGLRenderingContextBase.CLAMP_TO_EDGE);
            gl.TexParameteri(
                WebGLRenderingContextBase.TEXTURE_2D, 
                WebGLRenderingContextBase.TEXTURE_WRAP_T,
                (int)WebGLRenderingContextBase.CLAMP_TO_EDGE);
            gl.TexParameteri(
                WebGLRenderingContextBase.TEXTURE_2D, 
                WebGLRenderingContextBase.TEXTURE_MIN_FILTER,
                (int)WebGLRenderingContextBase.NEAREST);
            gl.TexParameteri(
                WebGLRenderingContextBase.TEXTURE_2D, 
                WebGLRenderingContextBase.TEXTURE_MAG_FILTER,
                (int)WebGLRenderingContextBase.NEAREST);

            var imageData = new ImageData(DemoImage.ARGBColors, DemoImage.Width, DemoImage.Height);
            gl.TexImage2D(
                WebGLRenderingContextBase.TEXTURE_2D, 
                0,
                WebGLRenderingContextBase.RGB,
                WebGLRenderingContextBase.RGB,
                WebGLRenderingContextBase.UNSIGNED_BYTE, 
                imageData);

            this.resolutionUniform = gl.GetUniformLocation(shaderProgram, "resolution");

            gl.EnableVertexAttribArray(positionAttribute);
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, positionBuffer);
            gl.VertexAttribPointer(positionAttribute, 2, WebGLRenderingContextBase.FLOAT, false, 0, 0);

            gl.EnableVertexAttribArray(textureCoordinateAttribute);
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, textureCoordinateBuffer);
            gl.VertexAttribPointer(textureCoordinateAttribute, 2, WebGLRenderingContextBase.FLOAT, false, 0, 0);
        }

        public override void Draw()
        {
            base.Draw();

            gl.Uniform2f(resolutionUniform, canvasWidth, canvasHeight);
            gl.DrawArrays(WebGLRenderingContextBase.TRIANGLES, 0, 6);
        }
    }
}
