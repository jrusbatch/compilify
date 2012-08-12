using System;
using System.Linq;
using Compilify.Models;

namespace Compilify.LanguageServices
{
    public interface ICodeExecutor : IDisposable
    {
        ExecutionResult Execute(ICodeAssembly assembly, TimeSpan timeout);
    }
}
