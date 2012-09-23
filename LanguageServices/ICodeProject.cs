using System;
using System.Collections.Generic;

namespace Compilify
{
    public interface ICodeProject
    {
        string Name { get; }

        string Language { get; }

        IEnumerable<Document> Documents { get; }

        TimeSpan TimeoutPeriod { get; }
    }
}