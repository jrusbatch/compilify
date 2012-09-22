using Compilify.Models;
using Roslyn.Compilers.CSharp;

namespace Compilify.LanguageServices
{
    public interface ICodeCompiler
    {
        ICodeAssembly Compile(ICodeProject job);
    }
}