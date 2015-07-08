using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion
    {
        Task<TResult> _t;

        internal TaskAwaiter(Task<TResult> t) { _t = t; }

        public void OnCompleted(Action continuation) => _t.OnCompleted(continuation);
        public void UnsafeOnCompleted(Action continuation) => _t.UnsafeOnCompleted(continuation);
        public bool IsCompleted => _t.IsCompleted;
        public TResult GetResult() => _t.GetResult();
    }
}
