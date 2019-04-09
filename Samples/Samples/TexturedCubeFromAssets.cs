using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WebGLDotNET;

namespace Samples
{
    public class TexturedCubeFromAssets : BaseTexturedCube
    {
        const string AssetPath = "Assets/PlainConcepts.bmp";

        public override string Description =>
            $"Texture comes a HttpClient retrieving <a href=\"{AssetPath}\">it</a> and load through " +
            "SixLabors' ImageSharp which returns its colors array.";

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

        private async Task<Image<Rgba32>> GetImageFromAssetsAsync(string path)
        {
            var content = await WasmResourceLoader.LoadAsync(path, WasmResourceLoader.GetLocalAddress());

            var stopwatch = Stopwatch.StartNew();
            var image = Image.Load(content);
            stopwatch.Stop();

#if DEBUG
            Console.WriteLine($"Image.Load() elapsed: {stopwatch.Elapsed}");
#endif

            return image;
        }

        private byte[] GetRGBAColors(Image<Rgba32> img)
        {
            var numBytes = img.Width * img.Height * 4;
            var colors = new byte[numBytes];
            var colorIndex = 0;

            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    var pixel = img[i, j];

                    colors[colorIndex++] = pixel.R;
                    colors[colorIndex++] = pixel.G;
                    colors[colorIndex++] = pixel.B;
                    colors[colorIndex++] = pixel.A;
                }
            }

            return colors;
        }
    }
}
