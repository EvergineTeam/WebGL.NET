using System;
using System.Runtime.InteropServices;
using WebAssembly;
using WebGLDotNET;
using Xunit;

namespace Tests
{
    public class WebGL2Tests : BaseTests
    {
        private const int TextureWidthOrHeight = 1;
        private const int BytesPerPixel = 4;

        private readonly WebGL2RenderingContext gl;
        private readonly byte[] pixels;
        private readonly GCHandle pixelsHandle;
        private readonly WebGLTexture texture;

        public WebGL2Tests(JSObject canvas)
        {
            if (!WebGL2RenderingContextBase.IsSupported)
            {
                throw new InconclusiveException("WebGL 2 is not supported");
            }

            gl = new WebGL2RenderingContext(canvas);

            pixels = new byte[TextureWidthOrHeight * 2 * BytesPerPixel];
            pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            texture = gl.CreateTexture();
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/5
        public void GetUniformBlockIndexRegressionTest()
        {
            var program = SetUpProgram(gl);

            gl.GetUniformBlockIndex(program, "foo");
        }

        // https://github.com/WaveEngine/WebGL.NET/issues/6
        public void BindBufferRangeRegressionTest()
        {
            var buffer = gl.CreateBuffer();

            gl.BindBufferRange(WebGL2RenderingContextBase.UNIFORM_BUFFER, 0, buffer, 0, 4);
        }

        public void GetActiveUniformRegressionTest()
        {
            var program = SetUpProgram(
                gl,
@"attribute vec3 position;
attribute vec3 color;

uniform mat4 pMatrix;
uniform mat4 vMatrix;
uniform mat4 wMatrix;

varying vec3 vColor;

void main(void) {
    gl_Position = pMatrix * vMatrix * wMatrix * vec4(position, 1.0);
    vColor = color;
}");

            var uniform = gl.GetActiveUniform(program, 0);

            Assert.IsType(typeof(uint), uniform.Type);
        }

        public void GetSupportedExtensionsRegressionTest()
        {
            var extensions = gl.GetSupportedExtensions();

            Assert.NotEmpty(extensions);
        }

        public void GetUniformIndicesTest()
        {
            var program = SetUpProgram(
                gl,
@"attribute vec3 position;
attribute vec3 color;

uniform mat4 pMatrix;
uniform mat4 vMatrix;
uniform mat4 wMatrix;

varying vec3 vColor;

void main(void) {
    gl_Position = pMatrix * vMatrix * wMatrix * vec4(position, 1.0);
    vColor = color;
}");
            var names = new string[] { "pMatrix", "vMatrix", "wMatrix" };

            var indices = gl.GetUniformIndices(program, names);

            if (indices == null)
            {
                // This happens "only" in Safari
                throw new InconclusiveException("The indices array is empty");
            }

            Assert.Equal(names.Length, indices.Length);
        }

        public void SameBufferBindMultipleTargetsTest()
        {
            var buffer = gl.CreateBuffer();
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, buffer);
            gl.BindBuffer(WebGLRenderingContextBase.ELEMENT_ARRAY_BUFFER, buffer);
            var error = gl.GetError();

            Assert.Equal(WebGLRenderingContextBase.INVALID_OPERATION, error);
        }

        public unsafe void ReadOnlySpanByteSourceTexImage2DTest()
        {
            var span = new ReadOnlySpan<byte>(pixelsHandle.AddrOfPinnedObject().ToPointer(), pixels.Length);

            gl.BindTexture(WebGLRenderingContextBase.TEXTURE_2D, texture);
            gl.TexImage2D(
                WebGLRenderingContextBase.TEXTURE_2D,
                0,
                (int)WebGLRenderingContextBase.RGBA,
                TextureWidthOrHeight,
                TextureWidthOrHeight,
                0,
                WebGLRenderingContextBase.RGBA,
                WebGLRenderingContextBase.UNSIGNED_BYTE,
                span);
            var error = gl.GetError();

            Assert.Equal((uint)0, error);
        }

        public unsafe void ReadOnlySpanByteSourceTexImage3DTest()
        {
            var span = new ReadOnlySpan<byte>(pixelsHandle.AddrOfPinnedObject().ToPointer(), pixels.Length);

            gl.BindTexture(WebGL2RenderingContextBase.TEXTURE_2D_ARRAY, texture);
            gl.TexImage3D(
                WebGL2RenderingContextBase.TEXTURE_2D_ARRAY,
                0,
                (int)WebGLRenderingContextBase.RGBA,
                TextureWidthOrHeight,
                TextureWidthOrHeight,
                0,
                0,
                WebGLRenderingContextBase.RGBA,
                WebGLRenderingContextBase.UNSIGNED_BYTE,
                span);
            var error = gl.GetError();

            Assert.Equal((uint)0, error);
        }

        public unsafe void ReadOnlySpanByteSourceTexSubImage3DTest()
        {
            var span = new ReadOnlySpan<byte>(pixelsHandle.AddrOfPinnedObject().ToPointer(), pixels.Length);

            gl.BindTexture(WebGL2RenderingContextBase.TEXTURE_2D_ARRAY, texture);
            gl.TexStorage3D(
                WebGL2RenderingContextBase.TEXTURE_2D_ARRAY, 
                1, 
                WebGL2RenderingContextBase.RGBA8, 
                TextureWidthOrHeight, 
                TextureWidthOrHeight, 
                1);
            gl.TexSubImage3D(
                WebGL2RenderingContextBase.TEXTURE_2D_ARRAY,
                0,
                0,
                0,
                0,
                TextureWidthOrHeight,
                TextureWidthOrHeight,
                1,
                WebGLRenderingContextBase.RGBA,
                WebGLRenderingContextBase.UNSIGNED_BYTE,
                span);
            var error = gl.GetError();

            Assert.Equal((uint)0, error);
        }

        public unsafe void ReadOnlySpanByteSourceTexSubImage2DTest()
        {
            var span = new ReadOnlySpan<byte>(pixelsHandle.AddrOfPinnedObject().ToPointer(), pixels.Length);

            gl.BindTexture(WebGLRenderingContextBase.TEXTURE_2D, texture);
            gl.TexStorage2D(
                WebGLRenderingContextBase.TEXTURE_2D,
                1,
                WebGL2RenderingContextBase.RGBA8,
                TextureWidthOrHeight,
                TextureWidthOrHeight);
            gl.TexSubImage2D(
                WebGLRenderingContextBase.TEXTURE_2D,
                0,
                0,
                TextureWidthOrHeight,
                TextureWidthOrHeight,
                0,
                WebGLRenderingContextBase.RGBA,
                WebGLRenderingContextBase.UNSIGNED_BYTE,
                span);
            var error = gl.GetError();

            Assert.Equal((uint)0, error);
        }

        public void TextureFilterAnisotropicExtensionTest()
        {
            var extension = (JSObject)gl.GetExtension("EXT_texture_filter_anisotropic");

            if (extension == null)
            {
                throw new InconclusiveException();
            }

            var parameterName = (uint)(int)extension.GetObjectProperty("MAX_TEXTURE_MAX_ANISOTROPY_EXT");
            var maxAnisotropy = gl.GetParameter(parameterName);

            Assert.IsType<int>(maxAnisotropy);
        }
    }
}