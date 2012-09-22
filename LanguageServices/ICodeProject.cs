using System;
using System.Collections.Generic;

namespace Compilify
{
    public interface ICodeDocument
    {
        string Name { get; }

        string GetText();
    }

    public interface ICodeProject
    {
        string Name { get; }

        string Language { get; }

        IEnumerable<ICodeDocument> Documents { get; }

        TimeSpan TimeoutPeriod { get; }
    }
}