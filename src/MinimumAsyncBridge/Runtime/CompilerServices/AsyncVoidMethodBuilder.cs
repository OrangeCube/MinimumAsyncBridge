#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(AsyncVoidMethodBuilder))]

#else

namespace System.Runtime.CompilerServices
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a builder for asynchronous methods that return void. This type is intended for compiler use only.
    /// </summary>
    public struct AsyncVoidMethodBuilder : IAsyncMethodBuilder
    {
        /// <summary>The synchronization context associated with this operation.</summary>
        private readonly SynchronizationContext _synchronizationContext;

        /// <summary>State related to the <see cref="IAsyncStateMachine"/>.</summary>
        private AsyncMethodBuilderCore _coreState;

        /// <summary>An object used by the debugger to uniquely identify this builder. Lazily initialized.</summary>
        private object _objectIdForDebugger;

        /// <summary>Non-zero if <see cref="PreventUnobservedTaskExceptions"/> has already been invoked.</summary>
        private static int _preventUnobservedTaskExceptionsInvoked;

        /// <summary>
        /// Gets an object that may be used to uniquely identify this builder to the debugger.
        /// </summary>
        /// <remarks>
        /// This property lazily instantiates the ID in a non-thread-safe manner. It must only be used by the debugger
        /// and only in a single-threaded manner.
        /// </remarks>
        object IAsyncMethodBuilder.ObjectIdForDebugger
        {
            get
            {
                if (_objectIdForDebugger == null)
                {
                    _objectIdForDebugger = new object();
                }

                return _objectIdForDebugger;
            }
        }

        /// <summary>Temporary support for disabling crashing if tasks go unobserved.</summary>
        static AsyncVoidMethodBuilder()
        {
            try
            {
                PreventUnobservedTaskExceptions();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Registers with <see cref="TaskScheduler.UnobservedTaskException"/> to suppress exception crashing.
        /// </summary>
        internal static void PreventUnobservedTaskExceptions()
        {
            //if (Interlocked.CompareExchange(ref _preventUnobservedTaskExceptionsInvoked, 1, 0) == 0)
            //{
            //    TaskScheduler.UnobservedTaskException += (s, e) => e.SetObserved();
            //}
        }

        /// <summary>Initializes a new <see cref="AsyncVoidMethodBuilder"/>.</summary>
        /// <returns>The initialized <see cref="AsyncVoidMethodBuilder"/>.</returns>
        public static AsyncVoidMethodBuilder Create()
        {
            return new AsyncVoidMethodBuilder(SynchronizationContext.Current);
        }

        /// <summary>Initializes the <see cref="AsyncVoidMethodBuilder"/>.</summary>
        /// <param name="synchronizationContext">
        /// The synchronization context associated with this operation. This may be <see langword="null"/>.
        /// </param>
        private AsyncVoidMethodBuilder(SynchronizationContext synchronizationContext)
        {
            _synchronizationContext = synchronizationContext;
            if (synchronizationContext != null)
            {
                synchronizationContext.OperationStarted();
            }

            _coreState = default(AsyncMethodBuilderCore);
            _objectIdForDebugger = null;
        }

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stateMachine"/> is <see langword="null"/>.
        /// </exception>
        [DebuggerStepThrough]
        public void Start<TStateMachine>(ref TStateMachine stateMachine)
            where TStateMachine : IAsyncStateMachine
        {
            _coreState.Start(ref stateMachine);
        }

        /// <summary>Associates the builder with the state machine it represents.</summary>
        /// <param name="stateMachine">The heap-allocated state machine object.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="stateMachine"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The builder is incorrectly initialized.</exception>
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            _coreState.SetStateMachine(stateMachine);
        }

        /// <summary>Perform any initialization necessary prior to lifting the builder to the heap.</summary>
        void IAsyncMethodBuilder.PreBoxInitialization<TStateMachine>(ref TStateMachine stateMachine)
        {
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                Action completionAction = _coreState.GetCompletionAction(ref this, ref stateMachine);
                awaiter.OnCompleted(completionAction);
            }
            catch (Exception exception)
            {
                AsyncServices.ThrowAsync(exception, null);
            }
        }

        /// <summary>
        /// Schedules the specified state machine to be pushed forward when the specified awaiter completes.
        /// </summary>
        /// <typeparam name="TAwaiter">Specifies the type of the awaiter.</typeparam>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="awaiter">The awaiter.</param>
        /// <param name="stateMachine">The state machine.</param>
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            try
            {
                Action completionAction = _coreState.GetCompletionAction(ref this, ref stateMachine);
                awaiter.UnsafeOnCompleted(completionAction);
            }
            catch (Exception exception)
            {
                AsyncServices.ThrowAsync(exception, null);
            }
        }

        /// <summary>Completes the method builder successfully.</summary>
        public void SetResult()
        {
            if (_synchronizationContext != null)
            {
                NotifySynchronizationContextOfCompletion();
            }
        }

        /// <summary>Faults the method builder with an exception.</summary>
        /// <param name="exception">The exception that is the cause of this fault.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="exception"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The builder is not initialized.</exception>
        public void SetException(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            if (_synchronizationContext != null)
            {
                try
                {
                    AsyncServices.ThrowAsync(exception, _synchronizationContext);
                    return;
                }
                finally
                {
                    NotifySynchronizationContextOfCompletion();
                }
            }

            AsyncServices.ThrowAsync(exception, null);
        }

        /// <summary>Notifies the current synchronization context that the operation completed.</summary>
        private void NotifySynchronizationContextOfCompletion()
        {
            try
            {
                _synchronizationContext.OperationCompleted();
            }
            catch (Exception exception)
            {
                AsyncServices.ThrowAsync(exception, null);
            }
        }
    }
}

#endif
