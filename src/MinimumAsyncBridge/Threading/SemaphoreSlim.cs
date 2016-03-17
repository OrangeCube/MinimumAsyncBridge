namespace System.Threading
{
    using Tasks;

    /// <summary>
    /// Simplified implementation of SemaphoreSlim.
    /// 
    /// </summary>
    public class SemaphoreSlim : IDisposable
    {
        private object _lockObj = new object();
        private int _currentCount;
        private TaskNode _head;
        private TaskNode _tail;

        public SemaphoreSlim(int initialCount)
        {
            _currentCount = initialCount;
        }

        public int CurrentCount => _currentCount;

        /// <summary>
        /// <see cref="TaskCompletionSource{TResult}"/>-derived linked list node so as to have links intrusively。
        /// </summary>
        class TaskNode : TaskCompletionSource<bool>
        {
            public TaskNode Next;
            public CancellationTokenRegistration Cancellation;
        }

        public Task WaitAsync() => WaitAsync(CancellationToken.None);

        public Task WaitAsync(CancellationToken cancellationToken)
        {
            lock (_lockObj)
            {
                if (_currentCount > 0)
                {
                    --_currentCount;
                    return Task.CompletedTask;
                }
                else
                {
                    var task = new TaskNode();
                    if (_head == null)
                    {
                        _head = _tail = task;
                    }
                    else
                    {
                        _tail.Next = task;
                        _tail = task;
                    }

                    if (cancellationToken != CancellationToken.None)
                    {
                        task.Cancellation = cancellationToken.Register(() =>
                        {
                            task.TrySetCanceled();
                            if (task.Cancellation != default(CancellationTokenRegistration))
                                task.Cancellation.Dispose();
                        });
                    }

                    return task.Task;
                }
            }
        }

        public int Release()
        {
            TaskNode head = null;
            int count;

            do
            {
                lock (_lockObj)
                {
                    count = _currentCount;

                    if (_head == null)
                    {
                        ++_currentCount;
                        head = null;
                    }
                    else
                    {
                        head = _head;

                        if (_head == _tail)
                        {
                            _head = _tail = null;
                        }
                        else
                        {
                            _head = _head.Next;
                        }
                    }
                }
            } while (head != null && head.Task.IsCompleted);

            if (head != null)
            {
                if (head.Cancellation != default(CancellationTokenRegistration))
                    head.Cancellation.Dispose();
                Task.Run(() => head.TrySetResult(false));
            }

            return count;
        }

        public void Dispose()
        {
            // nop, because this simplified implementation does not use WaitHandle
        }
    }
}
