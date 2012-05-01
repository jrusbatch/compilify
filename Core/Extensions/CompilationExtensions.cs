using System.IO;
using Roslyn.Compilers.Common;

namespace Compilify.Extensions
{
    internal static class CompilationExtensions
    {
        internal static IEmitResult Emit(this ICompilation compilation)
        {
            using (var stream = new MemoryStream())
            {
                return compilation.Emit(stream);
            }
        }
    }
}
