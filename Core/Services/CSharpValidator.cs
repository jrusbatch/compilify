using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;

namespace Compilify.Services
{
    public class CSharpValidator
    {
        public CSharpValidator(ICSharpCompilationProvider compilationProvider)
        {
            compiler = compilationProvider;
        }

        private readonly ICSharpCompilationProvider compiler;

        public IEnumerable<IDiagnostic> GetCompilationErrors(string command, string classes)
        {
            var builder = new StringBuilder();

            builder.AppendLine("public static object Eval() {");
            builder.AppendLine("#line 1");
            builder.Append(command);
            builder.AppendLine();
            builder.AppendLine("}");

            var script = builder.ToString();

            var trees = new[]
            {
                SyntaxTree.ParseCompilationUnit(CSharpExecutor.EntryPoint),
                SyntaxTree.ParseCompilationUnit(script, fileName: "Prompt", options: new ParseOptions(kind: SourceCodeKind.Interactive)),
                SyntaxTree.ParseCompilationUnit(classes ?? string.Empty, fileName: "Editor", options: new ParseOptions(kind: SourceCodeKind.Script))
            };

            var compilation = compiler.Compile("foo", trees);
            using (var output = new MemoryStream())
            {
                var emitResult = compilation.Emit(output);

                if (!emitResult.Success)
                {
                    return emitResult.Diagnostics;
                }
            }

            return Enumerable.Empty<IDiagnostic>();
        } 
    }
}