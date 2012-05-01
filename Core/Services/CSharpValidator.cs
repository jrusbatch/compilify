using System.Collections.Generic;
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
            var entryPoint = SyntaxTree.ParseCompilationUnit(CSharpExecutor.EntryPoint);

            var prompt = SyntaxTree.ParseCompilationUnit(BuildScript(post.Content), 
                                                         options: new ParseOptions(kind: SourceCodeKind.Interactive))
                                   .RewriteWith<MissingSemicolonRewriter>();

            var editor = SyntaxTree.ParseCompilationUnit(post.Classes ?? string.Empty, fileName: "Editor", 
                                                         options: new ParseOptions(kind: SourceCodeKind.Script))
                                   .RewriteWith<MissingSemicolonRewriter>();

            var result = compiler.Compile("foo", new[] { entryPoint, prompt, editor }).Emit();

            return result.Diagnostics;
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