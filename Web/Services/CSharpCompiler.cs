using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Compilify.Web.Services
{
    public class CSharpCompiler
    {
        public IEnumerable<Diagnostic> GetCompilationErrors(string code)
        {
            var errors = new List<Diagnostic>();

            var mscorlib = Assembly.Load("mscorlib,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");
            var system = Assembly.Load("System,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");
            var core = Assembly.Load("System.Core,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");

            var namespaces = ReadOnlyArray<string>.CreateFrom(new[]
                             {
                                 "System", 
                                 "System.IO", 
                                 "System.Net", 
                                 "System.Linq", 
                                 "System.Text", 
                                 "System.Text.RegularExpressions", 
                                 "System.Collections.Generic"
                             });

            var compilation = Compilation.Create("foo", 
                new CompilationOptions(assemblyKind: AssemblyKind.DynamicallyLinkedLibrary, usings: namespaces),
                new[]
                {
                    SyntaxTree.ParseCompilationUnit(code, options: new ParseOptions(languageVersion: LanguageVersion.CSharp6, kind: SourceCodeKind.Interactive))
                },
                new MetadataReference[]
                { 
                    new AssemblyFileReference(mscorlib.Location),
                    new AssemblyFileReference(core.Location), 
                    new AssemblyFileReference(system.Location)
                });

            using (var output = new MemoryStream())
            {
                var emitResult = compilation.Emit(output);

                if (!emitResult.Success)
                {
                    errors.AddRange(emitResult.Diagnostics);
                }
            }

            return errors;
        } 
    }
}