using System;
using System.IO;
using Compilify.Models;

namespace Compilify.Services
{
    public class CSharpExecutor
    {
        private readonly ICSharpCompilationProvider compiler;

        public CSharpExecutor()
            : this(new CSharpCompilationProvider())
        {
        }

        public CSharpExecutor(ICSharpCompilationProvider compilationProvider)
        {
            compiler = compilationProvider;
        }

        public ExecutionResult Execute(Post post)
        {
            var compilation = compiler.Compile(post);

            byte[] compiledAssembly;
            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    return new ExecutionResult { Result = "[Compilation failed]" };
                }

                compiledAssembly = stream.ToArray();
            }

            using (var sandbox = new Sandbox(compiledAssembly))
            {
                return sandbox.Run("EntryPoint", "Result", TimeSpan.FromSeconds(5));
            }
        }
    }
}
