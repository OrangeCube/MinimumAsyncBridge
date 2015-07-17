using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;

namespace MinimuAsyncBridgeUnitTest
{
    [TestClass]
    public class UnitTestWhenAll
    {
        [TestMethod]
        public void TestWhenAll()
        {
            WhenAllForCompletedTask().Wait();
            WhenAllDelaysShouldAwaitForMaxDelay().Wait();
        }

        private async Task WhenAllDelaysShouldAwaitForMaxDelay()
        {
            var t = DateTime.Now;

            var t1 = Task.Delay(10);
            var t2 = Task.Delay(20);
            var t3 = Task.Delay(50);
            var t4 = Task.Delay(100);

            await Task.WhenAll(
                t1,
                t2,
                t3,
                t4);

            var elapsed = (DateTime.Now) - t;

            // elapsed time is expected to be 100 + some overheads.
            Assert.IsTrue(elapsed.TotalMilliseconds >= 100);
            Assert.IsTrue(elapsed.TotalMilliseconds < 200);
            Assert.AreEqual(t1.Status, TaskStatus.RanToCompletion);
            Assert.AreEqual(t2.Status, TaskStatus.RanToCompletion);
            Assert.AreEqual(t3.Status, TaskStatus.RanToCompletion);
            Assert.AreEqual(t4.Status, TaskStatus.RanToCompletion);
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
    }
}
