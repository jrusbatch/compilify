using System;
using System.Threading.Tasks;
using Compilify.Web.Services;
using SignalR;

namespace Compilify.Web.Infrastructure
{
    public class CompilerConnection : PersistentConnection
    {
        protected override Task OnReceivedAsync(string connectionId, string data)
        {
            var compiler = new CodeExecuter();

            dynamic result;
            int i = 0;
            
            try
            {
                result = compiler.Execute(data);
            }
            catch (Exception ex)
            {
                result = ex.ToString();
            }

            // Send the response only to the person who wrote the code
            return Connection.Send(new
                                   {
                                       sender = connectionId,
                                       code = data,
                                       result = result
                                   });

        }
    }
}