using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SkiaSharp;
using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    public class TexturedCubeFromAssets : BaseTexturedCube
    {
        const string AssetPath = "Assets/PlainConcepts.bmp";

        public override string Description =>
            $"Texture comes from a HttpClient retrieving <a href=\"{AssetPath}\">it</a> and them we load colors through " +
            "[Uno.SkiaSharp] SKBitmap.Decode.";

        public override bool LazyLoad => true;

        private TaskCompletionSource<bool> canvasKitTcs = new TaskCompletionSource<bool>();

        public override void Init(JSObject canvas, int canvasWidth, int canvasHeight, Vector4 clearColor)
        {
            base.Init(canvas, canvasWidth, canvasHeight, clearColor);

            attachButtonEvent();
        }

        protected override async void LoadImage()
        {
            var image = await GetImageFromAssetsAsync(AssetPath);

            if (image == null)
            {
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var colors = GetRGBAColors(image);
            stopwatch.Stop();

            // TODO: :S
            Console.WriteLine($"GetRGBAColors elapsed: {stopwatch.Elapsed}");

            stopwatch = Stopwatch.StartNew();
            var imageData = new ImageData(colors, image.Width, image.Height);
            stopwatch.Stop();

            Console.WriteLine($"ImageData ctor elapsed: {stopwatch.Elapsed}");

            gl.BindTexture(WebGLRenderingContextBase.TEXTURE_2D, texture);

            gl.TexParameteri(
                WebGLRenderingContextBase.TEXTURE_2D,
                WebGLRenderingContextBase.TEXTURE_WRAP_S,
                (int)WebGLRenderingContextBase.CLAMP_TO_EDGE);
            gl.TexParameteri(
                WebGLRenderingContextBase.TEXTURE_2D,
                WebGLRenderingContextBase.TEXTURE_WRAP_T,
                (int)WebGLRenderingContextBase.CLAMP_TO_EDGE);
            gl.TexParameteri(
                WebGLRenderingContextBase.TEXTURE_2D,
                WebGLRenderingContextBase.TEXTURE_MIN_FILTER,
                (int)WebGLRenderingContextBase.NEAREST);
            gl.TexParameteri(
                WebGLRenderingContextBase.TEXTURE_2D,
                WebGLRenderingContextBase.TEXTURE_MAG_FILTER,
                (int)WebGLRenderingContextBase.NEAREST);

            gl.TexImage2D(
                WebGLRenderingContextBase.TEXTURE_2D,
                0,
                WebGLRenderingContextBase.RGB,
                WebGLRenderingContextBase.RGB,
                WebGLRenderingContextBase.UNSIGNED_BYTE,
                imageData);

            textureLoaded = true;
        }

        private async Task<SKBitmap> GetImageFromAssetsAsync(string path)
        {
            var content = await WasmResourceLoader.LoadAsync(path, WasmResourceLoader.GetLocalAddress());

            await canvasKitTcs.Task;

            var stopwatch = Stopwatch.StartNew();

            // TODO: check jpg
            var image = SKBitmap.Decode(content);

            stopwatch.Stop();

            Console.WriteLine($"Image load elapsed: {stopwatch.Elapsed}");

            return image;
        }

        private void CheckCanvasKitReady()
        {
            if (canvasKitTcs.Task.IsCompleted)
            {
                return;
            }

            var ready = WebAssemblyRuntime.InvokeJS($"typeof CanvasKit !== 'undefined'") == "true";
            if (ready)
            {
                canvasKitTcs.TrySetResult(true);
            }
        }

        private void attachButtonEvent()
        {
            var name = $"load_{this.GetType().Name}";

            var onClick = new Action<JSObject>(clickEvent =>
            {
                Run();

                clickEvent.Dispose();
            });

            using (var document = (JSObject)Runtime.GetGlobalObject("document"))
            using (var button = (JSObject)document.Invoke("getElementById", name))
            {
                button.Invoke("addEventListener", "click", onClick, false);
            }
        }

        public override void Update(double elapsedTime)
        {
            CheckCanvasKitReady();

            base.Update(elapsedTime);
        }

        public override void Draw()
        {
            CheckCanvasKitReady();

            base.Draw();
        }

        private byte[] GetRGBAColors(SKBitmap img)
        {
            var numBytes = img.Width * img.Height * 4;
            var colors = new byte[numBytes];
            var colorIndex = 0;

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    var pixel = img.GetPixel(i, j);

                    colors[colorIndex++] = pixel.Red;
                    colors[colorIndex++] = pixel.Green;
                    colors[colorIndex++] = pixel.Blue;
                    colors[colorIndex++] = pixel.Alpha;
                }
            }

            return colors;
        }
    }
}
