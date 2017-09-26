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
        Running = 3,

        /// <summary>
        /// The task completed execution successfully.
        /// </summary>
        RanToCompletion = 5,

        /// <summary>
        /// The task acknowledged cancellation by throwing an OperationCanceledException with its own CancellationToken while the token was in signaled state, or the task's CancellationToken was already signaled before the task started executing. For more information, see Task Cancellation.
        /// </summary>
        Canceled = 6,

        /// <summary>
        /// The task completed due to an unhandled exception.
        /// </summary>
        Faulted = 7,

        // System.Threading.Tasks.TaskStatus value order.。
        //Created = 0,
        //WaitingForActivation = 1,
        //WaitingToRun = 2,
        //Running = 3,
        //WaitingForChildrenToComplete = 4,
        //RanToCompletion = 5,
        //Canceled = 6,
        //Faulted = 7
    }
}
