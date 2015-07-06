#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(System.Threading.Tasks.Task))]

#else

using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an asynchronous operation.
    /// </summary>
    public class Task : ICriticalNotifyCompletion
    {
        public TaskStatus Status { get; protected internal set; }

        /// <summary>
        /// Gets whether this Task has completed.
        /// </summary>
        public bool IsCompleted => Status == TaskStatus.RanToCompletion || IsCanceled || IsFaulted;

        /// <summary>
        /// Gets whether this Task instance has completed execution due to being canceled.
        /// </summary>
        public bool IsCanceled => Status == TaskStatus.Canceled;

        /// <summary>
        /// Gets whether the Task completed due to an unhandled exception.
        /// </summary>
        public bool IsFaulted => Status == TaskStatus.Faulted;

        private object _sync = new object();

        internal bool Cancel()
        {
            lock(_sync)
            {
                if (Status == TaskStatus.Running)
                {
                    Status = TaskStatus.Canceled;
                    return true;
                }
                return false;
            }
        }

        internal bool SetException(Exception exception)
        {
            lock (_sync)
            {
                if (Status == TaskStatus.Running)
                {
                    Status = TaskStatus.Faulted;
                    Exception = exception;
                    return true;
                }
                return false;
            }
        }

        public Exception Exception { get; private set; }

        protected internal bool Complete()
        {
            var sc = SynchronizationContext.Current;

            lock (_sync)
            {
                if (Status == TaskStatus.Running)
                {
                    Status = TaskStatus.RanToCompletion;

                    if (sc == null)
                    {
                        Completed?.Invoke();
                    }
                    else
                    {
                        sc.Post(state => ((Action)state).Invoke(), Completed);
                    }
                    return true;
                }
                return false;
            }
        }

        public void Wait()
        {
            while (!IsCompleted)
                Thread.Sleep(10);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }

        public void OnCompleted(Action continuation)
        {
            lock (_sync)
            {
                if (IsCompleted)
                {
                    continuation();
                    return;
                }

                Action x = null;
                x = () =>
                {
                    continuation();
                    Completed -= x;
                };

                Completed += x;
            }
        }

        private event Action Completed;

        public Task GetAwaiter() => this;

        public void GetResult()
        {
            if(Exception != null)
                throw Exception;
        }
    }
}

#endif
