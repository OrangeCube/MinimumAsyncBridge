using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an asynchronous operation.
    /// </summary>
    public class Task
    {
        public TaskStatus Status { get; protected internal set; }

        /// <summary>
        /// Gets whether this Task has completed.
        /// </summary>
        public bool IsCompleted => Status == TaskStatus.RanToCompletion || IsCanceled || IsFaulted;

        /// <summary>
        /// Gets whether this Task instance has completed execution due to being canceled.
        /// </summary>
        public bool IsCanceled => Status == TaskStatus.Canceled;

        /// <summary>
        /// Gets whether the Task completed due to an unhandled exception.
        /// </summary>
        public bool IsFaulted => Status == TaskStatus.Faulted;

        private object _sync = new object();

        internal bool Cancel()
        {
            lock(_sync)
            {
                if (Status == TaskStatus.Running)
                {
                    Status = TaskStatus.Canceled;
                    return true;
                }
                return false;
            }
        }

        internal bool SetException(Exception exception)
        {
            lock (_sync)
            {
                if (Status == TaskStatus.Running)
                {
                    Status = TaskStatus.Faulted;
                    Exception = exception;
                    return true;
                }
                return false;
            }
        }

        public Exception Exception { get; private set; }

        protected internal bool Complete()
        {
            lock (_sync)
            {
                if (Status == TaskStatus.Running)
                {
                    Status = TaskStatus.RanToCompletion;
                    _completed?.Invoke();
                    return true;
                }
                return false;
            }
        }

        public void Wait()
        {
            while (!IsCompleted)
                Thread.Sleep(10);
        }

        internal void OnCompleted(Action continuation)
        {
            lock (_sync)
            {
                if (IsCompleted)
                {
                    continuation();
                    return;
                }

                Action x = null;
                x = () =>
                {
                    continuation();
                    _completed -= x;
                };

                _completed += x;
            }
        }

        private Action _completed;

        public TaskAwaiter GetAwaiter() => new TaskAwaiter(this);

        public ConfiguredTaskAwaitable ConfigureAwait(bool continueOnCapturedContext) => new ConfiguredTaskAwaitable(this, continueOnCapturedContext);

        internal void GetResult()
        {
            if(Exception != null)
                throw Exception;
        }

        public static Task<TResult> FromResult<TResult>(TResult value)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetResult(value);
            return tcs.Task;
        }

        public static Task CompletedTask { get; } = FromResult<object>(null);

        public static Task FromException(Exception exception)
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        public static Task<TResult> FromException<TResult>(Exception exception)
        {
            var tcs = new TaskCompletionSource<TResult>();
            tcs.SetException(exception);
            return tcs.Task;
        }

        public static Task<Task> WhenAny(params Task[] tasks)
        {
            var tcs = new TaskCompletionSource<Task>();

            foreach (var t in tasks)
            {
                if (t.IsCompleted)
                {
                    tcs.TrySetResult(t);
                    break;
                }

                t.OnCompleted(() => tcs.TrySetResult(t));
            }

            return tcs.Task;
        }

        public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks)
        {
            var tcs = new TaskCompletionSource<Task<TResult>>();

            foreach (var t in tasks)
            {
                if (t.IsCompleted)
                {
                    tcs.TrySetResult(t);
                    break;
                }

                t.OnCompleted(() => tcs.TrySetResult(t));
            }

            return tcs.Task;
        }

        public static Task WhenAll(IEnumerable<Task> tasks) => WhenAll(tasks.ToArray());

        public static Task WhenAll(params Task[] tasks)
        {
            var tcs = new TaskCompletionSource<object>();
            int count = 0;

            for (int i = 0; i < tasks.Length; i++)
            {
                var t = tasks[i];

                if (t.IsCompleted)
                {
                    Interlocked.Increment(ref count);
                    if (count == tasks.Length)
                        tcs.TrySetResult(null);
                }
                else
                {
                    t.OnCompleted(() =>
                    {
                        Interlocked.Increment(ref count);
                        if (count == tasks.Length)
                            tcs.TrySetResult(null);
                    });
                }
            }

            return tcs.Task;
        }

        public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks) => WhenAll(tasks.ToArray());

        public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks)
        {
            var tcs = new TaskCompletionSource<TResult[]>();
            var results = new TResult[tasks.Length];
            int count = 0;

            for (var j = 0; j < tasks.Length; j++)
            {
                var i = j;
                var t = tasks[i];

                if (t.IsCompleted)
                {
                    results[i] = t.Result;
                    Interlocked.Increment(ref count);
                    if (count == tasks.Length)
                        tcs.TrySetResult(results);
                    break;
                }

                t.OnCompleted(() =>
                {
                    results[i] = t.Result;
                    Interlocked.Increment(ref count);
                    if (count == tasks.Length)
                        tcs.TrySetResult(results);
                });
            }

            return tcs.Task;
        }

        public Task ContinueWith(Action<Task> continuationAction)
        {
            var tcs = new TaskCompletionSource<object>();
            OnCompleted(() =>
            {
                continuationAction(this);
                tcs.TrySetResult(null);
            });
            return tcs.Task;
        }

        public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction)
        {
            var tcs = new TaskCompletionSource<TResult>();
            OnCompleted(() =>
            {
                var r = continuationFunction(this);
                tcs.TrySetResult(r);
            });
            return tcs.Task;
        }

        public static Task Delay(TimeSpan delay) => Delay((int)delay.TotalMilliseconds, CancellationToken.None);
        public static Task Delay(TimeSpan delay, CancellationToken cancellationToken) => Delay((int)delay.TotalMilliseconds, cancellationToken);
        public static Task Delay(int millisecondsDelay) => Delay(millisecondsDelay, CancellationToken.None);

        public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (millisecondsDelay <= 0)
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            Timer t = null;
            t = new Timer(_ =>
            {
                t.Dispose();
                tcs.TrySetResult(false);
                t = null;
                tcs = null;
            }, null, millisecondsDelay, Timeout.Infinite);

            if (cancellationToken != CancellationToken.None)
            {
                cancellationToken.Register(() =>
                {
                    t.Dispose();
                    tcs.TrySetCanceled();
                    t = null;
                    tcs = null;
                });
            }

            return tcs.Task;
        }
    }
}
