using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Attributes
{
    public class GRColumnNameAttribute : Attribute
    {
        public GRColumnNameAttribute(string columnName)
        {
            this.ColumnName = columnName;
        }
        public string ColumnName { get; set; }

        public GRColumnNameAttribute()
        {

        }
    }
}
