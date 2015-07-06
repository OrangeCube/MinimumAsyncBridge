#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(INotifyCompletion))]

#else

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Represents an operation that will schedule continuations when the operation completes.
    /// </summary>
    public interface INotifyCompletion
    {
        /// <summary>Schedules the continuation action to be invoked when the instance completes.</summary>
        /// <param name="continuation">The action to invoke when the operation completes.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="continuation"/> is <see langword="null"/>.
        /// </exception>
        void OnCompleted(Action continuation);
    }
}

#endif
