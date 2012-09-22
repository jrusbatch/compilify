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

        public ICodeAssembly Compile(ICodeProject program)
        {
            var compilation = RoslynCompile(program);
            var assembly = new ProgramAssembly
                           {
                               EntryPointClassName = "EntryPoint",
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

        private static ISolution CreateSolution(ICodeProject codeProject, out ProjectId projectId)
        {
            var solutionId = SolutionId.CreateNewId();
            var solution =
                Solution.Create(solutionId)
                    .AddCSharpProject(codeProject.Name, codeProject.Name, out projectId)
                    .AddMetadataReferences(projectId, DefaultReferences)
                    .UpdateCompilationOptions(projectId, DefaultCompilationOptions)
                    .UpdateParseOptions(projectId, DefaultParseOptions);

            DocumentId docId;
            return solution.AddDocument(projectId, "EntryPoint", EntryPoint, out docId);
        }

        public CommonCompilation RoslynCompile(ICodeProject codeProject)
        {
            if (codeProject == null)
            {
                throw new ArgumentNullException("codeProject");
            }

            ProjectId projectId;
            var solution = CreateSolution(codeProject, out projectId);

            var documentIds = new Dictionary<string, DocumentId>();

            foreach (var document in codeProject.Documents)
            {
                var documentId = DocumentId.CreateNewId(projectId, document.Name);

                var text = document.GetText();
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
