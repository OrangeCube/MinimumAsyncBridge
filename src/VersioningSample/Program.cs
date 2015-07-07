using System.Threading.Tasks;

namespace VersioningSample
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            await Sample35.Class1.MainAsync();
            await Sample45.Class1.MainAsync();
        }
    }
}
