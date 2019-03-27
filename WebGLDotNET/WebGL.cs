/*
 * WebGL 1.x specs.: https://github.com/KhronosGroup/WebGL/blob/master/specs/1.0.3/webgl.idl
 * 
 * Random thoughts:
 * - There are too many "object", we could go forward by using int for GLint and similar, with the goal of providing 
 *   better types while using the API
 */

using System;
using WebAssembly;

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

        public JSObject CastNativeArray(object managedArray)
        {
            var arrayType = managedArray.GetType();
            JSObject array;

            // Here are listed some JavaScript array types:
            // https://github.com/mono/mono/blob/a7f5952c69ae76015ccaefd4dfa8be2274498a21/sdks/wasm/bindings-test.cs
            if (arrayType == typeof(byte[]))
            {
                var uint8Array = (JSObject)Runtime.GetGlobalObject("Uint8Array");
                array = Runtime.NewJSObject(uint8Array, managedArray);
            }
            else if (arrayType == typeof(float[]))
            {
                var float32Array = (JSObject)Runtime.GetGlobalObject("Float32Array");
                array = Runtime.NewJSObject(float32Array, managedArray);
            }
            else if (arrayType == typeof(ushort[]))
            {
                var uint16Array = (JSObject)Runtime.GetGlobalObject("Uint16Array");
                array = Runtime.NewJSObject(uint16Array, managedArray);
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

        public void AttachShader(object program, object shader) => Invoke("attachShader", program, shader);

        public void BindBuffer(object target, object buffer) => Invoke("bindBuffer", target, buffer);

        public void BindTexture(object target, object texture) => Invoke("bindTexture", target, texture);

        public void BufferData(object target, object srcData, object usage)
        {
            var array = CastNativeArray(srcData);
            Invoke("bufferData", target, array, usage);
        }

        public void Clear(object mask) => Invoke("clear", mask);

        public void ClearColor(double red, double green, double blue, double alpha) =>
            Invoke("clearColor", red, green, blue, alpha);

        public void ClearDepth(double depth) => Invoke("clearDepth", depth);

        public void CompileShader(object shader) => Invoke("compileShader", shader);

        public object CreateBuffer() => Invoke("createBuffer");

        public object CreateProgram() => Invoke("createProgram");

        public object CreateShader(object type) => Invoke("createShader", type);

        public object CreateTexture() => Invoke("createTexture");

        public void DepthFunc(object func) => Invoke("depthFunc", func);

        public void DrawArrays(object mode, object first, object count) =>
            Invoke("drawArrays", mode, first, count);

        public void DrawElements(object mode, int count, object type, int offset) =>
            Invoke("drawElements", mode, count, type, offset);

        public void Enable(object cap) => Invoke("enable", cap);

        public void EnableVertexAttribArray(object index) => Invoke("enableVertexAttribArray", index);

        public void GenerateMipmap(object target) => Invoke("generateMipmap", target);

        public object GetAttribLocation(object program, string name) => 
            Invoke("getAttribLocation", program, name);

        public object GetUniformLocation(object program, string name) =>
            Invoke("getUniformLocation", program, name);

        public void LinkProgram(object program) => Invoke("linkProgram", program);

        public void ShaderSource(object shader, string source) => Invoke("shaderSource", shader, source);

        public void TexImage2D(
            object target,
            object level,
            object internalformat,
            object format,
            object type,
            ImageData pixels) =>
            Invoke("texImage2D", target, level, internalformat, format, type, pixels.Handle);

        public void TexParameteri(object target, object pname, object param) =>
            Invoke("texParameteri", target, pname, param);

        public void Uniform1i(object location, object x) => Invoke("uniform1i", location, x);

        public void Uniform2f(object location, object v0, object v1) =>
            Invoke("uniform2f", location, v0, v1);

        public void UniformMatrix4fv(object location, bool transpose, float[] value) =>
            Invoke("uniformMatrix4fv", location, transpose, value);

        public void UseProgram(object program) => Invoke("useProgram", program);

        public void VertexAttribPointer(
            object index, 
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
