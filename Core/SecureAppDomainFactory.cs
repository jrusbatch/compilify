using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace Compilify
{
    public class SecureAppDomainFactory
    {
        public static AppDomain Create()
        {
            return Create("Sandbox");
        }

        public static AppDomain Create(string name)
        {
            var evidence = new Evidence();
            evidence.AddHostEvidence(new Zone(SecurityZone.Internet));

            var permissions = SecurityManager.GetStandardSandbox(evidence);
            var security = new SecurityPermission(SecurityPermissionFlag.Execution);

            permissions.AddPermission(security);

            var setup = new AppDomainSetup
            {
                ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            };

            return AppDomain.CreateDomain(name, null, setup, permissions);
        }
    }
}
