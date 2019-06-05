// https://astexplorer.net/

/* TODO

- Reuse TypedArrays instead of allocating new ones:

Float32Array transposeMatrix = null;  // Let's keep one around instead of always allocating a new one.
public void UniformMatrix4fv(WebGLUniformLocation location, bool transpose, float[] value)
{
    if (transposeMatrix == null)
        transposeMatrix = Float32Array.From(value);
    else
        transposeMatrix.CopyFrom(value);
    Invoke("uniformMatrix4fv", location?.Handle, transpose, transposeMatrix);
}

 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace WebIDLToCSharp
{
    class Program
    {
        const string WebGL1SpecFile = "webgl.idl";
        const string WebGL2SpecFile = "webgl2.idl";
        const string OutputFile = "../../../../WebGLDotNET/WebGL.cs";

        static void Main(string[] args)
        {
            var input = File.ReadAllText(WebGL1SpecFile);
            input += File.ReadAllText(WebGL2SpecFile);
            var inputStream = CharStreams.fromstring(input);
            var lexer = new WebIDLLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new WebIDLParser(tokenStream);
            var contextSyntaxTree = parser.webIDL();

            using (var outputStream = File.CreateText(OutputFile))
            {
                outputStream.WriteLine("using WebAssembly.Core;");
                outputStream.WriteLine();
                outputStream.WriteLine("namespace WebGLDotNET");
                outputStream.WriteLine("{");

                var listener = new WebIDLListener(outputStream);
                ParseTreeWalker.Default.Walk(listener, contextSyntaxTree);

                outputStream.WriteLine("}");
            }
        }

        class WebIDLListener : WebIDLBaseListener
        {
            const string SequencePrefix = "sequence<";

            readonly StreamWriter outputStream;
            readonly Dictionary<string, string> typesDictionary;
            readonly List<string> constants = new List<string>();
            readonly List<string> operations = new List<string>();

            string returnType;
            string rawMethodName;
            string @params;

            public WebIDLListener(StreamWriter outputStream)
            {
                this.outputStream = outputStream;

                typesDictionary = new Dictionary<string, string>
                {
                    { "any", "object" },
                    { "ArrayBufferView", "ITypedArray"},
                    { "boolean", "bool" },
                    { "DOMString", "string" },
                    { "GLbitfield", "uint" },
                    { "GLboolean", "bool" },
                    { "GLclampf", "float" },
                    { "GLenum", "uint" },
                    { "GLfloat", "float" },
                    { "GLint", "int" },
                    { "GLint64", "long" },
                    { "GLintptr", "uint" },
                    { "GLsizei", "int" },
                    { "GLsizeiptr", "ulong" },
                    { "GLuint", "uint" },
                    { "GLuint64", "ulong" },
                    // Workarounds to bypass commented typedefs with "or"
                    { "BufferDataSource", "System.Array" },
                    // typedef (ImageData or HTMLImageElement or HTMLCanvasElement or HTMLVideoElement) TexImageSource;
                    { "TexImageSource", "object" },
                    // Workarounds for WebGL 2 missing definitions
                    { "BufferSource", "object" },
                    { "Float32List", "object" },
                    { "Int32List", "object" },
                    { "Uint32List", "object" }
                };
            }

            public override void ExitConst_([NotNull] WebIDLParser.Const_Context context)
            {
                base.ExitConst_(context);

                var type = TranslateType(context.constType().GetText());
                var rawName = context.IDENTIFIER_WEBIDL().GetText();
                //var name = CSharpify(rawName);
                outputStream.Write($"        public");

                if (constants.Contains(rawName))
                {
                    outputStream.Write(" new");
                }
                else
                {
                    constants.Add(rawName);
                }

                outputStream.WriteLine($" const {type} {rawName} = {context.constValue().GetText()};");
                outputStream.WriteLine();
            }

            public override void EnterDictionary([NotNull] WebIDLParser.DictionaryContext context)
            {
                base.EnterDictionary(context);

                outputStream.WriteLine($"    public partial class {context.IDENTIFIER_WEBIDL().GetText()}");
                outputStream.WriteLine( "    {");
            }

            public override void ExitDictionary([NotNull] WebIDLParser.DictionaryContext context)
            {
                base.ExitDictionary(context);

                outputStream.WriteLine("    }");
                outputStream.WriteLine();
            }

            public override void EnterDictionaryMember([NotNull] WebIDLParser.DictionaryMemberContext context)
            {
                base.EnterDictionaryMember(context);

                var type = TranslateType(context.type().GetText());
                var name = CSharpify(context.IDENTIFIER_WEBIDL().GetText());
                var value = context.default_().defaultValue()?.GetText();
                outputStream.Write($"        public {type} {name} {{ get; set; }}");

                if (value != null)
                {
                    outputStream.WriteLine($" = {value};");
                }

                outputStream.WriteLine();
            }

            public override void ExitDictionaryMember([NotNull] WebIDLParser.DictionaryMemberContext context)
            {
                base.ExitDictionaryMember(context);
            }

            public override void EnterInterface_([NotNull] WebIDLParser.Interface_Context context)
            {
                base.EnterInterface_(context);

                // partial because can be backed with additional glue outside
                outputStream.Write($"    public partial class {context.IDENTIFIER_WEBIDL().GetText()}");

                var inheritance = context.inheritance().IDENTIFIER_WEBIDL();

                if (inheritance == null)
                {
                    outputStream.WriteLine();
                }
                else
                {
                    outputStream.WriteLine($" : {inheritance.GetText()}");
                }

                outputStream.WriteLine( "    {");
            }

            public override void ExitInterface_([NotNull] WebIDLParser.Interface_Context context)
            {
                base.ExitInterface_(context);

                outputStream.WriteLine("    }");
                outputStream.WriteLine();
            }

            public override void EnterOperation([NotNull] WebIDLParser.OperationContext context)
            {
                base.EnterOperation(context);

                returnType = TranslateType(context.returnType().GetText());
                rawMethodName = context.operationRest().optionalIdentifier().GetText();
                var methodName = CSharpify(rawMethodName);
                outputStream.Write($"        public");

                var rawOperation = context.GetText();

                if (operations.Contains(rawOperation))
                {
                    outputStream.Write($" new");

                    Console.WriteLine($"Operation already processed: {rawOperation}");
                }
                else
                {
                    operations.Add(rawOperation);

                    Console.WriteLine($"Operation added: {rawOperation}");
                }

                outputStream.Write($" {returnType} {methodName}(");

                @params = string.Empty;
            }

            public override void ExitOperation([NotNull] WebIDLParser.OperationContext context)
            {
                base.ExitOperation(context);

                outputStream.Write(") => ");

                var isArray = returnType.EndsWith("[]", StringComparison.InvariantCultureIgnoreCase);
                var isReturnTypeBasic = typesDictionary.ContainsValue(returnType) && returnType != "object";

                if (isArray)
                {
                    var finalReturnType = returnType.Substring(0, returnType.Length - 2);
                    outputStream.Write($"InvokeForArray<{finalReturnType}>");
                }
                else if (isReturnTypeBasic)
                {
                    outputStream.Write($"InvokeForBasicType<{returnType}>");
                }
                else
                {
                    outputStream.Write("Invoke");

                    var isGenericNeeded = returnType != "void" && returnType != "object";

                    if (isGenericNeeded)
                    {
                        outputStream.Write($"<{returnType}>");
                    }
                }

                outputStream.Write($"(\"{rawMethodName}\"{@params});");
                outputStream.WriteLine();
                outputStream.WriteLine();
            }

            public override void ExitOptionalOrRequiredArgument(
                [NotNull] WebIDLParser.OptionalOrRequiredArgumentContext context)
            {
                base.ExitOptionalOrRequiredArgument(context);

                var type = TranslateType(context.type().GetText());
                var argument = context.argumentName().GetText();

                if (argument == "ref")
                {
                    argument = argument.Insert(0, "@");
                }

                outputStream.Write($"{type} {argument}");

                @params += $", {argument}";

                var areThereMoreParams =
                    (context.Parent.Parent is WebIDLParser.ArgumentsContext arguments &&
                    arguments != null &&
                    arguments.arguments().ChildCount > 0) ||
                    (context.Parent.Parent is WebIDLParser.ArgumentListContext argumentList &&
                    argumentList != null &&
                    argumentList.arguments().arguments() != null);

                if (areThereMoreParams)
                {
                    outputStream.Write(", ");
                }
            }

            static string CSharpify(string value)
            {
                var result = value;

                if (result.Contains("_"))
                {
                    result = result
                        .ToLowerInvariant()
                        .Split('_')
                        .Aggregate((aggregated, item) =>
                            aggregated + item[0].ToString().ToUpperInvariant() + item.Substring(1));
                }

                if (char.IsLower(result[0]))
                {
                    var firstChar = result[0];
                    var headlessValue = result.Substring(1);
                    result = headlessValue.Insert(0, firstChar.ToString().ToUpperInvariant());
                }

                return result;
            }

            string TranslateType(string rawType)
            {
                var type = rawType.TrimEnd('?');
                var isSequence = false;

                if (rawType.StartsWith(SequencePrefix, StringComparison.InvariantCulture))
                {
                    type = type.Substring(
                        SequencePrefix.Length, 
                        type.Length - SequencePrefix.Length - 1);
                    isSequence = true;
                }

                if (typesDictionary.ContainsKey(type))
                {
                    type = typesDictionary[type];
                }

                if (isSequence)
                {
                    type += "[]";
                }

                return type;
            }
        }
    }
}
