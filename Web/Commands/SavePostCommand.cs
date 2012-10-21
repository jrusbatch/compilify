using System;
using System.Threading.Tasks;
using Compilify.Models;
using Raven.Client;

namespace Compilify.Web.Commands
{
    public class SavePostCommand : ICommand
    {
        private readonly IDocumentSession session;

        public SavePostCommand(IDocumentSession documentSession)
        {
            session = documentSession;
        }

        public Task<Project> Execute(string slug, Project postViewModel)
        {
            throw new NotImplementedException();

            // var result = posts.Save(slug, postViewModel.ToPost());

            // return Task.FromResult(result);
        }
    }
}