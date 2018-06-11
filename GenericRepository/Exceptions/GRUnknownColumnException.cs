using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Exceptions
{
    public class GRUnknownColumnException : ApplicationException
    {
        public string ColumnName { get; private set; }

        public GRUnknownColumnException(string columnName) : base(string.Format("Column '{0}' was not found in output data.", columnName))
        {
            this.ColumnName = columnName;
        }
    }
}
