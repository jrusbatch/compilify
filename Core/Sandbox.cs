using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;
using Compilify.Services;

namespace Compilify
{
    public sealed class Sandbox : IDisposable
    {
        public Sandbox(string name, byte[] compiledAssemblyBytes)
        {
            assemblyBytes = compiledAssemblyBytes;

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

            domain = AppDomain.CreateDomain(name, null, setup, permissions);
        }
        
        private readonly byte[] assemblyBytes;
        private readonly AppDomain domain;
        private bool disposed;

        public object Run(string className, string resultProperty, TimeSpan timeout)
        {
            var task = Task<object>.Factory.StartNew(() => Execute(className, resultProperty));

            if (!task.Wait(timeout))
            {
                return "[Execution timed out]";
            }

            return task.Result ?? "null";
        }

        public object Execute(string className, string resultProperty)
        {
            try
            {
                var type = typeof(ByteCodeLoader);
                var loader = (ByteCodeLoader)Activator.CreateInstance(domain, type.Assembly.FullName, type.FullName).Unwrap();
                return loader.Run(className, resultProperty, assemblyBytes);
            }
            catch (SerializationException ex)
            {
                return ex.Message;
            }
            catch (TargetInvocationException ex)
            {
                return ex.InnerException.ToString();
            }
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
