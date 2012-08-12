using System;

namespace Compilify
{
    public interface ICodeProgram
    {
        string Name { get; }

        string Language { get; }

        string Content { get; }

        string Classes { get; }

        TimeSpan TimeoutPeriod { get; }
    }
}