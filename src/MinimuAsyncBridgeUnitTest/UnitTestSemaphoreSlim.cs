using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace MinimuAsyncBridgeUnitTest
{
    [TestClass]
    public class UnitTestSemaphoreSlim
    {
        [TestMethod]
        public void TestWaitAsync()
        {
            TestWaitAsyncInternal().Wait();
        }

        private async Task TestWaitAsyncInternal()
        {
            const int N = 1000;
            const double MaxDelayMilliseconds = 1;

            var r = new Random();
            var s = new SemaphoreSlim(1);
            var count = new Integer();

            await Task.WhenAll(Enumerable.Range(0, N)
                .Select(_ => ExclusiveTask(s, count,
                    TimeSpan.FromMilliseconds(r.NextDouble() * MaxDelayMilliseconds),
                    TimeSpan.FromMilliseconds(r.NextDouble() * MaxDelayMilliseconds)
                    ))
                .ToArray());

            Assert.AreEqual(N, count.Value);
        }

        class Integer
        {
            public int Value { get; set; }
        }

        private async Task ExclusiveTask(SemaphoreSlim s, Integer count, TimeSpan delay1, TimeSpan delay2)
        {
            await Task.Delay(delay1).ConfigureAwait(false);

            try
            {
                await s.WaitAsync();

                var localCount = count.Value;

                await Task.Delay(delay2).ConfigureAwait(false);

                count.Value = localCount + 1;
            }
            finally
            {
                s.Release();
            }
        }
    }
}
