using Roslyn.Compilers.CSharp;

namespace Compilify.Extensions
{
    internal static class SyntaxTreeExtensions
    {
        internal static SyntaxTree RewriteWith<TRewriter>(this SyntaxTree tree) where TRewriter : SyntaxRewriter, new()
        {
            var rewriter = new TRewriter();
            return SyntaxTree.Create(tree.FileName, (CompilationUnitSyntax)rewriter.Visit(tree.Root), tree.Options);
        }
    }
}