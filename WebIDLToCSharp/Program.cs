// https://astexplorer.net/

using System;
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

            using (var outputStream = File.CreateText("bin/Debug/netcoreapp2.1/WebGL.cs"))
            {
                outputStream.WriteLine("namespace WebGLDotNET");
                outputStream.WriteLine("{");

                var listener = new WebIDLListener(tokenStream, outputStream);
                ParseTreeWalker.Default.Walk(listener, contextSyntaxTree);

                outputStream.WriteLine("}");
            }
        }

        class WebIDLListener : WebIDLBaseListener
        {
            readonly CommonTokenStream tokenStream;
            readonly StreamWriter outputStream;
            readonly Dictionary<string, string> typesDictionary;

            public WebIDLListener(CommonTokenStream tokenStream, StreamWriter outputStream)
            {
                this.tokenStream = tokenStream;
                this.outputStream = outputStream;

                typesDictionary = new Dictionary<string, string>();
                typesDictionary.Add("GLboolean", "bool");
                typesDictionary.Add("GLenum", "ulong");
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
                var value = context.default_().defaultValue().GetText();
                outputStream.WriteLine($"        public {type} {name} {{ get; set; }} = {value};");
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

            public override void ExitOperation([NotNull] WebIDLParser.OperationContext context)
            {
                base.ExitOperation(context);

                var returnType = context.returnType().GetText();
                returnType = returnType.TrimEnd('?');
                var methodName = context.operationRest().optionalIdentifier().GetText();
                var arguments = context.operationRest().argumentList().GetText();
                outputStream.WriteLine(
                    $"        public {returnType} {methodName}({arguments}) => Invoke(\"{methodName}\", texture);");
            }

            public override void ExitTypedef([NotNull] WebIDLParser.TypedefContext context)
            {
                base.ExitTypedef(context);

                // Here we get every WebGL type equivalence: i.e. GLenum -> unsigned long
                //typesDictionary.Add(context.IDENTIFIER_WEBIDL().GetText(), context.type().GetText());
            }

            static string CSharpify(string value)
            {
                var result = value.ToLowerInvariant();

                if (result.Contains("_"))
                {
                    result = result
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
        }
    }
}
