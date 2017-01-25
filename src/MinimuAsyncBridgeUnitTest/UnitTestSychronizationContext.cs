using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace MinimuAsyncBridgeUnitTest
{
    [TestClass]
    public class UnitTestSychronizationContext
    {
        SingleThreadSynchronizationContext _context;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new SingleThreadSynchronizationContext();
        }

        [TestCleanup]
        public void TestCelean()
        {
            _context.Stop();
        }

        private void StartOnSingleThreadContext(Func<Task> test)
        {
            var tcs = new TaskCompletionSource<object>();
            _context.Post(t => ((Func<Task>)t)().ContinueWith(result =>
            {
                if (result.IsFaulted) tcs.SetException(result.Exception);
                else if (result.IsCanceled) tcs.SetCanceled();
                else  tcs.TrySetResult(null);
            }), test);

            tcs.Task.Wait();
        }

        private void ContextShouldBeSingleThread()
        {
            var sync = SynchronizationContext.Current as SingleThreadSynchronizationContext;
            Assert.AreEqual(_context, sync);

            var tid = Thread.CurrentThread.ManagedThreadId;
            Assert.AreEqual(sync.MainThreadId, tid);
        }

        private static void ContextShouldBeLost()
        {
            Assert.IsNull(SynchronizationContext.Current);
        }

        private static Task RandomDelay(Random r) => Task.Delay(r.Next(1, 5));

        private static readonly Random _defaultRandom = new Random();
        private static Task RandomDelay() => RandomDelay(_defaultRandom);

        [TestMethod]
        public void AwaitOperatorShouldPreserveSynchronizationContext()
        {
            StartOnSingleThreadContext(RunRandomTasksAsync);
        }

        private async Task RunRandomTasksAsync()
        {
            await Task.Delay(1);
            var r = new Random();
            await Task.WhenAll(Enumerable.Range(0, 50).Select(_ => RunRandomTasksAsync(r.Next())));
            ContextShouldBeSingleThread();
            await Task.WhenAny(Enumerable.Range(0, 50).Select(_ => RunRandomTasksAsync(r.Next())));
            ContextShouldBeSingleThread();
        }

        private async Task RunRandomTasksAsync(int seed)
        {
            var r = new Random(seed);
            for (int i = 0; i < 100; i++)
            {
                await RandomDelay(r);
                ContextShouldBeSingleThread();
            }
            await RandomDelay(r).ConfigureAwait(false);
            ContextShouldBeLost();
        }
        [TestMethod]
        public void ContextShouldBePreservedOverLostMethod()
        {
            StartOnSingleThreadContext(ContextShouldBePreservedOverLostMethodAsync);
        }

        private async Task ContextShouldBePreservedOverLostMethodAsync()
        {
            ContextShouldBeSingleThread();

            await ContextShouldNeverReturnEverAfterLostAsync();

            ContextShouldBeSingleThread();
        }

        private async Task ContextShouldNeverReturnEverAfterLostAsync()
        {
            ContextShouldBeSingleThread();

            await RandomDelay();
            ContextShouldBeSingleThread();

            await RandomDelay().ConfigureAwait(true);
            ContextShouldBeSingleThread();

            await RandomDelay().ConfigureAwait(false);
            ContextShouldBeLost();

            await RandomDelay();
            ContextShouldBeLost();

            await RandomDelay().ConfigureAwait(true);
            ContextShouldBeLost();
        }

        [TestMethod]
        public void ContextShouldNotBeChangedAfterCompletedTask()
        {
            StartOnSingleThreadContext(ContextShouldNotBeChangedAfterCompletedTaskAsync);
        }

        private async Task ContextShouldNotBeChangedAfterCompletedTaskAsync()
        {
            ContextShouldBeSingleThread();
            await Task.CompletedTask;
            ContextShouldBeSingleThread();
            await Task.CompletedTask.ConfigureAwait(false);
            ContextShouldBeSingleThread();
            await Task.CompletedTask.ConfigureAwait(true);
            ContextShouldBeSingleThread();
        }
    }
}
