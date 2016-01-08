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
        }

        public Task WaitAsync()
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
                    return task.Task;
                }
            }
        }

        public int Release()
        {
            TaskNode head = null;
            int count;

            lock (_lockObj)
            {
                count = _currentCount;

                if (_head == null)
                {
                    ++_currentCount;
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

            if (head != null)
                head.TrySetResult(false);

            return count;
        }

        public void Dispose()
        {
            // nop, because this simplified implementation does not use WaitHandle
        }
    }
}
