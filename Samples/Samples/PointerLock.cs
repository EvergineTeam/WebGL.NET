using System;
using WaveEngine.Common.Math;
using WebAssembly;

namespace Samples
{
    public class PointerLock : ISample
    {
        private JSObject ctx;

        private float x = 50;
        private float y = 50;
        private JSObject currentCanvas;
        private float cWidth;
        private float cHeight;

        private const float RADIUS = 20;

        public double degToRad(float degrees)
        {
            var result = Math.PI / 180 * degrees;
            return result;
        }

        public string Description => "Pointer lock demo. See the <a href=\"https://mdn.github.io/dom-examples/pointer-lock/\"> original sample </a>";

        public bool LazyLoad { get; set; } = false;

        public void Init(JSObject canvas, int canvasWidth, int canvasHeight, Vector4 clearColor)
        {
            currentCanvas = canvas;

            cWidth = canvasWidth;
            cHeight = canvasHeight;
        }

        public void Run()
        { 
            ctx = (JSObject)currentCanvas.Invoke("getContext", "2d");

            var canvasClick = new Action<JSObject>((o) => {
                using (var document = (JSObject)Runtime.GetGlobalObject("document"))
                {
                    var canvasObject = (JSObject)document.Invoke("getElementById", this.GetType().Name);

                    var supportPointerLock = canvasObject.GetObjectProperty("requestPointerLock");
                    if (supportPointerLock != null)
                    {
                        canvasObject.Invoke("requestPointerLock");
                    }
                    
                    var supportMozRequestPointerLock = canvasObject.GetObjectProperty("mozRequestPointerLock");
                    if(supportMozRequestPointerLock != null)
                    {
                        canvasObject.Invoke("mozRequestPointerLock");
                    }

                }

                o.Dispose();
            });

            currentCanvas.Invoke("addEventListener", "click", canvasClick, false);

            CanvasDraw();

            var updatePosition = new Action<JSObject>((mEvent) => {
                var mX = (int)mEvent.GetObjectProperty("movementX");
                var mY = (int)mEvent.GetObjectProperty("movementY");

                x += mX;
                y += mY;

                if (x > cWidth + RADIUS)
                {
                    x = -RADIUS;
                }
                if (y > cHeight + RADIUS)
                {
                    y = -RADIUS;
                }
                if (x < -RADIUS)
                {
                    x = cWidth + RADIUS;
                }
                if (y < -RADIUS)
                {
                    y = cHeight + RADIUS;
                }

                mEvent.Dispose();

            });

            var lockChangeAlert = new Action<JSObject>((o) => {

                Console.WriteLine("lockChangeAlert");
                using (var document = (JSObject)Runtime.GetGlobalObject("document"))
                {
                    var canvasObject = (JSObject)document.Invoke("getElementById", this.GetType().Name);

                    var lockElement = (JSObject)document.GetObjectProperty("pointerLockElement");
                    var mozLockElement = (JSObject)document.GetObjectProperty("mozPointerLockElement");

                    // TODO: don't know how to check this
                    var enter = (lockElement != null) || (mozLockElement != null); 
                    if (enter)
                    {
                        document.Invoke("addEventListener", "mousemove", updatePosition, false);
                    }
                    else
                    {
                        document.Invoke("removeEventListener", "mousemove", updatePosition, false);
                    }
                }

                o.Dispose();

            });
            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            {
                document.Invoke("addEventListener", "pointerlockchange", lockChangeAlert, false);
                document.Invoke("addEventListener", "mozpointerlockchange", lockChangeAlert, false);
            }
        }

        private void CanvasDraw()
        {
            ctx.SetObjectProperty("fillStyle", "black");
            ctx.Invoke("fillRect", 0, 0, cWidth, cHeight);

            ctx.SetObjectProperty("fillStyle", "#f00");
            ctx.Invoke("beginPath");

            ctx.Invoke("arc", x, y, RADIUS, 0, degToRad(360), true);

            ctx.Invoke("fill");
        }

        public void Update(double elapsedTime)
        {
        }

        public void Draw()
        {
            CanvasDraw();
        }
    }
}
