using Samples.Helpers;
using System;
using WaveEngine.Common.Math;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    // https://www.ibiblio.org/e-notes/webgl/gpu/bounce.htm
    // https://webglreport.com/?v=2
    public class TransformFeedback : ISample
    {
        private const string ButtonId = "transformNext";

        private WebGL2RenderingContext gl;

        private uint aPosLoc;
        private WebGLBuffer bufA;
        private WebGLBuffer bufB;

        private bool shouldDraw = true;

        private Vector4 clearColor;
        private JSObject canvas;
        private static JSObject contextAttributes;

        private int canvasWidth;
        private int canvasHeight;

        public bool EnableFullScreen => false;

        public string Description => "Simple Transform Feedback WebGL 2 demo from <a href=\"https://www.ibiblio.org/e-notes/webgl/gpu/bounce.htm\">here</a>. " +
            "Points from vertex shader output are swapped between buffers. Then we unbind it and swap buffers for the next draw.";

        public void Init(JSObject canvas, Vector4 clearColor)
        {
            this.clearColor = clearColor;
            this.canvas = canvas;

            canvasWidth = (int)canvas.GetObjectProperty("width");
            canvasHeight = (int)canvas.GetObjectProperty("height");

            HtmlHelper.AddButton(ButtonId, "Next");
            HtmlHelper.AttachButtonOnClickEvent(ButtonId, new Action<JSObject>(clickEvent =>
            {
                shouldDraw = true;
                clickEvent.Dispose();
            }));
        }

        public void Run()
        {
            InitContextAttributes();

            gl = new WebGL2RenderingContext(canvas, contextAttributes);

            var vertexShaderCode =
@"#version 300 es
in vec4 aPos;
void main(void) 
{
   gl_PointSize = 20.;
   gl_Position = vec4(-aPos.x, aPos.yzw);
}
";

            var fragmentShaderCode =
@"#version 300 es
precision highp float;
out vec4 fragColor;
void main(void)
{
    fragColor = vec4(1., 0., 0., 1. );
}
";
            var vertexShader = GLExtensions.GetShader(gl, vertexShaderCode, WebGLRenderingContextBase.VERTEX_SHADER);
            var fragmentShader = GLExtensions.GetShader(gl, fragmentShaderCode, WebGLRenderingContextBase.FRAGMENT_SHADER);

            var shaderProgram = gl.CreateProgram();
            gl.AttachShader(shaderProgram, vertexShader);
            gl.AttachShader(shaderProgram, fragmentShader);
            gl.TransformFeedbackVaryings(shaderProgram, new string[] { "gl_Position" }, WebGL2RenderingContextBase.SEPARATE_ATTRIBS);
            gl.LinkProgram(shaderProgram);
            gl.UseProgram(shaderProgram);

            aPosLoc = (uint)gl.GetAttribLocation(shaderProgram, "aPos");
            gl.EnableVertexAttribArray(aPosLoc);

            var bufAData = new float[] { 0.8f, 0, 0, 1 };
            bufA = gl.CreateArrayBufferWithUsage(bufAData, WebGL2RenderingContextBase.DYNAMIC_COPY);

            var bufBData = new float[4 * 4];
            bufB = gl.CreateArrayBufferWithUsage(bufBData, WebGL2RenderingContextBase.DYNAMIC_COPY);

            var transformFeedback = gl.CreateTransformFeedback();
            gl.BindTransformFeedback(WebGL2RenderingContextBase.TRANSFORM_FEEDBACK, transformFeedback);
        }

        private static void InitContextAttributes()
        {
            Runtime.InvokeJS(
@"
            var obj = {
              antialias: false,
              depth: false
            };
            Module.mono_call_static_method (""[Samples] Samples.TransformFeedback:GetCanvasContextAttributes"", [ obj ]);
");
        }

        public void Update(double elapsedTime)
        { }

        public void Draw()
        {
            if (!shouldDraw)
                return;

            gl.Enable(WebGLRenderingContextBase.DEPTH_TEST);

            gl.Viewport(0, 0, canvasWidth, canvasHeight);

            gl.ClearColor(clearColor.X, clearColor.Y, clearColor.Z, clearColor.W);
            gl.Clear(WebGLRenderingContextBase.COLOR_BUFFER_BIT);

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, bufA);
            gl.VertexAttribPointer(aPosLoc, 4, WebGLRenderingContextBase.FLOAT, false, 0, 0);
            gl.BindBufferBase(WebGL2RenderingContextBase.TRANSFORM_FEEDBACK_BUFFER, 0, bufB);

            gl.BeginTransformFeedback(WebGLRenderingContextBase.POINTS);
            gl.DrawArrays(WebGLRenderingContextBase.POINTS, 0, 1);
            gl.EndTransformFeedback();

            gl.BindBufferBase(WebGL2RenderingContextBase.TRANSFORM_FEEDBACK_BUFFER, 0, null);
            var t = bufA;
            bufA = bufB;
            bufB = t;

            shouldDraw = false;
        }

        public static void GetCanvasContextAttributes(JSObject obj)
        {
            contextAttributes = obj;
        }

        public void Resize(int width, int height)
        {
            canvasWidth = width;
            canvasHeight = height;
        }
    }
}
