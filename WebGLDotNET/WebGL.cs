/*
 * WebGL 1.x specs.: https://github.com/KhronosGroup/WebGL/blob/master/specs/1.0.3/webgl.idl
 * 
 * Random thoughts:
 * - There are too many "object", we could go forward by using int for GLint and similar, with the goal of providing 
 *   better types while using the API
 */

using System;
using WebAssembly;
using WebAssembly.Core;

namespace WebGLDotNET
{
    public class WebGL
    {
        private JSObject gl;

        private WebGL(JSObject canvas)
        {
            gl = (JSObject)canvas.Invoke("getContext", "webgl");
        }

        public static WebGL GetContext(JSObject canvas)
        {
            var instance = new WebGL(canvas);

            return instance;
        }

        public ITypedArray CastNativeArray(object managedArray)
        {
            var arrayType = managedArray.GetType();
            ITypedArray array;

            // Here are listed some JavaScript array types:
            // https://github.com/mono/mono/blob/a7f5952c69ae76015ccaefd4dfa8be2274498a21/sdks/wasm/bindings-test.cs
            if (arrayType == typeof(byte[]))
            {
                array = Uint8Array.From((byte[])managedArray);
            }
            else if (arrayType == typeof(float[]))
            {
                array = Float32Array.From((float[])managedArray);
            }
            else if (arrayType == typeof(ushort[]))
            {
                array = Uint16Array.From((ushort[])managedArray);
            }
            else
            {
                throw new NotImplementedException();
            }

            return array;
        }

        public object ArrayBuffer => GetProperty("ARRAY_BUFFER");

        public object ClampToEdge => GetProperty("CLAMP_TO_EDGE");

        public object ColorBufferBit => GetProperty("COLOR_BUFFER_BIT");

        public object DepthBufferBit => GetProperty("DEPTH_BUFFER_BIT");

        public object DepthTest => GetProperty("DEPTH_TEST");

        public object ElementArrayBuffer => GetProperty("ELEMENT_ARRAY_BUFFER");

        public object Float => GetProperty("FLOAT");

        public object FragmentShader => GetProperty("FRAGMENT_SHADER");

        public object LEqual => GetProperty("LEQUAL");

        public object Nearest => GetProperty("NEAREST");

        public object RGB => GetProperty("RGB");

        public object RGBA => GetProperty("RGBA");

        public object StaticDraw => GetProperty("STATIC_DRAW");

        public object Texture0 => GetProperty("TEXTURE0");

        public object Texture2D => GetProperty("TEXTURE_2D");

        public object TextureMagFilter => GetProperty("TEXTURE_MAG_FILTER");

        public object TextureMinFilter => GetProperty("TEXTURE_MIN_FILTER");

        public object TextureWrapS => GetProperty("TEXTURE_WRAP_S");

        public object TextureWrapT => GetProperty("TEXTURE_WRAP_T");

        public object Triangles => GetProperty("TRIANGLES");

        public object UnsignedByte => GetProperty("UNSIGNED_BYTE");

        public object UnsignedShort => GetProperty("UNSIGNED_SHORT");

        public object VertexShader => GetProperty("VERTEX_SHADER");

        public void ActiveTexture(object texture) => Invoke("activeTexture", texture);

        public void AttachShader(WebGLProgram program, WebGLShader shader) => 
            Invoke("attachShader", program?.Handle, shader?.Handle);

        public void BindBuffer(object target, WebGLBuffer buffer) => Invoke("bindBuffer", target, buffer?.Handle);

        public void BindTexture(object target, WebGLTexture texture) => Invoke("bindTexture", target, texture?.Handle);

        public void BufferData(object target, object srcData, object usage)
        {
            var array = CastNativeArray(srcData);
            Invoke("bufferData", target, array, usage);
        }

        public void Clear(object mask) => Invoke("clear", mask);

        public void ClearColor(double red, double green, double blue, double alpha) =>
            Invoke("clearColor", red, green, blue, alpha);

        public void ClearDepth(double depth) => Invoke("clearDepth", depth);

        public void CompileShader(WebGLShader shader) => Invoke("compileShader", shader?.Handle);

        public WebGLBuffer CreateBuffer()
        {
            var jsObject = (JSObject)Invoke("createBuffer");
            var wrapper = new WebGLBuffer(jsObject);

            return wrapper;
        }

        public WebGLProgram CreateProgram()
        {
            var jsObject = (JSObject)Invoke("createProgram");
            var wrapper = new WebGLProgram(jsObject);

            return wrapper;
        }

        public WebGLShader CreateShader(object type)
        {
            var jsObject = (JSObject)Invoke("createShader", type);
            var wrapper = new WebGLShader(jsObject);

            return wrapper;
        }

        public WebGLTexture CreateTexture()
        {
            var jsObject = (JSObject)Invoke("createTexture");
            var wrapper = new WebGLTexture(jsObject);

            return wrapper;
        }

        public void DepthFunc(object func) => Invoke("depthFunc", func);

        public void DrawArrays(object mode, object first, object count) =>
            Invoke("drawArrays", mode, first, count);

        public void DrawElements(object mode, int count, object type, int offset) =>
            Invoke("drawElements", mode, count, type, offset);

        public void Enable(object cap) => Invoke("enable", cap);

        public void EnableVertexAttribArray(int index) => Invoke("enableVertexAttribArray", index);

        public void GenerateMipmap(object target) => Invoke("generateMipmap", target);

        public int GetAttribLocation(WebGLProgram program, string name) => 
            (int)Invoke("getAttribLocation", program?.Handle, name);

        public WebGLUniformLocation GetUniformLocation(WebGLProgram program, string name)
        {
            var jsObject = (JSObject)Invoke("getUniformLocation", program?.Handle, name);
            var wrapper = new WebGLUniformLocation(jsObject);

            return wrapper;
        }

        public void LinkProgram(WebGLProgram program) => Invoke("linkProgram", program?.Handle);

        public void ShaderSource(WebGLShader shader, string source) => Invoke("shaderSource", shader?.Handle, source);

        public void TexImage2D(
            object target,
            object level,
            object internalformat,
            object format,
            object type,
            ImageData pixels) =>
            Invoke("texImage2D", target, level, internalformat, format, type, pixels?.Handle);

        // This overload serves for pixels being HTMLImageElement
        public void TexImage2D(
            object target,
            object level,
            object internalformat,
            object format,
            object type,
            JSObject pixels) =>
            Invoke("texImage2D", target, level, internalformat, format, type, pixels);

        public void TexParameteri(object target, object pname, object param) =>
            Invoke("texParameteri", target, pname, param);

        public void Uniform1i(WebGLUniformLocation location, object x) => Invoke("uniform1i", location?.Handle, x);

        public void Uniform2f(WebGLUniformLocation location, object v0, object v1) =>
            Invoke("uniform2f", location?.Handle, v0, v1);

        public void UniformMatrix4fv(WebGLUniformLocation location, bool transpose, float[] value)
        {
            var array = CastNativeArray(value);
            Invoke("uniformMatrix4fv", location?.Handle, transpose, array);
        }

        public void UseProgram(WebGLProgram program) => Invoke("useProgram", program?.Handle);

        public void VertexAttribPointer(
            int index, 
            int size, 
            object type, 
            bool normalized, 
            int stride, 
            int offset) =>
            Invoke("vertexAttribPointer", index, size, type, normalized, stride, offset);

        public void Viewport(int x, int y, object width, object height) => 
            Invoke("viewport", x, y, width, height);

        private object GetProperty(string name) => gl.GetObjectProperty(name);

        private object Invoke(string method, params object[] args) => gl.Invoke(method, args);
    }
}
