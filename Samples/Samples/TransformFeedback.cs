using System.Drawing;
using WebAssembly;
using WebGLDotNET;

namespace Samples
{
    // https://www.ibiblio.org/e-notes/webgl/gpu/bounce.htm
    // https://webglreport.com/?v=2
    public class TransformFeedback : ISample
    {
        private WebGL2RenderingContext gl;

        private uint aPosLoc;
        private WebGLBuffer bufA;
        private WebGLBuffer bufB;

        public string Description => "Simple Transform Feedback WebGL 2 demo from <a href=\"https://www.ibiblio.org/e-notes/webgl/gpu/bounce.htm\">here</a>. " +
            "Points from vertex shader output are swapped between buffers. Then we unbind it and swap buffers for the next draw.";

        public void Run(JSObject canvas, float canvasWidth, float canvasHeight, Color clearColor)
        {
            gl = new WebGL2RenderingContext(canvas);

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

            var bufAData = new float[]{ 0.8f, 0, 0, 1 };
            bufA = gl.CreateArrayBufferWithUsage(bufAData, WebGL2RenderingContextBase.DYNAMIC_COPY);

            var bufBData = new float[] { 4 * 4 };
            bufB = gl.CreateArrayBufferWithUsage(bufBData, WebGL2RenderingContextBase.DYNAMIC_COPY);

            var transformFeedback = gl.CreateTransformFeedback();
            gl.BindTransformFeedback(WebGL2RenderingContextBase.TRANSFORM_FEEDBACK, transformFeedback);
        }

        public void Update(double elapsedTime)
        {
            var t = bufA;
            bufA = bufB;
            bufB = t;
        }

        public void Draw()
        {
            gl.Clear(WebGLRenderingContextBase.COLOR_BUFFER_BIT);

            gl.BindBuffer(WebGLRenderingContextBase.ARRAY_BUFFER, bufA);
            gl.VertexAttribPointer(aPosLoc, 4, WebGLRenderingContextBase.FLOAT, false, 0, 0);
            gl.BindBufferBase(WebGL2RenderingContextBase.TRANSFORM_FEEDBACK_BUFFER, 0, bufB);

            gl.BeginTransformFeedback(WebGLRenderingContextBase.POINTS);
            gl.DrawArrays(WebGLRenderingContextBase.POINTS, 0, 1);
            gl.EndTransformFeedback();

            gl.BindBufferBase(WebGL2RenderingContextBase.TRANSFORM_FEEDBACK_BUFFER, 0, null);
        }
    }
}
