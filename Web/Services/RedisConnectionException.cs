using System;
using System.Runtime.Serialization;

namespace Compilify.Web.Services
{
    [Serializable]
    public class RedisConnectionException : Exception
    {
        public RedisConnectionException() { }

        public RedisConnectionException(string message) 
            : base(message) { }

        public RedisConnectionException(string message, Exception inner) 
            : base(message, inner) { }

        protected RedisConnectionException(SerializationInfo info, StreamingContext context) 
            : base(info, context) { }
    }
}