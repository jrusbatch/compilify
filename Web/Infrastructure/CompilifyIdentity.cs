using System;
using System.Security.Principal;
using System.Web.Security;

namespace Compilify.Web.Infrastructure
{
    [Serializable]
    public sealed class CompilifyIdentity : IIdentity
    {
        public CompilifyIdentity(FormsAuthenticationTicket authenticationTicket)
        {
            ticket = authenticationTicket;

            Guid id;
            userId = Guid.TryParse(ticket.UserData, out id) ? id : default(Guid);
        }

        private readonly FormsAuthenticationTicket ticket;
        private readonly Guid userId;

        public string Name
        {
            get { return ticket.Name; }
        }

        public string AuthenticationType
        {
            get { return "Custom"; }
        }

        public bool IsAuthenticated
        {
            get { return ticket != null; }
        }

        public Guid UserId
        {
            get { return userId; }
        }
    }
}