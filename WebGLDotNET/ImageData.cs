using System;
using WebAssembly;
using WebAssembly.Core;
using WebAssembly.Host;

namespace WebGLDotNET
{
    public class ImageData : JSHandler, IDisposable
    {
        // to detect redundant calls
        public bool IsDisposed { get; internal set; }

        public ImageData(byte[] image, int imageWidth, int imageHeight)
        {
            // Make sure we dispose of the clamped array as it is only used here to
            // to create an ImageData.  If not it stays in managed memory and on the 
            // javascript side.
            using (var uint8ClampedArray = Uint8ClampedArray.From(image))
                Handle = new HostObject("ImageData", uint8ClampedArray, imageWidth, imageHeight);
        }

        ~ImageData()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {

            if (!IsDisposed)
            {
                if (disposing)
                {

                    // Free any other managed objects here.
                    //
                }

                IsDisposed = true;

                // Free unmanaged objects here.  i.e. our javascript ImageData object handle that we created
                // 
                Handle.Dispose();

            }
        }
    }
}
