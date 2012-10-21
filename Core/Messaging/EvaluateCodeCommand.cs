using System;
using System.Collections.Generic;
using System.Linq;
using Compilify.Models;

namespace Compilify.Messaging
{
    [Serializable]
    public sealed class EvaluateCodeCommand : ICodeProgram
    {
        public EvaluateCodeCommand()
        {
            Documents = new List<Document>();
            References = new List<Reference>();
        }

        public EvaluateCodeCommand(IEnumerable<Document> documents, IEnumerable<Reference> references)
        {
            Documents = new List<Document>(documents ?? Enumerable.Empty<Document>());
            References = new List<Reference>(references ?? Enumerable.Empty<Reference>());
        }

        public string Name { get; set; }

        public string ClientId { get; set; }

        public string Result { get; set; }

        public DateTimeOffset Submitted { get; set; }

        public DateTimeOffset Expires { get; set; }

        public IList<Document> Documents { get; set; }

        public IList<Reference> References { get; set; }

        IEnumerable<Document> ICodeProgram.Documents
        {
            get { return Documents; }
        }

        IEnumerable<Reference> ICodeProgram.References
        {
            get { return References; }
        }
    }
}
