using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Exceptions
{
    public class GRUnsuportedOperatorException : ApplicationException
    {
        public string UnsuportedOperator { get; private set; }

        public GRUnsuportedOperatorException(string unsuportedOperator) : base(string.Format("Operator '{0}' is not supported.", unsuportedOperator))
        {
            this.UnsuportedOperator = unsuportedOperator;
        }
    }
}
