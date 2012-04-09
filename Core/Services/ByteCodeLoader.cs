using System;
using System.Reflection;

namespace Compilify.Services
{
    public sealed class ByteCodeLoader : MarshalByRefObject
    {
        public ByteCodeLoader() { }

        public object Run(byte[] compiledAssembly)
        {
            var assembly = Assembly.Load(compiledAssembly);
            assembly.EntryPoint.Invoke(null, new object[] { });

            var result = assembly.GetType("EntryPoint").GetProperty("Result").GetValue(null, null);
            return result;
        }
    }
}
