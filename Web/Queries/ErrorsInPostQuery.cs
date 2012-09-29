using System.Collections.Generic;
using Compilify.LanguageServices;
using Compilify.Models;

namespace Compilify.Web.Queries
{
    public class ErrorsInPostQuery : IQuery
    {
        private readonly ICodeValidator validator;

        public ErrorsInPostQuery(ICodeValidator codeValidator)
        {
            validator = codeValidator;
        }

        public IEnumerable<EditorError> Execute(Post post)
        {
            return validator.GetCompilationErrors(post);
        }
    }
}