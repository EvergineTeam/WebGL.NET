using WebAssembly;

namespace WebGLDotNET
{
    public class WebGLObject
    {
        public WebGLObject(JSObject handle)
        {
            Handle = handle;
        }

        public JSObject Handle { get; set; }
    }
}