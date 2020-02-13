using System;
using System.Collections.Generic;
using System.Text;

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
