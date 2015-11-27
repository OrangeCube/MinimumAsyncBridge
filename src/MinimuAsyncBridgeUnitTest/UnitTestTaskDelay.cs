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
            var delays = new[] { 1000, 100, 10, 1 };

            // cancel immediately
            foreach (var delay in delays)
            {
                var cts = new CancellationTokenSource();
                var d = Task.Delay(delay, cts.Token);
                cts.Cancel();
                Assert.AreEqual(TaskStatus.Canceled, d.Status);
            }

            var longerDelays = new[] { 1000, 100 };

            foreach (var delay in longerDelays)
            {
                var cts = new CancellationTokenSource();
                var d = Task.Delay(delay, cts.Token);

                await Task.Delay(10);

                cts.Cancel();
                Assert.AreEqual(TaskStatus.Canceled, d.Status);
            }

            var shorterDelays = new[] { 5, 1 };

            foreach (var delay in shorterDelays)
            {
                var cts = new CancellationTokenSource();
                var d = Task.Delay(delay, cts.Token);

                await Task.Delay(100);

                cts.Cancel();
                Assert.AreEqual(TaskStatus.RanToCompletion, d.Status);
            }
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
