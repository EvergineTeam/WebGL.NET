using WebGLDotNET;

namespace Samples
{
    // Translated from: https://stackoverflow.com/a/46806403
    public class DepthStencil : BaseSample
    {
        private WebGLProgram shadersProgram;
        private uint vertexAttributeLocation;
        private uint colorAttributeLocation;
        private WebGLBuffer triangleVertexBuffer;
        private WebGLBuffer triangleColorBuffer;
        private WebGLBuffer quadVertexBuffer;
        private WebGLBuffer quadColorBuffer;

        public override void Run()
        {
            base.Run();

            InitShadersProgram();
            InitGLAttribLocations();
            InitBuffers();
        }

        public override void Draw()
        {
            gl.Enable(WebGLRenderingContextBase.STENCIL_TEST);
            
            base.Draw();

            gl.UseProgram(shadersProgram);

            gl.Clear(
                WebGLRenderingContextBase.COLOR_BUFFER_BIT | 
                WebGLRenderingContextBase.DEPTH_BUFFER_BIT | 
                WebGLRenderingContextBase.STENCIL_BUFFER_BIT);

            gl.EnableVertexAttribArray(vertexAttributeLocation);
            gl.EnableVertexAttribArray(colorAttributeLocation);

            gl.StencilOp(
                WebGLRenderingContextBase.KEEP, 
                WebGLRenderingContextBase.KEEP, 
                WebGLRenderingContextBase.REPLACE);

            gl.StencilFunc(WebGLRenderingContextBase.ALWAYS, 1, 0xff);
            gl.StencilMask(0xff);
            gl.DepthMask(false);
            gl.ColorMask(false, false, false, false);

            DrawQuads();

            gl.StencilFunc(WebGLRenderingContextBase.EQUAL, 1, 0xff);
            gl.StencilMask(0x00);
            gl.DepthMask(true);
            gl.ColorMask(true, true, true, true);

            DrawTriagles();

            gl.DisableVertexAttribArray(vertexAttributeLocation);
            gl.DisableVertexAttribArray(colorAttributeLocation);

            gl.Flush();
        }

        private void DrawTriagles()
        {
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, triangleVertexBuffer);
            gl.VertexAttribPointer(vertexAttributeLocation, 3, WebGLRenderingContextBase.FLOAT, false, 0, 0);

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, triangleColorBuffer);
            gl.VertexAttribPointer(colorAttributeLocation, 4, WebGLRenderingContextBase.FLOAT, false, 0, 0);

            gl.DrawArrays(WebGLRenderingContextBase.TRIANGLES, 0, 6);
        }

        private void DrawQuads()
        {
            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, quadVertexBuffer);
            gl.VertexAttribPointer(vertexAttributeLocation, 3, WebGLRenderingContextBase.FLOAT, false, 0, 0);

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, quadColorBuffer);
            gl.VertexAttribPointer(colorAttributeLocation, 4, WebGLRenderingContextBase.FLOAT, false, 0, 0);

            gl.DrawArrays(WebGLRenderingContextBase.TRIANGLE_STRIP, 0, 4);
        }

        private void InitBuffers()
        {
            triangleVertexBuffer = gl.CreateBuffer();
            triangleColorBuffer = gl.CreateBuffer();
            quadVertexBuffer = gl.CreateBuffer();
            quadColorBuffer = gl.CreateBuffer();

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, triangleVertexBuffer);
            var vertices = new float[]
            {
                0.0f, 1.0f, 0.0f,
                -1.0f, -1.0f, 0.0f,
                1.0f, -1.0f, 0.0f,

                0.0f, -1.0f, 0.0f,
                -1.0f, 1.0f, 0.0f,
                1.0f, 1.0f, 0.0f
            };
            gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, vertices, WebGLRenderingContextBase.STATIC_DRAW);

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, triangleColorBuffer);
            var colors = new float[]
            {
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,
                0.0f, 1.0f, 0.0f, 1.0f,

                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f,
                0.0f, 0.0f, 1.0f, 1.0f
            };
            gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, colors, WebGLRenderingContextBase.STATIC_DRAW);


            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, quadVertexBuffer);
            vertices = new float[]
            {
                -1.0f, 1.0f, 0.0f,
                -1.0f, -1.0f, 0.0f,
                1.0f, 1.0f, 0.0f,
                1.0f, -1.0f, 0.0f
            };

            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] *= 0.75f;
            }

            gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, vertices, WebGLRenderingContextBase.STATIC_DRAW);

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, quadColorBuffer);
            colors = new float[]
            {
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
                1.0f, 0.0f, 0.0f, 1.0f,
            };
            gl.BufferData(WebGLRenderingContextBase.ARRAY_BUFFER, colors, WebGLRenderingContextBase.STATIC_DRAW);
        }

        private void InitGLAttribLocations()
        {
            vertexAttributeLocation = (uint)gl.GetAttribLocation(shadersProgram, "a_vertex");
            colorAttributeLocation = (uint)gl.GetAttribLocation(shadersProgram, "a_color");
        }

        private void InitShadersProgram()
        {
            shadersProgram = gl.InitializeShaders(
                            vertexShaderCode:
            @"attribute vec3 a_vertex;
attribute vec4 a_color;

varying vec4 v_color;

void main(void) {
	v_color = a_color;
	gl_Position = vec4(a_vertex, 1.0);
}",
                            fragmentShaderCode:
            @"precision mediump float;

varying vec4 v_color;
void main(void) {
    gl_FragColor = v_color;
}");
        }
    }
}
