using System.Threading.Tasks;
using Compilify.Models;
using Compilify.Utilities;

namespace Compilify
{
    public interface ICodeEvaluator
    {
        Task<WorkerResult> Handle(EvaluateCodeCommand command);
    }
}
