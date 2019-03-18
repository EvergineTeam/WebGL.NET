using System.Drawing;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    // Based on: https://webglfundamentals.org/webgl/lessons/webgl-image-processing.html
    public partial class Draw2DImage : ISample
    {
        public string Description => "The image is passed as byte[] in ARGB.";

        public void Run(JSObject canvas, int canvasWidth, int canvasHeight, Color clearColor)
        {
            var gl = WebGL.GetContext(canvas);

            var vertexShaderCode =
@"attribute vec2 a_position;
attribute vec2 a_texCoord;

uniform vec2 u_resolution;

varying vec2 v_texCoord;

void main() {
   vec2 zeroToOne = a_position / u_resolution;
   vec2 zeroToTwo = zeroToOne * 2.0;
   vec2 clipSpace = zeroToTwo - 1.0;
   gl_Position = vec4(clipSpace * vec2(1, -1), 0, 1);

   v_texCoord = a_texCoord;
}";
            var vertexShader = gl.CreateShader(gl.VertexShader);
            gl.ShaderSource(vertexShader, vertexShaderCode);
            gl.CompileShader(vertexShader);

            var fragmentShaderCode =
@"precision mediump float;

uniform sampler2D u_image;

varying vec2 v_texCoord;

void main() {
   gl_FragColor = texture2D(u_image, v_texCoord);
}";
            var fragmentShader = gl.CreateShader(gl.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentShaderCode);
            gl.CompileShader(fragmentShader);

            var program = gl.CreateProgram();
            gl.AttachShader(program, vertexShader);
            gl.AttachShader(program, fragmentShader);
            gl.LinkProgram(program);

            var positionLocation = gl.GetAttribLocation(program, "a_position");
            var texcoordLocation = gl.GetAttribLocation(program, "a_texCoord");

            var positionBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ArrayBuffer, positionBuffer);
            var x1 = 0;
            var x2 = imageWidth;
            var y1 = 0;
            var y2 = imageHeight;
            gl.BufferData(
                gl.ArrayBuffer,
                new float[]
                {
                    x1, y1,
                    x2, y1,
                    x1, y2,
                    x1, y2,
                    x2, y1,
                    x2, y2
                },
                gl.StaticDraw);

            var texcoordBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ArrayBuffer, texcoordBuffer);
            gl.BufferData(
                gl.ArrayBuffer, 
                new float[]
                {
                    0, 0, 
                    0, 1, 
                    1, 0, 
                    1, 0, 
                    0, 1, 
                    1, 1,
                },
                gl.StaticDraw);

            var texture = gl.CreateTexture();
            gl.BindTexture(gl.Texture2D, texture);

            gl.TexParameteri(gl.Texture2D, gl.TextureWrapS, gl.ClampToEdge);
            gl.TexParameteri(gl.Texture2D, gl.TextureWrapT, gl.ClampToEdge);
            gl.TexParameteri(gl.Texture2D, gl.TextureMinFilter, gl.Nearest);
            gl.TexParameteri(gl.Texture2D, gl.TextureMagFilter, gl.Nearest);

            var uint8ClampedArray = (JSObject)Runtime.GetGlobalObject("Uint8ClampedArray");
            var imageNativeArray = Runtime.NewJSObject(uint8ClampedArray, image);
            var imageDataObject = (JSObject)Runtime.GetGlobalObject("ImageData");
            var imageData = Runtime.NewJSObject(imageDataObject, imageNativeArray, imageWidth, imageHeight);
            gl.TexImage2D(gl.Texture2D, 0, gl.RGB, gl.RGB, gl.UnsignedByte, imageData);

            var resolutionLocation = gl.GetUniformLocation(program, "u_resolution");

            gl.Viewport(0, 0, canvasWidth, canvasHeight);

            gl.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
            gl.Clear(gl.ColorBufferBit);

            gl.UseProgram(program);

            gl.EnableVertexAttribArray(positionLocation);
            gl.BindBuffer(gl.ArrayBuffer, positionBuffer);
            gl.VertexAttribPointer(positionLocation, 2, gl.Float, false, 0, 0);

            gl.EnableVertexAttribArray(texcoordLocation);
            gl.BindBuffer(gl.ArrayBuffer, texcoordBuffer);
            gl.VertexAttribPointer(texcoordLocation, 2, gl.Float, false, 0, 0);

            gl.Uniform2f(resolutionLocation, canvasWidth, canvasHeight);

            gl.DrawArrays(gl.Triangles, 0, 6);
        }
    }
}
