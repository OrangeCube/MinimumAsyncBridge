using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

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

        [TestMethod]
        public void ResultsOfWhenAllShouldHaveDefinitOrder()
        {
            WhenAllShoudBeResultsOrderIsGuaranteed().Wait();
        }

        private async Task WhenAllShoudBeResultsOrderIsGuaranteed()
        {
            var planned = new[] { 1, 2, 3, 4 };
            var t1 = Task.Delay(10).ContinueWith(_ => planned[0]);
            var t2 = Task.Delay(300).ContinueWith(_ => planned[1]);
            var t3 = Task.Delay(50).ContinueWith(_ => planned[2]);
            var t4 = Task.Delay(200).ContinueWith(_ => planned[3]);

            var results = await Task.WhenAll(
                t1,
                t2,
                t3,
                t4);

            for(var i = 0; i < 4; ++i)
            {
                Assert.AreEqual(results[i], planned[i]);
            }
        }

        #region cancel
        [TestMethod]
        public void TestWhenAllShouldBeCanceledIfAnyOfTheTaskIsCanceled()
        {
            WhenAllShouldBeCanceledIfAnyOfTheTaskIsCanceled().Wait();
            WhenAllShouldBeCanceledIfAnyOfTheTaskIsCanceledWithTResult().Wait();
            WhenAllShouldBeCanceledIfAnyOfTheTaskIsCanceledAsync().Wait();
            WhenAllShouldBeCanceledIfAnyOfTheTaskIsCanceledWithTResultAsync().Wait();
        }

        private async Task WhenAllShouldBeCanceledIfAnyOfTheTaskIsCanceled()
        {
            var tcs1 = new TaskCompletionSource<object>();
            tcs1.TrySetCanceled();
            var tcs2 = new TaskCompletionSource<int>();
            tcs2.TrySetResult(2);
            var t1 = tcs1.Task;
            var t2 = tcs2.Task;

            var exceptionCount = 0;
            try
            {
                await Task.WhenAll(t1, t2);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(TaskCanceledException));
                exceptionCount++;
            }

            Assert.AreEqual(exceptionCount, 1);
        }

        private async Task WhenAllShouldBeCanceledIfAnyOfTheTaskIsCanceledWithTResult()
        {
            var tcs1 = new TaskCompletionSource<object>();
            tcs1.TrySetCanceled();
            var tcs2 = new TaskCompletionSource<object>();
            tcs2.TrySetResult(null);
            var t1 = tcs1.Task;
            var t2 = tcs2.Task;

            var exceptionCount = 0;
            Task<object[]> t = null;
            try
            {
                t = Task.WhenAll(t1, t2);
                var res = await t;
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(TaskCanceledException));
                exceptionCount++;
            }

            try
            {
                var res = t.Result;
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(1, e.InnerExceptions.Count);
                Assert.AreEqual(typeof(TaskCanceledException), e.InnerException.GetType());
                exceptionCount++;
            }

            Assert.AreEqual(exceptionCount, 2);
        }

        private async Task WhenAllShouldBeCanceledIfAnyOfTheTaskIsCanceledAsync()
        {
            var tcs1 = new TaskCompletionSource<object>();
            Task.Delay(10).ContinueWith(_ => tcs1.TrySetCanceled());
            var tcs2 = new TaskCompletionSource<int>();
            Task.Delay(200).ContinueWith(_ => tcs2.TrySetResult(2));
            var t1 = tcs1.Task;
            var t2 = tcs2.Task;

            var exceptionCount = 0;
            try
            {
                await Task.WhenAll(t1, t2);
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(TaskCanceledException));
                exceptionCount++;
            }

            Assert.AreEqual(exceptionCount, 1);
        }

        private async Task WhenAllShouldBeCanceledIfAnyOfTheTaskIsCanceledWithTResultAsync()
        {
            var tcs1 = new TaskCompletionSource<object>();
            Task.Delay(10).ContinueWith(_ => tcs1.TrySetCanceled());
            var tcs2 = new TaskCompletionSource<object>();
            tcs2.TrySetResult(null);
            var t1 = tcs1.Task;
            var t2 = tcs2.Task;

            var exceptionCount = 0;
            Task<object[]> t = null;
            try
            {
                t = Task.WhenAll(t1, t2);
                var res = await t;
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.GetType(), typeof(TaskCanceledException));
                exceptionCount++;
            }

            try
            {
                var res = t.Result;
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(1, e.InnerExceptions.Count);
                Assert.AreEqual(typeof(TaskCanceledException), e.InnerException.GetType());
                exceptionCount++;
            }

            Assert.AreEqual(exceptionCount, 2);
        }
        #endregion

        #region exception
        [TestMethod]
        public void TestWhenAllShouldHasExceptionsIfAnyOfTheTaskGetException()
        {
            WhenAllShouldHasExceptionsIfAnyOfTheTaskGetException().Wait();
            WhenAllShouldHasExceptionsIfAnyOfTheTaskGetExceptionWithTResult().Wait();
            WhenAllShouldHasExceptionsIfAnyOfTheTaskGetExceptionAsync().Wait();
            WhenAllShouldHasExceptionsIfAnyOfTheTaskGetExceptionWithTResultAsync().Wait();
        }

        private async Task WhenAllShouldHasExceptionsIfAnyOfTheTaskGetException()
        {
            var tcs1 = new TaskCompletionSource<object>();
            tcs1.TrySetCanceled();
            var tcs2 = new TaskCompletionSource<int>();
            tcs2.TrySetResult(2);
            var tcs3 = new TaskCompletionSource<int>();
            var ex1 = new Exception("ex1");
            tcs3.TrySetException(ex1);
            var tcs4 = new TaskCompletionSource<int>();
            var ex2 = new Exception("ex2");
            tcs4.TrySetException(ex2);
            var t1 = tcs1.Task;
            var t2 = tcs2.Task;
            var t3 = tcs3.Task;
            var t4 = tcs4.Task;

            var exceptionCount = 0;
            Task t = Task.WhenAll(t1, t2, t3, t4);
            try
            {
                await t;
            }
            catch (Exception e)
            {
                Assert.AreEqual(ex1, e);
                exceptionCount++;
            }

            Assert.AreEqual(exceptionCount, 1);
            Assert.AreEqual(t.Status, TaskStatus.Faulted);
            Assert.AreEqual(2, t.Exception.InnerExceptions.Count);
            Assert.AreEqual(t.Exception.InnerExceptions[0], ex1);
            Assert.AreEqual(t.Exception.InnerExceptions[1], ex2);
        }

        private async Task WhenAllShouldHasExceptionsIfAnyOfTheTaskGetExceptionWithTResult()
        {
            var tcs1 = new TaskCompletionSource<object>();
            tcs1.TrySetCanceled();
            var tcs2 = new TaskCompletionSource<object>();
            tcs2.TrySetResult(null);
            var tcs3 = new TaskCompletionSource<object>();
            var ex1 = new Exception("ex1");
            tcs3.TrySetException(ex1);
            var tcs4 = new TaskCompletionSource<object>();
            var ex2 = new Exception("ex2");
            tcs4.TrySetException(ex2);
            var t1 = tcs1.Task;
            var t2 = tcs2.Task;
            var t3 = tcs3.Task;
            var t4 = tcs4.Task;

            var exceptionCount = 0;
            Task<object[]> t = null;
            try
            {
                t = Task.WhenAll(t1, t2, t3, t4);
                var res = await t;
            }
            catch (Exception e)
            {
                Assert.AreEqual(ex1, e);
                exceptionCount++;
            }

            try
            {
                var res = t.Result;
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(e.InnerExceptions.Count, 2);
                Assert.AreEqual(e.InnerExceptions[0], ex1);
                Assert.AreEqual(e.InnerExceptions[1], ex2);
                exceptionCount++;
            }

            Assert.AreEqual(exceptionCount, 2);
            Assert.AreEqual(t.Status, TaskStatus.Faulted);
            Assert.AreEqual(t.Exception.InnerExceptions[0], ex1);
            Assert.AreEqual(t.Exception.InnerExceptions[1], ex2);
        }

        private async Task WhenAllShouldHasExceptionsIfAnyOfTheTaskGetExceptionAsync()
        {
            var tcs1 = new TaskCompletionSource<object>();
            Task.Delay(10).ContinueWith(_ => tcs1.TrySetCanceled());
            var tcs2 = new TaskCompletionSource<int>();
            Task.Delay(200).ContinueWith(_ => tcs2.TrySetResult(2));
            var tcs3 = new TaskCompletionSource<int>();
            var ex1 = new Exception("ex1");
            Task.Delay(400).ContinueWith(_ => tcs3.TrySetException(ex1));
            var tcs4 = new TaskCompletionSource<int>();
            var ex2 = new Exception("ex2");
            Task.Delay(300).ContinueWith(_ => tcs4.TrySetException(ex2));
            var t1 = tcs1.Task;
            var t2 = tcs2.Task;
            var t3 = tcs3.Task;
            var t4 = tcs4.Task;

            var exceptionCount = 0;
            Task t = Task.WhenAll(t1, t2, t3, t4);
            try
            {
                await t;
            }
            catch (Exception e)
            {
                Assert.AreEqual(ex1, e);
                exceptionCount++;
            }

            Assert.AreEqual(exceptionCount, 1);
            Assert.AreEqual(t.Status, TaskStatus.Faulted);
            Assert.AreEqual(2, t.Exception.InnerExceptions.Count);
            Assert.AreEqual(t.Exception.InnerExceptions[0], ex1);
            Assert.AreEqual(t.Exception.InnerExceptions[1], ex2);
        }

        private async Task WhenAllShouldHasExceptionsIfAnyOfTheTaskGetExceptionWithTResultAsync()
        {
            var tcs1 = new TaskCompletionSource<object>();
            Task.Delay(10).ContinueWith(_ => tcs1.TrySetCanceled());
            var tcs2 = new TaskCompletionSource<object>();
            tcs2.TrySetResult(null);
            var tcs3 = new TaskCompletionSource<object>();
            var ex1 = new Exception("ex1");
            Task.Delay(300).ContinueWith(_ => tcs3.TrySetException(ex1));
            var tcs4 = new TaskCompletionSource<object>();
            var ex2 = new Exception("ex2");
            Task.Delay(400).ContinueWith(_ => tcs4.TrySetException(ex2));
            var t1 = tcs1.Task;
            var t2 = tcs2.Task;
            var t3 = tcs3.Task;
            var t4 = tcs4.Task;

            var exceptionCount = 0;
            Task<object[]> t = null;
            try
            {
                t = Task.WhenAll<object>(t1, t2, t3, t4);
                var res = await t;
            }
            catch (Exception e)
            {
                Assert.AreEqual(ex1, e);
                exceptionCount++;
            }

            try
            {
                var res = t.Result;
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(e.InnerExceptions.Count, 2);
                Assert.AreEqual(e.InnerExceptions[0], ex1);
                Assert.AreEqual(e.InnerExceptions[1], ex2);
                exceptionCount++;
            }

            Assert.AreEqual(exceptionCount, 2);
            Assert.AreEqual(t.Status, TaskStatus.Faulted);
            Assert.AreEqual(t.Exception.InnerExceptions[0], ex1);
            Assert.AreEqual(t.Exception.InnerExceptions[1], ex2);
        }
        #endregion

        #region nest
        [TestMethod]
        public void TestNestedWhenAll()
        {
            TestNestedWhenAllAsync().Wait();
        }

        private async Task TestNestedWhenAllAsync()
        {
            var tcs1 = new TaskCompletionSource<object>();
            tcs1.TrySetCanceled();
            var tcs2 = new TaskCompletionSource<int>();
            tcs2.TrySetResult(2);
            var tcs3 = new TaskCompletionSource<int>();
            var ex1 = new Exception("ex1");
            tcs3.TrySetException(ex1);
            var tcs4 = new TaskCompletionSource<int>();
            var ex2 = new Exception("ex2");
            tcs4.TrySetException(ex2);
            var t1 = tcs1.Task;
            var t2 = tcs2.Task;
            var t3 = tcs3.Task;
            var t4 = tcs4.Task;

            var tcs5 = new TaskCompletionSource<object>();
            tcs5.TrySetCanceled();
            var tcs6 = new TaskCompletionSource<int>();
            tcs6.TrySetResult(2);
            var tcs7 = new TaskCompletionSource<int>();
            var ex3 = new Exception("ex3");
            tcs7.TrySetException(ex3);
            var tcs8 = new TaskCompletionSource<int>();
            var ex4 = new Exception("ex4");
            tcs8.TrySetException(ex4);
            var t5 = tcs5.Task;
            var t6 = tcs6.Task;
            var t7 = tcs7.Task;
            var t8 = tcs8.Task;

            var exceptionCount = 0;
            Task all1 = Task.WhenAll(t1, t2, t3, t4);
            Task all2 = Task.WhenAll(t5, t6, t7, t8, all1);
            try
            {
                await all2;
            }
            catch (Exception e)
            {
                Assert.AreEqual(ex3, e);
                exceptionCount++;
            }

            Assert.AreEqual(exceptionCount, 1);

            Assert.AreEqual(all1.Status, TaskStatus.Faulted);
            Assert.AreEqual(2, all1.Exception.InnerExceptions.Count);
            Assert.AreEqual(all1.Exception.InnerExceptions[0], ex1);
            Assert.AreEqual(all1.Exception.InnerExceptions[1], ex2);

            Assert.AreEqual(all2.Status, TaskStatus.Faulted);
            Assert.AreEqual(4, all2.Exception.InnerExceptions.Count);
            Assert.AreEqual(all2.Exception.InnerExceptions[0], ex3);
            Assert.AreEqual(all2.Exception.InnerExceptions[1], ex4);
            Assert.AreEqual(all2.Exception.InnerExceptions[2], ex1);
            Assert.AreEqual(all2.Exception.InnerExceptions[3], ex2);
        }
        #endregion
    }
}
