using System;

namespace GenericRepository.Exceptions
{
    public class GRAttributeApplicationFailedException : ApplicationException
    {
        public Attribute Attribute { get; private set; }

        public GRAttributeApplicationFailedException(Attribute attribute, string message) : base(message)
        {
            this.Attribute = attribute;
        }
    }
}
