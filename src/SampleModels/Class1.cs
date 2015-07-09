using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UniRx;
using I = IteratorTasks;

namespace SampleModels
{
    public class Class1
    {
        private static readonly TimeSpan delay = TimeSpan.FromSeconds(1);

        public static async Task MainAsync(IObserver<string> observer)
        {
            observer.OnNext("start");
            await I.Task.Delay(delay);
            observer.OnNext("1");
            await Observable.Timer(delay);
            observer.OnNext("2");
            await Task.Delay(delay);
            observer.OnNext("3");

            await I.Task.Delay(delay);

            var urls = new[]
            {
                "http://yahoo.co.jp",
                "http://google.co.jp",
                "http://bing.co.jp",
                "http://awsedrftgyhujikol.jp/",
            };

            foreach (var url in urls)
            {
                try
                {
                    var res = await GetAsStringAsync(url);
                    observer.OnNext(res);
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
            }

            observer.OnCompleted();
        }

        public static async Task<string> GetAsStringAsync(string url)
        {
            var req = WebRequest.Create(url);
            var res = await req.GetResponseAsObservable();

            using (var sr = new StreamReader(res.GetResponseStream()))
                return sr.ReadToEnd();
        }
    }
}
