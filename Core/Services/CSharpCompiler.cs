using System.Collections.Generic;
using System.Reflection;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Compilify.Services
{
    public class CSharpCompiler
    {
        public IEnumerable<Diagnostic> GetCompilationErrors(string code)
        {
            var errors = new List<Diagnostic>();

            var script = "public static object Eval() {" + code + "}";
            var tree = SyntaxTree.ParseCompilationUnit(script, options: new ParseOptions(kind: SourceCodeKind.Interactive));

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
                new[] { SyntaxTree.ParseCompilationUnit(CodeExecuter.EntryPoint), tree },
                new MetadataReference[]
                { 
                    new AssemblyFileReference(mscorlib.Location),
                    new AssemblyFileReference(core.Location), 
                    new AssemblyFileReference(system.Location)
                });

            errors.AddRange(compilation.GetDiagnostics());

            return errors;
        } 
    }
}