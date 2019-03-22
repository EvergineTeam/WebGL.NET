using WebAssembly;

namespace WebGLDotNET
{
    public class ImageData
    {
        public ImageData(byte[] image, int imageWidth, int imageHeight)
        {
            var uint8ClampedArray = (JSObject)Runtime.GetGlobalObject("Uint8ClampedArray");
            var imageNativeArray = Runtime.NewJSObject(uint8ClampedArray, image);
            var imageDataObject = (JSObject)Runtime.GetGlobalObject("ImageData");
            Handle = Runtime.NewJSObject(imageDataObject, imageNativeArray, imageWidth, imageHeight);
        }

        public object Handle { get; }
    }
}
