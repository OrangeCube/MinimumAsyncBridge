using System;
using System.Threading.Tasks;

namespace Sample35
{
    public class Class1
    {
        public static async Task MainAsync()
        {
            var sec = TimeSpan.FromSeconds(1);

            for (int i = 0; i < 10; i++)
            {
                await TaskEx.Delay(sec);
                Console.WriteLine("Sample 3.5: " + i);


            }
        }
    }
}
