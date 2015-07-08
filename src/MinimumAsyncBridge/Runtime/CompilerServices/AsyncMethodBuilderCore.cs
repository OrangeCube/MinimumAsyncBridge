#if !NET40PLUS

namespace System.Runtime.CompilerServices
{
    using System.Diagnostics;
    using System.Threading;


    /// <summary>Holds state related to the builder's <see cref="IAsyncStateMachine"/>.</summary>
    /// <remarks>This is a mutable struct. Be very delicate with it.</remarks>
    internal struct AsyncMethodBuilderCore
    {
        /// <summary>A reference to the heap-allocated state machine object associated with this builder.</summary>
        internal IAsyncStateMachine _stateMachine;

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stateMachine"/> is <see langword="null"/>.
        /// </exception>
        [DebuggerStepThrough]
        internal void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            if (stateMachine == null)
                throw new ArgumentNullException("stateMachine");

            stateMachine.MoveNext();
        }

        /// <summary>Associates the builder with the state machine it represents.</summary>
        /// <param name="stateMachine">The heap-allocated state machine object.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stateMachine"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The builder is incorrectly initialized.</exception>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            if (stateMachine == null)
                throw new ArgumentNullException("stateMachine");
            if (_stateMachine != null)
                throw new InvalidOperationException("The builder was not properly initialized.");

            _stateMachine = stateMachine;
        }

        /// <summary>
        /// Gets the <see cref="Action"/> to use with an awaiter's <see cref="INotifyCompletion.OnCompleted"/> or
        /// <see cref="ICriticalNotifyCompletion.UnsafeOnCompleted"/> method. On first invocation, the supplied state
        /// machine will be boxed.
        /// </summary>
        /// <typeparam name="TMethodBuilder">Specifies the type of the method builder used.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine used.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="stateMachine">The state machine.</param>
        /// <returns>An <see cref="Action"/> to provide to the awaiter.</returns>
        internal Action GetCompletionAction<TMethodBuilder, TStateMachine>(ref TMethodBuilder builder, ref TStateMachine stateMachine)
            where TMethodBuilder : IAsyncMethodBuilder
            where TStateMachine : IAsyncStateMachine
        {
            ExecutionContext context = ExecutionContext.Capture();
            MoveNextRunner moveNextRunner = new MoveNextRunner(context);
            Action result = moveNextRunner.Run;
            if (_stateMachine == null)
            {
                builder.PreBoxInitialization(ref stateMachine);
                _stateMachine = stateMachine;
                _stateMachine.SetStateMachine(_stateMachine);
            }

            moveNextRunner._stateMachine = _stateMachine;
            return result;
        }

        /// <summary>
        /// Provides the ability to invoke a state machine's <see cref="IAsyncStateMachine.MoveNext"/> method under a
        /// supplied <see cref="ExecutionContext"/>.
        /// </summary>
        private sealed class MoveNextRunner
        {
            /// <summary>The context with which to run <see cref="IAsyncStateMachine.MoveNext"/>.</summary>
            private readonly ExecutionContext _context;

            /// <summary>
            /// The state machine whose <see cref="IAsyncStateMachine.MoveNext"/> method should be invoked.
            /// </summary>
            internal IAsyncStateMachine _stateMachine;

            /// <summary>Cached delegate used with <see cref="ExecutionContext.Run"/>.</summary>
            private static Action<object> _invokeMoveNext;

            /// <summary>Initializes the runner.</summary>
            /// <param name="context">The context with which to run <see cref="IAsyncStateMachine.MoveNext"/>.</param>
            internal MoveNextRunner(ExecutionContext context)
            {
                _context = context;
            }

            /// <summary>Invokes <see cref="IAsyncStateMachine.MoveNext"/> under the provided context.</summary>
            internal void Run()
            {
                if (_context != null)
                {
                    try
                    {
                        Action<object> action = _invokeMoveNext;
                        if (action == null)
                        {
                            action = (_invokeMoveNext = InvokeMoveNext);
                        }

                        if (_context == null)
                        {
                            action.Invoke(_stateMachine);
                        }
                        else
                        {
                            ExecutionContext.Run(_context, x => action(x), _stateMachine);
                        }

                        return;
                    }
                    finally
                    {
                    }
                }

                _stateMachine.MoveNext();
            }

            /// <summary>
            /// Invokes the <see cref="IAsyncStateMachine.MoveNext"/> method on the supplied
            /// <see cref="IAsyncStateMachine"/>.
            /// </summary>
            /// <param name="stateMachine">The <see cref="IAsyncStateMachine"/> machine instance.</param>
            private static void InvokeMoveNext(object stateMachine)
            {
                ((IAsyncStateMachine)stateMachine).MoveNext();
            }
        }
    }
}

#endif
