using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Compilify.LanguageServices;
using Compilify.Web.Models;
using Raven.Client;

namespace Compilify.Web.Queries
{
    public class PostByIdAndVersionQuery : IQuery
    {
        private readonly IDocumentSession session;
        private readonly ICodeValidator validator;

        public PostByIdAndVersionQuery(IDocumentSession documentSession, ICodeValidator codeValidator)
        {
            session = documentSession;
            validator = codeValidator;
        }

        public Task<PostViewModel> Execute(string slug, int version)
        {
            throw new NotImplementedException();
            //var post = posts.GetVersion(slug, version);

            //if (post == null)
            //{
            //    return Task.FromResult<PostViewModel>(null);
            //}

            //var errors = GetErrorsInPost(post);

            //var viewModel = new PostViewModel(post);

            //viewModel.Errors = errors;

            //return Task.FromResult(viewModel);
        }

        private IEnumerable<EditorError> GetErrorsInPost(ICodeProgram post)
        {
            return validator.GetCompilationErrors(post);
        }
    }
}
