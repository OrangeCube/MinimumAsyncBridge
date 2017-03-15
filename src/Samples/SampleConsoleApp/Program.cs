using SampleModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using I = IteratorTasks;

namespace SampleConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = new Thread(TaskLoop);
            var cts = new I.CancellationTokenSource();
            t.Start(cts.Token);

            MainAsync().Wait();

            cts.Cancel();
            t.Join();
        }

        static async Task MainAsync()
        {
            var s = new Subject<string>();

            s.Subscribe(
                result => Console.WriteLine(result),
                ex => Console.WriteLine(ex.Message)
                );

            await Class1.MainAsync(s);
        }

        static void TaskLoop(object state)
        {
            var s = I.Task.DefaultScheduler;
            var ct = (I.CancellationToken)state;

            while (!ct.IsCancellationRequested)
            {
                s.Update();
                Thread.Sleep(10);
            }
        }
    }
}
