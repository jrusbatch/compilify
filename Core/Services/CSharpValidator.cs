using System.Collections.Generic;
using Compilify.Extensions;
using Compilify.Models;
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

        public IEnumerable<IDiagnostic> GetCompilationErrors(Post post)
        {
            var result = compiler.Compile(post).Emit();

            return result.Diagnostics;
        }
    }
}