using System;
using WaveEngine.Common.Math;
using WebAssembly;

namespace Samples
{
    // https://www.html5rocks.com/en/tutorials/pointerlock/intro/
    public class PointerLock : ISample
    {
        private const float RADIUS = 20;

        private JSObject ctx;

        private float x = 50;
        private float y = 50;

        private JSObject currentCanvas;

        private int canvasWidth;
        private int canvasHeight;

        private bool listenToMouseEvent;

        public bool EnableFullScreen => true;

        public string Description => "Pointer lock demo. See the <a href=\"https://mdn.github.io/dom-examples/pointer-lock/\"> original sample </a>";

        public void Init(JSObject canvas, Vector4 clearColor)
        {
            currentCanvas = canvas;

            canvasWidth = (int)canvas.GetObjectProperty("width");
            canvasHeight = (int)canvas.GetObjectProperty("height");
        }

        public void Run()
        {
            ctx = (JSObject)currentCanvas.Invoke("getContext", "2d");

            currentCanvas.Invoke("addEventListener", "click", new Action<JSObject>((o) => {
                using (var document = (JSObject)Runtime.GetGlobalObject("document"))
                {
                    var canvasName = $"canvas_{this.GetType().Name}";
                    var canvasObject = (JSObject)document.Invoke("getElementById", canvasName);

                    var supportPointerLock = canvasObject.GetObjectProperty("requestPointerLock");
                    if (supportPointerLock != null)
                    {
                        canvasObject.Invoke("requestPointerLock");
                    }

                    var supportMozRequestPointerLock = canvasObject.GetObjectProperty("mozRequestPointerLock");
                    if (supportMozRequestPointerLock != null)
                    {
                        canvasObject.Invoke("mozRequestPointerLock");
                    }
                }

                o.Dispose();
            }), false);

            var lockChangeAlert = new Action<JSObject>((o) => {

                using (var document = (JSObject)Runtime.GetGlobalObject("document"))
                {
                    var lockElement = (JSObject)document.GetObjectProperty("pointerLockElement");
                    var mozLockElement = (JSObject)document.GetObjectProperty("mozPointerLockElement");

                    var enter = (lockElement != null) || (mozLockElement != null);
                    if (enter)
                    {
                        listenToMouseEvent = true;
                    }
                    else
                    {
                        listenToMouseEvent = false;
                    }
                }

                o.Dispose();
            });

            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            {
                document.Invoke("addEventListener", "pointerlockchange", lockChangeAlert, false);
                document.Invoke("addEventListener", "mozpointerlockchange", lockChangeAlert, false);

                document.Invoke("addEventListener", "mousemove", new Action<JSObject>((mEvent) => {

                    var mX = (int)mEvent.GetObjectProperty("movementX");
                    var mY = (int)mEvent.GetObjectProperty("movementY");

                    mEvent.Dispose();

                    if (!listenToMouseEvent)
                    {
                        return;
                    }

                    x += mX;
                    y += mY;

                    if (x > canvasWidth + RADIUS)
                    {
                        x = -RADIUS;
                    }
                    if (y > canvasHeight + RADIUS)
                    {
                        y = -RADIUS;
                    }
                    if (x < -RADIUS)
                    {
                        x = canvasWidth + RADIUS;
                    }
                    if (y < -RADIUS)
                    {
                        y = canvasHeight + RADIUS;
                    }

                }), false);
            }
        }

        private void CanvasDraw()
        {
            ctx.SetObjectProperty("fillStyle", "pink");
            ctx.Invoke("fillRect", 0, 0, canvasWidth, canvasHeight);

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

        public void Resize(int width, int height)
        {
            canvasWidth = width;
            canvasHeight = height;
        }

        private double degToRad(float degrees)
        {
            var result = Math.PI / 180 * degrees;
            return result;
        }
    }
}
