using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GenericRepository.Helpers
{
    public static class GRStringHelpers
    {
        public static string GetExceptionString(Exception e)
        {
            StringBuilder sb = new StringBuilder();

            Exception currentException = e;
            int counter = 0;
            string caller = GetCaller();

            do
            {
                string line = string.Empty;
                for (int i = 0; i < counter; i++) line += "   ";
                line += "-> ";

                counter++;
                line += currentException.Message;

                if (counter == 1)
                {
                    try
                    {
                        // Get stack trace for the exception with source file information
                        var st = new StackTrace(e, true);
                        // Get the top stack frame
                        var frame = st.GetFrame(0);
                        // Get the line number from the stack frame
                        int lineNumber = frame.GetFileLineNumber();
                        string filename = frame.GetFileName();
                        string[] split = filename.Split('\\');
                        if (split.Length > 1)
                        {
                            filename = split.Last();
                        }

                        line += string.Format(", file: {0}, line: {1}, caller: {2}{3}.", filename, lineNumber, caller, Environment.NewLine);
                    }
                    catch { }
                }

                sb.AppendLine(line);
                sb.AppendFormat("Stack trace: " + e.StackTrace);
                sb.AppendLine();
                currentException = currentException.InnerException;
            } while (currentException != null);
            return sb.ToString();
        }

        static bool logStack = false;

        private static string GetCaller()
        {
            if (!logStack) return string.Empty;

            StackTrace stackTrace = new StackTrace();
            MethodBase methodBase = stackTrace.GetFrame(2).GetMethod();
            return string.Format("{0}>{1}: ", methodBase.ReflectedType.Name, methodBase.Name);
        }
    }
}
