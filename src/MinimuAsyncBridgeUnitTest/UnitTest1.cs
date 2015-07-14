using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;

namespace MinimuAsyncBridgeUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCompletedTask()
        {
            AwaitCompletedTaskShouldBeExecutedSynchronously().Wait();
        }

        private async Task AwaitCompletedTaskShouldBeExecutedSynchronously()
        {
            var tid0 = Thread.CurrentThread.ManagedThreadId;

            {
                var t = DateTime.Now;
                await Task.CompletedTask;
                var elapsed = (DateTime.Now) - t;
                Assert.IsTrue(elapsed.TotalSeconds < 20);
                var tid = Thread.CurrentThread.ManagedThreadId;
                Assert.AreEqual(tid0, tid);
            }
            {
                var t = DateTime.Now;
                await Task.FromResult(10);
                var elapsed = (DateTime.Now) - t;
                Assert.IsTrue(elapsed.TotalSeconds < 20);
                var tid = Thread.CurrentThread.ManagedThreadId;
                Assert.AreEqual(tid0, tid);
            }
            {
                var t = DateTime.Now;
                await Task.FromResult("");
                var elapsed = (DateTime.Now) - t;
                Assert.IsTrue(elapsed.TotalSeconds < 20);
                var tid = Thread.CurrentThread.ManagedThreadId;
                Assert.AreEqual(tid0, tid);
            }
        }

        [TestMethod]
        public void TestWhenAll()
        {
            WhenAllForCompletedTask().Wait();
            WhenAllDelaysShouldAwaitForMaxDelay().Wait();
        }

        private async Task WhenAllDelaysShouldAwaitForMaxDelay()
        {
            var t = DateTime.Now;

            await Task.WhenAll(
                Task.Delay(10),
                Task.Delay(20),
                Task.Delay(50),
                Task.Delay(100));

            var elapsed = (DateTime.Now) - t;

            // elapsed time is expected to be 100 + some overheads.
            Assert.IsTrue(elapsed.TotalSeconds >= 100);
            Assert.IsTrue(elapsed.TotalSeconds < 200);
        }

        private async Task WhenAllForCompletedTask()
        {
            var t = DateTime.Now;

            await Task.WhenAll(
                Task.CompletedTask,
                Task.Delay(10));

            await Task.WhenAll(
                Task.CompletedTask,
                Task.CompletedTask,
                Task.Delay(10));

            await Task.WhenAll(
                Task.CompletedTask,
                Task.CompletedTask,
                Task.CompletedTask,
                Task.CompletedTask);
        }

        [TestMethod]
        public void TestWhenAny()
        {
            WhenAnyDelaysShouldAwaitForMinDelay().Wait();
        }

        private async Task WhenAnyDelaysShouldAwaitForMinDelay()
        {
            var t = DateTime.Now;

            await Task.WhenAny(
                Task.Delay(10),
                Task.Delay(20),
                Task.Delay(50),
                Task.Delay(100));

            var elapsed = (DateTime.Now) - t;

            // elapsed time is expected to be 10 + some overheads.
            Assert.IsTrue(elapsed.TotalSeconds >= 10);
            Assert.IsTrue(elapsed.TotalSeconds < 30);
        }
    }
}
