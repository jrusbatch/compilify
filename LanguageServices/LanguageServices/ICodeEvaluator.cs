using System.Threading;
using System.Threading.Tasks;

namespace Compilify.LanguageServices
{
    public interface ICodeEvaluator
    {
        Task<ICodeRunResult> EvaluateAsync(ICodeProgram program, CancellationToken cancellationToken);
    }
}
