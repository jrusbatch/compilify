using System.Diagnostics;
using System.Runtime.Serialization;

namespace Compilify
{
    [DebuggerDisplay("{Name}, {Text}")]
    public class Document
    {
        public Document(string name, string text)
        {
            Name = name;
            Text = text;
        }

        [DataMember(Order = 1)]
        public string Name { get; set; }

        [DataMember(Order = 2)]
        public string Text { get; set; }

        public string GetText()
        {
            return Text;
        }
    }
}