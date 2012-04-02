using System;
using System.Collections.Generic;
using Roslyn.Compilers.CSharp;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace Compilify.Services
{
    public class ScriptExecuter : MarshalByRefObject
    {
        static readonly ScriptExecuter Host = new ScriptExecuter();

        public string Execute(string code)
        {
            var engine = new ScriptEngine(new [] { "System" });
            
            var session = Session.Create(Host);
            engine.Execute("using System;", session);
            try
            {
                if (!Validate(code))
                {
                    return "Not implemeted";
                }

                var result = engine.Execute(code, session);
                if(result != null)
                {
                    return result.ToString();
                }
            }
            catch (Exception ex)
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
                    if (!ScanSyntax(syntaxNode.ChildNodes())) return false;
                }

                Console.WriteLine(syntaxNode.GetType());
                if (syntaxNode is QualifiedNameSyntax || syntaxNode is InvocationExpressionSyntax && ((InvocationExpressionSyntax)syntaxNode).Expression is MemberAccessExpressionSyntax)
                {
                    return false;
                }
            }

            return true;
        }
    }
}