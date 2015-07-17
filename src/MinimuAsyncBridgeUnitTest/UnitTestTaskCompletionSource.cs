using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;

namespace MinimuAsyncBridgeUnitTest
{
    [TestClass]
    public class UnitTestTaskCompletionSource
    {
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
    }
}
