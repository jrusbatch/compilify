using System.Threading.Tasks;
using Compilify.Models;

namespace Compilify.Web.Queries
{
    public class LatestVersionOfPostQuery : IQuery
    {
        private readonly IPostRepository posts;

        public LatestVersionOfPostQuery(IPostRepository postRepository)
        {
            posts = postRepository;
        }

        public Task<int> Execute(string slug)
        {
            var version = posts.GetLatestVersion(slug);
            return Task.FromResult(version);
        }
    }
}