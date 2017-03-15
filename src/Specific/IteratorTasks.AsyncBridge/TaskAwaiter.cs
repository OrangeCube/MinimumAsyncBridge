using System;
using System.Runtime.CompilerServices;

namespace IteratorTasks.AsyncBridge
{
    public struct TaskAwaiter : INotifyCompletion
    {
        private readonly Task _t;

        internal TaskAwaiter(Task t) { _t = t; }

        public bool IsCompleted => _t.IsCompleted;

        public void OnCompleted(Action continuation)
        {
            if (_t.IsCompleted)
                continuation();
            else
                _t.ContinueWith(_ => continuation());
        }

        public void GetResult()
        {
            if (_t.Exception != null)
                throw _t.Exception;
        }
    }
}
