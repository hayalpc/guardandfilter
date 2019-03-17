using System;

namespace Filter
{
    public static class Logger
    {
        public enum LogLevel
        {
            Notify,
            Warning,
            Error,
            Debug,
            EventBot,
            MikeMode
        }

        public static object locker = new object();

        public static void WriteLine(LogLevel level, string msg, params object[] args)
        {
            lock (locker)
            {
                try
                {
                    //if (!FilterMain.debug_mike && level == LogLevel.MikeMode)
                    //{
                    //    return;
                    //}
                    DateTime date = DateTime.Now;
                    string consoleStr =
                        string.Format("[{0}] -> {1}", level, msg, args);

                    switch (level)
                    {
                        case LogLevel.Notify:
                            {
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
                            }
                            break;
                        case LogLevel.Warning:
                            {
                                Console.BackgroundColor = ConsoleColor.DarkYellow;
                            }
                            break;
                        case LogLevel.Error:
                            {
                                Console.BackgroundColor = ConsoleColor.DarkRed;
                            }
                            break;
                        case LogLevel.Debug:
                            {
                                Console.BackgroundColor = ConsoleColor.DarkMagenta;
                            }
                            break;
                        case LogLevel.EventBot:
                            {
                                Console.BackgroundColor = ConsoleColor.DarkCyan;
                            }
                            break;
                        case LogLevel.MikeMode:
                            {
                                Console.BackgroundColor = ConsoleColor.DarkCyan;
                            }
                            break;
                        default:
                            {
                                Console.BackgroundColor = ConsoleColor.White;
                            }
                            break;
                    }
                    Console.WriteLine($"[{date}]" + consoleStr);
                    Console.ResetColor();

                }
                catch (Exception ex)
                {
                    DateTime date = DateTime.Now;
                    Console.WriteLine($"[{date}]" + ex.ToString());
                    Console.ResetColor();
                }
            }
        }

        public static void WriteLine(string msg, params object[] args)
        {
            WriteLine(LogLevel.Notify, msg, args);
        }
    }
}
