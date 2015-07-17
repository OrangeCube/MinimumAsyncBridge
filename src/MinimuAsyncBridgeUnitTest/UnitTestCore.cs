using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;

namespace MinimuAsyncBridgeUnitTest
{
    [TestClass]
    public class UnitTestCore
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
        public void TestFromException()
        {
            AwaitOnTaskFromExceptionShouldThrow().Wait();
        }

        private async Task AwaitOnTaskFromExceptionShouldThrow()
        {
            try
            {
                await Task.FromException<int>(new InvalidOperationException());
            }
            catch (InvalidOperationException)
            {
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void TestCancellationTokenSource()
        {
            CancellationTokenSource_CancelShouldWorkIfFirstTime();
        }

        private void CancellationTokenSource_CancelShouldWorkIfFirstTime()
        {
            var cancelCount = 0;
            var cts = new CancellationTokenSource();
            cts.Token.Register(() => { cancelCount++; cts.Cancel(); });
            cts.Cancel();

            Assert.IsTrue(cancelCount == 1);
        }
    }
}
