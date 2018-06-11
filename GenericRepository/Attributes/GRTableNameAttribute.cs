using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Attributes
{
    public class GRTableNameAttribute : Attribute
    {
        public GRTableNameAttribute(string tableName)
        {
            this.TableName = tableName;
        }

        public GRTableNameAttribute()
        {

        }
        public string TableName { get; set; }
    }
}
