using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SkiaSharp;
using WebGLDotNET;

namespace Samples
{
    public class TexturedCubeFromAssets : BaseTexturedCube
    {
        const string AssetPath = "Assets/PlainConcepts.bmp";

        public override string Description =>
            $"Texture comes a HttpClient retrieving <a href=\"{AssetPath}\">it</a> and load through " +
            "[Uno.SkiaSharp] SKBitmap.Decode which returns its colors array.";

        protected override async void LoadImage()
        {
            var image = await GetImageFromAssetsAsync(AssetPath);

            if (image == null)
            {
                return;
            }

            var colors = GetRGBAColors(image);

            var imageData = new ImageData(colors, image.Width, image.Height);

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
        }

        private async Task<SKBitmap> GetImageFromAssetsAsync(string path)
        {
            await LoadCanvasKitAsync();

            var content = await WasmResourceLoader.LoadAsync(path, WasmResourceLoader.GetLocalAddress());

            var stopwatch = Stopwatch.StartNew();

            // png and bmp now working in Chrome and Firefox :D 
            // jpg fails
            var image = SKBitmap.Decode(content);

            stopwatch.Stop();

#if DEBUG
            Console.WriteLine($"Image load elapsed: {stopwatch.Elapsed}");
#endif

            return image;
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

        private async Task LoadCanvasKitAsync()
        {
            while (WebAssemblyRuntime.InvokeJS($"typeof CanvasKit !== 'undefined'") != "true")
            {
                Console.WriteLine("Waiting for Skia init");
                await Task.Delay(500);
            }
        }
    }
}
