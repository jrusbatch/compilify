using System.Collections.Generic;
using Compilify.Models;

namespace Compilify.LanguageServices
{
    public interface ICodeValidator
    {
        IEnumerable<EditorError> GetCompilationErrors(ICodeProgram program);
    }
}