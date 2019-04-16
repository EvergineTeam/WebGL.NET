using System;
using System.Linq;
using WebAssembly;
using WebAssembly.Core;

namespace WebGLDotNET
{
    public abstract class JSHandler
    {
        internal JSObject Handle { get; set; }
    }

    public partial class WebGLActiveInfo : JSHandler
    {
    }

    public partial class WebGLContextAttributes : JSHandler
    {
    }

    public partial class WebGLObject : JSHandler, IDisposable
    {
        // to detect redundant calls
        public bool IsDisposed { get; internal set; }

        ~WebGLObject()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {

            if (!IsDisposed)
            {
                if (disposing)
                {

                    // Free any other managed objects here.
                    //
                }

                IsDisposed = true;

                // Free unmanaged objects here.
                Handle.Dispose();

            }
        }
    }

    public partial class WebGLRenderingContext : WebGLRenderingContextBase
    {
        public WebGLRenderingContext(JSObject canvas) : base(canvas, "webgl")
        {
        }
    }

    public abstract partial class WebGLRenderingContextBase
    {
        protected readonly JSObject gl;

        public WebGLRenderingContextBase(JSObject canvas, string contextType)
        {
            gl = (JSObject)canvas.Invoke("getContext", contextType);
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

        private void DisposeTypedArrays(object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg is ITypedArray typedArray && typedArray != null)
                {
                    var disposable = (IDisposable)typedArray;
                    disposable.Dispose();
                }
            }
        }

        protected object Invoke(string method, params object[] args)
        {
            var actualArgs = Translate(args);
            var result = gl.Invoke(method, actualArgs);
            DisposeTypedArrays(actualArgs);

            return result;
        }

        protected T Invoke<T>(string method, params object[] args)
            where T : JSHandler, new()
        {
            var actualArgs = Translate(args);
            var rawResult = gl.Invoke(method, actualArgs);
            DisposeTypedArrays(actualArgs);

            //Console.WriteLine($"{nameof(Invoke)}<{typeof(T)}>(): {rawResult}");

            var result = new T();
            result.Handle = (JSObject)rawResult;

            return result;
        }

        protected T[] InvokeForArray<T>(string method, params object[] args) =>
            ((object[])gl.Invoke(method, args))
                .Cast<T>()
                .ToArray();

        protected T InvokeForBasicType<T>(string method, params object[] args)
            where T : IConvertible =>
            (T)Invoke(method, args);

        private object[] Translate(object[] args)
        {
            var actualArgs = new object[args.Length];

            for (int i = 0; i < actualArgs.Length; i++)
            {
                var arg = args[i];

                if (arg == null)
                {
                    actualArgs[i] = null;
                    continue;
                }

                if (arg is JSHandler jsHandler)
                {
                    arg = jsHandler.Handle;
                }
                else if (arg is System.Array array)
                {
                    arg = CastNativeArray(array);
                }

                actualArgs[i] = arg;

                //Console.WriteLine($"{args[i].GetType()} vs. {arg.GetType()}");
            }

            return actualArgs;
        }
    }

    public partial class WebGLShaderPrecisionFormat : JSHandler
    {
    }

    public partial class WebGLUniformLocation : JSHandler, IDisposable
    {
        // to detect redundant calls
        public bool IsDisposed { get; internal set; }

        ~WebGLUniformLocation()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {

            if (!IsDisposed)
            {
                if (disposing)
                {

                    // Free any other managed objects here.
                    //
                }

                IsDisposed = true;

                // Free unmanaged objects here.
                // 
                Handle.Dispose();

            }
        }

    }

    public partial class WebGL2RenderingContext : WebGL2RenderingContextBase
    {
        public WebGL2RenderingContext(JSObject canvas) : base(canvas, "webgl2")
        { 
        }
    }

    public abstract partial class WebGL2RenderingContextBase : WebGLRenderingContextBase
    {
        public WebGL2RenderingContextBase(JSObject canvas, string contextType) : base(canvas, contextType)
        {
        }
    }
}
