using System;
using System.Reflection;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Compilify.Services
{
    public interface ICSharpCompilationProvider
    {
        Compilation Compile(string compilationName, params SyntaxTree[] syntaxTrees);
    }

    public class CSharpCompilationProvider : ICSharpCompilationProvider
    {
        private static readonly ReadOnlyArray<string> DefaultNamespaces =
            ReadOnlyArray<string>.CreateFrom(new[]
            {
                "System", 
                "System.IO", 
                "System.Net", 
                "System.Linq", 
                "System.Text", 
                "System.Text.RegularExpressions", 
                "System.Collections.Generic"
            });

        public Compilation Compile(string compilationName, params SyntaxTree[] syntaxTrees)
        {
            if (string.IsNullOrEmpty(compilationName))
            {
                throw new ArgumentNullException("compilationName");
            }
            
            var options = new CompilationOptions(assemblyKind: AssemblyKind.ConsoleApplication, 
                                                 usings: DefaultNamespaces);

            // Load basic .NET assemblies into our sandbox
            var mscorlib = Assembly.Load("mscorlib,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");
            var system = Assembly.Load("System,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");
            var core = Assembly.Load("System.Core,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");

            var compilation = Compilation.Create(compilationName, options, syntaxTrees,
                                                 new MetadataReference[] { 
                                                     new AssemblyFileReference(core.Location), 
                                                     new AssemblyFileReference(system.Location),
                                                     new AssemblyFileReference(mscorlib.Location)
                                                 });

            return compilation;
        }
    }
}
