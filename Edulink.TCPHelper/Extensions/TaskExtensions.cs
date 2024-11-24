using System;
using System.Threading;
using System.Threading.Tasks;

namespace Edulink.Classes
{
    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancellationToken));
            if (completedTask == task)
            {
                return await task;
            }
            else
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }
    }
}
