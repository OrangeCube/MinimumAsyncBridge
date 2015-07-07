using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample35
{
    public class TaskEx
    {
        public static Task Delay(TimeSpan delay)
        {
            var tcs = new TaskCompletionSource<bool>();

            if (delay <= TimeSpan.Zero)
            {
                tcs.SetResult(false);
                return tcs.Task;
            }

            Timer t = null;
            t = new Timer(_ =>
            {
                t.Dispose();
                tcs.SetResult(false);
                t = null;
                tcs = null;
            }, null, (int)delay.TotalMilliseconds, Timeout.Infinite);

            return tcs.Task;
        }
    }
}
