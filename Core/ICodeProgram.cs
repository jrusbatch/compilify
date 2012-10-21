using System;
using System.Collections.Generic;
using Compilify.Models;

namespace Compilify
{
    public interface ICodeProgram
    {
        string Name { get; }

        string Language { get; }

        IEnumerable<Document> Documents { get; }

        TimeSpan TimeoutPeriod { get; }
    }
}