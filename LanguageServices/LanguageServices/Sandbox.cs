using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading.Tasks;
using Compilify.Models;
using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace Compilify.LanguageServices
{
    public sealed class Sandbox : ICodeExecutor
    {
        private const string DefaultName = "Sandbox";

        private readonly AppDomain domain;
        private bool disposed;
        
        static Sandbox()
        {
            AppDomain.MonitoringIsEnabled = true;
        }

        public Sandbox(string name = DefaultName)
        {
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

        public ExecutionResult Execute(ICodeAssembly assembly, TimeSpan timeout)
        {
            var task = Task<ExecutionResult>.Factory.StartNew(Execute, assembly);

            if (!task.Wait(timeout))
            {
                return new ExecutionResult { Result = "[Execution timed out]" };
            }

            return task.Result ?? new ExecutionResult { Result = "null" };
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                AppDomain.Unload(domain);
            }
        }

        private ExecutionResult Execute(object assemblyInput)
        {
            var assembly = (ICodeAssembly)assemblyInput;
            var result = new ExecutionResult();
            var type = typeof(ByteCodeLoader);
            var formatter = ObjectFormatter.Instance;
            var formattingOptions = new ObjectFormattingOptions(maxOutputLength: 5120);

            var className = assembly.EntryPointClassName;
            var methodName = assembly.EntryPointMethodName;
            var resultProperty = "Result";
            var assemblyBytes = assembly.CompiledAssembly;

            try
            {
                var handle = Activator.CreateInstanceFrom(domain, type.Assembly.ManifestModule.FullyQualifiedName, type.FullName);
                var loader = (ByteCodeLoader)handle.Unwrap();
                var unformattedResult = loader.Run(className, methodName, resultProperty, assemblyBytes);

                result.Result = (unformattedResult != null) ? formatter.FormatObject(unformattedResult.ReturnValue, formattingOptions) : "null";
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

        private sealed class ByteCodeLoader : MarshalByRefObject
        {
            public ByteCodeLoader()
            {
            }

            public SandboxResult Run(string className, string methodName, string resultProperty, byte[] compiledAssembly)
            {
                var assembly = Assembly.Load(compiledAssembly);
                var target = assembly.GetType(className).GetMethod(methodName);
                var console = (StringWriter)assembly.GetType("Script").GetField("__Console").GetValue(null);
                var returnValue = (object)null;

                try
                {
                    target.Invoke(null, new object[0]);
                    returnValue = assembly.GetType(className).GetProperty(resultProperty).GetValue(null, null);
                }
                catch (Exception exc)
                {
                    returnValue = exc;
                }

                var result = new SandboxResult
                             {
                                 ConsoleOutput = console.ToString(),
                                 ReturnValue = returnValue
                             };

                return result;
            }
        }

        [Serializable]
        private class SandboxResult
        {
            public string ConsoleOutput { get; set; }

            public object ReturnValue { get; set; }
        }
    }
}