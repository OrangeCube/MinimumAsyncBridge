using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    public struct TaskAwaiter : ICriticalNotifyCompletion
    {
        Task _t;

        internal TaskAwaiter(Task t) { _t = t; }

        public void OnCompleted(Action continuation) => _t.OnCompleted(continuation);
        public void UnsafeOnCompleted(Action continuation) => _t.UnsafeOnCompleted(continuation);
        public bool IsCompleted => _t.IsCompleted;
        public void GetResult() => _t.GetResult();
    }
}
