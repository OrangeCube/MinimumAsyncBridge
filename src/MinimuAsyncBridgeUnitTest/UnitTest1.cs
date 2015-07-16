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

        [TestMethod]
        public void TestWhenAny()
        {
            WhenAnyDelaysShouldAwaitForMinDelay().Wait();
            WhenAnyDelaysShouldAwaitForMinDelayWithTResult().Wait();
        }

        [TestMethod]
        public void WhenAnyShouldBeCanceledIfFirstItemOfTheTasksIsCancled()
        {
            WhenAnyShouldBeCanceledIfFirstItemOfTheTasksIsCancledAsync().Wait();
            WhenAnyShouldBeCanceledIfFirstItemOfTheTasksOfTResultIsCancledAsync().Wait();
        }

        [TestMethod]
        public void WhenAnyで最初に例外で終わった場合はResultに例外がでたタスクが返る()
        {
            WhenAnyShouldHaveExceptionIfFirstItemOfTheTasksGetErrorAsync().Wait();
            WhenAnyShouldHaveExceptionIfFirstItemOfTheTasksOfTResultGetErrorAsync().Wait();
        }

        private async Task WhenAnyShouldBeCanceledIfFirstItemOfTheTasksOfTResultIsCancledAsync()
        {
            var tcs = new TaskCompletionSource<int>();
            Task.Delay(10).ContinueWith(_ => tcs.SetCanceled());
            var t2 = Task.Delay(500).ContinueWith(_ => 2);

            var r = await Task.WhenAny<int>(tcs.Task, t2);

            Assert.AreSame(r, tcs.Task);
            Assert.AreEqual(r.Result, default(int));
        }

        private async Task WhenAnyShouldBeCanceledIfFirstItemOfTheTasksIsCancledAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            Task.Delay(10).ContinueWith(_ => tcs.SetCanceled());
            var t2 = Task.Delay(500);

            var r = await Task.WhenAny(tcs.Task, t2);

            Assert.AreSame(r, tcs.Task);
        }

        private async Task WhenAnyShouldHaveExceptionIfFirstItemOfTheTasksOfTResultGetErrorAsync()
        {
            var tcs = new TaskCompletionSource<int>();
            Task.Delay(10).ContinueWith(_ => tcs.SetException(new Exception("planned exception")));
            var t2 = Task.Delay(500).ContinueWith(_ => 2);

            var r = await Task.WhenAny<int>(tcs.Task, t2);

            Assert.AreSame(r, tcs.Task);
            Assert.AreEqual(r.Result, default(int));
        }

        private async Task WhenAnyShouldHaveExceptionIfFirstItemOfTheTasksGetErrorAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            Task.Delay(10).ContinueWith(_ => tcs.SetException(new Exception("planned exception")));
            var t2 = Task.Delay(500);

            var r = await Task.WhenAny(tcs.Task, t2);

            Assert.AreSame(r, tcs.Task);
        }

        private async Task WhenAnyDelaysShouldAwaitForMinDelayWithTResult()
        {
            var t1 = Task.Delay(10).ContinueWith(_ => 1);
            var t2 = Task.Delay(20).ContinueWith(_ => 2);
            var t3 = Task.Delay(50).ContinueWith(_ => 3);
            var t4 = Task.Delay(100).ContinueWith(_ => 4);

            var r = await Task.WhenAny<int>(
                t2,
                t1,
                t3,
                t4).ConfigureAwait(false);

            Assert.AreSame(r, t1);
            Assert.AreEqual(r.Result, 1);
        }

        private async Task WhenAnyDelaysShouldAwaitForMinDelay()
        {
            var t1 = Task.Delay(10);
            var t2 = Task.Delay(20);
            var t3 = Task.Delay(50);
            var t4 = Task.Delay(100);

            var r = await Task.WhenAny(
                t2,
                t1,
                t3,
                t4).ConfigureAwait(false);

            Assert.AreSame(r, t1);
        }

        [TestMethod]
        public void TestTaskCompletionSource()
        {
            TrySetResultShouldWorkIifFirstTime().Wait();
        }

        private async Task TrySetResultShouldWorkIifFirstTime()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TrySetResult(1);
            tcs.TrySetResult(2);

            var res = await tcs.Task.ConfigureAwait(true);
            Assert.IsTrue(res == 1);
        }

        [TestMethod]
        public void TestCancellationTokenSource()
        {
            CancellationTokenSource_CancelShouldWorkIifFirstTime();
        }

        private void CancellationTokenSource_CancelShouldWorkIifFirstTime()
        {
            var cancelCount = 0;
            var cts = new CancellationTokenSource();
            cts.Token.Register(() => { cancelCount++; cts.Cancel(); });
            cts.Cancel();

            Assert.IsTrue(cancelCount == 1);

        }
    }
}
