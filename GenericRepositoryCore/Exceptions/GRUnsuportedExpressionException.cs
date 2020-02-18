using System;
using System.Linq.Expressions;

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
