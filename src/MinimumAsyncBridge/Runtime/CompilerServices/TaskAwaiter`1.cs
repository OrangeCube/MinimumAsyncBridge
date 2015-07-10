using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct TaskAwaiter<TResult> : ICriticalNotifyCompletion
    {
        Task<TResult> _t;
        SynchronizationContext _capturedContext;

        internal TaskAwaiter(Task<TResult> t)
        {
            _t = t;
            _capturedContext = SynchronizationContext.Current;
        }

        public void OnCompleted(Action continuation) => TaskAwaiter.OnCompletedInternal(_t, continuation, _capturedContext);
        public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);
        public bool IsCompleted => _t.IsCompleted;
        public TResult GetResult() => _t.GetResult();
    }
}
