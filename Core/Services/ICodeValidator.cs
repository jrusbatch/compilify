using System.Collections.Generic;
using Compilify.Models;

namespace Compilify.Services
{
    public interface ICodeValidator
    {
        IEnumerable<EditorError> GetCompilationErrors(Post post);
    }
}