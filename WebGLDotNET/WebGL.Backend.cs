using System;
using WebAssembly;
using WebAssembly.Core;

namespace WebGLDotNET
{
    public partial class WebGLRenderingContextBase
    {
        private JSObject gl;

        private WebGLRenderingContextBase(JSObject canvas)
        {
            gl = (JSObject)canvas.Invoke("getContext", "webgl");
        }

        public static WebGLRenderingContextBase GetContext(JSObject canvas)
        {
            var instance = new WebGLRenderingContextBase(canvas);

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

        private void Invoke(string method, params object[] args) => Invoke<object>(method, args);

        private T Invoke<T>(string method, params object[] args) => (T)gl.Invoke(method, args);
    }
}
