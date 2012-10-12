using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace Compilify.LanguageServices
{
    public class CSharpCompiler : ICodeCompiler
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

        private const string Console = "public static readonly StringWriter __Console = new StringWriter();";

        private static readonly IEnumerable<string> DefaultNamespaces =
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

        private static readonly IEnumerable<MetadataReference> DefaultReferences =
            new[]
            {
                MetadataReference.CreateAssemblyReference("mscorlib"),
                MetadataReference.CreateAssemblyReference("System"),
                MetadataReference.CreateAssemblyReference("System.Core")
            };

        private static readonly CommonParseOptions DefaultParseOptions = 
            ParseOptions.Default.WithKind(SourceCodeKind.Script);

        private static readonly CompilationOptions DefaultCompilationOptions =
            new CompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOverflowChecks(true)
                    .WithOptimizations(true)
                    .WithUsings(DefaultNamespaces);

        public ICodeAssembly Compile(ICodeProgram program)
        {
            var compilation = RoslynCompile(program);
            var assembly = new ProgramAssembly
                           {
                               EntryPointClassName = "Script+EntryPoint",
                               EntryPointMethodName = "Main"
                           };

            using (var stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);

                if (!emitResult.Success)
                {
                    return null;
                }

                assembly.CompiledAssembly = stream.ToArray();
            }

            return assembly;
        }

        private static ISolution CreateSolution(ICodeProgram codeProgram, out ProjectId projectId)
        {
            DocumentId entryPointDocumentId;
            DocumentId consoleDocumentId;

            var solutionId = SolutionId.CreateNewId();

            return
                Solution.Create(solutionId)
                    .AddCSharpProject(codeProgram.Name ?? "Untitled", codeProgram.Name ?? "Untitled", out projectId)
                    .AddMetadataReferences(projectId, DefaultReferences)
                    .UpdateCompilationOptions(projectId, DefaultCompilationOptions)
                    .UpdateParseOptions(projectId, DefaultParseOptions)
                    .AddDocument(projectId, "EntryPoint", EntryPoint, out entryPointDocumentId)
                    .AddDocument(projectId, "Console", Console, out consoleDocumentId);
        }

        public CommonCompilation RoslynCompile(ICodeProgram codeProgram)
        {
            if (codeProgram == null)
            {
                throw new ArgumentNullException("codeProgram");
            }

            ProjectId projectId;
            var solution = CreateSolution(codeProgram, out projectId);

            var documentIds = new Dictionary<string, DocumentId>();

            foreach (var document in codeProgram.Documents)
            {
                var documentId = DocumentId.CreateNewId(projectId, document.Name);

                var text = document.Content;
                if (string.Equals(document.Name, "Content"))
                {
                    // This smells terrible.
                    text = BuildScript(text);
                }

                solution = solution.AddDocument(documentId, document.Name, new StringText(text));
                documentIds.Add(document.Name, documentId);
            }

            foreach (var documentId in solution.GetProject(projectId).DocumentIds)
            {
                var document = solution.GetDocument(documentId);
                var root = document.GetSyntaxRoot();
                var semanticModel = document.GetSemanticModel();

                var rewriter = new ConsoleRewriter("__Console", semanticModel);

                solution = solution.UpdateDocument(documentId, rewriter.Visit((SyntaxNode)root));
            }

            return solution.GetProject(projectId).GetCompilation();
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
