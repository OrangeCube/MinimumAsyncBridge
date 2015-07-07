using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: TypeForwardedTo(typeof(AggregateException))]
[assembly: TypeForwardedTo(typeof(Task))]
[assembly: TypeForwardedTo(typeof(Task<>))]
[assembly: TypeForwardedTo(typeof(TaskCanceledException))]
[assembly: TypeForwardedTo(typeof(TaskCompletionSource<>))]
[assembly: TypeForwardedTo(typeof(TaskStatus))]

[assembly: TypeForwardedTo(typeof(AsyncStateMachineAttribute))]
[assembly: TypeForwardedTo(typeof(AsyncTaskMethodBuilder))]
[assembly: TypeForwardedTo(typeof(AsyncTaskMethodBuilder<>))]
[assembly: TypeForwardedTo(typeof(AsyncVoidMethodBuilder))]
[assembly: TypeForwardedTo(typeof(IAsyncStateMachine))]
[assembly: TypeForwardedTo(typeof(ICriticalNotifyCompletion))]
[assembly: TypeForwardedTo(typeof(INotifyCompletion))]
[assembly: TypeForwardedTo(typeof(IteratorStateMachineAttribute))]
[assembly: TypeForwardedTo(typeof(StateMachineAttribute))]

[assembly: TypeForwardedTo(typeof(TaskAwaiter))]
[assembly: TypeForwardedTo(typeof(TaskAwaiter<>))]
