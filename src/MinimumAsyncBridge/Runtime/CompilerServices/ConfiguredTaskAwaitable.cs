using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct ConfiguredTaskAwaitable
    {
        Task _t;
        bool _continueOnCapturedContext;

        internal ConfiguredTaskAwaitable(Task t, bool continueOnCapturedContext)
        {
            _t = t;
            _continueOnCapturedContext = continueOnCapturedContext;
        }

        public ConfiguredTaskAwaiter GetAwaiter() => new ConfiguredTaskAwaiter(_t, _continueOnCapturedContext);

        public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
        {
            Task _t;
            SynchronizationContext _capturedContext;

            internal ConfiguredTaskAwaiter(Task t, bool continueOnCapturedContext)
            {
                _t = t;
                _capturedContext = continueOnCapturedContext ? SynchronizationContext.Current : null;
            }

            public void OnCompleted(Action continuation) => TaskAwaiter.OnCompletedInternal(_t, continuation, _capturedContext);
            public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);
            public bool IsCompleted => _t.IsCompleted;
            public void GetResult() => _t.GetResult();
        }
    }
}
