using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static System.Reflection.BindingFlags;

namespace MinimuAsyncBridgeUnitTest
{
    [TestClass]
    public class UnitTestMemoryLeak
    {
        List<WeakReference> _refs = new List<WeakReference>();

        void Add<T>(T item)
        {
            lock (_refs)
            {
                _refs.Add(new WeakReference(item));
            }
        }

        void AllReferencesShouldBeGarbageCollected()
        {
            GC.Collect(2, GCCollectionMode.Forced);

            lock(_refs)
            {
                foreach (var r in _refs)
                    Assert.IsNull(r.Target);
                _refs.Clear();
            }
        }

        [System.Diagnostics.Conditional("V35")]
        private static void CancellationTokenSourceShouldHaveNoEventListener(CancellationTokenSource cts)
        {
            var f = cts.GetType().GetField("_canceled", NonPublic | Instance);
            var v = f.GetValue(cts);
            Assert.IsNull(v);
        }

        [TestMethod]
        public void TestTaskRun()
        {
            TaskRun().Wait();
            AllReferencesShouldBeGarbageCollected();
        }

        async Task TaskRun()
        {
            for (int i = 0; i < 1000; i++)
            {
                var t = Task.Run(() => { });
                Add(t);
                await t;
            }
        }

        [TestMethod]
        public void TestTaskDelay()
        {
            TaskDelay().Wait();
            AllReferencesShouldBeGarbageCollected();
        }

        [TestMethod]
        public void TestTaskDelayWithoutCancel()
        {
            var cts = new CancellationTokenSource();
            TaskDelay(cts.Token).Wait();
            CancellationTokenSourceShouldHaveNoEventListener(cts);
            AllReferencesShouldBeGarbageCollected();
        }

        [TestMethod]
        public void TestTaskDelayWithCancel()
        {
            var cts = new CancellationTokenSource();
            var t = TaskDelay(cts.Token);
            cts.Cancel();
            try
            {
                t.Wait();
            }
            catch (OperationCanceledException) { }
            CancellationTokenSourceShouldHaveNoEventListener(cts);
            AllReferencesShouldBeGarbageCollected();
        }

        async Task TaskDelay()
        {
            for (int i = 0; i < 100; i++)
            {
                var t = Task.Delay(1);
                Add(t);
                await t;
            }
        }

        async Task TaskDelay(CancellationToken ct)
        {
            for (int i = 0; i < 50; i++)
            {
                var t = Task.Delay(1, ct);
                Add(t);
                await t;
            }
        }
    }
}
