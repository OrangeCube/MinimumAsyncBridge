using Sample35;
using System;
using System.Threading.Tasks;

namespace Sample45
{
    public class Class1
    {
        public static async Task MainAsync()
        {
            var sec = TimeSpan.FromSeconds(1);

            for (int i = 0; i < 10; i++)
            {
                await TaskEx.Delay(sec);
                Console.WriteLine("Sample 4.5: " + i);
            }
        }
    }
}
