using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Compilify.Models;

namespace Compilify.Messaging
{
    [Serializable]
    public sealed class EvaluateCodeCommand : ICodeProgram
    {
        public EvaluateCodeCommand()
        {
            Documents = new List<Document>();
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
            get { return new ReadOnlyCollection<Document>(Documents); }
        }

        IEnumerable<Reference> ICodeProgram.References
        {
            get { return new ReadOnlyCollection<Reference>(References); }
        }
    }
}
