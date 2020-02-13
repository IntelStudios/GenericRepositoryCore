using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Exceptions
{
    public class GRUnsuportedAttributeException : ApplicationException
    {
        public Attribute UnsuportedAttribute { get; private set; }

        public GRUnsuportedAttributeException(Attribute unsuportedAttribute) : base(string.Format("Attribute '{0}' is not supported.", unsuportedAttribute))
        {
            this.UnsuportedAttribute = unsuportedAttribute;
        }
    }
}
