using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;
using Compilify.Models;
using Compilify.Services;
using Roslyn.Scripting.CSharp;

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

        static Sandbox()
        {
            AppDomain.MonitoringIsEnabled = true;
        }

        private readonly byte[] assemblyBytes;
        private readonly AppDomain domain;
        private bool disposed;

        public ExecutionResult Run(string className, string resultProperty, TimeSpan timeout)
        {
            var task = Task<ExecutionResult>.Factory.StartNew(() => Execute(className, resultProperty));

            if (!task.Wait(timeout))
            {
                return new ExecutionResult { Result = "[Execution timed out]" };
            }

            return task.Result ?? new ExecutionResult { Result = "null" };
        }

        private ExecutionResult Execute(string className, string resultProperty)
        {
            var result = new ExecutionResult();
            var type = typeof(ByteCodeLoader);
            var formatter = new ObjectFormatter(maxLineLength: 5120);

            try
            {
                var loader = (ByteCodeLoader)domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
                
                var unformattedResult = loader.Run(className, resultProperty, assemblyBytes);

                result.Result = (unformattedResult != null) ? formatter.FormatObject(unformattedResult.ReturnValue) : "null";
                result.ConsoleOutput = (unformattedResult != null) ? unformattedResult.ConsoleOutput : string.Empty;
                result.ProcessorTime = domain.MonitoringTotalProcessorTime;
                result.TotalMemoryAllocated = domain.MonitoringTotalAllocatedMemorySize;
            }
            catch (SerializationException ex)
            {
                result.Result = ex.Message;
            }
            catch (TargetInvocationException ex)
            {
                result.Result = ex.InnerException.ToString();
            }

            return result;
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

            public SandboxResult Run(string className, string resultProperty, byte[] compiledAssembly)
            {
                var assembly = Assembly.Load(compiledAssembly);
                assembly.EntryPoint.Invoke(null, new object[] { });

                var console = (StringWriter)assembly.GetType("Script").GetField("__Console").GetValue(null);

                var result = new SandboxResult
                             {
                                 ConsoleOutput = console.ToString(),
                                 ReturnValue = assembly.GetType(className).GetProperty(resultProperty).GetValue(null, null)
                             };

                return result;
            }
        }
    }
}
