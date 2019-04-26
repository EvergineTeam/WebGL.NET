using System;
using System.IO;
using System.Threading.Tasks;
using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public abstract class BaseSample : ISample
    {
        protected WebGLRenderingContextBase gl;
        protected float canvasWidth;
        protected float canvasHeight;
        protected Vector4 clearColor;
        protected WebGLShader vertexShader;
        protected WebGLShader fragmentShader;
        protected WebGLProgram shaderProgram;

        public virtual string Description => string.Empty;

        public double OldMilliseconds { get; set; }

        public bool IsReady { get; set; }

        public virtual void Draw()
        {
            gl.Enable(WebGLRenderingContextBase.DEPTH_TEST);

            gl.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
            gl.Clear(WebGLRenderingContextBase.COLOR_BUFFER_BIT);
        }

        public virtual void Run(JSObject canvas, float canvasWidth, float canvasHeight, Vector4 clearColor)
        {
            gl = new WebGL2RenderingContext(canvas);
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

            var message = gl.GetShaderInfoLog(vertexShader);
            if (message.Length > 0)
            {
                throw new InvalidOperationException($"Shader Error: {message}");
            }

            fragmentShader = gl.CreateShader(WebGLRenderingContextBase.FRAGMENT_SHADER);
            gl.ShaderSource(fragmentShader, fragmentShaderCode);
            gl.CompileShader(fragmentShader);

            message = gl.GetShaderInfoLog(fragmentShader);
            if (message.Length > 0)
            {
                throw new InvalidOperationException($"Shader Error: {message}");
            }

            shaderProgram = gl.CreateProgram();
            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.LinkProgram(shaderProgram);
            gl.UseProgram(shaderProgram);
        }

        protected async Task InitializeShadersFromAssetsAsync(string vertexShaderPath, string fragmentShaderPath)
        {
            var baseAddress = WasmResourceLoader.GetLocalAddress();

            var vertexShaderStream = await WasmResourceLoader.LoadAsync(vertexShaderPath, baseAddress);
            string vertexShaderCode;

            using (var reader = new StreamReader(vertexShaderStream))
            {
                vertexShaderCode = reader.ReadToEnd();
            }

            var fragmentShaderStream = await WasmResourceLoader.LoadAsync(fragmentShaderPath, baseAddress);
            string fragmentShaderCode;

            using (var reader = new StreamReader(fragmentShaderStream))
            {
                fragmentShaderCode = reader.ReadToEnd();
            }

            InitializeShaders(vertexShaderCode, fragmentShaderCode);
        }
    }
}
