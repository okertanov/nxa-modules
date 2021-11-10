using Nxa.Plugins.Tasks;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Nxa.Plugins
{
    public static class ConsoleWriter
    {
        private static bool WriteToScreen = false;

        private static readonly BlockingCollection<string> BblockingCollection = new();
        private static TaskManager TaskManager;
        private static string BlockListenerState { get { if (TaskManager.Active) { return "Active"; } else { return "Inactive"; } } }

        static ConsoleWriter()
        {
        }
        public static void WriteLine(string value)
        {
            if (WriteToScreen)
                BblockingCollection.Add(value);
        }

        public static void SetUpState(ref TaskManager taskManager)
        {
            TaskManager = taskManager;
        }
        public static void ShowState()
        {
            WriteToScreen = true;
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
                    WriteLineWithoutFlicker($"NXABlockListener console (Active: {BlockListenerState})", Console.WindowWidth - 1);
                    WriteLineWithoutFlicker($"", Console.WindowWidth - 1);

                    Console.SetCursorPosition(left: left, top: top);
                    string message = BblockingCollection.Take(cancel.Token);
                    WriteLineWithoutFlicker($"{message}", Console.WindowWidth - 1);
                }
            });

            ReadLine();
            cancel.Cancel();
            try { Task.WaitAll(task); } catch { }
            Console.WriteLine();
            Console.CursorVisible = true;

            WriteToScreen = false;
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

        public static void Release()
        {
            _shutdownToken.Cancel();
        }

    }
}
