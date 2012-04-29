using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using Compilify.Services;

namespace Compilify
{
    public sealed class Sandbox : IDisposable
    {
        public Sandbox(string name = "Sandbox")
        {
            var evidence = new Evidence();
            evidence.AddHostEvidence(new Zone(SecurityZone.Internet));

            var permissions = SecurityManager.GetStandardSandbox(evidence);
            var security = new SecurityPermission(SecurityPermissionFlag.Execution);

            permissions.AddPermission(security);

            var setup = new AppDomainSetup
                        {
                            ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            PrivateBinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            PrivateBinPathProbe = string.Empty
                        };

            //var references = new[]
            //{
            //    Assembly.Load("mscorlib,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089").Evidence.GetHostEvidence<StrongName>(),
            //    Assembly.Load("System,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089").Evidence.GetHostEvidence<StrongName>(),
            //    Assembly.Load("System.Core,Version=4.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089").Evidence.GetHostEvidence<StrongName>()
            //};
            
            var loaderType = typeof(ByteCodeLoader);
            domain = AppDomain.CreateDomain(name, null, setup, permissions /*, references */);
            loader = (ByteCodeLoader)Activator.CreateInstance(domain, loaderType.Assembly.FullName, loaderType.FullName).Unwrap();
        }

        private readonly ByteCodeLoader loader;
        private readonly AppDomain domain;
        private bool disposed;

        public string Name
        {
            get { return domain.FriendlyName; }
        }

        public AppDomain Domain
        {
            get { return domain; }
        }

        public ByteCodeLoader Loader
        {
            get { return loader; }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                AppDomain.Unload(domain);
            }
        }
    }
}
