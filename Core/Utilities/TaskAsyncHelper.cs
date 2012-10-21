using System.Threading.Tasks;

namespace Compilify.Utilities
{
    public static class TaskAsyncHelper
    {
        private static readonly Task emptyTask = MakeTask<object>(null);

        public static Task Empty
        {
            get { return emptyTask; }
        }

        private static Task<T> MakeTask<T>(T value)
        {
            return Task.FromResult<T>(value);
        }
    }
}