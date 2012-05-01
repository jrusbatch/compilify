using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compilify.Models;
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
            var result = compiler.Compile(post).Emit();

            return result.Diagnostics;
        }
    }
}