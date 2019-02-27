using System;
using WebAssembly;

namespace WebGLDotNET
{
    public static class WebGL
    {
        private static JSObject gl;

        public static JSObject CastNativeArray(object managedArray)
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

        public static void RequestAnimationFrame(string loopMemberName, Type callerType)
        {
            var animationBootstrap = 
                "var animate = function(time) {\n" +
                $"    BINDING.call_static_method('[{callerType.Namespace}] {callerType.FullName}:{loopMemberName}', " +
                    "[time]);\n" +
                "    window.requestAnimationFrame(animate);\n" +
                "}\n" +
                "animate(0);";
            Runtime.InvokeJS(animationBootstrap);
        }

        public static object ArrayBuffer => GetProperty("ARRAY_BUFFER");

        public static object ClampToEdge => GetProperty("CLAMP_TO_EDGE");

        public static object ColorBufferBit => GetProperty("COLOR_BUFFER_BIT");

        public static object DepthBufferBit => GetProperty("DEPTH_BUFFER_BIT");

        public static object DepthTest => GetProperty("DEPTH_TEST");

        public static object ElementArrayBuffer => GetProperty("ELEMENT_ARRAY_BUFFER");

        public static object Float => GetProperty("FLOAT");

        public static object FragmentShader => GetProperty("FRAGMENT_SHADER");

        public static object LEqual => GetProperty("LEQUAL");

        public static object Nearest => GetProperty("NEAREST");

        public static object RGB => GetProperty("RGB");

        public static object RGBA => GetProperty("RGBA");

        public static object StaticDraw => GetProperty("STATIC_DRAW");

        public static object Texture2D => GetProperty("TEXTURE_2D");

        public static object TextureMagFilter => GetProperty("TEXTURE_MAG_FILTER");

        public static object TextureMinFilter => GetProperty("TEXTURE_MIN_FILTER");

        public static object TextureWrapS => GetProperty("TEXTURE_WRAP_S");

        public static object TextureWrapT => GetProperty("TEXTURE_WRAP_T");

        public static object Triangles => GetProperty("TRIANGLES");

        public static object UnsignedByte => GetProperty("UNSIGNED_BYTE");

        public static object UnsignedShort => GetProperty("UNSIGNED_SHORT");

        public static object VertexShader => GetProperty("VERTEX_SHADER");

        public static void AttachShader(object program, object shader) => Invoke("attachShader", program, shader);

        public static void BindBuffer(object target, object buffer) => Invoke("bindBuffer", target, buffer);

        public static void BindTexture(object target, object texture) => Invoke("bindTexture", target, texture);

        public static void BufferData(object target, object srcData, object usage)
        {
            var array = CastNativeArray(srcData);
            Invoke("bufferData", target, array, usage);
        }

        public static void Clear(object mask) => Invoke("clear", mask);

        public static void ClearColor(double red, double green, double blue, double alpha) =>
            Invoke("clearColor", red, green, blue, alpha);

        public static void ClearDepth(double depth) => Invoke("clearDepth", depth);

        public static void CompileShader(object shader) => Invoke("compileShader", shader);

        public static object CreateBuffer() => Invoke("createBuffer");

        public static object CreateProgram() => Invoke("createProgram");

        public static object CreateShader(object type) => Invoke("createShader", type);

        public static object CreateTexture() => Invoke("createTexture");

        public static void DepthFunc(object func) => Invoke("depthFunc", func);

        public static void DrawArrays(object mode, object first, object count) =>
            Invoke("drawArrays", mode, first, count);

        public static void DrawElements(object mode, int count, object type, int offset) =>
            Invoke("drawElements", mode, count, type, offset);

        public static void Enable(object cap) => Invoke("enable", cap);

        public static void EnableVertexAttribArray(object index) => Invoke("enableVertexAttribArray", index);

        public static object GetAttribLocation(object program, string name) => 
            Invoke("getAttribLocation", program, name);

        public static object GetUniformLocation(object program, string name) =>
            Invoke("getUniformLocation", program, name);

        public static void Init(JSObject canvas)
        {
            gl = (JSObject)canvas.Invoke("getContext", "webgl");
        }

        public static void LinkProgram(object program) => Invoke("linkProgram", program);

        public static void ShaderSource(object shader, string source) => Invoke("shaderSource", shader, source);

        public static void TexImage2D(
            object target,
            object level,
            object internalformat,
            object format,
            object type,
            object pixels) =>
            Invoke("texImage2D", target, level, internalformat, format, type, pixels);

        public static void TexParameteri(object target, object pname, object param) =>
            Invoke("texParameteri", target, pname, param);

        public static void Uniform2f(object location, object v0, object v1) =>
            Invoke("uniform2f", location, v0, v1);

        public static void UniformMatrix4fv(object location, bool transpose, float[] value) =>
            Invoke("uniformMatrix4fv", location, transpose, value);

        public static void UseProgram(object program) => Invoke("useProgram", program);

        public static void VertexAttribPointer(
            object index, 
            int size, 
            object type, 
            bool normalized, 
            int stride, 
            int offset) =>
            Invoke("vertexAttribPointer", index, size, type, normalized, stride, offset);

        public static void Viewport(int x, int y, object width, object height) => 
            Invoke("viewport", x, y, width, height);

        private static object GetProperty(string name) => gl.GetObjectProperty(name);

        private static object Invoke(string method, params object[] args) => gl.Invoke(method, args);
    }
}
