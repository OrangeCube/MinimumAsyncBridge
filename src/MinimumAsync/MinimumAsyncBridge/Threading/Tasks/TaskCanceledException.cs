namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an exception used to communicate task cancellation.
    /// </summary>
    public class TaskCanceledException : OperationCanceledException
    {
        /// <summary>
        /// Initializes a new instance of the TaskCanceledException class with a system-supplied message that describes the error.
        /// </summary>
        public TaskCanceledException() { }

        /// <summary>
        /// Initializes a new instance of the TaskCanceledException class with a specified message that describes the error.
        /// </summary>
        /// <param name="message"></param>
        public TaskCanceledException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the TaskCanceledException class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public TaskCanceledException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the TaskCanceledException class with a reference to the <see cref="Task"/> that has been canceled.
        /// </summary>
        /// <param name="task"></param>
        public TaskCanceledException(Task task)
        {
        }
    }
}
