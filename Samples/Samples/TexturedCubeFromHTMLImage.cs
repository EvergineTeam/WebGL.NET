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

                var imageData = new ImageData(DemoImage.ARGBColors, DemoImage.Width, DemoImage.Height);
                gl.TexImage2D(
                    WebGLRenderingContextBase.TEXTURE_2D,
                    0,
                    WebGLRenderingContextBase.RGB,
                    WebGLRenderingContextBase.RGB,
                    WebGLRenderingContextBase.UNSIGNED_BYTE,
                    image);

                // Make sure we tell managed code and javascript that we can dispose of the
                // event object.
                loadEvent.Dispose();

                // The image is already bound and in this case is no longer needed.  This removes the element
                // from managed code as well as the javascript reference.

                // Unattach the event property.  Not really necassary.
                image.SetObjectProperty("onload", null);
                // The Action is really a bridge object in this case on the javascript side and will not
                // be garbage collected by itself because managed code and javascript do not know about each 
                // other so we need to help the GC out a little bit.  Basically we are telling javascript that
                // this bridge object is no longer needed by managed code so release it.  Hopefully in the future
                // this will be eliviated when webassembly and host objects implements GC.
                Runtime.FreeObject(onLoad);
                // and now mark the image as disposed so GC has less work.
                image.Dispose();

            });
            image.SetObjectProperty("onload", onLoad);
            image.SetObjectProperty("src", AssetPath);
        }
    }
}
