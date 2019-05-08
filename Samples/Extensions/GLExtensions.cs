using System;
using System.IO;
using System.Threading.Tasks;
using WebGLDotNET;

namespace Samples
{
    public static class GLExtensions
    {
        public static WebGLBuffer CreateArrayBufferWithUsage(this WebGLRenderingContextBase gl, Array items, uint usage)
        {
            var arrayBuffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, arrayBuffer);
            gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, items, usage);

            return arrayBuffer;
        }

        public static WebGLBuffer CreateArrayBuffer(this WebGLRenderingContextBase gl, Array items)
        {
            var arrayBuffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, arrayBuffer);
            gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, items, WebGLRenderingContextBase.STATIC_DRAW);
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, null);

            return arrayBuffer;
        }

        public static WebGLBuffer CreateElementArrayBuffer(this WebGLRenderingContextBase gl, Array items)
        {
            var elementArrayBuffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, elementArrayBuffer);
            gl.BufferData(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, items, WebGLRenderingContextBase.STATIC_DRAW);
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, null);

            return elementArrayBuffer;
        }

        public static WebGLProgram InitializeShaders(this WebGLRenderingContextBase gl, string vertexShaderCode, string fragmentShaderCode)
        {
            var shaderProgram = gl.CreateProgram();

            var vertexShader = GetShader(gl, vertexShaderCode, WebGLRenderingContextBase.VERTEX_SHADER);
            var fragmentShader = GetShader(gl, fragmentShaderCode, WebGLRenderingContextBase.FRAGMENT_SHADER);

            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);

            gl.LinkProgram(shaderProgram);

            gl.UseProgram(shaderProgram);

            return shaderProgram;
        }

        public static WebGLShader GetShader(this WebGLRenderingContextBase gl, string shaderSource, uint type)
        {
            var shader = gl.CreateShader(type);
            gl.ShaderSource(shader, shaderSource);
            gl.CompileShader(shader);

            var message = gl.GetShaderInfoLog(shader);
            if (message.Length > 0)
            {
                var msg = $"Shader Error: {message}";
                throw new Exception(msg);
            }

            return shader;
        }
    }
}