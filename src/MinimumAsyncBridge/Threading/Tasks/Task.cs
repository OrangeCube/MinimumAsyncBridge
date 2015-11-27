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
                    _completed?.Invoke();
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
                    MergeException(exception);
                    _completed?.Invoke();
                    return true;
                }
                return false;
            }
        }

        private void MergeException(Exception ex)
        {
            var agex = ex as AggregateException;
            if (Exception == null)
            {
                if (agex != null)
                    Exception = agex;
                else
                    Exception = new AggregateException(ex);
            }
            else
            {
                if (agex != null)
                    Exception = new AggregateException(Exception.InnerExceptions.Concat(agex.InnerExceptions).ToArray());
                else
                    Exception = new AggregateException(Exception.InnerExceptions.Concat(new[] { ex }).ToArray());
            }
        }

        public AggregateException Exception { get; private set; }

        protected internal bool Complete(Action onComplete)
        {
            lock (_sync)
            {
                if (Status == TaskStatus.Running)
                {
                    Status = TaskStatus.RanToCompletion;
                    onComplete();
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
            if (Exception != null)
                throw Exception;
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
                throw Exception.InnerExceptions.First();

            if (IsCanceled)
                throw new TaskCanceledException();
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
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));
            if (tasks.Length == 0) throw new ArgumentException(nameof(tasks) + " empty", nameof(tasks));

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
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));
            if (tasks.Length == 0) throw new ArgumentException(nameof(tasks) + " empty", nameof(tasks));

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
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));
            if (tasks.Length == 0) return CompletedTask;

            var tcs = new TaskCompletionSource<object>();
            var exceptions = new AggregateException[tasks.Length];
            int count = 0;

            for (int j = 0; j < tasks.Length; j++)
            {
                var index = j;
                var t = tasks[index];

                if (t.IsCompleted)
                {
                    if (t.IsFaulted)
                        lock (exceptions)
                        exceptions[index] = t.Exception;

                    CheckWhenAllCompletetion(tasks, tcs, null, exceptions, ref count);
                }
                else
                {
                    t.OnCompleted(() =>
                    {
                        if (t.IsFaulted)
                            lock (exceptions)
                                exceptions[index] = t.Exception;

                        CheckWhenAllCompletetion(tasks, tcs, null, exceptions, ref count);
                    });
                }
            }

            return tcs.Task;
        }

        private static void CheckWhenAllCompletetion<TResult>(Task[] tasks, TaskCompletionSource<TResult> tcs, TResult result, AggregateException[] exceptions, ref int count)
        {
            Interlocked.Increment(ref count);
            if (count == tasks.Length)
            {
                bool any;
                Exception[] innerExceptions;
                lock (exceptions)
                {
                    innerExceptions = exceptions.Where(x => x != null).SelectMany(x => x.InnerExceptions).ToArray();
                    any = innerExceptions.Any();
                }
                if (any)
                    tcs.TrySetException(new AggregateException(innerExceptions));
                else if (tasks.Any(x => x.IsCanceled))
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(result);
            }
        }

        public static Task<TResult[]> WhenAll<TResult>(IEnumerable<Task<TResult>> tasks) => WhenAll(tasks.ToArray());

        public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks)
        {
            if (tasks == null) throw new ArgumentNullException(nameof(tasks));
            if (tasks.Length == 0) return FromResult(new TResult[0]);

            var tcs = new TaskCompletionSource<TResult[]>();
            var exceptions = new AggregateException[tasks.Length];
            var results = new TResult[tasks.Length];
            int count = 0;

            for (var j = 0; j < tasks.Length; j++)
            {
                var index = j;
                var t = tasks[index];

                if (t.IsCompleted)
                {
                    if (t.IsFaulted)
                        lock (exceptions)
                            exceptions[index] = t.Exception;
                    else if (!t.IsCanceled)
                        results[index] = t.Result;

                    CheckWhenAllCompletetion(tasks, tcs, results, exceptions, ref count);
                }
                else
                {
                    t.OnCompleted(() =>
                    {
                        if (t.IsFaulted)
                            lock (exceptions)
                                exceptions[index] = t.Exception;
                        else if (!t.IsCanceled)
                            results[index] = t.Result;

                        CheckWhenAllCompletetion(tasks, tcs, results, exceptions, ref count);
                    });
                }
            }

            return tcs.Task;
        }

        public Task ContinueWith(Action<Task> continuationAction)
        {
            var tcs = new TaskCompletionSource<object>();
            OnCompleted(() =>
            {
                try
                {
                    continuationAction(this);
                    tcs.TrySetResult(null);
                }
                catch(Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }

        public Task<TResult> ContinueWith<TResult>(Func<Task, TResult> continuationFunction)
        {
            var tcs = new TaskCompletionSource<TResult>();
            OnCompleted(() =>
            {
                try
                {
                    var r = continuationFunction(this);
                    tcs.TrySetResult(r);
                }
                catch(Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });
            return tcs.Task;
        }

        public static Task Delay(TimeSpan delay) => Delay((int)delay.TotalMilliseconds, CancellationToken.None);
        public static Task Delay(TimeSpan delay, CancellationToken cancellationToken) => Delay((int)delay.TotalMilliseconds, cancellationToken);
        public static Task Delay(int millisecondsDelay) => Delay(millisecondsDelay, CancellationToken.None);

        public static Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (cancellationToken.IsCancellationRequested)
            {
                tcs.SetCanceled();
                return tcs.Task;
            }

            if (millisecondsDelay <= 0)
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            Timer t = null;

            Action<bool> stop = (canceled) =>
            {
                bool done = false;
                TaskCompletionSource<bool> tcs1;

                lock (t)
                {
                    done = tcs == null;
                    tcs1 = tcs;
                    tcs = null;
                }

                if (!done)
                {
                    t.Dispose();
                    if (canceled) tcs1.TrySetCanceled();
                    else tcs.TrySetResult(false);
                }
            };

            t = new Timer(_ => stop(false), null, Timeout.Infinite, Timeout.Infinite);

            t.Change(millisecondsDelay, Timeout.Infinite);

            if (cancellationToken != CancellationToken.None)
            {
                cancellationToken.Register(() => stop(true));
            }

            return tcs.Task;
        }
    }
}
