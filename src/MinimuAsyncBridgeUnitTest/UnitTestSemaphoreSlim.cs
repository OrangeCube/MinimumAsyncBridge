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
        public void TestCancelWaitAsync()
        {
            TestCancelWaitAsyncInternal().Wait();
            TestCancelWaitAsyncInternal2().Wait();
        }

        private async Task TestCancelWaitAsyncInternal()
        {
            var r = new Random();
            var s = new SemaphoreSlim(1);

            await Task.WhenAll(Enumerable.Range(0, 100).Select(async i =>
            {
                var ct = CancellationToken.None;
                if((i % 5) == 0)
                {
                    var cts = new CancellationTokenSource();
                    var t = Task.Delay(1).ContinueWith(_ => cts.Cancel());
                    ct = cts.Token;
                }

                try
                {
                    await s.WaitAsync(ct);
                    await Delay(r, ct).ConfigureAwait(false);
                }
                catch (TestException) { }
                catch (OperationCanceledException) { }
                finally
                {
                    s.Release();
                }
            }));
        }

        private async Task TestCancelWaitAsyncInternal2()
        {
            var s = new SemaphoreSlim(1);
            var cts = new CancellationTokenSource();

            var wait1 = WaitAsync(s, cts);
            var wait2 = WaitWithCancellationTokenAsync(s, cts.Token);

            await Task.WhenAll(wait1, wait2);
        }

        private async Task WaitAsync(SemaphoreSlim s, CancellationTokenSource cts)
        {
            await s.WaitAsync();
            await Task.Delay(10);
            cts.Cancel();
            s.Release();
        }

        private async Task WaitWithCancellationTokenAsync(SemaphoreSlim s, CancellationToken ct)
        {
            await s.WaitAsync(ct);

            if (ct.IsCancellationRequested)
                return;

            await Task.Delay(10);
            s.Release();
        }

        [TestMethod]
        public void TestWaitAsync()
        {
            TestWaitAsyncInternal().Wait();
        }

        private async Task TestWaitAsyncInternal()
        {
            await TestWaitAsyncInternal(parallel: 50, innerLoop: 100, outerLoop: 3, due: true);
            await TestWaitAsyncInternal(parallel: 50, innerLoop: 1, outerLoop: 100, due: false);
        }

        private async Task TestWaitAsyncInternal(int parallel, int innerLoop, int outerLoop, bool due)
        {
            var r = new Random();
            var s = new SemaphoreSlim(1);

            for (int i = 0; i < outerLoop; i++)
            {
                var count = new Integer();

                await Task.WhenAll(Enumerable.Range(0, parallel)
                    .Select(_ => ExclusiveTask(s, count, r.Next(), innerLoop, due))
                    .ToArray());

                Assert.AreEqual(parallel * innerLoop, count.Value);
            }
        }

        class Integer
        {
            public int Value { get; set; }
        }

        private async Task ExclusiveTask(SemaphoreSlim s, Integer count, int seed, int iteration, bool due)
        {
            var r = new Random(seed);

            if (due)
            {
                try
                {
                    await Delay(r).ConfigureAwait(false);
                }
                catch { }
            }

            for (int i = 0; i < iteration; i++)
            {
                int localCount = 0;
                try
                {
                    await s.WaitAsync();
                    localCount = count.Value;
                    await Delay(r).ConfigureAwait(false);
                }
                catch (TestException) { }
                finally
                {
                    count.Value = localCount + 1;
                    s.Release();
                }
            }
        }

        class TestException : Exception { }

        private async Task Delay(Random random, CancellationToken ct = default(CancellationToken))
        {
            var selection = random.Next() % 100;

            if(selection < 2)
            {
                await Task.Delay(random.Next(3), ct).ConfigureAwait(false);
            }
            else if(selection < 5)
            {
                throw new TestException();
            }
            else if(selection < 20)
            {
                await Task.Run(() => { }).ConfigureAwait(false);
            }
            else if (selection < 90)
            {
                await Task.Run(() =>
                {
                    var count = random.Next(10);
                    for (var i = 0; i < count && !ct.IsCancellationRequested; i++) ;
                }).ConfigureAwait(false);
            }
            else
            {
                var count = random.Next(10);
                for (var i = 0; i < count && !ct.IsCancellationRequested; i++) ;
            }
        }
    }
}
