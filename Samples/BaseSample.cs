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
        protected WebGLShader vertexShader;
        protected WebGLShader fragmentShader;
        protected WebGLProgram shaderProgram;

        public virtual string Description => string.Empty;

        public double OldMilliseconds { get; set; }

        public virtual void Draw()
        {
            gl.Enable(gl.DepthTest);

            gl.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
            gl.Clear(gl.ColorBufferBit);
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

        protected WebGLBuffer CreateArrayBuffer(Array items)
        {
            var arrayBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ArrayBuffer, arrayBuffer);
            gl.BufferData(gl.ArrayBuffer, items, gl.StaticDraw);
            gl.BindBuffer(gl.ArrayBuffer, null);

            return arrayBuffer;
        }

        protected WebGLBuffer CreateElementArrayBuffer(Array items)
        {
            var elementArrayBuffer = gl.CreateBuffer();
            gl.BindBuffer(gl.ElementArrayBuffer, elementArrayBuffer);
            gl.BufferData(gl.ElementArrayBuffer, items, gl.StaticDraw);
            gl.BindBuffer(gl.ElementArrayBuffer, null);

            return elementArrayBuffer;
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
