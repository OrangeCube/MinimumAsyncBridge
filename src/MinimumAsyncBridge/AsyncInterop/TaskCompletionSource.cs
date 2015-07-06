#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(System.Threading.Tasks.TaskCompletionSource<>))]

#else

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents the producer side of a <see cref="Task{TResult}"/> unbound to a delegate, providing access to the consumer side through the Task property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaskCompletionSource<TResult>
    {
        /// <summary>
        /// Gets the <see cref="Task{TResult}"/> created by this TaskCompletionSource<TResult>.
        /// </summary>
        public Task<TResult> Task { get; } = new Task<TResult> { Status = TaskStatus.Running };

        /// <summary>
        /// Attempts to transition the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.Canceled"/> state.
        /// </summary>
        public bool TrySetCanceled() => Task.Cancel();

        /// <summary>
        /// Attempts to transition the underlying <see cref="Task{TResult}"/>into the <see cref="TaskStatus.Faulted"/> state.
        /// </summary>
        public bool TrySetException(Exception exception) => Task.SetException(exception);

        /// <summary>
        /// Attempts to transition the underlying <see cref="Task{TResult}"/>into the <see cref="TaskStatus.RanToCompletion"/> state.
        /// </summary>
        /// <param name="result"></param>
        public bool TrySetResult(TResult result) => Task.SetResult(result);

        /// <summary>
        /// Transitions the underlying <see cref="Task{TResult}"/> into the <see cref="TaskStatus.Canceled"/> state.
        /// </summary>
        public void SetCanceled()
        {
            if (Task.IsCompleted)
                throw new InvalidOperationException();
            Task.Cancel();
        }

        /// <summary>
        /// Transitions the underlying <see cref="Task{TResult}"/>into the <see cref="TaskStatus.Faulted"/> state.
        /// </summary>
        /// <param name="exception"></param>
        public void SetException(Exception exception)
        {
            if (Task.IsCompleted)
                throw new InvalidOperationException();
            Task.SetException(exception);
        }

        /// <summary>
        /// Transitions the underlying <see cref="Task{TResult}"/>into the <see cref="TaskStatus.RanToCompletion"/> state.
        /// </summary>
        /// <param name="result"></param>
        public void SetResult(TResult result)
        {
            if (Task.IsCompleted)
                throw new InvalidOperationException();
            Task.SetResult(result);
        }
    }
}

#endif
