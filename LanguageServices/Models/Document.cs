using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Compilify.Models
{
    public class Document
    {
        public Document()
        {
            LastEdited = DateTime.UtcNow;
        }

        public Document(string name, string content)
            : this()
        {
            Name = name;
            Content = content;
        }

        public string Name { get; set; }
        public string Content { get; set; }
        public DateTime LastEdited { get; set; }

        public bool IsEntryPoint
        {
            get { return Name == "Main"; }
        }
    }
}