using System.Runtime.CompilerServices;

namespace System.Threading.Tasks
{
    /// <summary>
    /// Represents an asynchronous operation that can return a value.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public class Task<TResult> : Task
    {
        public TResult Result { get; private set; }

        internal bool SetResult(TResult result)
        {
            Result = result;
            return Complete();
        }

        public new TaskAwaiter<TResult> GetAwaiter() => new TaskAwaiter<TResult>(this);

        internal new TResult GetResult()
        {
            if (Exception != null)
                throw Exception;

            return Result;
        }
    }
}
