using System.Collections.Generic;
using Roslyn.Compilers.CSharp;

namespace Compilify.Services
{
    public static class Validator
    {
        public static bool Validate(string code)
        {
            var syntax = Syntax.ParseStatement(code);
            return ScanSyntax(syntax.ChildNodes());
        }

        public static bool ScanSyntax(IEnumerable<SyntaxNode> syntaxNodes)
        {
            foreach (var syntaxNode in syntaxNodes)
            {
                if (syntaxNode.HasChildren && !ScanSyntax(syntaxNode.ChildNodes()))
                {
                    return false;
                }

                if (syntaxNode is QualifiedNameSyntax || 
                    syntaxNode is InvocationExpressionSyntax && 
                    ((InvocationExpressionSyntax)syntaxNode).Expression is MemberAccessExpressionSyntax)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
