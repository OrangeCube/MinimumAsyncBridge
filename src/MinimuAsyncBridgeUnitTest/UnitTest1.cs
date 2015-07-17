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
        public void WhenAnyShouldHaveExceptionIfFirstItemOfTheTasksGetError()
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

            var exceptionCount = 0;
            try
            {
                var result = r.Result;
            }
            catch(Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(TaskCanceledException));
                exceptionCount++;
            }
            Assert.AreEqual(exceptionCount, 1);
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
            var ex = new Exception("planned exception");
            Task.Delay(10).ContinueWith(_ => tcs.SetException(ex));
            var t2 = Task.Delay(500).ContinueWith(_ => 2);

            var r = await Task.WhenAny<int>(tcs.Task, t2);

            Assert.AreSame(r, tcs.Task);

            var exceptionCount = 0;
            try
            {
                var result = r.Result;
            }
            catch (Exception e)
            {
                Assert.AreEqual(e, ex);
                exceptionCount++;
            }
            Assert.AreEqual(exceptionCount, 1);
        }

        private async Task WhenAnyShouldHaveExceptionIfFirstItemOfTheTasksGetErrorAsync()
        {
            var tcs = new TaskCompletionSource<object>();
            var plannedException = new Exception("planned exception");
            Task.Delay(10).ContinueWith(_ => tcs.SetException(plannedException));
            var t2 = Task.Delay(500);

            var r = await Task.WhenAny(tcs.Task, t2);

            Assert.AreSame(r, tcs.Task);
            Assert.AreSame(r.Exception, plannedException);
        }

        private async Task WhenAnyDelaysShouldAwaitForMinDelayWithTResult()
        {
            var t1 = Task.Delay(10).ContinueWith(_ => 1);
            var t2 = Task.Delay(200).ContinueWith(_ => 2);
            var t3 = Task.Delay(500).ContinueWith(_ => 3);
            var t4 = Task.Delay(1000).ContinueWith(_ => 4);

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
            var t2 = Task.Delay(200);
            var t3 = Task.Delay(500);
            var t4 = Task.Delay(1000);

            var r = await Task.WhenAny(
                t2,
                t1,
                t3,
                t4).ConfigureAwait(false);

            Assert.AreSame(r, t1);
        }

        [TestMethod]
        public void TestTaskCompletionSource_Result()
        {
            GetResultShouldHaveExceptionIfTheTasksGetError();
            GetResultShouldHaveTaskCanceledExceptionIfTheTasksIsCanceled();
            GetResultShouldWaitIfTheTasksIsRunning();
        }

        private void GetResultShouldHaveExceptionIfTheTasksGetError()
        {
            var tcs = new TaskCompletionSource<int>();
            var ex = new Exception();
            tcs.SetException(ex);
            var exceptionCount = 0;

            try
            {
                var res = tcs.Task.Result;
            }
            catch (Exception e)
            {
                // todo AggregateException
                Assert.AreEqual(e, ex);
                exceptionCount++;
            }
            Assert.AreEqual(exceptionCount, 1);
        }

        private void GetResultShouldHaveTaskCanceledExceptionIfTheTasksIsCanceled()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetCanceled();
            var exceptionCount = 0;

            try
            {
                var res = tcs.Task.Result;
            }
            catch (Exception e)
            {
                // todo AggregateException
                Assert.AreEqual(e.GetType(), typeof(TaskCanceledException));
                exceptionCount++;
            }
            Assert.AreEqual(exceptionCount, 1);
        }

        private void GetResultShouldWaitIfTheTasksIsRunning()
        {
            var tcs = new TaskCompletionSource<int>();
            Task.Delay(200).ContinueWith(_ => tcs.SetResult(321));
            var res = tcs.Task.Result;
            Assert.AreEqual(res, 321);
        }

        [TestMethod]
        public void TestTaskCompletionSource_SetXxx()
        {
            SetResultShouldHaveExceptionIfSecondTimeCalls().Wait();
            SetCanceledShouldHaveExceptionIfSecondTimeCalls().Wait();
            SetExceptionShouldHaveExceptionIfSecondTimeCalls().Wait();
        }

        private async Task SetResultShouldHaveExceptionIfSecondTimeCalls()
        {
            var tcs = new TaskCompletionSource<int>();
            var exceptionCount = 0;
            tcs.SetResult(1);
            try
            {
                tcs.SetResult(2);
            }
            catch(Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
                exceptionCount++;
            }

            try
            {
                tcs.SetCanceled();
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
                exceptionCount++;
            }

            try
            {
                tcs.SetException(new Exception());
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
                exceptionCount++;
            }
            Assert.AreEqual(exceptionCount, 3);

            var r = await tcs.Task;

            Assert.AreEqual(r, 1);
        }

        private async Task SetCanceledShouldHaveExceptionIfSecondTimeCalls()
        {
            var tcs = new TaskCompletionSource<int>();
            var exceptionCount = 0;
            tcs.SetCanceled();
            try
            {
                tcs.SetResult(2);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
                exceptionCount++;
            }

            try
            {
                tcs.SetCanceled();
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
                exceptionCount++;
            }

            try
            {
                tcs.SetException(new Exception());
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
                exceptionCount++;
            }

            try
            {
                var r = await tcs.Task;
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(TaskCanceledException));
                exceptionCount++;
            }

            Assert.IsNull(tcs.Task.Exception);
            Assert.AreEqual(exceptionCount, 4);
        }

        private async Task SetExceptionShouldHaveExceptionIfSecondTimeCalls()
        {
            var tcs = new TaskCompletionSource<int>();
            var exceptionCount = 0;
            tcs.SetException(new Exception("first"));
            try
            {
                tcs.SetResult(2);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
                exceptionCount++;
            }

            try
            {
                tcs.SetCanceled();
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
                exceptionCount++;
            }

            try
            {
                tcs.SetException(new Exception("second"));
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(InvalidOperationException));
                exceptionCount++;
            }

            try
            {
                var r = await tcs.Task;
            }
            catch(Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(Exception));
                Assert.AreEqual(e.Message, "first");
                exceptionCount++;
            }

            Assert.IsNotNull(tcs.Task.Exception);
            Assert.AreEqual(exceptionCount, 4);
        }

        [TestMethod]
        public void TestTaskCompletionSource_TrySetXxx()
        {
            TrySetResultShouldWorkIfFirstTime().Wait();
            TrySetCanceledShouldWorkIfFirstTime().Wait();
            TrySetExceptionShouldWorkIfFirstTime().Wait();
        }

        private async Task TrySetResultShouldWorkIfFirstTime()
        {
            var tcs = new TaskCompletionSource<int>();
            var completedCount = 0;
            tcs.Task.ContinueWith(t => completedCount++);
            tcs.TrySetResult(1);
            tcs.TrySetResult(2);
            tcs.TrySetCanceled();
            tcs.TrySetException(new Exception());

            var res = await tcs.Task.ConfigureAwait(true);
            Assert.AreEqual(completedCount, 1);
            Assert.AreEqual(res, 1);
        }

        private async Task TrySetCanceledShouldWorkIfFirstTime()
        {
            var tcs = new TaskCompletionSource<int>();
            var completedCount = 0;
            tcs.Task.ContinueWith(t => completedCount++);
            tcs.TrySetCanceled();
            tcs.TrySetCanceled();
            tcs.TrySetResult(1);
            tcs.TrySetException(new Exception());

            var exceptionCount = 0;
            try
            {
                var res = await tcs.Task.ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(TaskCanceledException));
                exceptionCount++;
            }

            Assert.AreEqual(completedCount, 1);
            Assert.AreEqual(exceptionCount, 1);
            Assert.AreEqual(tcs.Task.Status, TaskStatus.Canceled);
        }

        private async Task TrySetExceptionShouldWorkIfFirstTime()
        {
            var tcs = new TaskCompletionSource<int>();
            var completedCount = 0;
            tcs.Task.ContinueWith(t => completedCount++);
            tcs.TrySetException(new Exception("first"));
            tcs.TrySetException(new Exception("second"));
            tcs.TrySetResult(1);
            tcs.TrySetCanceled();

            var exceptionCount = 0;
            try
            {
                var res = await tcs.Task.ConfigureAwait(true);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "first");
                exceptionCount++;
            }

            Assert.AreEqual(completedCount, 1);
            Assert.AreEqual(exceptionCount, 1);
            Assert.AreEqual(tcs.Task.Status, TaskStatus.Faulted);
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
    }
}
