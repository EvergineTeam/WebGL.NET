using System.Drawing;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    // Based on: https://webglfundamentals.org/webgl/lessons/webgl-image-processing.html
    public partial class Draw2DImage : ISample
    {
        int imageWidth = 64;
        int imageHeight = 64;

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
   // convert the rectangle from pixels to 0.0 to 1.0
   vec2 zeroToOne = a_position / u_resolution;

   // convert from 0->1 to 0->2
   vec2 zeroToTwo = zeroToOne * 2.0;

   // convert from 0->2 to -1->+1 (clipspace)
   vec2 clipSpace = zeroToTwo - 1.0;

   gl_Position = vec4(clipSpace * vec2(1, -1), 0, 1);

   // pass the texCoord to the fragment shader
   // The GPU will interpolate this value between points.
   v_texCoord = a_texCoord;
}";
            var vertexShader = gl.CreateShader(gl.VertexShader);
            gl.ShaderSource(vertexShader, vertexShaderCode);
            gl.CompileShader(vertexShader);

            var fragmentShaderCode =
@"precision mediump float;

// our texture
uniform sampler2D u_image;

// the texCoords passed in from the vertex shader.
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
            SetRectangle(gl, 0, 0, imageWidth, imageHeight);

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

        void SetRectangle(WebGL gl, float x, float y, float width, float height)
        {
            var x1 = x;
            var x2 = x + width;
            var y1 = y;
            var y2 = y + height;
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
        }
    }
}
