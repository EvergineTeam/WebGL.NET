using System.Threading.Tasks;
using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public abstract class BaseSample : ISample
    {
        protected WebGLRenderingContextBase gl;
        protected Vector4 clearColor;
        protected JSObject canvas;
        protected int canvasWidth;
        protected int canvasHeight;

        public virtual string Description => string.Empty;

        public virtual bool EnableFullScreen => true;

        public virtual Task InitAsync(JSObject canvas, Vector4 clearColor)
        {
            this.clearColor = clearColor;
            this.canvas = canvas;

            canvasWidth = (int)canvas.GetObjectProperty("width");
            canvasHeight = (int)canvas.GetObjectProperty("height");

            var contextAttributes = new WebGLContextAttributes { Stencil = true };
            gl = new WebGL2RenderingContext(canvas, contextAttributes);

            return Task.CompletedTask;
        }

        public virtual void Run()
        {
        }

        public virtual void Update(double elapsedMilliseconds)
        {
        }

        public virtual void Draw()
        {
            gl.Enable(WebGLRenderingContextBase.DEPTH_TEST);

            gl.Viewport(0, 0, canvasWidth, canvasHeight);

            gl.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
            gl.Clear(WebGLRenderingContextBase.COLOR_BUFFER_BIT);
        }

        public virtual void Resize(int width, int height)
        {
            canvasWidth = width;
            canvasHeight = height;
        }
    }
}
