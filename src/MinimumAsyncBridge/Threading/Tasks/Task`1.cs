using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an asynchronous operation that can return a value.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class Task<TResult> : Task
    {
        public TResult Result
        {
            get
            {
                if (Status == TaskStatus.Running)
                    Wait();
                return GetResult();
            }
            private set { _result = value; }
        }
        private TResult _result;

        internal bool SetResult(TResult result)
        {
            return Complete(() => Result = result);
        }

        public new TaskAwaiter<TResult> GetAwaiter() => new TaskAwaiter<TResult>(this);

        public new ConfiguredTaskAwaitable<TResult> ConfigureAwait(bool continueOnCapturedContext) => new ConfiguredTaskAwaitable<TResult>(this, continueOnCapturedContext);

        internal new TResult GetResult()
        {
            if (Exception != null)
                throw Exception;

            if (IsCanceled)
                throw new TaskCanceledException();

            return _result;
        }

        public Task ContinueWith(Action<Task<TResult>> continuationAction)
        {
            var tcs = new TaskCompletionSource<object>();
            OnCompleted(() =>
            {
                continuationAction(this);
                tcs.TrySetResult(null);
            });
            return tcs.Task;
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction)
        {
            var tcs = new TaskCompletionSource<TNewResult>();
            OnCompleted(() =>
            {
                var r = continuationFunction(this);
                tcs.TrySetResult(r);
            });
            return tcs.Task;
        }
    }
}
