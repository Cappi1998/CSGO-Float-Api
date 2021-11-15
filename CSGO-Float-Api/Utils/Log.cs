using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CSGO_Float_Api.Utils
{
    class Log
    {
        public static void error(string msg, Exception e = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            msg = $"{DateTime.Now} <{NameOfCallingClass()}> {msg}";
            if (e != null)
            {
                msg = $"\n{msg} - Exception_MSG:{e.Message}\n Exception_StackTrace:{e.StackTrace}";
                File.AppendAllText(Program.ErrorLogFile_Path, msg + "\n");
            }
            else
            {
                File.AppendAllText(Program.LogFile_Path, msg + "\n");
            }

            Console.WriteLine(msg);
            Console.ResetColor();
        }

        public static void info(string msg, ConsoleColor color = ConsoleColor.DarkCyan)
        {
            Console.ForegroundColor = color;

            msg = $"{DateTime.Now} <{NameOfCallingClass()}> {msg}";

            Console.WriteLine(msg);
            Console.ResetColor();

            File.AppendAllText(Program.LogFile_Path, msg + "\n");
        }
        public static string NameOfCallingClass()
        {
            string fullName;
            Type declaringType;
            int skipFrames = 2;
            do
            {
                MethodBase method = new StackFrame(skipFrames, false).GetMethod();
                declaringType = method.DeclaringType;
                if (declaringType == null)
                {
                    return method.Name;
                }
                skipFrames++;
                fullName = declaringType.Name;
            }
            while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

            return fullName;
        }
    }

}