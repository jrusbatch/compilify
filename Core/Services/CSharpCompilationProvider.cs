using System;
using System.Reflection;
using System.Text;
using Compilify.Extensions;
using Compilify.Models;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Compilify.Services
{
    public interface ICSharpCompilationProvider
    {
        Compilation Compile(Post post);
    }

    public class CSharpCompilationProvider : ICSharpCompilationProvider
    {
        private const string EntryPoint = 
            @"public class EntryPoint 
              {
                  public static object Result { get; set; }
                  
                  public static void Main()
                  {
                      Result = Script.Eval();
                  }
              }";

        private static readonly ReadOnlyArray<string> DefaultNamespaces =
            ReadOnlyArray<string>.CreateFrom(new[]
            {
                "System", 
                "System.IO", 
                "System.Net", 
                "System.Linq", 
                "System.Text", 
                "System.Text.RegularExpressions", 
                "System.Collections.Generic"
            });

        public Compilation Compile(Post post)
        {
            if (post == null)
            {
                throw new ArgumentNullException("post");
            }

            
            var entryPoint = SyntaxTree.ParseCompilationUnit(EntryPoint);

            var prompt = SyntaxTree.ParseCompilationUnit(BuildScript(post.Content), fileName: "Prompt",
                                                         options: new ParseOptions(kind: SourceCodeKind.Interactive))
                                   .RewriteWith<MissingSemicolonRewriter>();

            var editor = SyntaxTree.ParseCompilationUnit(post.Classes ?? string.Empty, fileName: "Editor", 
                                                         options: new ParseOptions(kind: SourceCodeKind.Script))
                                   .RewriteWith<MissingSemicolonRewriter>();


            return Compile(post.Title ?? "Untitled", new[] { entryPoint, prompt, editor });
        }

        public Compilation Compile(string compilationName, params SyntaxTree[] syntaxTrees)
        {
            if (String.IsNullOrEmpty(compilationName))
            {
                throw new ArgumentNullException("compilationName");
            }
            
            var options = new CompilationOptions(assemblyKind: AssemblyKind.ConsoleApplication, 
                                                 usings: DefaultNamespaces);

            // Load basic .NET assemblies into our sandbox
            var mscorlib = Assembly.Load("mscorlib,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");
            var system = Assembly.Load("System,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");
            var core = Assembly.Load("System.Core,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089");

            var compilation = Compilation.Create(compilationName, options, syntaxTrees,
                                                 new MetadataReference[] { 
                                                     new AssemblyFileReference(core.Location), 
                                                     new AssemblyFileReference(system.Location),
                                                     new AssemblyFileReference(mscorlib.Location)
                                                 });

            return compilation;
        }

        private static string BuildScript(string content)
        {
            var builder = new StringBuilder();

            builder.AppendLine("public static object Eval() {");
            builder.AppendLine("#line 1");
            builder.Append(content);
            builder.AppendLine("}");

            return builder.ToString();
        }

    }
}
