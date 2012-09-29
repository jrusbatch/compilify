using System.Threading.Tasks;
using Compilify.Models;
using Compilify.Web.Models;

namespace Compilify.Web.Commands
{
    public class SavePostCommand : ICommand
    {
        private readonly IPostRepository posts;

        public SavePostCommand(IPostRepository postRepository)
        {
            posts = postRepository;
        }

        public Task<Post> Execute(string slug, PostViewModel postViewModel)
        {
            var result = posts.Save(slug, postViewModel.ToPost());

            return Task.FromResult(result);
        }
    }
}