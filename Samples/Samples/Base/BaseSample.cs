using System.Drawing;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public abstract class BaseSample : ISample
    {
        Color clearColor;

        protected WebGLRenderingContextBase gl;

        public virtual string Description => string.Empty;

        public virtual void Draw()
        {
            gl.Enable(WebGLRenderingContextBase.DEPTH_TEST);

            gl.ClearColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A);
            gl.Clear(WebGLRenderingContextBase.COLOR_BUFFER_BIT);
        }

        public virtual void Run(JSObject canvas, float canvasWidth, float canvasHeight, Color clearColor)
        {
            gl = new WebGL2RenderingContext(canvas);
            this.clearColor = clearColor;
        }

        public virtual void Update(double elapsedMilliseconds)
        {
        }
    }
}
