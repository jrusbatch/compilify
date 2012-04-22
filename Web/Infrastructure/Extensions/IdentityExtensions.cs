using System.Security.Principal;

namespace Compilify.Web.Infrastructure.Extensions
{
    public static class IdentityExtensions
    {
        public static CompilifyIdentity ToCompilifyIdentity(this IIdentity identity)
        {
            return (CompilifyIdentity)identity;
        }
    }
}