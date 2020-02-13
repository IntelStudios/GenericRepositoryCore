using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Exceptions
{
    public class GRQueryBuildFailedException : ApplicationException
    {
        public GRQueryBuildFailedException(string message) : base(message)
        {
        }

        public GRQueryBuildFailedException(Exception innerException, string message) : base(message, innerException)
        {
        }

        public GRQueryBuildFailedException(string message, params object[] args) : base(string.Format(message, args))
        {
        }

        public GRQueryBuildFailedException(Exception innerException, string message, params object[] args) : base(string.Format(message, args), innerException)
        {
        }
    }
}
