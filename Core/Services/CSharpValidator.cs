using System.Collections.Generic;
using System.Linq;
using Compilify.Extensions;
using Compilify.Models;
using Roslyn.Compilers;
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

        public IEnumerable<CommonDiagnostic> GetCompilationErrors(Post post)
        {
            var result = compiler.Compile(post).Emit();

            return result.Diagnostics.Where(x => x.Info.Severity > DiagnosticSeverity.Error);
        }
    }
}