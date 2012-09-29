using System;
using System.Runtime.Serialization;

namespace Compilify
{
    [Serializable]
    public class CompilifyException : Exception
    {
        public CompilifyException() { }

        public CompilifyException(string message)
            : base(message) { }

        public CompilifyException(string message, Exception inner)
            : base(message, inner) { }

        protected CompilifyException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
