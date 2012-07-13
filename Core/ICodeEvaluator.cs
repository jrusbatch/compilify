using System.Threading.Tasks;
using Compilify.Models;

namespace Compilify
{
    public interface ICodeEvaluator
    {
        Task<WorkerResult> Handle(EvaluateCodeCommand command);
    }
}
