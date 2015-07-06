using System;
using System.Runtime.CompilerServices;

namespace IteratorTasks.AsyncBridge
{
    public struct TaskAwaiter<TResult> : INotifyCompletion
    {
        private readonly Task<TResult> _t;

        internal TaskAwaiter(Task<TResult> t) { _t = t; }

        public bool IsCompleted => _t.IsCompleted;

        public void OnCompleted(Action continuation)
        {
            if (_t.IsCompleted)
                continuation();
            else
                _t.ContinueWith(_ => continuation());
        }

        public TResult GetResult() => _t.Result;
    }
}
