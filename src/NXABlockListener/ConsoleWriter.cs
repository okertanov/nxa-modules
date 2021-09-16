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
        private static uint rmqBlockIndex = 0;
        private static string rmqConnection = "Unconnected";
        private static string blockListenerState = "Not started";

        static ConsoleWriter()
        {
        }
        public static void WriteLine(string value)
        {
            if (writeToScreen)
                blockingCollection.Add(value);
        }
        public static void UpdateRmqBlock(uint index)
        {
            rmqBlockIndex = index;
        }
        public static void UpdateRmqConnection(string conn)
        {
            rmqConnection = conn;
        }
        public static void UpdateBlockListenerState(bool active)
        {
            if (active)
                blockListenerState = "Active";
            else
                blockListenerState = "Inactive";
        }
        public static void ShowState()
        {
            writeToScreen = true;
            var cancel = new CancellationTokenSource();

            Console.CursorVisible = false;
            Console.Clear();

            Task task = Task.Run(() =>
            {
                while (!cancel.Token.IsCancellationRequested)
                {
                    var (left, top) = Console.GetCursorPosition();
                    if (top <= 3) { top = 3; }
                    if (left != 0) { left = 0; }

                    Console.SetCursorPosition(0, 0);
                    WriteLineWithoutFlicker($"NXABlockListener state: {blockListenerState}", Console.WindowWidth - 1);
                    WriteLineWithoutFlicker($"RMQ block: {rmqBlockIndex}  RMQ connection: {rmqConnection}", Console.WindowWidth - 1);
                    WriteLineWithoutFlicker($"", Console.WindowWidth - 1);

                    Console.SetCursorPosition(left: left, top: top);
                    string message = blockingCollection.Take(cancel.Token);
                    WriteLineWithoutFlicker($"{message}", Console.WindowWidth - 1);
                }
            });

            ReadLine();
            cancel.Cancel();
            try { Task.WaitAll(task); } catch { }
            Console.WriteLine();
            Console.CursorVisible = true;

            writeToScreen = false;
        }

        private static void WriteLineWithoutFlicker(string message = "", int maxWidth = 80)
        {
            if (message.Length > 0) Console.Write(message);
            var spacesToErase = maxWidth - message.Length;
            if (spacesToErase < 0) spacesToErase = 0;
            Console.WriteLine(new string(' ', spacesToErase));
        }
        private static readonly CancellationTokenSource _shutdownToken = new();
        private static string ReadLine()
        {
            Task<string> readLineTask = Task.Run(() => Console.ReadLine());
            try
            {
                readLineTask.Wait(_shutdownToken.Token);
            }
            catch (OperationCanceledException)
            {
                return null;
            }

            return readLineTask.Result;
        }

        #region dispose
        public static void Dispose()
        {
            _shutdownToken.Cancel();
        }

        #endregion
    }
}
