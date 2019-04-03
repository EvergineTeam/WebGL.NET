// https://astexplorer.net/

using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using System.Linq;

namespace WebIDLToCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadAllText("webgl.idl");
            var inputStream = CharStreams.fromstring(input);
            var lexer = new WebIDLLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new WebIDLParser(tokenStream);
            var contextSyntaxTree = parser.webIDL();

            using (var outputStream = File.CreateText("WebGL.cs"))
            {
                outputStream.WriteLine("namespace WebGLDotNET");
                outputStream.WriteLine("{");

                var listener = new WebIDLListener(outputStream);
                ParseTreeWalker.Default.Walk(listener, contextSyntaxTree);

                outputStream.WriteLine("}");
            }
        }

        class WebIDLListener : WebIDLBaseListener
        {
            readonly StreamWriter outputStream;
            readonly Dictionary<string, string> typesDictionary;

            string rawMethodName;
            string @params;

            public WebIDLListener(StreamWriter outputStream)
            {
                this.outputStream = outputStream;

                typesDictionary = new Dictionary<string, string>
                {
                    { "any", "object" },
                    { "boolean", "bool" },
                    { "DOMString", "string" },
                    { "GLbitfield", "uint" },
                    { "GLboolean", "bool" },
                    { "GLclampf", "float" },
                    { "GLenum", "uint" },
                    { "GLfloat", "float" },
                    { "GLint", "int" },
                    { "GLsizei", "int" },
                    { "GLsizeiptr", "ulong" },
                    { "GLuint", "uint" },
                    { "sequence<DOMString>", "string[]" }
                };
            }

            public override void ExitConst_([NotNull] WebIDLParser.Const_Context context)
            {
                base.ExitConst_(context);

                var type = typesDictionary[context.constType().GetText()];
                var name = CSharpify(context.IDENTIFIER_WEBIDL().GetText());
                outputStream.WriteLine($"        public const {type} {name} = {context.constValue().GetText()};");
                outputStream.WriteLine();
            }

            public override void EnterDictionary([NotNull] WebIDLParser.DictionaryContext context)
            {
                base.EnterDictionary(context);

                outputStream.WriteLine($"    public class {context.IDENTIFIER_WEBIDL().GetText()}");
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

                var type = typesDictionary[context.type().GetText()];
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

                outputStream.Write($"    public class {context.IDENTIFIER_WEBIDL().GetText()}");

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

                var returnType = TranslateType(context.returnType().GetText());
                rawMethodName = context.operationRest().optionalIdentifier().GetText();
                var methodName = CSharpify(rawMethodName);
                outputStream.Write($"        public {returnType} {methodName}(");

                @params = string.Empty;
            }

            public override void ExitOperation([NotNull] WebIDLParser.OperationContext context)
            {
                base.ExitOperation(context);

                outputStream.Write($") => Invoke(\"{rawMethodName}\"{@params});");
                outputStream.WriteLine();
                outputStream.WriteLine();
            }

            public override void ExitOptionalOrRequiredArgument([NotNull] WebIDLParser.OptionalOrRequiredArgumentContext context)
            {
                base.ExitOptionalOrRequiredArgument(context);

                var type = TranslateType(context.type().GetText());
                var argument = context.argumentName().GetText();
                outputStream.Write($"{type} {argument}");

                @params += $", {argument}";

                if ((context.Parent.Parent is WebIDLParser.ArgumentsContext arguments && 
                    arguments != null &&
                    arguments.arguments().ChildCount > 0) ||
                    (context.Parent.Parent is WebIDLParser.ArgumentListContext argumentList && 
                    argumentList != null &&
                    argumentList.arguments().arguments() != null))
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
                var returnType = rawType.TrimEnd('?');

                if (typesDictionary.ContainsKey(returnType))
                {
                    returnType = typesDictionary[returnType];
                }

                return returnType;
            }
        }
    }
}
