using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct ConfiguredTaskAwaitable<TResult>
    {
        private readonly ConfiguredTaskAwaiter _configuredTaskAwaiter;

        internal ConfiguredTaskAwaitable(Task<TResult> t, bool continueOnCapturedContext)
        {
            _configuredTaskAwaiter = new ConfiguredTaskAwaiter(t, continueOnCapturedContext);
        }

        public ConfiguredTaskAwaiter GetAwaiter() => _configuredTaskAwaiter;

        public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
        {
            Task<TResult> _t;
            SynchronizationContext _capturedContext;

            internal ConfiguredTaskAwaiter(Task<TResult> t, bool continueOnCapturedContext)
            {
                _t = t;
                _capturedContext = continueOnCapturedContext ? SynchronizationContext.Current : null;
            }

            public void OnCompleted(Action continuation)
            {
                var back = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(_capturedContext);
                TaskAwaiter.OnCompletedInternal(_t, continuation, _capturedContext);
                SynchronizationContext.SetSynchronizationContext(back);
            }
            public void UnsafeOnCompleted(Action continuation)
            {
                var back = SynchronizationContext.Current;
                SynchronizationContext.SetSynchronizationContext(_capturedContext);
                OnCompleted(continuation);
                SynchronizationContext.SetSynchronizationContext(back);
            }
            public bool IsCompleted => _t.IsCompleted;
            public TResult GetResult() => _t.GetResult();
        }
    }
}
