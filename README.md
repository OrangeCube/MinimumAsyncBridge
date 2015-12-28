# MinimumAsyncBridge

Miminum set of the async/await portability libararies.

## Usage

NuGet packages:

- [MinimumThreadingBridge](https://www.nuget.org/packages/MinimumThreadingBridge/)
  - contains: `CancellationToken`, `IProgress<T>`, ...
- [MinimumAsyncBridge](https://www.nuget.org/packages/MinimumAsyncBridge/)
  - contains: `Task`, `Task<TResult>`, `TaskCompletionSource<TResult>`, `TaskAwaiter`, ...
- [MvvmBridge](https://www.nuget.org/packages/MvvmBridge/)
  - contains: `INotifyCollectionChanged`, `ObservableCollection<T>`, `CallerMemberNameAttribute`, ...

These packages includes:

- Back-porting implementation for .NET 3.5
- Type forwarding for .NET 4.5 or later

So, these libraries are supported in mixed versions with both 3.5 and 4.5 (but not with .NET 4).
You can create a library with .NET 3.5 with the back-ports, and then, consume it from an app with .NET 4.5.

## Motivation

I'd like to use async/await functionality in [Unity game engine](http://unity3d.com/).
The runtime version of Mono used by Unity is 2.8 which is nearly equivalent to .NET 3.5 and does not have the `Task` class.

In this background, I have implemented back-porting libararies of the classes related to async/await  to .NET 3.5, especially Unity.
The back-porting libararies includes the `Task`, `TaskCompletionSource`, `TaskAwaiter`, and so on.

## Implementation

The implementation is based on code in [https://github.com/Microsoft/referencesource](https://github.com/Microsoft/referencesource), 
I use [tunnelvisionlabs/dotnet-threading#90](https://github.com/tunnelvisionlabs/dotnet-threading/pull/90) as a reference, 
and the MinimumAsyncBridge has the same problem as the dotnet-threading.

The `Task` class in .NET has multiple roles:

1. threading: `Task.Run`
1. timer: `Task.Delay`
1. future/promise: `TaskCompletionSource`

ThE back-porting libararies implement only 2. and 3. (Minimum requirement of async/await is 3.). 
For 1., only a few overloads of `Task.Run` and `WhenAny` and `WhenAll` are implemented (but, it's simplified implementation).
If you would need the threading capability, you could use another threading library and interop by using `TaskCompletionSource`.

## Known problem

The debbugability of the `Task` in the MinimumAsyncBridge is poorer than original `Task`.
Stack trace information is lost because the back-port does not have `System.Runtime.ExceptionServices.ExceptionDispatchInfo`,
which is impossible without runtime support.

## Notice: Use in Unity

The back-porting libararies contains classes required for async/await. 
However, the C# 5.0 (or later) compiler is also required. Thus:

- you can use async/await with .NET 3.5 and these libararies with the C# 5.0 compiler.
- if you create a DLL with the C# 5.0 compiler and copy it to a Unity project, you can use async/await in the DLL.
- but, you can still not use async/await with the compiler bundled with Unity.

