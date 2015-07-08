#if !NET40PLUS

namespace System.Runtime.CompilerServices
{
    using System.Threading.Tasks;

    /// <summary>Provides a base class used to cache tasks of a specific return type.</summary>
    /// <typeparam name="TResult">Specifies the type of results the cached tasks return.</typeparam>
    internal class AsyncMethodTaskCache<TResult>
    {
        /// <summary>
        /// A singleton cache for this result type. This may be <see langword="null"/> if there are no cached tasks for
        /// this <typeparamref name="TResult"/>.
        /// </summary>
        internal static readonly AsyncMethodTaskCache<TResult> Singleton = CreateCache();

        /// <summary>Creates a non-disposable task.</summary>
        /// <param name="result">The result for the task.</param>
        /// <returns>The cacheable task.</returns>
        internal static TaskCompletionSource<TResult> CreateCompleted(TResult result)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
            taskCompletionSource.TrySetResult(result);
            return taskCompletionSource;
        }

        /// <summary>Creates a cache.</summary>
        /// <returns>A task cache for this result type.</returns>
        private static AsyncMethodTaskCache<TResult> CreateCache()
        {
            Type typeFromHandle = typeof(TResult);
            if (typeFromHandle == typeof(bool))
            {
                return (AsyncMethodTaskCache<TResult>)(object)new AsyncMethodBooleanTaskCache();
            }

            if (typeFromHandle == typeof(int))
            {
                return (AsyncMethodTaskCache<TResult>)(object)new AsyncMethodInt32TaskCache();
            }

            return null;
        }

        /// <summary>Gets a cached task if one exists.</summary>
        /// <param name="result">The result for which we want a cached task.</param>
        /// <returns>A cached task if one exists; otherwise, <see langword="null"/>.</returns>
        internal virtual TaskCompletionSource<TResult> FromResult(TResult result)
        {
            return CreateCompleted(result);
        }

        /// <summary>Provides a cache for <see cref="bool"/> tasks.</summary>
        private sealed class AsyncMethodBooleanTaskCache : AsyncMethodTaskCache<bool>
        {
            /// <summary>A <see langword="true"/> task.</summary>
            private readonly TaskCompletionSource<bool> _true = CreateCompleted(true);

            /// <summary>A <see langword="false"/> task.</summary>
            private readonly TaskCompletionSource<bool> _false = CreateCompleted(false);

            /// <summary>Gets a cached task for the <see cref="bool"/> result.</summary>
            /// <param name="result"><see langword="true"/> or <see langword="false"/></param>
            /// <returns>A cached task for the <see cref="bool"/> result.</returns>
            internal sealed override TaskCompletionSource<bool> FromResult(bool result)
            {
                if (!result)
                {
                    return _false;
                }

                return _true;
            }
        }

        /// <summary>Provides a cache for <see cref="int"/> tasks.</summary>
        private sealed class AsyncMethodInt32TaskCache : AsyncMethodTaskCache<int>
        {
            /// <summary>The minimum value, inclusive, for which we want a cached task.</summary>
            internal const int INCLUSIVE_INT32_MIN = -1;

            /// <summary>The maximum value, exclusive, for which we want a cached task.</summary>
            internal const int EXCLUSIVE_INT32_MAX = 9;

            /// <summary>The cache of <see cref="int"/>-returning <see cref="Task{TResult}"/> instances.</summary>
            internal static readonly TaskCompletionSource<int>[] Int32Tasks = CreateInt32Tasks();

            /// <summary>
            /// Creates an array of cached tasks for the values in the range [<see cref="INCLUSIVE_INT32_MIN"/>,
            /// <see cref="EXCLUSIVE_INT32_MAX"/>).
            /// </summary>
            private static TaskCompletionSource<int>[] CreateInt32Tasks()
            {
                TaskCompletionSource<int>[] array = new TaskCompletionSource<int>[EXCLUSIVE_INT32_MAX - INCLUSIVE_INT32_MIN];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = CreateCompleted(i + INCLUSIVE_INT32_MIN);
                }

                return array;
            }

            /// <summary>Gets a cached task for the <see cref="int"/> result.</summary>
            /// <param name="result">The integer value.</param>
            /// <returns>
            /// A cached task for the <see cref="int"/> result; otherwise, <see langword="null"/> if no task is cached
            /// for the value <paramref name="result"/>.
            /// </returns>
            internal sealed override TaskCompletionSource<int> FromResult(int result)
            {
                if (result < INCLUSIVE_INT32_MIN || result >= EXCLUSIVE_INT32_MAX)
                {
                    return CreateCompleted(result);
                }

                return Int32Tasks[result - INCLUSIVE_INT32_MIN];
            }
        }
    }
}

#endif
