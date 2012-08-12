using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using SignalR;

namespace Compilify.Web.Infrastructure.DependencyInjection
{
    public class AutofacDepenedencyResolverForSignalR : DefaultDependencyResolver, IRegistrationSource
    {
        private readonly ILifetimeScope lifetimeScope;

        public AutofacDepenedencyResolverForSignalR(ILifetimeScope lifetimeScope)
        {
            this.lifetimeScope = lifetimeScope;
            this.lifetimeScope.ComponentRegistry.AddRegistrationSource(this);
        }

        public override object GetService(Type serviceType)
        {
            object result;
            if (lifetimeScope.TryResolve(serviceType, out result))
            {
                return result;
            }

            return null;
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            object result;
            if (lifetimeScope.TryResolve(typeof(IEnumerable<>).MakeGenericType(serviceType), out result))
            {
                return (IEnumerable<object>)result;
            }

            return Enumerable.Empty<object>();
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var typedService = service as TypedService;
            if (typedService != null)
            {
                var instances = base.GetServices(typedService.ServiceType);

                if (instances != null)
                {
                    return instances
                            .Select(i => RegistrationBuilder.ForDelegate(i.GetType(), (c, p) => i).As(typedService.ServiceType)
                            .InstancePerMatchingLifetimeScope(lifetimeScope.Tag)
                            .PreserveExistingDefaults()
                            .CreateRegistration());
                }
            }

            return Enumerable.Empty<IComponentRegistration>();
        }

        bool IRegistrationSource.IsAdapterForIndividualComponents
        {
            get { return false; }
        }
    }
}