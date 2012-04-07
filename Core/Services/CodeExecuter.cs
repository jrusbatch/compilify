using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Compilify.Services
{
    public class CodeExecuter
    {
        private static AppDomain CreateSandbox(string name)
        {
            var evidence = new Evidence();
            evidence.AddHostEvidence(new Zone(SecurityZone.Internet));

            var permissions = SecurityManager.GetStandardSandbox(evidence);
            var security = new SecurityPermission(SecurityPermissionFlag.Execution);

            permissions.AddPermission(security);

            var setup = new AppDomainSetup
                        {
                            ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                        };

            return AppDomain.CreateDomain(name, null, setup, permissions);
        }

        public object Execute(string code)
        {
            var sandbox = CreateSandbox("Sandbox");

            // Load basic .NET assemblies into our sandbox
            var system = sandbox.Load("System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            var core = sandbox.Load("System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            var script = "public static object Eval() {" + code + "}";
            const string entryPoint =
                @"using System.Reflection;
                  
                  public class EntryPoint 
                  {
                      public static object Result { get; set; }
                      
                      public static void Main()
                      {
                          Result = Script.Eval();
                      }
                  }";

            var namespaces = new[]
                             {
                                 "System", 
                                 "System.IO", 
                                 "System.Net", 
                                 "System.Linq", 
                                 "System.Text", 
                                 "System.Text.RegularExpressions", 
                                 "System.Collections.Generic"
                             };

            var options = new CompilationOptions(assemblyKind: AssemblyKind.ConsoleApplication, usings: ReadOnlyArray<string>.CreateFrom(namespaces));

            var compilation = Compilation.Create("foo", options,
                new[]
                {
                    SyntaxTree.ParseCompilationUnit(entryPoint),
                    // This is the syntax tree represented in the `Script` variable.
                    SyntaxTree.ParseCompilationUnit(script, options: new ParseOptions(kind: SourceCodeKind.Interactive))
                },
                new MetadataReference[] { 
                    new AssemblyFileReference(typeof(object).Assembly.Location),
                    new AssemblyFileReference(core.Location), 
                    new AssemblyFileReference(system.Location)
                });

            byte[] compiledAssembly;
            using (var output = new MemoryStream())
            {
                var emitResult = compilation.Emit(output);

                if (!emitResult.Success)
                {
                    var errors = emitResult.Diagnostics.Select(x => x.Info.GetMessage().Replace("Eval()", "<Factory>()")).ToArray();
                    return string.Join(", ", errors);
                }

                compiledAssembly = output.ToArray();
            }

            if (compiledAssembly.Length == 0)
            {
                return "Incorrect data";
            }

            var loader = (ByteCodeLoader)Activator.CreateInstance(sandbox, typeof(ByteCodeLoader).Assembly.FullName, typeof(ByteCodeLoader).FullName).Unwrap();

            object result = null;
            try
            {
                var task = Task.Factory.StartNew(() =>
                                                 {

                                                     try
                                                     {
                                                         result = loader.Run(compiledAssembly);
                                                     }
                                                     catch (Exception ex)
                                                     {
                                                         result = ex.ToString();
                                                     }
                                                 });

                if (!task.Wait(5000))
                {
                    AppDomain.Unload(sandbox);
                    result = "[Execution timed out after 6 seconds]";
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            AppDomain.Unload(sandbox);

            if (result == null || string.IsNullOrEmpty(result.ToString()))
            {
                result = "null";
            }

            return result;
        }
    }
}