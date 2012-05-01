using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Compilify.Models;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;
using Compilify.Extensions;

namespace Compilify.Services
{
    public class CSharpValidator
    {
        public CSharpValidator(ICSharpCompilationProvider compilationProvider)
        {
            compiler = compilationProvider;
        }

        private readonly ICSharpCompilationProvider compiler;

        public IEnumerable<IDiagnostic> GetCompilationErrors(Post post)
        {
            var builder = new StringBuilder();

            builder.AppendLine("public static object Eval() {");
            builder.AppendLine("#line 1");
            builder.Append(post.Content);
            builder.AppendLine("}");

            var script = builder.ToString();

            var entryPoint = SyntaxTree.ParseCompilationUnit(CSharpExecutor.EntryPoint);

            var prompt = SyntaxTree.ParseCompilationUnit(script, fileName: "Prompt", 
                                                         options: new ParseOptions(kind: SourceCodeKind.Interactive));

            var editor = SyntaxTree.ParseCompilationUnit(post.Classes ?? string.Empty, fileName: "Editor", 
                                                         options: new ParseOptions(kind: SourceCodeKind.Script));

            var result = compiler.Compile("foo", new[] { entryPoint, prompt, editor }).Emit();

            return result.Diagnostics;
        } 
    }
}