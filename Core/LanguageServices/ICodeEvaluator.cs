using System.Threading;
using System.Threading.Tasks;
using Compilify.Models;

namespace Compilify.LanguageServices
{
    public interface ICodeEvaluator
    {
        Task<WorkerResult> Handle(EvaluateCodeCommand command, CancellationToken cancellationToken);
    }
}
