using System.Threading.Tasks;
using Compilify.Models;
using Raven.Client;

namespace Compilify.Web.Commands
{
    public class SavePostCommand : ICommand
    {
        private readonly IAsyncDocumentSession session;

        public SavePostCommand(IAsyncDocumentSession documentSession)
        {
            session = documentSession;
        }

        public async Task<Project> Execute(Project project)
        {
            session.Store(project);
            await session.SaveChangesAsync();
            return project;
        }
    }
}