using System;
using UniRx;
using UniRx.AsyncBridge;
/// <summary>
/// Provides an awaiter for awaiting a <see cref="IteratorTasks.Task"/>.
/// </summary>
public static class AsyncSubjectAwaitExtensions
{
    public static AsyncSubjectAwaiter<TResult> GetAwaiter<TResult>(this AsyncSubject<TResult> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        return new AsyncSubjectAwaiter<TResult>(source);
    }


    public static AsyncSubjectAwaiter<TSource> GetAwaiter<TSource>(this IObservable<TSource> source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        var s = new AsyncSubject<TSource>();
        source.Subscribe(s);
        return GetAwaiter(s);
    }
}
