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

        protected WebGLRenderingContextBase gl;
        protected WebGLShader vertexShader;
        protected WebGLShader fragmentShader;
        protected WebGLProgram shaderProgram;

        public virtual string Description => string.Empty;

        public double OldMilliseconds { get; set; }

        public virtual void Draw()
        {
            gl.Enable(WebGLRenderingContextBase.DEPTH_TEST);

            gl.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
            gl.Clear(WebGLRenderingContextBase.COLOR_BUFFER_BIT);
        }

        public virtual void Run(JSObject canvas, float canvasWidth, float canvasHeight, Color clearColor)
        {
            gl = WebGLRenderingContextBase.GetContext(canvas);
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
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, arrayBuffer);
            gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, items, WebGLRenderingContextBase.STATIC_DRAW);
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, null);

            return arrayBuffer;
        }

        protected WebGLBuffer CreateElementArrayBuffer(Array items)
        {
            var elementArrayBuffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, elementArrayBuffer);
            gl.BufferData(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, items, WebGLRenderingContextBase.STATIC_DRAW);
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, null);

            return elementArrayBuffer;
        }

        protected void InitializeShaders(string vertexShaderCode, string fragmentShaderCode)
        {
            vertexShader = gl.CreateShader(WebGLRenderingContextBase.VERTEX_SHADER);
            gl.ShaderSource(vertexShader, vertexShaderCode);
            gl.CompileShader(vertexShader);

            fragmentShader = gl.CreateShader(WebGLRenderingContextBase.FRAGMENT_SHADER);
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
