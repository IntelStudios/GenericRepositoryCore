using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Exceptions
{
    public class GRUnsuportedDataTypeException : ApplicationException
    {
        public Type UnsuportedType { get; private set; }

        public GRUnsuportedDataTypeException(Type unsuportedType) : base(string.Format("Data type '{0}' is not supported.", unsuportedType))
        {
            this.UnsuportedType = unsuportedType;
        }
    }
}
