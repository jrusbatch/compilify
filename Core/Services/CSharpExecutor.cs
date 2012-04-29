using System;
using System.IO;
using System.Linq;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Compilify.Services
{
    public class CSharpExecutor
    {
        public const string EntryPoint = @"public class EntryPoint 
                                           {
                                               public static object Result { get; set; }
                      
                                               public static void Main()
                                               {
                                                   Result = Script.Eval();
                                               }
                                           }";

        public CSharpExecutor()
            : this(new CSharpCompilationProvider()) { }

        public CSharpExecutor(ICSharpCompilationProvider compilationProvider)
        {
            compiler = compilationProvider;
        }

        private readonly ICSharpCompilationProvider compiler;

        public object Execute(string command, string classes)
        {
            var script = "public static object Eval() {" + command + "}";

            var name = "_" + Guid.NewGuid().ToString("N");
            
            var compilation = compiler.Compile(name,
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
    }
}
