using System;
using System.Runtime.Serialization;

namespace Compilify.Web.Infrastructure
{
    [Serializable]
    public class ServiceNotFoundException : CompilifyException
    {
        public const string DefaultMessage =
            "A concrete implementation of the service \"{0}\" could not be found.";

        private readonly Type type;

        public ServiceNotFoundException(Type serviceType)
            : this(serviceType, string.Format(DefaultMessage, serviceType.FullName)) { }

        public ServiceNotFoundException(Type serviceType, string message)
            : base(message)
        {
            type = serviceType;
        }

        public ServiceNotFoundException(Type serviceType, string message, Exception inner)
            : base(message, inner)
        {
            type = serviceType;
        }

        protected ServiceNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public Type Type
        {
            get { return type; }
        }
    }
}