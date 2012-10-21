using System;
using System.Threading.Tasks;
using Raven.Client;

namespace Compilify.Web.Queries
{
    public class LatestVersionOfPostQuery : IQuery
    {
        private readonly IDocumentSession session;

        public LatestVersionOfPostQuery(IDocumentSession documentSession)
        {
            session = documentSession;
        }

        public Task<int> Execute(string slug)
        {
            throw new NotImplementedException();
        }
    }
}