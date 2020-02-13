using System;

namespace GenericRepository.Interfaces
{
    public interface IGRContextLogger
    {
        void LogDebug(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(string message, params object[] args);
        void LogError(Exception exc, string message, params object[] args);
    }
}
