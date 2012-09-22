using System.Diagnostics;

namespace Compilify.Models
{
    [DebuggerDisplay("{Name}, {text}")]
    public class Document : ICodeDocument
    {
        private readonly string text;

        public Document(string name, string text)
        {
            Name = name;
            this.text = text;
        }

        public string Name { get; private set; }

        public string GetText()
        {
            return text;
        }
    }
}