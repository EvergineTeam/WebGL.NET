using WebAssembly;
using WebAssembly.Core;

namespace WebGLDotNET
{
    public class ImageData
    {
        public ImageData(byte[] image, int imageWidth, int imageHeight)
        {
            var uint8ClampedArray = new Uint8ClampedArray(image.Length);
            uint8ClampedArray.CopyFrom(image);
            var imageDataObject = (JSObject)Runtime.GetGlobalObject("ImageData");
            Handle = Runtime.NewJSObject(imageDataObject, uint8ClampedArray, imageWidth, imageHeight);
        }

        public object Handle { get; }
    }
}
