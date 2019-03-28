using WebAssembly;

namespace WebGLDotNET
{
    public class WebGLUniformLocation
    {
        public WebGLUniformLocation(JSObject handle)
        {
            Handle = handle;
        }

        public JSObject Handle { get; set; }
    }
}