using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.ContextLoggers
{
    public class GRConsoleLogger : IGRContextLogger
    {
        string LineStart
        {
            get
            {
                return DateTime.Now.ToString(CultureInfo.InvariantCulture) + ": ";
            }
        }

        public void LogDebug(string message, params object[] args)
        {
            var prevColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Cyan;
            try
            {
                Console.WriteLine(LineStart + message, args);
            }
            catch
            {
                Console.WriteLine(LineStart + message);
            }
            Console.ForegroundColor = prevColor;
        }

        public void LogWarning(string message, params object[] args)
        {
            var prevColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Yellow;
            try
            {
                Console.WriteLine(LineStart + message, args);
            }
            catch
            {
                Console.WriteLine(LineStart + message);
            }
            Console.ForegroundColor = prevColor;
        }

        public void LogError(string message, params object[] args)
        {
            var prevColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;

            try
            {
                Console.WriteLine(LineStart + message, args);
            }
            catch
            {
                Console.WriteLine(LineStart + message);
            }
            Console.ForegroundColor = prevColor;
        }

        public void LogError(Exception exc, string message, params object[] args)
        {
            var prevColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            try
            {
                Console.WriteLine(LineStart + message, args);
            }
            catch
            {
                Console.WriteLine(LineStart + message);
            }
            Console.WriteLine(LineStart + GRStringHelpers.GetExceptionString(exc));
            Console.ForegroundColor = prevColor;
        }
    }
}
