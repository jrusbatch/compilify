using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Compilify.Services
{
    public class CSharpCompiler
    {
        public IEnumerable<string> GetCompilationErrors(string command, string classes)
        {
            var errors = new List<string>();

            var script = "public static object Eval() {" + command + "}";

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
                    SyntaxTree.ParseCompilationUnit(CodeExecuter.EntryPoint), 
                    SyntaxTree.ParseCompilationUnit(script, options: new ParseOptions(kind: SourceCodeKind.Interactive)),
                    SyntaxTree.ParseCompilationUnit(classes ?? string.Empty, options: new ParseOptions(kind: SourceCodeKind.Script))
                },
                new MetadataReference[]
                { 
                    new AssemblyFileReference(mscorlib.Location),
                    new AssemblyFileReference(core.Location), 
                    new AssemblyFileReference(system.Location)
                });

            errors.AddRange(compilation.GetDiagnostics().Select(x => x.Info.GetMessage().Replace("Eval()", "<Factory>()")));

            return errors;
        } 
    }
}