using System.Collections.Generic;
using Compilify.Models;

namespace Compilify
{
    public interface ICodeProgram
    {
        string Name { get; }

        IEnumerable<Reference> References { get; }
        
        IEnumerable<Document> Documents { get; }
    }
}