using System;

namespace Compilify.Models
{
    public class Document
    {
        public Document()
        {
            Name = string.Empty;
            Content = string.Empty;
            LastEdited = DateTimeOffset.UtcNow;
        }

        public Document(string name, string content)
            : this()
        {
            Name = name ?? string.Empty;
            Content = content ?? string.Empty;
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