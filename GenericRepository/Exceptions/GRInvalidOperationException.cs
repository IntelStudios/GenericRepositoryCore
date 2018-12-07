using System;

namespace GenericRepository.Exceptions
{
    class GRInvalidOperationException : ApplicationException
    {
        public GRInvalidOperationException(string message) : base(message)
        {
        }

        public GRInvalidOperationException(string message, params object[] args) : this(string.Format(message, args))
        {
        }
    }
}
