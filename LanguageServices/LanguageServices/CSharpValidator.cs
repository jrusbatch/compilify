using System.Collections.Generic;
using System.Linq;
using Compilify.Extensions;
using Compilify.Models;
using Roslyn.Compilers;

namespace Compilify.LanguageServices
{
    public class CSharpValidator : ICodeValidator
    {
        private readonly CSharpCompiler compiler;

        public CSharpValidator()
        {
            compiler = new CSharpCompiler();
        }

        public IEnumerable<EditorError> GetCompilationErrors(ICodeProgram post)
        {
            var result = compiler.RoslynCompile(post).Emit();
            return result.Diagnostics
                         .Where(x => x.Info.Severity == DiagnosticSeverity.Error)
                         .Select(x => new EditorError
                         {
                             Location = DocumentLineSpan.Create(x.Location.GetLineSpan(true)),
                             Message = x.Info.GetMessage()
                         });
        }
    }
}