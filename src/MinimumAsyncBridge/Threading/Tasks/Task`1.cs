using System.Linq;
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

                if (Exception != null)
                    throw Exception;

                if (IsCanceled)
                    throw new AggregateException(new TaskCanceledException());

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
                throw Exception.InnerExceptions.First();

            if (IsCanceled)
                throw new TaskCanceledException();

            return _result;
        }

        public Task ContinueWith(Action<Task<TResult>> continuationAction)
        {
            var tcs = new TaskCompletionSource<object>();
            OnCompleted(() =>
            {
                try
                {
                    continuationAction(this);
                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }

        public Task<TNewResult> ContinueWith<TNewResult>(Func<Task<TResult>, TNewResult> continuationFunction)
        {
            var tcs = new TaskCompletionSource<TNewResult>();
            OnCompleted(() =>
            {
                try
                {
                    var r = continuationFunction(this);
                    tcs.TrySetResult(r);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }
    }
}
