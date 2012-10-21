using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Compilify.LanguageServices;
using Compilify.Models;
using Raven.Client;

namespace Compilify.Web.Queries
{
    public class ProjectByIdQuery : IQuery
    {
        private readonly IAsyncDocumentSession session;

        public ProjectByIdQuery(IAsyncDocumentSession documentSession)
        {
            session = documentSession;
        }

        public Task<Project> Execute(string id)
        {
            return session.LoadAsync<Project>(id);
        }
    }
}