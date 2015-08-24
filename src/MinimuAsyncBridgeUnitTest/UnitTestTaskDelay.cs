using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;

namespace MinimuAsyncBridgeUnitTest
{
    [TestClass]
    public class UnitTestTaskDelay
    {
        [TestMethod]
        public void TaskDelayShouldIgnoreCancelAfterIsCompleted()
        {
            TaskDelayShouldCancelIfCanceledAsync().Wait();
        }

        private async Task TaskDelayShouldCancelIfCanceledAsync()
        {
            var cts = new CancellationTokenSource();
            var d = Task.Delay(10, cts.Token);

            await d;

            await Task.Delay(1);

            cts.Cancel();
        }

        [TestMethod]
        public void TaskDelayCanCancel()
        {
            TaskDelayCanCancelAsync().Wait();
        }

        private async Task TaskDelayCanCancelAsync()
        {
            var cts1 = new CancellationTokenSource();
            var d1 = Task.Delay(1000, cts1.Token);
            cts1.Cancel();
            Assert.AreEqual(d1.Status, TaskStatus.Canceled);

            var cts2 = new CancellationTokenSource();
            var d2 = Task.Delay(1000, cts2.Token);

            await Task.Delay(10);

            cts2.Cancel();

            Assert.AreEqual(d2.Status, TaskStatus.Canceled);
        }


        [TestMethod]
        public void TaskDelayWithInitiallyCanceledToken()
        {
            TaskDelayWithInitiallyCanceledTokenAsync().Wait();
        }

        private async Task TaskDelayWithInitiallyCanceledTokenAsync()
        {
            var cts1 = new CancellationTokenSource();
            cts1.Cancel();
            var d1 = Task.Delay(1000, cts1.Token);
            Assert.AreEqual(d1.Status, TaskStatus.Canceled);

            var exceptionCount = 0;
            try
            {
                await d1;
            }
            catch(Exception ex)
            {
                Assert.AreEqual(ex.GetType(), typeof(TaskCanceledException));
                exceptionCount++;
            }
            Assert.AreEqual(exceptionCount, 1);
        }
    }
}
