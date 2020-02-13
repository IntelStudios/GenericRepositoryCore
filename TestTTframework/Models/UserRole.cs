using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestTTframework.Models
{
    [GRTableName(TableName = "UserRole")]
    class UserRole
    {
        
        [GRColumnName(ColumnName = "UserId")]
        public int UserId { get; set; }

        [GRColumnName(ColumnName = "RoleId")]
        public int RoleId { get; set; }
    }
}
