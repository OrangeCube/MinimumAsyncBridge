using System;
using System.Runtime.CompilerServices;

namespace UniRx.AsyncBridge
{
    public struct AsyncSubjectAwaiter<TResult> : INotifyCompletion
    {
        private readonly AsyncSubject<TResult> _subject;

        internal AsyncSubjectAwaiter(AsyncSubject<TResult> subject)
        {
            _subject = subject;
        }

        public bool IsCompleted => _subject.IsCompleted;

        public void OnCompleted(Action continuation)
        {
            if (_subject.IsCompleted)
                continuation();
            else
                _subject.ObserveOn(Scheduler.CurrentThread).First().Subscribe(_ => continuation());
        }

        public TResult GetResult() => _subject.Value;
    }
}
