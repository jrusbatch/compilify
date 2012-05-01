using System;
using System.IO;
using Compilify.Extensions;
using Compilify.Models;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Compilify.Services
{
    public class CSharpExecutor
    {
        public CSharpExecutor()
            : this(new CSharpCompilationProvider()) { }

        public CSharpExecutor(ICSharpCompilationProvider compilationProvider)
        {
            compiler = compilationProvider;
        }

        private readonly ICSharpCompilationProvider compiler;

        public object Execute(Post post)
        {
            var compilation = compiler.Compile(post);

            byte[] compiledAssembly;
            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    return "[Compilation failed]";
                }

                compiledAssembly = stream.ToArray();
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
