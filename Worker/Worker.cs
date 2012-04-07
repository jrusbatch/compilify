using Compilify.Services;
using Newtonsoft.Json;

namespace Compilify.Worker
{
    internal class Worker
    {
        internal Worker()
        {
            executer = new CodeExecuter();
        }

        private readonly CodeExecuter executer;

        internal ExecuteCommand ProcessItem(ExecuteCommand command)
        {
            var result = executer.Execute(command.Code);
            command.Result = JsonConvert.SerializeObject(result);
            return command;
        }
    }
}
