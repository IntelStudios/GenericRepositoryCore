using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestTTframework.Models
{
    [GRTableName(TableName = "User")]
    public class User
    {
        [GRAIPrimaryKey]
        [GRColumnName(ColumnName = "Id")]
        public int Id { get; set; }


        [GRColumnName(ColumnName = "FirstName")]
        public string FirstName { get; set; }


        [GRColumnName(ColumnName = "LastName")]
        public string LastName { get; set; }
    }
}
