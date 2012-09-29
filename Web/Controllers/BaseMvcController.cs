using System.Web.Mvc;
using Compilify.Web.Infrastructure;

namespace Compilify.Web.Controllers
{
    public abstract class BaseMvcController : AsyncController
    {
        protected virtual TService Resolve<TService>() where TService : class
        {
            var service = DependencyResolver.Current.GetService<TService>();

            if (ReferenceEquals(null, service))
            {
                throw new ServiceNotFoundException(typeof(TService));
            }

            return service;
        }
    }
}
