
using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace testORM.Model
{

[GRTableName(TableName = "AspNetUserRoles")]
public class AspNetUserRoles
{

[GRColumnName(ColumnName = "UserId")]
	public string Userid { get; set;}

[GRColumnName(ColumnName = "RoleId")]
	public string Roleid { get; set;}
}

}
