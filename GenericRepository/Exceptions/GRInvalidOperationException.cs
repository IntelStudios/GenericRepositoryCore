using System;

namespace GenericRepository.Exceptions
{
    class GRInvalidOperationException : ApplicationException
    {
        public GRInvalidOperationException(string message) : base(message)
        {
        }
    }
}
