#if NET40PLUS

using System.Runtime.CompilerServices;

[assembly: TypeForwardedTo(typeof(AsyncTaskMethodBuilder<>))]

#else

namespace System.Runtime.CompilerServices
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides a builder for asynchronous methods that return <see cref="Task{TResult}"/>. This type is intended for
    /// compiler use only.
    /// </summary>
    /// <remarks>
    /// <see cref="AsyncTaskMethodBuilder{TResult}"/> is a value type, and thus it is copied by value. Prior to being
    /// copied, one of its <see cref="Task"/>, <see cref="SetResult(TResult)"/>, or
    /// <see cref="SetException(Exception)"/> members must be accessed, or else the copies may end up building distinct
    /// <see cref="Threading.Tasks.Task"/> instances.
    /// </remarks>
    public struct AsyncTaskMethodBuilder<TResult> : IAsyncMethodBuilder
    {
        /// <summary>A cached task for default(<typeparamref name="TResult"/>).</summary>
        internal static readonly TaskCompletionSource<TResult> _defaultResultTask;

        /// <summary>State related to the <see cref="IAsyncStateMachine"/>.</summary>
        private AsyncMethodBuilderCore _coreState;

        /// <summary>The lazily-initialized task.</summary>
        /// <remarks>Must be named <c>m_task</c> for debugger step-over to work correctly.</remarks>
        private Task<TResult> m_task;

        /// <summary>The lazily-initialized task completion source.</summary>
        private TaskCompletionSource<TResult> _taskCompletionSource;

        /// <summary>Gets the lazily-initialized <see cref="TaskCompletionSource{TResult}"/>.</summary>
        internal TaskCompletionSource<TResult> CompletionSource
        {
            get
            {
                TaskCompletionSource<TResult> taskCompletionSource = _taskCompletionSource;
                if (taskCompletionSource == null)
                {
                    taskCompletionSource = (_taskCompletionSource = new TaskCompletionSource<TResult>());
                    m_task = taskCompletionSource.Task;
                }

                return taskCompletionSource;
            }
        }

        /// <summary>Gets the <see cref="Task{TResult}"/> for this builder.</summary>
        /// <returns>The <see cref="Task{TResult}"/> representing the builder's asynchronous operation.</returns>
        public Task<TResult> Task
        {
            get
            {
                TaskCompletionSource<TResult> completionSource = CompletionSource;
                return completionSource.Task;
            }
        }

        /// <summary>
        /// Gets an object that may be used to uniquely identify this builder to the debugger.
        /// </summary>
        /// <remarks>
        /// This property lazily instantiates the ID in a non-thread-safe manner. It must only be used by the debugger,
        /// and only in a single-threaded manner when no other threads are in the middle of accessing this property or
        /// <see cref="Task"/>.
        /// </remarks>
        object IAsyncMethodBuilder.ObjectIdForDebugger
        {
            get
            {
                return Task;
            }
        }

        /// <summary>Temporary support for disabling crashing if tasks go unobserved.</summary>
        static AsyncTaskMethodBuilder()
        {
            _defaultResultTask = AsyncMethodTaskCache<TResult>.CreateCompleted(default(TResult));
            try
            {
                AsyncVoidMethodBuilder.PreventUnobservedTaskExceptions();
            }
            catch
            {
            }
        }

        /// <summary>Initializes a new <see cref="AsyncTaskMethodBuilder"/>.</summary>
        /// <returns>The initialized <see cref="AsyncTaskMethodBuilder"/>.</returns>
        public static AsyncTaskMethodBuilder<TResult> Create()
        {
            return default(AsyncTaskMethodBuilder<TResult>);
        }

        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
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
            Task<TResult> ignored = Task;
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

        /// <summary>
        /// Completes the <see cref="Task{TResult}"/> in the <see cref="TaskStatus.RanToCompletion"/> state with the
        /// specified result.
        /// </summary>
        /// <param name="result">The result to use to complete the task.</param>
        /// <exception cref="InvalidOperationException">The task has already completed.</exception>
        public void SetResult(TResult result)
        {
            TaskCompletionSource<TResult> taskCompletionSource = _taskCompletionSource;
            if (taskCompletionSource == null)
            {
                _taskCompletionSource = GetTaskForResult(result);
                m_task = _taskCompletionSource.Task;
                return;
            }

            if (!taskCompletionSource.TrySetResult(result))
            {
                throw new InvalidOperationException("The Task was already completed.");
            }
        }

        /// <summary>
        /// Completes the builder by using either the supplied completed task, or by completing
        /// the builder's previously accessed task using default(<typeparamref name="TResult"/>).
        /// </summary>
        /// <param name="completedTask">
        /// A task already completed with the value default(<typeparamref name="TResult"/>).
        /// </param>
        /// <exception cref="System.InvalidOperationException">The task has already completed.</exception>
        internal void SetResult(TaskCompletionSource<TResult> completedTask)
        {
            if (_taskCompletionSource == null)
            {
                _taskCompletionSource = completedTask;
                m_task = _taskCompletionSource.Task;
                return;
            }

            SetResult(default(TResult));
        }

        /// <summary>
        /// Completes the <see cref="Task{TResult}"/> in the <see cref="TaskStatus.Faulted"/> state with the specified
        /// exception.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to use to fault the task.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="exception"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">The task has already completed.</exception>
        public void SetException(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            TaskCompletionSource<TResult> completionSource = CompletionSource;
            if (!((exception is OperationCanceledException) ? completionSource.TrySetCanceled() : completionSource.TrySetException(exception)))
            {
                throw new InvalidOperationException("The Task was already completed.");
            }
        }

        /// <summary>
        /// Called by the debugger to request notification when the first wait operation (await, Wait, Result, etc.) on
        /// this builder's task completes.
        /// </summary>
        /// <param name="enabled">
        /// <see langword="true"/> to enable notification; <see langword="false"/> to disable a previously set
        /// notification.
        /// </param>
        /// <remarks>
        /// This should only be invoked from within an asynchronous method, and only by the debugger.
        /// </remarks>
        internal void SetNotificationForWaitCompletion(bool enabled)
        {
        }

        /// <summary>
        /// Gets a task for the specified result. This will either be a cached or new task, never
        /// <see langword="null"/>.
        /// </summary>
        /// <param name="result">The result for which we need a task.</param>
        /// <returns>The completed task containing the result.</returns>
        private TaskCompletionSource<TResult> GetTaskForResult(TResult result)
        {
            AsyncMethodTaskCache<TResult> singleton = AsyncMethodTaskCache<TResult>.Singleton;
            if (singleton == null)
            {
                return AsyncMethodTaskCache<TResult>.CreateCompleted(result);
            }

            return singleton.FromResult(result);
        }
    }
}

#endif
