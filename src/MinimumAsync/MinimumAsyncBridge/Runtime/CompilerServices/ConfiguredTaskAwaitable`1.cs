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

            public void OnCompleted(Action continuation) => TaskAwaiter.OnCompletedInternal(_t, continuation, _capturedContext);
            public void UnsafeOnCompleted(Action continuation) => OnCompleted(continuation);
            public bool IsCompleted => _t.IsCompleted;
            public TResult GetResult() => _t.GetResult();
        }
    }
}
