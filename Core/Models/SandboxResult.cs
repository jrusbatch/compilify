using System;

namespace Compilify.Models
{
    [Serializable]
    public class SandboxResult
    {
        public string ConsoleOutput { get; set; }
        public object ReturnValue { get; set; }
    }
}