using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestTTframework.Models
{
    [GRTableName(TableName = "Role")]
    class Role
    {
        [GRAIPrimaryKey]
        [GRColumnName(columnName: "RoleId")]
        public int Id { get; set; }

        [GRColumnName(columnName: "Name")]
        public string Name { get; set; }

        [GRColumnName(columnName: "Description")]
        public string Description { get; set; }

    }
}
