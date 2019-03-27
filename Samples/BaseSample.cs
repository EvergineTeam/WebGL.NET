using System;
using System.Drawing;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public abstract class BaseSample : ISample
    {
        float canvasWidth;
        float canvasHeight;
        Color clearColor;

        protected WebGL gl;
        protected object vertexShader;
        protected object fragmentShader;
        protected object shaderProgram;

        public virtual string Description => string.Empty;

        public double OldMilliseconds { get; set; }

        public virtual void Draw()
        {
            gl.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
            gl.Enable(gl.DepthTest);
            gl.Clear(gl.ColorBufferBit);
            gl.Viewport(0, 0, canvasWidth, canvasHeight);
        }

        public virtual void Run(JSObject canvas, float canvasWidth, float canvasHeight, Color clearColor)
        {
            gl = WebGL.GetContext(canvas);
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;
            this.clearColor = clearColor;
        }

        public virtual void Update(double elapsedMilliseconds)
        {
        }

        protected object CreateArrayBuffer(Array items)
        {
            var arrayBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ArrayBuffer, arrayBuffer);
            gl.BufferData(gl.ArrayBuffer, items, gl.StaticDraw);
            gl.BindBuffer(gl.ArrayBuffer, null);

            return arrayBuffer;
        }

        protected object CreateElementArrayBuffer(Array items)
        {
            var elementArrayBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ElementArrayBuffer, elementArrayBuffer);
            gl.BufferData(gl.ElementArrayBuffer, items, gl.StaticDraw);
            gl.BindBuffer(gl.ElementArrayBuffer, null);

            return elementArrayBuffer;
        }

        protected object CreateTexture()
        {
            var texture = gl.CreateTexture();
            gl.BindTexture(gl.Texture2D, texture);

            gl.TexParameteri(gl.Texture2D, gl.TextureWrapS, gl.ClampToEdge);
            gl.TexParameteri(gl.Texture2D, gl.TextureWrapT, gl.ClampToEdge);
            gl.TexParameteri(gl.Texture2D, gl.TextureMinFilter, gl.Nearest);
            gl.TexParameteri(gl.Texture2D, gl.TextureMagFilter, gl.Nearest);

            var imageData = new ImageData(Image.ARGBColors, Image.Width, Image.Height);
            gl.TexImage2D(gl.Texture2D, 0, gl.RGB, gl.RGB, gl.UnsignedByte, imageData);

            return texture;
        }

        protected void InitializeShaders(string vertexShaderCode, string fragmentShaderCode)
        {
            vertexShader = gl.CreateShader(gl.VertexShader);
            gl.ShaderSource(vertexShader, vertexShaderCode);
            gl.CompileShader(vertexShader);

            fragmentShader = gl.CreateShader(gl.FragmentShader);
            gl.ShaderSource(fragmentShader, fragmentShaderCode);
            gl.CompileShader(fragmentShader);

            shaderProgram = gl.CreateProgram();
            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.LinkProgram(shaderProgram);
            gl.UseProgram(shaderProgram);
        }
    }
}
