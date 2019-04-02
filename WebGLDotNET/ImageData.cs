using WebAssembly;
using WebAssembly.Core;
using WebAssembly.Host;

namespace WebGLDotNET
{
    public class ImageData
    {
        public ImageData(byte[] image, int imageWidth, int imageHeight)
        {
            // Use the following after issue https://github.com/mono/mono/issues/13787 is fixed
            //var uint8ClampedArray = Uint8ClampedArray.From(image);
            var uint8ClampedArray = new Uint8ClampedArray(image.Length);
            uint8ClampedArray.CopyFrom(image);
            Handle = new HostObject("ImageData", uint8ClampedArray, imageWidth, imageHeight);
        }

        public object Handle { get; }
    }
}
