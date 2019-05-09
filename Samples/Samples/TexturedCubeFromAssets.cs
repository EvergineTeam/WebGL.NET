using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SkiaSharp;
using WebGLDotNET;

namespace Samples
{
    public class TexturedCubeFromAssets : BaseTexturedCube
    {
        const string AssetPath = "Assets/PlainConcepts.png";

        public override string Description =>
            $"Texture comes from a HttpClient retrieving <a href=\"{AssetPath}\">it</a> and them we load colors through " +
            "[Uno.SkiaSharp] SKBitmap.Decode.";

        private TaskCompletionSource<bool> canvasKitTcs = new TaskCompletionSource<bool>();

        protected override async void LoadImage()
        {
            var image = await GetImageFromAssetsAsync(AssetPath);

            if (image == null)
            {
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var imageData2 = GetUnsafeImageDataFromRGBAColors(image);
            stopwatch.Stop();

            Console.WriteLine($"GetUnsafeRGBAColors elapsed: {stopwatch.Elapsed}");

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
                imageData2);

            textureLoaded = true;
        }

        private async Task<SKBitmap> GetImageFromAssetsAsync(string path)
        {
            var content = await WasmResourceLoader.LoadAsync(path, WasmResourceLoader.GetLocalAddress());

            await canvasKitTcs.Task;

            var stopwatch = Stopwatch.StartNew();

            // TODO: Uno.SkiaSharp unable to handle jpg format
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

        private unsafe ImageData GetUnsafeImageDataFromRGBAColors(SKBitmap img)
        {
            var numBytes = img.Width * img.Height * 4;

            var pointer = img.GetPixels().ToPointer();

            var span = new ReadOnlySpan<byte>(pointer, numBytes);

            var imageData = new ImageData(span, img.Width, img.Height);

            return imageData;
        }
    }
}
