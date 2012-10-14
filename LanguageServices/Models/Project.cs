using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compilify.Models
{
    public class Project : ICodeProgram
    {
        public Project()
        {
            Id = Guid.NewGuid().ToString("N");
            Documents = new List<Document>();
            References = new List<Reference>();
            Created = DateTime.UtcNow;
        }

        public string Id { get; set; }
        public IList<Document> Documents { get; set; }

        public IList<Reference> References { get; set; }

        IEnumerable<Document> ICodeProgram.Documents
        {
            get { return Documents; }
        } 

        public DateTime Created { get; set; }

        public Project AddOrUpdate(Document document)
        {
            if (document == null)
                return this;

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

            return this;
        }

        public Project Remove(string name)
        {
            Documents = Documents.Where(x => x.Name != name).ToList();
            return this;
        }

        string ICodeProgram.Name
        {
            get { return Id; }
        }

        public string Language { get; private set; }

        public string Content
        {
            get
            {
                var main = Documents.FirstOrDefault(x => x.IsEntryPoint);
                return main == null ? "" : main.Content;
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

        TimeSpan ICodeProgram.TimeoutPeriod
        {
            get { return TimeSpan.FromSeconds(5D); }
        }
    }

    public class Reference
    {
        public string Name { get; set; }

        public string Version { get; set; }
    }
}
