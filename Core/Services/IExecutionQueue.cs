using System.Threading.Tasks;

namespace Compilify.Services
{
    public interface IExecutionQueue
    {
        Task<long> QueueForExecution(ExecuteCommand command);
    }
}
