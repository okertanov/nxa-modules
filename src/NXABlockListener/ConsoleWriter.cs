using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace NXABlockListener
{
    //Non thread blocking write to console 
    public static class ConsoleWriter
    {
        private static BlockingCollection<string> blockingCollection = new BlockingCollection<string>();

        static ConsoleWriter()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    System.Console.WriteLine(blockingCollection.Take());
                }

            });
        }

        public static void WriteLine(string value)
        {
            blockingCollection.Add(value);
        }

    }
}
