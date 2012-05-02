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
        private const string DefaultName = "Sandbox";

        public Sandbox(byte[] compiledAssemblyBytes)
            : this(DefaultName, compiledAssemblyBytes) { }

        public Sandbox(string name, byte[] compiledAssemblyBytes)
        {
            assemblyBytes = compiledAssemblyBytes;

            var evidence = new Evidence();
            evidence.AddHostEvidence(new Zone(SecurityZone.Internet));

            var permissions = SecurityManager.GetStandardSandbox(evidence);
            permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));

            var setup = new AppDomainSetup
                        {
                            ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            ApplicationName = name,
                            DisallowBindingRedirects = true,
                            DisallowCodeDownload = true,
                            DisallowPublisherPolicy = true
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

        private object Execute(string className, string resultProperty)
        {
            var type = typeof(ByteCodeLoader);

            try
            {
                var loader = (ByteCodeLoader)domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);

                var result = loader.Run(className, resultProperty, assemblyBytes);

                return result;
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

        private sealed class ByteCodeLoader : MarshalByRefObject
        {
            public ByteCodeLoader() { }

            public object Run(string className, string resultProperty, byte[] compiledAssembly)
            {
                var assembly = Assembly.Load(compiledAssembly);
                assembly.EntryPoint.Invoke(null, new object[] { });

                return assembly.GetType(className).GetProperty(resultProperty).GetValue(null, null);
            }
        }
    }
}
