using System.Runtime.Serialization;

namespace Compilify
{
    [DataContract]
    public class EditorError
    {
        [DataMember]
        public DocumentLineSpan Location { get; set; }
        
        [DataMember]
        public string Message { get; set; }
    }
}