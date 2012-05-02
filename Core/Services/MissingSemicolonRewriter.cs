using Roslyn.Compilers.CSharp;

namespace Compilify.Services
{
    internal sealed class MissingSemicolonRewriter : SyntaxRewriter
    {
        protected override SyntaxToken VisitToken(SyntaxToken token)
        {
            if (token.IsMissing && token.Kind == SyntaxKind.SemicolonToken)
            {
                return Syntax.Token(SyntaxKind.SemicolonToken);
            }

            return token;
        }
    }
}
