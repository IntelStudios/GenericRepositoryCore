using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using System;
using System.Diagnostics;

namespace GenericRepository.Test.Loggers
{
    public class TraceLogger : IGRContextLogger
    {
        public void LogDebug(string message, params object[] args)
        {
            Trace.TraceInformation(message, args);
        }

        public void LogError(string message, params object[] args)
        {
            Trace.TraceError(message, args);
        }

        public void LogError(Exception exc, string message, params object[] args)
        {
            Trace.TraceError(message, args);
            Trace.TraceError(GRStringHelpers.GetExceptionString(exc));
        }

        public void LogWarning(string message, params object[] args)
        {
            Trace.TraceWarning(message, args);
        }
    }
}
