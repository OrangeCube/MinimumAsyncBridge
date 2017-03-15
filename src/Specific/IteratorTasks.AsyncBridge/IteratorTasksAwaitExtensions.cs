using IteratorTasks;
using IteratorTasks.AsyncBridge;
using System;

/// <summary>
/// Provides an awaiter for awaiting a <see cref="Task"/>.
/// </summary>
public static class IteratorTasksAwaitExtensions
{
    public static TaskAwaiter GetAwaiter(this Task task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        return new TaskAwaiter(task);
    }

    public static TaskAwaiter<TResult> GetAwaiter<TResult>(this Task<TResult> task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        return new TaskAwaiter<TResult>(task);
}
}
