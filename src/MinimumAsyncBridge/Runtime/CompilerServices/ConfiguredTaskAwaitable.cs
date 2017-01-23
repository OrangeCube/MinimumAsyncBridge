using System.Threading;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct ConfiguredTaskAwaitable
    {
        private readonly ConfiguredTaskAwaiter _configuredTaskAwaiter;

        internal ConfiguredTaskAwaitable(Task t, bool continueOnCapturedContext)
        {
            _configuredTaskAwaiter = new ConfiguredTaskAwaiter(t, continueOnCapturedContext);
        }

        public ConfiguredTaskAwaiter GetAwaiter() => _configuredTaskAwaiter;

        public struct ConfiguredTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
        {
            Task _t;
            SynchronizationContext _capturedContext;

            internal ConfiguredTaskAwaiter(Task t, bool continueOnCapturedContext)
            {
                _t = t;
                _capturedContext = continueOnCapturedContext ? SynchronizationContext.Current : null;
            }

            public void OnCompleted(Action continuation)
            {
                var back = SynchronizationContext.Current;
                try
                {
                    SynchronizationContext.SetSynchronizationContext(_capturedContext);
                    TaskAwaiter.OnCompletedInternal(_t, continuation, _capturedContext);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(back);
                }
            }
            public void UnsafeOnCompleted(Action continuation)
            {
                var back = SynchronizationContext.Current;
                try
                {
                    SynchronizationContext.SetSynchronizationContext(_capturedContext);
                    OnCompleted(continuation);
                }
                finally
                {
                    SynchronizationContext.SetSynchronizationContext(back);
                }
            }
            public bool IsCompleted => _t.IsCompleted;
            public void GetResult() => _t.GetResult();
        }
    }
}
