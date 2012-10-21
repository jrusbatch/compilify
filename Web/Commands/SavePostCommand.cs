using System;
using System.Threading.Tasks;
using Compilify.Models;
using Compilify.Web.Models;
using Raven.Client;

namespace Compilify.Web.Commands
{
    public class SavePostCommand : ICommand
    {
        private readonly IDocumentSession posts;

        public SavePostCommand(IDocumentSession postRepository)
        {
            posts = postRepository;
        }

        public Task<Post> Execute(string slug, PostViewModel postViewModel)
        {
            throw new NotImplementedException();

            // var result = posts.Save(slug, postViewModel.ToPost());

            // return Task.FromResult(result);
        }
    }
}