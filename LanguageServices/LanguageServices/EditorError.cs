using System.Runtime.Serialization;

namespace Compilify.LanguageServices
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