using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Compilify.LanguageServices
{
    public class CSharpCompilerTests
    {
        [Fact] // NOTE: This test will fail when executed by NCrunch.
        public void CodeCanBeCompiledAndExecuted()
        {
            var classes = new Document("Classes", "public static void SayHello() { Console.WriteLine(\"Hello, world!\"); }");
            var content = new Document("Content", "SayHello(); return \"Done!\";");

            var command = new EvaluateCodeCommand
                          {
                              ClientId = Guid.NewGuid().ToString("N"),
                              Documents = new List<Document> { content, classes },
                              Name = "Untitled",
                              Submitted = DateTime.UtcNow,
                              TimeoutPeriod = TimeSpan.FromSeconds(30)
                          };

            var sandbox = new Sandbox();
            var compiler = new CSharpCompiler();

            var actual = compiler.Compile(command);

            Assert.NotNull(actual);

            var result = sandbox.Execute(actual, Timeout.InfiniteTimeSpan);

            Assert.NotNull(result);
            Assert.Equal("Hello, world!\r\n", result.ConsoleOutput);
            Assert.Equal("\"Done!\"", result.Result);
        }
    }
}
