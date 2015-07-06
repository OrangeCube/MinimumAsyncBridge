#if NET45PLUS || !NET40PLUS

namespace System
{
    using System.Threading;

    internal static class AsyncServices
    {
        /// <summary>Throws the exception on the thread pool.</summary>
        /// <param name="exception">The exception to propagate.</param>
        /// <param name="targetContext">
        /// The target context on which to propagate the exception; otherwise, <see langword="null"/> to use the thread
        /// pool.
        /// </param>
        internal static void ThrowAsync(Exception exception, SynchronizationContext targetContext)
        {
            if (targetContext != null)
            {
                try
                {
                    targetContext.Post(
                        state =>
                        {
                            throw PrepareExceptionForRethrow((Exception)state);
                        }, exception);
                    return;
                }
                catch (Exception ex)
                {
                    exception = new AggregateException(exception, ex);
                }
            }

#if NET45PLUS
            Task.Run(() =>
            {
                throw PrepareExceptionForRethrow(exception);
            });
#else
            ThreadPool.QueueUserWorkItem(state =>
            {
                throw PrepareExceptionForRethrow((Exception)state);
            }, exception);
#endif
        }

        /// <summary>Copies the exception's stack trace so its stack trace isn't overwritten.</summary>
        /// <param name="exc">The exception to prepare.</param>
        internal static Exception PrepareExceptionForRethrow(Exception exc)
        {
            return exc;
        }
    }
}

#endif
