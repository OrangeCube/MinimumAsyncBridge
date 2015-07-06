#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(AsyncStateMachineAttribute))]

#else

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Indicates whether a method is marked with the <see langword="async"/> modifier.
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class AsyncStateMachineAttribute : StateMachineAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncStateMachineAttribute"/> class.
        /// </summary>
        /// <param name="stateMachineType">
        /// The type object for the underlying state machine type that's used to implement a state machine method.
        /// </param>
        public AsyncStateMachineAttribute(Type stateMachineType)
            : base(stateMachineType)
        {
        }
    }
}

#endif
