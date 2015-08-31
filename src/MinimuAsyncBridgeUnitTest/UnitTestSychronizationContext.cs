using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace MinimuAsyncBridgeUnitTest
{
    [TestClass]
    public class UnitTestSychronizationContext
    {
        [TestMethod]
        public void AwaitOperatorShouldPreserveSynchronizationContext()
        {
            var c = new SingleThreadSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(c);

            RunRandomTasksAsync().Wait();

            c.Stop();
        }

        private static async Task RunRandomTasksAsync()
        {
            await Task.Delay(1);
            var r = new Random();
            await Task.WhenAll(Enumerable.Range(0, 1000).Select(_ => RunRandomTasksAsync(r.Next())));
        }

        private static async Task RunRandomTasksAsync(int seed)
        {
            var r = new Random(seed);
            for (int i = 0; i < 10000; i++)
            {
                await Task.Delay(TimeSpan.FromTicks(r.Next(1, 100)));

                var sync = SynchronizationContext.Current as SingleThreadSynchronizationContext;

                if (sync == null)
                    Assert.Fail();

                var tid = Thread.CurrentThread.ManagedThreadId;
                if (tid != sync.MainThreadId)
                    Assert.Fail();
            }
        }
    }
}
