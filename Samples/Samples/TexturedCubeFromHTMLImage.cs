using System;
using WebAssembly;
using WebAssembly.Host;
using WebGLDotNET;

namespace Samples
{
    public class TexturedCubeFromHTMLImage : BaseTexturedCube
    {
        const string AssetPath = "Assets/PlainConcepts.png";

        public override string Description =>
            $"Texture comes from HTML's Image, whose src property points <a href=\"{AssetPath}\">here</a>.";

        protected override void LoadImage()
        {
            var image = new HostObject("Image");
            var onLoad = new Action<JSObject>(jsObject =>
            {
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

                var imageData = new ImageData(Image.ARGBColors, Image.Width, Image.Height);
                gl.TexImage2D(
                    WebGLRenderingContextBase.TEXTURE_2D,
                    0,
                    WebGLRenderingContextBase.RGB,
                    WebGLRenderingContextBase.RGB,
                    WebGLRenderingContextBase.UNSIGNED_BYTE,
                    image);
            });
            image.SetObjectProperty("onload", onLoad);
            image.SetObjectProperty("src", AssetPath);
        }
    }
}
