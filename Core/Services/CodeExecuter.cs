using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Compilers.Common;

namespace Compilify.Services
{
    public class CodeExecuter
    {
        private static readonly string[] Namespaces =
            new[]
            {
                "System", 
                "System.IO", 
                "System.Net", 
                "System.Linq", 
                "System.Text", 
                "System.Text.RegularExpressions", 
                "System.Collections.Generic"
            };

        public const string EntryPoint = @"public class EntryPoint 
                                           {
                                               public static object Result { get; set; }
                      
                                               public static void Main()
                                               {
                                                   Result = Script.Eval();
                                               }
                                           }";

        public object Execute(string command, string classes)
        {
            var script = "public static object Eval() {" + command + "}";
            
            var compilation = CreateCompilation(
                SyntaxTree.ParseCompilationUnit(EntryPoint),
                SyntaxTree.ParseCompilationUnit(script, options: new ParseOptions(kind: SourceCodeKind.Interactive)),
                SyntaxTree.ParseCompilationUnit(classes ?? string.Empty, options: new ParseOptions(kind: SourceCodeKind.Script))
            );

            byte[] compiledAssembly;
            using (var output = new MemoryStream())
            {
                var emitResult = compilation.Emit(output);

                if (!emitResult.Success)
                {
                    var errors = emitResult.Diagnostics
                                           .Select(x => x.Info.GetMessage().Replace("Eval()", "<Factory>()"))
                                           .ToArray();
                    return string.Join(", ", errors);
                }

                compiledAssembly = output.ToArray();
            }
            
            object result;
            using (var sandbox = new Sandbox("Sandbox", compiledAssembly))
            {
                result = sandbox.Run("EntryPoint", "Result", TimeSpan.FromSeconds(5));
            }
            
            return result;
        }

        private static ICompilation CreateCompilation(params SyntaxTree[] trees)
        {
            var options = new CompilationOptions(assemblyKind: AssemblyKind.ConsoleApplication, 
                                                 usings: ReadOnlyArray<string>.CreateFrom(Namespaces));

            // Load basic .NET assemblies into our sandbox
            var mscorlib = Assembly.Load("mscorlib,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");
            var system = Assembly.Load("System,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");
            var core = Assembly.Load("System.Core,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");

            var compilation = Compilation.Create(Guid.NewGuid().ToString("N"), options,
                trees,
                new MetadataReference[] { 
                    new AssemblyFileReference(core.Location), 
                    new AssemblyFileReference(system.Location),
                    new AssemblyFileReference(mscorlib.Location)
                });

            return compilation;
        }
    }
}