using System;
using System.Runtime.InteropServices;

namespace Samples.GLTF
{
    public class BufferInfo : IDisposable
    {
        public byte[] bufferBytes;
        public IntPtr bufferPointer;
        public GCHandle bufferHandle;

        public BufferInfo(byte[] bufferBytes)
        {
            this.bufferBytes = bufferBytes;
            this.bufferHandle = GCHandle.Alloc(this.bufferBytes, GCHandleType.Pinned);
            this.bufferPointer = Marshal.UnsafeAddrOfPinnedArrayElement(this.bufferBytes, 0);
        }

        public void Dispose()
        {
            this.bufferHandle.Free();
        }
    }
}
