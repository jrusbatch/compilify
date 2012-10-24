using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Compilify.Extensions;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;

namespace Compilify.LanguageServices
{
    public class CSharpCompiler : ICodeCompiler
    {
        private const string EntryPoint = 
            @"public static class EntryPoint 
              {
                  public static object Result { get; set; }
                  
                  public static void Main()
                  {
                      Result = Script.Eval();
                  }
              }";

        private const string Console = "public static readonly System.IO.StringWriter __Console = new System.IO.StringWriter();";

        private static readonly IEnumerable<string> DefaultNamespaces =
            new[]
            {
                "System", 
                "System.IO", 
                "System.Net", 
                "System.Linq", 
                "System.Text", 
                "System.Text.RegularExpressions", 
                "System.Collections.Generic"
            };

        private static readonly IEnumerable<MetadataReference> DefaultReferences =
            new[]
            {
                MetadataReference.CreateAssemblyReference("mscorlib"),
                MetadataReference.CreateAssemblyReference("System"),
                MetadataReference.CreateAssemblyReference("System.Core")
            };

        private static readonly ParseOptions DefaultParseOptions = 
            ParseOptions.Default.WithKind(SourceCodeKind.Interactive);

        private static readonly CompilationOptions DefaultCompilationOptions =
            new CompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithUsings(DefaultNamespaces);

        public ICodeAssembly Compile(ICodeProgram program)
        {
            var compilation = RoslynCompile(program);

            var assembly = new ProgramAssembly
                           {
                               EntryPointClassName = "Script+EntryPoint",
                               EntryPointMethodName = "Main"
                           };

            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    return null;
                }

                assembly.CompiledAssembly = stream.ToArray();
            }

            return assembly;
        }

        public CommonCompilation RoslynCompile(ICodeProgram codeProgram)
        {
            if (codeProgram == null)
            {
                throw new ArgumentNullException("codeProgram");
            }

            var references =
                codeProgram.References.Select(x => MetadataReference.CreateAssemblyReference(x.AssemblyName));

            var compilation =
                Compilation.Create(codeProgram.Name ?? "Untitled")
                           .WithReferences(references)
                           .WithOptions(DefaultCompilationOptions)
                           .AddSyntaxTrees(
                               SyntaxTree.ParseText(Console, options: DefaultParseOptions),
                               SyntaxTree.ParseText(EntryPoint, options: DefaultParseOptions));

            foreach (var document in codeProgram.Documents)
            {
                var text = document.IsEntryPoint ? BuildScript(document.Content) : document.Content;

                var tree = SyntaxTree.ParseText(text, document.Name, DefaultParseOptions);
                
                compilation = compilation.AddSyntaxTrees(tree);

                var rewriter = new ConsoleRewriter("__Console", compilation.GetSemanticModel(tree));

                compilation = compilation.ReplaceSyntaxTree(tree, tree.RewriteWith(rewriter));
            }

            return compilation;
        }

        private static string BuildScript(string content)
        {
            var builder = new StringBuilder();

            builder.AppendLine("public static object Eval() {");
            builder.AppendLine("#line 1");
            builder.Append(content);
            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
