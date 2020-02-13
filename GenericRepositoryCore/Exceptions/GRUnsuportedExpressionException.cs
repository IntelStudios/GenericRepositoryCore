using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Exceptions
{
    public class GRUnsuportedExpressionException : ApplicationException
    {
        public Expression UnsuportedExpression { get; private set; }

        public GRUnsuportedExpressionException(Expression unsuportedExpression) : base(string.Format("Expression '{0}' is not supported.", unsuportedExpression))
        {
            this.UnsuportedExpression = unsuportedExpression;
        }
    }
}
