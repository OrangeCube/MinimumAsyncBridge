#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(System.Threading.Tasks.TaskStatus))]

#else

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents the current stage in the lifecycle of a Task.
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// The task is running but has not yet completed.
        /// </summary>
        Running,

        /// <summary>
        /// The task completed execution successfully.
        /// </summary>
        RanToCompletion,

        /// <summary>
        /// The task acknowledged cancellation by throwing an OperationCanceledException with its own CancellationToken while the token was in signaled state, or the task's CancellationToken was already signaled before the task started executing. For more information, see Task Cancellation.
        /// </summary>
        Canceled,

        /// <summary>
        /// The task completed due to an unhandled exception.
        /// </summary>
        Faulted,
    }
}

#endif
