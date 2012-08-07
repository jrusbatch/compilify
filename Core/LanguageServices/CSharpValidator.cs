using System.Collections.Generic;
using System.Linq;
using Compilify.Extensions;
using Compilify.Models;
using Roslyn.Compilers;

namespace Compilify.LanguageServices
{
    public class CSharpValidator : ICodeValidator
    {
        private readonly ICSharpCompilationProvider compiler;

        public CSharpValidator(ICSharpCompilationProvider compilationProvider)
        {
            compiler = compilationProvider;
        }

        public IEnumerable<EditorError> GetCompilationErrors(Post post)
        {
            var result = compiler.Compile(post).Emit();
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