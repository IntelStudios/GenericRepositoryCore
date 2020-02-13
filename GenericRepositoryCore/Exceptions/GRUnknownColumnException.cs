using GenericRepository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Exceptions
{
    public class GRUnknownColumnException : ApplicationException
    {
        public string DBColumnName { get; private set; }
        public string PropertyName { get; private set; }

        public GRUnknownColumnException(string propertyName, string dbColumnName) :
            base($"Column '{dbColumnName}' (property {propertyName}) was not found in output data.")
        {
            this.DBColumnName = dbColumnName;
            this.PropertyName = propertyName;
        }
    }
}
