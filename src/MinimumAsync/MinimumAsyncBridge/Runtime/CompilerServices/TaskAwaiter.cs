using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct TaskAwaiter : ICriticalNotifyCompletion
    {
        Task _t;
        SynchronizationContext _capturedContext;

        internal TaskAwaiter(Task t)
        {
            _t = t;
            _capturedContext = SynchronizationContext.Current;
        }

        public void OnCompleted(Action continuation) => OnCompletedInternal(_t, continuation, _capturedContext);
        public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);
        public bool IsCompleted => _t.IsCompleted;
        public void GetResult() => _t.GetResult();

        internal static void OnCompletedInternal(Task t, Action continuation, SynchronizationContext capturedContext)
        {
            t.OnCompleted(() =>
            {
                if (capturedContext == null)
                {
                    continuation();
                }
                else
                {
                    capturedContext.Post(state => ((Action)state)(), continuation);
                }
            });
        }
    }
}
