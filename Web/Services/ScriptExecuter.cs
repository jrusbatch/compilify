using System;
using System.Collections.Generic;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace Compilify.Web.Services
{
    public class ScriptExecuter : MarshalByRefObject
    {
        private static readonly ScriptExecuter Host = new ScriptExecuter();

        public string Execute(string code)
        {
            var engine = new ScriptEngine(new [] { "System" }, new[] { "System" });
            
            var session = Session.Create(Host);
            //session.SetReferenceSearchPaths(new string[0]);
            try
            {
                if (!Validate(code))
                {
                    return "Not implemeted";
                }

                var result = engine.Execute(code, session);
                if (result != null)
                {
                    return result.ToString();
                }
            }
            catch (CompilationErrorException ex)
            {
                return ex.ToString();
            }

            return null;
        }

        public bool Validate(string code)
        {
            var syntax = Syntax.ParseStatement(code);
            return ScanSyntax(syntax.ChildNodes());
        }

        public bool ScanSyntax(IEnumerable<SyntaxNode> syntaxNodes)
        {
            foreach (var syntaxNode in syntaxNodes)
            {
                if (syntaxNode.HasChildren)
                {
                    if (!ScanSyntax(syntaxNode.ChildNodes()))
                    {
                        return false;
                    }
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
