using System;
using System.Reflection;

namespace Compilify.Services
{
    public sealed class ByteCodeLoader : MarshalByRefObject
    {
        public ByteCodeLoader() { }

        internal object Run(string className, string resultProperty, byte[] compiledAssembly)
        {
            var assembly = Assembly.Load(compiledAssembly);
            assembly.EntryPoint.Invoke(null, new object[] { });
            
            var result = assembly.GetType(className).GetProperty(resultProperty).GetValue(null, null);
            return result;
        }
    }
}
