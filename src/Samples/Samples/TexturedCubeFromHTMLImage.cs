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

        Action<JSObject> onLoad;

        protected override void LoadImage()
        {
            var image = new HostObject("Image");
            onLoad = new Action<JSObject>(loadEvent =>
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

                gl.TexImage2D(
                    WebGLRenderingContextBase.TEXTURE_2D,
                    0,
                    WebGLRenderingContextBase.RGB,
                    WebGLRenderingContextBase.RGB,
                    WebGLRenderingContextBase.UNSIGNED_BYTE,
                    image);

                loadEvent.Dispose();
                Runtime.FreeObject(onLoad);

                image.SetObjectProperty("onload", null);
                image.Dispose();
            });
            image.SetObjectProperty("onload", onLoad);
            image.SetObjectProperty("src", AssetPath);

            textureLoaded = true;
        }
    }
}
