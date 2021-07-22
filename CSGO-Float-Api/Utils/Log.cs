using System;
using System.IO;

namespace CSGO_Float_Api.Utils
{
    public class Log
    {
        private static readonly object locker = new object();
        public static string Database_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "Database\\");
        public static void info(string msg, ConsoleColor color = ConsoleColor.Blue)
        {
            lock (locker)
            {
                Console.ForegroundColor = color;
                msg = DateTime.Now + " - " + msg;
                Console.WriteLine(msg);
                Console.ResetColor();

                StreamWriter sw;
                sw = File.AppendText(Database_Path + "log.txt");
                sw.WriteLine(msg);
                sw.Close();
                sw.Dispose();
            }
        }

        public static void error(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            error(msg);
        }

        public static void error(string msg, Exception e)
        {
            lock (locker)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                msg = DateTime.Now + " - " + msg + ". " + e.Message;
                Console.WriteLine(msg);
                Console.ResetColor();

                StreamWriter sw;
                sw = File.AppendText(Database_Path + "error.txt");
                sw.WriteLine(msg);
                sw.Close();
                sw.Dispose();

                StreamWriter sw1;
                sw1 = File.AppendText(Database_Path + "error.txt");
                sw1.WriteLine(msg + "\n" + e.StackTrace);
                sw1.Close();
                sw1.Dispose();
            }
        }

        public static void error(string msg)
        {
            lock (locker)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                msg = DateTime.Now + " - " + msg;
                Console.WriteLine(msg);
                Console.ResetColor();

                StreamWriter sw;
                sw = File.AppendText(Database_Path + "error.txt");
                sw.WriteLine(msg);
                sw.Close();
                sw.Dispose();
            }
        }

    }


}