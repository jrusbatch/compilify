using System;

namespace Compilify.Models
{
    public class Document
    {
        public Document()
        {
            LastEdited = DateTimeOffset.UtcNow;
        }

        public Document(string name, string content)
            : this()
        {
            Name = name;
            Content = content;
        }

        public string Name { get; set; }
        public string Content { get; set; }
        public DateTimeOffset LastEdited { get; set; }

        public bool IsEntryPoint
        {
            get { return Name == "Main"; }
        }
    }
}