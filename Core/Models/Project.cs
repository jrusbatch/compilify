using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilify.Models
{
    public class Project : ICodeProgram
    {
        public static readonly IEnumerable<Reference> DefaultReferences =
            new List<Reference>
            {
                new Reference { AssemblyName = "mscorlib", Version = "4.0.0.0" },
                new Reference { AssemblyName = "System", Version = "4.0.0.0" },
                new Reference { AssemblyName = "System.Core", Version = "4.0.0.0" }
            };

        public Project()
        {
            Id = Guid.NewGuid().ToString("N");
            Documents = new List<Document>();
            References = new List<Reference>(DefaultReferences);
            Created = DateTimeOffset.UtcNow;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public IList<Document> Documents { get; set; }
        public IList<Reference> References { get; set; }

        string ICodeProgram.Name
        {
            get { return Id; }
        }

        IEnumerable<Document> ICodeProgram.Documents
        {
            get { return Documents; }
        }

        IEnumerable<Reference> ICodeProgram.References
        {
            get { return References; }
        } 

        public DateTimeOffset Created { get; set; }

        public Project AddOrUpdate(Document document)
        {
            if (document != null)
            {
                if (Documents.Any(x => x.Name == document.Name))
                {
                    var existing = Documents.First(x => x.Name == document.Name);

                    // update only if the edit is newer
                    if (document.LastEdited > existing.LastEdited)
                    {
                        existing.Content = document.Content;
                        existing.LastEdited = document.LastEdited;
                    }
                }
                else
                {
                    Documents.Add(document);
                }
            }

            return this;
        }

        public Project Remove(string name)
        {
            Documents = Documents.Where(x => x.Name != name).ToList();
            return this;
        }

        public string Content
        {
            get
            {
                var main = Documents.FirstOrDefault(x => x.IsEntryPoint);
                return main == null ? string.Empty : main.Content;
            }
        }

        public string Classes
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var document in Documents.Where(x => !x.IsEntryPoint))
                {
                    sb.AppendLine(document.Content);
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }
    }
}
