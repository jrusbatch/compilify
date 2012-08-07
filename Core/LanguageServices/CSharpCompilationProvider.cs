using System;
using System.Text;
using Compilify.Extensions;
using Compilify.Models;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace Compilify.LanguageServices
{
    public interface ICSharpCompilationProvider
    {
        Compilation Compile(Post post);
    }

    public class CSharpCompilationProvider : ICSharpCompilationProvider
    {
        private const string EntryPoint = 
            @"public static class EntryPoint 
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

            var asScript = ParseOptions.Default.WithKind(SourceCodeKind.Script);

            var console =
                SyntaxTree.ParseCompilationUnit(
                    "public static readonly StringWriter __Console = new StringWriter();", options: asScript);

            var entry = SyntaxTree.ParseCompilationUnit(EntryPoint);

            var prompt = SyntaxTree.ParseCompilationUnit(BuildScript(post.Content), path: "Prompt", options: asScript);

            var editor = SyntaxTree.ParseCompilationUnit(post.Classes ?? string.Empty, path: "Editor", options: asScript);

            var compilation = Compile(post.Title ?? "Untitled", new[] { entry, prompt, editor, console });

            var newPrompt = prompt.RewriteWith(new ConsoleRewriter("__Console", compilation.GetSemanticModel(prompt)));
            var newEditor = editor.RewriteWith(new ConsoleRewriter("__Console", compilation.GetSemanticModel(editor)));

            return compilation.ReplaceSyntaxTree(prompt, newPrompt).ReplaceSyntaxTree(editor, newEditor);
        }

        public Compilation Compile(string compilationName, params SyntaxTree[] syntaxTrees)
        {
            if (string.IsNullOrEmpty(compilationName))
            {
                throw new ArgumentNullException("compilationName");
            }
            
            var options = new CompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithUsings(DefaultNamespaces);

            var metadata = new[]
                           {
                               MetadataReference.Create("mscorlib"),
                               MetadataReference.Create("System"),
                               MetadataReference.Create("System.Core"),
                           };

            var compilation = Compilation.Create(compilationName, options, syntaxTrees,  metadata);

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
