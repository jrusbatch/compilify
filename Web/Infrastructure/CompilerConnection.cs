using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Roslyn.Compilers.CSharp;
using SignalR;

namespace Compilify.Web.Infrastructure
{
    public class CompilerConnection : PersistentConnection
    {
        protected override Task OnReceivedAsync(string connectionId, string data)
        {
            //var compiler = new CodeExecuter();

            //dynamic result;
            //int i = 0;
            
            //try
            //{
            //    result = compiler.Execute(data);
            //}
            //catch (Exception ex)
            //{
            //    result = ex.ToString();
            //}

            var tree = SyntaxTree.ParseCompilationUnit(data);

            var results = tree.GetDiagnostics()
                              .Select(x => new
                              {
                                  Start = x.Location.SourceSpan.Start,
                                  End = x.Location.GetLineSpan(false),
                                  Message = x.Info.GetMessage(CultureInfo.InvariantCulture)
                              })
                              .ToArray();

            var serializer = new JavaScriptSerializer();
            // Send the response only to the person who wrote the code
            return Connection.Send(serializer.Serialize(new
                                                        {
                                                            sender = connectionId,
                                                            code = data
                                                        }));

        }
    }
}