using System.Threading.Tasks;

namespace Compilify
{
    public interface IQueue<T>
    {
        T Enqueue(T message);

        Task<T> EnqueueAsync(T message);

        T Dequeue();

        Task<T> DequeueAsync();
    }
}
