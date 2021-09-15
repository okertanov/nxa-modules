using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Nxa.Plugins
{
    public static class ConsoleWriter
    {
        private static readonly BlockingCollection<string> blockingCollection = new();
        private static bool writeToScreen = false;

        static ConsoleWriter()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    string message = blockingCollection.Take();
                    if (writeToScreen)
                        System.Console.WriteLine(message);
                }

            });
        }

        public static void WriteLine(string value)
        {
            blockingCollection.Add(value);
        }

        public static void StartWriting()
        {
            writeToScreen = true;
        }
        public static void StopWriting()
        {
            writeToScreen = false;
        }

    }
}
