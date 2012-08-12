using System;
using System.Runtime.Serialization;
using Compilify.Utilities;
using Roslyn.Compilers;

namespace Compilify.LanguageServices
{
    [DataContract]
    public struct DocumentLineSpan : IEquatable<DocumentLineSpan>
    {
        private readonly string name;
        private readonly TextPosition start;
        private readonly TextPosition end;

        private DocumentLineSpan(string documentName, TextPosition startPosition, TextPosition endPosition)
        {
            name = documentName;
            start = startPosition;
            end = endPosition;
        }
        
        internal static DocumentLineSpan Create(FileLinePositionSpan span)
        {
            var startPosition = TextPosition.Create(span.StartLinePosition);
            var endPosition = TextPosition.Create(span.EndLinePosition);
            return new DocumentLineSpan(span.Path, startPosition, endPosition);
        }

        [DataMember]
        public string DocumentName
        {
            get { return name; }
        }

        [DataMember]
        public TextPosition StartLinePosition
        {
            get { return start; }
        }
        
        [DataMember]
        public TextPosition EndLinePosition
        {
            get { return end; }
        }

        public override bool Equals(object obj)
        {
            return obj is DocumentLineSpan && Equals((DocumentLineSpan)obj);
        }

        public bool Equals(DocumentLineSpan other)
        {
            return start == other.start && end == other.end;
        }

        public override int GetHashCode()
        {
            return Hash.Combine(name, Hash.Combine(start, end.GetHashCode()));
        }
    }
}
