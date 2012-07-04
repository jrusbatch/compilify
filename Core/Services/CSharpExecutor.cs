using System;
using System.IO;
using Compilify.Models;

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

        public ExecutionResult Execute(Post post)
        {
            var compilation = compiler.Compile(post);

            byte[] compiledAssembly;
            using (var stream = new MemoryStream())
            using (var fileStream = new FileStream("C:\\output2.dll", FileMode.Truncate))
            {
                var emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    return new ExecutionResult { Result = "[Compilation failed]" };
                }

                compilation.Emit(fileStream);

                compiledAssembly = stream.ToArray();
            }

            using (var sandbox = new Sandbox(compiledAssembly))
            {
                return sandbox.Run("EntryPoint", "Result", TimeSpan.FromSeconds(5));
            }
        }
    }
}
